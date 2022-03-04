using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * ScreenListItemsView
	 * 
	 * Display the a generic list of items
	 * 
	 * @author Esteban Gallardo
	 */
    public class ScreenListItemsView : ScreenBaseView, IBasicView
    {
        public const string SCREEN_NAME = "SCREEN_LIST_ITEMS";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public GameObject StringItemPrefab;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_root;
        private Transform m_container;
        private SlotManagerView m_slotmanager;
        private string m_selectedData;

        private Button m_actionButton;
        private Button m_buttonBack;

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

            m_actionButton = m_container.Find("Button_Action").GetComponent<Button>();
            m_container.Find("Button_Action/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.ok");
            m_actionButton.onClick.AddListener(OnActionPressed);

            m_buttonBack = m_container.Find("Button_Back").GetComponent<Button>();
            m_buttonBack.onClick.AddListener(BackPressed);

            // SLOT MANAGER
            m_slotmanager = m_container.Find("ListItems").GetComponent<SlotManagerView>();
            
            List<ItemMultiObjectEntry> sampleItems = new List<ItemMultiObjectEntry>();
            sampleItems.Add(new ItemMultiObjectEntry(this.gameObject, 0, "HOLA"));
            sampleItems.Add(new ItemMultiObjectEntry(this.gameObject, 1, "MON"));
            sampleItems.Add(new ItemMultiObjectEntry(this.gameObject, 2, "HELLO"));
            sampleItems.Add(new ItemMultiObjectEntry(this.gameObject, 3, "WORLD"));
            sampleItems.Add(new ItemMultiObjectEntry(this.gameObject, 4, "GUTEN"));
            sampleItems.Add(new ItemMultiObjectEntry(this.gameObject, 5, "MORGEN"));
             
            m_slotmanager.Initialize(4, sampleItems, StringItemPrefab);

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

            if (m_slotmanager != null)
            {
                m_slotmanager.Destroy();
                m_slotmanager = null;
            }

            return false;
        }

        // -------------------------------------------
        /* 
		 * BackPressed
		 */
        private void BackPressed()
        {
            Utilities.DebugLogError("ScreenListItemsView::BackPressed");
            // SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
            // UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenMenuMainView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
        }

        // -------------------------------------------
        /* 
		 * OnActionPressed
		 */
        private void OnActionPressed()
        {
            if (m_selectedData != null)
            {
                Utilities.DebugLogError("ScreenListItemsView::OnActionPressed::SELECTED["+ m_selectedData + "]");
            }
            else
            {
                Utilities.DebugLogError("ScreenListItemsView::OnActionPressed::NO ITEM SELECTED");
            }
        }

        // -------------------------------------------
        /*
		* OnMenuBasicEvent
		*/
        protected override void OnMenuEvent(string _nameEvent, params object[] _list)
        {
            base.OnMenuEvent(_nameEvent, _list);

            if (_nameEvent == ItemTextView.EVENT_ITEM_TEXT_SELECTED)
            {
                GameObject parentObject = (GameObject)_list[0];
                if (this.gameObject == parentObject)
                {
                    if ((int)_list[2] != -1)
                    {
                        m_selectedData = (string)_list[3];
                    }
                    else
                    {
                        m_selectedData = null;
                    }                    
                }
            }
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