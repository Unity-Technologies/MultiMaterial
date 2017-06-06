using System;
using System.Collections;
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
            materialArray = serializedObject.FindProperty("m_MaterialArray");
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

            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditorReady = RebuildMaterialEditors(true);
            }

            if (!m_MaterialEditorReady)
            {
                m_MaterialEditorReady = RebuildMaterialEditors(m_MultiEditorIsDirty);
            }

            if (m_MaterialEditorReady)
            {
                for (var i = 0; i < m_MaterialEditors.Length; i++)
                {
                    var matEditor = m_MaterialEditors[i];
                    if (matEditor != null)
                    {
                        //matEditor.DrawHeader();
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
//            if (HasPreviewGUI())
//            {
//                OnInteractivePreviewGUI(GUILayoutUtility.GetRect(128,128),new GUIStyle());
//            }
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
        /// or if 'forceRebuild' is true.
        /// </summary>
        /// <param name="forceRebuild"></param>
        /// <returns> true if materials rebuilt clean and in UI Layout Event</returns>
        bool RebuildMaterialEditors(bool forceRebuild = false)
        {
            if (forceRebuild)
            {
                m_MultiEditorIsDirty = true;
            }

            if (!m_MultiEditorIsDirty)
            {
                m_MultiEditorIsDirty = !CheckMaterialEditors();
            }
            else
            { 
                // TODO see if can reuse materialEditors before destroying them -JN
                var matData = target as MultiMaterialData;
                if (matData != null)
                {
                    if (m_MaterialEditors != null && m_MaterialEditors.Length > 0)
                    {
                        foreach (var matEditor in m_MaterialEditors)
                        {
                            if (matEditor != null)
                                DestroyImmediate(matEditor);
                        }
                    }

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

            return m_MultiEditorIsDirty && Event.current.type == EventType.Layout;
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

        bool m_OpenEditorChanged;
        int m_OpenEditor;
        MaterialEditor m_CurrentEditor;
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
