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

        MaterialEditor[] m_MaterialEditors;
        MaterialArray m_RendererMaterialArray;
        Renderer m_Renderer;

        public void OnEnable()
        {
            m_MaterialEditors = new MaterialEditor[] {};
            m_RendererMaterialArray = new MaterialArray();
            var multiMaterial = target as MultiMaterial;
            if (multiMaterial != null)
                m_Renderer = multiMaterial.gameObject.GetComponent<Renderer>();
            m_EditorIsReady = false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();

            var multiMaterial = target as MultiMaterial;
            var changed = EditorGUI.EndChangeCheck();

            if (multiMaterial.multiMaterialData == null)
            {
                if (m_Renderer == null)
                {
                    m_Renderer = multiMaterial.gameObject.GetComponent<Renderer>();
                }
                m_RendererMaterialArray.materials = m_Renderer.sharedMaterials;
//                EditorGUI.indentLevel++;
                //var helpRec = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                if (GUILayout.Button("Creat From Renderer"))
                {
                    var saveMultiMaterialData = new MultiMaterialData { materialArrayData = m_RendererMaterialArray };

                    var path = EditorUtility.SaveFilePanelInProject("Multi Material Data Save Window",
                        m_Renderer.gameObject.name + " Multi Material Data", "asset",
                        "Enter a file name to save the multi material data to");

                    if (!string.IsNullOrEmpty(path))
                    {
                        AssetDatabase.CreateAsset(saveMultiMaterialData, path);
                        AssetDatabase.Refresh();
                    }

                    var loadMultiMaterialData = AssetDatabase.LoadAssetAtPath<MultiMaterialData>(path);
                    if (loadMultiMaterialData != null)
                    {
                        serializedObject.Update();
                        var dataProp = serializedObject.FindProperty(MultiMaterial.multiMaterialPub);
                        dataProp.objectReferenceValue = loadMultiMaterialData;
                        serializedObject.ApplyModifiedProperties();
                    }

                }
                //EditorGUI.indentLevel--;
                //EditorGUI.DrawRect(helpRec, m_DarkWindow);
                MaterialArrayDrawers.OnInspectorGUI(serializedObject, m_RendererMaterialArray, ref m_MaterialEditors, changed);
                if (GUILayout.Button("Select Materials"))
                {
                    if (m_RendererMaterialArray != null && m_RendererMaterialArray.materials != null && m_RendererMaterialArray.materials.Length > 0)
                        Selection.objects = m_RendererMaterialArray.materials;
                }
                //EditorGUILayout.EndVertical();
//                EditorGUI.indentLevel--;
            }
            else
            {
                if (changed || !CheckEditor())
                {
                    m_EditorIsReady = RebuildEditor() && Event.current.type == EventType.Layout;
                }
                else
                {
                    m_EditorIsReady = true;
                }

//                EditorGUI.indentLevel++;
                var helpRec = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.DrawRect(helpRec, m_DarkWindow);
                if (m_EditorIsReady)
                {
                    m_DataEditor.OnInspectorGUI();
                }
                EditorGUILayout.EndVertical();
//                EditorGUI.indentLevel--;
            }
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
                
                if (m_DataEditor == null)
                {
                    var multiMaterial = target as MultiMaterial;
                    if (multiMaterial != null && multiMaterial.multiMaterialData != null)
                    {
                        m_DataEditor = CreateEditor(multiMaterial.multiMaterialData) as MultiMaterialDataEditor;
                    }
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

        void OnDestroy()
        {
            if (m_DataEditor != null)
            {
                DestroyImmediate(m_DataEditor);
                m_DataEditor = null;
            }
            if (m_MaterialEditors != null)
            {
                foreach (var materialEditor in m_MaterialEditors)
                {
                    DestroyImmediate(materialEditor);
                }
                m_MaterialEditors = null;
            }
        }
    }
}
