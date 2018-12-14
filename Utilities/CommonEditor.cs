#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace YourCommonTools
{
	/******************************************
	 * 
	 * Class that handles states
	 * 
	 * @author Esteban Gallardo
	 */
	public class CommonEditor : Editor 
	{
        void OnEnable()
        {
            CommonOnEnable();
            EditorApplication.update += OnEditorApplicationUpdate;
        }

        void OnDisable()
        {
            CommonOnDisable();
            EditorApplication.update -= OnEditorApplicationUpdate;
        }

        public void BeginChangeComparision()
        {
            Undo.RecordObject(target, "Changes");
            EditorGUI.BeginChangeCheck();
        }


        public void EndChangeComparision()
        {
            serializedObject.ApplyModifiedProperties();
            if (!Application.isPlaying && EditorGUI.EndChangeCheck())
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        public override void OnInspectorGUI()
        {
            BeginChangeComparision();
            CommonOnInspectorGUI();
            EndChangeComparision();
        }


        private static int lastFontsize;
        private static FontStyle lastFontStyle;

        public static void StoreLastStyles()
        {
            lastFontsize = EditorStyles.label.fontSize;
            lastFontStyle = EditorStyles.label.fontStyle;
        }

        public static void RestoreLastStyles()
        {
            EditorStyles.label.fontStyle = lastFontStyle;
            EditorStyles.label.fontSize = lastFontsize;
        }

        public static void DrawSectionTitle(string _title, int _size = 12)
        {
            StoreLastStyles();
            EditorStyles.label.fontStyle = FontStyle.Bold;
            EditorStyles.label.fontSize = 12;
            EditorGUILayout.LabelField(_title);
            RestoreLastStyles();
        }

        public static void DrawEditorHint(string _hint, bool _spaceAfterHint = true)
        {
            StoreLastStyles();
            EditorStyles.label.fontStyle = FontStyle.Italic;
            EditorStyles.label.fontSize = 9;
            EditorGUILayout.LabelField(_hint);
            RestoreLastStyles();
            if (_spaceAfterHint)
                EditorGUILayout.Space();
        }

        protected void ArrayGUI(SerializedObject obj, string name)
        {
            int no = obj.FindProperty(name + ".Array.size").intValue;
            EditorGUI.indentLevel = 3;
            int c = EditorGUILayout.IntField("Size", no);
            if (c != no)
                obj.FindProperty(name + ".Array.size").intValue = c;

            for (int i = 0; i < no; i++)
            {
                var prop = obj.FindProperty(string.Format("{0}.Array.data[{1}]", name, i));
                EditorGUILayout.PropertyField(prop);
            }
            EditorGUI.indentLevel = 0;
        }

        public virtual void CommonOnEnable()
        {

        }
        public virtual void CommonOnDisable()
        {

        }

        public virtual void OnEditorApplicationUpdate()
        {

        }

        public virtual void CommonOnInspectorGUI()
        {

        }
    }
}
#endif