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
        public const string EVENT_BUTTONVR_SELECTED_INPUTFIELD = "EVENT_BUTTONVR_SELECTED_INPUTFIELD";

        public const string EVENT_BUTTONVR_REQUEST_LAYER_INFORMATION = "EVENT_BUTTONVR_REQUEST_LAYER_INFORMATION";
        public const string EVENT_BUTTONVR_RESPONSE_LAYER_INFORMATION = "EVENT_BUTTONVR_RESPONSE_LAYER_INFORMATION";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const string SELECTOR_COMPONENT_NAME = "Selector";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private bool m_hasBeenInitialized = false;
        private GameObject m_selector;
		private string m_tagTrigger;
        private bool m_isInputField = false;

        private int m_layerScreen = 0;
        private string m_nameScreen = "";

        private bool m_disableSelectable = false;

        public bool IsSelected
        {
            get { return m_selector.activeSelf; }
        }

        // -------------------------------------------
        /* 
		 * We add a visual selector (if there is not already one with the name "Selector")
		 * and we also add a box collider to be able for the screen to be used
		 * with systems like Leap Motion
		 */
        public void Initialize(Sprite _selectorGraphic, string _tagTrigger, int _layerScreen, string _nameScreen)
		{
            if (m_hasBeenInitialized) return;
            m_hasBeenInitialized = true;
            m_hasBeenDestroyed = false;

            m_layerScreen = _layerScreen;
            m_nameScreen = _nameScreen;

            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

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
                    if (GetComponent<LayoutElement>() != null)
                    {
                        GetComponent<BoxCollider>().size = new Vector3(GetComponent<LayoutElement>().preferredWidth, GetComponent<LayoutElement>().preferredHeight, 0.1f);
                    }
                }
                
				GetComponent<BoxCollider>().isTrigger = true;
			}

            if (this.gameObject.GetComponent<ICustomButton>() != null)
            {
                this.gameObject.GetComponent<ICustomButton>().GetOnClick().AddListener(OnClickedButton);
            }
            else
            {
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
                    else
                    {
                        if (this.gameObject.GetComponent<InputField>() != null)
                        {
                            m_isInputField = true;
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        private void OnDestroy()
        {
            Destroy();
        }

        private bool m_hasBeenDestroyed = false;

        // -------------------------------------------
        /* 
		 * Destroy all the references
		 */
        public void Destroy()
		{
            if (m_hasBeenDestroyed) return;
            m_hasBeenDestroyed = true;

            m_hasBeenInitialized = false;

            UIEventController.Instance.UIEvent -= OnUIEvent;

            if (this.gameObject.GetComponent<ICustomButton>() != null) this.gameObject.GetComponent<ICustomButton>().GetOnClick().RemoveListener(OnClickedButton);
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
            UIEventController.Instance.DispatchUIEvent(EVENT_BUTTONVR_REQUEST_LAYER_INFORMATION, this.gameObject);
        }

        // -------------------------------------------
        /* 
		 * Will be called to invoke the button functionality
		 */
        private void RunInvokeButton()
        {
            if (this.gameObject.GetComponent<ICustomButton>() != null)
            {
                if (!this.gameObject.GetComponent<ICustomButton>().RunOnClick())
                {
                    this.gameObject.GetComponent<ICustomButton>().GetOnClick().Invoke();
                }
            }
            else
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
            if (!m_disableSelectable)
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

        // -------------------------------------------
        /* 
		 * Will enable the selector component
		 */
        public void DisableSelectable()
        {
            if (m_selector != null)
            {
                m_selector.SetActive(false);
                m_disableSelectable = true;
            }
        }

        // -------------------------------------------
        /* 
		 * OnUIEvent
		 */
        private void OnUIEvent(string _nameEvent, object[] _list)
        {
            if (this == null)
            {
                Destroy();
            }
            else
            {
                if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
                {
                    if (m_isInputField)
                    {
                        if (m_selector.activeSelf)
                        {
                            m_selector.SetActive(false);
                            UIEventController.Instance.DispatchUIEvent(EVENT_BUTTONVR_SELECTED_INPUTFIELD, this.gameObject.GetComponent<InputField>());
                        }
                    }
                }
                if (_nameEvent == EVENT_BUTTONVR_RESPONSE_LAYER_INFORMATION)
                {
                    if (this.gameObject == (GameObject)_list[0])
                    {
                        int currentMaxLayer = (int)_list[1];
                        if (m_layerScreen == currentMaxLayer)
                        {
                            RunInvokeButton();
                        }
                    }
                }
            }
        }

    }
}