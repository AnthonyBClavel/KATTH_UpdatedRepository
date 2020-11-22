using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace KATTH
{
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        protected static T _instance = null;
        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    T[] results = Resources.FindObjectsOfTypeAll<T>();
                    if (results.Length == 0) //none found
                    {
                        Debug.LogErrorFormat("[ScriptableSingleton] No instance of {0} found!", typeof(T).ToString());
                    }
                    else if (results.Length > 1) //more than one
                    {
                        Debug.LogErrorFormat("[ScriptableSingleton] Mutliple instances of {0} found! I will take the first one if I can find! It shall be mine!", typeof(T).ToString());
                        
                        //DeleteItem(results);

                        _instance = results[0];
                        _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    }
                    else //found one
                    {
                        //Debug.LogFormat("[ScriptableSingleton] An instance of {0} was found!", typeof(T).ToString());
                        
                        _instance = results[0];
                        _instance.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    }
                }
                return _instance;
            }
        }

        public static void DeleteItem(T[] results)
        {
            for (int i = 1; i < results.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(results[i]);
                string metaPath = path.Replace(".asset", ".asset.meta");

                //check if the file exists
                if (File.Exists(path))
                {
                    //delete actual file
                    File.Delete(metaPath);
                }
                //check if meta file exists
                if(File.Exists(metaPath))
                {
                    //delete actual file
                    File.Delete(metaPath);
                }
            }
            //refresh editor view
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }

}
