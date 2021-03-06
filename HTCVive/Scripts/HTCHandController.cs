using UnityEngine;
#if ENABLE_HTCVIVE
using wvr;
#endif

namespace YourCommonTools
{
    /******************************************
	 * 
	 * HTCHandController
	 * 
	 * @author Esteban Gallardo
	 */
    public class HTCHandController : MonoBehaviour
    {
        public Vector3 Shift = new Vector3(0.25f, 0.5f, 0.15f);

        public GameObject ControlledObject;
        public GameObject HTCCamera;
        public GameObject LaserPointer;
        public bool Is6DOF = false;

#if ENABLE_HTCVIVE
        public WVR_DeviceType Device = WVR_DeviceType.WVR_DeviceType_Controller_Left;

        // -------------------------------------------
        /* 
		 * Start
		 */
        private void Start()
        {
            if (ControlledObject != null)
            {
                if (WaveVR_Controller.IsLeftHanded)
                {
                    Device = WVR_DeviceType.WVR_DeviceType_Controller_Right;
                    if (!Is6DOF) ControlledObject.transform.localPosition = new Vector3(0, -Shift.y, 0);
                }
                else
                {
                    Device = WVR_DeviceType.WVR_DeviceType_Controller_Right;
                    if (!Is6DOF) ControlledObject.transform.localPosition = new Vector3(0, -Shift.y, 0);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
        {
            if (ControlledObject != null)
            {
                if (!Is6DOF)
                {
                    Vector3 fwd = HTCCamera.transform.forward.normalized * 0.2f;
                    Vector3 rgt = HTCCamera.transform.right * 0.2f;
                    if (WaveVR_Controller.IsLeftHanded)
                    {
                        this.transform.position = HTCCamera.transform.position + new Vector3(fwd.x, 0, fwd.z) - new Vector3(rgt.x, 0, rgt.z);
                    }
                    else
                    {
                        this.transform.position = HTCCamera.transform.position + new Vector3(fwd.x, 0, fwd.z) + new Vector3(rgt.x, 0, rgt.z);
                    }
                }
                else
                {
                    ControlledObject.transform.position = WaveVR_Controller.Input(Device).transform.pos;
                }
                ControlledObject.transform.localRotation = WaveVR_Controller.Input(Device).transform.rot;
            }
        }
#endif
    }
}

