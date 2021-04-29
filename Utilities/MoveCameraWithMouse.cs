using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{
    /******************************************
     * 
     * MoveCameraWithMouse
     * 
     * Use the mouse to move the camera
     * 
     * @author Esteban Gallardo
     */
    public class MoveCameraWithMouse : MonoBehaviour
    {
        private enum RotationAxes { None = 0, MouseXAndY = 1, MouseX = 2, MouseY = 3, Controller = 4 }

        // ----------------------------------------------
        // PUBLIC VARIABLES
        // ----------------------------------------------	
        public Transform CameraLocal;

        public float SensitivityX = 7F;
        public float SensitivityY = 7F;
        public float MinimumY = -60F;
        public float MaximumY = 60F;

        public float Speed = 4;

        // ----------------------------------------------
        // PRIVATE VARIABLES
        // ----------------------------------------------	
        public float m_rotationY = 0F;

        // -------------------------------------------
        /* 
		 * Start
		 */
        void Start()
        {
#if UNITY_EDITOR
            if (CameraLocal == null)
            {
                if (GameObject.FindObjectOfType<Camera>() != null)
                {
                    CameraLocal = GameObject.FindObjectOfType<Camera>().transform;
                }                
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * RotateCamera
		 */
        private void RotateCamera()
        {
            RotationAxes axes = RotationAxes.None;

            if ((Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0))
            {
                axes = RotationAxes.MouseXAndY;
            }

            if ((axes != RotationAxes.Controller) && (axes != RotationAxes.None))
            {
                if (axes == RotationAxes.MouseXAndY)
                {
                    float rotationX = CameraLocal.localEulerAngles.y + Input.GetAxis("Mouse X") * SensitivityX;

                    m_rotationY += Input.GetAxis("Mouse Y") * SensitivityY;
                    m_rotationY = Mathf.Clamp(m_rotationY, MinimumY, MaximumY);

                    CameraLocal.localEulerAngles = new Vector3(-m_rotationY, rotationX, 0);
                }
                else if (axes == RotationAxes.MouseX)
                {
                    CameraLocal.Rotate(0, Input.GetAxis("Mouse X") * SensitivityX, 0);
                }
                else
                {
                    m_rotationY += Input.GetAxis("Mouse Y") * SensitivityY;
                    m_rotationY = Mathf.Clamp(m_rotationY, MinimumY, MaximumY);

                    CameraLocal.localEulerAngles = new Vector3(-m_rotationY, transform.localEulerAngles.y, 0);
                }
            }
        }

        // -------------------------------------------
        /* 
         * MoveCamera
         */
        private void MoveCamera()
        {
            Vector3 forward = Input.GetAxis("Vertical") * CameraLocal.forward * Speed * Time.deltaTime;
            Vector3 lateral = Input.GetAxis("Horizontal") * CameraLocal.right * Speed * Time.deltaTime;

            Vector3 increment = forward + lateral;
            CameraLocal.transform.position += increment;
        }


        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
        {
#if UNITY_EDITOR
            if (CameraLocal != null)
            {
#if ENABLE_OCULUS || ENABLE_WORLDSENSE || ENABLE_HTCVIVE
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MoveCamera();
                    RotateCamera();
                }
#else
                MoveCamera();
                RotateCamera();
#endif
            }
#endif
            }
    }

}