using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

namespace KATTH
{
    [CreateAssetMenu(menuName = "KATTH/Create Profile")]
    public class Profiles : ScriptableObject
    {
        [Header("Profile Details")]
        [SerializeField]
        private string m_profileName;
        public string profileName { get { return m_profileName; } }

        [SerializeField]
        private string m_lastSave;
        public string lastSave { get { return m_lastSave; } private set { m_lastSave = value; } }

        [Header("PlayerPrefs Save Settings")]
        [SerializeField]
        private bool m_saveInPlayerPrefs = true;
        public bool saveInPlayerPrefs { get { return m_saveInPlayerPrefs; } }

        [SerializeField]
        private string m_prefPrefix = "Settings_";
        public string prefPrefix { get { return m_prefPrefix; } }


        /*[Header("Audio Settings")]
        [SerializeField]
        private AudioMixer m_audioMixer;
        public AudioMixer audioMixer { get { return m_audioMixer; } }

        public Volume[] volumeControl;*/

        //

        public void SaveProfileName(string t_profileName = null)
        {
            if (t_profileName != null)
            {
                m_profileName = t_profileName;
            }

            m_lastSave = DateTime.Now.ToLocalTime().ToString();
        }

        public void OnEnable()
        {
            if (string.IsNullOrEmpty(m_lastSave))
            {
                SaveProfileName();
            }
        }

        /*public float GetAudioLevels(string name)
        {
            float volume = 1f;

            if (!audioMixer)
            {
                Debug.LogWarning("There is no AudioMixer defined in the profiles file");
                return volume;
            }

            for (int i = 0; i < volumeControl.Length; i++)
            {
                if(volumeControl[i].name != name)
                {
                    continue;
                }
                else
                {
                    if(saveInPlayerPrefs)
                    {
                        if(PlayerPrefs.HasKey(prefPrefix + volumeControl[i].name))
                        {
                            volumeControl[i].volume = PlayerPrefs.GetFloat(prefPrefix + volumeControl[i].name);
                        }
                    }

                    //reset the audio volume
                    volumeControl[i].tempVolume = volumeControl[i].volume;

                    //set the mixer to match the volume
                    if (audioMixer)
                        audioMixer.SetFloat(volumeControl[i].name, Mathf.Log(volumeControl[i].volume) * 20f);

                    volume = volumeControl[i].volume;
                    break;
                }
            }
            return volume;
        }

        public void GetAudioLevels()
        {
            if(!audioMixer)
            {
                Debug.LogWarning("There is no AudioMixer defined in the profiles file");
                return;
            }

            for(int i = 0; i < volumeControl.Length; i++)
            {
                if(saveInPlayerPrefs)
                {
                    if(PlayerPrefs.HasKey(prefPrefix + volumeControl[i].name))
                    {
                        volumeControl[i].volume = PlayerPrefs.GetFloat(prefPrefix + volumeControl[i].name);
                    }
                }

                //reset the audio volume
                volumeControl[i].tempVolume = volumeControl[i].volume;

                //set the mixer to match the volume
                audioMixer.SetFloat(volumeControl[i].name, Mathf.Log(volumeControl[i].volume) * 20f);
            }
        }

        public void SetAudioLevels(string name, float volume)
        {
            if(!audioMixer)
            {
                Debug.LogWarning("There is no AudioMixer defined in the profiles file");
                return;
            }

            for (int i = 0; i < volumeControl.Length; i++)
            {
                if (volumeControl[i].name != name)
                {
                    continue;
                }
                else
                {
                    audioMixer.SetFloat(volumeControl[i].name, Mathf.Log(volume) * 20f);
                    volumeControl[i].tempVolume = volume;
                    break;
                }
            }
        }

        public void SaveAudioLevels()
        {
            if (!audioMixer)
            {
                Debug.LogWarning("There is no AudioMixer defined in the profiles file");
                return;
            }

            float volume = 0f;
            for(int i = 0; i < volumeControl.Length; i++)
            {
                volume = volumeControl[i].tempVolume;
                if(saveInPlayerPrefs)
                {
                    PlayerPrefs.SetFloat(prefPrefix + volumeControl[i].name, volume);
                }
                audioMixer.SetFloat(volumeControl[i].name, Mathf.Log(volume) * 20f);
                volumeControl[i].volume = volume;
            }
        }*/



    }
}
