﻿using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * ItemTextView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ItemTextView : MonoBehaviour, ISlotView
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_ITEM_TEXT_SELECTED = "EVENT_ITEM_TEXT_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_parent;
		private int m_index;
		private string m_data;
		private Image m_background;
		private Text m_text;
		private bool m_selected = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public int Index
		{
			get { return m_index; }
		}
		public string Data
		{
			get { return m_data; }
		}
		public virtual bool Selected
		{
			get { return m_selected; }
			set
			{
				m_selected = value;
				if (m_selected)
				{
					m_background.color = Color.cyan;
				}
				else
				{
					m_background.color = Color.white;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialize(params object[] _list)
		{
			m_parent = (GameObject)((ItemMultiObjectEntry)_list[0]).Objects[0];
			m_index = (int)((ItemMultiObjectEntry)_list[0]).Objects[1];
			m_text = transform.Find("Text").GetComponent<Text>();
			m_data = (string)((ItemMultiObjectEntry)_list[0]).Objects[2];
			m_text.text = m_data;
			m_background = transform.GetComponent<Image>();
			transform.GetComponent<Button>().onClick.AddListener(ButtonPressed);

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public bool Destroy()
		{
			m_parent = null;
			UIEventController.Instance.UIEvent -= OnMenuEvent;
            GameObject.Destroy(this.gameObject);
			return true;
		}


        // -------------------------------------------
        /* 
		 * GetOnClick
		 */
        public UnityEngine.UI.Button.ButtonClickedEvent GetOnClick()
        {
            return transform.GetComponent<Button>().onClick;
        }

        // -------------------------------------------
        /* 
		 * RunOnClick
		 */
        public bool RunOnClick()
        {
            return false;
        }

        // -------------------------------------------
        /* 
		 * OnMenuEvent
		 */
        private void OnMenuEvent(string _nameEvent, object[] _list)
		{
			if (_nameEvent == EVENT_ITEM_TEXT_SELECTED)
			{
				if ((GameObject)_list[0] == m_parent)
				{
					if ((GameObject)_list[1] != this.gameObject)
					{
						Selected = false;
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * ButtonPressed
		 */
		public void ButtonPressed()
		{
			Selected = !Selected;
			UIEventController.Instance.DispatchUIEvent(EVENT_ITEM_TEXT_SELECTED, m_parent, this.gameObject, (Selected?m_index:-1), m_data);
		}

	}
}