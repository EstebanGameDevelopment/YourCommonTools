using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * ScreenInformationView
	 * 
	 * Screen used to display pages of information
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenInformationView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_INFORMATION			= "SCREEN_INFORMATION";
		public const string SCREEN_CONFIRMATION			= "SCREEN_CONFIRMATION";
		public const string SCREEN_INFORMATION_IMAGE	= "SCREEN_INFORMATION_IMAGE";
        public const string SCREEN_INFORMATION_ICON     = "SCREEN_INFORMATION_ICON";
        public const string SCREEN_WAIT					= "SCREEN_WAIT";
        public const string SCREEN_BIG_WAIT	    		= "SCREEN_BIG_WAIT";
        public const string SCREEN_IMAGE_WAIT	    	= "SCREEN_IMAGE_WAIT";
		public const string SCREEN_INITIAL_CONNECTION	= "SCREEN_INITIAL_CONNECTION";
        public const string SCREEN_UNLOCK_CURRENCY      = "SCREEN_UNLOCK_CURRENCY";
        public const string SCREEN_UNLOCK_GAME          = "SCREEN_UNLOCK_GAME";
        public const string SCREEN_CHANGE_NETWORK		= "SCREEN_CHANGE_NETWORK";
		public const string SCREEN_FIT_SCAN				= "SCREEN_FIT_SCAN";
        public const string SCREEN_DIALOG               = "SCREEN_DIALOG";
        public const string SCREEN_TREE                 = "SCREEN_TREE";
        public const string SCREEN_INPUT                = "SCREEN_INPUT";

        // -------------------------------------------
        /* 
		 * CheckNameInformationScreen
		 */
        public static bool CheckNameGenericScreen(string _screenName)
        {
            return (_screenName.IndexOf(SCREEN_INFORMATION)!=-1) ||
                (_screenName.IndexOf(SCREEN_CONFIRMATION) != -1) ||
                (_screenName.IndexOf(SCREEN_INFORMATION_IMAGE) != -1) ||
                (_screenName.IndexOf(SCREEN_INFORMATION_ICON) != -1) ||
                (_screenName.IndexOf(SCREEN_WAIT) != -1) ||
                (_screenName.IndexOf(SCREEN_BIG_WAIT) != -1) ||
                (_screenName.IndexOf(SCREEN_IMAGE_WAIT) != -1) ||
                (_screenName.IndexOf(SCREEN_INITIAL_CONNECTION) != -1) ||
                (_screenName.IndexOf(SCREEN_UNLOCK_CURRENCY) != -1) ||
                (_screenName.IndexOf(SCREEN_UNLOCK_GAME) != -1) ||
                (_screenName.IndexOf(SCREEN_CHANGE_NETWORK) != -1) ||
                (_screenName.IndexOf(SCREEN_INFORMATION) != -1) ||
                (_screenName.IndexOf(SCREEN_FIT_SCAN) != -1) ||
                (_screenName.IndexOf(SCREEN_INPUT) != -1) ||
                (_screenName.IndexOf(SCREEN_DIALOG) != -1);
        }

        // -------------------------------------------
        /* 
		 * CheckNameInformationScreen
		 */
        public static bool CheckNameInformationScreen(string _screenName)
        {
            return (_screenName.IndexOf(SCREEN_INFORMATION) != -1) ||
                (_screenName.IndexOf(SCREEN_CONFIRMATION) != -1) ||
                (_screenName.IndexOf(SCREEN_INFORMATION_IMAGE) != -1) ||
                (_screenName.IndexOf(SCREEN_INFORMATION_ICON) != -1);
        }

        // -------------------------------------------
        /* 
		 * CheckNameConfirmationScreen
		 */
        public static bool CheckNameConfirmationScreen(string _screenName)
        {
            return  (_screenName.IndexOf(SCREEN_CONFIRMATION)!=-1);
        }

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREEN_UPDATE_TEXT_TITLE	        = "EVENT_SCREEN_UPDATE_TEXT_TITLE";
        public const string EVENT_SCREEN_UPDATE_TEXT_DESCRIPTION	= "EVENT_SCREEN_UPDATE_TEXT_DESCRIPTION";
        public const string EVENT_SCREEN_UPDATE_TEXTS_BUTTONS       = "EVENT_SCREEN_UPDATE_TEXTS_BUTTONS";
        public const string EVENT_SCREEN_ENABLE_OK_BUTTON			= "EVENT_SCREEN_ENABLE_OK_BUTTON";
        public const string EVENT_SCREEN_FADE_BACKGROUND            = "EVENT_SCREEN_FADE_BACKGROUND";
        public const string EVENT_SCREEN_RESET_ANIMATION_PARAMS     = "EVENT_SCREEN_RESET_ANIMATION_PARAMS";

        public const string EVENT_SCREEN_INFORMATION_DISPLAYED = "EVENT_SCREEN_INFORMATION_DISPLAYED";
        public const string EVENT_SCREEN_INFORMATION_CLOSED = "EVENT_SCREEN_INFORMATION_CLOSED";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		private Transform m_container;
        private Transform m_background;
        private Button m_okButton;
		private Button m_cancelButton;
		private Button m_nextButton;
		private Button m_previousButton;
		private Button m_abortButton;
		private Text m_textDescription;
        private Text m_title;
		private Image m_imageContent;

		private int m_currentPage = 0;
		private List<PageInformation> m_pagesInfo = new List<PageInformation>();
		private bool m_forceLastPage = false;
		private bool m_lastPageVisited = false;

        private float m_timeAcumProgress;
        private int m_dotNumber;
        private Text m_textProgress;

        private InputField m_inputField = null;

        private bool m_animationDissappearTriggered = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool ForceLastPage
		{
			get { return m_forceLastPage; }
			set { m_forceLastPage = value; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			List<PageInformation> listPages = (List<PageInformation>)_list[0];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

            if (m_root.transform.Find("Background") != null)
            {
                m_background = m_root.transform.Find("Background");
            }

            if (m_container.Find("Button_OK") != null)
			{
				m_okButton = m_container.Find("Button_OK").GetComponent<Button>();
				m_okButton.gameObject.GetComponent<Button>().onClick.AddListener(OkPressed);
                if (m_okButton.gameObject.transform.Find("Text") != null)
                {
                    m_okButton.gameObject.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.ok");
                }
			}
			if (m_container.Find("Button_Cancel") != null)
			{
				m_cancelButton = m_container.Find("Button_Cancel").GetComponent<Button>();
				m_cancelButton.gameObject.GetComponent<Button>().onClick.AddListener(CancelPressed);
                if (m_cancelButton.gameObject.transform.Find("Text") != null)
                {
                    m_cancelButton.gameObject.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.cancel");
                }
            }
            if (m_container.Find("Button_Next") != null)
			{
				m_nextButton = m_container.Find("Button_Next").GetComponent<Button>();
				m_nextButton.gameObject.GetComponent<Button>().onClick.AddListener(NextPressed);
			}
			if (m_container.Find("Button_Previous") != null)
			{
				m_previousButton = m_container.Find("Button_Previous").GetComponent<Button>();
				m_previousButton.gameObject.GetComponent<Button>().onClick.AddListener(PreviousPressed);
			}
			if (m_container.Find("Button_Abort") != null)
			{
				m_abortButton = m_container.Find("Button_Abort").GetComponent<Button>();
				m_abortButton.gameObject.GetComponent<Button>().onClick.AddListener(AbortPressed);
			}

			if (m_container.Find("Text") != null)
			{
				m_textDescription = m_container.Find("Text").GetComponent<Text>();
                if (m_textDescription.gameObject.GetComponent<Button>() != null) m_textDescription.gameObject.GetComponent<Button>().onClick.AddListener(ActionTextPressed);
            }
            if (m_container.Find("Title") != null)
			{
				m_title = m_container.Find("Title").GetComponent<Text>();
			}
            if (m_container.Find("Progress") != null)
            {
                m_timeAcumProgress = 0;
                m_dotNumber = 0;
                m_textProgress = m_container.Find("Progress").GetComponent<Text>();
                m_textProgress.text = ".";
            }

            if (m_container.Find("Image") != null)
			{
				m_imageContent = m_container.Find("Image").GetComponent<Image>();
                if (m_imageContent.gameObject.GetComponent<Button>() != null) m_imageContent.gameObject.GetComponent<Button>().onClick.AddListener(ActionImagePressed);
            }

            if (m_container.Find("InputField") != null)
            {
                m_inputField = m_container.Find("InputField").GetComponent<InputField>();
            }

            if (m_container.Find("Button_Unlock") != null)
            {
                Button unlockButton = m_container.Find("Button_Unlock").GetComponent<Button>();
                unlockButton.gameObject.GetComponent<Button>().onClick.AddListener(UnlockPressed);
                if (unlockButton.gameObject.transform.Find("Text") != null)
                {
                    unlockButton.gameObject.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.unlock.game");
                }
            }

            if (listPages != null)
			{
				for (int i = 0; i < listPages.Count; i++)
				{
                    m_pagesInfo.Add(listPages[i].Clone());
				}
			}

			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);

			ChangePage(0);

            if (UIEventController.Instance.ActivateAutoDestruction != -1)
            {
                UIEventController.Instance.DelayUIEvent(ScreenController.EVENT_FORCE_DESTRUCTION_POPUP, UIEventController.Instance.ActivateAutoDestruction);
            }
            UIEventController.Instance.DispatchUIEvent(EVENT_SCREEN_INFORMATION_DISPLAYED);
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public override bool Destroy()
		{
            if ((m_paramsAnimation != null) && (!m_animationDissappearTriggered))
            {
                if (!m_animationDissappearTriggered)
                {
                    m_animationDissappearTriggered = true;
                    DisappearAnimation(m_paramsAnimation, false);
                }                
                return true;
            }
            else
            {
                return DestroyReal();
            }			
		}

        // -------------------------------------------
        /* 
		 * DestroyReal
		 */
        public bool DestroyReal()
        {
            if (base.Destroy()) return true;

            UIEventController.Instance.UIEvent -= OnUIEvent;
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;

            UIEventController.Instance.DispatchUIEvent(EVENT_SCREEN_INFORMATION_CLOSED);
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject, this.gameObject.name);

            return false;
        }

        // -------------------------------------------
        /* 
		 * ActionImagePressed
		 */
        private void ActionImagePressed()
        {
            if (m_pagesInfo[m_currentPage].EventData.Length > 0)
            {
                if (m_pagesInfo[m_currentPage].EventData.IndexOf("http") != -1)
                {
                    Application.OpenURL(m_pagesInfo[m_currentPage].EventData);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * ActionTextPressed
		 */
        private void ActionTextPressed()
        {
            if (m_pagesInfo[m_currentPage].EventData.Length > 0)
            {
                if (m_pagesInfo[m_currentPage].EventData.IndexOf("http") != -1)
                {
                    Application.OpenURL(m_pagesInfo[m_currentPage].EventData);
                }                
            }
        }

        // -------------------------------------------
        /* 
		 * OkPressed
		 */
        private void OkPressed()
		{
			if (m_currentPage + 1 < m_pagesInfo.Count)
			{
				ChangePage(1);
				return;
			}

            if (m_inputField != null)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_CONFIRMATION_POPUP, this.gameObject, true, m_pagesInfo[m_currentPage].EventData, m_inputField.text);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_CONFIRMATION_POPUP, this.gameObject, true, m_pagesInfo[m_currentPage].EventData);
            }            
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * CancelPressed
		 */
		private void CancelPressed()
		{
            if (m_inputField != null)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_currentPage].EventData, m_inputField.text);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_currentPage].EventData);
            }
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * AbortPressed
		 */
		private void AbortPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * NextPressed
		 */
		private void NextPressed()
		{
			ChangePage(1);
		}

		// -------------------------------------------
		/* 
		 * PreviousPressed
		 */
		private void PreviousPressed()
		{
			ChangePage(-1);
		}

        // -------------------------------------------
        /* 
		 * UnlockPressed
		 */
        private void UnlockPressed()
        {
#if ENABLE_IAP
            IAPController.Instance.BuyProductID(m_pagesInfo[0].EventData);
            Destroy();
#endif
        }

        // -------------------------------------------
        /* 
		 * Chage the information page
		 */
        private void ChangePage(int _value)
		{
			m_currentPage += _value;
			if (m_currentPage < 0) m_currentPage = 0;
			if (m_pagesInfo.Count == 0)
			{
				return;
			}
			else
			{
				if (m_currentPage >= m_pagesInfo.Count - 1)
				{
					m_currentPage = m_pagesInfo.Count - 1;
					m_lastPageVisited = true;
				}
			}

			if ((m_currentPage >= 0) && (m_currentPage < m_pagesInfo.Count))
			{
                if (m_title != null) m_title.text = LanguageController.Instance.GetText(m_pagesInfo[m_currentPage].MyTitle);
                if (m_textDescription != null) m_textDescription.text = LanguageController.Instance.GetText(m_pagesInfo[m_currentPage].MyText);
                if (m_imageContent != null)
				{
					if (m_pagesInfo[m_currentPage].MySprite != null)
					{
						m_imageContent.sprite = m_pagesInfo[m_currentPage].MySprite;
					}
				}
			}

			if (m_cancelButton != null) m_cancelButton.gameObject.SetActive(true);
			if (m_pagesInfo.Count == 1)
			{
				if (m_nextButton != null) m_nextButton.gameObject.SetActive(false);
				if (m_previousButton != null) m_previousButton.gameObject.SetActive(false);
				if (m_okButton != null) m_okButton.gameObject.SetActive(true);
			}
			else
			{
				if (m_currentPage == 0)
				{
					if (m_previousButton != null) m_previousButton.gameObject.SetActive(false);
					if (m_nextButton != null) m_nextButton.gameObject.SetActive(true);
				}
				else
				{
					if (m_currentPage == m_pagesInfo.Count - 1)
					{
						if (m_previousButton != null) m_previousButton.gameObject.SetActive(true);
						if (m_nextButton != null) m_nextButton.gameObject.SetActive(false);
					}
					else
					{
						if (m_previousButton != null) m_previousButton.gameObject.SetActive(true);
						if (m_nextButton != null) m_nextButton.gameObject.SetActive(true);
					}
				}

				UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_CHANGED_PAGE_POPUP, this.gameObject, m_pagesInfo[m_currentPage].EventData);
			}
		}

		// -------------------------------------------
		/* 
		 * SetTitle
		 */
		public void SetTitle(string _text)
		{
			if (m_title != null)
			{
				m_title.text = _text;
			}
        }

        // -------------------------------------------
        /* 
         * OnBasicSystemEvent
         */
        private void OnBasicSystemEvent(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == EVENT_SCREEN_FADE_BACKGROUND)
            {
                if (m_background != null)
                {
                    m_background.GetComponent<CanvasGroup>().alpha = (float)_list[0];
                    AlphaController.Instance.Interpolate(m_background.gameObject, (float)_list[0], (float)_list[1], (float)_list[2]);
                }
            }

            if (m_animationDissappearTriggered)
            {
                if (_nameEvent == InterpolateData.EVENT_INTERPOLATE_STARTED)
                {
                    if (m_canvasGroup.gameObject == (GameObject)_list[0])
                    {
                        if (m_paramsAnimation != null)
                        {
                            if (m_background != null)
                            {
                                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_SCREEN_FADE_BACKGROUND, 1f, 0f, 0.4f);
                            }
                        }
                    }
                }
                if (_nameEvent == InterpolateData.EVENT_INTERPOLATE_COMPLETED)
                {
                    if (m_canvasGroup.gameObject == (GameObject)_list[0])
                    {
                        m_paramsAnimation = null;
                        DestroyReal();
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
         * OnUIEvent
         */
        private void OnUIEvent(string _nameEvent, params object[] _list)
		{
            base.OnMenuEvent(_nameEvent, _list);

            if (_nameEvent == ScreenController.EVENT_FORCE_DESTRUCTION_POPUP)
			{
				Destroy();
			}
			if (_nameEvent == ScreenController.EVENT_FORCE_TRIGGER_OK_BUTTON)
			{
				OkPressed();
			}
            if (_nameEvent == EVENT_SCREEN_UPDATE_TEXT_TITLE)
            {
                if (m_title != null) m_title.text = (string)_list[0];
            }
			if (_nameEvent == EVENT_SCREEN_UPDATE_TEXT_DESCRIPTION)
			{
				if (m_textDescription != null) m_textDescription.text = (string)_list[0];
			}
            if (_nameEvent == EVENT_SCREEN_UPDATE_TEXTS_BUTTONS)
            {
                if (m_okButton.gameObject.transform.Find("Text") != null)
                {
                    m_okButton.gameObject.transform.Find("Text").GetComponent<Text>().text = (string)_list[0];
                }
                if (m_cancelButton.gameObject.transform.Find("Text") != null)
                {
                    m_cancelButton.gameObject.transform.Find("Text").GetComponent<Text>().text = (string)_list[1];
                }
            }
            if (_nameEvent == EVENT_SCREEN_ENABLE_OK_BUTTON)
			{
				if (m_okButton != null) m_okButton.gameObject.SetActive((bool)_list[0]);
			}
			if (_nameEvent == ScreenController.EVENT_FORCE_DESTRUCTION_WAIT)
			{
				if ((m_nameOfScreen == SCREEN_WAIT) || (m_nameOfScreen == SCREEN_BIG_WAIT) || (m_nameOfScreen == SCREEN_IMAGE_WAIT))
				{
					Destroy();
				}
			}
            if (_nameEvent == EVENT_SCREEN_RESET_ANIMATION_PARAMS)
            {
                m_animationDissappearTriggered = true;
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SUBEVENT_SCREEN)
            {
                string subEvent = (string)_list[0];
                for (int i = 0; i < m_pagesInfo.Count; i++)
                {
                    if (m_pagesInfo[i].EventData == subEvent)
                    {
                        Destroy();
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
         * Update
         */
        void Update()
        {
            if (m_textProgress != null)
            {
                m_timeAcumProgress += Time.deltaTime;
                if (m_timeAcumProgress > 0.5f)
                {
                    m_timeAcumProgress = 0;
                    m_dotNumber = (m_dotNumber + 1) % 3;
                    string dots = "";
                    for (int i = 0; i < m_dotNumber + 1; i++)
                    {
                        dots += ".";
                    }
                    m_textProgress.text = dots;
                }
            }
        }
    }
}