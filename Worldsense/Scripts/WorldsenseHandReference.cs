using UnityEngine;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * WorldsenseHandReference
	 * 
	 * @author Esteban Gallardo
	 */
    public class WorldsenseHandReference : MonoBehaviour
    {
        private static WorldsenseHandReference _instance;

        public static WorldsenseHandReference Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(WorldsenseHandReference)) as WorldsenseHandReference;
                }
                return _instance;
            }
        }
    }
}

