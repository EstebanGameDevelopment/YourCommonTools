using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * ActivateLocalModel
	 * 
	 * @author Esteban Gallardo
	 */
    public class ActivateLocalModel : MonoBehaviour
	{
        public string Name;
        public GameObject[] Models;

        void Start()
        {
            for (int i = 0; i < Models.Length; i++)
            {
                Models[i].SetActive(false);
            }
        }

        public void ActivateModel(int _index)
        {
            Models[_index].SetActive(true);
        }
	}
}