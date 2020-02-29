using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourCommonTools
{

    /******************************************
	* 
	* CollisionColliderEvent
	* 
	* @author Esteban Gallardo
	*/
    public class CollisionColliderEvent : MonoBehaviour
	{
		public const string EVENT_COLLIDERCOLLIDER_ENTER_EVENT = "EVENT_COLLIDERCOLLIDER_ENTER_EVENT";
		public const string EVENT_COLLIDERCOLLIDER_EXIT_EVENT = "EVENT_COLLIDERCOLLIDER_EXIT_EVENT";

		public GameObject TargetObject = null;
        public string CustomEventEnter = "";
        public string CustomEventExit = "";
        public bool DestroyOnEnter = false;
		public bool DestroyOnExit = false;

		void OnCollisionEnter(Collision _collision)
		{
			bool hasCollided = true;
			if (TargetObject != null)
			{
				if (_collision.gameObject == TargetObject)
				{
					hasCollided = true;
				}
				else
				{
					hasCollided = false;
				}
			}
				
			if (hasCollided)
			{
                if (CustomEventEnter.Length > 0)
                {
                    // Debug.LogError("CollisionColliderEvent::CustomEventEnter["+ CustomEventEnter + "] COLLIDED WITH " + _collision.gameObject.name);
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(CustomEventEnter, this.gameObject, _collision.gameObject);
                }
				else
                {
                    // Debug.LogError("+++++CollisionColliderEvent::this.gameObject=" + this.gameObject.name+ " COLLIDED WITH " + _collision.gameObject.name);
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_COLLIDERCOLLIDER_ENTER_EVENT, this.gameObject, _collision.gameObject);
                }
				if (DestroyOnEnter)
				{
					TargetObject = null;
					GameObject.Destroy(this.gameObject);
				}
			}
		}

		void OnCollisionExit(Collision _collision)
		{
			bool hasCollided = true;
			if (TargetObject != null)
			{
				if (_collision.gameObject == TargetObject)
				{
					hasCollided = true;
				}
				else
				{
					hasCollided = false;
				}
			}

			if (hasCollided)
			{
                if (CustomEventExit.Length > 0)
                {
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(CustomEventExit, this.gameObject, _collision.gameObject);
                }
                else
                {
                    BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_COLLIDERCOLLIDER_EXIT_EVENT, this.gameObject, _collision.gameObject);
                }                    
				if (DestroyOnExit)
				{
					TargetObject = null;
					GameObject.Destroy(this.gameObject);
				}
			}
		}
	}
}