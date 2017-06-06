using UnityEngine;

namespace UnityLabs.Cinema
{
    public class MultiMaterial : MonoBehaviour
    {
        [SerializeField]
        MultiMaterialData m_MultiMaterialData;

        public MultiMaterialData multiMaterialData
        {
            get { return m_MultiMaterialData; }
        }

#if UNITY_EDITOR
        public const string multiMaterialPub = "m_MultiMaterialData";
#endif
    }
}
