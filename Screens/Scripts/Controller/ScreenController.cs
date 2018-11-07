using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

	public enum UIScreenTypePreviousAction
	{
		DESTROY_ALL_SCREENS = 0x00,
		DESTROY_CURRENT_SCREEN = 0x01,
		KEEP_CURRENT_SCREEN = 0x02,
		HIDE_CURRENT_SCREEN = 0x03,
        HIDE_ALL_SCREENS = 0x04
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

        public const int TOTAL_LAYERS_SCREENS = 5;

        public const int ANIMATION_MOVEMENT = 0;
        public const int DIRECTION_UP       = 0;
        public const int DIRECTION_DOWN     = 1;
        public const int DIRECTION_LEFT     = 2;
        public const int DIRECTION_RIGHT    = 3;

        public const int ANIMATION_ALPHA    = 1;

        public const int ANIMATION_FADE     = 2;

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        [Tooltip("It allows the debug of most common messages")]
        public bool DebugMode = true;

        [Tooltip("This shader will be applied on the UI elements and it allows to draw over everything else so the screen is not hidden by another object")]
        public Material MaterialDrawOnTop;

        [Tooltip("All the screens used by the application")]
        public GameObject[] ScreensPrefabs;

        [Tooltip("Screen used for fade to black")]
        public GameObject FadeScreen;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        protected Dictionary<int, List<GameObject>> m_screensPool = new Dictionary<int, List<GameObject>>();
        protected bool m_enableScreens = true;
        protected bool m_enableDebugTestingCode = false;
        protected bool m_hasBeenInitialized = false;

        protected List<GameObject> m_layers = new List<GameObject>();

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
            get {
                int totalScreens = 0;
                foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
                {
                    totalScreens += screenPool.Value.Count;
                }
                return totalScreens;
            }
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

            CreateNewScreenLayer(1, null, _nameScreen, _previousAction, false, pages);
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
            CreateNewScreenLayer(1, null, _nameScreen, _previousAction, false, _pages);
        }

        // -------------------------------------------
        /* 
		* Create a new screen
		*/
        public virtual void CreateNewScreen(string _nameScreen, UIScreenTypePreviousAction _previousAction, bool _ignore, params object[] _list)
        {
            CreateNewScreenLayer(0, null, _nameScreen, _previousAction, _list);
        }

        // -------------------------------------------
        /* 
		* Create a new screen
		*/
        public virtual void CreateNewScreenLayer(int _layer, object _animation, string _nameScreen, UIScreenTypePreviousAction _previousAction, params object[] _list)
        {
            if (!m_enableScreens) return;

            if (m_layers.Count == 0)
            {
                for (int i = 0; i < TOTAL_LAYERS_SCREENS; i++)
                {
                    GameObject layer = Utilities.AttachChild(this.transform, new GameObject());
                    layer.name = "Layer_" + i;
                    m_layers.Add(layer);
                    m_screensPool.Add(i, new List<GameObject>());
                }
            }

            if (DebugMode)
            {
                Debug.Log("EVENT_SCREENMANAGER_OPEN_SCREEN::Creating the screen[" + _nameScreen + "]");
            }

            // PREVIOUS ACTION
            switch (_previousAction)
            {
                case UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN:
                    if (m_screensPool[_layer].Count > 0)
                    {
                        for (int k = 0; k < m_screensPool[_layer].Count; k++)
                        {
                            m_screensPool[_layer][k].SetActive(false);
                        }
					}
					break;

                case UIScreenTypePreviousAction.HIDE_ALL_SCREENS:
                    foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
                    {
                        for (int i = 0; i < screenPool.Value.Count;i++)
                        {
                            screenPool.Value[i].SetActive(false);
                        }
                    }
                    break;

                case UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN:
					break;

				case UIScreenTypePreviousAction.DESTROY_CURRENT_SCREEN:
                    if (m_screensPool[_layer].Count > 0)
                    {
                        DestroyGameObjectSingleScreen(m_screensPool[_layer][m_screensPool[_layer].Count - 1], true);
                    }
					break;

				case UIScreenTypePreviousAction.DESTROY_ALL_SCREENS:
                    DestroyScreensPool();
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
                        currentScreen = (GameObject)Utilities.AddChild(m_layers[_layer].transform, ScreensPrefabs[i]);
                        currentScreen.GetComponent<Canvas>().sortingOrder = _layer;
                        currentScreen.GetComponent<IBasicView>().Initialize(_list);
						currentScreen.GetComponent<IBasicView>().NameOfScreen = _nameScreen;
						break;
					}
				}
			}

            m_screensPool[_layer].Add(currentScreen);
            currentScreen.GetComponent<IBasicView>().Layer = _layer;

            if (_animation != null)
            {
                UIEventController.Instance.DispatchUIEvent(ScreenBaseView.EVENT_SCREENBASE_ANIMATION_SHOW, currentScreen, _animation);
            }

			if (DebugMode)
			{
				Utilities.DebugLogError("CreateNewScreen::POOL[" + ScreensEnabled + "]");
			}
        }

        // -------------------------------------------
        /* 
         * AreThereLayersOverMe
         */
        public bool AreThereLayersOverMe(int _layer)
        {
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                if (screenPool.Value.Count > 0)
                {
                    if (screenPool.Key > _layer)
                    {
                        return true;
                    }
                }                
            }

            return false;
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
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                while (screenPool.Value.Count > 0)
                {
                    GameObject screen = screenPool.Value[0];
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
                screenPool.Value.Clear();
            }
            if (DebugMode)
            {
                Utilities.DebugLogError("ScreenController::DestroyScreensPool::POOL[" + ScreensEnabled + "]-----------------------------------------------------------");
            }
        }

        // -------------------------------------------
        /* 
		 * Destroy all the screens below a specific layer
		 */
        public void DestroyScreensBelowLayerPool(int _layer)
        {
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                if (screenPool.Key < _layer)
                {
                    while (screenPool.Value.Count > 0)
                    {
                        GameObject screen = screenPool.Value[0];
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
                    screenPool.Value.Clear();
                }
            }
            if (DebugMode)
            {
                Utilities.DebugLogError("ScreenController::DestroyScreensPool::POOL[" + ScreensEnabled + "]-----------------------------------------------------------");
            }
        }

        // -------------------------------------------
        /* 
		 * Remove the screen from the list of screens
		 */
        private void DestroyGameObjectSingleScreen(GameObject _screen, bool _runDestroy)
		{
			if (_screen == null) return;

            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                for (int i = 0; i < screenPool.Value.Count; i++)
                {
                    GameObject screen = screenPool.Value[i];
                    if (screen == _screen)
                    {
                        if (_runDestroy)
                        {
                            screen.GetComponent<IBasicView>().Destroy();
                        }
                        if (screen != null)
                        {
                            GameObject.Destroy(screen);
                        }
                        if (i < m_screensPool.Count) screenPool.Value.RemoveAt(i);
                        return;
                    }
                }
            }
		}

		// -------------------------------------------
		/* 
		 * Changes the enable of the screens
		 */
		private void EnableAllScreens(int _layer, bool _activation)
		{
            for (int i = 0; i < m_screensPool[_layer].Count; i++)
            {
                GameObject screen = m_screensPool[_layer][i];
                if (screen != null)
                {
                    if (screen.GetComponent<IBasicView>() != null)
                    {
                        screen.GetComponent<IBasicView>().SetActivation(_activation);
                    }
                }
            }
		}

		// -------------------------------------------
		/* 
		 * Changes the enable of the screens
		 */
		private void EnableScreens(int _layer, bool _activation)
		{
			if (m_screensPool[_layer].Count > 0)
			{
                List<GameObject> screensPool = m_screensPool[_layer];

                if (screensPool[screensPool.Count - 1] != null)
				{
					if (screensPool[screensPool.Count - 1].GetComponent<IBasicView>() != null)
					{
                        screensPool[screensPool.Count - 1].GetComponent<IBasicView>().SetActivation(_activation);
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
                        CreateNewScreenLayer(0, null, nameScreen, previousAction, pages);
                    }
                    else
                    {
                        object[] dataParams = new object[_list.Length - 3];
                        for (int k = 3; k < _list.Length; k++)
                        {
                            dataParams[k - 3] = _list[k];
                        }
                        CreateNewScreenLayer(0, null, nameScreen, previousAction, dataParams);
                    }
                }
                else
                {
                    CreateNewScreenLayer(0, null, nameScreen, previousAction, pages);
                }                
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN)
            {
                int layer = (int)_list[0];
                object animation = (object)_list[1];
                string nameScreen = (string)_list[2];
                UIScreenTypePreviousAction previousAction = (UIScreenTypePreviousAction)_list[3];
                bool hidePreviousScreens = (bool)_list[4];
                List<PageInformation> pages = null;
                if (_list.Length > 5)
                {
                    if (_list[5] is List<PageInformation>)
                    {
                        pages = (List<PageInformation>)_list[5];
                        CreateNewScreenLayer(layer, animation, nameScreen, previousAction, pages);
                    }
                    else
                    {
                        object[] dataParams = new object[_list.Length - 5];
                        for (int k = 5; k < _list.Length; k++)
                        {
                            dataParams[k - 5] = _list[k];
                        }
                        CreateNewScreenLayer(layer, animation, nameScreen, previousAction, dataParams);
                    }
                }
                else
                {
                    CreateNewScreenLayer(layer, animation, nameScreen, previousAction, pages);
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
                CreateNewScreenLayer(TOTAL_LAYERS_SCREENS - 1, null, nameScreen, previousAction, pages);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_INFORMATION_SCREEN)
            {
                int layer = (int)_list[0];
                object animation = (object)_list[1];
                string nameScreen = (string)_list[2];
                UIScreenTypePreviousAction previousAction = (UIScreenTypePreviousAction)_list[3];
                string title = (string)_list[4];
                string description = (string)_list[5];
                Sprite image = (Sprite)_list[6];
                string eventData = (string)_list[7];
                List<PageInformation> pages = new List<PageInformation>();
                pages.Add(new PageInformation(title, description, image, eventData));
                CreateNewScreenLayer(((layer == -1)?(TOTAL_LAYERS_SCREENS - 1):layer), animation, nameScreen, previousAction, pages);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN)
            {
                m_enableScreens = true;
                GameObject screen = (GameObject)_list[0];
                DestroyGameObjectSingleScreen(screen, true);
                EnableScreens(0, true);
                if (DebugMode)
                {
                    Utilities.DebugLogError("EVENT_SCREENMANAGER_DESTROY_SCREEN::POOL[" + ScreensEnabled + "]-----------------------------------------------------------");
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN)
            {
                DestroyScreensPool();
            }
            if (_nameEvent == EVENT_CONFIRMATION_POPUP)
            {
                GameObject screen = (GameObject)_list[0];
                bool accepted = (bool)_list[1];
                string subnameEvent = (string)_list[2];
                if (screen != null) Debug.Log("POP UP[" + screen.name + "] CLOSED");
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_CREATE_FADE_SCREEN)
            {
                if (FadeScreen != null)
                {
                    GameObject parentScreen = (GameObject)_list[0];
                    float startAlpha = (float)_list[1];
                    float endAlpha = (float)_list[2];
                    Color finalColor = (Color)_list[3];
                    float timeColor = (float)_list[4];
                    GameObject fadeScreen = Utilities.AddChild(parentScreen.transform, FadeScreen);
                    fadeScreen.GetComponent<Image>().color = finalColor;
                    fadeScreen.GetComponent<CanvasGroup>().alpha = startAlpha;
                    AlphaController.Instance.Interpolate(fadeScreen, startAlpha, endAlpha, timeColor, true);
                    GameObject.Destroy(fadeScreen, timeColor + 0.1f);
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