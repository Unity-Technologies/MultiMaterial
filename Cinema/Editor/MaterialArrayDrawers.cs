using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLabs.Cinema
{
    public static class MaterialArrayDrawers
    {
        public static string[] shaderNames = new string[0];
        public static GUIContent[] shaderNameGUIContents = new GUIContent[0];
        static GUIContent s_FoldoutPreDrop;
        static GUIContent s_Foldout;

        /// <summary>
        /// Draw the Multi Material Inspector GUI using Material Editors for each material in Material Array 
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="targetArray"></param>
        /// <param name="materialEditors"></param>
        /// <param name="changed"></param>
        /// <param name="MaterialProperties"></param>
        public static void OnInspectorGUI(SerializedObject serializedObject, 
            MaterialArray targetArray, ref MaterialEditor[] materialEditors, bool changed = false, SerializedProperty[] MaterialProperties = null)
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
                    if (targetArray.materials[i] != null &&  materialEditors[i] != null)
                    {
                        if (MaterialProperties != null)
                            OnMiniMaterialArrayHeaderGUI(serializedObject, ref materialEditors[i], targetArray, MaterialProperties[i]);
                        else
                            OnMiniMaterialArrayHeaderGUI(serializedObject, ref materialEditors[i], targetArray);
                        // Draw the Material Editor Body
                        if (materialEditors[i].isVisible)
                        {
                            EditorGUI.BeginChangeCheck();
                            if (GUILayout.Button("Sync to Material"))
                            {
                                MultiMaterialEditorUtilities.UpdateMaterials(targetArray, 
                                    materialEditors[i].target as Material, true);
                            }
                            materialEditors[i].OnInspectorGUI();

                            if (EditorGUI.EndChangeCheck())
                            {
                                MultiMaterialEditorUtilities.UpdateMaterials(targetArray, 
                                    materialEditors[i].target as Material);
                            }
                        }
                    }
                    else
                    {
                        if (MaterialProperties != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            MaterialProperties[i].serializedObject.Update();
                            EditorGUILayout.PropertyField(MaterialProperties[i], new GUIContent("Material"));
                            MaterialProperties[i].serializedObject.ApplyModifiedProperties();
                            if (EditorGUI.EndChangeCheck())
                            {
                                RebuildMaterialEditors(ref materialEditors, targetArray);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("IS NULL!!!");
                        }
                    }
                }
            }
        }

        public static void OnMaterialInspectorGUI(SerializedObject serializedObject, 
            MaterialArray targetArray, ref MaterialEditor materialEditor, Material material,
            bool changed = false)
        {
            if (material != null)
            {
                OnMiniMaterialArrayHeaderGUI(serializedObject, ref materialEditor, targetArray);
                // Draw the Material Editor Body
                if (materialEditor.isVisible)
                {
                    EditorGUI.BeginChangeCheck();
                    if (GUILayout.Button("Sync to Material"))
                    {
                        MultiMaterialEditorUtilities.UpdateMaterials(targetArray, 
                            materialEditor.target as Material, true);
                    }
                    materialEditor.OnInspectorGUI();

                    if (EditorGUI.EndChangeCheck())
                    {
                        MultiMaterialEditorUtilities.UpdateMaterials(targetArray, 
                            materialEditor.target as Material);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("IS NULL!!!");
            }
        }

        /// <summary>
        /// Rebuilds material editor if materials in editor do not match those in the material array 
        /// </summary>
        /// <param name="materialEditors"></param>
        /// <param name="targetArray"></param>
        /// <returns></returns>
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
                                    rebuildShaders = false;
                                }
                            }
                        }
                    }
                }
            }

            return CheckMaterialEditors(materialEditors, targetArray);
        }

        public static void FindMaterialArrayIcons()
        {
            s_FoldoutPreDrop = new GUIContent(EditorGUIUtility.IconContent("IN foldout"));
            s_Foldout = new GUIContent(EditorGUIUtility.IconContent("IN foldout on"));
        }

        /// <summary>
        /// Checks that Material Editors are valid
        /// </summary>
        /// <param name="materialEditors"></param>
        /// <param name="targetArray"></param>
        /// <returns></returns>
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
                if (targetArray.materials[i] != null)
                {
                    if (materialEditors[i] == null || (Material)materialEditors[i].target != targetArray.materials[i])
                    {
                        return false;
                    }
                }
                if (materialEditors[i] != null)
                {
                    if (targetArray.materials[i] == null || (Material)materialEditors[i].target != targetArray.materials[i])
                    {
                        return false;
                    }
                }
//                if (materialEditors[i] && targetArray.materials[i] != null)
//                {
//                    if (materialEditors[i] == null || materialEditors[i].target == null 
//                        || targetArray.materials[i] == null)
//                        return false;
//                    if ((Material)materialEditors[i].target != targetArray.materials[i])
//                        return false;
//                }
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

        /// <summary>
        /// Popup Shader selector that mimics the material shader popup in the material header
        /// </summary>
        /// <param name="material"></param>
        /// <param name="targetArray"></param>
        public static void ShaderPopup(Material material, MaterialArray targetArray)
        {
            var index = Array.FindIndex(shaderNames, s=> s == material.shader.name);
            // Have to use our own popup since you cannot use the material editor popup
            index = EditorGUILayout.Popup(index, shaderNameGUIContents);
            if (shaderNames[index] != material.shader.name)
            {
                var matSerial = new SerializedObject(material);
                matSerial.Update();

                var shaderSerial = matSerial.FindProperty("m_Shader");
                shaderSerial.objectReferenceValue = Shader.Find(shaderNames[index]);
                matSerial.ApplyModifiedProperties();

                MultiMaterialEditorUtilities.SetCheckMaterialShaders(targetArray, material);
            }
        }


        /// <summary>
        /// Draws a custom GUI that mimics the Material Editor Header.
        /// Need to use custom GUI since the normal Material Header does not respect all the Editor Gui functions 
        /// that can surround it and since we cannot detect changes in the Shader Foldout
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="materialEditor"></param>
        /// <param name="targetArray"></param>
        public static void OnMiniMaterialArrayHeaderGUI(SerializedObject serializedObject,
            ref MaterialEditor materialEditor, MaterialArray targetArray, SerializedProperty serializedMaterial = null)
        {
            if (materialEditor == null || !(materialEditor.target is Material))
                return;

            EditorGUILayout.BeginHorizontal(EditorStyles.textArea); // Begin Header Area
            if (s_Foldout == null || s_FoldoutPreDrop == null)
            {
                FindMaterialArrayIcons();
            }

            // Material Editor body is only drawn when ''m_IsVisible' == true
            // Normally set for Material Editor Header foldout
            // Need to be able to read and write to private field
            var isVisibleField = typeof(MaterialEditor).GetField("m_IsVisible", BindingFlags.NonPublic | BindingFlags.Instance);
            var isVisibleValue = isVisibleField.GetValue(materialEditor) as bool? ?? false;
            if (GUILayout.Button(isVisibleValue? s_Foldout: s_FoldoutPreDrop, GUIStyle.none, 
                GUILayout.ExpandWidth(false)))
            {
                isVisibleField.SetValue(materialEditor, !isVisibleValue);
            }

            var material = materialEditor.target as Material;
            var iconRect = EditorGUILayout.GetControlRect(false, 32, GUILayout.MaxWidth(32));

            OnHeaderIconGUI(ref materialEditor, iconRect);
            
            EditorGUILayout.BeginVertical(); // Begin Title and Shader Area
            if (serializedMaterial == null)
                EditorGUILayout.LabelField(new GUIContent(material.name));
            else
            {
                serializedMaterial.serializedObject.Update();
                EditorGUILayout.PropertyField(serializedMaterial, GUIContent.none);
                serializedMaterial.serializedObject.ApplyModifiedProperties();
            }
            ShaderPopup(material, targetArray);

            EditorGUILayout.EndVertical();  // End Title and Shader Area
            EditorGUILayout.EndHorizontal(); // End Header Area
        }

        static void OnHeaderIconGUI (ref MaterialEditor materialEditor, Rect iconRect)
        {
            Texture2D icon = null;
            if (!materialEditor.HasPreviewGUI ())
            {
                //  Fetch isLoadingAssetPreview to ensure that there is no situation where a preview needs a repaint because it hasn't finished loading yet.
                var isLoadingAssetPreview = AssetPreview.IsLoadingAssetPreview (materialEditor.target.GetInstanceID());
                icon = AssetPreview.GetAssetPreview (materialEditor.target);
                if (!icon)
                {
                    // We have a static preview it just hasn't been loaded yet. Repaint until we have it loaded.
                    if (isLoadingAssetPreview)
                        materialEditor.Repaint ();
                    icon = AssetPreview.GetMiniThumbnail (materialEditor.target);
                }
            }
        
            if (materialEditor.HasPreviewGUI ())
                // OnPreviewGUI must have all events; not just Repaint, or else the control IDs will mis-match.
                materialEditor.OnPreviewGUI (iconRect, GUIStyle.none);
            else if (icon)
                EditorGUI.DrawPreviewTexture(iconRect, icon);
        }
    }
}
