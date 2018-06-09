﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourCommonTools
{

	/******************************************
	* 
	* CollisionTriggerEvent
	* 
	* @author Esteban Gallardo
	*/
	public class CollisionTriggerEvent : MonoBehaviour
	{
		public const string EVENT_COLLIDERTRIGGER_ENTER_EVENT = "EVENT_COLLIDERTRIGGER_ENTER_EVENT";
		public const string EVENT_COLLIDERTRIGGER_EXIT_EVENT = "EVENT_COLLIDERTRIGGER_EXIT_EVENT";

		public GameObject TargetObject = null;
		public bool DestroyOnEnter = false;
		public bool DestroyOnExit = false;

		void OnTriggerEnter(Collider _collision)
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
				BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_COLLIDERTRIGGER_ENTER_EVENT, this.gameObject, _collision.gameObject);
				if (DestroyOnEnter)
				{
					TargetObject = null;
					GameObject.Destroy(this.gameObject);
				}
			}
		}

		void OnTriggerExit(Collider _collision)
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
				BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_COLLIDERTRIGGER_EXIT_EVENT, this.gameObject, _collision.gameObject);
				if (DestroyOnExit)
				{
					TargetObject = null;
					GameObject.Destroy(this.gameObject);
				}
			}
		}
	}
}