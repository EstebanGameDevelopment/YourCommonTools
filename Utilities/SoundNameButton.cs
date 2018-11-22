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
	 * SoundNameButton
	 * 
	 * @author Esteban Gallardo
	 */
     [RequireComponent(typeof(Button))]
    public class SoundNameButton : MonoBehaviour
	{
        public string NameSound = "";

        private void Start()
        {
            this.gameObject.GetComponent<Button>().onClick.AddListener(PlaySound);
        }

        private void PlaySound()
        {
            BasicSystemEventController.Instance.DispatchBasicSystemEvent(SoundsController.EVENT_SOUNDSCONTROLLER_PLAY_BUTTON_SOUND, NameSound);
        }
    }
}