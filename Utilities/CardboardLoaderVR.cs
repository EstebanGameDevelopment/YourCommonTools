using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.EventSystems;

namespace YourCommonTools
{
	public class CardboardLoaderVR : MonoBehaviour
	{
		public const string CARDBOARD_ENABLE_COOCKIE = "CARDBOARD_ENABLE_COOCKIE";

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
			if (LoadEnableCardboard())
			{
				StartCoroutine(LoadDevice("Cardboard"));
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
