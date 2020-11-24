using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * DebugInGamePanel
	 * 
	 * @author Esteban Gallardo
	 */
    public class DebugInGamePanel : MonoBehaviour
	{
        public const string EVENT_DEBUGINGAMEPANEL_CLEAR = "EVENT_DEBUGINGAMEPANEL_CLEAR";
        public const string EVENT_DEBUGINGAMEPANEL_REPORT = "EVENT_DEBUGINGAMEPANEL_REPORT";

        public Text[] Info;

        private void Start()
        {
            if (Info.Length > 0)
            {
                foreach (Text item in Info)
                {
                    if (item != null) item.text = "";
                }
                BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicSystemEvent);
            }                
        }

        private void OnDestroy()
        {
            if (Info.Length > 0)
            {
                BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicSystemEvent;
            }
        }

        private void OnBasicSystemEvent(string _nameEvent, object[] _list)
        {
            if (Info.Length > 0)
            {
                if (_nameEvent == EVENT_DEBUGINGAMEPANEL_CLEAR)
                {
                    foreach (Text item in Info)
                    {
                        if (item != null) item.text = "";
                    }
                }
                if (_nameEvent == EVENT_DEBUGINGAMEPANEL_REPORT)
                {
                    int index = (int)_list[0];
                    if (index < Info.Length)
                    {
                        if (Info[index] != null)
                        {
                            Info[index].text = (string)_list[1];
                            Debug.LogError((string)_list[1]);
                        }
                    }
                }
            }
        }
    }
}