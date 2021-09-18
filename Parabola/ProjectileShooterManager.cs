using System;
using System.Collections.Generic;
using UnityEngine;

namespace YourCommonTools
{
    /******************************************
     * 
     * ProjectileShooterPanel
     * 
     * @author Esteban Gallardo
     */
    public class ProjectileShooterManager : MonoBehaviour
    {
        // ----------------------------------------------
        // PUBLIC EVENTS
        // ----------------------------------------------	
        public const string EVENT_PROJECTILESHOOTERMANAGER_UPDATE_ENABLED = "EVENT_PROJECTILESHOOTERMANAGER_UPDATE_ENABLED";

        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------	
        public GameObject BallReference;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------	
        private bool m_updateEnabled = true;

        private List<ProjectileShooter> m_projectileShooter = new List<ProjectileShooter>();

        // -------------------------------------------
        /* 
		 * RunParabola
		 */
        public void RunParabola(GameObject _ball, Vector3 _origin, Vector3 _target, bool _calculatePower, float _power = 2, float _duration = 0.6f, float _delay = 0, bool _showTrajectory = true)
        {
            ProjectileShooter newProjectile = new ProjectileShooter();
            newProjectile.Initialitzation(_ball, (_showTrajectory?BallReference:null), _origin, _target, _calculatePower, _power, _duration, _delay);
            m_projectileShooter.Add(newProjectile);
            // Debug.LogError("RunParabola::ADD NEW PROJECTILE[" + m_projectileShooter.Count + "]");

            BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
        }

        // -------------------------------------------
        /* 
		 * ClearAll
		 */
        public void ClearAll()
        {
            for (int i = 0; i < m_projectileShooter.Count; i++)
            {
                m_projectileShooter[i].Destroy();
            }
            m_projectileShooter.Clear();
        }

        // -------------------------------------------
        /* 
		 * OnDestroy
		 */
        private void OnDestroy()
        {
            BasicSystemEventController.Instance.BasicSystemEvent -= new BasicSystemEventHandler(OnBasicSystemEvent);
        }

        // -------------------------------------------
        /* 
		 * OnBasicSystemEvent
		 */
        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == ProjectileShooter.EVENT_PROJECTILESHOOTER_DESTROY)
            {
                ProjectileShooter projectileToDelete = (ProjectileShooter)_list[0];
                for (int i = 0; i < m_projectileShooter.Count; i++)
                {
                    if (m_projectileShooter[i] == projectileToDelete)
                    {
                        projectileToDelete.Destroy();
                        m_projectileShooter.RemoveAt(i);
                        // Debug.LogError("EVENT_PROJECTILESHOOTER_DESTROY::TOTAL NUMBER OF PROJECTILES LEFT[" + m_projectileShooter.Count + "]");
                        break;
                    }
                }
            }
            if (_nameEvent == EVENT_PROJECTILESHOOTERMANAGER_UPDATE_ENABLED)
            {
                m_updateEnabled = (bool)_list[0];
            }
        }

        // -------------------------------------------
        /* 
		 * Update
		 */
        private void Update()
        {
            if (m_updateEnabled)
            {
                for (int i = 0; i < m_projectileShooter.Count; i++)
                {
                    m_projectileShooter[i].Update();
                }
            }
        }
    }

}