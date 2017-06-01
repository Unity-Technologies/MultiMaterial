using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    public class MultiMaterialWindow : EditorWindow
    {
        MultiMaterial m_MultiMaterial;
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
            m_MultiMaterial = EditorGUILayout.ObjectField("MultiMaterial: ",
                m_MultiMaterial, typeof(MultiMaterial), false) as MultiMaterial;
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


        void UpdateMaterials()
        {
            if (m_MultiMaterial.materialArray.Length < 1 && m_ControlMaterialEditor == null)
            {
                // help box goes here to tell you to assign control material to first item in array
                return;
            }
            SetCheckMaterialShaders();
            var controlMats = new Material[] { m_MultiMaterial.materialArray[0] };
            var controlProperties = MaterialEditor.GetMaterialProperties(controlMats);
            for (var i = 1; i < m_MultiMaterial.materialArray.Length; i++)
            {
                if (m_MultiMaterial.materialArray[i] != null)
                {
                    foreach (var controlProperty in controlProperties)
                    {
                        switch (controlProperty.type)
                        {
                            case MaterialProperty.PropType.Color:
                                m_MultiMaterial.materialArray[i].SetColor(controlProperty.name, controlProperty.colorValue);
                                break;
                            case MaterialProperty.PropType.Float:
                                m_MultiMaterial.materialArray[i].SetFloat(controlProperty.name, controlProperty.floatValue);
                                break;
                            case MaterialProperty.PropType.Range:
                                goto case MaterialProperty.PropType.Float;
                            case MaterialProperty.PropType.Vector:
                                m_MultiMaterial.materialArray[i].SetVector(controlProperty.name, controlProperty.vectorValue);
                                break;
                            case MaterialProperty.PropType.Texture:
                                // skipping texture set since used for udim mapping
                                break;
                        }
                    }
                }
            }

        }

        void SetCheckMaterialShaders()
        {
            foreach (var material in m_MultiMaterial.materialArray)
            {
                if (material.shader != m_MultiMaterial.materialArray[0].shader)
                {
                    material.shader = m_MultiMaterial.materialArray[0].shader;
                }
            }
        }

        void HandleContolMaterialDrawing()
        {
            if (m_MultiMaterial == null)
            {
                if (m_ControlMaterialEditor != null)
                    DestroyImmediate(m_ControlMaterialEditor);
                m_ContolMaterial = null;
                return;
            }

            if (m_MultiMaterial != null && m_ControlMaterialEditor != null)
            {
                EditorGUI.BeginChangeCheck();
                m_ControlMaterialEditor.DrawHeader();
                m_ControlMaterialEditor.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateMaterials();
                }
            }
            else if (m_MultiMaterial != null
                && m_MultiMaterial.materialArray != null
                && m_MultiMaterial.materialArray.Length > 0
                && m_MultiMaterial.materialArray[0] != null)
            {
                if (m_ContolMaterial != m_MultiMaterial.materialArray[0])
                {
                    if (m_ControlMaterialEditor != null)
                        DestroyImmediate(m_ControlMaterialEditor);
                    m_ContolMaterial = m_MultiMaterial.materialArray[0];
                    m_ControlMaterialEditor = Editor.CreateEditor(m_ContolMaterial, typeof(MaterialEditor)) as MaterialEditor;
                }
            }
        }

        void HandleMultiMaterilEditorDrawing()
        {
            if (m_MultiMaterial == null)
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
                m_SerializedMultiMaterial = new SerializedObject(m_MultiMaterial);
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
