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
        MaterialArray m_RendererMaterialArray;
        [SerializeField]
        Renderer m_Renderer;

        [SerializeField]
        SerializedProperty m_SerializedMaterials;

        [SerializeField]
        MultiMaterial m_MultiMaterial;
        [SerializeField]
        SerializedObject m_MultiMaterialData;


        public void OnEnable()
        {
            m_MaterialEditors = new MaterialEditor[] {};
            m_RendererMaterialArray = new MaterialArray();
            m_MultiMaterial = target as MultiMaterial;
            if (m_MultiMaterial != null)
            {
                m_Renderer = m_MultiMaterial.gameObject.GetComponent<Renderer>();
                m_MultiMaterialData = new SerializedObject(m_MultiMaterial.multiMaterialData);

                m_SerializedMaterials = m_MultiMaterialData.FindProperty(string.Format("{0}.{1}", 
                    MultiMaterialData.materialArrayPub, MaterialArray.materialsPub));
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            base.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties();

            var changed = EditorGUI.EndChangeCheck();

            if (m_MultiMaterial.multiMaterialData == null)
            {
                if (m_Renderer == null)
                {
                    m_Renderer = m_MultiMaterial.gameObject.GetComponent<Renderer>();
                }
                m_RendererMaterialArray.materials = m_Renderer.sharedMaterials;

                changed = CreateMultiMaterialDataButton(changed);

                MaterialArrayDrawers.OnInspectorGUI(serializedObject, m_RendererMaterialArray, ref m_MaterialEditors, 
                    changed);   
            }
            else
            {
                if (changed || m_MultiMaterialData == null)
                {
                    m_MultiMaterialData = new SerializedObject(m_MultiMaterial.multiMaterialData);
                    m_SerializedMaterials = m_MultiMaterialData.FindProperty(string.Format("{0}.{1}", 
                        MultiMaterialData.materialArrayPub, MaterialArray.materialsPub));
                }

                EditorGUI.BeginChangeCheck();
                m_MultiMaterialData.Update();
                var materialPropList = new List<SerializedProperty>();
                m_SerializedMaterials.arraySize = EditorGUILayout.IntField("Size", m_SerializedMaterials.arraySize);
                for (var i = 0; i < m_SerializedMaterials.arraySize; ++i)
                {
                    materialPropList.Add(m_SerializedMaterials.GetArrayElementAtIndex(i));
                }
                var materialProperties = materialPropList.ToArray();
                m_MultiMaterialData.ApplyModifiedProperties();
                changed = changed || EditorGUI.EndChangeCheck();

                MaterialArrayDrawers.OnInspectorGUI(m_MultiMaterialData, m_MultiMaterial.materialArray, 
                    ref m_MaterialEditors, changed, materialProperties); 

                if (GUILayout.Button("Add Selected"))
                {
                    m_MultiMaterialData.Update();
                    var matHash = new HashSet<Material>();
                    if (m_MultiMaterial.materialArray.materials.Length > 0)
                    {
                        foreach (var mat in m_MultiMaterial.materialArray.materials)
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
                }
            }

            if (GUILayout.Button("Select Materials"))
            {
                if (m_RendererMaterialArray != null && m_RendererMaterialArray.materials != null &&
                    m_RendererMaterialArray.materials.Length > 0)
                {
                    Selection.objects = m_RendererMaterialArray.materials;
                }
            }

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
