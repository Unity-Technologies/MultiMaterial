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
            var setProperties = new Dictionary<string, SerializedProperty>();
            if (!syncAll)
            {
                Material checkedMaterial = null;
                foreach (var material in materialArray.materials)
                {
                    if (material != controlMatial)
                    {
                        checkedMaterial = material;
                        break;
                    }
                }
                var controlMaterialObject = new SerializedObject(controlMatialEditor.target);
                var checkedMaterialObject = new SerializedObject(checkedMaterial);

                setProperties = GetPropertiesToChange(controlMaterialObject, checkedMaterialObject);
            }
            else
            {
                var controlMaterialObject = new SerializedObject(controlMatialEditor.target);

                setProperties = GetPropertiesToChange(controlMaterialObject, null, true);
            }




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
                                    prop.animationCurveValue = property.Value.animationCurveValue;
                                    break;
                                case SerializedPropertyType.ArraySize:
                                    if (prop.isArray)
                                        prop.arraySize = property.Value.arraySize;
                                    break;
                                case SerializedPropertyType.Boolean:
                                    prop.boolValue = property.Value.boolValue;
                                    break;
                                case SerializedPropertyType.Bounds:
                                    prop.boundsValue = property.Value.boundsValue;
                                    break;
                                case SerializedPropertyType.Character:
                                    // TODO figure out what value this is.
                                    break;
                                case SerializedPropertyType.Color:
                                    prop.colorValue = property.Value.colorValue;
                                    break;
                                case SerializedPropertyType.Enum:
                                    prop.enumValueIndex = property.Value.enumValueIndex;
                                    break;
                                case SerializedPropertyType.ExposedReference:
                                    prop.exposedReferenceValue = property.Value.exposedReferenceValue;
                                    break;
                                case SerializedPropertyType.FixedBufferSize:
                                    // SerializedProperty.fixedBufferSize is read only
                                    break;
                                case SerializedPropertyType.Generic:
                                    // TODO figure out what value this is.
                                    break;
                                case SerializedPropertyType.Gradient:
                                    // TODO figure out what value this is.
                                    break;
                                case SerializedPropertyType.Float:
                                    prop.floatValue = property.Value.floatValue;
                                    break;
                                case SerializedPropertyType.Integer:
                                    prop.intValue = property.Value.intValue;
                                    break;
                                case SerializedPropertyType.String:
                                    prop.stringValue = property.Value.stringValue;
                                    break;
                                case SerializedPropertyType.Rect:
                                    prop.rectValue = property.Value.rectValue;
                                    break;
                                case SerializedPropertyType.Quaternion:
                                    prop.quaternionValue = property.Value.quaternionValue;
                                    break;
                                case SerializedPropertyType.Vector2:
                                    prop.vector2Value = property.Value.vector2Value;
                                    break;
                                case SerializedPropertyType.Vector3:
                                    prop.vector3Value = property.Value.vector3Value;
                                    break;
                                case SerializedPropertyType.Vector4:
                                    prop.vector4Value = property.Value.vector4Value;
                                    break;
                                case SerializedPropertyType.ObjectReference:
                                    prop.objectReferenceValue = property.Value.objectReferenceValue;
                                    break;
                                case SerializedPropertyType.LayerMask:
                                    // TODO figure out what value this is.
                                    break;
                            }
                        }
                
                    }
                }
                foreach (var matObj in matObjs) { matObj.ApplyModifiedProperties();}
            }
        }

        public static Dictionary<string, SerializedProperty> GetPropertiesToChange(SerializedObject controlObject, 
            SerializedObject checkedObject, bool syncAll = false)
        {
            if (syncAll && controlObject == null || !syncAll && (controlObject == null || checkedObject == null))
                return null;
            
            var setProperties = new Dictionary<string, SerializedProperty>();
            var controlPoperty = controlObject.GetIterator();

            while (controlPoperty.NextVisible(true))
            {
                var property = syncAll ? controlObject.FindProperty(controlPoperty.propertyPath) 
                    : checkedObject.FindProperty(controlPoperty.propertyPath);

                if (property != null)
                {
                    switch (property.propertyType)
                    {
                        case SerializedPropertyType.AnimationCurve:
                            if (syncAll || controlPoperty.animationCurveValue != property.animationCurveValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.ArraySize:
                            if (controlPoperty.isArray)
                                if (syncAll || controlPoperty.arraySize != property.arraySize)
                                    setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Boolean:
                            if (syncAll || controlPoperty.boolValue != property.boolValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Bounds:
                            if (syncAll || controlPoperty.boundsValue != property.boundsValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Character:
                            // TODO figure out what value this is.
                            break;
                        case SerializedPropertyType.Color:
                            if (syncAll || controlPoperty.colorValue != property.colorValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Enum:
                            if (syncAll || controlPoperty.enumValueIndex != property.enumValueIndex)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.ExposedReference:
                            // Note: Does not handle syncing nulls. Cannot tell if the null should have been a texture. 
                            if (syncAll || controlPoperty.exposedReferenceValue != null && 
                                controlPoperty.exposedReferenceValue.GetType().IsAssignableFrom(typeof(Texture)))
                                if (controlPoperty.exposedReferenceValue != property.exposedReferenceValue)
                                    setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.FixedBufferSize:
                            // SerializedProperty.fixedBufferSize is read only
                            break;
                        case SerializedPropertyType.Generic:
                            // TODO figure out what value this is.
                            break;
                        case SerializedPropertyType.Gradient:
                            // TODO figure out what value this is.
                            break;
                        case SerializedPropertyType.Float:
                            if (syncAll || controlPoperty.floatValue != property.floatValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Integer:
                            if (syncAll || controlPoperty.intValue != property.intValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.String:
                            if (syncAll || controlPoperty.stringValue != property.stringValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Rect:
                            if (syncAll || controlPoperty.rectValue != property.rectValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Quaternion:
                            if (syncAll || controlPoperty.quaternionValue != property.quaternionValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Vector2:
                            if (syncAll || controlPoperty.vector2Value != property.vector2Value)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Vector3:
                            if (syncAll || controlPoperty.vector3Value != property.vector3Value)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.Vector4:
                            if (syncAll || controlPoperty.vector4Value != property.vector4Value)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.ObjectReference:
                            // Note: Does not handle syncing nulls. Cannot tell if the null should have been a texture. 
                            if (syncAll || controlPoperty.objectReferenceValue != null && 
                                controlPoperty.objectReferenceValue.GetType().IsAssignableFrom(typeof(Texture)))
                                if (controlPoperty.objectReferenceValue != property.objectReferenceValue)
                                    setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.LayerMask:
                            // TODO figure out what value this is.
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
