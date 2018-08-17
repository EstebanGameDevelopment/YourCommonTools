namespace YourCommonTools
{

	/******************************************
	 * 
	 * AppEventData
	 * 
	 * Class used to dispatch events with a certain delay in time
	 * 
	 * @author Esteban Gallardo
	 */
	public class AppEventData
	{
		public const int CONFIGURATION_INTERNAL_EVENT = 0;
		public const int CONFIGURATION_SERVER_EVENT = 1;
		public const int CONFIGURATION_CLIENT_EVENT = 2;
		public const int CONFIGURATION_SERVER_AND_CLIENT_EVENT = 3;

		private string m_nameEvent;
		private int m_configuration;
		private bool m_isLocalEvent;
		private float m_time;
		private int m_networkID;
		private object[] m_listParameters;

		public string NameEvent
		{
			get { return m_nameEvent; }
		}
		public int Configuration
		{
			get { return m_configuration; }
		}
		public bool IsLocalEvent
		{
			get { return m_isLocalEvent; }
		}
		public float Time
		{
			get { return m_time; }
			set { m_time = value; }
		}
		public int NetworkID
		{
			get { return m_networkID; }
			set { m_networkID = value; }
		}
		public object[] ListParameters
		{
			get { return m_listParameters; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public AppEventData(string _nameEvent, int _configuration, bool _isLocalEvent, int _networkID, float _time, params object[] _list)
		{
			m_nameEvent = _nameEvent;
			m_configuration = _configuration;
			m_isLocalEvent = _isLocalEvent;
			m_networkID = _networkID;
			m_time = _time;
			m_listParameters = _list;
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			m_listParameters = null;
		}

		// -------------------------------------------
		/* 
		 * ToStringParameters
		 */
		public string ToStringParameters()
		{
			string parameters = "";
			if (m_listParameters != null)
			{
				for (int i = 0; i < m_listParameters.Length; i++)
				{
					if (Utilities.IsNumber(m_listParameters[i]))
					{
						parameters += (float)Utilities.GetDouble(m_listParameters[i]);
					}
					else
					{
						if (m_listParameters[i] is string)
						{
							parameters += (string)m_listParameters[i];
						}
						else
						{
							parameters += m_listParameters[i].ToString();
						}
					}
					if (i + 1 < m_listParameters.Length)
					{
						parameters += ",";
					}
				}
			}
			return parameters;
		}

		// -------------------------------------------
		/* 
		 * ToString
		 */
		public override string ToString()
		{
			string parameters = ToStringParameters();
			if (parameters.Length > 0)
			{
				parameters = "," + parameters;
			}

			return m_nameEvent + parameters;
		}

		// -------------------------------------------
		/* 
		 * IsInternalEvent
		 */
		public bool IsInternalEvent()
		{
			return (m_configuration == CONFIGURATION_INTERNAL_EVENT);
		}
	}
}