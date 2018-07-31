using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.EventSystems;

namespace YourCommonTools
{
	public class CardboardLoaderVR : MonoBehaviour
	{
        public const string EVENT_VRLOADER_LOADED_DEVICE_NAME = "EVENT_VRLOADER_LOADED_DEVICE_NAME";

        public const string CARDBOARD_ENABLE_COOCKIE = "CARDBOARD_ENABLE_COOCKIE";

        public const string CARDBOARD_DEVICE_NAME = "Cardboard";
        public const string DAYDREAM_DEVICE_NAME = "Daydream";
        public const string OCULUS_DEVICE_NAME = "Oculus";

        public bool ForceActivation = false;
#if ENABLE_OCULUS
        public string DefaultDeviceName = OCULUS_DEVICE_NAME;
#else
        public string DefaultDeviceName = CARDBOARD_DEVICE_NAME;
#endif

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
            InitializeCardboard();
        }

        // -------------------------------------------
        /* 
		 * InitializeCardboard
		 */
        public void InitializeCardboard()
        {
            if (LoadEnableCardboard() || ForceActivation)
            {
                string nameDeviceLoaded = DefaultDeviceName;
                if (!UnityEngine.XR.XRSettings.enabled)
                {
                    // StartCoroutine(LoadDevice(DAYDREAM_DEVICE_NAME));
                    nameDeviceLoaded = DefaultDeviceName;
                    StartCoroutine(LoadDevice(DefaultDeviceName));
                }
                BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_VRLOADER_LOADED_DEVICE_NAME, 1, nameDeviceLoaded);
            }
            else
            {
                Input.gyro.enabled = true;
            }
        }


        // -------------------------------------------
        /* 
		 * LoadDevice
		 */
        IEnumerator LoadDevice(string newDevice)
		{
            UnityEngine.XR.XRSettings.LoadDeviceByName(newDevice);
            yield return null;
			UnityEngine.XR.XRSettings.enabled = true;
		}
	}	
}
