using UnityEngine;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * WorldsenseHandController
	 * 
	 * @author Esteban Gallardo
	 */
    public class WorldsenseHandController : MonoBehaviour
    {
        public Vector3 Shift = new Vector3(0.25f, 0.5f, 0.15f);

        public GameObject ControlledObject;
        public GameObject WorldsenseCamera;
        public GameObject LaserPointer;

        // -------------------------------------------
        /* 
		 * Start
		 */
        private void Start()
        {
            if (ControlledObject != null)
            {
#if ENABLE_WORLDSENSE
                if (WorldsenseHandReference.Instance != null)
                {
                    ControlledObject.transform.localPosition = new Vector3(0, -Shift.y, 0);
                }                    
#else
                Utilities.SetActiveRecursively(ControlledObject, false);
#endif
            }
        }

        // -------------------------------------------
        /* 
        * Update
        */
        void Update()
        {
#if ENABLE_WORLDSENSE
            if (ControlledObject != null)
            {
                if (WorldsenseHandReference.Instance != null)
                {
                    Vector3 fwd = WorldsenseCamera.transform.forward.normalized * 0.2f;
                    Vector3 rgt = WorldsenseCamera.transform.right * 0.2f;
                    if (KeysEventInputController.Instance.IsRightHanded())
                    {
                        this.transform.position = WorldsenseCamera.transform.position + new Vector3(fwd.x, 0, fwd.z) + new Vector3(rgt.x, 0, rgt.z);
                    }
                    else
                    {
                        this.transform.position = WorldsenseCamera.transform.position + new Vector3(fwd.x, 0, fwd.z) - new Vector3(rgt.x, 0, rgt.z);
                    }

                    ControlledObject.transform.localRotation = WorldsenseHandReference.Instance.transform.rotation;
                }
            }
#endif
        }
    }
}

