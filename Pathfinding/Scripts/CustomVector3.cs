using System;
using UnityEngine;
using System.Collections;

namespace YourCommonTools
{
    [System.Serializable]
    public class CustomVector3
    {
		public float x;
        public float y;
        public float z;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public CustomVector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public CustomVector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public CustomVector3(CustomVector3 _data)
		{
            x = _data.x;
            y = _data.y;
            z = _data.z;
        }

        // -------------------------------------------
        /* 
		 * Trace the data
		 */
        public override string ToString()
        {
            return "("+x+","+y+"," +z + ")";
        }

        // -------------------------------------------
        /* 
		 * Get Vector3
		 */
        public Vector3 GetVector3()
        {
            return new Vector3(x,y,z);
        }

        // -------------------------------------------
        /* 
		 * Set with Vector3
		 */
        public void SetVector3(Vector3 _data)
        {
            x = _data.x;
            y = _data.y;
            z = _data.z;
        }

    }
}