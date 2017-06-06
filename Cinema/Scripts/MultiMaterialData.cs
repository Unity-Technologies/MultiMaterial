using System;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CreateAssetMenu(menuName = "UnityLabs/Cinema/MultiMaterialData Data")]
    [Serializable]
    public class MultiMaterialData : ScriptableObject
    {
        [SerializeField]
        Material[] m_MaterialArray;

#if UNITY_EDITOR
        public bool[] overrideFields;
        public const string materialArrayPub = "m_MaterialArray";

#endif
        public Material[] materialArray
        {
            get { return m_MaterialArray; }
            set { m_MaterialArray = value; }
        }
    }

}
