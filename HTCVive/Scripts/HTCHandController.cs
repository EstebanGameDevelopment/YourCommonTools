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
        public Vector3 Shift = Vector3.zero;

#if ENABLE_HTCVIVE
        public WVR_DeviceType Device = WVR_DeviceType.WVR_DeviceType_Controller_Left;
        public GameObject ControlledObject;
        public GameObject HTCCamera;
        public bool Is6DOF = false;

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
                    Device = WVR_DeviceType.WVR_DeviceType_Controller_Left;
                    if (!Is6DOF) ControlledObject.transform.localPosition = new Vector3(-Shift.x, -Shift.y, Shift.z);
                }
                else
                {
                    Device = WVR_DeviceType.WVR_DeviceType_Controller_Right;
                    if (!Is6DOF) ControlledObject.transform.localPosition = new Vector3(Shift.x, -Shift.y, Shift.z);
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
                    Vector3 fwd = HTCCamera.transform.forward.normalized * 0.3f;
                    this.transform.position = HTCCamera.transform.position + new Vector3(fwd.x, 0, fwd.z);
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

