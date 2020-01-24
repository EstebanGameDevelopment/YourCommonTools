using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * InterpolateScaleData
	 * 
	 * Keeps the information of a gameobject to be interpolated
	 * 
	 * @author Esteban Gallardo
	 */
    public class InterpolateScaleData : IEquatable<InterpolatePositionData>, IInterpolateData
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

        // -----------------------------------------
        // GETTERS/SETTERS
        // -----------------------------------------
        public GameObject GameActor
		{
			get { return m_gameActor; }
		}
		public object Goal
		{
			get { return m_goal; }
			set { m_goal = (Vector3)value; }
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
        public int TypeData
        {
            get { return InterpolatorController.TYPE_INTERPOLATE_SCALE; }
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public InterpolateScaleData(GameObject _actor, Vector3 _origin, Vector3 _goal, float _totalTime, float _timeDone, bool _setTargetWhenFinished)
		{
			m_gameActor = _actor;
            m_activated = true;
            m_setTargetWhenFinished = _setTargetWhenFinished;

            ResetData(m_gameActor.transform, _goal, _totalTime, _timeDone);

            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
		}

        // -------------------------------------------
        /* 
		 * ResetData
		 */
        public void ResetData(Transform _origin, object _goal, float _totalTime, float _timeDone)
		{
			m_origin = new Vector3(_origin.localScale.x, _origin.localScale.y, _origin.localScale.z);
            Vector3 sgoal = (Vector3)_goal;
            m_goal = new Vector3(sgoal.x, sgoal.y, sgoal.z);
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

            m_timeDone += Time.deltaTime;
            if (m_timeDone <= m_totalTime)
			{
				Vector3 forwardTarget = (m_goal - m_origin);
				float increaseFactor = (1 - ((m_totalTime - m_timeDone) / m_totalTime));
				m_gameActor.transform.localScale = m_origin + (increaseFactor * forwardTarget);
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
		public bool Equals(InterpolatePositionData _other)
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