using UnityEngine;
using UnityEditor;

public class AnalyticEyeEditor : ShaderGUI
{
    MaterialEditor m_MaterialEditor;
    Material m_Material;
    
    // Property references
    MaterialProperty sphereOffset;
    MaterialProperty sphereRadius;
    MaterialProperty rotationEuler;
    MaterialProperty projectionType;
    MaterialProperty invertSphere;
    MaterialProperty backgroundColor;
    
    // Layer properties
    MaterialProperty[] layerTextures = new MaterialProperty[3];
    MaterialProperty[] layerScales = new MaterialProperty[3];
    MaterialProperty[] layerOffsets = new MaterialProperty[3];
    MaterialProperty[] layerBlends = new MaterialProperty[3];
    MaterialProperty[] layerOpacities = new MaterialProperty[3];
    MaterialProperty[] layerRadius = new MaterialProperty[3];
    MaterialProperty[] layerSphereScales = new MaterialProperty[3];
    MaterialProperty[] layerSphereOffsets = new MaterialProperty[3];
    MaterialProperty[] layerInverts = new MaterialProperty[3];
    MaterialProperty[] layerFresnelPowers = new MaterialProperty[3];
    MaterialProperty[] layerFresnelIntensities = new MaterialProperty[3];

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialEditor = materialEditor;
        m_Material = materialEditor.target as Material;
        
        FindProperties(properties);
        DrawGUI();
    }

    void FindProperties(MaterialProperty[] properties)
    {
        sphereOffset = FindProperty("_SphereOffset", properties);
        sphereRadius = FindProperty("_SphereRadius", properties);
        rotationEuler = FindProperty("_RotationEuler", properties);
        projectionType = FindProperty("_ProjectionType", properties);
        invertSphere = FindProperty("_InvertSphere", properties);
        backgroundColor = FindProperty("_BackgroundColor", properties);
        
        for(int i = 0; i < 3; i++)
        {
            int layerNum = i + 1;
            layerTextures[i] = FindProperty($"_Layer{layerNum}Texture", properties);
            layerScales[i] = FindProperty($"_Layer{layerNum}Scale", properties);
            layerOffsets[i] = FindProperty($"_Layer{layerNum}Offset", properties);
            layerBlends[i] = FindProperty($"_Layer{layerNum}Blend", properties);
            layerOpacities[i] = FindProperty($"_Layer{layerNum}Opacity", properties);
            layerRadius[i] = FindProperty($"_Layer{layerNum}Radius", properties);
            layerSphereScales[i] = FindProperty($"_Layer{layerNum}SphereScale", properties);
            layerSphereOffsets[i] = FindProperty($"_Layer{layerNum}SphereOffset", properties);
            layerInverts[i] = FindProperty($"_Layer{layerNum}Invert", properties);
            layerFresnelPowers[i] = FindProperty($"_Layer{layerNum}FresnelPower", properties);
            layerFresnelIntensities[i] = FindProperty($"_Layer{layerNum}FresnelIntensity", properties);
        }
    }

    void DrawGUI()
    {
        EditorGUILayout.Space();
        
        // General Settings
        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        m_MaterialEditor.ShaderProperty(backgroundColor, "Background Color");
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space();
        
        // Sphere Settings
        EditorGUILayout.LabelField("Sphere Configuration", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        m_MaterialEditor.ShaderProperty(sphereOffset, "Sphere Offset");
        m_MaterialEditor.ShaderProperty(sphereRadius, "Sphere Radius");
        
        // Rotation Euler as Vector3
        Vector4 rotation = rotationEuler.vectorValue;
        EditorGUI.BeginChangeCheck();
        Vector3 rotationXYZ = EditorGUILayout.Vector3Field("Rotation Euler (X, Y, Z)", new Vector3(rotation.x, rotation.y, rotation.z));
        if (EditorGUI.EndChangeCheck())
        {
            rotationEuler.vectorValue = new Vector4(rotationXYZ.x, rotationXYZ.y, rotationXYZ.z, rotation.w);
        }
        
        // Projection Type Dropdown
        string[] projectionOptions = { "Spherical", "Planar", "Cylindrical", "Radial (Iris)" };
        EditorGUI.BeginChangeCheck();
        int projectionIndex = EditorGUILayout.Popup("Projection Type", (int)projectionType.floatValue, projectionOptions);
        if (EditorGUI.EndChangeCheck())
        {
            projectionType.floatValue = projectionIndex;
        }
        
        m_MaterialEditor.ShaderProperty(invertSphere, "Invert Sphere (Concave)");
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(10);
        
        // Layer Settings
        for(int i = 0; i < 3; i++)
        {
            DrawLayerSection(i + 1, i);
            EditorGUILayout.Space(5);
        }
    }

    void DrawLayerSection(int layerNumber, int index)
    {
        EditorGUILayout.LabelField($"Layer {layerNumber}", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;
        
        // Texture
        m_MaterialEditor.TextureProperty(layerTextures[index], $"Layer {layerNumber} Texture");
        
        // Transform
        EditorGUILayout.LabelField("Transform", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        
        // Escala 3D (X, Y separados)
        Vector4 scale = layerScales[index].vectorValue;
        EditorGUI.BeginChangeCheck();
        Vector2 scaleXY = EditorGUILayout.Vector2Field("Scale (X, Y)", new Vector2(scale.x, scale.y));
        if (EditorGUI.EndChangeCheck())
        {
            layerScales[index].vectorValue = new Vector4(scaleXY.x, scaleXY.y, scale.z, scale.w);
        }
        
        m_MaterialEditor.ShaderProperty(layerOffsets[index], "Offset");
        EditorGUI.indentLevel--;
        
        // Sphere
        EditorGUILayout.LabelField("Sphere Properties", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        m_MaterialEditor.ShaderProperty(layerRadius[index], "Sphere Radius");
        
        // Posição da Esfera 3D (X, Y, Z separados)
        Vector4 sphereOffset = layerSphereOffsets[index].vectorValue;
        EditorGUI.BeginChangeCheck();
        Vector3 sphereOffsetXYZ = EditorGUILayout.Vector3Field("Sphere Position (X, Y, Z)", new Vector3(sphereOffset.x, sphereOffset.y, sphereOffset.z));
        if (EditorGUI.EndChangeCheck())
        {
            layerSphereOffsets[index].vectorValue = new Vector4(sphereOffsetXYZ.x, sphereOffsetXYZ.y, sphereOffsetXYZ.z, sphereOffset.w);
        }
        
        // Escala da Esfera 3D (X, Y, Z separados)
        Vector4 sphereScale = layerSphereScales[index].vectorValue;
        EditorGUI.BeginChangeCheck();
        Vector3 sphereScaleXYZ = EditorGUILayout.Vector3Field("Sphere Scale (X, Y, Z)", new Vector3(sphereScale.x, sphereScale.y, sphereScale.z));
        if (EditorGUI.EndChangeCheck())
        {
            layerSphereScales[index].vectorValue = new Vector4(sphereScaleXYZ.x, sphereScaleXYZ.y, sphereScaleXYZ.z, sphereScale.w);
        }
        
        m_MaterialEditor.ShaderProperty(layerInverts[index], "Invert (Concave)");
        EditorGUI.indentLevel--;
        
        // Fresnel
        EditorGUILayout.LabelField("Fresnel Effect", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        m_MaterialEditor.ShaderProperty(layerFresnelIntensities[index], "Fresnel Intensity");
        if(layerFresnelIntensities[index].floatValue > 0.0f)
        {
            m_MaterialEditor.ShaderProperty(layerFresnelPowers[index], "Fresnel Power");
        }
        EditorGUI.indentLevel--;
        
        // Blending
        EditorGUILayout.LabelField("Blending", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        string[] blendOptions = { "Normal", "Multiply", "Screen", "Overlay", "Add" };
        EditorGUI.BeginChangeCheck();
        int blendIndex = EditorGUILayout.Popup("Blend Mode", (int)layerBlends[index].floatValue, blendOptions);
        if (EditorGUI.EndChangeCheck())
        {
            layerBlends[index].floatValue = blendIndex;
        }
        
        m_MaterialEditor.ShaderProperty(layerOpacities[index], "Opacity");
        EditorGUI.indentLevel--;
        
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }
}
