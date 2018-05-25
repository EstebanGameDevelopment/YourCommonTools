using System;
using UnityEngine;
using System.Collections;

namespace YourCommonTools
{
	public class StateManagerNoMono
	{
		protected int m_state;
		protected int m_lastState;
		protected int m_iterator;
		protected float m_timeAcum;
		protected float m_scale;

		// -------------------------------------------
		/* 
		 * Constructor		
		 */
		public StateManagerNoMono()
		{
			m_iterator = 0;
			m_state = -1;
		}

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------		
		public int State
		{
			get { return m_state; }
		}
		public int Iterator
		{
			get { return m_iterator; }
			set { m_iterator = value; }
		}
		public float TimeAcum
		{
			get { return m_timeAcum; }
			set { m_timeAcum = value; }
		}

		// -------------------------------------------
		/* 
		 * Change the state of the object		
		 */
		protected virtual void ChangeState(int _newState)
		{
			m_lastState = m_state;
			m_iterator = 0;
			m_state = _newState;
			m_timeAcum = 0;
		}

		// -------------------------------------------
		/* 
		 * Change the state of the object		
		 */
		public virtual void ChangeStatePublic(int _newState)
		{
			m_lastState = m_state;
			m_iterator = 0;
			m_state = _newState;
			m_timeAcum = 0;
		}

		// -------------------------------------------
		/* 
		 * Update		
		 */
		public virtual void Logic()
		{
			if (m_iterator < 100) m_iterator++;
		}
	}
	
}

