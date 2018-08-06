using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * AutoRotate
	 * 
	 * @author Esteban Gallardo
	 */
    public class AutoRotate : MonoBehaviour
	{
        public float SpeedRotation = 0.3f;

        private float m_angleValue = 0;

        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
		{
            m_angleValue += SpeedRotation;
            this.transform.Rotate(new Vector3(0, 1, 0), SpeedRotation);
		}
	}
}