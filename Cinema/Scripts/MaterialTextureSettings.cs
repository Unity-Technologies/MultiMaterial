using System;
using UnityEngine;

namespace UnityLabs.Cinema
{
    [CreateAssetMenu(menuName = "UnityLabs/Cinema/MaterialTextureSettings")]
    [Serializable]
    public class MaterialTextureSettings : ScriptableObject
    {
        [Serializable]
        public struct TextureSearchSettings
        {
            public string textureName;
            public string searchDir;
        }

        [SerializeField]
        TextureSearchSettings[] m_SearchSettings;

        public TextureSearchSettings[] searchSettings
        {
            get { return m_SearchSettings; }
            set { m_SearchSettings = value; }
        }
    }

}
