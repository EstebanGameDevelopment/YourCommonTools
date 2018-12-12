using System;
using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YourCommonTools
{

    /******************************************
	 * 
	 * LoadSceneOnClick
	 * 
	 * @author Esteban Gallardo
	 */
    public class LoadSceneOnClick : MonoBehaviour
	{
        public bool InformationMessage = true;
        public string NameScene;

        public void OnGUI()
        {
            if (InformationMessage) GUI.Label(new Rect(0, 0, 100, 100), "Load scene[" + NameScene + "] on click");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene(NameScene);
            }
        }
    }
}