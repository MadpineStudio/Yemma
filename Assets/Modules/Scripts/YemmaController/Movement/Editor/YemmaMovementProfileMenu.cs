using UnityEngine;
using UnityEditor;
using Yemma.Movement.Data;
using Yemma.Movement.Utils;
using Yemma.Movement.Core;

namespace Yemma.Movement.Editor
{
    /// <summary>
    /// Menu customizado para criação de perfis de movimento
    /// </summary>
    public static class YemmaMovementProfileMenu
    {
        private const string MENU_ROOT = "Assets/Create/Yemma/Movement Profiles/";
        private const string PROFILES_FOLDER = "Assets/Modules/Scripts/YemmaController/Movement/Profiles/";

        [MenuItem(MENU_ROOT + "Default Profile", false, 1)]
        public static void CreateDefaultProfile()
        {
            CreateAndSaveProfile(YemmaMovementProfileFactory.CreateDefault(), "YemmaMovementProfile_Default");
        }

        [MenuItem(MENU_ROOT + "Slow Profile", false, 2)]
        public static void CreateSlowProfile()
        {
            CreateAndSaveProfile(YemmaMovementProfileFactory.CreateSlow(), "YemmaMovementProfile_Slow");
        }

        [MenuItem(MENU_ROOT + "Fast Profile", false, 3)]
        public static void CreateFastProfile()
        {
            CreateAndSaveProfile(YemmaMovementProfileFactory.CreateFast(), "YemmaMovementProfile_Fast");
        }

        [MenuItem(MENU_ROOT + "Smooth Profile", false, 4)]
        public static void CreateSmoothProfile()
        {
            CreateAndSaveProfile(YemmaMovementProfileFactory.CreateSmooth(), "YemmaMovementProfile_Smooth");
        }

        [MenuItem(MENU_ROOT + "Rough Terrain Profile", false, 5)]
        public static void CreateRoughTerrainProfile()
        {
            CreateAndSaveProfile(YemmaMovementProfileFactory.CreateRoughTerrain(), "YemmaMovementProfile_RoughTerrain");
        }

        [MenuItem(MENU_ROOT + "Precise Profile", false, 6)]
        public static void CreatePreciseProfile()
        {
            CreateAndSaveProfile(YemmaMovementProfileFactory.CreatePrecise(), "YemmaMovementProfile_Precise");
        }

        [MenuItem(MENU_ROOT + "Custom Profile", false, 20)]
        public static void CreateCustomProfile()
        {
            var profile = YemmaMovementProfileFactory.CreateDefault();
            profile.name = "Custom Movement Profile";
            CreateAndSaveProfile(profile, "YemmaMovementProfile_Custom");
        }

        private static void CreateAndSaveProfile(YemmaMovementProfile profile, string fileName)
        {
            // Garante que a pasta existe
            if (!AssetDatabase.IsValidFolder(PROFILES_FOLDER.TrimEnd('/')))
            {
                string[] folders = PROFILES_FOLDER.TrimEnd('/').Split('/');
                string currentPath = folders[0];
                
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }

            // Gera nome único se o arquivo já existir
            string path = PROFILES_FOLDER + fileName + ".asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            // Cria e salva o asset
            AssetDatabase.CreateAsset(profile, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Seleciona o asset criado
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);

            Debug.Log($"Created Yemma Movement Profile: {path}");
        }

        [MenuItem("Tools/Yemma/Movement System/Setup Yemma Controller", false, 100)]
        public static void SetupYemmaController()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Setup Yemma Controller", "Please select a GameObject first.", "OK");
                return;
            }

            GameObject selected = Selection.activeGameObject;
            
            // Adiciona YemmaController se não existir
            var yemmaController = selected.GetComponent<Yemma.YemmaController>();
            if (yemmaController == null)
            {
                yemmaController = selected.AddComponent<Yemma.YemmaController>();
            }

            // Adiciona YemmaMovementController se não existir
            var movementController = selected.GetComponent<YemmaMovementController>();
            if (movementController == null)
            {
                movementController = selected.AddComponent<YemmaMovementController>();
            }

            // Adiciona Rigidbody se não existir
            var rb = selected.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = selected.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.linearDamping = 0f;
                rb.angularDamping = 5f;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            // Adiciona InputManager se não existir
            var inputManager = selected.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("InputManager component not found. Please add it manually.");
            }

            EditorUtility.DisplayDialog("Setup Complete", 
                "Yemma Controller setup complete!\n\n" +
                "Components added:\n" +
                "- YemmaController\n" +
                "- YemmaMovementController\n" +
                "- Rigidbody\n\n" +
                "Don't forget to:\n" +
                "1. Add InputManager component\n" +
                "2. Assign a YemmaMovementProfile\n" +
                "3. Configure ground layers", "OK");

            // Marca a cena como modificada
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Tools/Yemma/Movement System/Validate All Profiles", false, 101)]
        public static void ValidateAllProfiles()
        {
            string[] guids = AssetDatabase.FindAssets("t:YemmaMovementProfile");
            int validProfiles = 0;
            int invalidProfiles = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                YemmaMovementProfile profile = AssetDatabase.LoadAssetAtPath<YemmaMovementProfile>(path);
                
                if (profile.ValidateProfile())
                {
                    validProfiles++;
                }
                else
                {
                    invalidProfiles++;
                    Debug.LogError($"Invalid profile: {profile.name} at {path}");
                }
            }

            EditorUtility.DisplayDialog("Profile Validation", 
                $"Validation complete!\n\n" +
                $"Valid profiles: {validProfiles}\n" +
                $"Invalid profiles: {invalidProfiles}\n\n" +
                (invalidProfiles > 0 ? "Check console for details." : "All profiles are valid!"), "OK");
        }
    }
}
