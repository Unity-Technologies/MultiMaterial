using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    public class MultiMaterialEditorUtilities
    {
        public static void UpdateMaterials(MultiMaterialData multiMaterialData, MaterialEditor controlMat)
        {
            if (multiMaterialData.materialArray.Length < 1 && controlMat == null || controlMat.target == null)
            {
                // help box goes here to tell you to assign control material to first item in array
                return;
            }
            SetCheckMaterialShaders(multiMaterialData, controlMat.target as Material);
//            var controlProperties = MaterialEditor.GetMaterialProperties(new[] { controlMat.target });
//            for (var i = 0; i < multiMaterialData.materialArray.Length; i++)
//            {
//                if (multiMaterialData.materialArray[i] == controlMat.target as Material)
//                    continue;
//                if (multiMaterialData.materialArray[i] != null)
//                {
//                    foreach (var controlProperty in controlProperties)
//                    {
//                        switch (controlProperty.type)
//                        {
//                            case MaterialProperty.PropType.Color:
//                                multiMaterialData.materialArray[i].SetColor(controlProperty.name, controlProperty.colorValue);
//                                break;
//                            case MaterialProperty.PropType.Float:
//                                multiMaterialData.materialArray[i].SetFloat(controlProperty.name, controlProperty.floatValue);
//                                break;
//                            case MaterialProperty.PropType.Range:
//                                goto case MaterialProperty.PropType.Float;
//                            case MaterialProperty.PropType.Vector:
//                                multiMaterialData.materialArray[i].SetVector(controlProperty.name, controlProperty.vectorValue);
//                                break;
//                            case MaterialProperty.PropType.Texture:
//                                // skipping texture set since used for udim mapping
//                                break;
//                        }
//                    }
//                }
//            }

            var matSerial = new SerializedObject(controlMat.target);
            var controlProps = matSerial.GetIterator();

            var matObjs = multiMaterialData.materialArray.Select(material => new SerializedObject(material)).ToList();

            foreach (var matObj in matObjs) { matObj.Update();}

            while (controlProps.NextVisible(true))
            {
                foreach (var matObj in matObjs)
                {
                    var prop = matObj.FindProperty(controlProps.propertyPath);
                    if (prop != null)
                    {
                        switch (prop.propertyType)
                        {
                            case SerializedPropertyType.AnimationCurve:
                                break;
                            case SerializedPropertyType.ArraySize:
                                break;
                            case SerializedPropertyType.Boolean:
                                prop.boolValue = controlProps.boolValue;
                                break;
                            case SerializedPropertyType.Bounds:
                                break;
                            case SerializedPropertyType.Character:
                                break;
                            case SerializedPropertyType.Color:
                                prop.colorValue = controlProps.colorValue;
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
                                prop.floatValue = controlProps.floatValue;
                                break;
                            case SerializedPropertyType.Integer:
                                break;
                            case SerializedPropertyType.String:
                                prop.stringValue = controlProps.stringValue;
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
                                if (controlProps.objectReferenceValue != null && 
                                    controlProps.objectReferenceValue.GetType().IsAssignableFrom(typeof(Texture)))
                                    prop.objectReferenceValue = controlProps.objectReferenceValue;
                                break;
                            case SerializedPropertyType.LayerMask:
                                break;
                        }
                    }
                }
            }

            foreach (var matObj in matObjs) { matObj.ApplyModifiedPropertiesWithoutUndo();}
        }

        public static void SetCheckMaterialShaders(MultiMaterialData multiMaterialData, Material mat)
        {
            var matSerial = new SerializedObject(mat);
            var shaderSerial = matSerial.FindProperty("m_Shader");
            foreach (var material in multiMaterialData.materialArray)
            {
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