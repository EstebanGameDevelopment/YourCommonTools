using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_HTCVIVE
using wvr;
#endif
#if ENABLE_YOURVRUI
using YourVRUI;
#endif
#if ENABLE_PICONEO
using Pvr_UnitySDKAPI;
#endif

namespace YourCommonTools
{

    /******************************************
	 * 
	 * LaserPointerSwitchController
	 * 
	 * @author Esteban Gallardo
	 */
    public class LaserPointerSwitchController : MonoBehaviour
	{
		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static LaserPointerSwitchController _instance;

		public static LaserPointerSwitchController Instance
		{
			get
			{
				if (!_instance)
				{
                    _instance = GameObject.FindObjectOfType(typeof(LaserPointerSwitchController)) as LaserPointerSwitchController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "LaserPointerSwitchController";
						_instance = container.AddComponent(typeof(LaserPointerSwitchController)) as LaserPointerSwitchController;
					}
				}
				return _instance;
			}
		}

        private bool m_isRightHandSelected = true;

        public bool IsRightHandSelected
        {
            get { return m_isRightHandSelected; }
        }

#if ENABLE_YOURVRUI && DISABLE_ONLY_ONE_HAND
        // -------------------------------------------
        /* 
        * SetPointerRightLaser
        */
        public bool SetPointerRightLaser(bool _force = false)
        {
            if ((YourVRUIScreenController.Instance.ContainerLaserRight != YourVRUIScreenController.Instance.ContainerLaser) || _force)
            {
                if (YourVRUIScreenController.Instance.ContainerLaserRight != null)
                {
                    YourVRUIScreenController.Instance.ContainerLaserRight.SetActive(true);
                    YourVRUIScreenController.Instance.LaserPointer = YourVRUIScreenController.Instance.ContainerLaserRight;
                    if (YourVRUIScreenController.Instance.ContainerLaserLeft != null)
                    {
                        YourVRUIScreenController.Instance.ContainerLaserLeft.SetActive(false);
                    }
                    m_isRightHandSelected = true;
                    return true;
                }
            }
            return false;
        }

        // -------------------------------------------
        /* 
        * SetPointerLeftLaser
        */
        public bool SetPointerLeftLaser(bool _force = false)
        {
            if ((YourVRUIScreenController.Instance.ContainerLaserLeft != YourVRUIScreenController.Instance.ContainerLaser) || _force)
            {
                if (YourVRUIScreenController.Instance.ContainerLaserLeft != null)
                {
                    YourVRUIScreenController.Instance.ContainerLaserLeft.SetActive(true);
                    YourVRUIScreenController.Instance.LaserPointer = YourVRUIScreenController.Instance.ContainerLaserLeft;
                    if (YourVRUIScreenController.Instance.ContainerLaserRight != null)
                    {
                        YourVRUIScreenController.Instance.ContainerLaserRight.SetActive(false);
                    }
                    m_isRightHandSelected = false;
                    return true;
                }
            }
            return false;
        }
#endif

        // -------------------------------------------
        /* 
        * Update
        */
        void Update()
        {
#if DISABLE_ONLY_ONE_HAND && ENABLE_YOURVRUI
#if ENABLE_OCULUS
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                if (SetPointerRightLaser())
                {
                    KeysEventInputController.Instance.IgnoreNextUp = true;
                }
            }
            else
            {
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
                {
                    if (SetPointerLeftLaser())
                    {
                        KeysEventInputController.Instance.IgnoreNextUp = true;
                    }
                }
            }
#elif ENABLE_HTCVIVE
            if (WaveVR_Controller.Input(wvr.WVR_DeviceType.WVR_DeviceType_Controller_Right).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Trigger)
#if UNITY_EDITOR
                || Input.GetKeyDown(KeyCode.UpArrow)
#endif
            )
            {
                if (SetPointerRightLaser())
                {
                    KeysEventInputController.Instance.IgnoreNextUp = true;
                }
            }
            else
            {
                if (WaveVR_Controller.Input(wvr.WVR_DeviceType.WVR_DeviceType_Controller_Left).GetPressDown(WVR_InputId.WVR_InputId_Alias1_Trigger)
#if UNITY_EDITOR
                || Input.GetKeyDown(KeyCode.DownArrow)
#endif
                )
                {
                    if (SetPointerLeftLaser())
                    {
                        KeysEventInputController.Instance.IgnoreNextUp = true;
                    }
                }
            }
#elif ENABLE_PICONEO
            if (Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TRIGGER))
            {
                if (SetPointerRightLaser())
                {
                    KeysEventInputController.Instance.IgnoreNextUp = true;
                }
            }
            else
            {
                if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TRIGGER))
                {
                    if (SetPointerLeftLaser())
                    {
                        KeysEventInputController.Instance.IgnoreNextUp = true;
                    }
                }
            }
#endif
#endif
        }

    }

}