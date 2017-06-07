using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CustomEditor(typeof(MultiMaterialData))]
    public class MultiMaterialDataEditor : Editor
    {
        public SerializedProperty materialArray;

        MaterialEditor[] m_MaterialEditors;
        bool m_MultiEditorIsDirty;
        bool m_MaterialEditorReady;

        void OnEnable()
        {
            materialArray = serializedObject.FindProperty(MultiMaterialData.materialArrayPub);
            m_MaterialEditors = new MaterialEditor[] {};
            m_MultiEditorIsDirty = true;
            m_MaterialEditorReady = false;
        }

        
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(materialArray, new GUIContent("Multi Material"), true);
            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck() || !CheckMaterialEditors())
            {
                m_MaterialEditorReady = RebuildMaterialEditors() && Event.current.type == EventType.Layout;
            }
            else
            {
                m_MaterialEditorReady = true;
            }

            if (m_MaterialEditorReady)
            {
                for (var i = 0; i < m_MaterialEditors.Length; i++)
                {
                    var matEditor = m_MaterialEditors[i];
                    if (matEditor != null)
                    {
                        DrawMaterialHeader(matEditor, i);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("IS NULL!!!");
                    }
                }
            }


            if (GUILayout.Button("Add Selected"))
            {
                serializedObject.Update();
                var matHash = new HashSet<Material>();
                var multMat = target as MultiMaterialData;
                foreach (var mat in multMat.materialArray)
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
                multMat.materialArray = matHash.ToArray();
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Select Materials"))
            {
                var multMat = target as MultiMaterialData;
                if (multMat != null)
                {
                    Selection.objects = multMat.materialArray;
                }
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
                EditorGUILayout.LabelField("trying to draw");
                m_MaterialEditors[0].OnInteractivePreviewGUI(r, background);
            }
            else
            {
                base.OnInteractivePreviewGUI(r, background);
            }
        }


        /// <summary>
        /// Rebuilds material editor if materials in editor do not match those in the material array 
        /// </summary>
        /// <returns> true if materials rebuilt clean</returns>
        bool RebuildMaterialEditors()
        {
            if (!CheckMaterialEditors())
            {
                if (m_MaterialEditors.Length > 0)
                {
                    foreach (var matEditor in m_MaterialEditors)
                    {
                        if (matEditor != null)
                            DestroyImmediate(matEditor);
                    }
                    m_MaterialEditors = new MaterialEditor[0];
                }
                
                if (m_MaterialEditors.Length == 0)
                {
                    var matData = target as MultiMaterialData;
                    if (matData != null && matData.materialArray.Length > 0)
                    {
                        m_MaterialEditors = new MaterialEditor[matData.materialArray.Length];
                        for (var i = 0; i < matData.materialArray.Length; i++)
                        {
                            var material = matData.materialArray[i];
                            if (material != null)
                            {
                                m_MaterialEditors[i] = CreateEditor(material) as MaterialEditor;
                            }
                        }
                    }
                }
            }

            return CheckMaterialEditors();
        }

        bool CheckMaterialEditors()
        {
            if (m_MaterialEditors == null || m_MaterialEditors.Length < 1)
                return false;

            var matData = target as MultiMaterialData;
            if (matData == null || matData.materialArray.Length < 1)
                return false;

            if (m_MaterialEditors.Length != matData.materialArray.Length)
                return false;
            for (var i = 0; i < matData.materialArray.Length; i++)
            {
                if (m_MaterialEditors[i].target as Material != matData.materialArray[i])
                    return false;
            }

            return true;
        }

        void DrawMaterialHeader(MaterialEditor materialEditor, int index)
        {

//            EditorGUILayout.BeginVertical();
//            EditorGUILayout.BeginHorizontal();//GUILayout.MaxWidth(20));
//                EditorGUILayout.Toggle(true);
//            EditorGUILayout.EndHorizontal();
//            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            materialEditor.DrawHeader();
            if (materialEditor.isVisible)
            {
                var matData = target as MultiMaterialData;
                // shader property is drawn in header of material
                MultiMaterialEditorUtilities.SetCheckMaterialShaders(matData, materialEditor.target as Material); 
            }

//            EditorGUILayout.EndHorizontal();
            
            if (materialEditor.isVisible)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    var matData = target as MultiMaterialData;
                    MultiMaterialEditorUtilities.UpdateMaterials(matData, materialEditor);
                }
            }
//            else
//            {
//                EditorGUILayout.Space();
//            }
//            EditorGUILayout.EndVertical();

        }
    }
}
