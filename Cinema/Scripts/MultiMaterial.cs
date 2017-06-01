using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityLabs.Cinema
{
    [CreateAssetMenu(menuName = "UnityLabs/Cinema/MultiMaterial")]
    [Serializable]
    public class MultiMaterial : ScriptableObject
    {
        [SerializeField]
        Material[] m_MaterialArray;

#if UNITY_EDITOR
        public bool[] overrideFields;
#endif
        public Material[] materialArray
        {
            get { return m_MaterialArray; }
            set { m_MaterialArray = value; }
        }
    }

}
