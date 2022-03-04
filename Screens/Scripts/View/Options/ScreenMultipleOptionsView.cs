using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * ScreenMultipleOptionsView
	 * 
	 * Multiple buttons to select between different options
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenMultipleOptionsView : ScreenBaseView, IBasicView
    {
        public const string SCREEN_NAME = "SCREEN_MULTIPLE_OPTIONS";

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
        private Transform m_container;

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public override void Initialize(params object[] _list)
        {
            base.Initialize(_list);

            m_root = this.gameObject;
            m_container = m_root.transform.Find("Content");

#if !ALTERNATIVE_TITLE
            m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");
#else
			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.mobile.title");
#endif

            GameObject optionOne = m_container.Find("Button_OptionOne").gameObject;
            optionOne.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.option.one");
            optionOne.GetComponent<Button>().onClick.AddListener(OptionOnePressed);

            GameObject optionTwo = m_container.Find("Button_OptionTwo").gameObject;
            optionTwo.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.option.two");
            optionTwo.GetComponent<Button>().onClick.AddListener(OptionTwoPressed);

            m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);

            UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);
        }

        // -------------------------------------------
        /* 
		 * GetGameObject
		 */
        public GameObject GetGameObject()
        {
            return this.gameObject;
        }

        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public override bool Destroy()
        {
            if (base.Destroy()) return true;

            UIEventController.Instance.UIEvent -= OnMenuEvent;
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

            return false;
        }

        // -------------------------------------------
        /* 
		 * OptionOnePressed
		 */
        private void OptionOnePressed()
        {
            Utilities.DebugLogError("SELECTED OPTION ONE");
        }

        // -------------------------------------------
        /* 
		 * OptionTwoPressed
		 */
        private void OptionTwoPressed()
        {
            Utilities.DebugLogError("SELECTED OPTION TWO");
        }

        // -------------------------------------------
        /* 
		 * Exit button pressed
		 */
        private void BackPressed()
        {
            // SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            // UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
        }

        // -------------------------------------------
        /* 
		* OnMenuBasicEvent
		*/
        protected override void OnMenuEvent(string _nameEvent, params object[] _list)
        {
            base.OnMenuEvent(_nameEvent, _list);

            if (this.gameObject.activeSelf)
            {
                if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
                {
                    BackPressed();
                }
            }
        }
    }
}
