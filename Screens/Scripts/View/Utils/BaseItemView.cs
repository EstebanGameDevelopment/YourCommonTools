using System;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * SelectableItemView
	 * 
	 * Base class for all selectable items
	 * 
	 * @author Esteban Gallardo
	 */
	public class BaseItemView : MonoBehaviour, IBasicItemView
	{
		// ----------------------
		// PUBLIC EVENTS
		// ----------------------
		public const string EVENT_ITEM_SELECTED = "EVENT_ITEM_SELECTED";
		public const string EVENT_ITEM_CLICKED = "EVENT_ITEM_CLICKED";
		public const string EVENT_ITEM_DELETE = "EVENT_ITEM_DELETE";
		public const string EVENT_ITEM_APPLY_ACTION = "EVENT_ITEM_APPLY_ACTION";

		// PRIVATE MEMBERS
		protected GameObject m_containerParent;
		protected GameObject m_goSelected;
		protected GameObject m_goForeground;
		protected Transform m_btnDelete;
		protected Transform m_btnApplyAction;
		protected bool m_selected = false;

		// GETTERS/SETTERS
		public virtual bool Selected
		{
			get { return m_selected; }
			set
			{
				m_selected = value;
				if (m_selected)
				{
					if (m_goSelected != null) m_goSelected.SetActive(true);
					if (m_goSelected != null) m_goForeground.SetActive(true);
					if (m_btnDelete != null) m_btnDelete.gameObject.SetActive(true);
					if (m_btnApplyAction != null) m_btnApplyAction.gameObject.SetActive(true);
				}
				else
				{
					if (m_goSelected != null) m_goSelected.SetActive(false);
					if (m_goForeground != null) m_goForeground.SetActive(false);
					if (m_btnDelete != null) m_btnDelete.gameObject.SetActive(false);
					if (m_btnApplyAction != null) m_btnApplyAction.gameObject.SetActive(false);
				}
			}
		}
		public GameObject ContainerParent
		{
			get { return m_containerParent; }
			set { m_containerParent = value; }
		}

		public string NameOfScreen
		{
			get
			{
				throw new NotImplementedException();
			}

			set
			{
				throw new NotImplementedException();
			}
		}

        public int Layer
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsMarkedToBeDestroyed
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        // -------------------------------------------
        /* 
		 * Initialitzation of all the references to the graphic resources
		 */
        public virtual void Initialize(params object[] _list)
		{
			string messageNoContainer = "BaseItemView::Initialize::The item should always have a reference to the container parent";
			if (_list == null)
			{
				Debug.LogError(messageNoContainer);
				return;
			}
			if (_list.Length == 0)
			{
				Debug.LogError(messageNoContainer);
				return;
			}
			if (!(_list[0] is GameObject))
			{
				Debug.LogError(messageNoContainer);
				return;
			}
			m_containerParent = (GameObject)_list[0];

			if (transform.Find("Selected") != null)
			{
				m_goSelected = transform.Find("Selected").gameObject;
			}
			if (transform.Find("Foreground") != null)
			{
				m_goForeground = transform.Find("Foreground").gameObject;
			}
			Selected = false;

			m_btnDelete = transform.Find("BtnDelete");
			if (m_btnDelete != null)
			{
				if (m_btnDelete.gameObject.GetComponent<Button>() != null)
				{
					m_btnDelete.gameObject.GetComponent<Button>().onClick.AddListener(OnButtonDelete);
					m_btnDelete.gameObject.SetActive(false);
				}
				else
				{
					m_btnDelete = null;
				}
			}

			m_btnApplyAction = transform.Find("BtnApplyAction");
			if (m_btnApplyAction != null)
			{
				if (m_btnApplyAction.gameObject.GetComponent<Button>() != null)
				{
					m_btnApplyAction.gameObject.GetComponent<Button>().onClick.AddListener(OnButtonApplyAction);
					m_btnApplyAction.gameObject.SetActive(false);
				}
				else
				{
					m_btnApplyAction = null;
				}
			}

			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.AddListener(OnClickButton);
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy all the references		
		 */
		public virtual void Destroy()
		{
			m_containerParent = null;
			if (this.gameObject.GetComponent<Button>() != null)
			{
				this.gameObject.GetComponent<Button>().onClick.RemoveListener(OnClickButton);
			}
			if (m_btnApplyAction != null)
			{
				if (m_btnApplyAction.gameObject.GetComponent<Button>() != null)
				{
					m_btnApplyAction.gameObject.GetComponent<Button>().onClick.RemoveListener(OnButtonApplyAction);
				}
				m_btnApplyAction = null;
			}
			if (m_btnDelete != null)
			{
				if (m_btnDelete.gameObject.GetComponent<Button>() != null)
				{
					m_btnDelete.gameObject.GetComponent<Button>().onClick.RemoveListener(OnButtonDelete);
				}
				m_btnDelete = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Call the button interaction
		 */
		public virtual void OnClickButton()
		{
			Selected = !Selected;
			UIEventController.Instance.DispatchUIEvent(EVENT_ITEM_SELECTED, this.gameObject, Selected);
		}

		// -------------------------------------------
		/* 
		 * Call the delete button 
		 */
		public virtual void OnButtonDelete()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_ITEM_DELETE, this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * Call the delete button 
		 */
		public virtual void OnButtonApplyAction()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_ITEM_APPLY_ACTION, this.gameObject);
		}

		// -------------------------------------------
		/* 
		 * SetActivation
		 */
		public void SetActivation(bool _activation)
		{
			throw new NotImplementedException();
		}


		// -------------------------------------------
		/* 
		 * Runs an action
		 */
		public virtual void ApplyAction()
		{
		}

		bool IBasicView.Destroy()
		{
			throw new NotImplementedException();
		}
	}
}