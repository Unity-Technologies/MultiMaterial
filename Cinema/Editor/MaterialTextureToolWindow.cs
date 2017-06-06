using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityLabs.Cinema
{
    public class MaterialTextureToolWindow : EditorWindow
    {
        MaterialTextureSettings m_MaterialTextureSettings;


        [MenuItem("UnityLabs/Cinema/MaterialTextureToolWindow")]
        public static void Open()
        {
            var window = GetWindow(typeof(MaterialTextureToolWindow));
            const string windowTitle = "MaterialTextureToolWindow";
            window.titleContent = new GUIContent(windowTitle);
            window.Show();
        }

        Vector2 m_LogsScrollView;
        Color m_DarkWindow = new Color(0, 0, 0, 0.2f);
        Vector2 m_ScrollView;
        bool m_SettingReady;
        string m_Logs;

        Dictionary<int, Texture> udimMapping;// = new Dictionary<int, Texture>();
        Dictionary<int, List<Material>> udimMaterial;// = new Dictionary<int, Material>();

        void OnGUI()
        {
            EditorGUILayout.Space();
            m_ScrollView = EditorGUILayout.BeginScrollView(m_ScrollView);

            m_MaterialTextureSettings = EditorGUILayout.ObjectField("Search Settings: ",
                m_MaterialTextureSettings, typeof(MaterialTextureSettings), false) as MaterialTextureSettings;

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            if (m_MaterialTextureSettings != null && m_MaterialTextureSettings.searchSettings.Length > 0)
            {

                var serializedObject = new SerializedObject(m_MaterialTextureSettings);
                var serailizedProp = serializedObject.FindProperty("m_SearchSettings");
                for (var i = 0; i < m_MaterialTextureSettings.searchSettings.Length; i++)
                {
                    var textureSearch = m_MaterialTextureSettings.searchSettings[i];
                    SearchSettingsDrawer(serializedObject, serailizedProp.GetArrayElementAtIndex(i), textureSearch, i);
                }
                EditorGUILayout.Separator();
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            m_SettingReady = m_MaterialTextureSettings != null && m_MaterialTextureSettings.searchSettings.Length > 0;
            EditorGUI.BeginDisabledGroup(!m_SettingReady);
            if (GUILayout.Button("Apply All"))
            {
                for (var i = 0; i < m_MaterialTextureSettings.searchSettings.Length; i++)
                {
                    var textureSearch = m_MaterialTextureSettings.searchSettings[i];
                    ApplySettingsToSelection(textureSearch);
                }
            }
            if (GUILayout.Button("Cleaar All"))
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

        void SearchSettingsDrawer(SerializedObject serializedObject, SerializedProperty serializedProperty, MaterialTextureSettings.TextureSearchSettings textureSearch, int index = -1)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedProperty, new GUIContent(string.Format("Element {0}: {1}", index, textureSearch.textureName)), true);
            serializedObject.ApplyModifiedProperties();

            var path = string.Format("{0}{1}{2}",
                Application.dataPath,
                Path.AltDirectorySeparatorChar, textureSearch.searchDir);
            var colorTags = "red";
            if (Directory.Exists(path))
            {
                colorTags = "green";
            }

            var ritchPath = string.Format("<size=12><color={0}>{1}</color></size>", colorTags, path);
            var style = new GUIStyle(EditorStyles.helpBox);
            style.richText = true;
            EditorGUILayout.TextArea(ritchPath, style);

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
                    m_Logs += string.Format("{0} check if has propery {1} : {2}\n", Selection.objects[0].name, textureSearch.textureName, hasProp);
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
            udimMapping = new Dictionary<int, Texture>();
            udimMaterial = new Dictionary<int, List<Material>>();
            var path = string.Format("{0}{1}{2}",
                Application.dataPath,
                Path.AltDirectorySeparatorChar, textureSearch.searchDir);
            if (!Directory.Exists(path))
            {
                m_Logs += string.Format("Error! Path: '{0}' does not exist!\n", path);
                return;
            }

            var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
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
                int id;
                if (int.TryParse(nameSplit[0], out id))
                {
                    udimMapping[id] = texture;
                    m_Logs += string.Format("Texture id: {0} name: {1} found\n", id, texture.name);
                }
            }

            var objectSelection = Selection.objects;
            foreach (var obj in objectSelection)
            {
                var mat = obj as Material;
                if (mat != null)
                {
                    var nameSplit = mat.name.Split('_');
                    int id;
                    if (int.TryParse(nameSplit[nameSplit.Length - 1], out id))
                    {
                        if (!udimMaterial.ContainsKey(id))
                        {
                            udimMaterial[id] = new List<Material>();
                        }
                        udimMaterial[id].Add(mat);
                        m_Logs += string.Format("Material id: {0} name: {1} found\n", id, mat.name);
                    }

                }
            }

            foreach (var kp in udimMaterial)
            {
                if (udimMapping.ContainsKey(kp.Key))
                {
                    foreach (var mat in kp.Value)
                    {
                        mat.SetTexture(textureSearch.textureName, udimMapping[kp.Key]);
                        var texName = udimMapping[kp.Key].name;
                        m_Logs += string.Format("{0} had texture {1} at {2}", mat.name, texName, textureSearch.textureName);
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

