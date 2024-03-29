﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * ScreenBaseView
	 * 
	 * Base class that will allow special management of the activation of the screen
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBaseView : MonoBehaviour
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SCREENBASE_OPENED = "EVENT_SCREENBASE_OPENED";
        public const string EVENT_SCREENBASE_ANIMATION_SHOW = "EVENT_SCREENBASE_ANIMATION_SHOW";
        public const string EVENT_SCREENBASE_ANIMATION_HIDE = "EVENT_SCREENBASE_ANIMATION_HIDE";
        public const string EVENT_SCREENBASE_FORCE_HIDE     = "EVENT_SCREENBASE_FORCE_HIDE";

        public const string EVENT_SCREENBASE_ANIMATION_SLIDE_APPLY      = "EVENT_SCREENBASE_ANIMATION_SLIDE_APPLY";
        public const string EVENT_SCREENBASE_ANIMATION_SLIDE_RECOVER    = "EVENT_SCREENBASE_ANIMATION_SLIDE_RECOVER";
        public const string EVENT_SCREENBASE_ANIMATION_SLIDE_RESET      = "EVENT_SCREENBASE_ANIMATION_SLIDE_RESET";
        public const string EVENT_SCREENBASE_REQUEST_SCREENVIEW_IN_POOL = "EVENT_SCREENBASE_REQUEST_SCREENVIEW_IN_POOL";
        public const string EVENT_SCREENBASE_RESPONSE_SCREENVIEW_IN_POOL = "EVENT_SCREENBASE_RESPONSE_SCREENVIEW_IN_POOL";

        public const string EVENT_SCREENBASE_BLOCK_INTERACTION = "EVENT_SCREENBASE_BLOCK_INTERACTION";

        public const string EVENT_SCREENBASE_CLEAR_ANIMATION_PARAMS = "EVENT_SCREENBASE_CLEAR_ANIMATION_PARAMS";

        public const string EVENT_SCREENBASE_GO_BACK_EVENT = "EVENT_SCREENBASE_GO_BACK_EVENT";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const string CONTENT_COMPONENT_NAME = "Content";
        public const string CONTENT_BACKGROUND_NAME = "Background";

        // ----------------------------------------------
        // PUBLIC VARIABLE MEMBERS
        // ----------------------------------------------	
        public Sprite SelectorGraphic;

		// ----------------------------------------------
		// PRIVATE VARIABLE MEMBERS
		// ----------------------------------------------	
		protected string m_nameOfScreen;
        protected int m_layer = -1;
        private GameObject m_screen;
        protected Transform m_containerBase;
        protected Transform m_backgroundBase;
        protected CanvasGroup m_canvasGroup;
        protected Vector3 m_initialPosition;
		protected bool m_hasFocus = true;

		private int m_selectionButton;
		private List<GameObject> m_selectors;
        protected bool m_allowAddingSelectors = true;

        private bool m_enabledSelector = true;

		protected bool m_hasBeenDestroyed = false;
        protected List<object> m_paramsAnimation;

        protected bool m_isMarkedToBeDestroyed = false;

        protected bool m_blocksInteraction = true;

        protected List<List<object>> m_paramsSlide = new List<List<object>>();

        protected Transform m_btnGoBack;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public string NameOfScreen
		{
			get { return m_nameOfScreen; }
			set { m_nameOfScreen = value; }
		}

		public GameObject Screen
		{
			get { return m_screen; }
		}
        public CanvasGroup CanvasGroup
		{
			get { return m_canvasGroup; }
		}
		public bool HasFocus
		{
			get { return m_hasFocus; }
			set { m_hasFocus = value; }
		}
        public int Layer
        {
            get { return m_layer; }
            set { m_layer = value; }
        }
        public bool IsMarkedToBeDestroyed
        {
            get { return m_isMarkedToBeDestroyed; }
            set { m_isMarkedToBeDestroyed = value;  }
        }
        public bool BlocksInteraction
        {
            get { return m_blocksInteraction; }
            set { m_blocksInteraction = value; }
        }
        public virtual bool MustBeDestroyed
        {
            get { return true; }
        }

        // -------------------------------------------
        /* 
		 * Initialitzation
		 */
        public virtual void Initialize(params object[] _list)
		{
			m_selectionButton = 0;
			m_selectors = new List<GameObject>();
			m_screen = this.gameObject;
			if (m_screen.transform.Find(CONTENT_COMPONENT_NAME) != null)
			{
                m_containerBase = m_screen.transform.Find(CONTENT_COMPONENT_NAME);
                m_canvasGroup = m_screen.transform.Find(CONTENT_COMPONENT_NAME).GetComponent<CanvasGroup>();
                if (m_canvasGroup != null)
				{
                    m_canvasGroup.alpha = 1;
                    m_initialPosition = Utilities.Clone(m_canvasGroup.transform.position);
                }
			}
            if (m_screen.transform.Find(CONTENT_BACKGROUND_NAME) != null)
            {
                m_backgroundBase = m_screen.transform.Find(CONTENT_BACKGROUND_NAME);
            }

            UIEventController.Instance.DispatchUIEvent(EVENT_SCREENBASE_OPENED, this.gameObject);

            if (m_containerBase != null)
            {
                m_btnGoBack = m_containerBase.transform.Find("Button_GoBack");
                if (m_btnGoBack != null)
                {
                    m_btnGoBack.GetComponent<Button>().onClick.AddListener(GoBackPressed);
                }
            }

            // AddAutomaticallyButtons(m_screen);
        }

        // -------------------------------------------
        /* 
		 * This functions needs to be overridden in certain classes in order 
		 * to discard/listen events or reload data
		 */
        public virtual void SetActivation(bool _activation)
		{
            m_hasFocus = _activation;
			this.gameObject.SetActive(_activation);
		}

        // -------------------------------------------
        /* 
		* ApplyCentered
		*/
        public virtual void ApplyCentered()
        {
            if (m_containerBase != null)
            {
                if (m_containerBase.GetComponent<RectTransform>() != null)
                {
                    if (m_containerBase.parent != null)
                    {
                        if (m_containerBase.parent.GetComponent<RectTransform>() != null)
                        {
                            m_containerBase.GetComponent<RectTransform>().SetCenteredToParentSize(m_containerBase.parent.GetComponent<RectTransform>());
                        }                        
                    }                    
                }
            }
            if (m_backgroundBase != null)
            {
                if (m_backgroundBase.GetComponent<RectTransform>() != null)
                {
                    if (m_backgroundBase.parent != null)
                    {
                        if (m_backgroundBase.parent.GetComponent<RectTransform>() != null)
                        {
                            m_backgroundBase.GetComponent<RectTransform>().SetCenteredToParentSize(m_backgroundBase.parent.GetComponent<RectTransform>());
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		* ApplyCentered
		*/
        protected void ApplyStretched()
        {
            if (m_containerBase != null)
            {
                if (m_containerBase.GetComponent<RectTransform>() != null)
                {
                    if (m_containerBase.parent != null)
                    {
                        if (m_containerBase.parent.GetComponent<RectTransform>() != null)
                        {
                            m_containerBase.GetComponent<RectTransform>().SetStretchedToParent(m_containerBase.parent.GetComponent<RectTransform>());
                        }
                    }
                }
            }
            if (m_backgroundBase != null)
            {
                if (m_backgroundBase.GetComponent<RectTransform>() != null)
                {
                    if (m_backgroundBase.parent != null)
                    {
                        if (m_backgroundBase.parent.GetComponent<RectTransform>() != null)
                        {
                            m_backgroundBase.GetComponent<RectTransform>().SetStretchedToParent(m_backgroundBase.parent.GetComponent<RectTransform>());
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		* Called on the destroy method of the object
		*/
        void OnDestroy()
		{
			Debug.Log("ScreenBaseView::OnDestroy::NAME OBJECT DESTROYED[" + this.gameObject.name + "]");

			ClearListSelectors();
			m_selectors = null;
            m_btnGoBack = null;
        }		

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public virtual bool Destroy()
		{
			if (m_hasBeenDestroyed) return true;
			m_hasBeenDestroyed = true;

			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_REPORT_DESTROYED, this.gameObject);			

			return false;
		}

		// -------------------------------------------
		/* 
		* It will go recursively through all the childs 
		* looking for interactable elements to add 
		* the beahavior of YourVRUI
		*/
		private void AddAutomaticallyButtons(GameObject _go)
		{
            if (!m_allowAddingSelectors) return;

			if (_go != null)
			{
                try
                {
                    if (_go.GetComponent<Button>() != null)
                    {
                        AddButtonToList(_go);
                    }
                }
                catch (Exception err) { };
                foreach (Transform child in _go.transform)
                {
                    AddAutomaticallyButtons(child.gameObject);
                }
            }
		}

		// -------------------------------------------
		/* 
		 * It will add the interactable element to the list
		 */
		private GameObject AddButtonToList(GameObject _button)
		{
            if (!m_allowAddingSelectors) return null;

            m_selectors.Add(_button);
			if (m_enabledSelector)
			{
				if (_button != null)
				{
					_button.AddComponent<SelectableButtonView>();
					_button.GetComponent<SelectableButtonView>().Initialize(SelectorGraphic);
				}
			}

			return _button;
		}

		// -------------------------------------------
		/* 
		 * It will remove and clean the interactable element and all his references
		 */
		protected void ClearListSelectors()
		{
			try
			{
				if (m_selectors != null)
				{
					for (int i = 0; i < m_selectors.Count; i++)
					{
						if (m_selectors[i] != null)
						{
							if (m_selectors[i].GetComponent<SelectableButtonView>() != null)
							{
								m_selectors[i].GetComponent<SelectableButtonView>().Destroy();
							}
						}
					}
					m_selectors.Clear();
				}
			}
			catch (Exception err)
			{
				Debug.LogError(err.StackTrace);
			};
		}

        // -------------------------------------------
        /* 
		 * AppearAnimation
		 */
        protected virtual void AppearAnimation(List<object> _paramsAnimation, bool _isSlideAnimation)
        {
            if (!_isSlideAnimation) m_paramsAnimation = _paramsAnimation;
            float totalTimeAppearAnimation = -1;
            switch ((int)_paramsAnimation[0])
            {
                case ScreenController.ANIMATION_MOVEMENT:
                    Vector3 startingPosition = new Vector3();
                    Vector3 endingPosition = new Vector3();
                    if (!_isSlideAnimation)
                    {
                        endingPosition = new Vector3(0, 0, 0);
                        switch ((int)_paramsAnimation[1])
                        {
                            case ScreenController.DIRECTION_UP:
                                startingPosition = new Vector3(0, UnityEngine.Screen.height, 0);
                                break;
                            case ScreenController.DIRECTION_DOWN:
                                startingPosition = new Vector3(0, -UnityEngine.Screen.height, 0);
                                break;
                            case ScreenController.DIRECTION_LEFT:
                                startingPosition = new Vector3(-UnityEngine.Screen.width, 0, 0);
                                break;
                            case ScreenController.DIRECTION_RIGHT:
                                startingPosition = new Vector3(UnityEngine.Screen.width, 0, 0);
                                break;
                        }
                    }
                    else
                    {
                        startingPosition = Utilities.Clone(m_canvasGroup.gameObject.transform.position);
                        switch ((int)_paramsAnimation[1])
                        {
                            case ScreenController.DIRECTION_UP:
                                endingPosition = startingPosition - new Vector3(0, UnityEngine.Screen.height, 0);
                                break;
                            case ScreenController.DIRECTION_DOWN:
                                endingPosition = startingPosition - new Vector3(0, -UnityEngine.Screen.height, 0);
                                break;
                            case ScreenController.DIRECTION_LEFT:
                                endingPosition = startingPosition - new Vector3(-UnityEngine.Screen.width, 0, 0);
                                break;
                            case ScreenController.DIRECTION_RIGHT:
                                endingPosition = startingPosition - new Vector3(UnityEngine.Screen.width, 0, 0);
                                break;
                        }
                    }
                    float animationTime = (float)_paramsAnimation[2];
                    totalTimeAppearAnimation = animationTime;
                    m_canvasGroup.gameObject.transform.position = startingPosition;
                    InterpolatorController.Instance.Interpolate(m_canvasGroup.gameObject, endingPosition, animationTime, true);
                    break;

                case ScreenController.ANIMATION_ALPHA:
                    float startAlpha = (float)_paramsAnimation[1];
                    float endAlpha = (float)_paramsAnimation[2];
                    float alphaTime = (float)_paramsAnimation[3];
                    totalTimeAppearAnimation = alphaTime;
                    m_canvasGroup.alpha = startAlpha;
                    AlphaController.Instance.Interpolate(m_canvasGroup.gameObject, startAlpha, endAlpha, alphaTime, true);
                    break;

                case ScreenController.ANIMATION_FADE:
                    Color endColor = (Color)_paramsAnimation[1];
                    float colorTime = (float)_paramsAnimation[2];
                    totalTimeAppearAnimation = colorTime;
                    float startingAlphaFade = 1f;
                    float endingAlphaFade = 0f;
                    if (!_isSlideAnimation)
                    {
                        startingAlphaFade = 1f;
                        endingAlphaFade = 0f;
                    }
                    else
                    {
                        startingAlphaFade = 0f;
                        endingAlphaFade = 1f;
                    }
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_CREATE_FADE_SCREEN, this.gameObject, startingAlphaFade, endingAlphaFade, endColor, colorTime);
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(InterpolateData.EVENT_INTERPOLATE_COMPLETED, colorTime, m_canvasGroup.gameObject);
                    break;
            }
            if (totalTimeAppearAnimation != -1)
            {
                UIEventController.Instance.DelayUIEvent(UIEventController.EVENT_SCREENMANAGER_ANIMATION_SCREEN, 0.01f, this.gameObject, false);
                UIEventController.Instance.DelayUIEvent(UIEventController.EVENT_SCREENMANAGER_ANIMATION_SCREEN, totalTimeAppearAnimation, this.gameObject, true);
            }
        }

        // -------------------------------------------
        /* 
		 * DisappearAnimation
		 */
        protected virtual void DisappearAnimation(List<object> _paramsAnimation, bool _isSlideAnimation, bool _withAnimation = true)
        {
            float totalTimeAppearAnimation = -1;
            List<object> paramsAnimation = m_paramsAnimation;
            if (_paramsAnimation != null)
            {
                paramsAnimation = _paramsAnimation;
            }
            switch ((int)paramsAnimation[0])
            {
                case ScreenController.ANIMATION_MOVEMENT:
                    Vector3 endingPosition = new Vector3();
                    switch ((int)paramsAnimation[1])
                    {
                        case ScreenController.DIRECTION_UP:
                            if (!_isSlideAnimation)
                            {
                                endingPosition = new Vector3(0, UnityEngine.Screen.height, 0);
                            }
                            else
                            {
                                endingPosition = new Vector3(0, UnityEngine.Screen.height, 0) + m_canvasGroup.gameObject.transform.position;
                            }                            
                            break;
                        case ScreenController.DIRECTION_DOWN:
                            if (!_isSlideAnimation)
                            {
                                endingPosition = new Vector3(0, -UnityEngine.Screen.height, 0);
                            }
                            else
                            {
                                endingPosition = new Vector3(0, -UnityEngine.Screen.height, 0) + m_canvasGroup.gameObject.transform.position;
                            }
                            break;
                        case ScreenController.DIRECTION_LEFT:
                            if (!_isSlideAnimation)
                            {
                                endingPosition = new Vector3(-UnityEngine.Screen.width, 0, 0);
                            }
                            else
                            {
                                endingPosition = new Vector3(-UnityEngine.Screen.width, 0, 0) + m_canvasGroup.gameObject.transform.position;
                            }
                            break;
                        case ScreenController.DIRECTION_RIGHT:
                            if (!_isSlideAnimation)
                            {
                                endingPosition = new Vector3(UnityEngine.Screen.width, 0, 0);
                            }
                            else
                            {
                                endingPosition = new Vector3(UnityEngine.Screen.width, 0, 0) + m_canvasGroup.gameObject.transform.position;
                            }
                            break;
                    }
                    float animationTime = (float)paramsAnimation[2];
                    totalTimeAppearAnimation = animationTime;
                    if (!_withAnimation)
                    {
                        animationTime = 0;
                        totalTimeAppearAnimation = 0.05f;
                    }                    
                    InterpolatorController.Instance.Interpolate(m_canvasGroup.gameObject, endingPosition, animationTime, true);
                    break;

                case ScreenController.ANIMATION_ALPHA:
                    float startAlpha = (float)paramsAnimation[1];
                    float endAlpha = (float)paramsAnimation[2];
                    float alphaTime = (float)paramsAnimation[3];
                    totalTimeAppearAnimation = alphaTime;
                    m_canvasGroup.alpha = endAlpha;
                    if (!_withAnimation)
                    {
                        alphaTime = 0;
                        totalTimeAppearAnimation = 0.05f;
                    }
                    AlphaController.Instance.Interpolate(m_canvasGroup.gameObject, endAlpha, startAlpha, alphaTime);
                    break;

                case ScreenController.ANIMATION_FADE:
                    Color endColor = (Color)paramsAnimation[1];
                    float colorTime = (float)paramsAnimation[2];
                    totalTimeAppearAnimation = colorTime;
                    float startingAlphaFade = 1f;
                    float endingAlphaFade = 0f;
                    if (!_isSlideAnimation)
                    {
                        startingAlphaFade = 0f;
                        endingAlphaFade = 1f;
                    }
                    else
                    {
                        startingAlphaFade = 1f;
                        endingAlphaFade = 0f;
                    }
                    if (!_withAnimation)
                    {
                        colorTime = 0;
                        totalTimeAppearAnimation = 0.05f;
                    }
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_CREATE_FADE_SCREEN, this.gameObject, startingAlphaFade, endingAlphaFade, endColor, colorTime);
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(InterpolateData.EVENT_INTERPOLATE_COMPLETED, colorTime, m_canvasGroup.gameObject);
                    break;
            }
            if (totalTimeAppearAnimation != -1)
            {
                UIEventController.Instance.DelayUIEvent(UIEventController.EVENT_SCREENMANAGER_ANIMATION_SCREEN, 0.01f, this.gameObject, false);
                UIEventController.Instance.DelayUIEvent(UIEventController.EVENT_SCREENMANAGER_ANIMATION_SCREEN, totalTimeAppearAnimation, this.gameObject, true);
            }
        }

        // -------------------------------------------
        /* 
		 * Global manager of events
		 */
        protected virtual void OnMenuEvent(string _nameEvent, params object[] _list)
		{
            if (this == null) return;
            if (this.gameObject == null) return;

            if ((_nameEvent == KeysEventInputController.ACTION_KEY_UP_PRESSED) ||
				(_nameEvent == KeysEventInputController.ACTION_KEY_DOWN_PRESSED) ||
				(_nameEvent == KeysEventInputController.ACTION_KEY_LEFT_PRESSED) ||
				(_nameEvent == KeysEventInputController.ACTION_KEY_RIGHT_PRESSED))
			{
				EnableSelector();
			}

            if (_nameEvent == EVENT_SCREENBASE_ANIMATION_SHOW)
            {
                if (this.gameObject == (GameObject)_list[0])
                {
                    AppearAnimation(((List<object>)_list[1]), false);
                }
            }
            if (_nameEvent == EVENT_SCREENBASE_ANIMATION_HIDE)
            {
                if (this.gameObject == (GameObject)_list[0])
                {
                    bool noAnimation = (_list.Length > 2);
                    DisappearAnimation((_list.Length > 1)?((List<object>)_list[1]):null, false, !noAnimation);
                }
            }
            if (_nameEvent == EVENT_SCREENBASE_FORCE_HIDE)
            {
                if (this.gameObject == (GameObject)_list[0])
                {
                    DisappearAnimation((_list.Length > 1) ? ((List<object>)_list[1]) : null, false, false);
                }
            }
            if (_nameEvent == EVENT_SCREENBASE_ANIMATION_SLIDE_APPLY)
            {
                List<object> currentParamsSlide = (List<object>)_list[0];
                bool applySlide = true;
                if (_list.Length > 1)
                {
                    for (int i = 1; i < _list.Length; i++)
                    {
                        if (this.gameObject == (GameObject)_list[i])
                        {
                            applySlide = false;
                        }
                    }
                }
                if (applySlide)
                {
                    m_paramsSlide.Add(currentParamsSlide);
                    AppearAnimation(currentParamsSlide, true);
                }
            }
            if (_nameEvent == EVENT_SCREENBASE_ANIMATION_SLIDE_RECOVER)
            {
                if (this.gameObject != (GameObject)_list[0])
                {
                    if (m_paramsSlide.Count > 0)
                    {
                        List<object> currentParamsSlide = m_paramsSlide[m_paramsSlide.Count - 1];
                        m_paramsSlide.RemoveAt(m_paramsSlide.Count - 1);
                        DisappearAnimation(currentParamsSlide, true);
                    }
                }
            }
            if (_nameEvent == EVENT_SCREENBASE_ANIMATION_SLIDE_RESET)
            {
                if (!((_list != null) && (_list.Length != 0) && (this.gameObject == (GameObject)_list[0])))
                {
                    m_canvasGroup.gameObject.transform.position = Utilities.Clone(m_initialPosition);
                }
            }
            if (_nameEvent == EVENT_SCREENBASE_BLOCK_INTERACTION)
            {
                m_blocksInteraction = (bool)_list[0];
            }
            if (_nameEvent == EVENT_SCREENBASE_CLEAR_ANIMATION_PARAMS)
            {
                m_paramsAnimation = null;
            }

            if (_nameEvent == ScreenController.EVENT_SCREENCONTROLLER_REPLACE_LOGO)
            {
                if (m_containerBase != null)
                {
                    Transform logo = m_containerBase.Find("Logo");
                    if (logo != null)
                    {
                        logo.GetComponent<Image>().sprite = (Sprite)_list[0];
                    }
                }
            }
            if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_DESTROY_NAME_SCREEN)
            {
                Debug.LogError("EVENT_SCREENMANAGER_DESTROY_NAME_SCREEN::(string)_list[0][" + (string)_list[0] + "]!!!!!!!!!!!!!!!!!!!!");
                if (this.gameObject.name.IndexOf((string)_list[0]) != -1)
                {
                    Debug.LogError("    DESTROYED(string)_list[0][" + (string)_list[0] + "]+++++++++++++");
                    Destroy();
                }
            }

            if (_nameEvent == EVENT_SCREENBASE_RESPONSE_SCREENVIEW_IN_POOL)
            {
                bool isVRScreen = (bool)_list[0];
                GameObject originGO = (GameObject)_list[1];
                if (originGO == this.gameObject)
                {
                    GameObject screenView = (GameObject)_list[2];
                    string previousScreenName = (string)_list[3];
                    ScreenController screenController = GameObject.FindObjectOfType<ScreenController>();
                    if (screenView != null)
                    {
                        screenView.GetComponent<IBasicView>().SetActivation(true);
                        Destroy();
                    }
                    else
                    {
                        if (screenController.AlphaAnimationNameStack == -1)
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, previousScreenName, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true, null);
                        }
                        else
                        {
                            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, -1, new List<object> { ScreenController.ANIMATION_ALPHA, 0f, 1f, screenController.AlphaAnimationNameStack }, previousScreenName, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true, null);
                        }
                    }
                }
            }

            if (!this.gameObject.activeSelf) return;
			if (m_selectors == null) return;
			if (m_selectors.Count == 0) return;
			if (!m_hasFocus) return;

			if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
			{
				if ((m_selectionButton >= 0) && (m_selectionButton < m_selectors.Count))
				{
					if (m_selectors[m_selectionButton] != null)
					{
						if (m_selectors[m_selectionButton].GetComponent<SelectableButtonView>() != null)
						{
							if (m_selectors[m_selectionButton].activeSelf)
							{
								m_selectors[m_selectionButton].GetComponent<SelectableButtonView>().InvokeButton();
							}
						}
					}
				}
			}

			bool keepSearching = true;

			// KEYS ACTION
			if (_nameEvent == KeysEventInputController.ACTION_KEY_UP_PRESSED)
			{
				do
				{
					m_selectionButton--;
					if (m_selectionButton < 0)
					{
						m_selectionButton = m_selectors.Count - 1;
					}
					if (m_selectors[m_selectionButton] == null)
					{
						keepSearching = true;
					}
					else
					{
						keepSearching = !m_selectors[m_selectionButton].activeSelf;
					}
				} while (keepSearching);
				EnableSelector();
			}
			if (_nameEvent == KeysEventInputController.ACTION_KEY_DOWN_PRESSED)
			{
				do
				{
					m_selectionButton++;
					if (m_selectionButton > m_selectors.Count - 1)
					{
						m_selectionButton = 0;
					}
					if (m_selectors[m_selectionButton] == null)
					{
						keepSearching = true;
					}
					else
					{
						keepSearching = !m_selectors[m_selectionButton].activeSelf;
					}
				} while (keepSearching);
				EnableSelector();
			}
			if (_nameEvent == KeysEventInputController.ACTION_KEY_LEFT_PRESSED)
			{
				do
				{
					m_selectionButton--;
					if (m_selectionButton < 0)
					{
						m_selectionButton = m_selectors.Count - 1;
					}
					if (m_selectors[m_selectionButton] == null)
					{
						keepSearching = true;
					}
					else
					{
						keepSearching = !m_selectors[m_selectionButton].activeSelf;
					}
				} while (keepSearching);
				EnableSelector();
			}
			if (_nameEvent == KeysEventInputController.ACTION_KEY_RIGHT_PRESSED)
			{
				do
				{
					m_selectionButton++;
					if (m_selectionButton > m_selectors.Count - 1)
					{
						m_selectionButton = 0;
					}
					if (m_selectors[m_selectionButton] == null)
					{
						keepSearching = true;
					}
					else
					{
						keepSearching = !m_selectors[m_selectionButton].activeSelf;
					}
				} while (keepSearching);
				EnableSelector();
			}
		}

		// -------------------------------------------
		/* 
		 * Enable the hightlight of the selected component
		 */
		private void EnableSelectedComponent(GameObject _componentSelected)
		{
			for (int i = 0; i < m_selectors.Count; i++)
			{
				if (m_selectors[i] == _componentSelected)
				{
					m_selectionButton = i;
					m_selectors[i].GetComponent<SelectableButtonView>().EnableSelector(true);
				}
				else
				{
					m_selectors[i].GetComponent<SelectableButtonView>().EnableSelector(false);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Enables the selectors to show with what elements
		 * the user is interacting with
		 */
		private void EnableSelector()
		{
			if (m_selectors == null) return;
			if (m_selectors.Count == 0) return;
			if (!((m_selectionButton >= 0) && (m_selectionButton < m_selectors.Count))) return;

			for (int i = 0; i < m_selectors.Count; i++)
			{
				if (m_selectors[i] != null)
				{
					if (m_selectors[i].transform != null)
					{
						if (m_selectors[i].GetComponent<SelectableButtonView>() != null)
						{
							if (m_selectionButton == i)
							{
								m_selectors[i].GetComponent<SelectableButtonView>().EnableSelector(true);
							}
							else
							{
								m_selectors[i].GetComponent<SelectableButtonView>().EnableSelector(false);
							}
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Disable the selectors
		 */
		private void DisableSelectors()
		{
			if (m_selectors == null) return;
			if (m_selectors.Count == 0) return;
			if (!((m_selectionButton >= 0) && (m_selectionButton < m_selectors.Count))) return;

			for (int i = 0; i < m_selectors.Count; i++)
			{
				if (m_selectors[i].GetComponent<SelectableButtonView>() != null)
				{
					m_selectors[i].GetComponent<SelectableButtonView>().EnableSelector(false);
				}
			}
		}


		// -------------------------------------------
		/* 
		 * SetAlpha
		 */
		private void SetAlpha(float _newAlpha)
		{
			if (m_canvasGroup != null)
			{
				if (m_canvasGroup.alpha != 1)
				{
					m_canvasGroup.alpha = _newAlpha;
				}
			}
		}

        // -------------------------------------------
        /* 
		 * SetLayer
		 */
        public void SetLayer(int _layer)
        {
        }

        // -------------------------------------------
        /* 
		 * CheckActivatedNameStack
		 */
        protected bool CheckActivatedNameStack()
        {
            if (ScreenController.InstanceBase != null)
            {
                return ScreenController.InstanceBase.ActivateNameStack;
            }
            return false;
        }

        // -------------------------------------------
        /* 
		 * Exit button pressed
		 */
        protected virtual void GoBackPressed()
        {
            if (ScreenController.InstanceBase != null)
            {
                if (ScreenController.InstanceBase.ActivateNameStack)
                {
                    ScreenController.InstanceBase.PopScreenNameFromStack();
                    string previousScreenName = ScreenController.InstanceBase.PopScreenNameFromStack(false);
                    if (previousScreenName != null)
                    {
                        UIEventController.Instance.DispatchUIEvent(EVENT_SCREENBASE_GO_BACK_EVENT);
                        GameObject screenView = ScreenController.InstanceBase.LookScreenInPool(previousScreenName);
#if ENABLE_OCULUS || ENABLE_WORLDSENSE || ENABLE_HTCVIVE || ENABLE_PICONEO
                        UIEventController.Instance.DispatchUIEvent(EVENT_SCREENBASE_REQUEST_SCREENVIEW_IN_POOL, true, this.gameObject, previousScreenName);
#else
                        UIEventController.Instance.DispatchUIEvent(EVENT_SCREENBASE_REQUEST_SCREENVIEW_IN_POOL, false, this.gameObject, previousScreenName);
#endif
                    }                    
                }
            }
        }
    }
}