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
#if ENABLE_WEBSOCKET_DRONEKIT
        public const string EVENT_WEBSOCKET_REQUESTED_DIRECTION = "EVENT_WEBSOCKET_REQUESTED_DIRECTION";

        public const float TOTAL_TIMEOUT_TO_DIRECTION = 2;

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

        private float m_timeoutToDirectionRoomba = 0;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool TakeoffAltitudeReached
        {
            get { return m_takeoffAltitudeReached; }
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
                string tagDirection = "requestdirection_success_";
                int indexDirection = e.Data.IndexOf(tagDirection);
                if (indexDirection != -1)
                {
                    int numberIndex = indexDirection + tagDirection.Length;
                    float differenceX = float.Parse(e.Data.Substring(numberIndex, e.Data.Length - numberIndex));
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_WEBSOCKET_REQUESTED_DIRECTION, differenceX);
                    Debug.LogError("DIRECTION REQUESTED["+ differenceX + "]++++++++++++++");
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
                if (m_errorProduced)
                {
                    DisarmDrone();
                }
            }
        }

        // -------------------------------------------
        /* 
		 * RoombaTurnLeft
		 */
        public void RoombaTurnLeft(float _time)
        {
            if (m_cws != null) m_cws.Send("turnLeft_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * RoombaTurnRight
		 */
        public void RoombaTurnRight(float _time)
        {
            if (m_cws != null) m_cws.Send("turnRight_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * RoombaMoveForward
		 */
        public void RoombaMoveForward(float _time)
        {
            if (m_cws != null) m_cws.Send("moveForward_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * RoombaMoveForward
		 */
        public void RoombaMoveBackward(float _time)
        {
            if (m_cws != null) m_cws.Send("moveBackward_time_" + _time + "_end");
        }

        // -------------------------------------------
        /* 
		 * RoombaCloseCommunication
		 */
        public void RoombaCloseCommunication()
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
                RoombaRequestDirection();
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