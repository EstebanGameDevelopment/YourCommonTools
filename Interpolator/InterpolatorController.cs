using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

namespace YourCommonTools
{
	/******************************************
	* 
	* InterpolatorController
	* 
	* @author Esteban Gallardo
	*/
	public class InterpolatorController : MonoBehaviour
	{
        // ----------------------------------------------
        // CONSTANTS
        // ----------------------------------------------	
        public const int TYPE_INTERPOLATE_POSITION = 0;
        public const int TYPE_INTERPOLATE_FORWARD  = 1;
        public const int TYPE_INTERPOLATE_ROTATION  = 2;
        public const int TYPE_INTERPOLATE_SCALE  = 3;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static InterpolatorController _instance;
		public static InterpolatorController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(InterpolatorController)) as InterpolatorController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "InterpolatorController";
						_instance = container.AddComponent(typeof(InterpolatorController)) as InterpolatorController;
					}
				}
				return _instance;
			}
		}

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public bool EnableOnUpdate = true;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private List<IInterpolateData> m_inteporlateObjects = new List<IInterpolateData>();
        private List<IInterpolateData> m_inteporlateQueue = new List<IInterpolateData>();

        // -------------------------------------------
        /* 
		* Destroy all references
		*/
        void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		* Destroy all references
		*/
		public void Destroy()
		{
			if (_instance != null)
			{
				GameObject.Destroy(_instance);
			}
			_instance = null;
		}

		// -------------------------------------------
		/* 
		* Stop existing gameobject
		*/
		public bool Stop(GameObject _actor)
		{
			for (int i = 0; i < m_inteporlateObjects.Count; i++)
			{
                IInterpolateData item = m_inteporlateObjects[i];
				if (item.GameActor == _actor)
				{
					item.Destroy();
					m_inteporlateObjects.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

        // -------------------------------------------
        /* 
		* Stop all existing animations
		*/
        public bool StopAll()
        {
            for (int i = 0; i < m_inteporlateObjects.Count; i++)
            {
                IInterpolateData item = m_inteporlateObjects[i];
                item.Destroy();
                m_inteporlateObjects.RemoveAt(i);
            }
            return false;
        }

        // -------------------------------------------
        /* 
		* InterpolatePosition
		*/
        public void Interpolate(GameObject _actor, Vector3 _goal, float _time, bool _setTargetWhenFinished = false, bool _loop = false)
        {
            m_inteporlateQueue.Add(new InterpolatePositionData(_actor, _actor.transform.position, _goal, _time, 0, _setTargetWhenFinished, _loop));
        }

        // -------------------------------------------
        /* 
		* InterpolatePosition
		*/
        public void InterpolatePosition(GameObject _actor, Vector3 _goal, float _time, bool _setTargetWhenFinished = false, bool _loop = false)
		{
            m_inteporlateQueue.Add(new InterpolatePositionData(_actor, _actor.transform.position, _goal, _time, 0, _setTargetWhenFinished, _loop));
		}
       
        // -------------------------------------------
        /* 
		* InterpolateForward
		*/
        public void InterpolateForward(GameObject _actor, Vector3 _goal, float _time, bool _setTargetWhenFinished = false)
        {
            m_inteporlateQueue.Add(new InterpolateForwardData(_actor, _actor.transform.forward, _goal, _time, 0, _setTargetWhenFinished));
        }

        // -------------------------------------------
        /* 
		* InterpolateRotation
		*/
        public void InterpolateRotation(GameObject _actor, Quaternion _goal, float _time, bool _setTargetWhenFinished = false)
        {
            m_inteporlateQueue.Add(new InterpolateRotationData(_actor, _actor.transform.rotation, _goal, _time, 0, _setTargetWhenFinished));
        }

        // -------------------------------------------
        /* 
		* InterpolateScale
		*/
        public void InterpolateScale(GameObject _actor, Vector3 _goal, float _time, bool _setTargetWhenFinished = false)
        {
            m_inteporlateQueue.Add(new InterpolateScaleData(_actor, _actor.transform.localScale, _goal, _time, 0, _setTargetWhenFinished));
        }

        // -------------------------------------------
        /* 
		 * Logic
		 */
        public void Logic()
        {
            try
            {
                for (int i = 0; i < m_inteporlateObjects.Count; i++)
                {
                    IInterpolateData itemData = m_inteporlateObjects[i];
                    if (itemData.Inperpolate())
                    {
                        itemData.Destroy();
                        m_inteporlateObjects.RemoveAt(i);
                        i--;
                    }
                }
            }
            catch (Exception err) { };
            for (int j = 0; j < m_inteporlateQueue.Count; j++)
            {
                IInterpolateData newItem = m_inteporlateQueue[j];
                bool found = false;
                for (int i = 0; i < m_inteporlateObjects.Count; i++)
                {
                    IInterpolateData item = m_inteporlateObjects[i];
                    if ((item.GameActor == newItem.GameActor) && (item.TypeData == newItem.TypeData))
                    {
                        item.ResetData(newItem.GameActor.transform, newItem.Goal, newItem.TotalTime, 0);
                        found = true;
                    }
                }
                if (!found)
                {
                    m_inteporlateObjects.Add(newItem);
                }
                else
                {
                    newItem.Destroy();
                    newItem = null;
                }
            }
            m_inteporlateQueue.Clear();
        }

        // -------------------------------------------
        /* 
		 * Run logic of the interpolation
		 */
        void Update()
		{
            if (EnableOnUpdate) Logic();
        }
	}
}