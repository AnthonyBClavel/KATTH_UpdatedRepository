using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace KATTH
{
    public class ProfilesMenuItem : MonoBehaviour
    {
        [Header("Profile Settings")]
        [SerializeField]
        private Profiles m_profile;
        public Profiles profile { get { return m_profile; } }

        [SerializeField]
        private CanvasGroup m_ProfileContents;
        public CanvasGroup profileContents { get { return m_ProfileContents; } }

        [SerializeField]
        private TextMeshProUGUI m_ProfileName;
        public TextMeshProUGUI profileName { get { return m_ProfileName; } }

        [SerializeField]
        private TextMeshProUGUI m_ProfileLastSave;
        public TextMeshProUGUI profileLastSave { get { return m_ProfileLastSave; } }

        [Header("Button Settings")]
        [SerializeField]
        private CanvasGroup m_ActionContents;
        public CanvasGroup actionContents { get { return m_ActionContents; } }

        [SerializeField]
        private CanvasGroup m_ConfirmContents;
        public CanvasGroup confirmContents { get { return m_ConfirmContents; } }

        private ProfilesMenu m_ProfilesMenu { get { return gameObject.GetComponentInParent<ProfilesMenu>(); } }

        public void SetProfile(Profiles name)
        {
            m_profile = name;
        }

        public void SetProfileName(string name)
        {
            m_ProfileName.text = name;
        }

        public void SetProfileSave(string name)
        {
            m_ProfileLastSave.text = name;
        }

        public void LoadProfile()
        {
            m_ProfilesMenu.LoadActiveProfile(m_profile);
            Debug.Log(ProfilesManager.profile);
        }

        public void Delesect()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void SelectButton(Button button)
        {
            button.Select();
        }

        public void DeleteSelf()
        {
            m_ProfilesMenu.DeleteProfile(this.gameObject);
        }

        public void ShowCanvas(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void hideCanvas(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

    }
}
