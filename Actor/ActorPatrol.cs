using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourCommonTools
{
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
    /******************************************
	* 
	* ActorPatrol
	* 
	* A simple patrol in waypoints behaviour
	* 
	* @author Esteban Gallardo
	*/
    public class ActorPatrol : MonoBehaviour
	{
        // ----------------------------------------------
        // PUBLIC MEMBERS
        // ----------------------------------------------
        public int CurrentWaypoint = 0;
        public GameObject[] Waypoints;
        public float[] SpeedToWaypoint;
        public float RotationSpeed = 0.05f;
        public float DistanceToWaypoint = 1f;

        public bool Loop = false;
        public bool LocalAnimation = false;

        // ----------------------------------------------
        // PRIVATE MEMBERS
        // ----------------------------------------------
        private Actor m_actor;
        private bool m_runPatrolling = true;
        private Vector3 m_nextWaypoint;

        // -------------------------------------------
        /* 
		 * Initialization of the element
		 */
        public void Start()
        {
            m_actor = this.gameObject.GetComponent<Actor>();
            if (m_actor == null)
            {
                throw new Exception("The behaviour ActorPatrol should be applied on a GameObject with the script Actor");
            }
            if (Waypoints.Length != SpeedToWaypoint.Length)
            {
                throw new Exception("The vectors of waypoints and speed don't match in the number of elements");
            }

            // HIDE WAYPOINTS
            for (int i = 0; i < Waypoints.Length; i++)
            {
                Waypoints[i].gameObject.SetActive(false);
            }

            // SET IN THE INITIAL WAYPOINT
            this.gameObject.transform.position = Waypoints[CurrentWaypoint].transform.position;
            IncreaseWaypoint();
        }

        // -------------------------------------------
        /* 
		 * IncreaseWaypoint
		 */
        private void IncreaseWaypoint()
        {
            CurrentWaypoint++;
            if (CurrentWaypoint < Waypoints.Length)
            {
                m_nextWaypoint = Waypoints[CurrentWaypoint].transform.position;
            }
            else
            {
                if (Loop)
                {
                    CurrentWaypoint = -1;
                    IncreaseWaypoint();
                }
                else
                {
                    m_runPatrolling = false;
                }
            }
        }

        // -------------------------------------------
        /* 
		 * Initialization of the element
		 */
        public void Update()
        {
            if (m_runPatrolling)
            {
                if (LocalAnimation)
                {
                    Utilities.LogicLocalAlineation(m_actor, m_nextWaypoint, SpeedToWaypoint[CurrentWaypoint], RotationSpeed);
                    if (Vector3.Distance(m_nextWaypoint, this.gameObject.transform.localPosition) < DistanceToWaypoint)
                    {
                        IncreaseWaypoint();
                    }
                }
                else
                {
                    Utilities.LogicAlineation(m_actor, m_nextWaypoint, SpeedToWaypoint[CurrentWaypoint], RotationSpeed);
                    if (Vector3.Distance(m_nextWaypoint, this.gameObject.transform.position) < DistanceToWaypoint)
                    {
                        IncreaseWaypoint();
                    }
                }
            }
        }

    }
}