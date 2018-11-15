﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

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
        public const string EVENT_SCREENBASE_ANIMATION_SHOW = "EVENT_SCREENBASE_ANIMATION_SHOW";
        public const string EVENT_SCREENBASE_ANIMATION_HIDE = "EVENT_SCREENBASE_ANIMATION_HIDE";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const string CONTENT_COMPONENT_NAME = "Content";

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
        protected CanvasGroup m_canvasGroup;
		private bool m_hasFocus = true;

		private int m_selectionButton;
		private List<GameObject> m_selectors;

		private bool m_enabledSelector = true;

		protected bool m_hasBeenDestroyed = false;
        protected List<object> m_paramsAnimation;

        protected bool m_isMarkedToBeDestroyed = false;

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
                m_canvasGroup = m_screen.transform.Find(CONTENT_COMPONENT_NAME).GetComponent<CanvasGroup>();
				if (m_canvasGroup != null)
				{
                    m_canvasGroup.alpha = 1;
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
		* Called on the destroy method of the object
		*/
		void OnDestroy()
		{
			Debug.Log("ScreenBaseView::OnDestroy::NAME OBJECT DESTROYED[" + this.gameObject.name + "]");

			ClearListSelectors();
			m_selectors = null;
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
			if (_go != null)
			{
				if (_go.GetComponent<Button>() != null)
				{
					AddButtonToList(_go);
				}
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
		private void ClearListSelectors()
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
        protected virtual void AppearAnimation(List<object> _paramsAnimation)
        {
            m_paramsAnimation = _paramsAnimation;
            switch ((int)m_paramsAnimation[0])
            {
                case ScreenController.ANIMATION_MOVEMENT:
                    Vector3 startingPosition = new Vector3();
                    switch ((int)m_paramsAnimation[1])
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
                    float animationTime = (float)m_paramsAnimation[2];
                    m_canvasGroup.gameObject.transform.position = startingPosition;
                    InterpolatorController.Instance.Interpolate(m_canvasGroup.gameObject, new Vector3(0, 0, 0), animationTime, true);
                    break;

                case ScreenController.ANIMATION_ALPHA:
                    float startAlpha = (float)m_paramsAnimation[1];
                    float endAlpha = (float)m_paramsAnimation[2];
                    float alphaTime = (float)m_paramsAnimation[3];
                    m_canvasGroup.alpha = startAlpha;
                    AlphaController.Instance.Interpolate(m_canvasGroup.gameObject, startAlpha, endAlpha, alphaTime, true);
                    break;

                case ScreenController.ANIMATION_FADE:
                    Color endColor = (Color)m_paramsAnimation[1];
                    float colorTime = (float)m_paramsAnimation[2];
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_CREATE_FADE_SCREEN, this.gameObject, 1f, 0f, endColor, colorTime);
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(InterpolateData.EVENT_INTERPOLATE_COMPLETED, colorTime, m_canvasGroup.gameObject);
                    break;
            }
        }

        // -------------------------------------------
        /* 
		 * DisappearAnimation
		 */
        protected virtual void DisappearAnimation(List<object> _paramsAnimation)
        {
            if (_paramsAnimation != null)
            {
                m_paramsAnimation = _paramsAnimation;
            }            
            switch ((int)m_paramsAnimation[0])
            {
                case ScreenController.ANIMATION_MOVEMENT:
                    Vector3 endingPosition = new Vector3();
                    switch ((int)m_paramsAnimation[1])
                    {
                        case ScreenController.DIRECTION_UP:
                            endingPosition = new Vector3(0, UnityEngine.Screen.height, 0);
                            break;
                        case ScreenController.DIRECTION_DOWN:
                            endingPosition = new Vector3(0, -UnityEngine.Screen.height, 0);
                            break;
                        case ScreenController.DIRECTION_LEFT:
                            endingPosition = new Vector3(-UnityEngine.Screen.width, 0, 0);
                            break;
                        case ScreenController.DIRECTION_RIGHT:
                            endingPosition = new Vector3(UnityEngine.Screen.width, 0, 0);
                            break;
                    }
                    float animationTime = (float)m_paramsAnimation[2];
                    InterpolatorController.Instance.Interpolate(m_canvasGroup.gameObject, endingPosition, animationTime, true);
                    break;

                case ScreenController.ANIMATION_ALPHA:
                    float startAlpha = (float)m_paramsAnimation[1];
                    float endAlpha = (float)m_paramsAnimation[2];
                    float alphaTime = (float)m_paramsAnimation[3];
                    m_canvasGroup.alpha = endAlpha;
                    AlphaController.Instance.Interpolate(m_canvasGroup.gameObject, endAlpha, startAlpha, alphaTime);
                    break;

                case ScreenController.ANIMATION_FADE:
                    Color endColor = (Color)m_paramsAnimation[1];
                    float colorTime = (float)m_paramsAnimation[2];
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_CREATE_FADE_SCREEN, this.gameObject, 0f, 1f, endColor, colorTime);
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(InterpolateData.EVENT_INTERPOLATE_COMPLETED, colorTime, m_canvasGroup.gameObject);
                    break;
            }
        }

        // -------------------------------------------
        /* 
		 * Global manager of events
		 */
        protected virtual void OnMenuEvent(string _nameEvent, params object[] _list)
		{
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
                    AppearAnimation(((List<object>)_list[1]));
                }
            }
            if (_nameEvent == EVENT_SCREENBASE_ANIMATION_HIDE)
            {
                if (this.gameObject == (GameObject)_list[0])
                {
                    DisappearAnimation((_list.Length > 1)?((List<object>)_list[1]):null);
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
	}
}