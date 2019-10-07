using UnityEngine;
using System.Collections;

namespace YourCommonTools
{
    public class FPSDisplay : MonoBehaviour
    {
        float deltaTime = 0.0f;

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------	
        private static FPSDisplay _instance;

        public static FPSDisplay Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = GameObject.FindObjectOfType(typeof(FPSDisplay)) as FPSDisplay;
                    if (!_instance)
                    {
                        GameObject container = new GameObject();
                        DontDestroyOnLoad(container);
                        container.name = "FPSDisplay";
                        _instance = container.AddComponent(typeof(FPSDisplay)) as FPSDisplay;
                    }
                }
                return _instance;
            }
        }

        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }

        void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            float heightLabel = h * 2 / 50;
            Rect rect = new Rect(0, h - heightLabel, w, heightLabel);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = (int)(heightLabel - 2);
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }
}