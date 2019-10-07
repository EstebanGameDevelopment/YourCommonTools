#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using System.Reflection;

namespace YourCommonTools
{
    /******************************************
	 * 
	 * Class with a collection of generic functionalities
	 * 
	 * @author Esteban Gallardo
	 */
    public static class UtilitiesEditor
    {
        static MethodInfo _clearConsoleMethod;
        static MethodInfo clearConsoleMethod
        {
            get
            {
                if (_clearConsoleMethod == null)
                {
                    Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                    Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                    _clearConsoleMethod = logEntries.GetMethod("Clear");
                }
                return _clearConsoleMethod;
            }
        }

        [MenuItem("Tools/Clear Console %#c")] // CMD + SHIFT + C
        public static void ClearLogConsole()
        {
            clearConsoleMethod.Invoke(new object(), null);
        }

        private static int m_LastFontsize;
        private static FontStyle m_LastFontStyle;

        public static void StoreLastStyles()
        {
            m_LastFontsize = EditorStyles.label.fontSize;
            m_LastFontStyle = EditorStyles.label.fontStyle;
        }

        public static void RestoreLastStyles()
        {
            EditorStyles.label.fontStyle = m_LastFontStyle;
            EditorStyles.label.fontSize = m_LastFontsize;
        }

        public static void DrawSectionTitle(string _title)
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
    }
}
#endif
