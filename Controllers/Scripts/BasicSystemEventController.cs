using UnityEngine;
using System.Collections.Generic;

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
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_BASICSYSTEMEVENT_OPEN_INFO_TEXT_SCREEN = "EVENT_BASICSYSTEMEVENT_OPEN_INFO_TEXT_SCREEN";
		public const string EVENT_BASICSYSTEMEVENT_OPEN_INFO_IMAGE_SCREEN = "EVENT_BASICSYSTEMEVENT_OPEN_INFO_IMAGE_SCREEN";

        public const string EVENT_BASICSYSTEMEVENT_REQUEST_SIGN_TEXT_DATA   = "EVENT_BASICSYSTEMEVENT_REQUEST_SIGN_TEXT_DATA";
        public const string EVENT_BASICSYSTEMEVENT_RESPONSE_SIGNED_TEXT_DATA = "EVENT_BASICSYSTEMEVENT_RESPONSE_SIGNED_TEXT_DATA";

        public const string EVENT_BASICSYSTEMEVENT_REQUEST_VERIFY_TEXT_DATA = "EVENT_BASICSYSTEMEVENT_REQUEST_VERIFY_TEXT_DATA";
        public const string EVENT_BASICSYSTEMEVENT_RESPONSE_VERIFICATION_TEXT_DATA = "EVENT_BASICSYSTEMEVENT_RESPONSE_VERIFICATION_TEXT_DATA";

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
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public Dictionary<string,object> GlobalVars = new Dictionary<string, object>();

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
		 * Will dispatch a delayed basic system events
		 */
        public void ClearBasicSystemEvents(string _nameEvent = "")
        {
            if (_nameEvent.Length == 0)
            {
                for (int i = 0; i < listEvents.Count; i++)
                {
                    listEvents[i].Time = -1000;
                }
            }
            else
            {
                for (int i = 0; i < listEvents.Count; i++)
                {
                    TimedEventData eventData = listEvents[i];
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
            // DELAYED EVENTS
            for (int i = 0; i < listEvents.Count; i++)
            {
                TimedEventData eventData = listEvents[i];
                if (eventData.Time == -1000)
                {
                    eventData.Destroy();
                    listEvents.RemoveAt(i);
                    break;
                }
                else
                {
                    eventData.Time -= Time.deltaTime;
                    if (eventData.Time <= 0)
                    {
                        if ((eventData != null) && (BasicSystemEvent != null))
                        {
                            BasicSystemEvent(eventData.NameEvent, eventData.List);
                            eventData.Destroy();
                        }
                        listEvents.RemoveAt(i);
                        break;
                    }
                }
            }
        }
	}
}