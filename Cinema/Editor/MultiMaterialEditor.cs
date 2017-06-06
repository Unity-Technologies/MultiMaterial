using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CustomEditor(typeof(MultiMaterial), true)]
    public class MultiMaterialEditor : Editor
    {
//        SerializedProperty m_MultiMaterialData;
        MultiMaterialDataEditor m_DataEditor;
        bool m_EditorIsDirty;
        bool m_EditorIsReady;
        Color m_DarkWindow = new Color(0, 0, 0, 0.2f);
        public void OnEnable()
        {
//            m_MultiMaterialData = serializedObject.FindProperty(MultiMaterial.multiMaterialPub);
            m_EditorIsDirty = true;
            m_EditorIsReady = false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            base.OnInspectorGUI();
            //EditorGUILayout.PropertyField(m_MultiMaterialData, new GUIContent("data"));
            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel++;
            var helpRec = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.DrawRect(helpRec, m_DarkWindow);
            if (EditorGUI.EndChangeCheck())
            {
                m_EditorIsReady = RebuildEditor(true);
            }

            if (!m_EditorIsReady)
            {
                m_EditorIsReady = RebuildEditor(m_EditorIsDirty);
            }

            if (m_EditorIsReady)
            {
                m_DataEditor.OnInspectorGUI();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        
        bool RebuildEditor(bool forceRebuild = false)
        {
            if (forceRebuild)
            {
                m_EditorIsDirty = true;
            }

            if (!m_EditorIsDirty)
            {
                m_EditorIsDirty = !CheckEditor();
            }
            else
            {
                var multiMaterial = target as MultiMaterial;
                if (multiMaterial != null)
                {
                    if (m_DataEditor != null)
                    {
                        DestroyImmediate(m_DataEditor);
                    }

                    m_DataEditor = CreateEditor(multiMaterial.multiMaterialData) as MultiMaterialDataEditor;
                }
            }
            return m_EditorIsDirty && Event.current.type == EventType.Layout;
        }
        bool CheckEditor()
        {
            if (m_DataEditor == null)
                return false;

            var multiMaterial = target as MultiMaterial;
            if (multiMaterial == null || multiMaterial.multiMaterialData != null)
                return false;

            if (m_DataEditor.target as MultiMaterialData != multiMaterial.multiMaterialData)
                return false;

            return true;
        }
    }
}
