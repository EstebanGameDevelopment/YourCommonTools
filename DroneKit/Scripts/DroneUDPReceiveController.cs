using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace YourCommonTools
{
    public class DroneUDPReceiveController : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_DRONEUDPSOCKET_RECEIVED_MESSAGE = "EVENT_DRONEUDPSOCKET_RECEIVED_MESSAGE";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static DroneUDPReceiveController _instance;

        public static DroneUDPReceiveController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(DroneUDPReceiveController)) as DroneUDPReceiveController;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        DontDestroyOnLoad(container);
                        container.name = "DroneUDPReceiveController";
                        _instance = container.AddComponent(typeof(DroneUDPReceiveController)) as DroneUDPReceiveController;
                    }
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private UdpClient m_client;

        // -------------------------------------------
        /* 
		 * Init
		 */
        public void Init(int _port)
        {
            m_client = new UdpClient(_port);
        }

        // -------------------------------------------
        /* 
		 * ReceiveData
		 */
        private void ReceiveData()
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = m_client.Receive(ref anyIP);

                string text = Encoding.UTF8.GetString(data);

                // Den abgerufenen Text anzeigen.
                Debug.LogError("DroneUDPReceiveController::ReceiveData::TEXT=" + text);

                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_DRONEUDPSOCKET_RECEIVED_MESSAGE, text);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        private void Update()
        {
            ReceiveData();
        }
    }
}