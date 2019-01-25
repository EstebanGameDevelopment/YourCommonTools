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
	* RotatorController
	* 
	* @author Esteban Gallardo
	*/
    public class RotatorController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static RotatorController _instance;
		public static RotatorController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(RotatorController)) as RotatorController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "RotatorController";
						_instance = container.AddComponent(typeof(RotatorController)) as RotatorController;
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
        private List<RotatorData> m_rotatorObjects = new List<RotatorData>();
        private List<RotatorData> m_rotatorQueue = new List<RotatorData>();

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
			for (int i = 0; i < m_rotatorObjects.Count; i++)
			{
                RotatorData item = m_rotatorObjects[i];
				if (item.GameActor == _actor)
				{
					item.Destroy();
					m_rotatorObjects.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		* Instantiate a new shoot
		*/
		public void Interpolate(GameObject _actor, Quaternion _goal, float _time, bool _setTargetWhenFinished = false)
		{
            m_rotatorQueue.Add(new RotatorData(_actor, _actor.transform.rotation, _goal, _time, 0, _setTargetWhenFinished));
		}

        // -------------------------------------------
        /* 
		 * Logic
		 */
        public void Logic()
        {
            try
            {
                for (int i = 0; i < m_rotatorObjects.Count; i++)
                {
                    RotatorData itemData = m_rotatorObjects[i];
                    if (itemData.Inperpolate())
                    {
                        itemData.Destroy();
                        m_rotatorObjects.RemoveAt(i);
                        i--;
                    }
                }
            }
            catch (Exception err) { };
            for (int j = 0; j < m_rotatorQueue.Count; j++)
            {
                RotatorData newItem = m_rotatorQueue[j];
                bool found = false;
                for (int i = 0; i < m_rotatorObjects.Count; i++)
                {
                    RotatorData item = m_rotatorObjects[i];
                    if (item.GameActor == newItem.GameActor)
                    {
                        item.ResetData(newItem.GameActor.transform.rotation, newItem.Goal, newItem.TotalTime, 0);
                        found = true;
                    }
                }
                if (!found)
                {
                    m_rotatorObjects.Add(newItem);
                }
                else
                {
                    newItem.Destroy();
                    newItem = null;
                }
            }
            m_rotatorQueue.Clear();
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