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
		// EVENTS
		// ----------------------------------------------	

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
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private List<InterpolateData> m_inteporlateObjects = new List<InterpolateData>();
        private List<InterpolateData> m_inteporlateQueue = new List<InterpolateData>();

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
				InterpolateData item = m_inteporlateObjects[i];
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
		* Instantiate a new shoot
		*/
		public void Interpolate(GameObject _actor, Vector3 _goal, float _time, bool _setTargetWhenFinished = false)
		{
            m_inteporlateQueue.Add(new InterpolateData(_actor, _actor.transform.position, _goal, _time, 0, _setTargetWhenFinished));
		}

		// -------------------------------------------
		/* 
		 * Run logic of the interpolation
		 */
		void Update()
		{
            try
            {
                for (int i = 0; i < m_inteporlateObjects.Count; i++)
                {
                    InterpolateData itemData = m_inteporlateObjects[i];
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
                InterpolateData newItem = m_inteporlateQueue[j];
                bool found = false;
                for (int i = 0; i < m_inteporlateObjects.Count; i++)
                {
                    InterpolateData item = m_inteporlateObjects[i];
                    if (item.GameActor == newItem.GameActor)
                    {
                        item.ResetData(newItem.GameActor.transform.position, newItem.Goal, newItem.TotalTime, 0);
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
	}
}