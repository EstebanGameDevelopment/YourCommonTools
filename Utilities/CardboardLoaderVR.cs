using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using UnityEngine.EventSystems;
using YourNetworkingTools;

namespace YourCommonTools
{
	public class CardboardLoaderVR : MonoBehaviour
	{
		void Start()
		{
			if (MultiplayerConfiguration.LoadEnableCardboard())
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
