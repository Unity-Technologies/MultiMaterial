using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLabs.Cinema
{
    public class MaterialArrayDrawers
    {
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
                        DrawMaterialHeader(serializedObject, ref materialEditors[i], targetArray);
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

        public static void DrawMaterialHeader(SerializedObject serializedObject, 
            ref MaterialEditor materialEditor, MaterialArray targetArray)
        {
            if (materialEditor == null)
            {
                return;
            }
//            EditorGUI.BeginChangeCheck();
            // TODO need to replace default draw header
            // TODO does not detect change in all cases for set shader 
            // TODO has issue with drawing in OnInspectorGUI context
            materialEditor.DrawHeader();

            EditorGUILayout.BeginHorizontal();

	        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(80));

	        var layout = EditorGUILayout.GetControlRect(false, 36, GUILayout.MinWidth(40), GUILayout.MaxWidth(40));
	        EditorGUI.DrawRect(layout, Color.blue);

			var imageRect = new Rect(layout.xMax-32, layout.y, 32, 32);
	        EditorGUI.DrawRect(imageRect, Color.green);
			EditorGUI.DrawPreviewTexture(imageRect, AssetPreview.GetAssetPreview( materialEditor.target as Material));
	        
			var foldControlRect = new Rect(layout.x-4, layout.yMax-12, 8, 8);
	        EditorGUI.DrawRect(foldControlRect, Color.red);
	        EditorGUI.Foldout(foldControlRect, true, GUIContent.none);

			EditorGUILayout.EndVertical();
	        EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField(new GUIContent(materialEditor.target.name));

	        UnityEditorInternal.InternalEditorUtility.SetupShaderMenu(materialEditor.target as Material);

			var guids = AssetDatabase.FindAssets("t:Shader");
	        var shaderList = new List<Shader>(guids.Select(s => AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(s)) as Shader));
			shaderList.AddRange((Shader[])Resources.FindObjectsOfTypeAll(typeof(Shader)));
	        var shadernames = shaderList.Select(n=>n.name).ToArray();

	        // Filter out those that are supposed to be hidden
	        shadernames = shadernames.Where(s => !string.IsNullOrEmpty(s) && !s.Contains("__") && !s.Contains("Hidden")).ToArray();
	        var mat = materialEditor.target as Material;
	        var intdex = Array.FindIndex(shadernames, s=> s == mat.shader.name);
	        EditorGUILayout.BeginVertical();

			var contents = shadernames.Select(s=> new GUIContent(s)).ToArray();
	        intdex = EditorGUILayout.Popup(new GUIContent("Shader"), intdex, contents, EditorStyles.popup);
			EditorGUILayout.EndVertical();
	        if (shadernames[intdex] != mat.shader.name)
	        {

		        var matSerial = new SerializedObject(materialEditor.target);
		        matSerial.Update();

		        var shaderSerial = matSerial.FindProperty("m_Shader");
		        shaderSerial.objectReferenceValue = Shader.Find(shadernames[intdex]);
		        matSerial.ApplyModifiedProperties();

		        MultiMaterialEditorUtilities.SetCheckMaterialShaders(targetArray, materialEditor.target as Material);
	        }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            
            if (materialEditor.isVisible)
            {
                EditorGUI.BeginChangeCheck();
                if (GUILayout.Button("Sync to Material"))
                {
                    MultiMaterialEditorUtilities.UpdateMaterials(targetArray, materialEditor, true);
                }
                materialEditor.OnInspectorGUI();

                if (EditorGUI.EndChangeCheck())
                {
                    MultiMaterialEditorUtilities.UpdateMaterials(targetArray, materialEditor);
                }
            }

        }

    }
}
