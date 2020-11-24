using UnityEngine;
#if ENABLE_HTCVIVE
using wvr;
#endif

namespace YourCommonTools
{
    /******************************************
	 * 
	 * HTCHeadController
	 * 
	 * @author Esteban Gallardo
	 */
    public class HTCHeadController : MonoBehaviour
    {
#if ENABLE_HTCVIVE
        public GameObject ControlledHead;

        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
        {
            if (ControlledHead != null)
            {
                ControlledHead.transform.position = WaveVR_Controller.Input(WVR_DeviceType.WVR_DeviceType_HMD).transform.pos;
                ControlledHead.transform.localRotation = WaveVR_Controller.Input(WVR_DeviceType.WVR_DeviceType_HMD).transform.rot;
            }
        }
#endif
    }
}

