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
        public GameObject ControlledHead;

#if ENABLE_HTCVIVE
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

