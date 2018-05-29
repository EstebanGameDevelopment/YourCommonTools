﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YourCommonTools
{
	/******************************************
	* 
	* CommEventData
	* 
	* Class used to dispatch events of communications HTTP
	* 
	* @author Esteban Gallardo
	*/
	public class CommEventData
	{
		private string m_nameEvent;
		private bool m_isBinaryResponse;
		private float m_time;
		private object[] m_list;

		public string NameEvent
		{
			get { return m_nameEvent; }
		}
		public bool IsBinaryResponse
		{
			get { return m_isBinaryResponse; }
			set { m_isBinaryResponse = value; }
		}
		public float Time
		{
			get { return m_time; }
			set { m_time = value; }
		}
		public object[] List
		{
			get { return m_list; }
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public CommEventData(string _nameEvent, bool _isBinaryResponse, float _time, params object[] _list)
		{
			m_nameEvent = _nameEvent;
			m_isBinaryResponse = _isBinaryResponse;
			m_time = _time;
			m_list = _list;
		}

		public void Destroy()
		{
			m_list = null;
		}

	}
}