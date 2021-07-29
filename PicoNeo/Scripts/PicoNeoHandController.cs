using UnityEngine;
#if ENABLE_PICONEO
using Pvr_UnitySDKAPI;
#endif

namespace YourCommonTools
{
    /******************************************
	 * 
	 * HTCHandController
	 * 
	 * @author Esteban Gallardo
	 */
    public class PicoNeoHandController : MonoBehaviour
    {
        public GameObject ControlledObject;
        public GameObject PicoNeoCamera;
        public GameObject PicoNeoLeftController;
        public GameObject PicoNeoRightController;
        public GameObject LaserPointer;

#if ENABLE_PICONEO
        private int m_handTypeSelected = 0;
        private GameObject m_currentController;

        public GameObject CurrentController
        {
            get { return m_currentController; }
        }
        public int HandTypeSelected
        {
            get { return m_handTypeSelected; }
        }

        // -------------------------------------------
        /* 
		 * Start
		 */
        private void Start()
        {
            Controller.UPvr_SetHandNess(Pvr_Controller.UserHandNess.Right);

            if (ControlledObject != null)
            {
                if (Controller.UPvr_GetMainHandNess() == 0)
                {
                    m_handTypeSelected = 0;
                    m_currentController = PicoNeoLeftController;
                }
                else
                {
                    m_handTypeSelected = 1;
                    m_currentController = PicoNeoRightController;
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
                ControlledObject.transform.position = m_currentController.transform.position;
                ControlledObject.transform.rotation = m_currentController.transform.rotation;
            }
        }
#endif
    }
}

