using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
#if !UNITY_WEBGL
using System.Net;
using System.IO;
#endif

namespace YourCommonTools
{
    public class CheckInternetAccess : MonoBehaviour
    {
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------	
        public const string EVENT_INTERNET_ACCESS_STATE = "EVENT_INTERNET_ACCESS_STATE";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static CheckInternetAccess _instance;

        public static CheckInternetAccess Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(CheckInternetAccess)) as CheckInternetAccess;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        container.name = "CheckInternetAccess";
                        _instance = container.AddComponent(typeof(CheckInternetAccess)) as CheckInternetAccess;
                    }
                }
                return _instance;
            }
        }

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private float m_timeoutToCheck = -1;
        private float m_timeoutLimit = 10;
        private bool m_isThereInternetConnection = true;

        // ----------------------------------------------
        // GETTERS/SETTERS
        // ----------------------------------------------	
        public bool IsThereInternetConnection
        {
            get { return m_isThereInternetConnection; }
        }
        public float TimeoutToCheck
        {
            set { m_timeoutToCheck = value; }
        }
        public float TimeoutLimit
        {
            get { return m_timeoutLimit; }
            set { m_timeoutLimit = value; }
        }

        // -------------------------------------------
        /* 
        * Update
        */
        void Update()
        {
            if (m_timeoutToCheck > 0)
            {
                m_timeoutToCheck -= Time.deltaTime;
                if (m_timeoutToCheck <= 0)
                {
                    RequestConnection();
                }
            }
        }

        // -------------------------------------------
        /* 
		 * RequestConnection
		 */
        public void RequestConnection(float _timeout = -1)
        {
            m_timeoutToCheck = _timeout;
#if !UNITY_WEBGL
            StartCoroutine(CheckGoogle());
#else
            BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_INTERNET_ACCESS_STATE, 0.1f, true);
#endif
        }

        // -------------------------------------------
        /* 
        * CheckGoogle
        */
        IEnumerator CheckGoogle()
        {
            string HtmlText = GetHtmlFromUri("http://google.com");
            yield return new WaitForSeconds(1);
            if (HtmlText == "")
            {
                //No connection
                m_isThereInternetConnection = false;
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_INTERNET_ACCESS_STATE, false);
            }
            else if (!HtmlText.Contains("schema.org/WebPage"))
            {
                //Redirecting since the beginning of googles html contains that 
                //phrase and it was not found
                m_isThereInternetConnection = false;
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_INTERNET_ACCESS_STATE, false);
            }
            else
            {
                //success
                m_isThereInternetConnection = true;
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_INTERNET_ACCESS_STATE, true);
            }
        }

        // -------------------------------------------
        /* 
		 * GetHtmlFromUri
		 */
        public string GetHtmlFromUri(string resource)
        {
#if UNITY_WEBGL
            return "";
#else
            string html = string.Empty;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                    if (isSuccess)
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            //We are limiting the array to 80 so we don't have
                            //to parse the entire html document feel free to 
                            //adjust (probably stay under 300)
                            char[] cs = new char[80];
                            reader.Read(cs, 0, cs.Length);
                            foreach (char ch in cs)
                            {
                                html += ch;
                            }
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return html;
#endif
        }
    }
}