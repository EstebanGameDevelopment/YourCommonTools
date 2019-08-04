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

        public void ActivateModel(int _index)
        {
            for (int i = 0; i < Models.Length; i++)
            {
                Models[i].SetActive(false);
            }
            Models[_index].SetActive(true);
        }
	}
}