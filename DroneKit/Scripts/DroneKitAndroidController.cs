using UnityEngine;

namespace YourCommonTools
{
	/******************************************
	 * 
	 * Runs a python script that allow to change the velocity vector
     * of the drone
	 * 
	 * @author Esteban Gallardo
	 */
	public class DroneKitAndroidController : StateManager 
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_DRONEKITCONTROLLER_CONNECTED          = "EVENT_DRONEKITCONTROLLER_CONNECTED";
        public const string EVENT_DRONEKITCONTROLLER_ARMED              = "EVENT_DRONEKITCONTROLLER_ARMED";
        public const string EVENT_DRONEKITCONTROLLER_TAKEN_OFF          = "EVENT_DRONEKITCONTROLLER_TAKEN_OFF";
        public const string EVENT_DRONEKITCONTROLLER_READY              = "EVENT_DRONEKITCONTROLLER_READY";
        public const string EVENT_DRONEKITCONTROLLER_START_FLYING       = "EVENT_DRONEKITCONTROLLER_START_FLYING";
        public const string EVENT_DRONEKITCONTROLLER_FLYING             = "EVENT_DRONEKITCONTROLLER_FLYING";
        public const string EVENT_DRONEKITCONTROLLER_LANDING            = "EVENT_DRONEKITCONTROLLER_LANDING";
        public const string EVENT_DRONEKITCONTROLLER_LANDED             = "EVENT_DRONEKITCONTROLLER_LANDED";

        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------
        public const int STATE_DISCONNECTED = 0;
        public const int STATE_CONNECTED    = 1;
        public const int STATE_ARMED        = 2;
        public const int STATE_TAKEOFF      = 3;
        public const int STATE_IDLE         = 4;
        public const int STATE_FLYING       = 5;
        public const int STATE_LANDING      = 6;
        public const int STATE_RETURN_HOME  = 7;
        public const int STATE_CRASH        = 8;

        public const int DRONE_DEFAULT_LISTENING_CHANNEL= 14550;
        public const int DRONE_DEFAULT_HEIGHT_TAKEOFF   = 10;

        public const int COMMAND_STATE_SUCCESS = 1;
        public const int COMMAND_STATE_ERROR = 0;
        public const int COMMAND_STATE_TIMEOUT = -1;

        public const int MODE_COPTER_STABILIZE  = 0;
        public const int MODE_COPTER_ACRO       = 1;
        public const int MODE_COPTER_ALT_HOLD   = 2;
        public const int MODE_COPTER_AUTO       = 3;
        public const int MODE_COPTER_GUIDED     = 4;
        public const int MODE_COPTER_LOITER     = 5;
        public const int MODE_COPTER_RTL        = 6;
        public const int MODE_COPTER_CIRCLE     = 7;
        public const int MODE_COPTER_LAND       = 9;
        public const int MODE_COPTER_DRIFT      = 11;
        public const int MODE_COPTER_SPORT      = 13;
        public const int MODE_COPTER_FLIP       = 14;
        public const int MODE_COPTER_AUTOTUNE   = 15;
        public const int MODE_COPTER_POSHOLD    = 16;
        public const int MODE_COPTER_BRAKE      = 17;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------
        private static DroneKitAndroidController _instance;

        public static DroneKitAndroidController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType<DroneKitAndroidController>();
                }
                return _instance;
            }
        }
        
        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
#if ENABLE_DRONEANDROIDCONTROLLER
        private AndroidJavaObject m_dronekitAndroid;
#endif
        private float m_timeoutRunning = 0;
        private float m_nextTimeout = 0;
        private int m_stateAndroid = -1;
        private Vector3 m_currentVelocity = Vector3.zero;
        private Vector3 m_nextVelocity = Vector3.zero;
        private float m_distance = 2;
        private float m_speedDrone = 5;
        private float m_heightTakeOff = 10;
        private float m_initialAltitude = -1;
        private bool m_reportedToTakeOff = false;
        private bool m_reportedLanding = false;
        private bool m_autoStart = false;

        // -------------------------------------------
        /* 
		 * Drone set up call
		 */
        public void Initialitzation(bool _autoStart = false, int _portNumberDrone = 14550, int _heightTakeOff = 10)
        {
            if (m_dronekitAndroid == null)
            {
                m_heightTakeOff = _heightTakeOff;
                m_autoStart = _autoStart;

                m_dronekitAndroid = new AndroidJavaObject("com.yourvrexperience.controldrone.ControlDroneDronekit");
                m_dronekitAndroid.Call("initControlDrone", _portNumberDrone, _heightTakeOff);

                Invoke("ConnectDrone", 3);
            }
        }

        // -------------------------------------------
        /* 
        * Start the drone with the desired vector velocity
        */
        public void RunVelocity(float _vx, float _vy, float _vz, bool _autoStart = false, float _timeoutRunning = 5, float _distance = 10, int _speedDrone = 5, int _heightTakeOff = 10)
		{
            m_autoStart = _autoStart;
            m_currentVelocity = new Vector3(_vx, _vy, _vz);
            m_distance = _distance;
            m_speedDrone = _speedDrone;
            m_heightTakeOff = _heightTakeOff;

            if (m_timeoutRunning > 0)
            {
                m_nextVelocity = new Vector3(_vx, _vy, _vz);
                m_nextTimeout = _timeoutRunning;
                return;
            }

#if ENABLE_DRONEANDROIDCONTROLLER
            m_timeoutRunning = _timeoutRunning;
            if (m_dronekitAndroid != null)
            {
                m_stateAndroid = m_dronekitAndroid.Call<System.Int32>("getStateDrone");
                if (m_autoStart)
                {
                    switch (m_stateAndroid)
                    {
                        case STATE_DISCONNECTED:
                            ChangeState(STATE_CONNECTED);
                            break;

                        case STATE_CONNECTED:
                            ChangeState(STATE_ARMED);
                            break;

                        case STATE_ARMED:
                            ChangeState(STATE_TAKEOFF);
                            break;

                        case STATE_LANDING:
                            ChangeState(STATE_LANDING);
                            break;

                        case STATE_FLYING:
                            ChangeState(STATE_FLYING);
                            break;
                    }
                }
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * ConnectDrone
		 */
        public void ConnectDrone()
        {
            ChangeState(STATE_CONNECTED);
        }

        // -------------------------------------------
        /* 
		 * ArmDrone
		 */
        public void ArmDrone()
        {
            ChangeState(STATE_ARMED);
        }

        // -------------------------------------------
        /* 
		 * TakeOffDrone
		 */
        public void TakeOffDrone()
        {
            ChangeState(STATE_TAKEOFF);
        }

        // -------------------------------------------
        /* 
		 * LandDrone
		 */
        public void LandDrone()
        {
            ChangeState(STATE_LANDING);
        }

        // -------------------------------------------
        /* 
		 * SetModeOperation
		 */
        public void SetModeOperation(int _modeOperation)
        {
            m_dronekitAndroid.Call("setModeDrone", _modeOperation);
        }

        // -------------------------------------------
        /* 
		 * FlyDrone
		 */
        public void FlyDrone()
        {
            Debug.LogError("DroneKit::ALTITUDE TAKEOFF REACHED, NOW GOING TO FLY STATE!!!!!");
            if (m_currentVelocity != Vector3.zero)
            {
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_START_FLYING, m_timeoutRunning);
                ChangeState(STATE_FLYING);
            }
        }

        // -------------------------------------------
        /* 
         * ChangeState
         */
        public override void ChangeState(int newState)
        {
            base.ChangeState(newState);

#if ENABLE_DRONEANDROIDCONTROLLER
            bool resultAction = false;
            switch (m_state)
            {
                case STATE_DISCONNECTED:
                    resultAction = m_dronekitAndroid.Call<System.Boolean>("disconnectDrone");
                    Debug.LogError("DroneKit::DISCONNECTION WITH DRONE[" + resultAction + "]+++++++++++++");
                    break;

                case STATE_CONNECTED:
                    resultAction = m_dronekitAndroid.Call<System.Boolean>("connectDrone");
                    Debug.LogError("DroneKit::CONNECTION WITH DRONE["+ resultAction + "]+++++++++++++");
                    if (resultAction)
                    {
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_CONNECTED);
                        if (m_autoStart) Invoke("ArmDrone", 3);
                    }
                    break;

                case STATE_ARMED:
                    resultAction = m_dronekitAndroid.Call<System.Boolean>("armDrone");
                    Debug.LogError("DroneKit::ARMING DRONE[" + resultAction + "]+++++++++++++");
                    if (resultAction)
                    {
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_ARMED);
                        m_initialAltitude = (float)m_dronekitAndroid.Call<System.Double>("getAltitude");
                        Debug.LogError("DroneKit::DRONE INITIAL ALTITUDE[" + m_initialAltitude + "]+++++++++++++");
                        if (m_autoStart) Invoke("TakeOffDrone", 3);
                    }
                    break;

                case STATE_TAKEOFF:
                    m_reportedToTakeOff = false;
                    resultAction = m_dronekitAndroid.Call<System.Boolean>("takeOffDrone");                    
                    Debug.LogError("DroneKit::TAKING OFF DRONE[" + resultAction + "]+++++++++++++");
                    break;

                case STATE_LANDING:
                    resultAction = m_dronekitAndroid.Call<System.Boolean>("landModeDrone");
                    Debug.LogError("DroneKit::LANDING DRONE[" + resultAction + "]+++++++++++++");
                    break;
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * Main loop
		 */
        private void Update()
        {
            base.Logic();

#if ENABLE_DRONEANDROIDCONTROLLER
            switch (m_state)
            {
                //////////////////////////////////////////////
                case STATE_TAKEOFF:
                    int takeOffState = m_dronekitAndroid.Call<System.Int32>("getTakeOffState");
                    Debug.LogError("DroneKit::STATE_TAKEOFF::takeOffState="+ takeOffState);
                    if (takeOffState == COMMAND_STATE_SUCCESS)
                    {
                        if (!m_reportedToTakeOff)
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_TAKEN_OFF);
                        }
                        m_reportedToTakeOff = true;
                        float currentAltitude = (float)m_dronekitAndroid.Call<System.Double>("getAltitude");
                        float targetAltitude = (m_initialAltitude + m_heightTakeOff) - 1;
                        Debug.LogError("DroneKit::TAKE OF SUCCESS::ALTITUDE["+ currentAltitude + "/" + targetAltitude + "]!!!!!");
                        if (currentAltitude >= targetAltitude)
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_READY);
                            if (m_autoStart)
                            {
                                FlyDrone();
                            }
                        }
                    }
                    break;

                //////////////////////////////////////////////
                case STATE_LANDING:
                    int landingState = m_dronekitAndroid.Call<System.Int32>("getLandingState");
                    if (landingState == COMMAND_STATE_SUCCESS)
                    {
                        if (!m_reportedLanding)
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_LANDING);
                        }
                        m_reportedLanding = true;

                        int stateAndroid = m_dronekitAndroid.Call<System.Int32>("getStateDrone");
                        if (stateAndroid != STATE_LANDING)
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_LANDED);
                            m_reportedToTakeOff = false;
                            m_reportedLanding = false;
                            m_autoStart = false;
                            ChangeState(STATE_CONNECTED);
                        }
                    }
                    break;
                    
                //////////////////////////////////////////////
                case STATE_FLYING:                                        
                    switch (m_iterator)
                    {
                        case 1:
                            if (!m_dronekitAndroid.Call<System.Boolean>("getVehicleMode"))
                            {
                                Debug.LogError("DroneKit::CHANGING DRONE TO MODE GUIDED!!!!!!!!!!!!!!!!!!!!!!");
                                m_dronekitAndroid.Call("guidedModeDrone");
                            }
                            else
                            {
                                m_iterator = 2;
                            }
                            break;

                        case 2:
                            int guidedModeState = m_dronekitAndroid.Call<System.Int32>("getGuidedState");
                            Debug.LogError("DroneKit::CHECKING DRONE MODE GUIDED["+ guidedModeState + "]!!!!!!!!!!!!!!!!!!!!!!");
                            if (guidedModeState != COMMAND_STATE_SUCCESS)
                            {
                                m_iterator = 1;
                            }
                            break;

                        case 3:
                            bool movementAction = m_dronekitAndroid.Call<System.Boolean>("setVelocityDrone", m_currentVelocity.x, m_currentVelocity.y, m_currentVelocity.z, m_speedDrone);
                            if (movementAction)
                            {
                                Debug.LogError("DroneKit::MOVEMENT TO ACTION EXECUTED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                            }
                            else
                            {
                                m_iterator = 2;
                            }
                            break;

                        default:
                            m_timeoutRunning -= Time.deltaTime;
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_FLYING, m_timeoutRunning);
                            if (m_timeoutRunning <= 0)
                            {
                                Debug.LogError("DroneKit::TIME COMPLETED!!!!!");
                                if (m_nextVelocity != Vector3.zero)
                                {
                                    Debug.LogError("DroneKit::SET NEW TARGET STORED!!!!!!!!!!!!!!");
                                    m_currentVelocity = Utilities.Clone(m_nextVelocity);
                                    m_timeoutRunning = m_nextTimeout;
                                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_START_FLYING, m_timeoutRunning);
                                    m_nextVelocity = Vector3.zero;
                                    m_nextTimeout = 0;
                                    ChangeState(STATE_FLYING);
                                }
                                else
                                {
                                    Debug.LogError("DroneKit::SETTING THE DRONE TO IDLE!!!!!!!!!!!!!!");
                                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_READY);
                                    m_dronekitAndroid.Call("pauseDrone");                                    
                                    ChangeState(STATE_IDLE);
                                }
                            }
                            break;
                    }
                    break;
            }
#endif
        }
    }
}