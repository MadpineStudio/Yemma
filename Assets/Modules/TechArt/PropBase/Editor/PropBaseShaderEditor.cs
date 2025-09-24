using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PropBase.Editor
{
    [System.Serializable]
    public class LayerSession
    {
        public string sessionName = "New Session";
        public Texture2D baseTexture = null;
        public Color baseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Vector4 baseUV = new Vector4(1, 1, 0, 0);
        public bool useLayerMask = false;
        public Texture2D layerMask = null;
    
    [System.Serializable]
    public class LayerData
    {
        public bool useLayer = false;
        public Texture2D texture = null;
        public Color color = Color.white;
        public float intensity = 1f;
        public Vector4 uv = new Vector4(1, 1, 0, 0);
        public bool usePBR = true;
        public Texture2D normal = null;
        public float normalScale = 1f;
        public float metallic = 0f;
        public float smoothness = 0.5f;
        public Color emission = Color.black;
        public float emissionIntensity = 1f;
        
        // MatCap properties
        public bool useMatCap = false;
        public Texture2D matCapTexture = null;
        public float matCapIntensity = 1f;
        public float matCapContrast = 1f;
        public float matCapSaturation = 1f;
        public float matCapRotation = 0f;
    }
    
    public LayerData layer01 = new LayerData();
    public LayerData layer02 = new LayerData();
    public LayerData layer03 = new LayerData();
    public LayerData layer04 = new LayerData();
}

    [System.Serializable]
    public class PropBaseSessionData : ScriptableObject
    {
        public List<LayerSession> sessions = new List<LayerSession>();
        public int selectedSessionIndex = 0;
    }

    public class PropBaseShaderEditor : ShaderGUI
    {
    private static PropBaseSessionData sessionData;
    private static string sessionDataPath = "Assets/Modules/TechArt/PropBase/PropBaseSessions.asset";
    
    private bool showLayers = true;
    private bool[] layerFoldouts = new bool[4] { true, false, false, false };
    
    private MaterialProperty FindPropertySafe(string propertyName, MaterialProperty[] properties)
    {
        foreach (var prop in properties)
        {
            if (prop.name == propertyName)
                return prop;
        }
        return null;
    }
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        InitializeSessionData();
        
        EditorGUILayout.Space(10);
        DrawSessionControls(materialEditor, properties);
        EditorGUILayout.Space(10);
        
        DrawBaseLayerProperties(materialEditor, properties);
        DrawLayerMaskProperties(materialEditor, properties);
        DrawLayerProperties(materialEditor, properties);
    }
    
    private void InitializeSessionData()
    {
        if (sessionData == null)
        {
            sessionData = AssetDatabase.LoadAssetAtPath<PropBaseSessionData>(sessionDataPath);
            if (sessionData == null)
            {
                sessionData = ScriptableObject.CreateInstance<PropBaseSessionData>();
                sessionData.sessions.Add(new LayerSession { sessionName = "Default Session" });
                
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(sessionDataPath));
                AssetDatabase.CreateAsset(sessionData, sessionDataPath);
                AssetDatabase.SaveAssets();
            }
        }
    }
    
    private void DrawSessionControls(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Layer Sessions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // Session dropdown
        string[] sessionNames = new string[sessionData.sessions.Count];
        for (int i = 0; i < sessionData.sessions.Count; i++)
        {
            sessionNames[i] = sessionData.sessions[i].sessionName;
        }
        
        int newIndex = EditorGUILayout.Popup("Current Session", sessionData.selectedSessionIndex, sessionNames);
        if (newIndex != sessionData.selectedSessionIndex)
        {
            sessionData.selectedSessionIndex = newIndex;
            LoadSession(materialEditor, properties);
            EditorUtility.SetDirty(sessionData);
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        // Session controls
        if (GUILayout.Button("Save Current", GUILayout.Width(100)))
        {
            SaveCurrentSession(materialEditor, properties);
        }
        
        if (GUILayout.Button("New Session", GUILayout.Width(100)))
        {
            CreateNewSession();
        }
        
        if (GUILayout.Button("Rename", GUILayout.Width(80)))
        {
            RenameCurrentSession();
        }
        
        if (sessionData.sessions.Count > 1 && GUILayout.Button("Delete", GUILayout.Width(80)))
        {
            DeleteCurrentSession(materialEditor, properties);
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    
    private void DrawBaseLayerProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Base Layer", EditorStyles.boldLabel);
        
        var baseTextureProp = FindPropertySafe("_BaseTexture", properties);
        if (baseTextureProp != null)
            materialEditor.ShaderProperty(baseTextureProp, "Base Texture");
        
        var baseColorProp = FindPropertySafe("_BaseColor", properties);
        if (baseColorProp != null)
            materialEditor.ShaderProperty(baseColorProp, "Base Color");
        
        var baseUVProp = FindPropertySafe("_BaseUV", properties);
        if (baseUVProp != null)
            materialEditor.ShaderProperty(baseUVProp, "Base UV (Scale XY, Offset ZW)");
        
        if (baseTextureProp == null || baseColorProp == null || baseUVProp == null)
        {
            EditorGUILayout.HelpBox("Some Base Layer properties not found in shader!", MessageType.Warning);
        }
    }
    
    private void DrawLayerMaskProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Layer Masks", EditorStyles.boldLabel);
        
        materialEditor.ShaderProperty(FindProperty("_UseLayerMask", properties), "Use Layer Mask");
        
        if (FindProperty("_UseLayerMask", properties).floatValue > 0.5f)
        {
            EditorGUI.indentLevel++;
            materialEditor.ShaderProperty(FindProperty("_LayerMask", properties), "Layer Mask (RGBA)");
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawLayerProperties(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        EditorGUILayout.Space(5);
        showLayers = EditorGUILayout.Foldout(showLayers, "Layers", true, EditorStyles.foldoutHeader);
        
        if (showLayers)
        {
            EditorGUI.indentLevel++;
            
            DrawSingleLayer(materialEditor, properties, "01", "R Channel", 0, Color.red);
            DrawSingleLayer(materialEditor, properties, "02", "G Channel", 1, Color.green);
            DrawSingleLayer(materialEditor, properties, "03", "B Channel", 2, Color.blue);
            DrawSingleLayer(materialEditor, properties, "04", "A Channel", 3, new Color(1f, 1f, 1f, 0.7f));
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void DrawSingleLayer(MaterialEditor materialEditor, MaterialProperty[] properties, string layerNum, string channelInfo, int layerIndex, Color headerColor)
    {
        EditorGUILayout.Space(3);
        
        var headerBackgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = headerColor;
        
        layerFoldouts[layerIndex] = EditorGUILayout.Foldout(layerFoldouts[layerIndex], $"Layer {layerNum} - {channelInfo}", true, EditorStyles.foldoutHeader);
        
        GUI.backgroundColor = headerBackgroundColor;
        
        if (layerFoldouts[layerIndex])
        {
            EditorGUI.indentLevel++;
            
            materialEditor.ShaderProperty(FindProperty($"_UseLayer{layerNum}", properties), $"Use Layer {layerNum}");
            
            if (FindProperty($"_UseLayer{layerNum}", properties).floatValue > 0.5f)
            {
                EditorGUI.indentLevel++;
                
                // Check if MatCap property exists
                var matCapProperty = FindPropertySafe($"_Layer{layerNum}UseMatCap", properties);
                if (matCapProperty == null)
                {
                    EditorGUILayout.HelpBox($"MatCap property _Layer{layerNum}UseMatCap not found in shader! Please recompile shader.", MessageType.Error);
                    
                    // Show available properties for debugging
                    EditorGUILayout.LabelField("Available Properties:", EditorStyles.boldLabel);
                    foreach (var prop in properties)
                    {
                        if (prop.name.Contains($"Layer{layerNum}"))
                            EditorGUILayout.LabelField($"- {prop.name}");
                    }
                    return;
                }
                
                // Mode selection - MatCap or PBR
                EditorGUILayout.Space(3);
                var buttonBackgroundColor = GUI.backgroundColor;
                
                bool useMatCap = matCapProperty.floatValue > 0.5f;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rendering Mode:", EditorStyles.boldLabel, GUILayout.Width(120));
                
                GUI.backgroundColor = useMatCap ? Color.cyan : Color.gray;
                if (GUILayout.Button("MatCap", useMatCap ? EditorStyles.miniButtonLeft : EditorStyles.miniButtonLeft))
                {
                    matCapProperty.floatValue = 1f;
                    useMatCap = true;
                }
                
                GUI.backgroundColor = !useMatCap ? Color.green : Color.gray;
                if (GUILayout.Button("PBR", !useMatCap ? EditorStyles.miniButtonRight : EditorStyles.miniButtonRight))
                {
                    matCapProperty.floatValue = 0f;
                    useMatCap = false;
                }
                
                GUI.backgroundColor = buttonBackgroundColor;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                if (useMatCap)
                {
                    // MatCap Mode
                    EditorGUILayout.LabelField("🌟 MatCap Mode", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.Space(3);
                    
                    var matCapTexProperty = FindPropertySafe($"_Layer{layerNum}MatCapTexture", properties);
                    if (matCapTexProperty != null)
                        materialEditor.ShaderProperty(matCapTexProperty, "MatCap Texture");
                    else
                        EditorGUILayout.HelpBox("MatCap Texture property not found!", MessageType.Warning);
                    
                    materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Color", properties), "Color Tint");
                    materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Intensity", properties), "Intensity");
                    
                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("MatCap Settings", EditorStyles.miniBoldLabel);
                    
                    var intensityProp = FindPropertySafe($"_Layer{layerNum}MatCapIntensity", properties);
                    if (intensityProp != null)
                        materialEditor.ShaderProperty(intensityProp, "MatCap Intensity");
                    
                    var contrastProp = FindPropertySafe($"_Layer{layerNum}MatCapContrast", properties);
                    if (contrastProp != null)
                        materialEditor.ShaderProperty(contrastProp, "Contrast");
                    
                    var saturationProp = FindPropertySafe($"_Layer{layerNum}MatCapSaturation", properties);
                    if (saturationProp != null)
                        materialEditor.ShaderProperty(saturationProp, "Saturation");
                    
                    var rotationProp = FindPropertySafe($"_Layer{layerNum}MatCapRotation", properties);
                    if (rotationProp != null)
                        materialEditor.ShaderProperty(rotationProp, "Rotation");
                }
                else
                {
                    // PBR Mode
                    EditorGUILayout.LabelField("⚙️ PBR Mode", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.Space(3);
                    
                    // Basic properties
                    materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Texture", properties), "Albedo Texture");
                    materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Color", properties), "Color");
                    materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Intensity", properties), "Intensity");
                    
                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("UV Transform", EditorStyles.miniBoldLabel);
                    materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}UV", properties), "UV (Scale XY, Offset ZW)");
                    
                    EditorGUILayout.Space(3);
                    
                    // PBR Toggle
                    var pbrToggleProperty = FindPropertySafe($"_Layer{layerNum}UsePBR", properties);
                    if (pbrToggleProperty != null)
                    {
                        materialEditor.ShaderProperty(pbrToggleProperty, "Use PBR Parameters");
                        
                        if (pbrToggleProperty.floatValue > 0.5f)
                        {
                            EditorGUILayout.LabelField("PBR Properties", EditorStyles.miniBoldLabel);
                            materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Normal", properties), "Normal Map");
                            materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}NormalScale", properties), "Normal Scale");
                            materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Metallic", properties), "Metallic");
                            materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Smoothness", properties), "Smoothness");
                            
                            EditorGUILayout.Space(3);
                            EditorGUILayout.LabelField("Emission", EditorStyles.miniBoldLabel);
                            materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Emission", properties), "Emission Color");
                            materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}EmissionIntensity", properties), "Emission Intensity");
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("PBR parameters disabled for better performance. Only albedo texture and color will be used.", MessageType.Info);
                        }
                    }
                    else
                    {
                        // Fallback - show all PBR properties if toggle not found
                        EditorGUILayout.LabelField("PBR Properties", EditorStyles.miniBoldLabel);
                        materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Normal", properties), "Normal Map");
                        materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}NormalScale", properties), "Normal Scale");
                        materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Metallic", properties), "Metallic");
                        materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Smoothness", properties), "Smoothness");
                        
                        EditorGUILayout.Space(3);
                        EditorGUILayout.LabelField("Emission", EditorStyles.miniBoldLabel);
                        materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}Emission", properties), "Emission Color");
                        materialEditor.ShaderProperty(FindProperty($"_Layer{layerNum}EmissionIntensity", properties), "Emission Intensity");
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.indentLevel--;
        }
    }
    
    private void SaveCurrentSession(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (sessionData.selectedSessionIndex >= 0 && sessionData.selectedSessionIndex < sessionData.sessions.Count)
        {
            var session = sessionData.sessions[sessionData.selectedSessionIndex];
            
            // Save base layer
            var baseTextureProp = FindPropertySafe("_BaseTexture", properties);
            if (baseTextureProp != null)
                session.baseTexture = baseTextureProp.textureValue as Texture2D;
                
            var baseColorProp = FindPropertySafe("_BaseColor", properties);
            if (baseColorProp != null)
                session.baseColor = baseColorProp.colorValue;
                
            var baseUVProp = FindPropertySafe("_BaseUV", properties);
            if (baseUVProp != null)
                session.baseUV = baseUVProp.vectorValue;
            
            // Save layer mask
            session.useLayerMask = FindProperty("_UseLayerMask", properties).floatValue > 0.5f;
            session.layerMask = FindProperty("_LayerMask", properties).textureValue as Texture2D;
            
            // Save layers
            SaveLayerData(session.layer01, properties, "01");
            SaveLayerData(session.layer02, properties, "02");
            SaveLayerData(session.layer03, properties, "03");
            SaveLayerData(session.layer04, properties, "04");
            
            EditorUtility.SetDirty(sessionData);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Session '{session.sessionName}' saved successfully!");
        }
    }
    
    private void SaveLayerData(LayerSession.LayerData layerData, MaterialProperty[] properties, string layerNum)
    {
        layerData.useLayer = FindProperty($"_UseLayer{layerNum}", properties).floatValue > 0.5f;
        layerData.texture = FindProperty($"_Layer{layerNum}Texture", properties).textureValue as Texture2D;
        layerData.color = FindProperty($"_Layer{layerNum}Color", properties).colorValue;
        layerData.intensity = FindProperty($"_Layer{layerNum}Intensity", properties).floatValue;
        layerData.uv = FindProperty($"_Layer{layerNum}UV", properties).vectorValue;
        
        // Save PBR toggle
        var pbrProperty = FindPropertySafe($"_Layer{layerNum}UsePBR", properties);
        layerData.usePBR = pbrProperty != null ? pbrProperty.floatValue > 0.5f : true;
        
        layerData.normal = FindProperty($"_Layer{layerNum}Normal", properties).textureValue as Texture2D;
        layerData.normalScale = FindProperty($"_Layer{layerNum}NormalScale", properties).floatValue;
        layerData.metallic = FindProperty($"_Layer{layerNum}Metallic", properties).floatValue;
        layerData.smoothness = FindProperty($"_Layer{layerNum}Smoothness", properties).floatValue;
        layerData.emission = FindProperty($"_Layer{layerNum}Emission", properties).colorValue;
        layerData.emissionIntensity = FindProperty($"_Layer{layerNum}EmissionIntensity", properties).floatValue;
        
        // Save MatCap properties
        layerData.useMatCap = FindProperty($"_Layer{layerNum}UseMatCap", properties).floatValue > 0.5f;
        layerData.matCapTexture = FindProperty($"_Layer{layerNum}MatCapTexture", properties).textureValue as Texture2D;
        layerData.matCapIntensity = FindProperty($"_Layer{layerNum}MatCapIntensity", properties).floatValue;
        layerData.matCapContrast = FindProperty($"_Layer{layerNum}MatCapContrast", properties).floatValue;
        layerData.matCapSaturation = FindProperty($"_Layer{layerNum}MatCapSaturation", properties).floatValue;
        layerData.matCapRotation = FindProperty($"_Layer{layerNum}MatCapRotation", properties).floatValue;
    }
    
    private void LoadSession(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (sessionData.selectedSessionIndex >= 0 && sessionData.selectedSessionIndex < sessionData.sessions.Count)
        {
            var session = sessionData.sessions[sessionData.selectedSessionIndex];
            
            // Load base layer
            var baseTextureProp = FindPropertySafe("_BaseTexture", properties);
            if (baseTextureProp != null)
                baseTextureProp.textureValue = session.baseTexture;
                
            var baseColorProp = FindPropertySafe("_BaseColor", properties);
            if (baseColorProp != null)
                baseColorProp.colorValue = session.baseColor;
                
            var baseUVProp = FindPropertySafe("_BaseUV", properties);
            if (baseUVProp != null)
                baseUVProp.vectorValue = session.baseUV;
            
            // Load layer mask
            FindProperty("_UseLayerMask", properties).floatValue = session.useLayerMask ? 1f : 0f;
            FindProperty("_LayerMask", properties).textureValue = session.layerMask;
            
            // Load layers
            LoadLayerData(session.layer01, properties, "01");
            LoadLayerData(session.layer02, properties, "02");
            LoadLayerData(session.layer03, properties, "03");
            LoadLayerData(session.layer04, properties, "04");
            
            Debug.Log($"Session '{session.sessionName}' loaded successfully!");
        }
    }
    
    private void LoadLayerData(LayerSession.LayerData layerData, MaterialProperty[] properties, string layerNum)
    {
        FindProperty($"_UseLayer{layerNum}", properties).floatValue = layerData.useLayer ? 1f : 0f;
        FindProperty($"_Layer{layerNum}Texture", properties).textureValue = layerData.texture;
        FindProperty($"_Layer{layerNum}Color", properties).colorValue = layerData.color;
        FindProperty($"_Layer{layerNum}Intensity", properties).floatValue = layerData.intensity;
        FindProperty($"_Layer{layerNum}UV", properties).vectorValue = layerData.uv;
        
        // Load PBR toggle
        var pbrProperty = FindPropertySafe($"_Layer{layerNum}UsePBR", properties);
        if (pbrProperty != null)
            pbrProperty.floatValue = layerData.usePBR ? 1f : 0f;
        
        FindProperty($"_Layer{layerNum}Normal", properties).textureValue = layerData.normal;
        FindProperty($"_Layer{layerNum}NormalScale", properties).floatValue = layerData.normalScale;
        FindProperty($"_Layer{layerNum}Metallic", properties).floatValue = layerData.metallic;
        FindProperty($"_Layer{layerNum}Smoothness", properties).floatValue = layerData.smoothness;
        FindProperty($"_Layer{layerNum}Emission", properties).colorValue = layerData.emission;
        FindProperty($"_Layer{layerNum}EmissionIntensity", properties).floatValue = layerData.emissionIntensity;
        
        // Load MatCap properties
        FindProperty($"_Layer{layerNum}UseMatCap", properties).floatValue = layerData.useMatCap ? 1f : 0f;
        FindProperty($"_Layer{layerNum}MatCapTexture", properties).textureValue = layerData.matCapTexture;
        FindProperty($"_Layer{layerNum}MatCapIntensity", properties).floatValue = layerData.matCapIntensity;
        FindProperty($"_Layer{layerNum}MatCapContrast", properties).floatValue = layerData.matCapContrast;
        FindProperty($"_Layer{layerNum}MatCapSaturation", properties).floatValue = layerData.matCapSaturation;
        FindProperty($"_Layer{layerNum}MatCapRotation", properties).floatValue = layerData.matCapRotation;
    }
    
    private void CreateNewSession()
    {
        var newSession = new LayerSession();
        newSession.sessionName = $"Session {sessionData.sessions.Count + 1}";
        sessionData.sessions.Add(newSession);
        sessionData.selectedSessionIndex = sessionData.sessions.Count - 1;
        
        EditorUtility.SetDirty(sessionData);
        AssetDatabase.SaveAssets();
    }
    
    private void RenameCurrentSession()
    {
        if (sessionData.selectedSessionIndex >= 0 && sessionData.selectedSessionIndex < sessionData.sessions.Count)
        {
            string newName = EditorInputDialog.Show("Rename Session", "Enter new session name:", sessionData.sessions[sessionData.selectedSessionIndex].sessionName);
            if (!string.IsNullOrEmpty(newName))
            {
                sessionData.sessions[sessionData.selectedSessionIndex].sessionName = newName;
                EditorUtility.SetDirty(sessionData);
                AssetDatabase.SaveAssets();
            }
        }
    }
    
    private void DeleteCurrentSession(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        if (sessionData.sessions.Count > 1 && EditorUtility.DisplayDialog("Delete Session", 
            $"Are you sure you want to delete session '{sessionData.sessions[sessionData.selectedSessionIndex].sessionName}'?", 
            "Delete", "Cancel"))
        {
            sessionData.sessions.RemoveAt(sessionData.selectedSessionIndex);
            sessionData.selectedSessionIndex = Mathf.Max(0, sessionData.selectedSessionIndex - 1);
            LoadSession(materialEditor, properties);
            
            EditorUtility.SetDirty(sessionData);
            AssetDatabase.SaveAssets();
        }
    }
    }

    // Utility class for input dialog
    public class EditorInputDialog : EditorWindow
    {
        private string inputText = "";
        private string description = "";
        private new string title = "";
        private System.Action<string> onComplete;    public static string Show(string title, string description, string defaultValue = "")
    {
        var window = GetWindow<EditorInputDialog>(true, title);
        window.title = title;
        window.description = description;
        window.inputText = defaultValue;
        window.minSize = new Vector2(300, 100);
        window.maxSize = new Vector2(400, 120);
        window.ShowModal();
        
        return window.inputText;
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(description);
        EditorGUILayout.Space(5);
        
        GUI.SetNextControlName("InputField");
        inputText = EditorGUILayout.TextField(inputText);
        
        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.FocusTextInControl("InputField");
        }
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("OK", GUILayout.Width(60)) || (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown))
        {
            Close();
        }
        
        if (GUILayout.Button("Cancel", GUILayout.Width(60)) || (Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyDown))
        {
            inputText = "";
            Close();
        }
        
        EditorGUILayout.EndHorizontal();
    }
}
}