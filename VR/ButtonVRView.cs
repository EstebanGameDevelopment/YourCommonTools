using System;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{
	/******************************************
	 * 
	 * ButtonVRView
	 * 
	 * This class will be added automatically (or manually)
	 * to all interactable elements of an screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class ButtonVRView : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SELECTED_VR_BUTTON_COMPONENT = "EVENT_SELECTED_VR_BUTTON_COMPONENT";
		public const string EVENT_CLICKED_VR_BUTTON = "EVENT_CLICKED_VR_BUTTON";
		public const string EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST = "EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string SELECTOR_COMPONENT_NAME = "Selector";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_selector;
		private string m_tagTrigger;

		// -------------------------------------------
		/* 
		 * We add a visual selector (if there is not already one with the name "Selector")
		 * and we also add a box collider to be able for the screen to be used
		 * with systems like Leap Motion
		 */
		public void Initialize(Sprite _selectorGraphic, string _tagTrigger)
		{
			m_tagTrigger = _tagTrigger;
			if (transform.Find(SELECTOR_COMPONENT_NAME) != null)
			{
				m_selector = transform.Find(SELECTOR_COMPONENT_NAME).gameObject;
				m_selector.SetActive(false);
			}
			else
			{
				GameObject nodeImage = new GameObject();
				nodeImage.transform.SetParent(transform, false);
				Rect rectBase = Utilities.Clone(GetComponent<RectTransform>().rect);
                if ((rectBase.width == 0) || (rectBase.height == 0))
                {
                    if (GetComponent<LayoutElement>() != null)
                    {
                        rectBase.width = GetComponent<LayoutElement>().preferredWidth;
                        rectBase.height = GetComponent<LayoutElement>().preferredHeight;
                    }
                }
                Rect mySpriteRect = new Rect(0, 0, _selectorGraphic.rect.width, _selectorGraphic.rect.height);
				Utilities.AddSprite(nodeImage, _selectorGraphic, mySpriteRect, rectBase, new Vector2(0.5f, 0.5f));
				nodeImage.name = SELECTOR_COMPONENT_NAME;
				nodeImage.transform.SetAsFirstSibling();
				m_selector = transform.Find(SELECTOR_COMPONENT_NAME).gameObject;
				m_selector.SetActive(false);
			}
			m_selector.layer = LayerMask.NameToLayer("UI");

			if (GetComponent<Collider>() == null)
			{
				this.gameObject.AddComponent<BoxCollider>();
                if ((GetComponent<RectTransform>().rect.width > 0) && (GetComponent<RectTransform>().rect.height > 0))
                {
                    GetComponent<BoxCollider>().size = new Vector3(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.height, 0.1f);
                }
                else
                {
                    if (GetComponent<LayoutElement>()!=null)
                    {
                        GetComponent<BoxCollider>().size = new Vector3(GetComponent<LayoutElement>().preferredWidth, GetComponent<LayoutElement>().preferredHeight, 0.1f);
                    }
                }
                
				GetComponent<BoxCollider>().isTrigger = true;
			}

			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.AddListener(OnClickedButton);
			}
			else
			{
				if (this.gameObject.GetComponent<Toggle>() != null)
				{
					this.gameObject.GetComponent<Toggle>().onValueChanged.AddListener(OnValueChangedToggle);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy all the references
		 */
		public void Destroy()
		{
            if (this.gameObject.GetComponent<Button>() != null) this.gameObject.GetComponent<Button>().onClick.RemoveListener(OnClickedButton);
            if (this.gameObject.GetComponent<Toggle>() != null) this.gameObject.GetComponent<Toggle>().onValueChanged.RemoveListener(OnValueChangedToggle);
            m_selector = null;
        }

        // -------------------------------------------
        /* 
		 * Triggered when there is collision
		 */
        public void OnTriggerEnter(Collider _collision)
		{
			if (_collision != null)
			{
				if (_collision.gameObject != null)
				{
					if (_collision.gameObject.tag == m_tagTrigger)
					{
						Debug.Log("ButtonVRView::OnTriggerEnter::NAME[" + this.gameObject.name + "]::_collision.collider.gameObject=" + _collision.gameObject.name);
						InvokeButton();
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Will be called to invoke the button functionality
		 */
		public void InvokeButton()
		{
			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.Invoke();
			}
			else
			{
				if (this.gameObject.GetComponent<Toggle>() != null)
				{
					this.gameObject.GetComponent<Toggle>().onValueChanged.Invoke(!this.gameObject.GetComponent<Toggle>().isOn);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Clicked the button
		 */
		public void OnClickedButton()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_CLICKED_VR_BUTTON, this.gameObject);
		}


		// -------------------------------------------
		/* 
		 * Changed the value of toggle
		 */
		public void OnValueChangedToggle(bool _value)
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_CLICKED_VR_BUTTON, this.gameObject, _value);
		}

		// -------------------------------------------
		/* 
		 * Will enable the selector component
		 */
		public void EnableSelector(bool _value)
		{
			if (m_selector != null)
			{
				m_selector.SetActive(_value);
				if (_value)
				{
					if (this.gameObject.GetComponent<RectTransform>() != null)
					{
						Rect corners = Utilities.GetCornersRectTransform(this.gameObject.GetComponent<RectTransform>());
						UIEventController.Instance.DispatchUIEvent(EVENT_CHECK_ELEMENT_CORNER_OUT_OF_LIST, this.gameObject, corners);
					}
				}
			}
		}
	}
}