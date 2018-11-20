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
		public const string SCREEN_WAIT					= "SCREEN_WAIT";
		public const string SCREEN_INITIAL_CONNECTION	= "SCREEN_INITIAL_CONNECTION";
        public const string SCREEN_UNLOCK_CURRENCY      = "SCREEN_UNLOCK_CURRENCY";
        public const string SCREEN_CHANGE_NETWORK		= "SCREEN_CHANGE_NETWORK";
		public const string SCREEN_FIT_SCAN				= "SCREEN_FIT_SCAN";
        public const string SCREEN_DIALOG               = "SCREEN_DIALOG";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREEN_UPDATE_TEXT_DESCRIPTION	= "EVENT_SCREEN_UPDATE_TEXT_DESCRIPTION";
        public const string EVENT_SCREEN_UPDATE_TEXTS_BUTTONS       = "EVENT_SCREEN_UPDATE_TEXTS_BUTTONS";
        public const string EVENT_SCREEN_ENABLE_OK_BUTTON			= "EVENT_SCREEN_ENABLE_OK_BUTTON";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		private Transform m_container;
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
			}
			if (m_container.Find("Title") != null)
			{
				m_title = m_container.Find("Title").GetComponent<Text>();
			}

			if (m_container.Find("Image") != null)
			{
				m_imageContent = m_container.Find("Image").GetComponent<Image>();
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
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
            if (m_paramsAnimation != null)
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

            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

            return false;
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

			UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_CONFIRMATION_POPUP, this.gameObject, true, m_pagesInfo[m_currentPage].EventData);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * CancelPressed
		 */
		private void CancelPressed()
		{
			UIEventController.Instance.DispatchUIEvent(ScreenController.EVENT_CONFIRMATION_POPUP, this.gameObject, false, m_pagesInfo[m_currentPage].EventData);
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
				if (m_title != null) m_title.text = m_pagesInfo[m_currentPage].MyTitle;
				if (m_textDescription != null) m_textDescription.text = m_pagesInfo[m_currentPage].MyText;
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
            if (m_animationDissappearTriggered)
            {
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
				if (m_nameOfScreen == SCREEN_WAIT)
				{
					Destroy();
				}
			}
		}
	}
}