using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.EventSystems;

namespace YourCommonTools
{
	public class CardboardLoaderVR : MonoBehaviour
	{

		public static void SaveEnableCardboard()
		{

		}

		public static bool LoadEnableCardboard()
		{
			return true;
		}

		void Start()
		{
			if (LoadEnableCardboard())
			{
				StartCoroutine(LoadDevice("cardboard"));
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
