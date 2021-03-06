using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using YourCommonTools;
#if ENABLE_FACEBOOK
using Facebook.Unity;
#endif

namespace YourCommonTools
{

	/******************************************
	 * 
	 * FacebookController
	 * 
	 * It manages all the Facebook operations, login and payments.
	 * 
	 * @author Esteban Gallardo
	 */
	public class FacebookController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_FACEBOOK_REQUEST_INITIALITZATION = "EVENT_FACEBOOK_REQUEST_INITIALITZATION";
        public const string EVENT_FACEBOOK_CANCELATION = "EVENT_FACEBOOK_CANCELATION";
        public const string EVENT_FACEBOOK_MY_INFO_LOADED = "EVENT_FACEBOOK_MY_INFO_LOADED";
		public const string EVENT_FACEBOOK_FRIENDS_LOADED = "EVENT_FACEBOOK_FRIENDS_LOADED";
		public const string EVENT_FACEBOOK_COMPLETE_INITIALITZATION = "EVENT_FACEBOOK_COMPLETE_INITIALITZATION";
		public const string EVENT_REGISTER_IAP_COMPLETED = "EVENT_REGISTER_IAP_COMPLETED";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static FacebookController _instance;

		public static FacebookController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(FacebookController)) as FacebookController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "FacebookController";
						_instance = container.AddComponent(typeof(FacebookController)) as FacebookController;
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
		private bool m_invitationAccepted = false;

		private List<ItemMultiTextEntry> m_friends = new List<ItemMultiTextEntry>();
        private string m_friendsCompact = "";

        private string m_accessToken = "";

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
		public List<ItemMultiTextEntry> Friends
		{
			get { return m_friends; }
		}
		public bool IsInited
		{
			get { return m_isInited; }
		}
		public bool InvitationAccepted
		{
			get { return m_invitationAccepted; }
		}
        public string AccessToken
        {
            get { return m_accessToken; }
        }

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private FacebookController()
		{
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
#if ENABLE_FACEBOOK
			if (!FB.IsInitialized)
			{
				if (!m_isInited)
				{
					// ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
					InitListener();
					FB.Init(this.OnInitComplete, this.OnHideUnity);
                }
				else
				{
					InitListener();
					RegisterConnectionFacebookID(true);
				}
			}
			else
			{
				// Already initialized, signal an app activation App Event
				InitListener();
				FB.ActivateApp();
				OnInitComplete();
			}
#endif
		}

        // -------------------------------------------
        /* 
		 * Logout
		 */
        public void Logout()
        {
#if ENABLE_FACEBOOK
            if (FB.IsInitialized && FB.IsLoggedIn)
            {
                FB.LogOut();
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * OnInitComplete
		 */
        private void OnInitComplete()
		{
#if ENABLE_FACEBOOK
			UIEventController.Instance.DispatchUIEvent(EVENT_FACEBOOK_REQUEST_INITIALITZATION);
			Debug.Log("Success - Check log for details");
			Debug.Log("Success Response: OnInitComplete Called");
			Debug.Log("OnInitCompleteCalled IsLoggedIn='{" + FB.IsLoggedIn + "}' IsInitialized='{" + FB.IsInitialized + "}'");
            if (FB.IsInitialized)
            {
                LogInWithPermissions();
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(EVENT_FACEBOOK_CANCELATION);
            }
#endif
		}

		// -------------------------------------------
		/* 
		 * OnHideUnity
		 */
		private void OnHideUnity(bool _isGameShown)
		{
			Debug.Log("Success - Check log for details");
			Debug.Log("Success Response: OnHideUnity Called {" + _isGameShown + "}");
			Debug.Log("Is game shown: " + _isGameShown);
            UIEventController.Instance.DispatchUIEvent(EVENT_FACEBOOK_CANCELATION);
        }

		// -------------------------------------------
		/* 
		 * LogInWithPermissions
		 */
		private void LogInWithPermissions()
		{
#if ENABLE_FACEBOOK
#if ENABLE_FACEBOOK_FRIENDS
            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, LoggedWithPermissions);
#else
            FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email" }, LoggedWithPermissions);
#endif
#endif
        }

        // -------------------------------------------
        /* 
		 * LoggedWithPermissions
		 */
#if ENABLE_FACEBOOK
        private void LoggedWithPermissions(IResult _result)
		{
			if (_result == null)
			{
                UIEventController.Instance.DispatchUIEvent(EVENT_FACEBOOK_CANCELATION);
                return;
			}

            if (Facebook.Unity.AccessToken.CurrentAccessToken != null)
            {
                m_accessToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;
            }

            if (FB.IsLoggedIn)
            {
                // if (ScreenController.Instance.DebugMode) Debug.Log("FacebookController::LoggedWithPermissions::result.RawResult=" + _result.RawResult);
                FB.API("/me?fields=id,name,email", HttpMethod.GET, HandleMyInformation);
            }
            else
            {
                UIEventController.Instance.DispatchUIEvent(EVENT_FACEBOOK_CANCELATION);
            }
		}
#endif

		// -------------------------------------------
		/* 
		 * HandleMyInformation
		 */
#if ENABLE_FACEBOOK
		private void HandleMyInformation(IResult _result)
		{
			if (_result == null)
			{
				// if (ScreenController.Instance.DebugMode) Debug.Log("Null Response");
				return;
			}

			JSONNode jsonResponse = JSONNode.Parse(_result.RawResult);

			m_id = jsonResponse["id"];
			m_nameHuman = jsonResponse["name"];
			m_email = jsonResponse["email"];

			// if (ScreenController.Instance.DebugMode) Debug.Log("CURRENT PLAYER NAME=" + m_nameHuman + ";ID=" + m_id);

			UIEventController.Instance.DispatchUIEvent(EVENT_FACEBOOK_MY_INFO_LOADED);

#if ENABLE_FACEBOOK_FRIENDS
            // if (ScreenController.Instance.DebugMode) Debug.Log("FacebookController::HandleMyInformation::result.RawResult=" + _result.RawResult);
            FB.API("/me/friends", HttpMethod.GET, HandleListOfFriends);
#else
            RegisterConnectionFacebookID(true);
#endif
        }
#endif

        // -------------------------------------------
        /* 
         * HandleListOfFriends
         */
#if ENABLE_FACEBOOK
        private void HandleListOfFriends(IResult _result)
		{
			if (_result == null)
			{
				Debug.Log("HandleListOfFriends::Null Response");
				return;
			}

			Debug.Log("FacebookController::HandleListOfFriends::result.RawResult=" + _result.RawResult);
			JSONNode jsonResponse = JSONNode.Parse(_result.RawResult);

			JSONNode friends = jsonResponse["data"];
			Debug.Log("FacebookController::HandleListOfFriends::friends.Count=" + friends.Count);
            m_friendsCompact = "";
			for (int i = 0; i < friends.Count; i++)
			{
				string nameFriend = friends[i]["name"];
				string idFriend = friends[i]["id"];
				m_friends.Add(new ItemMultiTextEntry(idFriend, nameFriend));
                if (m_friendsCompact.Length > 0)
                {
                    m_friendsCompact += ";";
                }
                m_friendsCompact += idFriend;
                Debug.Log("   NAME=" + nameFriend + ";ID=" + idFriend);
			}

			UIEventController.Instance.DispatchUIEvent(EVENT_FACEBOOK_FRIENDS_LOADED);

			// INIT PAYMENT METHOD
			RegisterConnectionFacebookID(true);
		}
#endif

		// -------------------------------------------
		/* 
		* RegisterConnectionFacebookID
		*/
		public void RegisterConnectionFacebookID(bool _dispatchCompletedFacebookInit)
		{
			// START BASIC CONNECTION
			if (m_id != null)
			{
				m_isInited = true;
			}
			else
			{
				m_isInited = false;
			}
			if (_dispatchCompletedFacebookInit)
			{
                DispatchCompletedConnectionEvent();
            }
		}

        // -------------------------------------------
        /* 
		* DispatchCompletedConnectionEvent
		*/
        public bool DispatchCompletedConnectionEvent()
        {
            if (m_id == null)
            {
                return false;
            }
            else
            {
                UIEventController.Instance.DelayUIEvent(EVENT_FACEBOOK_COMPLETE_INITIALITZATION, 0.1f, m_id, m_nameHuman, m_email, m_friendsCompact);
                return true;
            }            
        }

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnMenuEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_REGISTER_IAP_COMPLETED)
			{
			}
		}

		// -------------------------------------------
		/* 
		 * GetNameOfFriendID
		 */
		public string GetNameOfFriendID(string _facebookID)
		{
			for (int i = 0; i < m_friends.Count; i++)
			{
				if (m_friends[i].Items[0] == _facebookID)
				{
					return m_friends[i].Items[1];
				}
			}

			return null;
		}

		// -------------------------------------------
		/* 
		* GetPackageFriends
		*/
		public string GetPackageFriends()
		{
			string output = "";
			for (int i = 0; i < m_friends.Count; i++)
			{
				output += m_friends[i].Items[0] + "," + m_friends[i].Items[1];
				if (i < m_friends.Count - 1)
				{
					output += ";";
				}
			}
			return output;
		}

		// -------------------------------------------
		/* 
		 * SetFriends
		 */
		public void SetFriends(string _data)
		{
			string[] friendsList = _data.Split(';');
			m_friends.Clear();
			for (int i = 0; i < friendsList.Length; i++)
			{
				string[] sFriendEntry = friendsList[i].Split(',');
				if (sFriendEntry.Length == 2)
				{
					m_friends.Add(new ItemMultiTextEntry(sFriendEntry[0], sFriendEntry[1]));
					// if (ScreenController.Instance.DebugMode) Debug.Log("FacebookController::SetFriends::FRIEND[" + sFriendEntry[0] + "][" + sFriendEntry[1] + "]");
				}
			}
		}

	}
}

