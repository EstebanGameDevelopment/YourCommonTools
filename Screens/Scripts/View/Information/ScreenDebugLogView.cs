using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * ScreenDebugLogView
	 * 
	 * Screen used to display debug log error messages
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenDebugLogView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME			= "SCREEN_DEBUGLOG";

        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREEN_DEBUGLOG_NEW_TEXT	    = "EVENT_SCREEN_DEBUGLOG_NEW_TEXT";
        public const string EVENT_SCREEN_DEBUGLOG_CLEAR_TEXT	= "EVENT_SCREEN_DEBUGLOG_CLEAR_TEXT";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
		private Transform m_container;
		private Text m_textDescription;

        private Button m_clearButton;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

            if (m_container.Find("Button_Clear") != null)
			{
				m_clearButton = m_container.Find("Button_Clear").GetComponent<Button>();
                m_clearButton.gameObject.GetComponent<Button>().onClick.AddListener(ClearPressed);
                if (m_clearButton.gameObject.transform.Find("Text") != null)
                {
                    m_clearButton.gameObject.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.clear");
                }
			}

			if (m_container.Find("Text") != null)
			{
				m_textDescription = m_container.Find("Text").GetComponent<Text>();
                m_textDescription.text = "Inited";
            }

			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
        }

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
            return DestroyReal();
		}

        // -------------------------------------------
        /* 
		 * DestroyReal
		 */
        public bool DestroyReal()
        {
            if (base.Destroy()) return true;

            UIEventController.Instance.UIEvent -= OnUIEvent;

            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

            return false;
        }

        // -------------------------------------------
        /* 
		 * ClearPressed
		 */
        private void ClearPressed()
		{
            if (m_textDescription != null) m_textDescription.text = "";
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
				ClearPressed();
			}
			if (_nameEvent == EVENT_SCREEN_DEBUGLOG_NEW_TEXT)
			{
                bool clearPrevious = (bool)_list[0];
                if (clearPrevious)
                {
                    if (m_textDescription != null) m_textDescription.text = "";
                }
                if (m_textDescription != null) m_textDescription.text = (string)_list[1] + "\n" + m_textDescription.text;
			}
            if (_nameEvent == EVENT_SCREEN_DEBUGLOG_CLEAR_TEXT)
            {
                if (m_textDescription != null) m_textDescription.text = "";
            }
        }
    }
}