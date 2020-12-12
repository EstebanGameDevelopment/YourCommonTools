using System;
using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * InterpolateData
	 * 
	 * Keeps the information of a gameobject to be interpolated
	 * 
	 * @author Esteban Gallardo
	 */
	public class InterpolateData : IEquatable<InterpolateData>
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_INTERPOLATE_STARTED   = "EVENT_INTERPOLATE_STARTED";
        public const string EVENT_INTERPOLATE_COMPLETED = "EVENT_INTERPOLATE_COMPLETED";
        public const string EVENT_INTERPOLATE_FREEZE    = "EVENT_INTERPOLATE_FREEZE";
        public const string EVENT_INTERPOLATE_RESUME    = "EVENT_INTERPOLATE_RESUME";

        // -----------------------------------------
        // PRIVATE VARIABLES
        // -----------------------------------------
        private GameObject m_gameActor;
		private Vector3 m_origin;
		private Vector3 m_goal;
		private float m_totalTime;
		private float m_timeDone;
        private bool m_activated;
        private bool m_setTargetWhenFinished;
        private bool m_firstRun = true;
        private float m_delay = 0;

        // -----------------------------------------
        // GETTERS/SETTERS
        // -----------------------------------------
        public GameObject GameActor
		{
			get { return m_gameActor; }
		}
		public Vector3 Goal
		{
			get { return m_goal; }
			set { m_goal = value; }
		}
		public float TotalTime
		{
			get { return m_totalTime; }
			set { m_totalTime = value; }
		}
		public float TimeDone
		{
			get { return m_timeDone; }
			set { m_timeDone = 0; }
		}
        public bool SetTargetWhenFinished
        {
            get { return m_setTargetWhenFinished; }
            set { m_setTargetWhenFinished = value; }
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public InterpolateData(GameObject _actor, Vector3 _origin, Vector3 _goal, float _totalTime, float _timeDone, bool _setTargetWhenFinished, float _delay = 0)
		{
			m_gameActor = _actor;
            m_activated = true;
            m_setTargetWhenFinished = _setTargetWhenFinished;
            m_delay = _delay;

            ResetData(_origin, _goal, _totalTime, _timeDone);

            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
		}

        // -------------------------------------------
        /* 
		 * ResetData
		 */
        public void ResetData(Vector3 _origin, Vector3 _goal, float _totalTime, float _timeDone)
		{
			m_origin = new Vector3(_origin.x, _origin.y, _origin.z);
			m_goal = new Vector3(_goal.x, _goal.y, _goal.z);
			m_totalTime = _totalTime;
			m_timeDone = _timeDone;
            m_activated = true;
        }

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		public void Destroy()
		{
			m_gameActor = null;
            BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
        }

		// -------------------------------------------
		/* 
		 * Interpolate the position between two points
		 */
		public bool Inperpolate()
		{
            if (!m_activated) return false;
			if (m_gameActor == null) return true;

            if (m_firstRun)
            {
                m_firstRun = false;
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_INTERPOLATE_STARTED, m_gameActor);
            }

            if (m_delay > 0)
            {
                m_delay -= Time.deltaTime;
                return false;
            }

            m_timeDone += Time.deltaTime;
            if (m_timeDone <= m_totalTime)
			{
				Vector3 forwardTarget = (m_goal - m_origin);
				float increaseFactor = (1 - ((m_totalTime - m_timeDone) / m_totalTime));
				m_gameActor.transform.position = m_origin + (increaseFactor * forwardTarget);
				return false;
			}
			else
			{
				if (m_timeDone <= m_totalTime)
				{
					return false;
				}
				else
				{
                    if (m_setTargetWhenFinished)
                    {
                        m_gameActor.transform.position = m_goal;
                    }
					BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_INTERPOLATE_COMPLETED, m_gameActor);
					return true;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Equals
		 */
		public bool Equals(InterpolateData _other)
		{
			return m_gameActor == _other.GameActor;
		}


        // -------------------------------------------
        /* 
		 * OnBasicSystemEvent
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EVENT_INTERPOLATE_FREEZE)
            {
                GameObject target = (GameObject)_list[0];
                if (target == m_gameActor)
                {
                    m_activated = false;
                }
            }
            if (_nameEvent == EVENT_INTERPOLATE_RESUME)
            {
                GameObject target = (GameObject)_list[0];
                if (target == m_gameActor)
                {
                    m_activated = true;
                }
            }
        }
    }
}