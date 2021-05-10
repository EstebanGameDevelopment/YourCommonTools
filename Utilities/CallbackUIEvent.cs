using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{
	/******************************************
	 * 
	 * CallbackUIEvent
	 * 
	 * @author Esteban Gallardo
	 */
	public class CallbackUIEvent : MonoBehaviour
	{
		public string UIEventToDispatch = "";

        public void DispatchUIEvent()
        {
			UIEventController.Instance.DispatchUIEvent(UIEventToDispatch);
        }
	}
}