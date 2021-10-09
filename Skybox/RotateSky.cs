using UnityEngine;
using System.Collections;

namespace YourCommonTools
{
    public class RotateSky : MonoBehaviour
    {
        public float RotateSpeed = 0.8f;

        void Update()
        {
            RenderSettings.skybox.SetFloat("_Rotation", Time.time * RotateSpeed);
        }
    }
}