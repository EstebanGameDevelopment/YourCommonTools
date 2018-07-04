using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.EventSystems;

namespace YourCommonTools
{
	public class CardboardLoaderVR : MonoBehaviour
	{
		public const string CARDBOARD_ENABLE_COOCKIE = "CARDBOARD_ENABLE_COOCKIE";

        public const string CARDBOARD_DEVICE_NAME = "Cardboard";
        public const string DAYDREAM_DEVICE_NAME = "Daydream";

        public bool ForceActivation = false;

		// -------------------------------------------
		/* 
		* Save a flag to report if we need to use or not the Google VR
		*/
		public static void SaveEnableCardboard(bool _enabledCardboard)
		{
            PlayerPrefs.SetInt(CARDBOARD_ENABLE_COOCKIE, (_enabledCardboard ? 1 : 0));
		}

		// -------------------------------------------
		/* 
		 * Get the if we need to use or not the Google VR
		 */
		public static bool LoadEnableCardboard()
		{
			return (PlayerPrefs.GetInt(CARDBOARD_ENABLE_COOCKIE, 0) == 1);
		}


		// -------------------------------------------
		/* 
		 * Start
		 */
		void Start()
		{
			if (LoadEnableCardboard() || ForceActivation)
			{
                if (!UnityEngine.XR.XRSettings.enabled)
                {
                    // StartCoroutine(LoadDevice(DAYDREAM_DEVICE_NAME));
                    StartCoroutine(LoadDevice(CARDBOARD_DEVICE_NAME));
                }
            }
			else
			{
				Input.gyro.enabled = true;
			}
		}

		IEnumerator LoadDevice(string newDevice)
		{
            UnityEngine.XR.XRSettings.LoadDeviceByName(newDevice);
            yield return null;
			UnityEngine.XR.XRSettings.enabled = true;
		}
	}	
}
