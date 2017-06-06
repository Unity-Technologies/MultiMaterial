using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CustomEditor(typeof(MultiMaterial), true)]
    public class MultiMaterialEditor : Editor
    {
        SerializedProperty m_MultiMaterialData;
        MultiMaterialDataEditor m_DataEditor;
        public void OnEnable()
        {
            m_MultiMaterialData = serializedObject.FindProperty(MultiMaterial.multiMaterialPub);
            //m_DataEditor = new MultiMaterialDataEditor();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("test");

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_MultiMaterialData, new GUIContent("data"));
            serializedObject.ApplyModifiedProperties();

            if (m_DataEditor == null)
            {
                EditorGUILayout.LabelField("m_DataEditor == null");
                if (m_MultiMaterialData != null && m_MultiMaterialData.objectReferenceValue != null)
                {
                    m_DataEditor = CreateEditor(m_MultiMaterialData.objectReferenceValue) as MultiMaterialDataEditor;
                }
            }
            else
            {
                EditorGUILayout.LabelField("m_DataEditor != null");
                if (m_MultiMaterialData == null || m_MultiMaterialData.objectReferenceValue == null || 
                m_DataEditor.target == null)
                {
                    DestroyImmediate(m_DataEditor);
                    m_DataEditor = null;
                }
                else if (m_MultiMaterialData.objectReferenceValue != m_DataEditor.target)
                {
                    DestroyImmediate(m_DataEditor);
                    m_DataEditor = null;
                }
                else
                {
                    EditorGUILayout.LabelField("got here");
                    m_DataEditor.OnInspectorGUI();
                }
            }

            base.OnInspectorGUI();
        }
    }
}
