using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs
{
    [CustomEditor(typeof(MultiMaterial), true)]
    public class MultiMaterialEditor : Editor
    {
        MaterialEditor[] m_MaterialEditors;
        MaterialArray m_MaterialArray;
        Renderer m_Renderer;
        MaterialArray m_RendererMaterialArray;
        SerializedProperty m_SerializedMaterials;
        MultiMaterial m_MultiMaterial;
        SerializedObject m_MultiMaterialData;
        SerializedProperty[] m_MaterialProperties;
        bool m_SetDirty;

        public void OnEnable()
        {
            m_MaterialEditors = new MaterialEditor[] {};
            m_RendererMaterialArray = new MaterialArray();
            m_MultiMaterial = target as MultiMaterial;
            if (m_MultiMaterial != null)
            {
                ValidateEditorData(m_MultiMaterial.multiMaterialData == null);
            }
        }

        void ValidateEditorData(bool useRenderer)
        {
            m_MultiMaterial = target as MultiMaterial;
            m_Renderer = m_MultiMaterial.gameObject.GetComponent<Renderer>();
            m_RendererMaterialArray.materials = m_Renderer.sharedMaterials;
                
            m_MultiMaterialData = useRenderer? serializedObject : new SerializedObject(m_MultiMaterial.multiMaterialData);
            m_MaterialArray = useRenderer? m_RendererMaterialArray : m_MultiMaterial.materialArray;
            if (useRenderer)
            {
                m_MaterialProperties = null;
            }
            else
            {
                m_SerializedMaterials = m_MultiMaterialData.FindProperty(string.Format("{0}.{1}", 
                    MultiMaterialData.materialArrayPub, MaterialArray.materialsPub));

                var materialPropList = new List<SerializedProperty>();
                for (var i = 0; i < m_SerializedMaterials.arraySize; ++i)
                {
                    materialPropList.Add(m_SerializedMaterials.GetArrayElementAtIndex(i));
                }
                m_MaterialProperties = materialPropList.ToArray();
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();

            var useRenderer = m_MultiMaterial.multiMaterialData == null;

            if (!useRenderer && m_SerializedMaterials == null)
                m_SetDirty = true;

            if (m_SetDirty)
            {
                ValidateEditorData(useRenderer);
            }

            m_SetDirty = EditorGUI.EndChangeCheck();

            if (useRenderer)
            {
                m_SetDirty = CreateMultiMaterialDataButton(m_SetDirty);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                m_MultiMaterialData.Update();
                m_SerializedMaterials.arraySize = EditorGUILayout.DelayedIntField("Size", m_SerializedMaterials.arraySize);
                m_MultiMaterialData.ApplyModifiedProperties();
                m_SetDirty = m_SetDirty || EditorGUI.EndChangeCheck();
            }

            if (m_SetDirty)
                return;

            MaterialArrayDrawers.OnInspectorGUI(m_MultiMaterialData, m_MaterialArray, 
                ref m_MaterialEditors, m_SetDirty, m_MaterialProperties);

            if (!useRenderer)
            {
                if (GUILayout.Button("Add Selected"))
                {
                    m_MultiMaterialData.Update();
                    var matHash = new HashSet<Material>();
                    if (m_MaterialArray.materials.Length > 0)
                    {
                        foreach (var mat in m_MaterialArray.materials)
                        {
                            matHash.Add(mat);
                        }
                    }
                    foreach (var obj in Selection.objects)
                    {
                        var mat = obj as Material;
                        if (mat != null)
                        {
                            matHash.Add(mat);
                        }
                    }
                    m_MultiMaterial.materialArray.materials = matHash.ToArray();

                    m_MultiMaterialData.ApplyModifiedProperties();

                    m_SetDirty = true;
                    return;
                }
            }
            
            if (GUILayout.Button("Select Materials"))
            {
                if (m_MaterialArray != null && m_MaterialArray.materials != null &&
                    m_MaterialArray.materials.Length > 0)
                {
                    Selection.objects = m_MaterialArray.materials;
                }
            }

            m_SetDirty = false;
        }

        bool CreateMultiMaterialDataButton(bool changed)
        {
            if (GUILayout.Button("Create From Renderer"))
            {
                var saveMultiMaterialData = CreateInstance<MultiMaterialData>();
                saveMultiMaterialData.materialArrayData = m_RendererMaterialArray;

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
                    var dataProp = serializedObject.FindProperty(MultiMaterial.multiMaterialDataPub);
                    dataProp.objectReferenceValue = loadMultiMaterialData;
                    serializedObject.ApplyModifiedProperties();
                }
                return true;
            }
            return changed;
        }
        
    }
}
