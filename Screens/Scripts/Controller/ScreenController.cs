using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{

	public enum UIScreenTypePreviousAction
	{
		DESTROY_ALL_SCREENS = 0x00,
		DESTROY_CURRENT_SCREEN = 0x01,
		KEEP_CURRENT_SCREEN = 0x02,
		HIDE_CURRENT_SCREEN = 0x03
	}

	/******************************************
	 * 
	 * ScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_CHANGED_PAGE_POPUP = "EVENT_CHANGED_PAGE_POPUP";
		public const string EVENT_CONFIRMATION_POPUP = "EVENT_CONFIRMATION_POPUP";
		public const string EVENT_FORCE_DESTRUCTION_POPUP = "EVENT_FORCE_DESTRUCTION_POPUP";
		public const string EVENT_FORCE_TRIGGER_OK_BUTTON = "EVENT_FORCE_TRIGGER_OK_BUTTON";
		public const string EVENT_FORCE_DESTRUCTION_WAIT = "EVENT_FORCE_DESTRUCTION_WAIT";

        public const string EVENT_APP_LOST_FOCUS = "EVENT_APP_LOST_FOCUS";
        public const string EVENT_APP_PAUSED = "EVENT_APP_PAUSED";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        [Tooltip("It allows the debug of most common messages")]
		public bool DebugMode = true;

		[Tooltip("This shader will be applied on the UI elements and it allows to draw over everything else so the screen is not hidden by another object")]
		public Material MaterialDrawOnTop;

		[Tooltip("All the screens used by the application")]
		public GameObject[] ScreensPrefabs;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		protected List<GameObject> m_screensPool = new List<GameObject>();
		protected List<GameObject> m_screensOverlay = new List<GameObject>();
		protected bool m_enableScreens = true;
		protected bool m_enableDebugTestingCode = false;
		protected bool m_hasBeenInitialized = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool EnableDebugTestingCode
		{
			get { return m_enableDebugTestingCode; }
			set { m_enableDebugTestingCode = value; }
		}
		public int ScreensEnabled
		{
			get { return m_screensPool.Count + m_screensOverlay.Count; }
		}
        public List<GameObject> ScreensPool
        {
            get { return m_screensPool; }
        }
        public List<GameObject> ScreensOverlay
        {
            get { return m_screensOverlay; }
        }

        // -------------------------------------------
        /* 
		 * Force orientation of the device to landscape
		 */
        public virtual void Awake()
		{
		}

		// -------------------------------------------
		/* 
		 * Initialitzation listener
		 */
		public virtual void Start()
		{
			if (DebugMode)
			{
				Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public virtual void Destroy()
		{
			DestroyScreensPool();
			DestroyScreensOverlay();
		}

        // -------------------------------------------
        /* 
		 * OnApplicationFocus
		 */
        void OnApplicationFocus(bool hasFocus)
        {
            UIEventController.Instance.DispatchUIEvent(EVENT_APP_LOST_FOCUS, hasFocus);
        }

        // -------------------------------------------
        /* 
         * OnApplicationPause
         */
        void OnApplicationPause(bool pauseStatus)
        {
            UIEventController.Instance.DispatchUIEvent(EVENT_APP_PAUSED, pauseStatus);
        }

        // -------------------------------------------
        /* 
		 * Information screen
		 */
        public virtual void CreatePopUpScreenInfo(string _title, string _description, string _eventData)
		{
			CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, _title, _description, null, _eventData);
		}

        // -------------------------------------------
        /* 
		 * Confirmation screen
		 */
        public virtual void CreatePopUpScreenConfirmation(string _title, string _description, string _eventData)
		{
			CreateNewInformationScreen(ScreenInformationView.SCREEN_CONFIRMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, _title, _description, null, _eventData);
		}

        // -------------------------------------------
        /* 
		 * Create a new screen
		 */
        public virtual void CreateNewInformationScreen(string _nameScreen, UIScreenTypePreviousAction _previousAction, string _title, string _description, Sprite _image, string _eventData, string _okButtonText = "", string _cancelButtonText = "")
		{
			List<PageInformation> pages = new List<PageInformation>();
			pages.Add(new PageInformation(_title, _description, _image, _eventData, _okButtonText, _cancelButtonText));

			CreateNewScreen(_nameScreen, _previousAction, false, pages);
		}

        // -------------------------------------------
        /* 
		 * Create a new screen
		 */
        public virtual void CreateNewScreenNoParameters(string _nameScreen, UIScreenTypePreviousAction _previousAction)
		{
			CreateNewScreen(_nameScreen, _previousAction, true, null);
		}

        // -------------------------------------------
        /* 
		 * Create a new screen
		 */
        public virtual void CreateNewScreenNoParameters(string _nameScreen, bool _hidePreviousScreens, UIScreenTypePreviousAction _previousAction)
		{
			CreateNewScreen(_nameScreen, _previousAction, _hidePreviousScreens, null);
		}

        // -------------------------------------------
        /* 
		 * Create a new screen
		 */
        public virtual void CreateNewInformationScreen(string _nameScreen, UIScreenTypePreviousAction _previousAction, List<PageInformation> _pages)
		{
			CreateNewScreen(_nameScreen, _previousAction, false, _pages);
		}

		// -------------------------------------------
		/* 
		* Create a new screen
		*/
		public virtual void CreateNewScreen(string _nameScreen, UIScreenTypePreviousAction _previousAction, bool _hidePreviousScreens, params object[] _list)
		{
			if (!m_enableScreens) return;

			if (DebugMode)
			{
				Debug.Log("EVENT_SCREENMANAGER_OPEN_SCREEN::Creating the screen[" + _nameScreen + "]");
			}
			if (_hidePreviousScreens)
			{
				EnableAllScreens(false);
			}

			// PREVIOUS ACTION
			switch (_previousAction)
			{
				case UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN:
					if (m_screensPool.Count > 0)
					{
						m_screensPool[m_screensPool.Count - 1].SetActive(false);
					}
					break;

				case UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN:
					break;

				case UIScreenTypePreviousAction.DESTROY_CURRENT_SCREEN:
					if (m_screensPool.Count > 0)
					{
						GameObject sCurrentScreen = m_screensPool[m_screensPool.Count - 1];
						if (sCurrentScreen.GetComponent<IBasicView>() != null)
						{
							sCurrentScreen.GetComponent<IBasicView>().Destroy();
						}
						GameObject.Destroy(sCurrentScreen);
						m_screensPool.RemoveAt(m_screensPool.Count - 1);
					}
					break;

				case UIScreenTypePreviousAction.DESTROY_ALL_SCREENS:
					DestroyScreensPool();
					DestroyScreensOverlay();
					break;
			}

			// CREATE SCREEN
			GameObject currentScreen = null;
			for (int i = 0; i < ScreensPrefabs.Length; i++)
			{
				if (ScreensPrefabs[i] != null)
				{
					if (ScreensPrefabs[i].name == _nameScreen)
					{
						currentScreen = (GameObject)Instantiate(ScreensPrefabs[i]);
						currentScreen.GetComponent<IBasicView>().Initialize(_list);
						currentScreen.GetComponent<IBasicView>().NameOfScreen = _nameScreen;
						break;
					}
				}
			}

			if (_hidePreviousScreens)
			{
				m_screensPool.Add(currentScreen);
			}
			else
			{
				m_screensOverlay.Add(currentScreen);
			}

			if (DebugMode)
			{
				Utilities.DebugLogError("CreateNewScreen::POOL[" + m_screensPool.Count + "]::OVERLAY[" + m_screensOverlay.Count + "]");
			}
		}

        // -------------------------------------------
        /* 
		 * GetScreenPrefabByName
		 */
        public GameObject GetScreenPrefabByName(string _nameScreen)
        {
            for (int i = 0; i < ScreensPrefabs.Length; i++)
            {
                if (ScreensPrefabs[i] != null)
                {
                    if (ScreensPrefabs[i].name == _nameScreen)
                    {
                        return ScreensPrefabs[i];
                    }
                }
            }
            return null;
        }

		// -------------------------------------------
		/* 
		 * Destroy all the screens in memory
		 */
		public void DestroyScreensPool()
		{
			for (int i = 0; i < m_screensPool.Count; i++)
			{
				GameObject screen = m_screensPool[i];
				if (screen != null)
				{
					if (screen.GetComponent<IBasicView>() != null)
					{
						screen.GetComponent<IBasicView>().Destroy();
					}
					if (screen != null)
					{
						GameObject.Destroy(screen);
						screen = null;
					}
				}
			}
			m_screensPool.Clear();
		}

		// -------------------------------------------
		/* 
		 * Destroy all the screens in memory
		 */
		public void DestroyScreensOverlay()
		{
			for (int i = 0; i < m_screensOverlay.Count; i++)
			{
				GameObject screen = m_screensOverlay[i];
				if (screen != null)
				{
					if (screen.GetComponent<IBasicView>() != null)
					{
						screen.GetComponent<IBasicView>().Destroy();
					}
					if (screen != null)
					{
						GameObject.Destroy(screen);
						screen = null;
					}
				}
			}
			m_screensOverlay.Clear();
		}


		// -------------------------------------------
		/* 
		 * Remove the screen from the list of screens
		 */
		private void DestroyGameObjectSingleScreen(GameObject _screen, bool _runDestroy)
		{
			if (_screen == null) return;

			for (int i = 0; i < m_screensPool.Count; i++)
			{
				GameObject screen = (GameObject)m_screensPool[i];
				if (_screen == screen)
				{
					if (_runDestroy)
					{
						screen.GetComponent<IBasicView>().Destroy();
					}
					if (screen != null)
					{
						GameObject.Destroy(screen);
					}
					if (i < m_screensPool.Count) m_screensPool.RemoveAt(i);
					return;
				}
			}

			for (int i = 0; i < m_screensOverlay.Count; i++)
			{
				GameObject screen = (GameObject)m_screensOverlay[i];
				if (_screen == screen)
				{
					if (_runDestroy)
					{
						screen.GetComponent<IBasicView>().Destroy();
					}
					if (screen != null)
					{
						GameObject.Destroy(screen);
					}
					if (i < m_screensOverlay.Count) m_screensOverlay.RemoveAt(i);
					return;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Changes the enable of the screens
		 */
		private void EnableAllScreens(bool _activation)
		{
			for (int i = 0; i < m_screensPool.Count; i++)
			{
				if (m_screensPool[i] != null)
				{
					if (m_screensPool[i].GetComponent<IBasicView>() != null)
					{
						m_screensPool[i].GetComponent<IBasicView>().SetActivation(_activation);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Changes the enable of the screens
		 */
		private void EnableScreens(bool _activation)
		{
			if (m_screensPool.Count > 0)
			{
				if (m_screensPool[m_screensPool.Count - 1] != null)
				{
					if (m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicView>() != null)
					{
						m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicView>().SetActivation(_activation);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of ui events
		 */
		protected virtual void OnUIEvent(string _nameEvent, params object[] _list)
		{
            ProcessScreenEvents(_nameEvent, _list);
        }

        // -------------------------------------------
        /* 
		 * ProcessScreenEvents
		 */
        protected void ProcessScreenEvents(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN)
            {
                string nameScreen = (string)_list[0];
                UIScreenTypePreviousAction previousAction = (UIScreenTypePreviousAction)_list[1];
                bool hidePreviousScreens = (bool)_list[2];
                List<PageInformation> pages = null;
                if (_list.Length > 3)
                {
                    if (_list[3] is List<PageInformation>)
                    {
                        pages = (List<PageInformation>)_list[3];
                        CreateNewScreen(nameScreen, previousAction, hidePreviousScreens, pages);
                    }
                    else
                    {
                        object[] dataParams = new object[_list.Length - 3];
                        for (int k = 3; k < _list.Length; k++)
                        {
                            dataParams[k - 3] = _list[k];
                        }
                        CreateNewScreen(nameScreen, previousAction, hidePreviousScreens, dataParams);
                    }
                }
                else
                {
                    CreateNewScreen(nameScreen, previousAction, hidePreviousScreens, pages);
                }                
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN)
            {
                string nameScreen = (string)_list[0];
                UIScreenTypePreviousAction previousAction = (UIScreenTypePreviousAction)_list[1];
                string title = (string)_list[2];
                string description = (string)_list[3];
                Sprite image = (Sprite)_list[4];
                string eventData = (string)_list[5];
                List<PageInformation> pages = new List<PageInformation>();
                pages.Add(new PageInformation(title, description, image, eventData));
                CreateNewScreen(nameScreen, previousAction, false, pages);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN)
            {
                m_enableScreens = true;
                GameObject screen = (GameObject)_list[0];
                DestroyGameObjectSingleScreen(screen, true);
                EnableScreens(true);
                if (DebugMode)
                {
                    Utilities.DebugLogError("EVENT_SCREENMANAGER_DESTROY_SCREEN::POOL[" + m_screensPool.Count + "]::OVERLAY[" + m_screensOverlay.Count + "]");
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN)
            {
                DestroyScreensOverlay();
                DestroyScreensPool();
                if (DebugMode)
                {
                    Utilities.DebugLogError("EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN::POOL[" + m_screensPool.Count + "]::OVERLAY[" + m_screensOverlay.Count + "]");
                }
            }
            if (_nameEvent == EVENT_CONFIRMATION_POPUP)
            {
                GameObject screen = (GameObject)_list[0];
                bool accepted = (bool)_list[1];
                string subnameEvent = (string)_list[2];
                if (screen != null) Debug.Log("POP UP[" + screen.name + "] CLOSED");
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_POOL_DESTROY_LAST)
            {
                if (m_screensPool.Count > 0)
                {
                    if (m_screensPool[m_screensPool.Count - 1] != null)
                    {
                        if (m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicView>() != null)
                        {
                            GameObject refObject = m_screensPool[m_screensPool.Count - 1];
                            m_screensPool.RemoveAt(m_screensPool.Count - 1);
                            refObject.GetComponent<IBasicView>().Destroy();
                            refObject = null;
                            EnableScreens(true);
                            if (DebugMode)
                            {
                                Debug.LogError("EVENT_SCREENMANAGER_POOL_DESTROY_LAST::POOL[" + m_screensPool.Count + "]::OVERLAY[" + m_screensOverlay.Count + "]");
                            }
                            return;
                        }
                    }
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OVERLAY_DESTROY_LAST)
            {
                if (m_screensOverlay.Count > 0)
                {
                    if (m_screensOverlay[m_screensOverlay.Count - 1] != null)
                    {
                        if (m_screensOverlay[m_screensOverlay.Count - 1].GetComponent<IBasicView>() != null)
                        {
                            GameObject refObject = m_screensOverlay[m_screensOverlay.Count - 1];
                            m_screensOverlay.RemoveAt(m_screensOverlay.Count - 1);
                            refObject.GetComponent<IBasicView>().Destroy();
                            refObject = null;
                            if (DebugMode)
                            {
                                Utilities.DebugLogError("EVENT_SCREENMANAGER_OVERLAY_DESTROY_LAST::POOL[" + m_screensPool.Count + "]::OVERLAY[" + m_screensOverlay.Count + "]");
                            }
                            return;
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        public virtual void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON);
			}
		}
	}
}