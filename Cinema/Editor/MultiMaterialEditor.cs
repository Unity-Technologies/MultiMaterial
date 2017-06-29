using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CustomEditor(typeof(MultiMaterial), true)]
    public class MultiMaterialEditor : Editor
    {
        [SerializeField]
        MaterialEditor[] m_MaterialEditors;

        [SerializeField]
        MaterialArray m_MaterialArray;

        [SerializeField]
        Renderer m_Renderer;

        [SerializeField]
        MaterialArray m_RendererMaterialArray;

        [SerializeField]
        SerializedProperty m_SerializedMaterials;

        [SerializeField]
        MultiMaterial m_MultiMaterial;

        [SerializeField]
        SerializedObject m_MultiMaterialData;

        [SerializeField]
        SerializedProperty[] m_MaterialProperties;

        [SerializeField]
        bool m_isDirty;


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
                m_isDirty = true;

            if (m_isDirty)
            {
                ValidateEditorData(useRenderer);
            }

            m_isDirty = EditorGUI.EndChangeCheck();

            if (useRenderer)
            {
                m_isDirty = CreateMultiMaterialDataButton(m_isDirty);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                m_MultiMaterialData.Update();
                m_SerializedMaterials.arraySize = EditorGUILayout.IntField("Size", m_SerializedMaterials.arraySize);
                m_MultiMaterialData.ApplyModifiedProperties();
                m_isDirty = m_isDirty || EditorGUI.EndChangeCheck();
            }

            if (m_isDirty)
                return;

            MaterialArrayDrawers.OnInspectorGUI(m_MultiMaterialData, m_MaterialArray, 
                ref m_MaterialEditors, m_isDirty, m_MaterialProperties);

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

                    m_isDirty = true;
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

            m_isDirty = false;
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
