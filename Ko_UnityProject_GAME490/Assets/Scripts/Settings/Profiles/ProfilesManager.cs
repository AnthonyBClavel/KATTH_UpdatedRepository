using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KATTH
{
    [CreateAssetMenu(menuName = "KATTH/Create Profile Manager")]
    public class ProfilesManager : ScriptableSingleton<ProfilesManager>
    {
        [Header("Active Profile")]
        [SerializeField]
        private Profiles m_profile;
        public static Profiles profile
        {
            get {
                if (Instance != null)
                {
                    return Instance.m_profile;
                }
                else
                {
                    return null;
                }
            }
        }

        [Header("Profile List")]
        [SerializeField]
        private List<Profiles> m_List;
        public static List<Profiles> List { get { return Instance.m_List; } }

        public void Profile(Profiles profile)
        {
            m_profile = profile;
        }

        public void SortList()
        {
            //remove any that are missing or null
            m_List.RemoveAll(Profiles => Profiles == null);
            //sort the list by date
            m_List.Sort((x, y) => DateTime.Compare(Convert.ToDateTime(y.lastSave), Convert.ToDateTime(x.lastSave)));
        }

    }
}
