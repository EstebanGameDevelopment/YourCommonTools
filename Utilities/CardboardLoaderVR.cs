using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.EventSystems;

namespace YourCommonTools
{
	public class CardboardLoaderVR : MonoBehaviour
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_VRLOADER_LOADED_DEVICE_NAME = "EVENT_VRLOADER_LOADED_DEVICE_NAME";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const string CARDBOARD_ENABLE_COOCKIE = "CARDBOARD_ENABLE_COOCKIE";

        public const string CARDBOARD_DEVICE_NAME = "Cardboard";
        public const string DAYDREAM_DEVICE_NAME = "Daydream";
        public const string OCULUS_DEVICE_NAME = "Oculus";
        public const string HTCVIVE_DEVICE_NAME = "Mock HMD";

#if ENABLE_OCULUS
        public string DefaultDeviceName = OCULUS_DEVICE_NAME;
#elif ENABLE_HTCVIVE
        public string DefaultDeviceName = HTCVIVE_DEVICE_NAME;
#elif ENABLE_WORLDSENSE
        public string DefaultDeviceName = DAYDREAM_DEVICE_NAME;
#else
        public string DefaultDeviceName = CARDBOARD_DEVICE_NAME;
#endif

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static CardboardLoaderVR _instance;

        public static CardboardLoaderVR Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(CardboardLoaderVR)) as CardboardLoaderVR;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        container.name = "CardboardLoaderPlaceHolder";
                        _instance = container.AddComponent(typeof(CardboardLoaderVR)) as CardboardLoaderVR;
                    }
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public bool ForceActivation = false;
        public string CoockieName = "CARDBOARD_ENABLE_COOCKIE";

        public bool m_hasBeenInited = false;
        public bool m_isCardboardEnabled = false;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private bool m_deviceLoaded = false;

        public bool DeviceLoaded
        {
            get { return m_deviceLoaded; }
        }

        // -------------------------------------------
        /* 
		* Save a flag to report if we need to use or not the Google VR
		*/
        public void SaveEnableCardboard(bool _enabledCardboard)
		{
            if (CoockieName.Length > 0)
            {
                PlayerPrefs.SetInt(CoockieName, (_enabledCardboard ? 1 : 0));
            }
		}

		// -------------------------------------------
		/* 
		 * Get the if we need to use or not the Google VR
		 */
		public bool LoadEnableCardboard()
		{
#if ENABLE_WORLDSENSE
            return true;
#elif ENABLE_OCULUS
            return true;
#elif ENABLE_HTCVIVE
            return false;
#else
            if (m_hasBeenInited)
            {
                return m_isCardboardEnabled;
            }
            else
            {
                if (CoockieName.Length > 0)
                {
                    return (PlayerPrefs.GetInt(CoockieName, 0) == 1);
                }
                else
                {
                    return false;
                }
            }
#endif
        }

        // -------------------------------------------
        /* 
		* ResetEnableCardboard
		*/
        public void ResetEnableCardboard()
        {
            if (CoockieName.Length > 0)
            {
                PlayerPrefs.SetInt(CoockieName, 0);
            }
        }

        // -------------------------------------------
        /* 
		* ResetEnableCardboard
		*/
        private void Start()
        {
#if ENABLE_WORLDSENSE
            InitializeCardboard();
#elif ENABLE_OCULUS
            InitializeCardboard();
#elif ENABLE_HTCVIVE
            InitializeCardboard();
#endif

            m_isCardboardEnabled = LoadEnableCardboard();
            m_hasBeenInited = true;
        }

        // -------------------------------------------
        /* 
		 * InitializeCardboard
		 */
        public void InitializeCardboard()
        {
#if ENABLE_WORLDSENSE
            ForceActivation = true;
            DefaultDeviceName = DAYDREAM_DEVICE_NAME;
#elif ENABLE_OCULUS
            ForceActivation = true;
            DefaultDeviceName = OCULUS_DEVICE_NAME;
#elif ENABLE_HTCVIVE
            ForceActivation = false;
            DefaultDeviceName = HTCVIVE_DEVICE_NAME;
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
