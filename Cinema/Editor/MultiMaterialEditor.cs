using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CustomEditor(typeof(MultiMaterial), true)]
    public class MultiMaterialEditor : Editor
    {
//        SerializedProperty m_MultiMaterialData;
        MultiMaterialDataEditor m_DataEditor;
        bool m_EditorIsReady;
        Color m_DarkWindow = new Color(0, 0, 0, 0.2f);
        public void OnEnable()
        {
//            m_MultiMaterialData = serializedObject.FindProperty(MultiMaterial.multiMaterialPub);
            m_EditorIsReady = false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            base.OnInspectorGUI();
            //EditorGUILayout.PropertyField(m_MultiMaterialData, new GUIContent("data"));
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck() || !CheckEditor())
            {
                m_EditorIsReady = RebuildEditor() && Event.current.type == EventType.Layout;
            }
            else
            {
                m_EditorIsReady = true;
            }

            EditorGUI.indentLevel++;
            var helpRec = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.DrawRect(helpRec, m_DarkWindow);
            if (m_EditorIsReady)
            {
                m_DataEditor.OnInspectorGUI();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
        
        bool RebuildEditor()
        {
            if (!CheckEditor())
            {
                if (m_DataEditor != null)
                {
                    DestroyImmediate(m_DataEditor);
                    m_DataEditor = null;
                }
                var multiMaterial = target as MultiMaterial;
                if (m_DataEditor == null && multiMaterial != null && multiMaterial.multiMaterialData != null)
                {
                  
                    m_DataEditor = CreateEditor(multiMaterial.multiMaterialData) as MultiMaterialDataEditor;
                }
            }
            return CheckEditor();
        }

        bool CheckEditor()
        {
            if (m_DataEditor == null)
                return false;

            var multiMaterial = target as MultiMaterial;
            if (multiMaterial == null || multiMaterial.multiMaterialData == null)
                return false;

            return m_DataEditor.target as MultiMaterialData == multiMaterial.multiMaterialData;
        }
    }
}
