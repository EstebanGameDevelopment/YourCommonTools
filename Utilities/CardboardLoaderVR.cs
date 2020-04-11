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
#elif ENABLE_WORLDSENSE
        public string DefaultDeviceName = DAYDREAM_DEVICE_NAME;
#else
        public string DefaultDeviceName = CARDBOARD_DEVICE_NAME;
#endif

        private bool m_deviceLoaded = false;

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
#if ENABLE_WORLDSENSE
            return true;
#elif ENABLE_OCULUS
            return true;
#else
            return (PlayerPrefs.GetInt(CARDBOARD_ENABLE_COOCKIE, 0) == 1);
#endif
        }

        // -------------------------------------------
        /* 
		* ResetEnableCardboard
		*/
        public static void ResetEnableCardboard()
        {
            PlayerPrefs.SetInt(CARDBOARD_ENABLE_COOCKIE, 0);
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
#if ENABLE_WORLDSENSE
            ForceActivation = true;
            DefaultDeviceName = "Daydream";
#elif ENABLE_OCULUS
            ForceActivation = true;
            DefaultDeviceName = "Oculus";
#endif
            if (LoadEnableCardboard() || ForceActivation)
            {
                string nameDeviceLoaded = DefaultDeviceName;
                if (!UnityEngine.XR.XRSettings.enabled)
                {
                    if (!m_deviceLoaded)
                    {
                        m_deviceLoaded = true;
                        // StartCoroutine(LoadDevice(DAYDREAM_DEVICE_NAME));
                        nameDeviceLoaded = DefaultDeviceName;
                        StartCoroutine(LoadDevice(DefaultDeviceName));
                    }
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
		 * IsCardboardDevice
		 */
        public bool IsCardboardDevice()
        {
            return DefaultDeviceName.Equals(CARDBOARD_DEVICE_NAME);
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
