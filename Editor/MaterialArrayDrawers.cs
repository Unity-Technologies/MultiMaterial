using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnityLabs
{
    public static class MaterialArrayDrawers
    {
        public static string[] shaderNames;
        public static GUIContent[] shaderNameGUIContents;
        static GUIContent s_FoldoutPreDrop;
        static GUIContent s_Foldout;
        static GUIStyle s_RichTextStyle = new GUIStyle { richText = true };

        const string k_NullMaterialWarning = "<b><color=#ffffffff>Material is Null</color></b>";
        const int k_IconSize = 32;
        const string k_DefaultMaterial = "Default-Material";

        /// <summary>
        /// Draw the Multi Material Inspector GUI using Material Editors for each material in Material Array 
        /// </summary>
        /// <param name="serializedObject">Target serialized object from the inspector</param>
        /// <param name="materialArray">Material array to be used in inspector</param>
        /// <param name="materialEditors">Material Editors for each material in materialArray</param>
        /// <param name="changed">Editor property changed from outside of this method</param>
        /// <param name="materialProperties">Array of serialized properties that are the materials in the Material 
        /// Array. Used for property drawer in material header</param>
        public static void OnInspectorGUI(SerializedObject serializedObject, MaterialArray materialArray, 
            ref MaterialEditor[] materialEditors, bool changed = false, SerializedProperty[] materialProperties = null)
        {
            var materialEditorReady = true;

            if (changed || !CheckMaterialEditors(materialEditors, materialArray))
            {
                materialEditorReady = RebuildMaterialEditors(ref materialEditors, materialArray) 
                    && Event.current.type == EventType.Layout;
            }

            if (materialEditorReady)
            {
                for (var i = 0; i < materialEditors.Length; i++)
                {
                    if (materialArray.materials[i] != null && materialEditors[i] != null)
                    {
                        var material = materialEditors[i].target as Material;
                        
                        OnMiniMaterialArrayHeaderGUI(serializedObject, ref materialEditors[i], materialArray, 
                            materialProperties != null && materialProperties.Length > i && 
                            materialProperties[i] != null? materialProperties[i] : null);

                        EditorGUI.BeginDisabledGroup(material != null && material.name == k_DefaultMaterial);
                        // Draw the Material Editor Body
                        if (materialEditors[i].isVisible)
                        {
                            EditorGUI.BeginChangeCheck();
                            if (GUILayout.Button("Sync to Material"))
                            {
                                MultiMaterialEditorUtilities.UpdateMaterials(materialArray, material, true);
                            }
                            materialEditors[i].OnInspectorGUI();

                            if (EditorGUI.EndChangeCheck())
                            {
                                MultiMaterialEditorUtilities.UpdateMaterials(materialArray, material);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        if (materialProperties != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            materialProperties[i].serializedObject.Update();
                            EditorGUILayout.PropertyField(materialProperties[i], new GUIContent("Material"));
                            materialProperties[i].serializedObject.ApplyModifiedProperties();
                            if (EditorGUI.EndChangeCheck())
                            {
                                RebuildMaterialEditors(ref materialEditors, materialArray);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(k_NullMaterialWarning, s_RichTextStyle);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws a custom gui that mimics the Material Editor Header.
        /// We need to use custom gui since the normal Material Header does not respect all the editor gui functions 
        /// that can surround it and since we cannot detect changes in the Shader Foldout
        /// </summary>
        /// <param name="serializedObject">Target serialized object from the inspector</param>
        /// <param name="materialEditor">Material Editors for each material in materialArray</param>
        /// <param name="materialArray">Material array to be used in inspector</param>
        /// <param name="serializedMaterial">Serialized property that represents the material field in the header 
        /// If null the material name is drawn in place of the field.</param>
        public static void OnMiniMaterialArrayHeaderGUI(SerializedObject serializedObject,
            ref MaterialEditor materialEditor, MaterialArray materialArray, 
            SerializedProperty serializedMaterial = null)
        {
            if (materialEditor == null || !(materialEditor.target is Material))
                return;

            EditorGUILayout.BeginHorizontal(EditorStyles.textArea); // Begin Header Area
            if (s_Foldout == null || s_FoldoutPreDrop == null)
            {
                FindMaterialArrayIcons();
            }

            // Material Editor body is only drawn when 'm_IsVisible' == true
            // this is normally set in the Material Editor Inspector's Header with the foldout
            // We need to be able to read and write to private field to see the material editor body.
            var isVisibleField = typeof(MaterialEditor).GetField("m_IsVisible", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var isVisibleValue = isVisibleField.GetValue(materialEditor) as bool? ?? false;
            if (GUILayout.Button(isVisibleValue? s_Foldout: s_FoldoutPreDrop, GUIStyle.none, 
                GUILayout.ExpandWidth(false)))
            {
                isVisibleField.SetValue(materialEditor, !isVisibleValue);
            }

            var material = (Material)materialEditor.target;
            var iconRect = EditorGUILayout.GetControlRect(false, k_IconSize, GUILayout.MaxWidth(k_IconSize));

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

            EditorGUI.BeginDisabledGroup(material != null && material.name == k_DefaultMaterial);
            ShaderPopup(material, materialArray);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();  // End Title and Shader Area
            EditorGUILayout.EndHorizontal(); // End Header Area
        }

        /// <summary>
        /// Rebuilds material editor if materials in editor do not match those in the material array 
        /// </summary>
        /// <param name="materialEditors">Material Editors for each material in materialArray</param>
        /// <param name="materialArray">Material array to be used in inspector</param>
        /// <returns></returns>
        public static bool RebuildMaterialEditors(ref MaterialEditor[] materialEditors, MaterialArray materialArray)
        {
            // If check fails try to rebuild editors then recheck
            if (!CheckMaterialEditors(materialEditors, materialArray))
            {
                if (materialEditors != null)
                {
                    for (var i = 0; i < materialEditors.Length; i++)
                    {
                        if (materialEditors[i] == null)
                            continue;

                        UnityObject.DestroyImmediate(materialEditors[i]);
                        materialEditors[i] = null;
                    }
                }

                var rebuildShaders = true;

                if (materialArray != null && materialArray.materials != null && materialArray.materials.Length > 0)
                {
                    materialEditors = new MaterialEditor[materialArray.materials.Length];
                    for (var i = 0; i < materialArray.materials.Length; i++)
                    {
                        var material = materialArray.materials[i];
                        if (material == null)
                            continue;

                        materialEditors[i] = Editor.CreateEditor(material) as MaterialEditor;
                        if (!rebuildShaders)
                            continue;

                        UpdateShaderNames(material);
                        rebuildShaders = false;
                    }
                }
                else
                {
                    materialEditors = new MaterialEditor[0];
                }
                // Need to try and recheck after rebuild to avoid change in gui between layout and repaint
                return CheckMaterialEditors(materialEditors, materialArray);
            }
            return true;
        }

        /// <summary>
        /// Used to find icons for foldout button.
        /// </summary>
        public static void FindMaterialArrayIcons()
        {
            s_FoldoutPreDrop = new GUIContent(EditorGUIUtility.IconContent("IN foldout"));
            s_Foldout = new GUIContent(EditorGUIUtility.IconContent("IN foldout on"));
        }

        /// <summary>
        /// Checks that the individual Material Editor's material in the 'materialEditors' array matches the material 
        /// at the corresponding index in the 'materialArray'
        /// </summary>
        /// <param name="materialEditors">Material Editor Array that will be checked against 'materialArray'</param>
        /// <param name="materialArray">Material Array that the 'materialEditors' should match</param>
        /// <returns></returns>
        public static bool CheckMaterialEditors(MaterialEditor[] materialEditors, MaterialArray materialArray)
        {
            if (materialEditors == null || materialEditors.Length < 1)
                return false;

            if (materialArray == null || materialArray.materials.Length < 1)
                return false;

            if (materialEditors.Length != materialArray.materials.Length)
                return false;

            for (var i = 0; i < materialArray.materials.Length; i++)
            {
                if (materialArray.materials[i] == null && materialEditors[i] == null)
                    continue;

                if (materialArray.materials[i] != null && materialEditors[i] == null 
                    || materialEditors[i] != null && materialArray.materials[i] == null 
                    || (Material)materialEditors[i].target != materialArray.materials[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Used to generate data for popup that mimics the material shader popup.
        /// Note that it will not find Unity Included Shaders that are not loaded.
        /// </summary>
        /// <param name="material">Material used to set up Shader Menu.</param>
        public static void UpdateShaderNames(Material material = null)
        {
            var guids = AssetDatabase.FindAssets("t:Shader");
            var shaderList = new List<Shader>(guids.Select(s => AssetDatabase.LoadMainAssetAtPath(
                AssetDatabase.GUIDToAssetPath(s)) as Shader));
            shaderList.AddRange((Shader[])Resources.FindObjectsOfTypeAll(typeof(Shader)));
            shaderNames = shaderList.Select(n => n.name).ToArray();

            // Filter out shaders marked as 'Hidden' and auto generated internal sub shaders
            shaderNames = shaderNames.Where(s => !String.IsNullOrEmpty(s) && !s.Contains("__") && 
            !s.Contains("Hidden")).ToArray();
            shaderNameGUIContents = shaderNames.Select(s => new GUIContent(s)).ToArray();
        }

        /// <summary>
        /// Popup Shader selector that mimics the material shader popup in the material header
        /// </summary>
        /// <param name="material">The material that is the parent of the shader for popup.</param>
        /// <param name="materialArray">Material array the shader popup operates on.</param>
        public static void ShaderPopup(Material material, MaterialArray materialArray)
        {
            var index = Array.FindIndex(shaderNames, s => s == material.shader.name);
            // Have to use our own popup since you cannot use the material editor popup
            if (index < 0 || index > shaderNames.Length)
            {
                UpdateShaderNames(material);
                EditorGUILayout.Popup(index, shaderNameGUIContents);
                return;
            }
            index = EditorGUILayout.Popup(index, shaderNameGUIContents);
            if (shaderNames[index] != material.shader.name)
            {
                var matSerial = new SerializedObject(material);
                matSerial.Update();

                var shaderSerial = matSerial.FindProperty("m_Shader");
                shaderSerial.objectReferenceValue = Shader.Find(shaderNames[index]);
                matSerial.ApplyModifiedProperties();

                MultiMaterialEditorUtilities.SetCheckMaterialShaders(materialArray, material);
            }
        }

        public static bool AddSelectedButtons(SerializedObject serializedObject, MaterialArray materialArray)
        {
            EditorGUILayout.BeginVertical();
            var changed = AddSelectedButton(serializedObject, materialArray, "Add Selected", false, false);
            EditorGUILayout.BeginHorizontal();
            changed = AddSelectedButton(serializedObject, materialArray, "Include Children", true, false) || changed;
            changed = AddSelectedButton(serializedObject, materialArray, "Include Inactive", true, true) || changed;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return changed;
        }

        public static bool AddSelectedButton(SerializedObject serializedObject, MaterialArray materialArray, 
            string text, bool includeChildren, bool includeInactive)
        {
            if (GUILayout.Button(text))
            {
                serializedObject.Update();
                var matHash = new HashSet<Material>();
                if (materialArray.materials.Length > 0)
                {
                    foreach (var mat in materialArray.materials)
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
                    var go = obj as GameObject;
                    if (go != null)
                    {
                        var meshRenderers = includeChildren? go.GetComponentsInChildren<MeshRenderer>(includeInactive)
                            : go.GetComponents<MeshRenderer>();
                        foreach (var meshRenderer in meshRenderers)
                        {
                            foreach (var sharedMaterial in meshRenderer.sharedMaterials)
                            {
                                matHash.Add(sharedMaterial);
                            }
                        }
                        var skinnedMeshRenderers = includeChildren? 
                            go.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive) 
                            : go.GetComponents<SkinnedMeshRenderer>();
                        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                        {
                            foreach (var sharedMaterial in skinnedMeshRenderer.sharedMaterials)
                            {
                                matHash.Add(sharedMaterial);
                            }
                        }
                    }
                }
                Undo.RecordObject(serializedObject.targetObject, "add selected");
                materialArray.materials = matHash.ToArray();

                serializedObject.ApplyModifiedProperties();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws the icon for the material header as either an AssetPreview Thumbnail or Preview gui.
        /// </summary>
        /// <param name="materialEditor">Material Editor that contains the header.</param>
        /// <param name="iconRect">Rect area for drawing the icon.</param>
        static void OnHeaderIconGUI (ref MaterialEditor materialEditor, Rect iconRect)
        {
            Texture2D icon = null;
            if (!materialEditor.HasPreviewGUI())
            {
                // Fetch isLoadingAssetPreview to ensure that there is no situation where a preview needs a repaint 
                // because it hasn't finished loading yet.
                var isLoadingAssetPreview = AssetPreview.IsLoadingAssetPreview(materialEditor.target.GetInstanceID());
                icon = AssetPreview.GetAssetPreview(materialEditor.target);
                if (!icon)
                {
                    // We have a static preview it just hasn't been loaded yet. Repaint until we have it loaded.
                    if (isLoadingAssetPreview)
                        materialEditor.Repaint();
                    icon = AssetPreview.GetMiniThumbnail(materialEditor.target);
                }
            }
            else
            {
                // OnPreviewGUI must have all events; not just Repaint, or else the control IDs will mis-match.
                materialEditor.OnPreviewGUI (iconRect, GUIStyle.none);
            }

            if (icon)
            {
                EditorGUI.DrawPreviewTexture(iconRect, icon);
            }
        }
    }
}
