using UnityEditor;
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
                        DrawMaterialHeader(serializedObject, materialEditors[i], targetArray);
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
            MaterialEditor materialEditor, MaterialArray targetArray)
        {
            EditorGUI.BeginChangeCheck();
            // TODO need to replace default draw header
            // TODO does not detect change in all cases for set shader 
            // TODO has issue with drawing in OnInspectorGUI context
            materialEditor.DrawHeader();
            if (EditorGUI.EndChangeCheck())
            {
                // shader property is drawn in header of material
                MultiMaterialEditorUtilities.SetCheckMaterialShaders(targetArray, materialEditor.target as Material); 
            }
            
            if (materialEditor.isVisible)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    MultiMaterialEditorUtilities.UpdateMaterials(targetArray, materialEditor);
                }
            }

        }

    }
}
