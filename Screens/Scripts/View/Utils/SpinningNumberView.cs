using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * SpinningNumberView
	 * 
	 * @author Esteban Gallardo
	 */
    public class SpinningNumberView : MonoBehaviour
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_SPINNINGNUMBER_UPDATED_VALUE	= "EVENT_SPINNINGNUMBER_UPDATED_VALUE";
        public const string EVENT_SPINNINGNUMBER_REQUEST_RAYCAST = "EVENT_SPINNINGNUMBER_REQUEST_RAYCAST";
        public const string EVENT_SPINNINGNUMBER_RESPONSE_RAYCAST = "EVENT_SPINNINGNUMBER_RESPONSE_RAYCAST";
        public const string EVENT_SPINNINGNUMBER_INPUTFIELD_UPDATED = "EVENT_SPINNINGNUMBER_INPUTFIELD_UPDATED";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const float TIME_TO_AUTOMATICALLY_ACTIVATE = 0.5f;
        public const float TIME_TO_UPDATE = 0.1f;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private GameObject m_source;
        private int m_value = -1;
        private int m_increment = -1;
        private int m_bottom = -1;
        private int m_top = -1;

        private InputField m_inputField = null;
        private Button m_decrease = null;
        private Button m_increase = null;

        private bool m_hasBeenDestroyed = false;

        private bool m_increaseDownEvent = false;
        private bool m_decreaseDownEvent = false;
        private float m_increaseActivated = -1;
        private float m_decreaseActivated = -1;
        private float m_timeAcum = 0;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
        public int Value
        {
            get { return m_value; }
            set
            {
                m_value = ((value / m_increment) * m_increment);
                if (m_bottom > m_value) m_value = m_bottom;
                if (m_top < m_value) m_value = m_top;
                m_inputField.text = m_value.ToString();
                UIEventController.Instance.DispatchUIEvent(EVENT_SPINNINGNUMBER_UPDATED_VALUE, m_source, m_value);
            }
        }

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
            m_source = (GameObject)_list[0];
            m_value = (int)_list[1];
            m_increment = (int)_list[2];
            m_bottom = (int)_list[3];
            m_top = (int)_list[4];

            if (this.transform.Find("InputField") != null)
            {
                m_inputField = this.transform.Find("InputField").GetComponent<InputField>();
                m_inputField.text = m_value.ToString();
                m_inputField.onEndEdit.AddListener(OnEditChange);
            }

            if (this.transform.Find("Decrease") != null)
            {
                m_decrease = this.transform.Find("Decrease").GetComponent<CustomButton>();
            }

            if (this.transform.Find("Increase") != null)
            {
                m_increase = this.transform.Find("Increase").GetComponent<CustomButton>();
            }

            UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
        }

        // -------------------------------------------
        /* 
         * OnDestroy
         */

        private void OnDestroy()
        {
            Destroy();
        }

        // -------------------------------------------
        /* 
         * Destroy
         */

        public void Destroy()
        {
            if (m_hasBeenDestroyed) return;
            m_hasBeenDestroyed = true;

            UIEventController.Instance.UIEvent -= OnUIEvent;
        }

        // -------------------------------------------
        /* 
         * OnEditChange
         */
        private void OnEditChange(string arg0)
        {
            ParseTextNumber(arg0);
        }

        // -------------------------------------------
        /* 
         * ParseTextNumber
         */
        private void ParseTextNumber(string _textNumber)
        {
            int editedValue = -1;
            if (int.TryParse(_textNumber, out editedValue))
            {
                Value = editedValue;
            }
        }

        // -------------------------------------------
        /* 
         * ProcessReleasedButton
         */
        private void ProcessReleasedButton()
        {
            if (m_increaseDownEvent || (m_increaseActivated != -1))
            {
                m_increaseDownEvent = false;
                if (m_increaseActivated == -1)
                {
                    Value += m_increment;
                }
                m_increaseActivated = -1;
            }
            if (m_decreaseDownEvent || (m_decreaseActivated != -1))
            {
                m_decreaseDownEvent = false;
                if (m_decreaseActivated == -1)
                {
                    Value -= m_increment;
                }
                m_decreaseActivated = -1;
            }
        }

        // -------------------------------------------
        /* 
         * OnUIEvent
         */
        private void OnUIEvent(string _nameEvent, params object[] _list)
		{
#if (ENABLE_OCULUS || ENABLE_WORLDSENSE || ENABLE_HTCVIVE || ENABLE_PICONEO)
            if (_nameEvent == EVENT_SPINNINGNUMBER_INPUTFIELD_UPDATED)
            {
                InputField targetInput = (InputField)_list[0];
                if (m_inputField == targetInput)
                {
                    ParseTextNumber(m_inputField.text);
                }
            }
            if (!KeysEventInputController.Instance.EnableActionOnMouseDown)
            {
                if (_nameEvent == KeysEventInputController.ACTION_SET_ANCHOR_POSITION)
                {
                    UIEventController.Instance.DispatchUIEvent(EVENT_SPINNINGNUMBER_REQUEST_RAYCAST, this.gameObject);
                }
                if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
                {
                    ProcessReleasedButton();
                }
            }
            else
            {
                if (_nameEvent == KeysEventInputController.ACTION_BUTTON_DOWN)
                {
                    UIEventController.Instance.DispatchUIEvent(EVENT_SPINNINGNUMBER_REQUEST_RAYCAST, this.gameObject);
                }
                if (_nameEvent == KeysEventInputController.ACTION_BUTTON_UP)
                {
                    ProcessReleasedButton();
                }
            }
            if (_nameEvent == EVENT_SPINNINGNUMBER_RESPONSE_RAYCAST)
            {
                GameObject originGO = (GameObject)_list[0];
                if (this.gameObject == originGO)
                {
                    if (_list[1] != null)
                    {
                        GameObject targetGO = (GameObject)_list[1];
                        if (targetGO == m_decrease.gameObject)
                        {
                            m_decreaseDownEvent = true;
                        }
                        if (targetGO == m_increase.gameObject)
                        {
                            m_increaseDownEvent = true;
                        }
                    }
                }
            }
#else            
            if (_nameEvent == CustomButton.BUTTON_PRESSED_DOWN)
            {
                if ((GameObject)_list[0] == m_decrease.gameObject)
                {
                    m_decreaseDownEvent = true; 
                }
                if ((GameObject)_list[0] == m_increase.gameObject)
                {
                    m_increaseDownEvent = true;
                }
            }
            if ((_nameEvent == KeysEventInputController.ACTION_BUTTON_UP) 
                || (_nameEvent == CustomButton.BUTTON_RELEASE_UP)
                || (_nameEvent == CustomButton.BUTTON_EXITED_UP))
            {
                ProcessReleasedButton();
            }
#endif
        }

        // -------------------------------------------
        /* 
         * Update
         */
        private void Update()
        {
            if (m_decreaseDownEvent)
            {
                m_timeAcum += Time.deltaTime;
                if (m_timeAcum > TIME_TO_AUTOMATICALLY_ACTIVATE)
                {
                    m_timeAcum = 0;
                    m_decreaseDownEvent = false;
                    m_decreaseActivated = Time.deltaTime;
                }
            }
            else
            if (m_increaseDownEvent)
            {
                m_timeAcum += Time.deltaTime;
                if (m_timeAcum > TIME_TO_AUTOMATICALLY_ACTIVATE)
                {
                    m_timeAcum = 0;
                    m_increaseDownEvent = false;
                    m_increaseActivated = Time.deltaTime;
                }
            }

            if (m_decreaseActivated >= 0)
            {
                m_decreaseActivated += Time.deltaTime;
                if (m_decreaseActivated > TIME_TO_UPDATE)
                {
                    m_decreaseActivated = 0;
                    Value -= m_increment;
                }
            }
            if (m_increaseActivated >= 0)
            {
                m_increaseActivated += Time.deltaTime;
                if (m_increaseActivated > TIME_TO_UPDATE)
                {
                    m_increaseActivated = 0;
                    Value += m_increment;
                }
            }
        }
    }
}