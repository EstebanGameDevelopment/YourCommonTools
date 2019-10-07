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
        public const string EVENT_DRONEKITCONTROLLER_DISCONNECTED       = "EVENT_DRONEKITCONTROLLER_DISCONNECTED";
        public const string EVENT_DRONEKITCONTROLLER_ARMED              = "EVENT_DRONEKITCONTROLLER_ARMED";
        public const string EVENT_DRONEKITCONTROLLER_DISARMED           = "EVENT_DRONEKITCONTROLLER_DISARMED";
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
        public const int STATE_CHANGE_ALTITUDE = 9;
        public const int STATE_GUIDE_MODE   = 10;

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
        private AndroidJavaObject m_dronekitAndroid;
        private float m_timeoutRunning = 0;
        private float m_nextTimeout = 0;
        private int m_stateAndroid = -1;
        private Vector3 m_currentVelocity = Vector3.zero;
        private Vector3 m_nextVelocity = Vector3.zero;
        private float m_nextSpeedDrone = 5;
        private float m_distance = 2;
        private float m_speedDrone = 5;
        private float m_heightTakeOff = 10;
        private float m_initialAltitude = -1;
        private bool m_reportedToTakeOff = false;
        private bool m_reportedLanding = false;
        private bool m_takeoffAltitudeReached = false;
        private bool m_autoStart = false;
        private int m_nextState = -1;
        private bool m_activatedLanding = false;
        private bool m_gotoActivated = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------
        public float TimeoutRunning
        {
            get { return m_timeoutRunning; }
        }
        public bool TakeoffAltitudeReached
        {
            get { return m_takeoffAltitudeReached; }
        }

        // -------------------------------------------
        /* 
		 * Drone set up call
		 */
        public void Initialitzation(bool _autoStart = false, string _ipAddressDrone = "localhost", int _portNumberDrone = 14550, float _heightTakeOff = 5)
        {
#if ENABLE_DRONEANDROIDCONTROLLER
            if (m_dronekitAndroid == null)
            {
                m_heightTakeOff = _heightTakeOff;
                m_autoStart = _autoStart;

                m_state = STATE_DISCONNECTED;

                m_dronekitAndroid = new AndroidJavaObject("com.yourvrexperience.controldrone.ControlDroneDronekit");
                m_dronekitAndroid.Call("initControlDrone", _ipAddressDrone, _portNumberDrone, _heightTakeOff);

                Invoke("ConnectDrone", 3);
            }
#endif
        }

        // -------------------------------------------
        /* 
        * Start the drone with the desired vector velocity
        */
        public bool RunVelocity(float _vx, float _vy, float _vz, bool _autoStart = false, float _timeoutRunning = 5, float _speedDrone = 5)
		{
            // IF IT'S IN STATE GO TO, THE PAUSE THE DRONE BEFORE NEW VELOCITY
            if (m_gotoActivated)
            {
                m_gotoActivated = false;
                PauseDrone();
            }

            m_autoStart = _autoStart;

            if (m_timeoutRunning > 0)
            {
                m_nextTimeout = _timeoutRunning;
                m_nextSpeedDrone = _speedDrone;
                m_nextVelocity = new Vector3(_vx, _vy, _vz) * m_nextSpeedDrone;
                return false;
            }

#if ENABLE_DRONEANDROIDCONTROLLER
            m_timeoutRunning = _timeoutRunning;
            m_speedDrone = _speedDrone;
            m_currentVelocity = new Vector3(_vx, _vy, _vz) * m_speedDrone;
            if (m_dronekitAndroid != null)
            {
                if ((m_state != STATE_IDLE) && (m_state != STATE_FLYING) && (m_state != STATE_LANDING))
                {
                    m_stateAndroid = m_dronekitAndroid.Call<System.Int32>("getStateDrone");
                    if (m_autoStart)
                    {
                        switch (m_stateAndroid)
                        {
                            case STATE_DISCONNECTED:
                                ConnectDrone();
                                break;

                            case STATE_CONNECTED:
                                ArmDrone();
                                break;

                            case STATE_ARMED:
                                TakeOffDrone();
                                break;

                            default:
                                FlyDrone();
                                break;
                        }
                    }
                }
                else
                {
                    if (m_autoStart)
                    {
                        FlyDrone();
                    }
                }
            }            
#endif
            return true;
        }

        // -------------------------------------------
        /* 
		 * ConnectDrone
		 */
        public void ConnectDrone()
        {
            if (m_dronekitAndroid == null) return;
            if (m_state != STATE_DISCONNECTED) return;

            ChangeState(STATE_CONNECTED);
        }

        // -------------------------------------------
        /* 
		 * ArmDrone
		 */
        public void ArmDrone()
        {
            if (m_dronekitAndroid == null) return;
            if (m_state != STATE_CONNECTED) return;

            ChangeState(STATE_ARMED);
        }

        // -------------------------------------------
        /* 
		 * TakeOffDrone
		 */
        public void TakeOffDrone()
        {
            if (m_dronekitAndroid == null) return;
            if (m_state != STATE_ARMED) return;

            ChangeState(STATE_TAKEOFF);
        }

        // -------------------------------------------
        /* 
		 * LandDrone
		 */
        public void LandDrone()
        {
            if (m_dronekitAndroid == null) return;
            if ((m_state != STATE_FLYING) && (m_state != STATE_IDLE) && (m_state != STATE_TAKEOFF)) return;

            ChangeState(STATE_LANDING);
        }

        // -------------------------------------------
        /* 
		 * RTLDrone
		 */
        public void RTLDrone()
        {
            if (m_dronekitAndroid == null) return;
            if ((m_state != STATE_FLYING) && (m_state != STATE_IDLE) && (m_state != STATE_TAKEOFF)) return;

            SetModeOperation(MODE_COPTER_RTL);
        }

        // -------------------------------------------
        /* 
		 * GotoDrone
		 */
        public void GotoDrone(float _latitude, float _longitude, float _speed = 5)
        {
            if (m_dronekitAndroid == null) return;
            if ((m_state != STATE_FLYING) && (m_state != STATE_IDLE) && (m_state != STATE_TAKEOFF)) return;

            m_dronekitAndroid.Call<System.Boolean>("setGoToDrone", _latitude, _longitude);

            m_gotoActivated = true;
        }

        // -------------------------------------------
        /* 
		 * PauseDrone
		 */
        public void PauseDrone()
        {
            if (!m_gotoActivated)
            {
                ChangeState(STATE_IDLE);
                m_dronekitAndroid.Call("pauseDrone");
            }
        }

        // -------------------------------------------
        /* 
		 * SetModeOperation
		 */
        public void SetModeOperation(int _modeOperation)
        {
            ChangeState(STATE_IDLE);
            m_dronekitAndroid.Call("setModeDrone", _modeOperation);
        }

        // -------------------------------------------
        /* 
		 * ChangeAltitude
		 */
        public void ChangeAltitude(float _height)
        {
            if (m_dronekitAndroid == null) return;
            if ((m_state != STATE_FLYING) && (m_state != STATE_IDLE) && (m_state != STATE_TAKEOFF)) return;

            m_heightTakeOff = _height;
            ChangeState(STATE_CHANGE_ALTITUDE);
        }

        // -------------------------------------------
        /* 
		 * GetVehicleMode
		 */
        public int GetVehicleMode()
        {
            return m_dronekitAndroid.Call<System.Int32>("getVehicleMode");
        }

        // -------------------------------------------
        /* 
		 * GetGPSCoordinates
		 */
        public Vector2 GetGPSCoordinates()
        {
            float latitude = (float)m_dronekitAndroid.Call<System.Double>("getLatitude");
            float longitude = (float)m_dronekitAndroid.Call<System.Double>("getLongitude");
            return new Vector2(latitude, longitude);
        }

        // -------------------------------------------
        /* 
		 * FlyDrone
		 */
        public void FlyDrone()
        {
            if (m_dronekitAndroid == null) return;
            if (((m_state != STATE_TAKEOFF) && (m_state != STATE_IDLE) && (m_state != STATE_FLYING)) || !m_takeoffAltitudeReached) return;

            if (m_timeoutRunning > 0)
            {
                if (m_currentVelocity != Vector3.zero)
                {
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_START_FLYING, m_timeoutRunning, m_currentVelocity);
                    ChangeState(STATE_FLYING);
                }
            }
            else
            {
                ChangeState(STATE_IDLE);
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
                    if (m_dronekitAndroid.Call<System.Boolean>("getArmedDrone"))
                    {
                        if (m_autoStart)
                        {
                            Invoke("FlyDrone", 3);
                        }
                        else
                        {
                            m_reportedToTakeOff = true;
                            m_takeoffAltitudeReached = true;
                            m_activatedLanding = false;
                            m_reportedLanding = false;
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_READY);
                            ChangeState(STATE_IDLE);
                        }
                    }
                    else
                    {
                        resultAction = m_dronekitAndroid.Call<System.Boolean>("armDrone");
                        Debug.LogError("DroneKit::ARMING DRONE[" + resultAction + "]+++++++++++++");
                        if (resultAction)
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_ARMED);
                            m_initialAltitude = (float)m_dronekitAndroid.Call<System.Double>("getAltitude");
                            Debug.LogError("DroneKit::DRONE INITIAL ALTITUDE[" + m_initialAltitude + "]+++++++++++++");
                            if (m_autoStart) Invoke("TakeOffDrone", 3);
                        }
                    }
                    break;

                case STATE_TAKEOFF:
                    m_reportedToTakeOff = false;
                    m_takeoffAltitudeReached = false;
                    m_activatedLanding = false;
                    m_reportedLanding = false;
                    resultAction = m_dronekitAndroid.Call<System.Boolean>("takeOffDrone");                    
                    Debug.LogError("DroneKit::TAKING OFF DRONE[" + resultAction + "]+++++++++++++");
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
                case STATE_ARMED:
                    m_timeAcum += Time.deltaTime;
                    if (m_timeAcum > 3)
                    {
                        m_stateAndroid = m_dronekitAndroid.Call<System.Int32>("getStateDrone");
                        if (m_stateAndroid != STATE_ARMED)
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_CONNECTED, false);
                            m_state = STATE_CONNECTED;
                        }
                    }
                    break;

                //////////////////////////////////////////////
                case STATE_TAKEOFF:
                    int takeOffState = m_dronekitAndroid.Call<System.Int32>("getTakeOffState");
                    if (takeOffState == COMMAND_STATE_SUCCESS)
                    {
                        if (!m_reportedToTakeOff)
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_TAKEN_OFF);
                        }
                        m_reportedToTakeOff = true;
                        float currentAltitude = (float)m_dronekitAndroid.Call<System.Double>("getAltitude");
                        float targetAltitude = (m_initialAltitude + m_heightTakeOff) - 1;
                        if (currentAltitude >= targetAltitude)
                        {
                            m_takeoffAltitudeReached = true;
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_READY);
                            if (m_autoStart)
                            {
                                FlyDrone();
                            }
                            else
                            {
                                ChangeState(STATE_IDLE);
                            }
                        }
                    }
                    break;

                //////////////////////////////////////////////
                case STATE_LANDING:
                    if (!m_dronekitAndroid.Call<System.Boolean>("getGuidedDrone") && !m_activatedLanding)
                    {
                        m_nextState = STATE_LANDING;
                        ChangeState(STATE_GUIDE_MODE);
                    }
                    else
                    {
                        m_activatedLanding = true;
                        if (m_iterator == 1)
                        {
                            m_takeoffAltitudeReached = false;
                            m_dronekitAndroid.Call("landModeDrone");
                        }
                        else
                        {
                            int landingState = m_dronekitAndroid.Call<System.Int32>("getLandingState");
                            if (landingState == COMMAND_STATE_SUCCESS)
                            {
                                if (!m_reportedLanding)
                                {
                                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_LANDING);
                                }
                                m_reportedLanding = true;

                                if (!m_dronekitAndroid.Call<System.Boolean>("getArmedDrone"))
                                {
                                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_LANDED);
                                    m_reportedToTakeOff = false;
                                    m_reportedLanding = false;
                                    m_autoStart = false;
                                    m_activatedLanding = false;
                                    ChangeState(STATE_CONNECTED);
                                }
                            }
                        }
                    }
                    break;

                //////////////////////////////////////////////
                case STATE_CHANGE_ALTITUDE:
                    if (!m_dronekitAndroid.Call<System.Boolean>("getGuidedDrone"))
                    {
                        m_nextState = STATE_CHANGE_ALTITUDE;
                        ChangeState(STATE_GUIDE_MODE);
                    }
                    else
                    {
                        if (m_iterator == 1)
                        {
                            m_dronekitAndroid.Call("changeAltitudeDrone", m_heightTakeOff);
                        }
                        else
                        {
                            ChangeState(STATE_IDLE);
                        }
                    }
                    break;

                //////////////////////////////////////////////
                case STATE_FLYING:                                        
                    if (!m_dronekitAndroid.Call<System.Boolean>("getGuidedDrone"))
                    {
                        m_nextState = STATE_FLYING;
                        ChangeState(STATE_GUIDE_MODE);
                    }
                    else
                    {
                        if (m_dronekitAndroid.Call<System.Boolean>("setVelocityDrone", m_currentVelocity.x, -m_currentVelocity.y, m_currentVelocity.z, m_speedDrone))
                        {
                            m_timeoutRunning -= Time.deltaTime;
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_FLYING, m_timeoutRunning);
                            if (m_timeoutRunning <= 0)
                            {
                                if (m_nextVelocity != Vector3.zero)
                                {
                                    m_timeoutRunning = m_nextTimeout;
                                    m_speedDrone = m_nextSpeedDrone;
                                    m_currentVelocity = Utilities.Clone(m_nextVelocity);
                                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_START_FLYING, m_timeoutRunning, m_currentVelocity);
                                    m_nextVelocity = Vector3.zero;
                                    m_nextTimeout = 0;
                                    m_nextSpeedDrone = 0;
                                    ChangeState(STATE_FLYING);
                                }
                                else
                                {
                                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEKITCONTROLLER_READY);
                                    m_dronekitAndroid.Call("pauseDrone");
                                    ChangeState(STATE_IDLE);
                                }
                            }
                        }
                    }
                    break;

                //////////////////////////////////////////////
                case STATE_GUIDE_MODE:
                    switch (m_iterator)
                    {
                        case 1:
                            if (!m_dronekitAndroid.Call<System.Boolean>("getGuidedDrone"))
                            {
                                m_dronekitAndroid.Call("guidedModeDrone");
                            }
                            else
                            {
                                m_iterator = 2;
                                ChangeState(m_nextState);
                                m_nextState = -1;
                            }
                            break;

                        case 2:
                            int guidedModeState = m_dronekitAndroid.Call<System.Int32>("getGuidedState");
                            if (guidedModeState != COMMAND_STATE_SUCCESS)
                            {
                                m_iterator = 1;
                                m_timeAcum += Time.deltaTime;
                                if (m_timeAcum > 2)
                                {
                                    m_timeAcum = 0;
                                    m_iterator = 0;
                                }
                            }
                            else
                            {
                                ChangeState(m_nextState);
                                m_nextState = -1;
                            }
                            break;
                    }
                    break;
            }
#endif
        }
    }
}