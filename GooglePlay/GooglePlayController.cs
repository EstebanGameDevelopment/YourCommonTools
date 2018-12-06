using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using YourCommonTools;
using GooglePlayGames;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * GooglePlayController
	 * 
	 * It manages all the Google Play services operations, login and payments.
	 * 
	 * @author Esteban Gallardo
	 */
    public class GooglePlayController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_GOOGLEPLAY_REQUEST_INITIALITZATION    = "EVENT_GOOGLEPLAY_REQUEST_INITIALITZATION";
        public const string EVENT_GOOGLEPLAY_CANCELATION                = "EVENT_GOOGLEPLAY_CANCELATION";
		public const string EVENT_GOOGLEPLAY_COMPLETE_INITIALITZATION   = "EVENT_GOOGLEPLAY_COMPLETE_INITIALITZATION";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static GooglePlayController _instance;

		public static GooglePlayController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(GooglePlayController)) as GooglePlayController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "GooglePlayController";
						_instance = container.AddComponent(typeof(GooglePlayController)) as GooglePlayController;
						DontDestroyOnLoad(_instance);
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
		private string m_id = null;
		private string m_nameHuman;
		private string m_email;
		private bool m_isInited = false;

        public string Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string NameHuman
		{
			get { return m_nameHuman; }
			set { m_nameHuman = value; }
		}
		public string Email
		{
			get { return m_email; }
		}
		public bool IsInited
		{
			get { return m_isInited; }
		}

		// -------------------------------------------
		/* 
		 * InitListener
		 */
		public void InitListener()
		{
			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			UIEventController.Instance.UIEvent -= OnMenuEvent;
			Destroy(_instance.gameObject);
			_instance = null;
		}

        // -------------------------------------------
        /* 
		 * Initialitzation
		 */
        public void Initialitzation()
        {
#if ENABLE_GOOGLE_PLAY
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();

            Social.localUser.Authenticate((bool _success) =>
            {
                if (_success)
                {
                    Debug.LogError("GooglePlayController::Authenticate::SUCCESS");
                    m_id = Social.localUser.id;
                    m_email = (((PlayGamesLocalUser)Social.localUser).Email);
                    m_nameHuman = (((PlayGamesLocalUser)Social.localUser).userName);
                    Debug.LogError("m_email["+ m_email + "]!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    m_isInited = true;
                    UIEventController.Instance.DispatchUIEvent(EVENT_GOOGLEPLAY_COMPLETE_INITIALITZATION);
                }
                else
                {
                    Debug.LogError("GooglePlayController::Authenticate::FAILURE");
                    UIEventController.Instance.DispatchUIEvent(EVENT_GOOGLEPLAY_CANCELATION);
                }
            });
#endif
		}

		// -------------------------------------------
		/* 
		 * OnInitComplete
		 */
		private void OnInitComplete()
		{
#if ENABLE_GOOGLE_PLAY
            UIEventController.Instance.DispatchUIEvent(EVENT_GOOGLEPLAY_COMPLETE_INITIALITZATION);
#endif

        }

        // -------------------------------------------
        /* 
		 * OnBasicEvent
		 */
        private void OnMenuEvent(string _nameEvent, params object[] _list)
		{
		}
	}
}

