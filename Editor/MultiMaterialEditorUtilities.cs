using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs
{
    public class MultiMaterialEditorUtilities
    {
        const string k_DefaultMaterial = "Default-Material";

        public static void UpdateMaterials(MaterialArray materialArray, 
            Material controlMaterial, bool syncAll = false)
        {
            if (materialArray.materials.Length < 1 && controlMaterial == null)
            {
                return;
            }

            SetCheckMaterialShaders(materialArray, controlMaterial);
            var controlMaterialObject = new SerializedObject(controlMaterial);
            SerializedObject checkedMaterialObject = null;
            if (!syncAll)
            {
                Material checkedMaterial = null;
                foreach (var material in materialArray.materials)
                {
                    if (material != controlMaterial && material.name != k_DefaultMaterial)
                    {
                        checkedMaterial = material;
                        break;
                    }
                }
                if (checkedMaterial == null)
                    return;
                checkedMaterialObject = new SerializedObject(checkedMaterial);
            }

            // Find the Property (Properties) that changed in the Material Array 
            var propertiesToChange = GetPropertiesToChange(controlMaterialObject, checkedMaterialObject, syncAll);

            var matHash = new HashSet<Material>(materialArray.materials);

            //Convert each material in the hash into a list of serialized accessors.
            var matObjs = (from mat in matHash
                           where mat != null && mat.name != k_DefaultMaterial
                           select new SerializedObject(mat)).ToList();

            if (propertiesToChange != null && propertiesToChange.Count > 0)
            {
                foreach (var matObj in matObjs) { matObj.Update();}

                foreach (var propertyToChange in propertiesToChange)
                {
                    foreach (var matObj in matObjs)
                    {
                        var serializedProperty = matObj.FindProperty(propertyToChange.Key);
                        if (serializedProperty != null)
                        {
                            switch (serializedProperty.propertyType)
                            {
                                case SerializedPropertyType.AnimationCurve:
                                    serializedProperty.animationCurveValue = 
                                        propertyToChange.Value.animationCurveValue;
                                    break;
                                case SerializedPropertyType.ArraySize:
                                    if (serializedProperty.isArray)
                                        serializedProperty.arraySize = propertyToChange.Value.arraySize;
                                    break;
                                case SerializedPropertyType.Boolean:
                                    serializedProperty.boolValue = propertyToChange.Value.boolValue;
                                    break;
                                case SerializedPropertyType.Bounds:
                                    serializedProperty.boundsValue = propertyToChange.Value.boundsValue;
                                    break;
                                case SerializedPropertyType.Character:
                                    break;
                                case SerializedPropertyType.Color:
                                    serializedProperty.colorValue = propertyToChange.Value.colorValue;
                                    break;
                                case SerializedPropertyType.Enum:
                                    serializedProperty.enumValueIndex = propertyToChange.Value.enumValueIndex;
                                    break;
#if UNITY_2017_1_OR_NEWER
                                case SerializedPropertyType.ExposedReference:
                                    serializedProperty.exposedReferenceValue = 
                                        propertyToChange.Value.exposedReferenceValue;
                                    break;
                                case SerializedPropertyType.FixedBufferSize:
                                    // SerializedProperty.fixedBufferSize is read only
                                    break;
#endif
                                case SerializedPropertyType.Generic:
                                    break;
                                case SerializedPropertyType.Gradient:
                                    break;
                                case SerializedPropertyType.Float:
                                    serializedProperty.floatValue = propertyToChange.Value.floatValue;
                                    break;
                                case SerializedPropertyType.Integer:
                                    serializedProperty.intValue = propertyToChange.Value.intValue;
                                    break;
                                case SerializedPropertyType.String:
                                    serializedProperty.stringValue = propertyToChange.Value.stringValue;
                                    break;
                                case SerializedPropertyType.Rect:
                                    serializedProperty.rectValue = propertyToChange.Value.rectValue;
                                    break;
                                case SerializedPropertyType.Quaternion:
                                    serializedProperty.quaternionValue = propertyToChange.Value.quaternionValue;
                                    break;
                                case SerializedPropertyType.Vector2:
                                    serializedProperty.vector2Value = propertyToChange.Value.vector2Value;
                                    break;
                                case SerializedPropertyType.Vector3:
                                    serializedProperty.vector3Value = propertyToChange.Value.vector3Value;
                                    break;
                                case SerializedPropertyType.Vector4:
                                    serializedProperty.vector4Value = propertyToChange.Value.vector4Value;
                                    break;
                                case SerializedPropertyType.ObjectReference:
                                    serializedProperty.objectReferenceValue = 
                                        propertyToChange.Value.objectReferenceValue;
                                    break;
                                case SerializedPropertyType.LayerMask:
                                    break;
                            }
                        }
                
                    }
                }
                foreach (var matObj in matObjs) { matObj.ApplyModifiedProperties();}
            }
        }

        /// <summary>
        /// Compares the Control Object to the Checked Object and caches the properties that do not match 
        /// and are not texture objects.
        /// </summary>
        /// <param name="controlObject"></param>
        /// <param name="checkedObject"></param>
        /// <param name="syncAll"></param>
        /// <returns></returns>
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
#if UNITY_2017_1_OR_NEWER
                        case SerializedPropertyType.ExposedReference:
                            // Note: Does not handle syncing nulls. Cannot tell if the null should have been a texture. 
                            if ((syncAll || controlPoperty.exposedReferenceValue != null &&
                                controlPoperty.exposedReferenceValue.GetType().IsAssignableFrom(typeof(Texture))) && 
                                controlPoperty.exposedReferenceValue != property.exposedReferenceValue)
                                setProperties[controlPoperty.propertyPath] = controlPoperty.Copy();
                            break;
                        case SerializedPropertyType.FixedBufferSize:
                            // SerializedProperty.fixedBufferSize is read only
                            break;
#endif
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
            matSerial.Update();
            var shaderSerial = matSerial.FindProperty("m_Shader");
            matSerial.ApplyModifiedProperties();

            foreach (var material in materialArray.materials)
            {
                if (material == null || material.name == k_DefaultMaterial)
                    continue;
                var targetMatSerial = new SerializedObject(material);
                var targetShader = targetMatSerial.FindProperty("m_Shader");
                if (shaderSerial != targetShader)
                {
                    targetMatSerial.Update();
                    targetShader.objectReferenceValue = shaderSerial.objectReferenceValue;
                    targetMatSerial.ApplyModifiedProperties();
                }
            }
        }
    }
}
