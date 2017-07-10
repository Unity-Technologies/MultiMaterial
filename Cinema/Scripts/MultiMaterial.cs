using UnityEngine;

namespace UnityLabs
{
    [RequireComponent(typeof(Renderer))]
    public class MultiMaterial : MonoBehaviour
    {
        [SerializeField]
        MultiMaterialData m_MultiMaterialData;

        MaterialArray m_MaterialArray;

        public MultiMaterialData multiMaterialData
        {
            get { return m_MultiMaterialData; }
        }

        public MaterialArray materialArray
        {
            get { return m_MultiMaterialData != null ? m_MultiMaterialData.materialArrayData : m_MaterialArray; }
#if UNITY_EDITOR
            set { m_MaterialArray = value; }
#endif
        }

#if UNITY_EDITOR
        public const string multiMaterialDataPub = "m_MultiMaterialData";
        public const string materialArrayPub = "m_MaterialArray";
#endif
    }
}
