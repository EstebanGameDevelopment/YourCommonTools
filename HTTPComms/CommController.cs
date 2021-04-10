using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * CommController
	 * 
	 * It manages all the communications with the server
	 * 
	 * @author Esteban Gallardo
	 */
	public class CommController : StateManager
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const char TOKEN_SEPARATOR_COMA = ',';
        public const string TOKEN_SEPARATOR_BLOCKS = "<block>";
        public const string TOKEN_SEPARATOR_EVENTS = "<par>";
		public const string TOKEN_SEPARATOR_LINES = "<line>";
        public const string TOKEN_SEPARATOR_USER_DATA = "<udata>";

        public const bool DEBUG_LOG = false;

		public const int STATE_IDLE = 0;
		public const int STATE_COMMUNICATION = 1;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	

		private static CommController _instance;

		public static CommController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(CommController)) as CommController;
					if (!_instance)
					{
						GameObject container = new GameObject();
                        DontDestroyOnLoad(container);
                        container.name = "CommController";
						_instance = container.AddComponent(typeof(CommController)) as CommController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
		private string m_event;
		private IHTTPComms m_commRequest;
		private List<CommEventData> m_listTimedEvents = new List<CommEventData>();
		private List<CommEventData> m_listQueuedEvents = new List<CommEventData>();
		private List<CommEventData> m_priorityQueuedEvents = new List<CommEventData>();

		private string m_inGameLog = "";

		public bool ReloadXML = false;

		// -------------------------------------------
		/* 
		 * Will delete from the text introduced by the user any special token that can break the comunication
		 */
		public static string FilterSpecialTokens(string _text)
		{
			string output = _text;

			string[] arrayEvents = output.Split(new string[] { TOKEN_SEPARATOR_EVENTS }, StringSplitOptions.None);
			output = "";
			for (int i = 0; i < arrayEvents.Length; i++)
			{
				output += arrayEvents[i];
				if (i + 1 < arrayEvents.Length)
				{
					output += " ";
				}
			}


			string[] arrayLines = output.Split(new string[] { TOKEN_SEPARATOR_LINES }, StringSplitOptions.None);
			output = "";
			for (int i = 0; i < arrayLines.Length; i++)
			{
				output += arrayLines[i];
				if (i + 1 < arrayLines.Length)
				{
					output += " ";
				}
			}

			return output;
		}

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		private CommController()
		{
			ChangeState(STATE_IDLE);
		}

		// -------------------------------------------
		/* 
		 * Init
		 */
		public void Init()
		{
			ChangeState(STATE_IDLE);
		}

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        void OnDestroy()
        {
#if DESTROY_MEMORY_ALLOCATED
            Destroy();
#endif            
        }


        // -------------------------------------------
        /* 
		 * Destroy
		 */
        public void Destroy()
		{
			if (_instance != null)
			{
				Destroy(_instance.gameObject);
				_instance = null;
			}
		}

        // -------------------------------------------
        /* 
		 * RequestHeader
		 */
        public void RequestHeader(string _event, List<ItemMultiTextEntry> _headers, bool _isBinaryResponse, params object[] _list)
        {
            if (m_state != STATE_IDLE)
            {
                QueuedRequest(_event, _headers, _isBinaryResponse, _list);
                return;
            }

            RequestReal(_event, _headers, _isBinaryResponse, _list);
        }


        // -------------------------------------------
        /* 
		 * Request
		 */
        public void Request(string _event, bool _isBinaryResponse, params object[] _list)
		{
            if (m_state != STATE_IDLE)
			{
				QueuedRequest(_event, _isBinaryResponse, _list);
				return;
			}

            RequestReal(_event, null, _isBinaryResponse, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestPriority
		 */
		public void RequestPriority(string _event, bool _isBinaryResponse, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				InsertRequest(_event, _isBinaryResponse, _list);
				return;
			}

			RequestReal(_event, null, _isBinaryResponse, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestNoQueue
		 */
		public void RequestNoQueue(string _event, bool _isBinaryResponse, params object[] _list)
		{
			if (m_state != STATE_IDLE)
			{
				return;
			}

			RequestReal(_event, null, _isBinaryResponse, _list);
		}

		// -------------------------------------------
		/* 
		 * RequestReal
		 */
		private void RequestReal(string _event, List<ItemMultiTextEntry> _headers, bool _isBinaryResponse, params object[] _list)
		{
            m_event = _event;
			m_commRequest = (IHTTPComms)Activator.CreateInstance(Type.GetType(m_event));

			ChangeState(STATE_COMMUNICATION);
			string data = m_commRequest.Build(_list);
			if (DEBUG_LOG)
			{
                Utilities.DebugLogWarning("CommController::RequestReal:URL=" + m_commRequest.UrlRequest);
                Utilities.DebugLogWarning("CommController::RequestReal:data=" + data);
			}
			if (m_commRequest.Method == BaseDataHTTP.METHOD_GET)
			{
                UnityWebRequest www = UnityWebRequest.Get(m_commRequest.UrlRequest + data);
                if (_headers != null)
                {
                    for (int i = 0; i < _headers.Count; i++)
                    {
                        if (_headers[i].Items.Count > 1)
                        {
                            www.SetRequestHeader(_headers[i].Items[0], _headers[i].Items[1]);
                        }
                    }
                }
                

                if (_isBinaryResponse)
				{
					StartCoroutine(WaitForUnityWebRequest(www));
				}
				else
				{
					StartCoroutine(WaitForUnityWebStringRequest(www));
				}
			}
			else
			{
                UnityWebRequest www = UnityWebRequest.Post(m_commRequest.UrlRequest, m_commRequest.FormPost);

                if (_isBinaryResponse)
				{
                    StartCoroutine(WaitForUnityWebRequest(www));
                }
				else
				{
                    StartCoroutine(WaitForUnityWebStringRequest(www));
                }
			}
		}

		// -------------------------------------------
		/* 
		 * DelayRequest
		 */
		public void DelayRequest(string _nameEvent, bool _isBinaryResponse, float _time, params object[] _list)
		{
			m_listTimedEvents.Add(new CommEventData(_nameEvent, null, _isBinaryResponse, _time, _list));
		}

		// -------------------------------------------
		/* 
		 * QueuedRequest
		 */
		public void QueuedRequest(string _nameEvent, bool _isBinaryResponse, params object[] _list)
		{
            m_listQueuedEvents.Add(new CommEventData(_nameEvent, null, _isBinaryResponse, 0, _list));
		}

        // -------------------------------------------
        /* 
		 * QueuedRequest
		 */
        public void QueuedRequest(string _nameEvent, List<ItemMultiTextEntry> _headers, bool _isBinaryResponse, params object[] _list)
        {
            m_listQueuedEvents.Add(new CommEventData(_nameEvent, _headers, _isBinaryResponse, 0, _list));
        }

        // -------------------------------------------
        /* 
		 * InsertRequest
		 */
        public void InsertRequest(string _nameEvent, bool _isBinaryResponse, params object[] _list)
		{
			m_priorityQueuedEvents.Insert(0, new CommEventData(_nameEvent, null, _isBinaryResponse, 0, _list));
		}

		// -------------------------------------------
		/* 
		* WaitForRequest
		*/
		IEnumerator WaitForRequest(WWW www)
		{
			yield return www;

            // check for errors
            try
            {
			    if (www.error == null)
			    {
				    if (DEBUG_LOG) Debug.Log("WWW Ok!: " + www.text);
				    m_commRequest.Response(www.bytes);
			    }
			    else
			    {
				    if (DEBUG_LOG) Debug.LogError("WWW Error: " + www.error);
				    m_commRequest.Response(Encoding.ASCII.GetBytes(www.error));
			    }
            } catch (Exception err)
            {
                if (DEBUG_LOG) Utilities.DebugLogError("CommController::WaitForRequest::err=" + err.Message);
            }

            ChangeState(STATE_IDLE);
			ProcesQueuedComms();
		}

        // -------------------------------------------
        /* 
		* WaitForUnityWebRequest
		*/
        IEnumerator WaitForUnityWebRequest(UnityWebRequest www)
        {
            yield return www.SendWebRequest();

            try { 
                if (www.isNetworkError || www.isHttpError)
                {
                    if (DEBUG_LOG) Debug.LogError("WWW Error: " + www.error);
                    m_commRequest.Response(Encoding.ASCII.GetBytes(www.error));
                }
                else
                {
                    if (DEBUG_LOG) Debug.Log("WWW Ok!: " + www.downloadHandler.text);
                    m_commRequest.Response(www.downloadHandler.data);
                }
            } catch (Exception err)
            {
                if (DEBUG_LOG) Utilities.DebugLogError("CommController::WaitForUnityWebRequest::err=" + err.Message);
            }

            ChangeState(STATE_IDLE);
            ProcesQueuedComms();
        }
        

        // -------------------------------------------
        /* 
		* WaitForRequest
		*/
        IEnumerator WaitForStringRequest(WWW www)
		{
			yield return www;

            // check for errors
            try
            {
                if (www.error == null)
                {
                    if (DEBUG_LOG) Debug.Log("WWW Ok!: " + www.text);
                    m_commRequest.Response(www.text);
                }
                else
                {
                    if (DEBUG_LOG) Debug.LogError("WWW Error: " + www.error);
                    m_commRequest.Response(Encoding.ASCII.GetBytes(www.error));
                }
            } catch (Exception err)
            {
                if (DEBUG_LOG) Utilities.DebugLogError("CommController::WaitForStringRequest::err=" + err.Message);
            }

            ChangeState(STATE_IDLE);
			ProcesQueuedComms();
		}

        // -------------------------------------------
        /* 
		* WaitForUnityWebStringRequest
		*/
        IEnumerator WaitForUnityWebStringRequest(UnityWebRequest www)
        {
            yield return www.SendWebRequest();

            try
            { 
                if (www.isNetworkError || www.isHttpError)
                {
                    if (DEBUG_LOG) Debug.LogError("WWW Error: " + www.error);
                    m_commRequest.Response(www.error);
                }
                else
                {
                    if (DEBUG_LOG) Debug.Log("WWW Ok!: " + www.downloadHandler.text);
                    m_commRequest.Response(www.downloadHandler.text);
                }
            } catch (Exception err)
            {
                if (DEBUG_LOG) Utilities.DebugLogError("CommController::WaitForUnityWebStringRequest::err=" + err.Message);
            }

            ChangeState(STATE_IDLE);
            ProcesQueuedComms();
        }

        // -------------------------------------------
        /* 
		 * DisplayLog
		 */
        public void DisplayLog(string _data)
		{
			m_inGameLog = _data + "\n";
			if (DEBUG_LOG)
			{
				Debug.Log("CommController::DisplayLog::DATA=" + _data);
			}
		}

		// -------------------------------------------
		/* 
		 * ClearLog
		 */
		public void ClearLog()
		{
			m_inGameLog = "";
		}

		private bool m_enableLog = true;

		// -------------------------------------------
		/* 
		 * OnGUI
		 */
		void OnGUI()
		{
			if (DEBUG_LOG)
			{
				if (!m_enableLog)
				{
					if (m_inGameLog.Length > 0)
					{
						ClearLog();
					}
				}

				if (m_enableLog)
				{
					if (m_inGameLog.Length > 0)
					{
						GUILayout.BeginScrollView(Vector2.zero);
						if (GUILayout.Button(m_inGameLog))
						{
							ClearLog();
						}
						GUILayout.EndScrollView();
					}
					else
					{
						switch (m_state)
						{
							case STATE_IDLE:
								break;

							case STATE_COMMUNICATION:
								GUILayout.BeginScrollView(Vector2.zero);
								GUILayout.Label("COMMUNICATION::Event=" + m_event);
								GUILayout.EndScrollView();
								break;
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * ProcessTimedEvents
		 */
		private void ProcessTimedEvents()
		{
			switch (m_state)
			{
				case STATE_IDLE:
					for (int i = 0; i < m_listTimedEvents.Count; i++)
					{
						CommEventData eventData = m_listTimedEvents[i];
						eventData.Time -= Time.deltaTime;
						if (eventData.Time <= 0)
						{
							m_listTimedEvents.RemoveAt(i);
							Request(eventData.NameEvent, eventData.IsBinaryResponse, eventData.List);
							eventData.Destroy();
							break;
						}
					}
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * ProcesQueuedComms
		 */
		private void ProcesQueuedComms()
		{
			// PRIORITY QUEUE
			if (m_priorityQueuedEvents.Count > 0)
			{
				int i = 0;
				CommEventData eventData = m_priorityQueuedEvents[i];
				m_priorityQueuedEvents.RemoveAt(i);
				Request(eventData.NameEvent, eventData.IsBinaryResponse, eventData.List);
				eventData.Destroy();
				return;
			}
			// NORMAL QUEUE
			if (m_listQueuedEvents.Count > 0)
			{
				int i = 0;
				CommEventData eventData = m_listQueuedEvents[i];
				m_listQueuedEvents.RemoveAt(i);
				Request(eventData.NameEvent, eventData.IsBinaryResponse, eventData.List);
				eventData.Destroy();
				return;
			}
		}

		// -------------------------------------------
		/* 
		 * ProcessQueueEvents
		 */
		private void ProcessQueueEvents()
		{
			switch (m_state)
			{
				case STATE_IDLE:
					break;

				case STATE_COMMUNICATION:
					break;
			}
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		public void Update()
		{
			Logic();

			ProcessTimedEvents();
			ProcessQueueEvents();
		}
	}

}