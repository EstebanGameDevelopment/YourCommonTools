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
        public float ShiftX = 1;
        public float ShiftY = 1;
        public float ShiftZ = 1;

#if ENABLE_HTCVIVE
        public WVR_DeviceType Device = WVR_DeviceType.WVR_DeviceType_Controller_Left;
        public GameObject ControlledObject;
        public GameObject HTCCamera;

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
                    ControlledObject.transform.localPosition = new Vector3(-ShiftX, -ShiftY, ShiftZ);
                }
                else
                {
                    Device = WVR_DeviceType.WVR_DeviceType_Controller_Right;
                    ControlledObject.transform.localPosition = new Vector3(ShiftX, -ShiftY, ShiftZ);
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
                this.transform.position = HTCCamera.transform.position;
                ControlledObject.transform.localRotation = WaveVR_Controller.Input(Device).transform.rot;
            }
        }
#endif
    }
}

