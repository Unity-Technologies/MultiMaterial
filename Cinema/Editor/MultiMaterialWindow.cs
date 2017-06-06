using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    public class MultiMaterialWindow : EditorWindow
    {
        MultiMaterialData m_MultiMaterialData;
        MaterialEditor m_ControlMaterialEditor;
        Material m_ContolMaterial;
        SerializedObject m_SerializedMultiMaterial;
        SerializedProperty m_MaterialArray;

        [MenuItem("+MaterialTools/Multi Material")]
        public static void Open()
        {
            var window = GetWindow(typeof(MultiMaterialWindow));
            const string windowTitle = "Multi Material";
            window.titleContent = new GUIContent(windowTitle);
            window.Show();
        }

        Vector2 m_ScrollView;
        void OnGUI()
        {
            m_ScrollView = EditorGUILayout.BeginScrollView(m_ScrollView);
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            m_MultiMaterialData = EditorGUILayout.ObjectField("MultiMaterialData: ",
                m_MultiMaterialData, typeof(MultiMaterialData), false) as MultiMaterialData;
            if (EditorGUI.EndChangeCheck())
            {
                if (m_ControlMaterialEditor != null)
                    DestroyImmediate(m_ControlMaterialEditor);
                m_ContolMaterial = null;
                m_SerializedMultiMaterial = null;
                m_MaterialArray = null;

            }
            EditorGUILayout.Separator();
            HandleContolMaterialDrawing();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            HandleMultiMaterilEditorDrawing();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Separator();
            if (m_ControlMaterialEditor != null)
            {
                m_ControlMaterialEditor.DefaultPreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.helpBox);
            }
        }


        void HandleContolMaterialDrawing()
        {
            if (m_MultiMaterialData == null)
            {
                if (m_ControlMaterialEditor != null)
                    DestroyImmediate(m_ControlMaterialEditor);
                m_ContolMaterial = null;
                return;
            }

            if (m_MultiMaterialData != null && m_ControlMaterialEditor != null)
            {
                EditorGUI.BeginChangeCheck();
                m_ControlMaterialEditor.DrawHeader();
                m_ControlMaterialEditor.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    MultiMaterialEditorUtilities.UpdateMaterials(m_MultiMaterialData, m_ControlMaterialEditor);
                }
            }
            else if (m_MultiMaterialData != null
                && m_MultiMaterialData.materialArray != null
                && m_MultiMaterialData.materialArray.Length > 0
                && m_MultiMaterialData.materialArray[0] != null)
            {
                if (m_ContolMaterial != m_MultiMaterialData.materialArray[0])
                {
                    if (m_ControlMaterialEditor != null)
                        DestroyImmediate(m_ControlMaterialEditor);
                    m_ContolMaterial = m_MultiMaterialData.materialArray[0];
                    m_ControlMaterialEditor = Editor.CreateEditor(m_ContolMaterial, typeof(MaterialEditor)) as MaterialEditor;
                }
            }
        }

        void HandleMultiMaterilEditorDrawing()
        {
            if (m_MultiMaterialData == null)
            {
                m_SerializedMultiMaterial = null;
                m_MaterialArray = null;
                return;
            }

            var propertiesChanged = false;

            if (m_SerializedMultiMaterial != null && m_MaterialArray != null)
            {
                m_SerializedMultiMaterial.Update();
                EditorGUILayout.PropertyField(m_MaterialArray, new GUIContent(""), true);
                propertiesChanged = m_SerializedMultiMaterial.ApplyModifiedProperties();
            }
            else
            {
                m_SerializedMultiMaterial = new SerializedObject(m_MultiMaterialData);
                m_MaterialArray = m_SerializedMultiMaterial.FindProperty("m_MaterialArray");
            }
            if (propertiesChanged)
            {
                //rebuild material editor array
                //BuildMaterialEditorArray();
            }
        }
    }


}
