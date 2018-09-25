using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YourCommonTools
{
	public class ClearLocalStore {
	#if UNITY_EDITOR
		[MenuItem("Clear Local Data/Clear PlayerPrefs")]
		private static void NewMenuOption()
		{
			PlayerPrefs.DeleteAll();
			Debug.Log("PlayerPrefs CLEARED!!!");
		}

        [MenuItem("Clear Local Data/Clear Cache")]
        private static void CleanCache()
        {
            if (Caching.ClearCache())
            {
                Debug.Log("Successfully cleaned the cache.");
            }
            else
            {
                Debug.Log("Cache is being used.");
            }
        }
#endif
    }
}
