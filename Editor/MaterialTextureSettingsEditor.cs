using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityLabs
{
    [CustomEditor(typeof(MaterialTextureSettings))]
    public class MaterialTextureSettingsEditor : Editor
    {
        SerializedProperty m_SearchSettings;
        Vector2 m_LogsScrollView;
        Color m_DarkWindow = new Color(0, 0, 0, 0.2f);
        bool m_SettingReady;
        string m_Logs;
        MaterialTextureSettings m_MaterialTextureSettings;

        Dictionary<int, Texture> m_UdimIndexMapping;
        Dictionary<int, List<Material>> m_UdimMaterial;

        void OnEnable()
        {
            m_SearchSettings = serializedObject.FindProperty(MaterialTextureSettings.searchSettingsPub);
            m_MaterialTextureSettings = target as MaterialTextureSettings;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var arraySizeProp = m_SearchSettings.FindPropertyRelative("Array.size");
            EditorGUILayout.PropertyField(arraySizeProp);
            serializedObject.ApplyModifiedProperties();

            if (m_MaterialTextureSettings == null)
                m_MaterialTextureSettings = target as MaterialTextureSettings;

            EditorGUI.indentLevel++;
            serializedObject.Update();

            for (var i = 0; i < m_SearchSettings.arraySize; i++)
            {
                var textureSearch = m_MaterialTextureSettings.searchSettings[i];
                SearchSettingsDrawer(m_SearchSettings.GetArrayElementAtIndex(i), textureSearch, i);
            }

            EditorGUILayout.Separator();
            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(m_SearchSettings.arraySize < 1);
            if (GUILayout.Button("Apply All"))
            {
                for (var i = 0; i < m_MaterialTextureSettings.searchSettings.Length; i++)
                {
                    var textureSearch = m_MaterialTextureSettings.searchSettings[i];
                    ApplySettingsToSelection(textureSearch);
                }
            }
            if (GUILayout.Button("Clear All"))
            {
                for (var i = 0; i < m_MaterialTextureSettings.searchSettings.Length; i++)
                {
                    var textureSearch = m_MaterialTextureSettings.searchSettings[i];
                    ClearSettingsOnSelection(textureSearch);
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            var logsRec = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(150));
            EditorGUI.DrawRect(logsRec, m_DarkWindow);
            m_LogsScrollView = EditorGUILayout.BeginScrollView(m_LogsScrollView);
            EditorGUILayout.TextArea(m_Logs);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void SearchSettingsDrawer(SerializedProperty serializedProperty, 
            MaterialTextureSettings.TextureSearchSettings textureSearch, int index)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedProperty, 
                new GUIContent(string.Format("Element {0}: {1}", index, textureSearch.textureName)), true);
            serializedObject.ApplyModifiedProperties();

            var path = string.Format("{0}{1}{2}",Application.dataPath, Path.AltDirectorySeparatorChar, 
                textureSearch.searchDir);
            var colorTags = "red";
            if (Directory.Exists(path))
            {
                colorTags = "green";
            }

            var richTextPath = string.Format("<size=12><color={0}>{1}</color></size>", colorTags, path);
            var style = new GUIStyle(EditorStyles.helpBox) { richText = true };
            EditorGUILayout.TextArea(richTextPath, style);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Check"))
            {
                var hasProp = false;
                m_Logs = "";
                if (Selection.objects[0].GetType() == typeof(Material))
                {
                    var selectionMaterial = Selection.objects[0] as Material;
                    if (selectionMaterial != null)
                    {
                        hasProp = selectionMaterial.HasProperty(textureSearch.textureName);
                    }
                    m_Logs += string.Format("{0} check if has property {1} : {2}\n", 
                        Selection.objects[0].name, textureSearch.textureName, hasProp);
                }

            }
            if (GUILayout.Button("Clear"))
            {
                m_Logs = "";
                ClearSettingsOnSelection(textureSearch);
            }

            if (GUILayout.Button("Apply"))
            {
                ApplySettingsToSelection(textureSearch);
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }

        void ClearSettingsOnSelection(MaterialTextureSettings.TextureSearchSettings textureSearch)
        {
            m_Logs = "";
            foreach (var objs in Selection.objects)
            {
                var mat = objs as Material;
                if (mat != null)
                {
                    if (mat.HasProperty(textureSearch.textureName))
                    {
                        mat.SetTexture(textureSearch.textureName, null);
                        m_Logs += string.Format("{0} had {1} cleared\n", mat.name, textureSearch.textureName);
                    }
                }
            }
        }

        void ApplySettingsToSelection(MaterialTextureSettings.TextureSearchSettings textureSearch)
        {
            m_Logs = "";
            m_UdimIndexMapping = new Dictionary<int, Texture>();
            m_UdimMaterial = new Dictionary<int, List<Material>>();
            var path = string.Format("{0}{1}{2}",
                Application.dataPath,
                Path.AltDirectorySeparatorChar, textureSearch.searchDir);
            if (!Directory.Exists(path))
            {
                m_Logs += string.Format("Error! Path: '{0}' does not exist!\n", path);
                return;
            }

            var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                if (string.IsNullOrEmpty(file))
                    continue;
                if (Path.GetExtension(file) == ".meta")
                {
                    continue;
                }
                var assetPath = "Assets/" + textureSearch.searchDir + Path.GetFileName(file);
                m_Logs += string.Format("trying to load asset at {0}", assetPath);
                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset == null)
                {
                    m_Logs += " FAIL! file is null\n";
                    continue;
                }
                var texture = asset as Texture;
                if (texture == null)
                {
                    m_Logs += " FAIL! not a Texture\n";
                    continue;
                }
                m_Logs += " SUCCESS! texture loaded\n";

                var nameSplit = Path.GetFileNameWithoutExtension(file).Split('_');
                foreach (var split in nameSplit)
                {
                    int id;
                    if (int.TryParse(split, out id) && (id > 999 && id < 10000))
                    {
                        m_UdimIndexMapping[id] = texture;
                        m_Logs += string.Format("Texture id: {0} name: {1} found\n", id, texture.name);
                        break;
                    }
                }
            }

            var objectSelection = Selection.objects;
            foreach (var obj in objectSelection)
            {
                var mat = obj as Material;
                if (mat != null)
                {
                    var nameSplit = mat.name.Split('_');
                    foreach (var split in nameSplit)
                    {
                        int id;
                        if (int.TryParse(split, out id) && (id > 999 && id < 10000))
                        {
                            if (!m_UdimMaterial.ContainsKey(id))
                            {
                                m_UdimMaterial[id] = new List<Material>();
                            }
                            m_UdimMaterial[id].Add(mat);
                            m_Logs += string.Format("Material id: {0} name: {1} found\n", id, mat.name);
                            break;
                        }
                    }
                }
            }

            foreach (var kp in m_UdimMaterial)
            {
                if (m_UdimIndexMapping.ContainsKey(kp.Key))
                {
                    foreach (var mat in kp.Value)
                    {
                        mat.SetTexture(textureSearch.textureName, m_UdimIndexMapping[kp.Key]);
                        var texName = m_UdimIndexMapping[kp.Key].name;
                        m_Logs += string.Format("{0} had texture {1} at {2}", 
                            mat.name, texName, textureSearch.textureName);
                    }
                }
                else
                {
                    foreach (var mat in kp.Value)
                    {
                        mat.SetTexture(textureSearch.textureName, null);
                    }

                }

            }
        }

    }
}
