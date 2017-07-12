using System;
using UnityEngine;

namespace UnityLabs
{
    [CreateAssetMenu(menuName = "Multi Material/Multi Material Data")]
    [Serializable]
    public class MultiMaterialData : ScriptableObject
    {
        [SerializeField]
        MaterialArray m_MaterialArrayData;

#if UNITY_EDITOR
        public const string materialArrayPub = "m_MaterialArrayData";
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
        public const string materialsPub = "m_Materials";
#endif
        public Material[] materials
        {
            get { return m_Materials; }
            set { m_Materials = value; }
        }
    }
}
