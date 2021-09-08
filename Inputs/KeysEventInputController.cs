#if ENABLE_OCULUS
using OculusSampleFramework;
using YourVRUI;
#endif
#if ENABLE_HTCVIVE
using Wave.Native;
using Wave.Essence;
using YourVRUI;
#endif
#if ENABLE_PICONEO
using Pvr_UnitySDKAPI;
using YourVRUI;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;


namespace YourCommonTools
{

    public enum BASIC_DIRECTIONS { NONE = 0, LEFT = 1, RIGHT = 2, UP = 3, DOWN = 4 };

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

        public const string JOYSTICK_UP_PRESSED = "JOYSTICK_UP_PRESSED";
        public const string JOYSTICK_DOWN_PRESSED = "JOYSTICK_DOWN_PRESSED";
        public const string JOYSTICK_LEFT_PRESSED = "JOYSTICK_LEFT_PRESSED";
        public const string JOYSTICK_RIGHT_PRESSED = "JOYSTICK_RIGHT_PRESSED";

        public const string JOYSTICK_UP_RELEASED = "JOYSTICK_UP_RELEASED";
        public const string JOYSTICK_DOWN_RELEASED = "JOYSTICK_DOWN_RELEASED";
        public const string JOYSTICK_LEFT_RELEASED = "JOYSTICK_LEFT_RELEASED";
        public const string JOYSTICK_RIGHT_RELEASED = "JOYSTICK_RIGHT_RELEASED";

        public const string ACTION_RECENTER = "ACTION_RECENTER";

        public const string EVENT_REQUEST_TELEPORT_AVAILABLE = "EVENT_REQUEST_TELEPORT_AVAILABLE";

        private const int DIRECTION_NONE = -1;
		private const int DIRECTION_LEFT = 1;
		private const int DIRECTION_RIGHT = 2;
		private const int DIRECTION_DOWN = 3;
		private const int DIRECTION_UP = 4;

        private const float OCULUS_TRIGGER_SENSIBILITY = 0.7f;

        private const bool DEBUG_MESSAGES = false;

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        private const int AXIS_KEY_NONE = -1;
		private const int AXIS_KEY_DOWN_EVENT = 0;
		private const int AXIS_KEY_UP_EVENT = 1;
		private const int AXIS_KEY_DOWN_STILL_PRESSED_CODE = 2;

        // Buttons that can trigger pointer switching.

#if ENABLE_WORLDSENSE
        private const GvrControllerButton POINTER_TOUCHPAD_DAYDREAMCONTROLLER = GvrControllerButton.TouchPadButton;
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
				}
				return _instance;
			}
		}

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
        private bool m_hasBeenInited = false;
        private int m_currentDirection = -1;
		private bool m_enableActionOnMouseDown = true;
		private bool m_isDaydreamActivated = false;
		private int m_temporalNumberScreensActive = 0;
		private float m_timeAcumInventory = 0;

        // DAYDREAM
        private List<GameObject> m_controllerPointers;

        private bool m_enableActionButton = true;
        private bool m_enableInteractions = true;

        private bool m_ignoreNextAction = false;

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
        public bool IgnoreNextAction
        {
            get { return m_ignoreNextAction; }
            set { m_ignoreNextAction = value; }
        }
        
        // -------------------------------------------
        /* 
		 * Initialization
		 */
        public void Initialization()
		{
#if ENABLE_HTCVIVE
            if (!m_hasBeenInited)
            {
                m_hasBeenInited = true;
                // WaveVR_Utils.Event.Listen(wvr.WVR_EventType.WVR_EventType_RecenterSuccess.ToString(), OnRecentered);
                // WaveVR_Utils.Event.Listen(wvr.WVR_EventType.WVR_EventType_RecenterSuccess3DoF.ToString(), OnRecentered);
            }
#endif

#if ENABLE_OCULUS && !ENABLE_PARTY_2018
            if (!m_hasBeenInited)
            {
                m_hasBeenInited = true;
                OculusEventObserver.Instance.OculusEvent += new OculusEventHandler(OnOculusEvent);
            }
#endif


            m_hasBeenInited = true;
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
		 * Destroy all references
		 */
        public void Destroy()
		{
            if (Instance != null)
            {
#if ENABLE_HTCVIVE
                if (m_hasBeenInited)
                {
                    // WaveVR_Utils.Event.Remove(wvr.WVR_EventType.WVR_EventType_RecenterSuccess.ToString(), OnRecentered);
                    // WaveVR_Utils.Event.Remove(wvr.WVR_EventType.WVR_EventType_RecenterSuccess3DoF.ToString(), OnRecentered);
                }
#endif

#if ENABLE_OCULUS && !ENABLE_PARTY_2018
                if (m_addedRecenterListener)
                {
                    OVRManager.display.RecenteredPose -= DetectedRecentered;
                }
                if (m_hasBeenInited)
                {
                    if (OculusEventObserver.Instance != null) OculusEventObserver.Instance.OculusEvent -= OnOculusEvent;
                }            
#endif

                Destroy(_instance.gameObject);
                _instance = null;
                m_hasBeenInited = false;
            }
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

        // -------------------------------------------
        /* 
		 * IsRightHanded
		 */
        public bool IsRightHanded()
        {
#if ENABLE_OCULUS && ENABLE_QUEST
            return true;
#elif ENABLE_OCULUS && ENABLE_GO
            OVRPlugin.Handedness handedness = OVRPlugin.GetDominantHand();
            if (handedness == OVRPlugin.Handedness.RightHanded)
            {
                return true;
            }
            else
            {
                return false;
            }
#elif ENABLE_HTCVIVE
            // return !WaveVR_Controller.IsLeftHanded;
            return true;
#elif ENABLE_WORLDSENSE
            return IsRightHandWorldsense();
#elif ENABLE_PICONEO
            return true;
#else
            return true;
#endif
        }

        // -------------------------------------------
        /* 
		 * GetDominantDevice
		 */
#if ENABLE_HTCVIVE
        public WVR_DeviceType GetDominantDevice()
        {
            return HTCHandController.Instance.CurrentHandDevice;
        }
#endif

#if ENABLE_PICONEO
        public int GetDominantDevice()
        {
            return PicoNeoHandController.Instance.HandTypeSelected;
        }
#endif

        // -------------------------------------------
        /* 
		 * GetVectorThumbstick
		 */
        public Vector2 GetVectorThumbstick(bool _considerPressed = false)
        {

#if ENABLE_OCULUS && ENABLE_QUEST
#if ENABLE_PARTY_2018
            if (_considerPressed)
            {
                if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch))
                {
                    return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
                }
                else
                {
                    return Vector2.zero;
                }
            }
            else
            {
                return OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
            }
#else
            return OculusControllerInputs.Instance.GetInputThumbsticks(true);
#endif
#elif ENABLE_OCULUS && ENABLE_GO
            if (_considerPressed)
            {
                if (OVRInput.Get(OVRInput.Button.PrimaryTouchpad))
                {
                    return OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
                }
                else
                {
                    return Vector2.zero;
                }
            }
            else
            {
                return OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
            }
#elif ENABLE_HTCVIVE
            if (_considerPressed)
            {
                if (WXRDevice.ButtonHold(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Touchpad))
                {
                    return WXRDevice.ButtonAxis(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Touchpad);
                }
                else
                {
                    return Vector2.zero;
                }
            }
            else
            {
                return WXRDevice.ButtonAxis(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Touchpad);
            }
#elif ENABLE_PICONEO
            if (_considerPressed)
            {
                if (Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.TOUCHPAD))
                {
                    return Controller.UPvr_GetAxis2D(GetDominantDevice());
                }
                else
                {
                    return Vector2.zero;
                }
            }
            else
            {
                return Controller.UPvr_GetAxis2D(GetDominantDevice());
            }
#elif ENABLE_WORLDSENSE
            if (_considerPressed)
            {
                if (GetActionCurrentStateDaydreamController())
                {
                    return GetTouchVectorDaydreamController();
                }
                else
                {
                    return Vector2.zero;
                }
            }
            else
            {
                return GetTouchVectorDaydreamController();
            }
#else
            return Vector2.zero;
#endif
        }


        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************
        // OCULUS
        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************

        private bool m_vrActionPressed = false;

#if ENABLE_OCULUS
        // -------------------------------------------
        /* 
        * CheckOculusControllerOrHandActionDown
        */
        private bool CheckOculusControllerOrHandActionDown(bool _checkDown = true)
        {
#if ENABLE_GO
            if (_checkDown)
            {
                return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
            }
            else
            {
                return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger);  
            }
#else
            if (_checkDown)
            {
                return OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
            }
            else
            {
                return OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);                        
            }
#endif
        }

        // -------------------------------------------
        /* 
        * CheckOculusControllerOrHandStatus
        */
        private bool CheckOculusControllerOrHandStatus()
        {
#if ENABLE_GO
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
#else
            return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
#endif
        }

        private float m_timeoutStayRightHandIndex = 0;
        private float m_timerAcumOculusMenu = 0;

        // -------------------------------------------
        /* 
        * CheckOculusControllerOrHandAppDown
        */
        private bool CheckOculusControllerOrHandAppDown(bool _checkDown = true, bool _checkEvent = true)
        {
            bool buttonAWasTouched = false, buttonBWasTouched = false;

#if ENABLE_GO
            if (_checkDown)
            {
                buttonAWasTouched = OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad);
            }
            else
            {
                buttonAWasTouched = OVRInput.Get(OVRInput.Button.PrimaryTouchpad);
            }
#else
            if (_checkEvent)
            {
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
            }
            else
            {
                try { buttonAWasTouched = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch); } catch (Exception err) { Debug.LogError("++++OVRInput.Touch.One"); }
                try { buttonBWasTouched = OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch); } catch (Exception err) { Debug.LogError("++++OVRInput.Touch.Two"); }
            }
#endif

            return buttonAWasTouched || buttonBWasTouched;
        }

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
#if !ENABLE_PARTY_2018
            // DOWN
            bool resultDown = m_oculusActionButtonDown;
            m_oculusActionButtonDown = false;
            if (resultDown)
            {
                if (!m_ignoreNextAction)
                {
                    if (_isDown)
                    {
                        m_vrActionPressed = true;
                    }
                    else
                    {
                        m_vrActionPressed = false;
                    }
                    // Debug.LogError("DISPATCHING EVENT::m_vrActionPressed["+ m_vrActionPressed + "]::_eventDown=" + _eventDown);
                    if ((_eventDown != null) && (_eventDown.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventDown, 0.01f);
                    return m_vrActionPressed;
                }
                else
                {
                    m_ignoreNextAction = false;
                    return false;
                }
            }
            // UP
            bool resultUp = m_oculusActionButtonUp;
            m_oculusActionButtonUp = false;
            if (resultUp)
            {
                if (!m_ignoreNextAction)
                {
                    if (_isDown)
                    {
                        m_vrActionPressed = false;
                    }
                    else
                    {
                        m_vrActionPressed = true;
                    }
                    // Debug.LogError("DISPATCHING EVENT::m_vrActionPressed[" + m_vrActionPressed + "]::_eventUp=" + _eventUp);
                    if ((_eventUp != null) && (_eventUp.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventUp, 0.01f);
                    return m_vrActionPressed;
                }
                else
                {
                    m_ignoreNextAction = false;
                    return false;
                }
            }
#else
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
                                m_vrActionPressed = true;
                            }
                            else
                            {
                                m_vrActionPressed = false;
                            }
                            if ((_eventDown != null) && (_eventDown.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventDown, 0.01f);
                            return m_vrActionPressed;
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
                                m_vrActionPressed = false;
                            }
                            else
                            {
                                m_vrActionPressed = true;
                            }
                            if ((_eventUp != null) && (_eventUp.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventUp, 0.01f);
                            return m_vrActionPressed;
                        }
                    }
                    catch (Exception err) { }
#endif
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
#if !ENABLE_PARTY_2018
                if (m_oculusActionBeingPressed)
                {
                    if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                }
                return m_oculusActionBeingPressed;
#else
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
#endif
            }
            catch (Exception err) { }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetAppButtonDownOculusController
        */
        public bool GetAppButtonDownOculusController(string _event = null, bool _checkEvent = true, bool _consume = true)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_OCULUS
#if !ENABLE_PARTY_2018
                if ((m_isHandTrackingMode && m_oculusHandButtonDown && (m_oculusHandHandUsed == HAND.left)) ||
                    (!m_isHandTrackingMode && (m_buttonOneDown || m_buttonTwoDown)))
                {
                    if (_consume)
                    {
                        if (m_isHandTrackingMode)
                        {
                            m_oculusHandButtonDown = false;
                        }
                        else
                        {
                            m_buttonOneDown = false;
                            m_buttonTwoDown = false;
                        }
                    }

                    if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                    return true;
                }
#else
            if (CheckOculusControllerOrHandAppDown(true, _checkEvent))
            {
                if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                return true;
            }
#endif
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
#if !ENABLE_PARTY_2018
                if ((m_isHandTrackingMode && m_oculusHandButtonUp && (m_oculusHandHandUsed == HAND.left)) ||
                    (!m_isHandTrackingMode && (m_buttonOneUp || m_buttonTwoUp)))
                {
                    if (m_isHandTrackingMode)
                    {
                        m_oculusHandButtonUp = false;
                    }
                    else
                    {
                        m_buttonOneUp = false;
                        m_buttonTwoUp = false;
                    }

                    if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                    return true;
                }
#else
                if (CheckOculusControllerOrHandAppDown(false))
                {
                    if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
                    return true;
                }
#endif
#endif
            }
            catch (Exception err) { }
            return false;
        }

        // -------------------------------------------
        /* 
        * GetHandTriggerOculusController
        */
        public bool GetHandTriggerOculusController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }
            try
            { 
#if ENABLE_OCULUS
                bool isTeleportHandRight = false;
#if ENABLE_GO
                    Vector2 pressMenu = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
                    if (pressMenu.sqrMagnitude < 0.6f)
                    {
                        isTeleportHandRight = OVRInput.Get(OVRInput.Button.PrimaryTouchpad);
                    }
                    else
                    {
                        isTeleportHandRight = false;
                    }
#else
#if !ENABLE_PARTY_2018
                isTeleportHandRight = (m_oculusHandBeingPressed && (m_oculusHandHandUsed == HAND.right));
#else
                // RIGHT STICK
                isTeleportHandRight = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch) || OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
#endif
#endif
                // MANAGE RIGHT TOUCHED/DOWN
                if (isTeleportHandRight)
                {
				    if ((_event != null) && (_event.Length > 0)) UIEventController.Instance.DelayUIEvent(_event, 0.01f);
				    return true;
                }
#endif
            }
            catch (Exception err) { }
            return false;
        }
#endif

        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************
        // WORLDSENSE
        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************

        // -------------------------------------------
        /* 
        * GetToucVectorDaydreamController
        */
        public Vector2 GetTouchVectorDaydreamController()
        {
            if (!EnableInteractions)
            {
                return Vector2.zero;
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
                        return device.TouchPos;
                    }
                }
            }
#endif
            return Vector2.zero;
        }

        // -------------------------------------------
        /* 
        * IsRightHandWorldsense
        */
        public bool IsRightHandWorldsense()
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
                    foreach (GvrControllerHand hand in AllHands)
                    {
                        GvrControllerInputDevice device = GvrControllerInput.GetDevice(hand);
                        if ((hand == GvrControllerHand.Right) && device.IsDominantHand)
                        {
                            return true;
                        }
                    }
                }
            }
#endif
            return true;
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
        public bool GetAppButtonDowDaydreamController(bool _isDown = true, bool _checkEvent = true)
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
                        if (_checkEvent)
                        {
                            if (_isDown)
                            {
                                isPressedApp = device.GetButtonDown(APP_BUTTON_DAYDREAMCONTROLLER);
                            }
                            else
                            {
                                isPressedApp = device.GetButtonUp(APP_BUTTON_DAYDREAMCONTROLLER);
                            }
                        }
                        else
                        {
                            isPressedApp = device.GetButton(APP_BUTTON_DAYDREAMCONTROLLER);
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

        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************
        // HTC
        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************

        // -------------------------------------------
        /* 
        * GetActionCurrentStateHTCViveController
        */
        public bool GetActionCurrentStateHTCViveController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_HTCVIVE
                // +++++ TOUCH CONTROLLERS (PRESSED)
                try
                {
                    
                    if (WXRDevice.ButtonHold(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Trigger)
#if UNITY_EDITOR
                        || Input.GetKey(KeyCode.LeftControl)
#endif
                        )
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
        * GetActionHTCViveController
        */
        public bool GetActionHTCViveController(bool _isDown, string _eventDown = null, string _eventUp = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_HTCVIVE
                    // +++++ BUTTON CONTROLLERS (DOWN)
                    try
                    {
                        if (WXRDevice.ButtonPress(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Trigger)
#if UNITY_EDITOR
                            || Input.GetKeyDown(KeyCode.LeftControl)
#endif
                            )
                        {
                            if (!m_ignoreNextAction)
                            {
                                if (_isDown)
                                {
                                    m_vrActionPressed = true;
                                }
                                else
                                {
                                    m_vrActionPressed = false;
                                }
                                if ((_eventDown != null) && (_eventDown.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventDown, 0.01f);
                                return m_vrActionPressed;
                            }
                            else
                            {
                                m_ignoreNextAction = false;
                                return false;
                            }
                        }
                    }
                    catch (Exception err) { }
                
                    // +++++ BUTTON CONTROLLERS (UP)
                    try
                    {
                        if (WXRDevice.ButtonRelease(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Trigger)
#if UNITY_EDITOR
                            || Input.GetKeyUp(KeyCode.LeftControl)
#endif
                            )
                    {
                            if (!m_ignoreNextAction)
                            {
                                if (_isDown)
                                {
                                    m_vrActionPressed = false;
                                }
                                else
                                {
                                    m_vrActionPressed = true;
                                }
                                if ((_eventUp != null) && (_eventUp.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventUp, 0.01f);
                                return m_vrActionPressed;
                            }
                            else
                            {
                                m_ignoreNextAction = false;
                                return false;
                            }
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
        * GetMenuHTCViveController
        */
        public bool GetMenuHTCViveController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }
            
            try
            {
#if ENABLE_HTCVIVE
#if UNITY_EDITOR
                bool isTeleportHandRight = Input.GetKey(KeyCode.RightControl);
#else
                bool isTeleportHandRight = WXRDevice.ButtonHold(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Grip);
#endif

                // MANAGE RIGHT TOUCHED/DOWN
                if (isTeleportHandRight)
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
        * GetMenuDownHTCViveController
        */
        public bool GetMenuDownHTCViveController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_HTCVIVE
#if UNITY_EDITOR
                bool isTeleportHandRight = Input.GetKeyDown(KeyCode.RightControl);
#else
                bool isTeleportHandRight = WXRDevice.ButtonPress(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_A);
#endif

                // MANAGE RIGHT TOUCHED/DOWN
                if (isTeleportHandRight)
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
        * GetTeleportHTCViveController
        */
        public bool GetTeleportHTCViveController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_HTCVIVE
#if UNITY_EDITOR
                bool isTeleportHandRight = Input.GetKey(KeyCode.LeftControl);
#else
                bool isTeleportHandRight = WXRDevice.ButtonHold(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Grip);
#endif

                // MANAGE RIGHT TOUCHED/DOWN
                if (isTeleportHandRight)
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
        * GetAppDownHTCViveController
        */
        public bool GetAppDownHTCViveController(string _event = null, bool _checkEvent = true)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_HTCVIVE
            if (CheckHTCControllerAppDown(true, _checkEvent))
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
        * CheckHTCControllerAppDown
        */
        private bool CheckHTCControllerAppDown(bool _checkDown = true, bool _checkEvent = true)
        {
#if ENABLE_HTCVIVE
            bool buttonMenuTouched = false;
            if (_checkEvent)
            {
#if UNITY_EDITOR
                buttonMenuTouched = Input.GetKeyDown(KeyCode.Delete);
#else
                if (_checkDown)
                {
                    buttonMenuTouched = WXRDevice.ButtonPress(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Menu);

                }
                else
                {
                    buttonMenuTouched = WXRDevice.ButtonRelease(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Menu);
                }
#endif
            }
            else
            {
#if UNITY_EDITOR
                buttonMenuTouched = Input.GetKey(KeyCode.Delete);
#else
                buttonMenuTouched = WXRDevice.ButtonHold(GetDominantDevice(), WVR_InputId.WVR_InputId_Alias1_Menu);
#endif
            }
            return buttonMenuTouched;
#else
            return false;
#endif
            }

        // -------------------------------------------
        /* 
        * OnRecentered
        */
        private void OnRecentered(params object[] args)
        {
            UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
        }

        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************
        // OCULUS EXTRA
        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************

#if ENABLE_OCULUS
        private bool m_isHandTrackingMode = false;

        private bool m_oculusActionButtonDown = false;
        private bool m_oculusActionButtonUp = false;
        private bool m_oculusActionBeingPressed = false;
        private HAND m_oculusActionHandUsed = HAND.none;

        private bool m_oculusHandButtonDown = false;
        private bool m_oculusHandButtonUp = false;
        private bool m_oculusHandBeingPressed = false;
        private HAND m_oculusHandHandUsed = HAND.none;

        private bool m_buttonOneDown = false;
        private bool m_buttonTwoDown = false;
        private bool m_buttonOneUp = false;
        private bool m_buttonTwoUp = false;
        private bool m_buttonOneStay = false;
        private bool m_buttonTwoStay = false;

        private bool m_discardNextActionButton = false;

        private BASIC_DIRECTIONS m_previousDirectionJoystick = BASIC_DIRECTIONS.NONE;

        // -------------------------------------------
        /* 
		 * OnOculusEvent
		 */
        private void OnOculusEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_DOWN)
            {
                m_isHandTrackingMode = (bool)_list[0];
                m_oculusActionHandUsed = (HAND)_list[1];
                m_oculusActionButtonDown = true;
                if (m_isHandTrackingMode)
                {
                    if (m_oculusActionHandUsed == HAND.right)
                    {
                        OculusEventObserver.Instance.DispatchOculusEvent(OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_DOWN, m_oculusActionHandUsed, OculusControllerInputs.Instance.HandRightController.transform, OculusControllerInputs.Instance.RaycastLineRight);
                    }
                    else
                    {
                        OculusEventObserver.Instance.DispatchOculusEvent(OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_DOWN, m_oculusActionHandUsed, OculusControllerInputs.Instance.HandLeftController.transform, OculusControllerInputs.Instance.RaycastLineLeft);
                    }
                }
                if (DEBUG_MESSAGES)
                {
                    if (m_isHandTrackingMode)
                    {
                        Debug.LogError("EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_DOWN::++HAND TRACKING++ (ACTION) DOWN DETECTED[" + m_oculusActionHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                    }
                    else
                    {
                        Debug.LogError("EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_DOWN::**CONTROLLER** (ACTION) DOWN DETECTED[" + m_oculusActionHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                    }
                }
                m_oculusActionBeingPressed = true;
            }
            if (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_UP)
            {
                m_isHandTrackingMode = (bool)_list[0];
                m_oculusActionHandUsed = (HAND)_list[1];
                if (m_discardNextActionButton)
                {
                    m_discardNextActionButton = false;
                }
                else
                {
                    m_oculusActionButtonUp = true;
                    if (m_isHandTrackingMode)
                    {
                        if (m_oculusActionHandUsed == HAND.right)
                        {
                            OculusEventObserver.Instance.DispatchOculusEvent(OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UP, m_oculusActionHandUsed, OculusControllerInputs.Instance.HandRightController.transform, OculusControllerInputs.Instance.RaycastLineRight);
                        }
                        else
                        {
                            OculusEventObserver.Instance.DispatchOculusEvent(OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_INDEX_TRIGGER_UP, m_oculusActionHandUsed, OculusControllerInputs.Instance.HandLeftController.transform, OculusControllerInputs.Instance.RaycastLineLeft);
                        }
                    }
                    if (DEBUG_MESSAGES)
                    {
                        if (m_isHandTrackingMode)
                        {
                            Debug.LogError("EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_UP::++HAND TRACKING++ (ACTION) UP DETECTED[" + m_oculusActionHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                        }
                        else
                        {
                            Debug.LogError("EVENT_OCULUSHANDMANAGER_ACTION_BUTTON_UP::**CONTROLLER** (ACTION) UP DETECTED[" + m_oculusActionHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                        }
                    }
                }
                m_oculusActionBeingPressed = false;                
            }
            if (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_BUTTON_DOWN)
            {
                m_isHandTrackingMode = (bool)_list[0];
                m_oculusHandHandUsed = (HAND)_list[1];
                m_discardNextActionButton = (bool)_list[2];
                m_oculusHandButtonDown = true;
                m_oculusHandBeingPressed = true;
                if (DEBUG_MESSAGES)
                {
                    if (m_isHandTrackingMode)
                    {
                        Debug.LogError("EVENT_OCULUSHANDMANAGER_HAND_BUTTON_DOWN::++HAND TRACKING++ (HAND) DOWN DETECTED[" + m_oculusHandHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                    }
                    else
                    {
                        Debug.LogError("EVENT_OCULUSHANDMANAGER_HAND_BUTTON_DOWN::**CONTROLLER** (HAND) DOWN DETECTED[" + m_oculusHandHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                    }
                }                                 
            }
            if (_nameEvent == OculusHandsManager.EVENT_OCULUSHANDMANAGER_HAND_BUTTON_UP)
            {
                m_isHandTrackingMode = (bool)_list[0];
                m_oculusHandHandUsed = (HAND)_list[1];
                m_oculusHandButtonUp = true;
                m_oculusHandBeingPressed = false;
                if (DEBUG_MESSAGES)
                {
                    if (m_isHandTrackingMode)
                    {
                        Debug.LogError("EVENT_OCULUSHANDMANAGER_HAND_BUTTON_UP::++HAND TRACKING++ (HAND) UP DETECTED[" + m_oculusHandHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                    }
                    else
                    {
                        Debug.LogError("EVENT_OCULUSHANDMANAGER_HAND_BUTTON_UP::**CONTROLLER** (HAND) UP DETECTED[" + m_oculusHandHandUsed.ToString() + "]+++++++++++++++++++++++++++++++++++++++++++");
                    }
                }
            }
            if (_nameEvent == OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_ONE_DOWN)
            {
                m_buttonOneDown = true;
                m_buttonOneStay = true;
            }
            if (_nameEvent == OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_TWO_DOWN)
            {
                m_buttonTwoDown = true;
                m_buttonTwoStay = true;
            }
            if (_nameEvent == OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_ONE_UP)
            {
                m_buttonOneUp = true;
                m_buttonOneStay = false;
            }
            if (_nameEvent == OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_TWO_UP)
            {
                m_buttonTwoUp = true;
                m_buttonTwoStay = false;
            }
            if (_nameEvent == OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_ACTIVATION)
            {
                HAND hand = (HAND)_list[0];
                Transform handTransform = (Transform)_list[1];
                Vector2 handVector = (Vector2)_list[2];
                if (Mathf.Abs(handVector.x) > Mathf.Abs(handVector.y))
                {
                    if (handVector.x < 0)
                    {
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.LEFT;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_LEFT_PRESSED);
                    }
                    else
                    {
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.RIGHT;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_RIGHT_PRESSED);
                    }
                }
                else
                {
                    if (handVector.y < 0)
                    {
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.DOWN;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_DOWN_PRESSED);
                    }
                    else
                    {
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.UP;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_UP_PRESSED);
                    }
                }
            }
            if (_nameEvent == OculusControllerInputs.EVENT_OCULUSINPUTCONTROLLER_THUMBSTICK_DEACTIVATION)
            {
                HAND hand = (HAND)_list[0];
                Transform handTransform = (Transform)_list[1];
                Vector2 handVector = (Vector2)_list[2];
                switch (m_previousDirectionJoystick)
                {
                    case BASIC_DIRECTIONS.LEFT:
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.NONE;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_LEFT_RELEASED);
                        break;
                    case BASIC_DIRECTIONS.RIGHT:
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.NONE;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_RIGHT_RELEASED);
                        break;
                    case BASIC_DIRECTIONS.UP:
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.NONE;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_UP_RELEASED);
                        break;
                    case BASIC_DIRECTIONS.DOWN:
                        m_previousDirectionJoystick = BASIC_DIRECTIONS.NONE;
                        UIEventController.Instance.DispatchUIEvent(JOYSTICK_DOWN_RELEASED);
                        break;
                }
            }
        }
#endif

        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************
        // PICONEO
        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************

        // -------------------------------------------
        /* 
        * GetActionCurrentStatePicoNeoController
        */
        public bool GetActionCurrentStatePicoNeoController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_PICONEO
                // +++++ TOUCH CONTROLLERS (PRESSED)
                try
                {
                    if (Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.TRIGGER)
#if UNITY_EDITOR
                        || Input.GetKey(KeyCode.LeftControl)
#endif
                        )
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
        * GetActionPicoNeoController
        */
        public bool GetActionPicoNeoController(bool _isDown, string _eventDown = null, string _eventUp = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_PICONEO
                    // +++++ BUTTON CONTROLLERS (DOWN)
                    try
                    {
                        if (Controller.UPvr_GetKeyDown(GetDominantDevice(), Pvr_KeyCode.TRIGGER)
#if UNITY_EDITOR
                            || Input.GetKeyDown(KeyCode.LeftControl)
#endif
                            )
                        {
                            if (!m_ignoreNextAction)
                            {
                                if (_isDown)
                                {
                                    m_vrActionPressed = true;
                                }
                                else
                                {
                                    m_vrActionPressed = false;
                                }
                                if ((_eventDown != null) && (_eventDown.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventDown, 0.01f);
                                return m_vrActionPressed;
                            }
                            else
                            {
                                m_ignoreNextAction = false;
                                return false;
                            }
                        }
                    }
                    catch (Exception err) { }
                
                    // +++++ BUTTON CONTROLLERS (UP)
                    try
                    {
                        if (Controller.UPvr_GetKeyUp(GetDominantDevice(), Pvr_KeyCode.TRIGGER)
#if UNITY_EDITOR
                            || Input.GetKeyUp(KeyCode.LeftControl)
#endif
                            )
                    {
                            if (!m_ignoreNextAction)
                            {
                                if (_isDown)
                                {
                                    m_vrActionPressed = false;
                                }
                                else
                                {
                                    m_vrActionPressed = true;
                                }
                                if ((_eventUp != null) && (_eventUp.Length > 0)) UIEventController.Instance.DelayUIEvent(_eventUp, 0.01f);
                                return m_vrActionPressed;
                            }
                            else
                            {
                                m_ignoreNextAction = false;
                                return false;
                            }
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
        * GetMenuPicoNeoController
        */
        public bool GetMenuPicoNeoController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_PICONEO
#if UNITY_EDITOR
                bool isTeleportHandRight = Input.GetKey(KeyCode.RightControl);
#else
                bool isTeleportHandRight = Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.B) || Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.Y);
#endif

                // MANAGE RIGHT TOUCHED/DOWN
                if (isTeleportHandRight)
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
        * GetTeleportPicoNeoController
        */
        public bool GetTeleportPicoNeoController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_PICONEO
#if UNITY_EDITOR
                bool isTeleportHandRight = Input.GetKey(KeyCode.LeftControl);
#else
                bool isTeleportHandRight = Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.A) || Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.X);
#endif

                // MANAGE RIGHT TOUCHED/DOWN
                if (isTeleportHandRight)
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
        * GetMenuDownPicoNeoController
        */
        public bool GetMenuDownPicoNeoController(string _event = null)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_PICONEO
#if UNITY_EDITOR
                bool isTeleportHandRight = Input.GetKeyDown(KeyCode.RightControl);
#else
                bool isTeleportHandRight = Controller.UPvr_GetKeyDown(GetDominantDevice(), Pvr_KeyCode.B) || Controller.UPvr_GetKeyDown(GetDominantDevice(), Pvr_KeyCode.Y);
#endif

                // MANAGE RIGHT TOUCHED/DOWN
                if (isTeleportHandRight)
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
        * GetAppDownPicoNeoController
        */
        public bool GetAppDownPicoNeoController(string _event = null, bool _checkEvent = true)
        {
            if (!EnableInteractions)
            {
                return false;
            }

            try
            {
#if ENABLE_PICONEO
            if (CheckPicoNeoControllerAppDown(true, _checkEvent))
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
        * CheckPicoNeoControllerAppDown
        */
        private bool CheckPicoNeoControllerAppDown(bool _checkDown = true, bool _checkEvent = true)
        {
#if ENABLE_PICONEO
            bool buttonMenuTouched = false;
            if (_checkEvent)
            {
#if UNITY_EDITOR
                buttonMenuTouched = Input.GetKeyDown(KeyCode.Delete);
#else
                if (_checkDown)
                {
                    buttonMenuTouched = Controller.UPvr_GetKeyDown(GetDominantDevice(), Pvr_KeyCode.B) || Controller.UPvr_GetKeyDown(GetDominantDevice(), Pvr_KeyCode.Y);
                }
                else
                {
                    buttonMenuTouched = Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.B) || Controller.UPvr_GetKey(GetDominantDevice(), Pvr_KeyCode.Y);
                }
#endif
            }
            else
            {
#if UNITY_EDITOR
                buttonMenuTouched = Input.GetKey(KeyCode.Delete);
#else
                buttonMenuTouched = Controller.UPvr_GetKeyDown(GetDominantDevice(), Pvr_KeyCode.B) || Controller.UPvr_GetKeyDown(GetDominantDevice(), Pvr_KeyCode.Y);
#endif
            }
            return buttonMenuTouched;
#else
            return false;
#endif
        }

        // -------------------------------------------
        /* 
        * OnPicoNeoRecentered
        */
        private void OnPicoNeoRecentered(params object[] args)
        {
            UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
        }

        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************
        // COMMON
        // *****************************************************************************************************************************************************************
        // *****************************************************************************************************************************************************************

        private bool m_addedRecenterListener = false;

        // -------------------------------------------
        /* 
        * DetectedRecentered
        */
        private void DetectedRecentered()
        {
            UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
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
#if ENABLE_PARTY_2018
            if (OVRInput.GetControllerWasRecentered())
            {
                UIEventController.Instance.DispatchUIEvent(KeysEventInputController.ACTION_RECENTER);
            }
#else
            if (!m_addedRecenterListener)
            {
                m_addedRecenterListener = true;
                OVRManager.display.RecenteredPose += DetectedRecentered;
            }
#endif
#endif

            if (m_enableActionOnMouseDown)
            {
#if ENABLE_WORLDSENSE && !UNITY_EDITOR
                GetActionDaydreamController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
#elif ENABLE_OCULUS
                GetActionOculusController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
#elif ENABLE_HTCVIVE
                GetActionHTCViveController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
#elif ENABLE_PICONEO
                GetActionPicoNeoController(true, ACTION_BUTTON_DOWN, ACTION_BUTTON_UP);
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
#elif ENABLE_HTCVIVE
                GetActionHTCViveController(false, ACTION_SET_ANCHOR_POSITION, ACTION_BUTTON_DOWN);
#elif ENABLE_PICONEO
                GetActionPicoNeoController(false, ACTION_SET_ANCHOR_POSITION, ACTION_BUTTON_DOWN);
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
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIEventController.Instance.DispatchUIEvent(ACTION_CANCEL_BUTTON);
            }
#endif

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