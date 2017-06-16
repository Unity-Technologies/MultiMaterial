using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLabs.Cinema
{
    public class MaterialArrayDrawers
    {
        public static string[] shaderNames = new string[0];
        public static GUIContent[] shaderNameGUIContents = new GUIContent[0];
        public static void DrawInspectorGUI(SerializedObject serializedObject, 
            MaterialArray targetArray, ref MaterialEditor[] materialEditors, bool changed = false)
        {
            bool materialEditorReady;
            
            if (changed || !CheckMaterialEditors(materialEditors, targetArray))
            {
                materialEditorReady = RebuildMaterialEditors(ref materialEditors, targetArray) 
                    && Event.current.type == EventType.Layout;
            }
            else
            {
                materialEditorReady = true;
            }

            if (materialEditorReady)
            {
                for (var i = 0; i < materialEditors.Length; i++)
                {
                    // for some reason materialEditors[i] is not null here if materials[i] is nulled from select object
                    // popout selector but will register nulled in CheckMaterialEditors()
                    //if (materialEditors[i] != null)
                    if (targetArray.materials[i] != null)
                    {
                        DrawMaterialHeaderMaterialView(serializedObject, ref materialEditors[i], targetArray);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("IS NULL!!!");
                    }
                }
            }
        }

        /// <summary>
        /// Rebuilds material editor if materials in editor do not match those in the material array 
        /// </summary>
        /// <returns> true if materials rebuilt clean</returns>
        public static bool RebuildMaterialEditors(ref MaterialEditor[] materialEditors, MaterialArray targetArray)
        {
            if (!CheckMaterialEditors(materialEditors, targetArray))
            {
                if (materialEditors.Length > 0)
                {
                    for (var i = 0; i < materialEditors.Length; i++)
                    {
                        if (materialEditors[i] != null)
                        {
                            Object.DestroyImmediate(materialEditors[i]);
                            materialEditors[i] = null;
                        }
                    }
                    materialEditors = new MaterialEditor[0];
                }
                var rebuildShaders = true;
                if (materialEditors.Length == 0)
                {
                    if (targetArray != null && targetArray.materials!= null && targetArray.materials.Length > 0)
                    {
                        materialEditors = new MaterialEditor[targetArray.materials.Length];
                        for (var i = 0; i < targetArray.materials.Length; i++)
                        {
                            var material = targetArray.materials[i];
                            if (material != null)
                            {
                                materialEditors[i] = Editor.CreateEditor(material) as MaterialEditor;
                                if (rebuildShaders)
                                {
                                    UpdateShaderNames(material);
                                }
                            }
                        }
                    }
                }
            }

            return CheckMaterialEditors(materialEditors, targetArray);
        }

        public static bool CheckMaterialEditors(MaterialEditor[] materialEditors, MaterialArray targetArray)
        {
            if (materialEditors == null || materialEditors.Length < 1)
                return false;

            if (targetArray == null || targetArray.materials.Length < 1)
                return false;

            if (materialEditors.Length != targetArray.materials.Length)
                return false;
            for (var i = 0; i < targetArray.materials.Length; i++)
            {
                if (materialEditors[i] && targetArray.materials[i] != null)
                {
                    if (materialEditors[i] == null || materialEditors[i].target == null 
                        || targetArray.materials[i] == null)
                        return false;
                    if ((Material)materialEditors[i].target != targetArray.materials[i])
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Used to generate data for popup that mimics the material shader popup
        /// </summary>
        public static void UpdateShaderNames()
        {
            UpdateShaderNames(null);
        }

        /// <summary>
        /// Used to generate data for popup that mimics the material shader popup.
        /// Note that it will not find Unity Included Shaders that are not loaded.
        /// </summary>
        /// <param name="material"></param>
        public static void UpdateShaderNames(Material material)
        {
            if (material != null)
                UnityEditorInternal.InternalEditorUtility.SetupShaderMenu(material);

            var guids = AssetDatabase.FindAssets("t:Shader");
            var shaderList = new List<Shader>(guids.Select(s => AssetDatabase.LoadMainAssetAtPath(
                AssetDatabase.GUIDToAssetPath(s)) as Shader));
            shaderList.AddRange((Shader[])Resources.FindObjectsOfTypeAll(typeof(Shader)));
            shaderNames = shaderList.Select(n=>n.name).ToArray();

            // Filter out hidden shaders
            shaderNames = shaderNames.Where(s => !string.IsNullOrEmpty(s) && !s.Contains("__") && 
            !s.Contains("Hidden")).ToArray();
            shaderNameGUIContents = shaderNames.Select(s=> new GUIContent(s)).ToArray();
        }

        public static void DrawMaterialHeaderMaterialView(SerializedObject serializedObject, 
            ref MaterialEditor materialEditor, MaterialArray targetArray)
        {
            if (materialEditor == null || !(materialEditor.target is Material))
                return;

            // Material Editor body is only drawn when ''m_IsVisible' == true
            // Normaly set for Material Editor Header foldout
            // Need to be able to read and write to private field
            var isVisibleField = typeof(MaterialEditor).GetField("m_IsVisible", BindingFlags.NonPublic | BindingFlags.Instance);
            var isVisibleValue = isVisibleField.GetValue(materialEditor) as bool? ?? false;
            var material = materialEditor.target as Material;
            
            // We Draw a custom material editor like header since the normal Material Header does not respect all the 
            // Editor Gui functions that can surround it and since we cannot detect changes in the Shader Foldout
            EditorGUILayout.BeginHorizontal(EditorStyles.textArea); // Begin Header Area

            isVisibleValue = EditorGUILayout.Foldout(isVisibleValue, GUIContent.none);
            isVisibleField.SetValue(materialEditor, isVisibleValue);
            var layout = EditorGUILayout.GetControlRect(false, 36, GUILayout.MinWidth(40), GUILayout.MaxWidth(40));

            var imageRect = new Rect(layout.xMax-32, layout.y, 32, 32);
            var image = AssetPreview.GetAssetPreview(material);
            if (image != null)
            {
                EditorGUI.DrawPreviewTexture(imageRect, AssetPreview.GetAssetPreview(material));
            }
            
            
            EditorGUILayout.BeginVertical(); // Begin Title and Shader Area

            EditorGUILayout.LabelField(new GUIContent(material.name));


            var intdex = Array.FindIndex(shaderNames, s=> s == material.shader.name);

            // Have to use our own popup since you cannot use the material editor popup
            intdex = EditorGUILayout.Popup(intdex, shaderNameGUIContents);
//            intdex = EditorGUILayout.Popup(new GUIContent("Shader"), intdex, shaderNameGUIContents, EditorStyles.popup);
            if (shaderNames[intdex] != material.shader.name)
            {
                var matSerial = new SerializedObject(material);
                matSerial.Update();

                var shaderSerial = matSerial.FindProperty("m_Shader");
                shaderSerial.objectReferenceValue = Shader.Find(shaderNames[intdex]);
                matSerial.ApplyModifiedProperties();

                MultiMaterialEditorUtilities.SetCheckMaterialShaders(targetArray, material);
            }

            EditorGUILayout.EndVertical();  // End Title and Shader Area
            EditorGUILayout.EndHorizontal(); // End Header Area

            // Draw the Material Editor Body
            if (isVisibleValue)
            {
                EditorGUI.BeginChangeCheck();
                if (GUILayout.Button("Sync to Material"))
                {
                    MultiMaterialEditorUtilities.UpdateMaterials(targetArray, material, true);
                }
                materialEditor.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    MultiMaterialEditorUtilities.UpdateMaterials(targetArray, material);
                }
            }
        }

    }
}
