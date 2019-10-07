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

            }
            else
            {

            }
        }

        // -------------------------------------------
        /* 
		 * ProccessErrorMessage
		 */
        private void ProccessErrorMessage(object sender, ErrorEventArgs e)
        {

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
            // BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_ARMED);
        }

        // -------------------------------------------
        /* 
		 * TakeOffDrone
		 */
        public void TakeOffDrone()
        {
            m_cws.Send("takeOffDrone");
            // BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_TAKEN_OFF);
        }

        // -------------------------------------------
        /* 
		 * LandDrone
		 */
        public void LandDrone()
        {
            m_cws.Send("landDrone");
            // BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_TAKEN_OFF);
        }

        // -------------------------------------------
        /* 
		 * RTLDrone
		 */
        public void RTLDrone()
        {
            m_cws.Send("RTLDrone");
            // BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_TAKEN_OFF);
        }

        // -------------------------------------------
        /* 
		 * RTLDrone
		 */
        public void DisarmDrone()
        {
            m_cws.Send("disconnectDrone");
            // BasicSystemEventController.Instance.DispatchBasicSystemEvent(DroneKitAndroidController.EVENT_DRONEKITCONTROLLER_TAKEN_OFF);
        }
#endif
    }
}