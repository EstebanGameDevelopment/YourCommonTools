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
            m_cws.OnMessage -= ReceivedMessage;
            m_cws.OnError -= ProccessErrorMessage;
            m_cws.OnClose -= ProccessCloseConnection;
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
            m_cws.Send("armDrone");            
        }

        // -------------------------------------------
        /* 
		 * TakeOffDrone
		 */
        public void TakeOffDrone(int _height)
        {
            m_cws.Send("takeOffDrone_"+ _height);
        }

        // -------------------------------------------
        /* 
		 * LandDrone
		 */
        public void LandDrone()
        {
            m_cws.Send("landDrone");
            BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_LANDING, 0.1f);
        }

        // -------------------------------------------
        /* 
		 * RTLDrone
		 */
        public void RTLDrone()
        {
            m_cws.Send("RTLDrone");
            BasicSystemEventController.Instance.DelayBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_LANDING, 0.1f);
        }

        // -------------------------------------------
        /* 
		 * RTLDrone
		 */
        public void DisarmDrone()
        {
            m_cws.Send("disconnectDrone");
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
		 * Start the drone with the desired vector velocity
		 */
        public bool RunVelocity(float _vx, float _vy, float _vz, bool _returnHome = false, float _totalTime = 2, float _totalSpeed = 5)
        {
            m_cws.Send("Velocity_vx_"+_vx+"_vy_"+_vy+"_vz_"+_vz+"_time_"+ _totalTime+ "_speed_" + _totalSpeed + "_end");
            return true;
        }

        // -------------------------------------------
        /* 
        * SetModeOperation
        */
        public void SetModeOperation(string _operationMode)
        {
            m_cws.Send("operationDrone_"+_operationMode);
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
		 * Update
		 */
        void Update()
        {
            if (m_hasTakenOff)
            {
                if (m_timerToReturn > 0)
                {
                    m_timerToReturn -= Time.deltaTime;
                    if (m_timerToReturn <= 0)
                    {
                        BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_ERROR);
                    }
                }
            }
        }

#endif
    }
}