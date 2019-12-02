using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace YourCommonTools
{

    public class DroneUDPSendController : MonoBehaviour
    {
        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static DroneUDPSendController _instance;

        public static DroneUDPSendController Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(DroneUDPSendController)) as DroneUDPSendController;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        DontDestroyOnLoad(container);
                        container.name = "DroneUDPSendController";
                        _instance = container.AddComponent(typeof(DroneUDPSendController)) as DroneUDPSendController;
                    }
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private IPEndPoint m_remoteEndPoint;
        private UdpClient m_client;

        // -------------------------------------------
        /* 
		 * Init
		 */
        public void Init(string _ip, int _port)
        {
            m_remoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
            m_client = new UdpClient();

            // status
            Debug.LogError("DroneUDPSendController::Init::Sending to " + _ip + " : " + _port);
        }

        // -------------------------------------------
        /* 
		 * SendString
		 */
        private void SendString(string _message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(_message);
                m_client.Send(data, data.Length, m_remoteEndPoint);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }
}