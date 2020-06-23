using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * SubmodelSelector
	 * 
	 * @author Esteban Gallardo
	 */
    public class SubmodelSelector : MonoBehaviour
	{
        public const string EVENT_SUBMODEL_SELECTOR_ON_TRIGGER_ENTER = "EVENT_SUBMODEL_SELECTOR_ON_TRIGGER_ENTER";

        public string Name;
        public GameObject[] Submodels;

        void OnTriggerEnter(Collider _collision)
        {
            BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_SUBMODEL_SELECTOR_ON_TRIGGER_ENTER, this.gameObject, _collision.gameObject);
        }

        public void ActivateSubmodel(int _index)
        {
            for (int i = 0; i < Submodels.Length; i++)
            {
                if (Submodels[i] != null)
                {
                    Submodels[i].SetActive(false);
                }
            }

            if ((_index>=0)&&(_index< Submodels.Length))
            {
                if (Submodels[_index] != null)
                {
                    Submodels[_index].SetActive(true);
                }
            }
        }
    }
}