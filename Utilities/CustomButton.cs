using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace YourCommonTools
{
    /******************************************
     * 
     * CustomButton
     * 
     * @author Esteban Gallardo
     */
    public class CustomButton : Button
    {
        public const string BUTTON_PRESSED_DOWN = "BUTTON_PRESSED_DOWN";
        public const string BUTTON_RELEASE_UP   = "BUTTON_RELEASE_UP";

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            UIEventController.Instance.DispatchUIEvent(BUTTON_PRESSED_DOWN, this.gameObject);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            UIEventController.Instance.DispatchUIEvent(BUTTON_RELEASE_UP, this.gameObject);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            // UIEventController.Instance.DispatchUIEvent(BUTTON_RELEASE_UP, this.gameObject);
        }

    }
}