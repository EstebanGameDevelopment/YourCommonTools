using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

namespace YourCommonTools
{
	public delegate void BasicSystemEventHandler(string _nameEvent, params object[] _list);

	/******************************************
	 * 
	 * BasicEventController
	 * 
	 * Class used to dispatch events through all the system
	 * 
	 * @author Esteban Gallardo
	 */
	public class BasicSystemEventController : MonoBehaviour
	{
		public event BasicSystemEventHandler BasicSystemEvent;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static BasicSystemEventController _instance;

		public static BasicSystemEventController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(BasicSystemEventController)) as BasicSystemEventController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						DontDestroyOnLoad(container);
						container.name = "BasicSystemEventController";
						_instance = container.AddComponent(typeof(BasicSystemEventController)) as BasicSystemEventController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private List<TimedEventData> listEvents = new List<TimedEventData>();

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private BasicSystemEventController()
		{
		}

		// -------------------------------------------
		/* 
		 * OnDestroy
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
			if (_instance != null)
			{
				Destroy(_instance.gameObject);
				_instance = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Will dispatch a basic system event
		 */
		public void DispatchBasicSystemEvent(string _nameEvent, params object[] _list)
		{
			if (BasicSystemEvent != null) BasicSystemEvent(_nameEvent, _list);
		}

		// -------------------------------------------
		/* 
		 * Will add a new delayed local event to the queue
		 */
		public void DelayBasicSystemEvent(string _nameEvent, float _time, params object[] _list)
		{
			listEvents.Add(new TimedEventData(_nameEvent, _time, _list));
		}

		
		// -------------------------------------------
		/* 
		 * Will process the queue of delayed events 
		 */
		void Update()
		{
			// DELAYED EVENTS
			for (int i = 0; i < listEvents.Count; i++)
			{
				TimedEventData eventData = listEvents[i];
				eventData.Time -= Time.deltaTime;
				if (eventData.Time <= 0)
				{
					BasicSystemEvent(eventData.NameEvent, eventData.List);
					eventData.Destroy();
					listEvents.RemoveAt(i);
					break;
				}
			}
		}
	}
}