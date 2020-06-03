#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YourCommonTools
{
    /******************************************
     * 
     * CustomButtonEditor
     * 
     * @author Esteban Gallardo
     */
    [CustomEditor(typeof(CustomButton))]
    [CanEditMultipleObjects]
    public class CustomButtonEditor : Editor
    {
        private CustomButton m_customButton;

        public void OnEnable()
        {
            m_customButton = (CustomButton)target;
        }

        private void DisplaySprites(Sprite[] _list)
        {
            for (int i = 0; i < _list.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _list[i] = (Sprite)EditorGUILayout.ObjectField("", _list[i], typeof(Sprite), true, GUILayout.Width(300));
                EditorGUILayout.EndHorizontal();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif