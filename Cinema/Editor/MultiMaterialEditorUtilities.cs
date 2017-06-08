using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    public class MultiMaterialEditorUtilities
    {
        public static void UpdateMaterials(MaterialArray materialArray, 
            MaterialEditor controlMatialEditor, bool syncAll = false)
        {
            if (materialArray.materials.Length < 1 && controlMatialEditor == null 
                || controlMatialEditor.target == null)
            {
                return;
            }

            SetCheckMaterialShaders(materialArray, controlMatialEditor.target as Material);

            var controlMatial = controlMatialEditor.target as Material;
            Material checkedMaterial = null;
            if (!syncAll)
            {
                foreach (var material in materialArray.materials)
                {
                    if (material != controlMatial)
                    {
                        checkedMaterial = material;
                        break;
                    }
                }
            }

            var controlMaterialObject = new SerializedObject(controlMatialEditor.target);

            var checkedMaterialObject = new SerializedObject(checkedMaterial);

            var setProperties = GetPropertiesToChange(controlMaterialObject, checkedMaterialObject, syncAll);


            var matHash = new HashSet<Material>(materialArray.materials);

            //Linq experession contents
            /*
            var matObjs = new List<SerializedObject>();
            foreach (var mat in matHash)
            {
                if (mat != null)
                    matObjs.Add(new SerializedObject(mat));
            }
            */
            var matObjs = (from mat in matHash where mat != null select new SerializedObject(mat)).ToList();

            if (setProperties != null && setProperties.Count > 0)
            {
                foreach (var matObj in matObjs) { matObj.Update();}

                foreach (var property in setProperties)
                {
                    foreach (var matObj in matObjs)
                    {
                        var prop = matObj.FindProperty(property.Key);
                        if (prop != null)
                        {
                            switch (prop.propertyType)
                            {
                                case SerializedPropertyType.AnimationCurve:
                                    break;
                                case SerializedPropertyType.ArraySize:
                                    break;
                                case SerializedPropertyType.Boolean:
                                    prop.boolValue = property.Value.boolValue;
                                    break;
                                case SerializedPropertyType.Bounds:
                                    break;
                                case SerializedPropertyType.Character:
                                    break;
                                case SerializedPropertyType.Color:
                                    prop.colorValue = property.Value.colorValue;
                                    break;
                                case SerializedPropertyType.Enum:
                                    break;
                                case SerializedPropertyType.ExposedReference:
                                    break;
                                case SerializedPropertyType.FixedBufferSize:
                                    break;
                                case SerializedPropertyType.Generic:
                                    break;
                                case SerializedPropertyType.Gradient:
                                    break;
                                case SerializedPropertyType.Float:
                                    prop.floatValue = property.Value.floatValue;
                                    break;
                                case SerializedPropertyType.Integer:
                                    break;
                                case SerializedPropertyType.String:
                                    prop.stringValue = property.Value.stringValue;
                                    break;
                                case SerializedPropertyType.Rect:
                                    break;
                                case SerializedPropertyType.Quaternion:
                                    break;
                                case SerializedPropertyType.Vector2:
                                    break;
                                case SerializedPropertyType.Vector3:
                                    break;
                                case SerializedPropertyType.Vector4:
                                    break;
                                case SerializedPropertyType.ObjectReference:
                                    prop.objectReferenceValue = property.Value.objectReferenceValue;
                                    break;
                                case SerializedPropertyType.LayerMask:
                                    break;
                            }
                        }
                
                    }
                }
                foreach (var matObj in matObjs) { matObj.ApplyModifiedPropertiesWithoutUndo();}
            }
    
        }

        public static Dictionary<string, SerializedProperty> GetPropertiesToChange(SerializedObject controlObject, 
            SerializedObject checkedObject, bool syncAll = false)
        {
            if (syncAll && controlObject == null || !syncAll && (controlObject == null || checkedObject == null))
                return null;
            var controlPoperty = controlObject.GetIterator();

            var setProperties = new Dictionary<string, SerializedProperty>();
            while (controlPoperty.NextVisible(true))
            {
                var property = checkedObject.FindProperty(controlPoperty.propertyPath);
                if (property != null)
                {
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.AnimationCurve:
                            break;
                        case SerializedPropertyType.ArraySize:
                            break;
                        case SerializedPropertyType.Boolean:
                            if (syncAll || controlPoperty.boolValue != property.boolValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Bounds:
                            break;
                        case SerializedPropertyType.Character:
                            break;
                        case SerializedPropertyType.Color:
                            if (syncAll || controlPoperty.colorValue != property.colorValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Enum:
                            break;
                        case SerializedPropertyType.ExposedReference:
                            break;
                        case SerializedPropertyType.FixedBufferSize:
                            break;
                        case SerializedPropertyType.Generic:
                            break;
                        case SerializedPropertyType.Gradient:
                            break;
                        case SerializedPropertyType.Float:
                            if (syncAll || controlPoperty.floatValue != property.floatValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Integer:
                            break;
                        case SerializedPropertyType.String:
                            if (syncAll || controlPoperty.stringValue != property.stringValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Rect:
                            break;
                        case SerializedPropertyType.Quaternion:
                            break;
                        case SerializedPropertyType.Vector2:
                            break;
                        case SerializedPropertyType.Vector3:
                            break;
                        case SerializedPropertyType.Vector4:
                            break;
                        case SerializedPropertyType.ObjectReference:
                            // Note: Does not handle syncing nulls. Cannot tell if the null should have been a texture. 
                            if (syncAll || controlPoperty.objectReferenceValue != null && 
                                controlPoperty.objectReferenceValue.GetType().IsAssignableFrom(typeof(Texture)))
                                if (controlPoperty.objectReferenceValue != property.objectReferenceValue)
                                    setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.LayerMask:
                            break;
                    }
                   
                }

            }
            return setProperties;
        }

        public static void SetCheckMaterialShaders(MaterialArray materialArray, Material mat)
        {
            var matSerial = new SerializedObject(mat);
            var shaderSerial = matSerial.FindProperty("m_Shader");
            foreach (var material in materialArray.materials)
            {
                if (material == null)
                    continue;
                var targetMatSerial = new SerializedObject(material);
                var targetShader = targetMatSerial.FindProperty("m_Shader");
                if (shaderSerial != targetShader)
                {
                    targetMatSerial.Update();
                    targetShader.objectReferenceValue = shaderSerial.objectReferenceValue;
                    targetMatSerial.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

    }
}
