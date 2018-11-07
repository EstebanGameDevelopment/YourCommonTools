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
	* AlphaController
	* 
	* @author Esteban Gallardo
	*/
	public class AlphaController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static AlphaController _instance;
		public static AlphaController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(AlphaController)) as AlphaController;
					if (!_instance)
					{
						GameObject container = new GameObject();
						container.name = "AlphaController";
						_instance = container.AddComponent(typeof(AlphaController)) as AlphaController;
					}
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private List<AlphaData> m_inteporlateObjects = new List<AlphaData>();
        private List<AlphaData> m_inteporlateQueue = new List<AlphaData>();

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
                AlphaData item = m_inteporlateObjects[i];
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
		public void Interpolate(GameObject _actor, float _origin, float _goal, float _time, bool _setTargetWhenFinished = false)
		{
            m_inteporlateQueue.Add(new AlphaData(_actor, _origin, _goal, _time, 0, _setTargetWhenFinished));
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
                    AlphaData itemData = m_inteporlateObjects[i];
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
                AlphaData newItem = m_inteporlateQueue[j];
                bool found = false;
                for (int i = 0; i < m_inteporlateObjects.Count; i++)
                {
                    AlphaData item = m_inteporlateObjects[i];
                    if (item.GameActor == newItem.GameActor)
                    {
                        item.ResetData(newItem.GameActor.GetComponent<CanvasGroup>().alpha, newItem.Goal, newItem.TotalTime, 0);
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