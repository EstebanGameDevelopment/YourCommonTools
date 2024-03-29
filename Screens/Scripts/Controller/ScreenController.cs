using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

        public const string EVENT_SCREENCONTROLLER_REPLACE_LOGO = "EVENT_SCREENCONTROLLER_REPLACE_LOGO";

        public const string EVENT_SCREENCONTROLLER_ENABLE_PROCESS_EVENTS = "EVENT_SCREENCONTROLLER_ENABLE_PROCESS_EVENTS";
        public const string EVENT_SCREENCONTROLLER_DISABLE_ALL_PROCESS_EVENTS = "EVENT_SCREENCONTROLLER_DISABLE_ALL_PROCESS_EVENTS";

        public const string EVENT_SCREENCONTROLLER_REMOVE_SCREEN_NAME = "EVENT_SCREENCONTROLLER_REMOVE_SCREEN_NAME";
        public const string EVENT_SCREENCONTROLLER_CLEAR_STACK_NAMES = "EVENT_SCREENCONTROLLER_CLEAR_STACK_NAMES";
        public const string EVENT_SCREENCONTROLLER_ADD_NAME_TO_STACKNAMES = "EVENT_SCREENCONTROLLER_ADD_NAME_TO_STACKNAMES";

        public const int TOTAL_LAYERS_SCREENS = 10;

        public const int ANIMATION_MOVEMENT = 0;
        public const int DIRECTION_UP       = 0;
        public const int DIRECTION_DOWN     = 1;
        public const int DIRECTION_LEFT     = 2;
        public const int DIRECTION_RIGHT    = 3;

        public const int ANIMATION_ALPHA    = 1;

        public const int ANIMATION_FADE     = 2;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static ScreenController _instanceBase;

        public static ScreenController InstanceBase
        {
            get
            {
                if (!_instanceBase)
                {
                    _instanceBase = GameObject.FindObjectOfType(typeof(ScreenController)) as ScreenController;
                }
                return _instanceBase;
            }
        }

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

        [Tooltip("Time used for default animation transitions")]
        public float PushAnimationTime = 0.3f;

        [Tooltip("Limit the total number of stacked screens")]
        public int TotalStackedScreensAllowed = 5;

        [Tooltip("Back up system for navigation")]
        public bool ActivateNameStack = false;
        public float AlphaAnimationNameStack = -1;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        protected Dictionary<int, List<GameObject>> m_screensPool = new Dictionary<int, List<GameObject>>();
        protected bool m_enableScreens = true;
        protected bool m_enableDebugTestingCode = false;
        protected bool m_hasBeenInitialized = false;
        protected GameObject m_eventSystem;

        protected List<GameObject> m_layers = new List<GameObject>();

        protected bool m_enableProcessEvents = true;

        protected List<ItemMultiObjectEntry> m_stackScreenNames = new List<ItemMultiObjectEntry>();

        protected int m_lastLayerUsed = -1;

        protected bool m_loadingRequestedConnection = false;

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
        public bool EnableProcessEvents
        {
            set { m_enableProcessEvents = value; }
        }
        public bool LoadingRequestedConnection
        {
            get { return m_loadingRequestedConnection; }
            set { m_loadingRequestedConnection = value; }
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

            if (GameObject.FindObjectOfType<EventSystem>() != null)
            {
                m_eventSystem = GameObject.FindObjectOfType<EventSystem>().gameObject;
            }
        }

        // -------------------------------------------
        /* 
		 * Destroy all references
		 */
        public virtual void Destroy()
        {
            DestroyScreensPool();
            _instanceBase = null;
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

            CreateNewScreenLayer(TOTAL_LAYERS_SCREENS - 1, null, _nameScreen, _previousAction, pages);
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
            CreateNewScreenLayer(1, null, _nameScreen, _previousAction, _pages);
        }

        // -------------------------------------------
        /* 
		* Create a new screen
		*/
        public virtual void CreateNewScreen(string _nameScreen, UIScreenTypePreviousAction _previousAction, bool _ignore, params object[] _list)
        {
            CreateNewScreenLayer(-1, null, _nameScreen, _previousAction, _list);
        }

        // -------------------------------------------
        /* 
		* Create a new screen
		*/
        public virtual void CreateNewScreenLayer(int _layer, object _animation, string _nameScreen, UIScreenTypePreviousAction _previousAction, params object[] _list)
        {
            if (!m_enableScreens) return;

            if (ScreensEnabled > TotalStackedScreensAllowed)
            {
                List<PageInformation> pages = new List<PageInformation>();
                pages.Add(new PageInformation(LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("total.maximum.screen.reached"), null, "",  "", ""));

                CreateRealNewScreenLayer(TOTAL_LAYERS_SCREENS - 1, null, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, pages);
            }
            else
            {
                CreateRealNewScreenLayer(_layer, _animation, _nameScreen, _previousAction, _list);
            }
        }

        // -------------------------------------------
        /* 
		* CreateRealNewScreenLayer
		*/
        protected void CreateRealNewScreenLayer(int _layer, object _animation, string _nameScreen, UIScreenTypePreviousAction _previousAction, params object[] _list)
        {
            if (!m_enableScreens) return;

            int finalLayer = _layer;
            if (finalLayer < 0) finalLayer = 0;

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

            if (m_lastLayerUsed == -1)
            {
                m_lastLayerUsed = finalLayer;
            }

            // PREVIOUS ACTION
            switch (_previousAction)
            {
                case UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN:
                    // Debug.LogError("HIDE_CURRENT_SCREEN::m_lastLayerUsed[" + m_lastLayerUsed + "] WITH TOTAL SCREENS["+ m_screensPool[m_lastLayerUsed].Count + "]");
                    if (m_screensPool[m_lastLayerUsed].Count > 0)
                    {
                        for (int k = 0; k < m_screensPool[m_lastLayerUsed].Count; k++)
                        {
                            m_screensPool[m_lastLayerUsed][k].GetComponent<IBasicView>().SetActivation(false);
                        }
                    }
                    break;

                case UIScreenTypePreviousAction.HIDE_ALL_SCREENS:
                    foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
                    {
                        for (int i = 0; i < screenPool.Value.Count; i++)
                        {
                            screenPool.Value[i].GetComponent<IBasicView>().SetActivation(false);
                        }
                    }
                    break;

                case UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN:
                    break;

                case UIScreenTypePreviousAction.DESTROY_CURRENT_SCREEN:
                    if (m_screensPool[m_lastLayerUsed].Count > 0)
                    {
                        DestroyGameObjectSingleScreen(m_screensPool[finalLayer][m_screensPool[finalLayer].Count - 1], true);
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
                        currentScreen = (GameObject)Utilities.AddChild(m_layers[finalLayer].transform, ScreensPrefabs[i]);
                        if (_layer >= 0)
                        {
                            currentScreen.GetComponent<Canvas>().sortingOrder = _layer;
                        }
                        currentScreen.GetComponent<IBasicView>().Initialize(_list);
                        currentScreen.GetComponent<IBasicView>().NameOfScreen = _nameScreen;
                        if ((currentScreen.GetComponent<ScreenInformationView>() == null) && (currentScreen.GetComponent<ScreenLoadingView>() == null))
                        {
                            AddScreenNameToStack(_nameScreen, _previousAction, _layer);
                        }                        
                        break;
                    }
                }
            }

            if (currentScreen != null)
            {
                if (currentScreen.GetComponent<ScreenInformationView>() == null)
                {
                    m_lastLayerUsed = finalLayer;
                }

                m_screensPool[finalLayer].Add(currentScreen);
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
            else
            {
                if (this != null)
                {
                    if (this.gameObject != null)
                    {
                        Debug.LogError("ScreenController[" + this.gameObject.name + "]::SCREEN NAME[" + _nameScreen + "] DOESN'T EXIST FOR THIS MANAGER");
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
         * AreThereBlockingLayersOverMe
         */
        public bool AreThereBlockingLayersOverMe(int _layer)
        {
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                if (screenPool.Value.Count > 0)
                {
                    if (screenPool.Key > _layer)
                    {
                        bool isBlockingInteraction = false;
                        for (int i = 0; i< screenPool.Value.Count; i++)
                        {
                            if (screenPool.Value[i].GetComponent<ScreenBaseView>() != null)
                            {
                                isBlockingInteraction = isBlockingInteraction || screenPool.Value[i].GetComponent<ScreenBaseView>().BlocksInteraction;
                            }
                        }
                        return isBlockingInteraction;
                    }
                }
            }

            return false;
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
         * EnableLayersBelowMe
         */
        public bool EnableLayersBelowMe(int _layer, bool _enable)
        {
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                if (screenPool.Value.Count > 0)
                {
                    if (screenPool.Key < _layer)
                    {
                        for (int i = 0; i < screenPool.Value.Count; i++)
                        {
                            screenPool.Value[i].SetActive(_enable);
                        }
                    }
                }
            }

            return false;
        }

        // -------------------------------------------
        /* 
         * EnableOneLayerBelowMe
         */
        public bool EnableOneLayerBelowMe(int _layer, bool _enable)
        {
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                if (screenPool.Value.Count > 0)
                {
                    if (screenPool.Key == _layer - 1)
                    {
                        for (int i = 0; i < screenPool.Value.Count; i++)
                        {
                            screenPool.Value[i].SetActive(_enable);
                        }
                    }
                }
            }

            return false;
        }
        

        // -------------------------------------------
        /* 
         * MoveScreenToLayer
         */
        public void MoveScreenToLayer(GameObject _screen, int _layer)
        {
            if (m_screensPool[_screen.GetComponent<IBasicView>().Layer].Remove(_screen))
            {
                m_screensPool[_layer].Add(_screen);
                _screen.GetComponent<IBasicView>().Layer = _layer;
                _screen.GetComponent<Canvas>().sortingOrder = _layer;
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
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                List<GameObject> listNotToDestroy = new List<GameObject>();
                int iteratorPool = 0;
                while ((screenPool.Value.Count > 0) && (iteratorPool < 50))
                {
                    GameObject screen = screenPool.Value[0];
                    iteratorPool++;
                    if (screen != null)
                    {
                        if (screen.GetComponent<IBasicView>() != null)
                        {
                            if (screen.GetComponent<IBasicView>().MustBeDestroyed)
                            {
                                screen.GetComponent<IBasicView>().Destroy();
                            }
                            else
                            {
                                if (!listNotToDestroy.Contains(screen)) listNotToDestroy.Add(screen);
                            }                            
                        }
                        if (screen != null)
                        {
                            GameObject.Destroy(screen);
                            screen = null;
                        }
                    }
                }
                screenPool.Value.Clear();
                if (listNotToDestroy.Count > 0)
                {
                    for (int i = 0; i < listNotToDestroy.Count; i++)
                    {
                        screenPool.Value.Add(listNotToDestroy[i]);
                    }
                    listNotToDestroy.Clear();
                }
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

            // SET THE CURRENT LAYER TO 0
            List<GameObject> screenToBottom = new List<GameObject>();
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                if (screenPool.Key == _layer)
                {
                    for (int k = 0; k < screenPool.Value.Count; k++)
                    {
                        GameObject screen = screenPool.Value[k];
                        if (screen != null)
                        {
                            if (screen.GetComponent<IBasicView>() != null)
                            {
                                screen.GetComponent<IBasicView>().Layer = 0;
                                Utilities.AttachChild(m_layers[0].transform, screen);
                                screen.GetComponent<Canvas>().sortingOrder = 0;
                                screenToBottom.Add(screen);
                                screenPool.Value.Remove(screen);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < screenToBottom.Count; i++)
            {
                if (!m_screensPool[0].Contains(screenToBottom[i]))
                {
                    m_screensPool[0].Add(screenToBottom[i]);
                }                
            }

            if (DebugMode)
            {
                Utilities.DebugLogError("ScreenController::DestroyScreensBelowLayerPool::POOL[" + ScreensEnabled + "]-----------------------------------------------------------");
            }
        }

        // -------------------------------------------
        /* 
		 * Destroy all the screens above a specific layer
		 */
        public void DestroyScreensFromLayerPool(bool _above = true, int _layer = 0, GameObject[] _excludeScreens = null)
        {
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                bool considerScreen = true;
                if (_above)
                {
                    considerScreen = (screenPool.Key > _layer);
                }
                else
                {
                    considerScreen = (screenPool.Key < _layer);
                }

                if (considerScreen)
                {
                    bool ignoreDestruction = false;
                    while ((screenPool.Value.Count > 0) && (!ignoreDestruction))
                    {
                        GameObject screen = screenPool.Value[0];
                        if (screen != null)
                        {
                            if (_excludeScreens!=null)
                            {
                                for (int k = 0; k < _excludeScreens.Length; k++)
                                {
                                    if (screen == _excludeScreens[k])
                                    {
                                        ignoreDestruction = true;
                                        break;
                                    }
                                }
                            }

                            if (!ignoreDestruction)
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
                    }
                    if (!ignoreDestruction) screenPool.Value.Clear();
                }
            }

            if (DebugMode)
            {
                Utilities.DebugLogError("ScreenController::DestroyScreensAboveLayerPool::POOL[" + ScreensEnabled + "]-----------------------------------------------------------");
            }
        }

        // -------------------------------------------
        /* 
		 * Remove the screen from the list of screens
		 */
        private void DestroyGameObjectSingleScreen(GameObject _screen, bool _runDestroy)
		{
			if (_screen == null) return;

            string nameScreen = _screen.name;
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
                        try
                        {
                            if (i < m_screensPool.Count) screenPool.Value.RemoveAt(i);
                        }
                        catch (Exception err)
                        {
                            Debug.LogError("DestroyGameObjectSingleScreen::ERROR TO DESTROY::nameScreen[" + nameScreen + "]");
                        }                        
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
		 * Changes the visibility of all the layers
		 */
        protected void VisibilityLayers(bool _visibility)
        {
            for (int i = 0; i < m_layers.Count; i++)
            {
                m_layers[i].SetActive(_visibility);
            }
        }

        // -------------------------------------------
        /* 
		 * Changes the enable of the screens
		 */
        private void EnableScreens(int _layer, bool _activation)
		{
            if (m_screensPool.ContainsKey(_layer))
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
        }

        // -------------------------------------------
        /* 
		 * HideCurrentScreen
		 */
        private void HideCurrentScreen(int _layer = -1, GameObject _screen = null)
        {
            int finalLayer = _layer;
            if (finalLayer < 0) finalLayer = 0;

            if (m_screensPool[finalLayer].Count > 0)
            {
                for (int k = 0; k < m_screensPool[finalLayer].Count; k++)
                {
                    if (_screen == null)
                    {
                        m_screensPool[finalLayer][k].SetActive(false);
                    }
                    else
                    {
                        if (m_screensPool[finalLayer][k].gameObject == _screen)
                        {
                            m_screensPool[finalLayer][k].SetActive(false);
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * ShowCurrentScreen
		 */
        private void ShowCurrentScreen(int _layer = -1, GameObject _screen = null)
        {
            int finalLayer = _layer;
            if (finalLayer < 0) finalLayer = 0;

            if (m_screensPool[finalLayer].Count > 0)
            {
                for (int k = 0; k < m_screensPool[finalLayer].Count; k++)
                {
                    if (_screen == null)
                    {
                        m_screensPool[finalLayer][k].SetActive(true);
                    }
                    else
                    {
                        if (m_screensPool[finalLayer][k].gameObject == _screen)
                        {
                            m_screensPool[finalLayer][k].SetActive(true);
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * AddScreenNameToStack
		 */
        public void AddScreenNameToStack(string _nameScreen, UIScreenTypePreviousAction _previousAction, int _layerScreen)
        {
            bool addScreenName = true;
            if (m_stackScreenNames.Count > 0)
            {
                if ((string)m_stackScreenNames[m_stackScreenNames.Count - 1].Objects[0] == _nameScreen)
                {
                    addScreenName = false;
                }
            }

            if (addScreenName)
            {
                m_stackScreenNames.Add(new ItemMultiObjectEntry(_nameScreen, _layerScreen));
                DebugLogStackNameScreens(_nameScreen);
            }
        }

        // -------------------------------------------
        /* 
		 * PopScreenNameFromStack
		 */
        public string PopScreenNameFromStack(bool _remove = true)
        {
            if (m_stackScreenNames.Count == 0)
            {
                return null;
            }
            else
            {
                string output = (string)m_stackScreenNames[m_stackScreenNames.Count - 1].Objects[0];
                m_lastLayerUsed = (int)m_stackScreenNames[m_stackScreenNames.Count - 1].Objects[1];
                if (_remove) m_stackScreenNames.RemoveAt(m_stackScreenNames.Count - 1);
                DebugLogStackNameScreens("");
                return output;
            }
        }

        // -------------------------------------------
        /* 
		 * RemoveScreensUntilScreenName
		 */
        public int RemoveScreensUntilScreenName(string _screenName)
        {
            string output = "";
            int counterDeleted = 0;
            do
            {
                output = (string)m_stackScreenNames[m_stackScreenNames.Count - 1].Objects[0];
                m_lastLayerUsed = (int)m_stackScreenNames[m_stackScreenNames.Count - 1].Objects[1];
                if (output != _screenName)
                {
                    m_stackScreenNames.RemoveAt(m_stackScreenNames.Count - 1);
                }
                else
                {
                    counterDeleted++;
                }
            } while (output != _screenName);
            return counterDeleted;
        }

        // -------------------------------------------
        /* 
		 * LookScreenInPool
		 */
        public GameObject LookScreenInPool(string _screenName)
        {
            foreach (KeyValuePair<int, List<GameObject>> screenPool in m_screensPool)
            {
                if (screenPool.Value.Count > 0)
                {
                    for (int i = 0; i < screenPool.Value.Count; i++)
                    {
                        if (screenPool.Value[i].GetComponent<ScreenBaseView>() != null)
                        {
                            if (screenPool.Value[i].GetComponent<ScreenBaseView>().NameOfScreen == _screenName)
                            {
                                return screenPool.Value[i];
                            }
                        }
                    }
                }
            }
            return null;
        }

        // -------------------------------------------
        /* 
		 * RemoveScreenName
		 */
        public bool RemoveScreenName(string _screenName)
        {
            foreach (ItemMultiObjectEntry item in m_stackScreenNames)
            {
                if (((string)item.Objects[0]) == _screenName)
                {
                    return m_stackScreenNames.Remove(item);
                }                
            }
            return false;
        }

        // -------------------------------------------
        /* 
		 * AddScreenNameToStack
		 */
        public void ClearScreenNameStack()
        {
            m_stackScreenNames.Clear();
            Debug.LogError("ScreenController::CLEARED ALL THE STACK OF SCREEN NAMES");
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
		 * PreProcessScreenEvents
		 */
        protected virtual bool PreProcessScreenEvents(string _nameEvent, params object[] _list)
        {
            if (_nameEvent == EVENT_SCREENCONTROLLER_ENABLE_PROCESS_EVENTS)
            {
                if (this.gameObject == (GameObject)_list[0])
                {
                    m_enableProcessEvents = (bool)_list[1];
                }
            }
            if (_nameEvent == EVENT_SCREENCONTROLLER_DISABLE_ALL_PROCESS_EVENTS)
            {
                m_enableProcessEvents = false;
            }

            return m_enableProcessEvents;
        }

        // -------------------------------------------
        /* 
        * ProcessScreenEvents
        */
        protected void ProcessScreenEvents(string _nameEvent, params object[] _list)
        {
            if (!PreProcessScreenEvents(_nameEvent, _list)) return;

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
                        CreateNewScreenLayer(-1, null, nameScreen, previousAction, pages);
                    }
                    else
                    {
                        object[] dataParams = new object[_list.Length - 3];
                        for (int k = 3; k < _list.Length; k++)
                        {
                            dataParams[k - 3] = _list[k];
                        }
                        CreateNewScreenLayer(-1, null, nameScreen, previousAction, dataParams);
                    }
                }
                else
                {
                    CreateNewScreenLayer(-1, null, nameScreen, previousAction, pages);
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
                if (animation != null) BasicSystemEventController.Instance.DispatchBasicSystemEvent(ScreenInformationView.EVENT_SCREEN_FADE_BACKGROUND, 0f, 1f, 0.4f);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN)
            {
                m_enableScreens = true;
                GameObject screen = (GameObject)_list[0];
                DestroyGameObjectSingleScreen(screen, true);
                if (screen.GetComponent<ScreenBaseView>() != null)
                {
                    if (((screen.GetComponent<ScreenBaseView>().Layer == 0) || (screen.GetComponent<ScreenBaseView>().Layer == -1))
                        && (screen.GetComponent<ScreenInformationView>() == null))
                    {
                        EnableScreens(0, true);
                    }
                }
                if (DebugMode)
                {
                    Utilities.DebugLogError("EVENT_SCREENMANAGER_DESTROY_SCREEN::POOL[" + ScreensEnabled + "]-----------------------------------------------------------");
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_HIDE_CURRENT_SCREEN)
            {
                int layer = (int)_list[0];
                GameObject screenToHide = (GameObject)_list[1];
                HideCurrentScreen(layer, screenToHide);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_SHOW_CURRENT_SCREEN)
            {
                int layer = (int)_list[0];
                GameObject screenToShow = (GameObject)_list[1];
                ShowCurrentScreen(layer, screenToShow);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN)
            {
                DestroyScreensPool();
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ENABLE_LAYERS_BELOW_ME)
            {
                int layer = (int)_list[0];
                bool activation = (bool)_list[1];
                EnableLayersBelowMe(layer, activation);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ENABLE_ONE_LAYER_BELOW_ME)
            {
                int layer = (int)_list[0];
                bool activation = (bool)_list[1];
                EnableOneLayerBelowMe(layer, activation);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREENS_LAYERS_ABOVE)
            {
                int layer = (int)_list[0];
                GameObject[] excludeScreens = null;
                if (_list.Length > 1)
                {
                    excludeScreens = new GameObject[_list.Length - 1];
                    for (int k = 1; k < _list.Length; k++)
                    {
                        excludeScreens[k - 1] = (GameObject)_list[k];
                    }
                }
                DestroyScreensFromLayerPool(true, layer, excludeScreens);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREENS_LAYERS_BELOW)
            {
                int layer = (int)_list[0];
                GameObject[] excludeScreens = null;
                if (_list.Length > 1)
                {
                    excludeScreens = new GameObject[_list.Length - 1];
                    for (int k = 1; k < _list.Length; k++)
                    {
                        excludeScreens[k - 1] = (GameObject)_list[k];
                    }
                }
                DestroyScreensFromLayerPool(false, layer, excludeScreens);
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_MOVE_SCREEN_TO_LAYER)
            {
                MoveScreenToLayer((GameObject)_list[0], (int)_list[1]);
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
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANIMATION_SCREEN)
            {
                if (m_eventSystem != null)
                {
                    m_eventSystem.SetActive((bool)_list[1]);
                }
            }
            if (_nameEvent == EVENT_SCREENCONTROLLER_REMOVE_SCREEN_NAME)
            {
                RemoveScreenName((string)_list[0]);
            }
            if (_nameEvent == ScreenBaseView.EVENT_SCREENBASE_REQUEST_SCREENVIEW_IN_POOL)
            {
                bool isVRScreen = (bool)_list[0];
                if (!isVRScreen)
                {
                    GameObject originGO = (GameObject)_list[1];
                    string previousScreenName = (string)_list[2];
                    GameObject screenView = LookScreenInPool(previousScreenName);
                    UIEventController.Instance.DispatchUIEvent(ScreenBaseView.EVENT_SCREENBASE_RESPONSE_SCREENVIEW_IN_POOL, isVRScreen, originGO, screenView, previousScreenName);
                }
            }
            if (_nameEvent == EVENT_SCREENCONTROLLER_CLEAR_STACK_NAMES)
            {
                ClearScreenNameStack();
            }
            if (_nameEvent == EVENT_SCREENCONTROLLER_ADD_NAME_TO_STACKNAMES)
            {
                string nameScreen = (string)_list[0];
                int layerScreen = (int)_list[1];
                m_stackScreenNames.Add(new ItemMultiObjectEntry(nameScreen, layerScreen));
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        public virtual void Update()
		{
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON);
			}
#endif

#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.Escape))
            {
#if !ENABLE_WORLDSENSE && !ENABLE_OCULUS && !ENABLE_HTCVIVE && !ENABLE_PICONEO
                SceneManager.LoadSceneAsync("EmptyScene");
#endif
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * DebugLogStackNameScreens
		 */
        public void DebugLogStackNameScreens(string _newScreen)
        {
#if UNITY_EDITOR
            string stackedNames = "";
            for (int i = 0; i < m_stackScreenNames.Count; i++)
            {
                if (i == 0)
                {
                    stackedNames = (string)m_stackScreenNames[i].Objects[0];
                }
                else
                {
                    stackedNames += "," + (string)m_stackScreenNames[i].Objects[0];
                }                
            }
            if (_newScreen.Length > 0)
            {
                Debug.LogError("TOTAL STACK SCREENS[" + m_stackScreenNames.Count + "]::new screen[" + _newScreen + "]::STACKED NAMES[" + stackedNames + "]");
            }
            else
            {
                Debug.LogError("TOTAL STACK SCREENS[" + m_stackScreenNames.Count + "]::STACKED NAMES[" + stackedNames + "]");
            }
#endif
        }
    }
}