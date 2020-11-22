using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEditor;

namespace KATTH
{
    //used by the properties and settings
    [System.Serializable]
    public class Volume
    {
        public string name;
        public float volume = 1f;
        public float tempVolume = 1f;
    }

    //global methods that are used all over the place
    public static class SO
    {
        public static T[] GetInstance<T>(string name) where T : ScriptableObject
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets(name + " t:" + typeof(T).Name); //FindAssets uses tags
            T[] a = new T[guids.Length];
            //probably could get optimized
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(T).Name); //FindAssets uses tags
            T[] a = new T[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }
    }

    /*public class Settings
    {
        public static Profiles profile;
    }*/
}
