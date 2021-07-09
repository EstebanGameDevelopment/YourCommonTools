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
    public class CustomButton : Button, ICustomButton
    {
        public const string BUTTON_PRESSED_DOWN = "BUTTON_PRESSED_DOWN";
        public const string BUTTON_RELEASE_UP   = "BUTTON_RELEASE_UP";

        public int Id;

        public Sprite[] DefaultSprites;
        public Sprite[] HighlightSprites;
        public Sprite[] PressedSprites;
        public Sprite[] DisabledSprites;

        public void SetSprites(int _index)
        {
            if (_index < DefaultSprites.Length) GetComponent<Image>().sprite = DefaultSprites[_index];
            if ((_index < HighlightSprites.Length) && (_index < PressedSprites.Length) && (_index < DisabledSprites.Length))
            {
                SpriteState ss = new SpriteState();
                ss.highlightedSprite = HighlightSprites[_index];
                ss.pressedSprite = PressedSprites[_index];
                ss.disabledSprite = DisabledSprites[_index];

                GetComponent<Button>().spriteState = ss;
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            
            UIEventController.Instance.DispatchUIEvent(BUTTON_PRESSED_DOWN, this.gameObject, eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            UIEventController.Instance.DispatchUIEvent(BUTTON_RELEASE_UP, this.gameObject, eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            // UIEventController.Instance.DispatchUIEvent(BUTTON_RELEASE_UP, this.gameObject);
        }

        public ButtonClickedEvent GetOnClick()
        {
            return onClick;
        }

        public bool RunOnClick()
        {
            UIEventController.Instance.DispatchUIEvent(BUTTON_PRESSED_DOWN, this.gameObject);
            return true;
        }
    }
}