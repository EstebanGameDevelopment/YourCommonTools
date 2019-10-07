#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YourCommonTools
{
    /******************************************
     * 
     * BaseCustomEditor
     * 
     * @author Esteban Gallardo
     */
    public abstract class BaseCustomEditor : Editor
    {
        public abstract void OnEditorUI();
        public abstract void OnRuntimeUI();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Application.isPlaying)
                OnRuntimeUI();
            else
                OnEditorUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif