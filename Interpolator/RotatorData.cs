using System;
using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * RotatorData
	 * 
	 * Keeps the information of a gameobject to be rotated
	 * 
	 * @author Esteban Gallardo
	 */
    public class RotatorData : IEquatable<RotatorData>
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_INTERPOLATE_ROTATION_STARTED   = "EVENT_INTERPOLATE_ROTATION_STARTED";
        public const string EVENT_INTERPOLATE_ROTATION_COMPLETED = "EVENT_INTERPOLATE_ROTATION_COMPLETED";

        // -----------------------------------------
        // PRIVATE VARIABLES
        // -----------------------------------------
        private GameObject m_gameActor;
		private Quaternion m_origin;
		private Quaternion m_goal;
		private float m_totalTime;
		private float m_timeDone;
        private bool m_activated;
        private bool m_setTargetWhenFinished;
        private float m_angularSpeed;
        private bool m_firstRun = true;

        // -----------------------------------------
        // GETTERS/SETTERS
        // -----------------------------------------
        public GameObject GameActor
		{
			get { return m_gameActor; }
		}
		public Quaternion Goal
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
        public RotatorData(GameObject _actor, Quaternion _origin, Quaternion _goal, float _totalTime, float _timeDone, bool _setTargetWhenFinished)
		{
			m_gameActor = _actor;
            m_activated = true;
            m_setTargetWhenFinished = _setTargetWhenFinished;

            ResetData(_origin, _goal, _totalTime, _timeDone);

            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
		}

        // -------------------------------------------
        /* 
		 * ResetData
		 */
        public void ResetData(Quaternion _origin, Quaternion _goal, float _totalTime, float _timeDone)
		{
			m_origin = new Quaternion(_origin.x, _origin.y, _origin.z, _origin.w);
			m_goal = new Quaternion(_goal.x, _goal.y, _goal.z, _goal.w);
			m_totalTime = _totalTime;
			m_timeDone = _timeDone;
            m_angularSpeed = Quaternion.Angle(m_origin, m_goal);
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
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_INTERPOLATE_ROTATION_STARTED, m_gameActor);
            }

            m_timeDone += Time.deltaTime;
            if (m_timeDone <= m_totalTime)
			{
                float currentAngle = m_angularSpeed * (m_timeDone / m_totalTime);
                m_gameActor.transform.rotation = Quaternion.RotateTowards(m_origin, m_goal, currentAngle);
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
                        m_gameActor.transform.rotation = m_goal;
                    }
					BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_INTERPOLATE_ROTATION_COMPLETED, m_gameActor);
					return true;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Equals
		 */
		public bool Equals(RotatorData _other)
		{
			return m_gameActor == _other.GameActor;
		}


        // -------------------------------------------
        /* 
		 * OnBasicSystemEvent
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == InterpolatePositionData.EVENT_INTERPOLATE_FREEZE)
            {
                GameObject target = (GameObject)_list[0];
                if (target == m_gameActor)
                {
                    m_activated = false;
                }
            }
            if (_nameEvent == InterpolatePositionData.EVENT_INTERPOLATE_RESUME)
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