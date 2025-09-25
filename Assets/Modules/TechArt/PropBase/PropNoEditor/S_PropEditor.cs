using UnityEditor;
using UnityEngine;

public class S_PropEditor : ShaderGUI
{
    MaterialEditor m_MaterialEditor;
    MaterialProperty[] m_Properties;
    
    // Foldout states
    bool m_BaseFoldout = true;
    bool m_Layer0Foldout = false;
    bool m_Layer1Foldout = false;
    bool m_Layer2Foldout = false;
    bool m_Layer3Foldout = false;
    bool m_UVFoldout = true;
    bool m_GlobalFoldout = false;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialEditor = materialEditor;
        m_Properties = properties;

        Material material = materialEditor.target as Material;

        // Ensure keywords are set correctly
        UpdateShaderKeywords(material);

        EditorGUI.BeginChangeCheck();

        DrawBaseSection();
        DrawLayerMaskSection();
        DrawLayerSection("Layer 0 (Red Channel)", ref m_Layer0Foldout, "LayerR");
        DrawLayerSection("Layer 1 (Green Channel)", ref m_Layer1Foldout, "LayerG");
        DrawLayerSection("Layer 2 (Blue Channel)", ref m_Layer2Foldout, "LayerB");
        DrawLayerSection("Layer 3 (Alpha Channel)", ref m_Layer3Foldout, "LayerA");
        DrawUVSection();
        DrawGlobalSection();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(material);
        }
    }

    void DrawBaseSection()
    {
        m_BaseFoldout = EditorGUILayout.Foldout(m_BaseFoldout, "Base Material", true, EditorStyles.foldoutHeader);
        if (m_BaseFoldout)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_BaseColor", "Base Color");
            DrawProperty("_BaseMap", "Base Map");
            DrawProperty("_NormalMap", "Normal Map");
            DrawProperty("_PhysicsMap", "Physics Map (R=Rough, G=Metal, B=AO)");
            DrawProperty("_BaseSpecular", "Reflectance");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);
    }

    void DrawLayerMaskSection()
    {
        EditorGUILayout.LabelField("Layer Blending", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DrawProperty("_MaskMap", "Layer Mask (R=L0, G=L1, B=L2, A=L3)");
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
    }

    void DrawLayerSection(string title, ref bool foldout, string prefix)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Enable/Disable toggle
        MaterialProperty toggleProp = FindProperty($"_Use{prefix}", m_Properties, false);
        bool isEnabled = toggleProp != null && toggleProp.floatValue > 0.5f;
        
        GUI.enabled = true;
        bool newEnabled = EditorGUILayout.Toggle(isEnabled, GUILayout.Width(20));
        if (newEnabled != isEnabled && toggleProp != null)
        {
            toggleProp.floatValue = newEnabled ? 1.0f : 0.0f;
            // Update shader keywords
            Material material = m_MaterialEditor.target as Material;
            if (newEnabled)
                material.EnableKeyword($"USE_{prefix.ToUpper()}");
            else
                material.DisableKeyword($"USE_{prefix.ToUpper()}");
        }
        
        // Gray out if disabled
        GUI.enabled = isEnabled;
        
        foldout = EditorGUILayout.Foldout(foldout, title, true, EditorStyles.foldoutHeader);
        
        // Quick color preview
        MaterialProperty colorProp = FindProperty($"_{prefix}Color", m_Properties, false);
        if (colorProp != null && isEnabled)
        {
            EditorGUILayout.Space();
            Color oldColor = colorProp.colorValue;
            Color newColor = EditorGUILayout.ColorField(GUIContent.none, oldColor, false, true, false, GUILayout.Width(50));
            if (newColor != oldColor)
                colorProp.colorValue = newColor;
        }
        EditorGUILayout.EndHorizontal();
        
        if (foldout && isEnabled)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            DrawProperty($"_{prefix}Color", "Tint");
            EditorGUILayout.EndHorizontal();
            
            // Compact texture layout
            EditorGUILayout.BeginVertical("box");
            DrawTexturePropertyCompact($"_{prefix}_Map", "Albedo");
            DrawTexturePropertyCompact($"_{prefix}_NormalMap", "Normal");
            DrawTexturePropertyCompact($"_{prefix}_PhysicsMap", "Physics");
            EditorGUILayout.EndVertical();
            
            // Reflectance control
            DrawProperty($"_{prefix}Specular", "Reflectance");
            
            EditorGUI.indentLevel--;
        }
        
        GUI.enabled = true; // Reset GUI state
        EditorGUILayout.Space(3);
    }
    
    void DrawTexturePropertyCompact(string propertyName, string displayName)
    {
        MaterialProperty property = FindProperty(propertyName, m_Properties, false);
        if (property != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(displayName, GUILayout.Width(60));
            m_MaterialEditor.TexturePropertySingleLine(GUIContent.none, property);
            EditorGUILayout.EndHorizontal();
        }
    }

    void DrawUVSection()
    {
        m_UVFoldout = EditorGUILayout.Foldout(m_UVFoldout, "UV Controls", true, EditorStyles.foldoutHeader);
        if (m_UVFoldout)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_Tiling", "Tiling");
            DrawProperty("_Offset", "Offset");
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.Space(5);
    }

    void DrawGlobalSection()
    {
        m_GlobalFoldout = EditorGUILayout.Foldout(m_GlobalFoldout, "Global Adjustments", true, EditorStyles.foldoutHeader);
        if (m_GlobalFoldout)
        {
            EditorGUI.indentLevel++;
            DrawProperty("_NormalStrength", "Normal Strength");
            DrawProperty("_RoughAdd", "Roughness Modifier");
            DrawProperty("_MetalAdd", "Metallic Modifier");
            EditorGUI.indentLevel--;
        }
    }

    void DrawProperty(string propertyName, string displayName = null)
    {
        MaterialProperty property = FindProperty(propertyName, m_Properties, false);
        if (property != null)
        {
            string label = string.IsNullOrEmpty(displayName) ? property.displayName : displayName;
            m_MaterialEditor.ShaderProperty(property, label);
        }
    }

    void UpdateShaderKeywords(Material material)
    {
        // Update layer keywords based on toggle values
        string[] layers = { "LayerR", "LayerG", "LayerB", "LayerA" };
        string[] keywords = { "USE_LAYER_R", "USE_LAYER_G", "USE_LAYER_B", "USE_LAYER_A" };
        
        for (int i = 0; i < layers.Length; i++)
        {
            MaterialProperty toggleProp = FindProperty($"_Use{layers[i]}", m_Properties, false);
            if (toggleProp != null)
            {
                bool isEnabled = toggleProp.floatValue > 0.5f;
                if (isEnabled)
                    material.EnableKeyword(keywords[i]);
                else
                    material.DisableKeyword(keywords[i]);
            }
        }
    }
}