using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * SetTriggersOnStart
	 * 
	 * @author Esteban Gallardo
	 */
    public class SetTriggersOnStart : MonoBehaviour
	{
        public bool Trigger;

        private void Start()
        {
            Utilities.SetAllChildCollidersTrigger(this.gameObject, Trigger);
        }
    }
}