#if ENABLE_OCULUS
using OculusSampleFramework;
#endif
using System;
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

        public const string EVENT_REQUEST_TELEPORT_AVAILABLE = "EVENT_REQUEST_TELEPORT_AVAILABLE";

        private int DIRECTION_NONE = -1;
		private int DIRECTION_LEFT = 1;
		private int DIRECTION_RIGHT = 2;
		private int DIRECTION_DOWN = 3;
		private int DIRECTION_UP = 4;

        private float OCULUS_TRIGGER_SENSIBILITY = 0.7f;

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
        // PRIVATE MEMBERS
        // ----------------------------------------------
        private int m_currentDirection = -1;
		private bool m_enableActionOnMouseDown = true;
		private bool m_isDaydreamActivated = false;
		private int m_temporalNumberScreensActive = 0;
		private float m_timeAcumInventory = 0;

        // DAYDREAM
        private List<GameObject> m_controllerPointers;

        private bool m_enableActionButton = true;
        private bool m_enableInteractions = true;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool EnableActionOnMouseDown
		{
			get { return m_enableActionOnMouseDown; }
			set { m_enableActionOnMouseDown = value; }
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
        public bool EnableActionButton
        {
            get { return m_enableActionButton; }
            set { m_enableActionButton = value; }
        }
        public bool EnableInteractions
        {
            get { return m_enableInteractions; }
            set { m_enableInteractions = value; }
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
        * GetActionDefaultController
        */
        public bool GetActionDefaultController(bool _isDown, string _eventDown = null, string _eventUp = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            // KEY PRESSED
            if (Input.GetKeyDown(KeyCode.LeftControl)
                || Input.GetKeyDown(KeyCode.JoystickButton0)
                || Input.GetMouseButtonDown(0)
#if !UNITY_EDITOR
                || Input.GetButtonDown("Fire1")
#endif
                )
            {
                if ((_eventDown != null) && (_eventDown.Length > 0)) UIEventController.Instance.DispatchUIEvent(_eventDown);
                if (_isDown) return true;
            }

            // KEY RELEASE
            if (Input.GetKeyUp(KeyCode.LeftControl)
                || Input.GetKeyUp(KeyCode.JoystickButton0)
                || Input.GetMouseButtonUp(0)
#if !UNITY_EDITOR
                        || Input.GetButtonUp("Fire1")
#endif
                        )
            {
                if ((_eventUp != null) && (_eventUp.Length > 0)) UIEventController.Instance.DispatchUIEvent(_eventUp);
                if (!_isDown) return true;
            }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetActionCurrentStateDefaultController
        */
        public bool GetActionCurrentStateDefaultController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            // KEY PRESSED
            if (Input.GetKey(KeyCode.LeftControl)
                || Input.GetKey(KeyCode.JoystickButton0)
                || Input.GetMouseButton(0)
#if !UNITY_EDITOR
                || Input.GetButton("Fire1")
#endif
                )
            {
                if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DispatchUIEvent(_event);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool m_oculusActionPressed = false;

#if ENABLE_OCULUS
        private PinchInteractionTool m_rightHandTrigger = null;
        private PinchInteractionTool m_leftHandTrigger = null;

        // -------------------------------------------
        /* 
        * GetRightPinchInteractionTool
        */
        private void GetRightPinchInteractionTool(bool _selectRightHand = true)
        {
            PinchInteractionTool finalSelectedHand = null;
            if (_selectRightHand)
            {
                finalSelectedHand = m_rightHandTrigger;
            }
            else
            {
                finalSelectedHand = m_leftHandTrigger;
            }

            if (finalSelectedHand == null)
            {
                PinchInteractionTool[] handsTriggers = GameObject.FindObjectsOfType<PinchInteractionTool>();
                for (int j = 0; j < handsTriggers.Length; j++)
                {
                    if (_selectRightHand)
                    {
                        if (handsTriggers[j].IsRightHandedTool)
                        {
                            m_rightHandTrigger = handsTriggers[j];
                        }
                    }
                    else
                    {
                        if (!handsTriggers[j].IsRightHandedTool)
                        {
                            m_leftHandTrigger = handsTriggers[j];
                        }
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
        * CheckOculusControllerOrHandActionDown
        */
        private bool CheckOculusControllerOrHandActionDown(bool _checkDown = true)
        {
            if (m_rightHandTrigger != null)
            {
                if (_checkDown)
                {
                    return (m_rightHandTrigger.ToolInputState == ToolInputState.PrimaryInputDown);
                }
                else
                {
                    return (m_rightHandTrigger.ToolInputState == ToolInputState.PrimaryInputUp);
                }
            }
            else
            {
                if (ScreenOculusControlSelectionView.ControOculusWithHands() && 
                    (GameObject.FindObjectOfType<PinchInteractionTool>() != null))
                {
                    GetRightPinchInteractionTool();
                    return false;
                }
                else
                {
                    if (_checkDown)
                    {
                        return Input.GetKeyDown(KeyCode.LeftControl) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
                    }
                    else
                    {
                        return Input.GetKeyUp(KeyCode.LeftControl) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);                        
                    }
                }
            }            
        }

        // -------------------------------------------
        /* 
        * CheckOculusControllerOrHandStatus
        */
        private bool CheckOculusControllerOrHandStatus()
        {
            if (m_rightHandTrigger != null)
            {
                return (m_rightHandTrigger.ToolInputState == ToolInputState.PrimaryInputDownStay);
            }
            else
            {
                if (ScreenOculusControlSelectionView.ControOculusWithHands() && 
                    (GameObject.FindObjectOfType<PinchInteractionTool>() != null))
                {
                    GetRightPinchInteractionTool();
                    return false;
                }
                else
                {
                    return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
                }
            }
        }

        private float m_timeoutStayRightHandIndex = 0;
        private float m_timerAcumOculusMenu = 0;

        // -------------------------------------------
        /* 
        * CheckOculusControllerOrHandAppDown
        */
        private bool CheckOculusControllerOrHandAppDown(bool _checkDown = true)
        {
            if (m_rightHandTrigger != null)
            {
                if (_checkDown)
                {
                    if (m_rightHandTrigger.ToolInputState == ToolInputState.PrimaryInputDownStay)
                    {
                        m_timeoutStayRightHandIndex += Time.deltaTime;
                        if (m_timeoutStayRightHandIndex > 2)
                        {
                            m_timeoutStayRightHandIndex = 0;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        m_timeoutStayRightHandIndex = 0;
                        return false;
                    }                        
                }
                else
                {
                    return (m_rightHandTrigger.ToolInputState == ToolInputState.PrimaryInputUp);
                }
            }
            else
            {
                if (ScreenOculusControlSelectionView.ControOculusWithHands() 
                    && (GameObject.FindObjectOfType<PinchInteractionTool>() != null))
                {
                    GetRightPinchInteractionTool(false);
                    return false;
                }
                else
                {
                    bool buttonAWasTouched = false, buttonBWasTouched = false;

#if ENABLE_GO
                    if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
                    {
                        m_timerAcumOculusMenu += Time.deltaTime;
                        if (m_timerAcumOculusMenu > 1)
                        {
                            buttonAWasTouched = true;
                        }
                        else
                        {
                            buttonAWasTouched = false;
                        }
                    }
                    else
                    {
                        m_timerAcumOculusMenu = 0;
                        buttonAWasTouched = false;
                    }
#else

                    if (_checkDown)
                    {
                        try { buttonAWasTouched = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch); } catch (Exception err) { Debug.LogError("++++OVRInput.Touch.One"); }
                        try { buttonBWasTouched = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch); } catch (Exception err) { Debug.LogError("++++OVRInput.Touch.Two"); }
                    }
                    else
                    {
                        try { buttonAWasTouched = OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch); } catch (Exception err) { Debug.LogError("++++OVRInput.Touch.One"); }
                        try { buttonBWasTouched = OVRInput.GetUp(OVRInput.Button.Two, OVRInput.Controller.RTouch); } catch (Exception err) { Debug.LogError("++++OVRInput.Touch.Two"); }
                    }
#endif

                    return buttonAWasTouched || buttonBWasTouched;
                }
            }
        }
#endif

        // -------------------------------------------
        /* 
        * GetActionOculusController
        */
        public bool GetActionOculusController(bool _isDown, string _eventDown = null, string _eventUp = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_OCULUS
                
                    // +++++ BUTTON CONTROLLERS (DOWN)
                    try
                    {
#if ENABLE_GO
                        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
#else
                        if (CheckOculusControllerOrHandActionDown(true)) {
#endif
                            if (_isDown)
                            {
                                m_oculusActionPressed = true;
                            }
                            else
                            {
                                m_oculusActionPressed = false;
                            }
                            if ((_eventDown != null) && (_eventDown.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventDown, 0.01f);
                            return m_oculusActionPressed;
                        }
                    }
                    catch (Exception err) { }
                
                    // +++++ BUTTON CONTROLLERS (UP)
                    try
                    {
#if ENABLE_GO
                    if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger)) {
#else
                        if (CheckOculusControllerOrHandActionDown(false)) {
#endif
                            if (_isDown)
                            {
                                m_oculusActionPressed = false;
                            }
                            else
                            {
                                m_oculusActionPressed = true;
                            }
                            if ((_eventUp != null) && (_eventUp.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventUp, 0.01f);
                            return m_oculusActionPressed;
                        }
                    }
                    catch (Exception err) { }
#endif
            }
            catch (Exception err) { }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetActionOculusController
        */
        public bool GetActionCurrentStateOculusController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_OCULUS
                // +++++ TOUCH CONTROLLERS (PRESSED)
                try
                {
                    if (CheckOculusControllerOrHandStatus())
                    {
                        if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                        return true;
                    }
                }
                catch (Exception err) { }
#endif
            }
            catch (Exception err) { }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetAppButtonDownOculusController
        */
        public bool GetAppButtonDownOculusController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_OCULUS
            if (CheckOculusControllerOrHandAppDown(true))
            {
                if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                return true;
            }
#endif
            }
            catch (Exception err) { }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetAppButtonUpOculusController
        */
        public bool GetAppButtonUpOculusController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_OCULUS
            if (CheckOculusControllerOrHandAppDown(false))
            {
                if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                return true;
            }
#endif
            }
            catch (Exception err) { }
            return false;
        }

        private bool m_rightPrimaryThumbstickWasTouched = false;
        private bool m_rightPrimaryThumbstickWasUntouched = false;
        private bool m_stickRightPressed = false;

        private float m_timeoutStayleftHandIndex = 0;
        private bool m_teleportActivated = false;

        // -------------------------------------------
        /* 
        * GetTeleportOculusController
        */
        public bool GetTeleportOculusController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }
            
            try
            {
#if ENABLE_OCULUS
                if (m_leftHandTrigger != null)
                {
                    if (m_leftHandTrigger.ToolInputState == ToolInputState.PrimaryInputDownStay)
                    {
                        if (!m_teleportActivated)
                        {
                            m_timeoutStayleftHandIndex += Time.deltaTime;
                            if (m_timeoutStayleftHandIndex > 2)
                            {
                                m_teleportActivated = true;
                                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_REQUEST_TELEPORT_AVAILABLE, m_leftHandTrigger.RayToolView.ReferenceRay.transform);
                                m_timeoutStayleftHandIndex = 0;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        m_timeoutStayleftHandIndex = 0;
                        return false;
                    }
                }
                else
                {
                    if (ScreenOculusControlSelectionView.ControOculusWithHands() &&
                        (GameObject.FindObjectOfType<PinchInteractionTool>() != null))
                    {
                        GetRightPinchInteractionTool(false);
                        return false;
                    }
                    else
                    {
                        // RIGHT STICK
                        bool isTeleportHandRight = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

                        // MANAGE RIGHT TOUCHED/DOWN
                        if (isTeleportHandRight)
                        {
                            if (!m_stickRightPressed)
                            {
                                m_stickRightPressed = true;
                                if (m_stickRightPressed)
                                {
                                    if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                                    return true;
                                }
                            }
                        }
                    }
                }
#endif
            }
            catch (Exception err) { }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetThumbstickUpOculusController
        */
        public bool GetTeleportUpOculusController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }
            
            try
            {
#if ENABLE_OCULUS
                if (m_leftHandTrigger != null)
                {
                    if (m_leftHandTrigger.ToolInputState == ToolInputState.PrimaryInputUp)
                    {
                        m_timeoutStayleftHandIndex = 0;
                        m_teleportActivated = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    bool isTeleportHandRight = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
                    if (!isTeleportHandRight)
                    {
                        if (m_stickRightPressed)
                        {
                            m_stickRightPressed = false;
                            if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                            return true;
                        }
                    }
                }
#endif
            }
            catch (Exception err) { }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetActionDaydreamController
        */
        public bool GetActionDaydreamController(bool _isDown, string _eventDown = null, string _eventUp = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

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
#endif
            return false;
        }


        // -------------------------------------------
        /* 
        * GetActionCurrentStateDaydreamController
        */
        public bool GetActionCurrentStateDaydreamController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

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
                        if (device.GetButton(POINTER_ACTION_DOWN_DAYDREAMCONTROLLER))
                        {
                            // Match the button to our own controllerPointers list.
                            if (device == trackedController1.ControllerInputDevice)
                            {
                                if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                                return true;
                            }
                            else
                            {
                                return false;
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
		 * GetAppButtonDowDaydreamController
		 */
        public bool GetAppButtonDowDaydreamController(bool _isDown = true)
        {
            if (!EnableInteractions) 
            {
                return false;
            }

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
                        bool isPressedApp = false;
                        if (_isDown)
                        {
                            isPressedApp = device.GetButtonDown(APP_BUTTON_DAYDREAMCONTROLLER);
                        }
                        else
                        {
                            isPressedApp = device.GetButtonUp(APP_BUTTON_DAYDREAMCONTROLLER);
                        }
                        if (isPressedApp)
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
		 * KeyInputActionButton
		 */
        private void KeyInputActionButton()
		{
            if (!EnableActionButton) return;

#if ENABLE_WORLDSENSE
            m_isDaydreamActivated = true;

            if (GvrControllerInput.Recentered)
            {
                UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
            }
#endif

#if ENABLE_OCULUS
            if (OVRInput.GetControllerWasRecentered())
            {
                UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
            }
#endif

			if (m_enableActionOnMouseDown)
			{
#if ENABLE_WORLDSENSE && !UNITY_EDITOR
                GetActionDaydreamController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
#elif ENABLE_OCULUS
                GetActionOculusController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
#else
                GetActionDefaultController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
#endif
            }
			else
			{
#if ENABLE_WORLDSENSE && !UNITY_EDITOR
                GetActionDaydreamController(false, ACTION_SET_ANCHOR_POSITION, ACTION_BUTTON_DOWN);
#elif ENABLE_OCULUS
                GetActionOculusController(false, ACTION_SET_ANCHOR_POSITION, ACTION_BUTTON_DOWN);
#else
                GetActionDefaultController(false, ACTION_SET_ANCHOR_POSITION, ACTION_BUTTON_DOWN);
#endif
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
#if ENABLE_OCULUS && !ENABLE_QUEST
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
			KeyInputActionButton();

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