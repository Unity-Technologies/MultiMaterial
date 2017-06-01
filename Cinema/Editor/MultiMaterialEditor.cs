using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CustomEditor(typeof(MultiMaterial))]
    public class MultiMaterialEditor : Editor
    {
        public SerializedProperty materialArray;

        void OnEnable()
        {
            materialArray = serializedObject.FindProperty("m_MaterialArray");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Add Selected"))
            {
                serializedObject.Update();
                var matHash = new HashSet<Material>();
                var multMat = target as MultiMaterial;
                foreach (var mat in multMat.materialArray)
                {
                    matHash.Add(mat);
                }
                foreach (var obj in Selection.objects)
                {
                    var mat = obj as Material;
                    if (mat != null)
                    {
                        matHash.Add(mat);
                    }
                }
                multMat.materialArray = matHash.ToArray();
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Select Materials"))
            {
                var multMat = target as MultiMaterial;
                if (multMat != null)
                {
                    Selection.objects = multMat.materialArray;
                }
            }
        }
    }

}
