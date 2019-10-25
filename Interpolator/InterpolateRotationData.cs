using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * InterpolateRotationData
	 * 
	 * Keeps the information of a gameobject to be interpolated
	 * 
	 * @author Esteban Gallardo
	 */
    public class InterpolateRotationData : IEquatable<InterpolatePositionData>, IInterpolateData
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
		private Vector4 m_origin;
		private Vector4 m_goal;
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
			set { m_goal = (Vector4)value; }
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
            get { return InterpolatorController.TYPE_INTERPOLATE_ROTATION; }
        }

        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public InterpolateRotationData(GameObject _actor, Quaternion _origin, Quaternion _goal, float _totalTime, float _timeDone, bool _setTargetWhenFinished)
		{
			m_gameActor = _actor;
            m_activated = true;
            m_setTargetWhenFinished = _setTargetWhenFinished;

            ResetData(m_gameActor.transform, new Vector4(_goal.x, _goal.y, _goal.z, _goal.w), _totalTime, _timeDone);

            
            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
		}

        // -------------------------------------------
        /* 
		 * ResetData
		 */
        public void ResetData(Transform _origin, object _goal, float _totalTime, float _timeDone)
		{
			m_origin = new Vector4(_origin.rotation.x, _origin.rotation.y, _origin.rotation.z, _origin.rotation.w);
            Vector4 sgoal = (Vector4)_goal;
            m_goal = new Vector4(sgoal.x, sgoal.y, sgoal.z, sgoal.w);
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
				Vector4 quaternionTarget = (m_goal - m_origin);
				float increaseFactor = (1 - ((m_totalTime - m_timeDone) / m_totalTime));
                Vector4 newRotation = m_origin + (increaseFactor * quaternionTarget);
                m_gameActor.transform.rotation = new Quaternion(newRotation.x, newRotation.y, newRotation.z, newRotation.w);
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
                        m_gameActor.transform.rotation = new Quaternion(m_goal.x, m_goal.y, m_goal.z, m_goal.w);
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