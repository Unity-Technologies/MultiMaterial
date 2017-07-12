using System;
using UnityEngine;

namespace UnityLabs
{
    [CreateAssetMenu(menuName = "Multi Material/Material Texture Settings")]
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

#if UNITY_EDITOR
        public const string searchSettingsPub = "m_SearchSettings";
#endif
        public TextureSearchSettings[] searchSettings
        {
            get { return m_SearchSettings; }
            set { m_SearchSettings = value; }
        }
    }
}
