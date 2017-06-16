using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CustomEditor(typeof(MultiMaterialData))]
    public class MultiMaterialDataEditor : Editor
    {
        public SerializedProperty multiMaterialData;
        public SerializedProperty materialArray;

        MaterialEditor[] m_MaterialEditors;

        void OnEnable()
        {
            multiMaterialData = serializedObject.FindProperty(MultiMaterialData.materialArrayDataPub);
            materialArray = multiMaterialData.FindPropertyRelative(MaterialArray.materialArrayPub);
            m_MaterialEditors = new MaterialEditor[] {};
			MaterialArrayDrawers.UpdateShaderNames();
        }

        
        public override void OnInspectorGUI()
        {
            var targetData = target as MultiMaterialData;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(materialArray, new GUIContent("Multi Material"), true);
            serializedObject.ApplyModifiedProperties();
            var changed = EditorGUI.EndChangeCheck();

            MaterialArrayDrawers.DrawInspectorGUI(serializedObject, 
                targetData.materialArrayData, ref m_MaterialEditors, changed);

            var targetArray = targetData.materialArrayData;

            if (GUILayout.Button("Add Selected"))
            {
                serializedObject.Update();
                var matHash = new HashSet<Material>();
                foreach (var mat in targetArray.materials)
                {
                    matHash.Add(mat);
                }
                foreach (var obj in Selection.objects)
                {
                    var mat = obj as Material;
                    if (mat != null)
                    {
                        matHash.Add(mat);
                    }
                }
                targetArray.materials = matHash.ToArray();

                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Select Materials"))
            {
                if (targetArray != null && targetArray.materials != null && targetArray.materials.Length > 0)
                    Selection.objects = targetArray.materials;
            }
        }

        public override bool HasPreviewGUI()
        {
            if (m_MaterialEditors != null && m_MaterialEditors.Length > 0 && m_MaterialEditors[0] != null)
            {
                return true;
            }
            else
            {
                return base.HasPreviewGUI();
            }
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (m_MaterialEditors != null && m_MaterialEditors.Length > 0 && m_MaterialEditors[0] != null)
            {
                HasPreviewGUI();
                m_MaterialEditors[0].OnInteractivePreviewGUI(r, background);
            }
            else
            {
                base.OnInteractivePreviewGUI(r, background);
            }
        }

        void OnDestroy()
        {
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
