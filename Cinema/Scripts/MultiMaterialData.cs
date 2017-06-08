using System;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CreateAssetMenu(menuName = "UnityLabs/Cinema/MultiMaterialData")]
    [Serializable]
    public class MultiMaterialData : ScriptableObject
    {
        [SerializeField]
        MaterialArray m_MaterialArrayData;

#if UNITY_EDITOR
        public bool[] overrideFields;
        public const string materialArrayDataPub = "m_MaterialArrayData";
#endif
        public MaterialArray materialArrayData
        {
            get { return m_MaterialArrayData; }
            set { m_MaterialArrayData = value; }
        }
    }

    [Serializable]
    public class MaterialArray : object
    {
        [SerializeField]
        Material[] m_Materials;

#if UNITY_EDITOR
        public const string materialArrayPub = "m_Materials";
#endif
        // TODO if want to add runtime modifications should allow use of SharedMaterials
        public Material[] materials
        {
            get { return m_Materials; }
            set { m_Materials = value; }
        }
    }
}
