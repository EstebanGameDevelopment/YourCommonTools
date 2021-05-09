using UnityEngine;
using System.Collections.Generic;

namespace YourCommonTools
{

	public delegate void OculusEventHandler(string _nameEvent, params object[] _list);

	/******************************************
	 * 
	 * OculusEventController
	 * 
	 * Observer class that triggers Oculus events
	 * 
	 * @author Esteban Gallardo
	 */
	public class OculusEventObserver : MonoBehaviour
	{
		public event OculusEventHandler OculusEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static OculusEventObserver instance;

		public static OculusEventObserver Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(OculusEventObserver)) as OculusEventObserver;
					if (!instance)
					{
						GameObject container = new GameObject();
						DontDestroyOnLoad(container);
						container.name = "OculusEventController";
						instance = container.AddComponent(typeof(OculusEventObserver)) as OculusEventObserver;
					}
				}
				return instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<OculusEventData> m_listEvents = new List<OculusEventData>();

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			if (instance != null)
			{
				Destroy(instance.gameObject);
				instance = null;
			}
		}

        // -------------------------------------------
        /* 
		 * Will dispatch a Oculus event
		 */
        public void DispatchOculusEvent(string _nameEvent, params object[] _list)
		{
            if (instance == null) return;

			if (OculusEvent != null) OculusEvent(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a delayed Oculus event
		 */
		public void DelayOculusEvent(string _nameEvent, float _time, params object[] _list)
		{
			if (instance == null) return;

			m_listEvents.Add(new OculusEventData(_nameEvent, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a delayed Oculus event
		 */
		public void ClearOculusEvents(string _nameEvent = "")
		{
			if (_nameEvent.Length == 0)
			{
				for (int i = 0; i < m_listEvents.Count; i++)
				{
					m_listEvents[i].Time = -1000;
				}
			}
			else
			{
				for (int i = 0; i < m_listEvents.Count; i++)
				{
					OculusEventData eventData = m_listEvents[i];
					if (eventData.NameEvent == _nameEvent)
					{
						eventData.Time = -1000;
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Will process the queue of delayed events 
		 */
		void Update()
		{
			if (instance == null) return;

			// DELAYED EVENTS
			for (int i = 0; i < m_listEvents.Count; i++)
			{
				OculusEventData eventData = m_listEvents[i];
				if (eventData.Time == -1000)
				{
					eventData.Destroy();
					m_listEvents.RemoveAt(i);
					break;
				}
				else
				{
					eventData.Time -= Time.deltaTime;
					if (eventData.Time <= 0)
					{
						if (OculusEvent != null) OculusEvent(eventData.NameEvent, eventData.ListParameters);
						eventData.Destroy();
						m_listEvents.RemoveAt(i);
						break;
					}
				}
			}
		}

	}
}
