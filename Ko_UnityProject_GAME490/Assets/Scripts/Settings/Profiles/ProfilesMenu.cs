using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace KATTH
{
    public class MakeScriptableObject
    {
        //CreateAsset<Profiles>("Name", "Assets/_SaveProfiles/");
        public static T CreateAsset<T>(string name, string path) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            UnityEditor.AssetDatabase.CreateAsset(asset, path + name + ".asset");
            UnityEditor.AssetDatabase.SaveAssets();

            return asset;
        }
    }

    public class ProfilesMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_Template;
        public GameObject template { get { return m_Template; } }

        [SerializeField]
        private Transform m_ListParent;
        public Transform listParent { get { return listParent; } }

        private Dictionary<GameObject, Profiles> m_ProfileList = new Dictionary<GameObject, Profiles>();

        [Header("New Profile")]
        [SerializeField]
        private int m_MaxProfiles = 6;
        [SerializeField]
        private CanvasGroup m_CreateProfileGroup;
        [SerializeField]
        private TextMeshProUGUI m_ProfileText;
        [SerializeField]
        private TMP_InputField m_ProfileInput;
        [SerializeField]
        private Button m_CreateProfileButton;
        private string m_CreateProfileName { get { return m_ProfileInput.text.ToString(); } }

        //Loading Profile
        private void SetActiveProfile()
        {
            if (ProfilesManager.List.Count <= 0)
            {
                ProfilesManager.Instance.Profile(null);
            }
        }

        public void LoadActiveProfile(Profiles profile)
        {
            ProfilesManager.Instance.Profile(profile);
        }

        //New Profile
        //need to check if the file already exists when creating new files
        public void NewProfile()
        {
            if (m_ProfileText && (string.IsNullOrEmpty(m_ProfileText.text) || string.IsNullOrEmpty(m_ProfileText.text.Trim())))
                return;

            Profiles profile = null;

            //look for the file if it exists
            string filePath = Application.dataPath + "/_SaveProfiles/" + m_CreateProfileName.ToString() + ".asset";

            if (File.Exists(filePath))
            {
                Profiles[] profiles = SO.GetInstance<Profiles>(m_CreateProfileName);
                //if it already is in the list, then add version number after its name
                if (ProfilesManager.List.Count > 0)
                {
                    for (int i = 0; i < ProfilesManager.List.Count; i++)
                    {
                        if (ProfilesManager.List[i] != null && ProfilesManager.List[i].name == m_CreateProfileName)
                        {
                            string multiple = " (" + profiles.Length.ToString() + ")";
                            //Debug.LogWarning("File already in the list");
                            profile = MakeScriptableObject.CreateAsset<Profiles>(m_CreateProfileName + multiple, "Assets/_SaveProfiles/");
                            break;
                        }
                    }
                }
                //otherwise find it and use it
                if (profile == null)
                {
                    if(profiles.Length > 0)
                    {
                        profile = profiles[0];
                    }                  
                }
            }
            //if it doesnt already exist then make a new one
            else
            {
                profile = MakeScriptableObject.CreateAsset<Profiles>(m_CreateProfileName, "Asset/_SaveProfiles/");
            }

            //clear last saved profile
            m_ProfileText.text = string.Empty;
            m_ProfileInput.text = string.Empty;
            m_CreateProfileButton.interactable = false;

            //create new profile & add to the list
            ProfilesManager.List.Add(profile);

            //create game object in scene and add to dictionary
            InstantiateProfileUI(profile);

            ProfilesManager.Instance.SortList();
            RemoveProfilesFromScene();
            CreateProfileDictionary();
            SetActiveProfile();

            //make sure we haven't maxed out
            CheckProfileList();
            SelectFirst();
        }

        //Deleting a Profile
        public void RemoveProfilesFromScene()
        {
            //clear the dictionary
            m_ProfileList.Clear();
            //remove from the scene
            foreach (Transform child in m_ListParent)
            {
                Destroy(child.gameObject);
            }
        }

        //delete the profile completely
        public void DeleteProfile(GameObject profileItem)
        {
            if(m_ProfileList.ContainsKey(profileItem))
            {
                Profiles profile;
                if (m_ProfileList.TryGetValue(profileItem, out profile))
                {
                    string path = UnityEditor.AssetDatabase.GetAssetPath(profile);
                    string metaPath = path.Replace(".asset", ".asset.meta");

                    //remove list
                    for (int i = 0; i < ProfilesManager.List.Count; i++)
                    {
                        Profiles profileListItem = ProfilesManager.List[i];
                        if(profileListItem == profile)
                        {
                            ProfilesManager.List.Remove(profileListItem);
                            break;
                        }
                    }

                    //remove from scene
                    Destroy(profileItem);

                    //remove from dictionary
                    m_ProfileList.Remove(profileItem);

                    //clear if its the current profile
                    if(ProfilesManager.profile == profile)
                    {
                        ProfilesManager.Instance.Profile(null);
                    }

                    //check if the file exists
                    if(File.Exists(path))
                    {
                        //delete actual file
                        File.Delete(path);
                    }
                    if(File.Exists(metaPath))
                    {
                        //delete actual file
                        File.Delete(metaPath);
                    }

                    //refresh editor view
                    RefreshEditorProjectWindow();

                    SelectFirst();
                }
            }
            CheckProfileList();
        }

        void RefreshEditorProjectWindow()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        public void ResetNewButton()
        {
            if (!m_CreateProfileButton || !m_ProfileInput)
                return;

            if (string.IsNullOrEmpty(m_ProfileInput.text.Trim()))
            {
                m_CreateProfileButton.interactable = false;
            }
            else
            {
                m_CreateProfileButton.interactable = true;
            }
        }

        //check to make sure we can add more profiles
        public void CheckProfileList()
        {
            if (!m_CreateProfileGroup)
                return;

            if (m_ProfileList.Count >= m_MaxProfiles)
            {
                m_CreateProfileGroup.interactable = false;
                m_CreateProfileGroup.alpha = 0;
            }
            else
            {
                m_CreateProfileGroup.interactable = true;
                m_CreateProfileGroup.alpha = 1;
                if (m_ProfileList.Count == 0)
                {
                    m_ProfileInput.Select();
                }
            }
            ResetNewButton();
        }

        //select the first profile in our list
        private void SelectFirst()
        {
            if (m_ProfileList.Count > 0)
            {
                EventSystem.current.SetSelectedGameObject(null);
                var enumerator = m_ProfileList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    //access the value with enumerator.Current.Value;
                    enumerator.Current.Key.GetComponent<Button>().Select();
                    break;
                }
            }
        }

        public void SelectButton(Button button)
        {
            button.Select();
        }

        //create the menu item object in the canvas
        private void InstantiateProfileUI(Profiles profile)
        {
            if (profile == null || template == null)
                return;

            GameObject profileItem = Instantiate(template, transform.position, transform.rotation);
            profileItem.transform.SetParent(m_ListParent);

            profileItem.GetComponent<ProfilesMenuItem>().SetProfile(profile);

            string name = profile.name;

            if (!string.IsNullOrEmpty(profile.profileName))
            {
                name = profile.profileName;
            }
            profileItem.name = "profile: " + name;

            profileItem.GetComponent<ProfilesMenuItem>().SetProfileName(name);
            profileItem.GetComponent<ProfilesMenuItem>().SetProfileSave(profile.lastSave);

            m_ProfileList.Add(profileItem, profile);

            profileItem.GetComponent<Button>().Select();
        }

        //go through the dictionary and instantiate each profile item
        private void CreateProfileDictionary()
        {
            if(ProfilesManager.List.Count > 0)
            {
                for (int i = 0; i < ProfilesManager.List.Count; i++)
                {
                    if (ProfilesManager.List[i] == null)
                        continue;

                    InstantiateProfileUI(ProfilesManager.List[i]);
                }
            }
        }
        
        //use this for initialization
        private void Awake()
        {
            if (ProfilesManager.Instance != null)
                ProfilesManager.Instance.SortList();

            CreateProfileDictionary();

            CheckProfileList();

            SetActiveProfile();
        }

        private void Start()
        {
            SelectFirst();
        }
    }
}
