using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * ClassFinder
	 * 
	 * @author Esteban Gallardo
	 */
    public class ClassFinder : MonoBehaviour
	{
		public const string EVENT_CLASSFINDER_START = "EVENT_CLASSFINDER_START";

		public string Name;

		void Start()
        {
			BasicSystemEventController.Instance.DelayBasicSystemEvent(EVENT_CLASSFINDER_START, 0.1f, this);
        }
	}
}