using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace YourCommonTools
{
    public class CustomAudioSource : MonoBehaviour
    {
        public AudioSource AudioSource
        {
            get { return this.gameObject.GetComponent<AudioSource>(); }
        }

        public void Initialize()
        {
            this.gameObject.AddComponent<AudioSource>();
        }
    }
}
