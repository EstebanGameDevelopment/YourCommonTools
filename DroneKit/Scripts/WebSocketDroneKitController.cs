using System;
using System.Text;
using System.Threading;
using UnityEngine;
using YourCommonTools;
#if ENABLE_WEBSOCKET_DRONEKIT
using WebSocketSharp;
#endif

namespace YourCommonTools
{
    public class WebSocketDroneKitController : MonoBehaviour
    {
        enum ROOMBA_STATES { IDLE = 0, TURNING, MOVING};

#if ENABLE_WEBSOCKET_DRONEKIT
        public const string EVENT_WEBSOCKET_REQUESTED_DIRECTION = "EVENT_WEBSOCKET_REQUESTED_DIRECTION";
        public const string EVENT_WEBSOCKET_TARGET_ANGLE_SUCCESS = "EVENT_WEBSOCKET_TARGET_ANGLE_SUCCESS";

        public const float TOTAL_TIMEOUT_TO_DIRECTION = 3f;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static WebSocketDroneKitController _instance;

        public static WebSocketDroneKitController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(WebSocketDroneKitController)) as WebSocketDroneKitController;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        DontDestroyOnLoad(container);
                        container.name = "WebSocketDroneKitController";
                        _instance = container.AddComponent(typeof(WebSocketDroneKitController)) as WebSocketDroneKitController;
                    }
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private Uri m_urlConnectionWebSocket;
        private WebSocket m_cws = null;
        private bool m_errorProduced = false;
        private float m_timerToReturn = 1000;
        private bool m_hasTakenOff = false;
        private bool m_takeoffAltitudeReached = false;

        private Vector3 m_currentVelocity;
        private float m_timeoutFlying = -1;
        private float m_speedDrone = -1;

        private Vector3 m_nextVelocity;
        private float m_nextTimeout = -1;
        private float m_nextSpeedDrone = -1;

        // ROOMBA
        private float m_timeoutToDirectionRoomba = 0;
        private bool m_enableGoToTarget = true;
        private ROOMBA_STATES m_stateRoomba = ROOMBA_STATES.IDLE;
        private bool m_turnDirectionRoomba = false;
        private bool m_runActionRoomba = false;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool TakeoffAltitudeReached
        {
            get { return m_takeoffAltitudeReached; }
        }
        public bool EnableGoToTarget
        {
            get { return m_enableGoToTarget; }
            set {
                m_enableGoToTarget = value;
                if (!m_enableGoToTarget)
                {
                    DroneMoveForward(0);
                }                
            }
        }

        // -------------------------------------------
        /* 
		 * Connect
		 */
        public void Connect(string _url)
        {
            m_urlConnectionWebSocket = new Uri("ws://" + _url);
            try
            {
                Debug.LogError("++WEB SERVER SOCKET [TRYING TO CONNECT("+ m_urlConnectionWebSocket.AbsoluteUri + ").........]++");
                m_cws = new WebSocket(m_urlConnectionWebSocket.AbsoluteUri);
                m_cws.Connect();
                m_cws.OnMessage += ReceivedMessage;
                m_cws.OnError += ProccessErrorMessage;
                m_cws.OnClose += ProccessCloseConnection;
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_CONNECTED);
                BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
            }
            catch (Exception e) { Debug.LogError("WebSharpSocketController::ERROR:: " + e.Message); }
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        public void OnDestroy()
        {
            if (m_cws != null)
            {
                m_cws.OnMessage -= ReceivedMessage;
                m_cws.OnError -= ProccessErrorMessage;
                m_cws.OnClose -= ProccessCloseConnection;
            }
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
            _instance = null;
        }

        // -------------------------------------------
        /* 
		 * ReceivedMessage
		 */
        private void ReceivedMessage(object sender, MessageEventArgs e)
        {
            if (e.IsPing)
            {
                Debug.LogError("++WEB SERVER SOCKET::ReceivedMessage::PING RECEIVED");
            }
            else
            {
#if !ENABLE_ROOMBA
                Debug.LogError("++WEB SERVER SOCKET::ReceivedMessage::DATA RECEIVED["+ e.Data + "]");
                if (e.Data.IndexOf("armed_success") != -1)
                {
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_ARMED, 0.1f);
                }
                if (e.Data.IndexOf("altitude_success") != -1)
                {
                    m_takeoffAltitudeReached = true;
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_TAKEN_OFF, 0.1f);
                }
                if (e.Data.IndexOf("rtl_success") != -1)
                {
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_LANDED, 0.1f);
                }                
                if (e.Data.IndexOf("landed_success") != -1)
                {
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_LANDED, 0.1f);
                }
                if (e.Data.IndexOf("disconnected_success") != -1)
                {
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_DISARMED, 0.1f);
                }
                if (e.Data.IndexOf("opdrone_success") != -1)
                {
                    BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_VEHICLEMODE_CHANGED, 0.1f);
                }
#else
                if (e.Data.IndexOf("turnleft_success") != -1)
                {
                    Debug.LogError("TURN LEFT CONFIRMED BY RASPBERRY++++++++++++++");
                }
                if (e.Data.IndexOf("turnright_success") != -1)
                {
                    Debug.LogError("TURN RIGHT CONFIRMED BY RASPBERRY++++++++++++++");
                }
                if (e.Data.IndexOf("moveforward_success") != -1)
                {
                    Debug.LogError("MOVE FORWARD CONFIRMED BY RASPBERRY++++++++++++++");
                }
                if (e.Data.IndexOf("movebackward_success") != -1)
                {
                    Debug.LogError("MOVE BACKWARD CONFIRMED BY RASPBERRY++++++++++++++");
                }
                if (e.Data.IndexOf("targetangle_success") != -1)
                {
                    Debug.LogError("ANGLE SUCCESS CONFIRMED BY RASPBERRY++++++++++++++");
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_WEBSOCKET_TARGET_ANGLE_SUCCESS);
                }
                string tagDirection = "requestdirection_success_";
                int indexDirection = e.Data.IndexOf(tagDirection);
                if (indexDirection != -1)
                {
                    int numberIndex = indexDirection + tagDirection.Length;
                    float differenceX = float.Parse(e.Data.Substring(numberIndex, e.Data.Length - numberIndex));
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_WEBSOCKET_REQUESTED_DIRECTION, differenceX);
                }
#endif
            }
        }

        // -------------------------------------------
        /* 
		 * ProccessErrorMessage
		 */
        private void ProccessErrorMessage(object sender, ErrorEventArgs e)
        {
            Debug.LogError("++WEB SERVER SOCKET::ProccessErrorMessage::ERROR="+e.Message);
            // BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_ERROR, 0.1f);
        }

        // -------------------------------------------
        /* 
		 * ProccessCloseConnection
		 */
        private void ProccessCloseConnection(object sender, CloseEventArgs e)
        {

        }

        // -------------------------------------------
        /* 
		 * ArmDrone
		 */
        public void ArmDrone()
        {
            if (m_cws != null) m_cws.Send("armDrone");            
        }

        // -------------------------------------------
        /* 
		 * TakeOffDrone
		 */
        public void TakeOffDrone(int _height)
        {
            if (m_cws != null) m_cws.Send("takeOffDrone_"+ _height);
        }

        // -------------------------------------------
        /* 
		 * LandDrone
		 */
        public void LandDrone()
        {
            if (m_cws != null) m_cws.Send("landDrone");
            BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_LANDING, 0.1f);
        }

        // -------------------------------------------
        /* 
		 * RTLDrone
		 */
        public void RTLDrone()
        {
            if (m_cws != null) m_cws.Send("RTLDrone");
            BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_LANDING, 0.1f);
        }

        // -------------------------------------------
        /* 
		 * RTLDrone
		 */
        public void DisarmDrone()
        {
            if (m_cws != null) m_cws.Send("disconnectDrone");
        }

        // -------------------------------------------
        /* 
		 * ChangeAltitude
		 */
        public void ChangeAltitude(float _height)
        {

        }

        // -------------------------------------------
        /* 
		 * Requests the drone with the desired vector velocity
		 */
        public bool RunVelocity(float _vx, float _vy, float _vz, bool _returnHome = false, float _totalTime = 2, float _totalSpeed = 5)
        {
            if (m_timeoutFlying > 0)
            {
                m_nextVelocity = new Vector3(_vx, _vy, _vz);
                m_nextTimeout = _totalTime;
                m_nextSpeedDrone = _totalSpeed;
                return false;
            }
            else
            {
                m_currentVelocity = new Vector3(_vx, _vy, _vz);
                m_timeoutFlying = _totalTime;
                m_speedDrone = _totalSpeed;
                return true;
            }            
        }

        // -------------------------------------------
        /* 
		 * Start the drone with the desired vector velocity
		 */
        private void RunDronekitVelocity()
        {
            // Debug.LogError("VECTOR VELOCITY[" + m_currentVelocity.ToString() + "]::TOTAL TIME[" + m_timeoutFlying + "]::SPEED["+ m_speedDrone + "]");
            if (m_cws != null) m_cws.Send("Velocity_vx_" + m_currentVelocity.x + "_vy_" + m_currentVelocity.y + "_vz_" + m_currentVelocity.z + "_time_" + m_timeoutFlying + "_speed_" + m_speedDrone + "_end");
        }

        // -------------------------------------------
        /* 
        * SetModeOperation
        */
        public void SetModeOperation(int _operationMode)
        {
            if (m_cws != null) m_cws.Send("operationDrone_"+_operationMode);
        }

        // -------------------------------------------
        /* 
		 * OnBasicSystemEvent
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_TAKEN_OFF)
            {
                m_hasTakenOff = true;
            }
            if (_nameEvent == DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_ERROR)
            {
                if (!m_errorProduced)
                {
                    m_errorProduced = true;
                    RTLDrone();
                }                
            }
            if (_nameEvent == DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_LANDED)
            {
                DisarmDrone();
            }
            // ROOMBA
            if (_nameEvent == EVENT_WEBSOCKET_REQUESTED_DIRECTION)
            {
                float distanceFromCenter = (float)_list[0];
                Debug.LogError("DISTANCE FROM CENTER[" + distanceFromCenter + "]++++++++++++++");

                if (Mathf.Abs(distanceFromCenter) > 10)
                {
                    m_turnDirectionRoomba = (distanceFromCenter < 0);
                    m_stateRoomba = ROOMBA_STATES.TURNING;
                }
                else
                {
                    m_stateRoomba = ROOMBA_STATES.MOVING;
                }

                if (m_enableGoToTarget)
                {
                    switch (m_stateRoomba)
                    {
                        case ROOMBA_STATES.IDLE:
                            break;

                        case ROOMBA_STATES.TURNING:
                            if (m_turnDirectionRoomba)
                            {
                                DroneTurnLeft(1);
                            }
                            else
                            {
                                DroneTurnRight(1);
                            }
                            break;

                        case ROOMBA_STATES.MOVING:
                            DroneMoveForward(1);
                            break;
                    }
                }
            }
        }

        // -------------------------------------------
        /* 
		 * DroneTurnLeft
		 */
        public void DroneTurnLeft(float _time)
        {
            if (m_cws != null) m_cws.Send("turnLeft_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * DroneTurnRight
		 */
        public void DroneTurnRight(float _time)
        {
            if (m_cws != null) m_cws.Send("turnRight_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * DroneMoveForward
		 */
        public void DroneMoveForward(float _time)
        {
            if (m_cws != null) m_cws.Send("moveForward_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * DroneMoveBackward
		 */
        public void DroneMoveBackward(float _time)
        {
            if (m_cws != null) m_cws.Send("moveBackward_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * DroneMoveAscend
		 */
        public void DroneMoveAscend(float _time)
        {
            if (m_cws != null) m_cws.Send("moveAscend_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * DroneMoveDescend
		 */
        public void DroneMoveDescend(float _time)
        {
            if (m_cws != null) m_cws.Send("moveDescend_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * DroneActionTakeOff
		 */
        public void DroneActionTakeOff(float _height)
        {
            if (m_cws != null) m_cws.Send("actionTakeOff_height_" + _height + "_end");
        }

        // -------------------------------------------
        /* 
		 * DroneActionLand
		 */
        public void DroneActionLand()
        {
            if (m_cws != null) m_cws.Send("actionLand");
        }

        // -------------------------------------------
        /* 
		 * GenericAction
		 */
        public void GenericAction()
        {
            if (m_cws != null) m_cws.Send("genericAction");
        }

        // -------------------------------------------
        /* 
		 * StopAction
		 */
        public void StopAction()
        {
            if (m_cws != null) m_cws.Send("stopAction");
        }

        // -------------------------------------------
        /* 
		 * RoombaCloseCommunication
		 */
        public void DroneCloseCommunication()
        {
            if (m_cws != null) m_cws.Send("closeCommunication");
        }

        // -------------------------------------------
        /* 
		 * RoombaTurnRight
		 */
        public void RoombaRequestDirection()
        {
            if (m_cws != null) m_cws.Send("requestDirection");
        }

        // -------------------------------------------
        /* 
		 * SetTargetAngle
		 */
        public void SetTargetAngle(float _angleDeg)
        {
            if (m_cws != null) m_cws.Send("setTarget_angle_" + _angleDeg + "_end");
        }

        // -------------------------------------------
        /* 
		 * SetTargetGPS
		 */
        public void SetTargetGPS(float _angleDeg, double _latitude, double _longitude)
        {
            if (m_cws != null) m_cws.Send("setTarget_gps_" + _angleDeg + "," + _latitude + "," + _longitude + "_end");
        }

        // -------------------------------------------
        /* 
		 * GoToGPSCoordinates
		 */
        public void GoToGPSCoordinates(double _latitude, double _longitude)
        {
            if (m_cws != null) m_cws.Send("goTo_GPS_" + _latitude + "," + _longitude + "_end");
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        void Update()
        {
#if UNITY_EDITOR
            m_hasTakenOff = true;
#endif

#if ENABLE_ROOMBA
            m_timeoutToDirectionRoomba += Time.deltaTime;
            if (m_timeoutToDirectionRoomba > TOTAL_TIMEOUT_TO_DIRECTION)
            {
                m_timeoutToDirectionRoomba = 0;
                // RoombaRequestDirection();
            }
#else
            if (m_hasTakenOff)
            {
                // FALLBACK PROTOCAL TO RETURN HONE
                if (m_timerToReturn > 0)
                {
                    m_timerToReturn -= Time.deltaTime;
                    if (m_timerToReturn <= 0)
                    {
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_ERROR);
                        return;
                    }
                }

                // FLY VECTOR VELOCITY
                if (m_timeoutFlying > 0)
                {
                    RunDronekitVelocity();
                    m_timeoutFlying -= Time.deltaTime;
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_FLYING, m_timeoutFlying);
                    if (m_timeoutFlying <= 0)
                    {
                        m_currentVelocity = Vector3.zero;
                        m_speedDrone = -1;
                        if (m_nextVelocity != Vector3.zero)
                        {
                            m_timeoutFlying = m_nextTimeout;
                            m_speedDrone = m_nextSpeedDrone;
                            m_currentVelocity = Utilities.Clone(m_nextVelocity);
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_START_FLYING, m_timeoutFlying, m_currentVelocity);
                            m_nextVelocity = Vector3.zero;
                            m_nextTimeout = -1;
                            m_nextSpeedDrone = -1;
                        }
                        else
                        {
                            BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_READY);
                        }
                    }
                }
            }
#endif

        }

#endif
    }
        }