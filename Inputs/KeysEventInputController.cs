using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * KeyEventInputController
	 * 
	 * Class used to process the system game input's
	 * 
	 * @author Esteban Gallardo
	 */
	public class KeysEventInputController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string ACTION_BUTTON_DOWN = "ACTION_BUTTON";
		public const string ACTION_BUTTON_UP = "ACTION_BUTTON_UP";
		public const string ACTION_SECONDARY_BUTTON_DOWN = "ACTION_SECONDARY_BUTTON";
		public const string ACTION_SECONDARY_BUTTON_UP = "ACTION_SECONDARY_BUTTON_UP";
		public const string ACTION_CANCEL_BUTTON = "ACTION_CANCEL_BUTTON";
		public const string ACTION_SET_ANCHOR_POSITION = "ACTION_SET_ANCHOR_POSITION";
		public const string ACTION_BACK_BUTTON = "ACTION_BACK_BUTTON";

		public const string ACTION_INVENTORY_VERTICAL = "ACTION_INVENTORY_VERTICAL";
		public const string ACTION_INVENTORY_HORIZONTAL = "ACTION_INVENTORY_HORIZONTAL";

		public const string ACTION_KEY_UP_PRESSED = "ACTION_KEY_UP_PRESSED";
		public const string ACTION_KEY_DOWN_PRESSED = "ACTION_KEY_DOWN_PRESSED";
		public const string ACTION_KEY_LEFT_PRESSED = "ACTION_KEY_LEFT_PRESSED";
		public const string ACTION_KEY_RIGHT_PRESSED = "ACTION_KEY_RIGHT_PRESSED";

		public const string ACTION_KEY_UP_RELEASED = "ACTION_KEY_UP_RELEASED";
		public const string ACTION_KEY_DOWN_RELEASED = "ACTION_KEY_DOWN_RELEASED";
		public const string ACTION_KEY_LEFT_RELEASED = "ACTION_KEY_LEFT_RELEASED";
		public const string ACTION_KEY_RIGHT_RELEASED = "ACTION_KEY_RIGHT_RELEASED";

        public const string ACTION_RECENTER = "ACTION_RECENTER";

        private int DIRECTION_NONE = -1;
		private int DIRECTION_LEFT = 1;
		private int DIRECTION_RIGHT = 2;
		private int DIRECTION_DOWN = 3;
		private int DIRECTION_UP = 4;

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		private const int AXIS_KEY_NONE = -1;
		private const int AXIS_KEY_DOWN_EVENT = 0;
		private const int AXIS_KEY_UP_EVENT = 1;
		private const int AXIS_KEY_DOWN_STILL_PRESSED_CODE = 2;

        // Buttons that can trigger pointer switching.

#if ENABLE_WORLDSENSE
        private const GvrControllerButton POINTER_ACTION_DOWN_DAYDREAMCONTROLLER = GvrControllerButton.TouchPadButton | GvrControllerButton.Trigger;
        private const GvrControllerButton APP_BUTTON_DAYDREAMCONTROLLER = GvrControllerButton.App;

        private static readonly GvrControllerHand[] AllHands = { GvrControllerHand.Right, GvrControllerHand.Left };
#endif

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static KeysEventInputController _instance;

		public static KeysEventInputController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(KeysEventInputController)) as KeysEventInputController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "KeysEventInputController";
						_instance = container.AddComponent(typeof(KeysEventInputController)) as KeysEventInputController;
					}
				}
				return _instance;
			}
		}

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public bool EnableActionButton = true;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
        private int m_currentDirection = -1;
		private bool m_enableActionOnMouseDown = true;
		private bool m_isDaydreamActivated = false;
		private int m_temporalNumberScreensActive = 0;
		private float m_timeAcumInventory = 0;

        // DAYDREAM
        private List<GameObject> m_controllerPointers;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool EnableActionOnMouseDown
		{
			get { return m_enableActionOnMouseDown; }
			set { m_enableActionOnMouseDown = value;
                Debug.LogError("m_enableActionOnMouseDown=" + m_enableActionOnMouseDown);
            }
		}

		public bool IsDaydreamActivated
		{
			get { return m_isDaydreamActivated; }
			set { m_isDaydreamActivated = value; }
		}

		public int TemporalNumberScreensActive
		{
			get { return m_temporalNumberScreensActive; }
			set { m_temporalNumberScreensActive = value; }			
		}

		// -------------------------------------------
		/* 
		 * Initialization
		 */
		public void Initialization()
		{
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public void Destroy()
		{
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * GetControllerKeyCode
		 */
		private int GetControllerKeyCode(int _directionCheck)
		{
			int eventType = AXIS_KEY_NONE;

			switch (_directionCheck)
			{
				case Utilities.DIRECTION_LEFT:
					if (Input.GetAxis("Horizontal") < 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = Utilities.DIRECTION_LEFT;
						}
					}
					break;

				case Utilities.DIRECTION_RIGHT:
					if (Input.GetAxis("Horizontal") > 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = Utilities.DIRECTION_RIGHT;
						}
					}
					break;

				case Utilities.DIRECTION_UP:
					if (Input.GetAxis("Vertical") > 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = Utilities.DIRECTION_UP;
						}
					}
					break;

				case Utilities.DIRECTION_DOWN:
					if (Input.GetAxis("Vertical") < 0)
					{
						if (m_currentDirection == -1)
						{
							eventType = AXIS_KEY_DOWN_EVENT;
							m_currentDirection = Utilities.DIRECTION_DOWN;
						}
					}
					break;

				case Utilities.DIRECTION_NONE:
					if ((Input.GetAxis("Horizontal") == 0) && (Input.GetAxis("Vertical") == 0) && (m_currentDirection != -1))
					{
						eventType = AXIS_KEY_UP_EVENT;
						m_currentDirection = -1;
					}
					break;
			}

			return eventType;
		}

        // -------------------------------------------
        /* 
		 * GetActionDaydreamController
		 */
        public bool GetActionDaydreamController(bool _isDown, string _eventDown = null, string _eventUp = null)
        {
#if ENABLE_WORLDSENSE
            if (m_controllerPointers == null)
            {
                GvrTrackedController[] gvrTrackedControllers = GameObject.FindObjectsOfType<GvrTrackedController>();
                if (gvrTrackedControllers.Length > 0)
                {
                    m_controllerPointers = new List<GameObject>();
                    foreach (GvrTrackedController trackControl in gvrTrackedControllers)
                    {
                        m_controllerPointers.Add(trackControl.gameObject);
                    }
                }
            }

            if (m_controllerPointers != null)
            {
                if (m_controllerPointers.Count > 0 && m_controllerPointers[0] != null)
                {
                    GvrTrackedController trackedController1 = m_controllerPointers[0].GetComponent<GvrTrackedController>();
                    foreach (var hand in AllHands)
                    {
                        GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
                        if (device.GetButtonDown(POINTER_ACTION_DOWN_DAYDREAMCONTROLLER))
                        {
                            // Match the button to our own controllerPointers list.
                            if (device == trackedController1.ControllerInputDevice)
                            {
                                if ((_eventDown != null) && (_eventDown.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventDown, 0.01f);
                                if (_isDown) return true;
                            }
                            else
                            {
                                if (_isDown) return false;
                            }
                        }
                        if (device.GetButtonUp(POINTER_ACTION_DOWN_DAYDREAMCONTROLLER))
                        {
                            // Match the button to our own controllerPointers list.
                            if (device == trackedController1.ControllerInputDevice)
                            {
                                if ((_eventUp != null) && (_eventUp.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventUp, 0.01f);
                                if (!_isDown) return true;
                            }
                            else
                            {
                                if (!_isDown) return false;
                            }
                        }
                    }
                }
            }
            return false;
#endif
            return false;
        }


        // -------------------------------------------
        /* 
		 * GetAppButtonDowDaydreamController
		 */
        public bool GetAppButtonDowDaydreamController()
        {
#if ENABLE_WORLDSENSE
            if (m_controllerPointers == null)
            {
                GvrTrackedController[] gvrTrackedControllers = GameObject.FindObjectsOfType<GvrTrackedController>();
                if (gvrTrackedControllers.Length > 0)
                {
                    m_controllerPointers = new List<GameObject>();
                    foreach (GvrTrackedController trackControl in gvrTrackedControllers)
                    {
                        m_controllerPointers.Add(trackControl.gameObject);
                    }
                }
            }

            if (m_controllerPointers != null)
            {
                if (m_controllerPointers.Count > 0 && m_controllerPointers[0] != null)
                {
                    GvrTrackedController trackedController1 = m_controllerPointers[0].GetComponent<GvrTrackedController>();
                    foreach (var hand in AllHands)
                    {
                        GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
                        if (device.GetButtonDown(APP_BUTTON_DAYDREAMCONTROLLER))
                        {
                            // Match the button to our own controllerPointers list.
                            if (device == trackedController1.ControllerInputDevice)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
#endif
            return false;
        }

        // -------------------------------------------
        /* 
		 * KeyInputPressActionButton
		 */
        private void KeyInputPressActionButton()
		{
            if (!EnableActionButton) return;

#if ENABLE_WORLDSENSE
            m_isDaydreamActivated = true;

            if (GvrControllerInput.Recentered)
            {
                UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
            }
#endif

#if ENABLE_OCULUS && !UNITY_EDITOR
            if (OVRInput.GetControllerWasRecentered())
            {
                UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
            }
#endif

            bool hasEntered = false;
			if (m_enableActionOnMouseDown)
			{
				// DAYDREAM CONTROLLER				
				if (m_isDaydreamActivated)
				{
                    hasEntered = GetActionDaydreamController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
                }

                if (!hasEntered)
				{
					bool fire1Triggered = false;

#if ENABLE_OCULUS && !UNITY_EDITOR
                    if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
                    {
                        UIEventController.Instance.DispatchUIEvent(ACTION_BUTTON_DOWN);
                    }
#else
                    if (m_temporalNumberScreensActive == 0)
                    {
                        if (Input.GetButtonDown("Fire1"))
                        {
                            fire1Triggered = true;
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.LeftControl)
                        || Input.GetKeyDown(KeyCode.JoystickButton0)
#if !UNITY_EDITOR
                        || Input.GetButtonDown("Fire1")
#endif
                        || fire1Triggered)
                    {
                        UIEventController.Instance.DispatchUIEvent(ACTION_BUTTON_DOWN);
                    }
#endif
                }
            }
			else
			{
				// DAYDREAM CONTROLLER
				if (m_isDaydreamActivated)
				{
                    hasEntered = GetActionDaydreamController(false, ACTION_SET_ANCHOR_POSITION, ACTION_BUTTON_DOWN);
                }

				if (!hasEntered)
				{
#if ENABLE_OCULUS && !UNITY_EDITOR
                    if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
                    {
                        UIEventController.Instance.DelayUIEvent(ACTION_SET_ANCHOR_POSITION, 0.01f);
                    }

                    if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
                    {
                        UIEventController.Instance.DelayUIEvent(ACTION_BUTTON_DOWN, 0.01f);
                    }
#else

                    bool fire1Triggered = false;
                    if (m_temporalNumberScreensActive == 0)
                    {
                        if (Input.GetButtonDown("Fire1"))
                        {
                            fire1Triggered = true;
                        }
                    }

                    bool fire1Released = false;
					if (m_temporalNumberScreensActive == 0)
					{
						if (Input.GetButtonUp("Fire1"))
						{
							fire1Released = true;
						}
					}

					// ACTION BUTTON ANCHOR
					if (Input.GetKeyDown(KeyCode.LeftControl)
						|| Input.GetKeyDown(KeyCode.JoystickButton0)
#if !UNITY_EDITOR
                        || Input.GetButtonDown("Fire1")
#endif
                        || fire1Triggered)
					{
						UIEventController.Instance.DispatchUIEvent(ACTION_SET_ANCHOR_POSITION);
					}

                    // ACTION BUTTON RELEASED
                    if (Input.GetKeyUp(KeyCode.LeftControl)
						|| Input.GetKeyUp(KeyCode.JoystickButton0)
#if !UNITY_EDITOR
                        || Input.GetButtonUp("Fire1")
#endif
                        || fire1Released)
					{
						UIEventController.Instance.DispatchUIEvent(ACTION_BUTTON_DOWN);
					}
#endif
                }
			}
		}

        // -------------------------------------------
        /* 
		 * KeyInputReleasedActionButton
		 */
        private void KeyInputReleasedActionButton()
        {
            if (!EnableActionButton) return;

            bool hasEntered = false;
            if (m_enableActionOnMouseDown)
            {
                // DAYDREAM CONTROLLER				
                if (m_isDaydreamActivated)
                {
#if ENABLE_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
                    hasEntered = GetActionDaydreamController(false, null, ACTION_BUTTON_UP);
#endif
                }
                if (!hasEntered)
                {
                    bool fire1Released = false;

#if ENABLE_OCULUS && !UNITY_EDITOR
                    if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
                    {
                        UIEventController.Instance.DelayUIEvent(ACTION_BUTTON_UP, 0.01f);
                    }
#else
                    if (m_temporalNumberScreensActive == 0)
                    {
                        if (Input.GetButtonUp("Fire1"))
                        {
                            fire1Released = true;
                        }
                    }

                    if (Input.GetKeyUp(KeyCode.LeftControl)
                        || Input.GetKeyUp(KeyCode.JoystickButton0)
#if !UNITY_EDITOR
                        || Input.GetButtonUp("Fire1")
#endif
                        || fire1Released)
                    {
                        UIEventController.Instance.DispatchUIEvent(ACTION_BUTTON_UP);
                    }
#endif
                }
            }
        }

        // -------------------------------------------
        /* 
		 * KeyInputCancelManagement
		 */
        private void KeyInputCancelManagement()
		{
			// DAYDREAM CONTROLLER
#if ENABLE_GOOGLEVR && (UNITY_ANDROID || UNITY_EDITOR)
			if (m_isDaydreamActivated)
			{

			}
#endif

			// CANCEL BUTTON
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_CANCEL_BUTTON);
			}
			// BACK BUTTON
			if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Joystick1Button1)
#if ENABLE_OCULUS
                || OVRInput.GetDown(OVRInput.Button.Back)
#endif
                )
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_BACK_BUTTON);
			}
		}

		// -------------------------------------------
		/* 
		 * KeyInputDirectionsManagement
		 */
		private void KeyInputDirectionsManagement()
		{
			// ARROWS KEYPAD
			if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				m_currentDirection = DIRECTION_LEFT;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_LEFT_PRESSED);
			}
			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				m_currentDirection = DIRECTION_RIGHT;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_RIGHT_PRESSED);
			}
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				m_currentDirection = DIRECTION_DOWN;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_UP_PRESSED);
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				m_currentDirection = DIRECTION_UP;
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_DOWN_PRESSED);
			}

			// ARROW KEYS UP
			if (Input.GetKeyUp(KeyCode.LeftArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_LEFT_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.RightArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_RIGHT_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.UpArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_UP_RELEASED);
			}
			if (Input.GetKeyUp(KeyCode.DownArrow))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_KEY_DOWN_RELEASED);
			}
		}

		// -------------------------------------------
		/* 
		 * KeyInputExtraManagement
		 */
		private void KeyInputExtraManagement()
		{
			// ACTION_SECONDARY_BUTTON
			if (Input.GetKeyDown(KeyCode.LeftShift) || (Input.GetKeyDown(KeyCode.Joystick1Button2)))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_SECONDARY_BUTTON_DOWN);
			}

			// INVENTORY BUTTON
			if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Joystick1Button6))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_INVENTORY_VERTICAL);
			}
			// INVENTORY BUTTON
			if (Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.Joystick1Button7))
			{
				UIEventController.Instance.DispatchUIEvent(ACTION_INVENTORY_HORIZONTAL);
			}
		}

        // -------------------------------------------
        /* 
		 * KeyInputManagment
		 */
        private void KeyInputManagment()
		{
			// ACTION BUTTON MANAGEMENT
			KeyInputPressActionButton();
            KeyInputReleasedActionButton();

            // CANCEL BUTTON MANAGEMENT
            KeyInputCancelManagement();

			// DIRECTIONS MANAGEMENT
			KeyInputDirectionsManagement();

			// EXTRA BUTTONS
			KeyInputExtraManagement();
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		void Update()
		{
            KeyInputManagment();
		}
    }

}