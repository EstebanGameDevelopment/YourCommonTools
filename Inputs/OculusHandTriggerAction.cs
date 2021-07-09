using System;
using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * OculusHandTriggerAction
	 * 
	 * @author Esteban Gallardo
	 */
    public class OculusHandTriggerAction : MonoBehaviour
	{
        public const string EVENT_HANDTRIGGERACTION_OPEN_MENU = "EVENT_HANDTRIGGERACTION_OPEN_MENU";

        public void DispatchActionButtonDown()
        {
            UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_BUTTON_DOWN);
        }

        public void DispatchMenuButtonDown()
        {
            UIEventController.Instance.DispatchUIEvent(EVENT_HANDTRIGGERACTION_OPEN_MENU);
        }
    }

}