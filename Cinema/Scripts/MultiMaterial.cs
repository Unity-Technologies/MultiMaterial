using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityLabs;
using UnityLabs.Cinema;

namespace UnityLabs.Cinema
{
    public class MultiMaterial : MonoBehaviour
    {
        [SerializeField]
        MultiMaterialData m_MultiMaterial;

        public MultiMaterialData multiMaterial
        {
            get { return m_MultiMaterial; }
        }

#if UNITY_EDITOR
        public const string multiMaterialPub = "m_MultiMaterial";
#endif
    }
}
