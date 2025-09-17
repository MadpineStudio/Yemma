using UnityEngine;
using UnityEditor;
using Yemma.Movement.Data;

namespace Yemma.Movement.Editor
{
    /// <summary>
    /// Custom Inspector para YemmaMovementProfile
    /// Facilita a configuração e validação dos perfis de movimento
    /// </summary>
    [CustomEditor(typeof(YemmaMovementProfile))]
    public class YemmaMovementProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty maxVelocityProp;
        private SerializedProperty accelerationProp;
        private SerializedProperty decelerationProp;
        private SerializedProperty velocityPowerProp;
        private SerializedProperty rotationSpeedProp;
        private SerializedProperty terrainAlignmentSpeedProp;
        private SerializedProperty tiltAmountProp;
        private SerializedProperty tiltSpeedProp;
        private SerializedProperty groundLayersProp;
        private SerializedProperty groundCheckDistanceProp;
        private SerializedProperty groundCheckOffsetProp;
        private SerializedProperty jumpForceProp;
        private SerializedProperty frictionMultiplierProp;
        private SerializedProperty airDragProp;
        private SerializedProperty additionalGravityProp;
        private SerializedProperty movementInputThresholdProp;
        private SerializedProperty minimumWalkSpeedProp;
        private SerializedProperty inputResponseCurveProp;
        private SerializedProperty slopeSpeedMultiplierProp;
        private SerializedProperty maxWalkableAngleProp;
        private SerializedProperty showDebugRaysProp;
        private SerializedProperty debugRayColorProp;

        // Ground Damping System
        private SerializedProperty enableGroundDampingProp;
        private SerializedProperty desiredGroundDistanceProp;
        private SerializedProperty springForceProp;
        private SerializedProperty springDampingProp;
        private SerializedProperty maxDampingForceProp;
        private SerializedProperty dampingToleranceProp;
        private SerializedProperty showDampingDebugProp;
        private SerializedProperty dampingDebugColorProp;

        private bool showBasicSettings = true;
        private bool showRotationSettings = true;
        private bool showGroundSettings = true;
        private bool showPhysicsSettings = true;
        private bool showStateSettings = true;
        private bool showAdvancedSettings = false;
        private bool showDebugSettings = false;
        private bool showDampingSettings = true;

        private void OnEnable()
        {
            // Basic Movement
            maxVelocityProp = serializedObject.FindProperty("maxVelocity");
            accelerationProp = serializedObject.FindProperty("acceleration");
            decelerationProp = serializedObject.FindProperty("deceleration");
            velocityPowerProp = serializedObject.FindProperty("velocityPower");

            // Rotation & Orientation
            rotationSpeedProp = serializedObject.FindProperty("rotationSpeed");
            terrainAlignmentSpeedProp = serializedObject.FindProperty("terrainAlignmentSpeed");
            tiltAmountProp = serializedObject.FindProperty("tiltAmount");
            tiltSpeedProp = serializedObject.FindProperty("tiltSpeed");

            // Ground Detection
            groundLayersProp = serializedObject.FindProperty("groundLayers");
            groundCheckDistanceProp = serializedObject.FindProperty("groundCheckDistance");
            groundCheckOffsetProp = serializedObject.FindProperty("groundCheckOffset");

            // Physics
            jumpForceProp = serializedObject.FindProperty("jumpForce");
            frictionMultiplierProp = serializedObject.FindProperty("frictionMultiplier");
            airDragProp = serializedObject.FindProperty("airDrag");
            additionalGravityProp = serializedObject.FindProperty("additionalGravity");

            // State Transition
            movementInputThresholdProp = serializedObject.FindProperty("movementInputThreshold");
            minimumWalkSpeedProp = serializedObject.FindProperty("minimumWalkSpeed");

            // Advanced Settings
            inputResponseCurveProp = serializedObject.FindProperty("inputResponseCurve");
            slopeSpeedMultiplierProp = serializedObject.FindProperty("slopeSpeedMultiplier");
            maxWalkableAngleProp = serializedObject.FindProperty("maxWalkableAngle");

            // Debug
            showDebugRaysProp = serializedObject.FindProperty("showDebugRays");
            debugRayColorProp = serializedObject.FindProperty("debugRayColor");

            // Ground Damping System
            enableGroundDampingProp = serializedObject.FindProperty("enableGroundDamping");
            desiredGroundDistanceProp = serializedObject.FindProperty("desiredGroundDistance");
            springForceProp = serializedObject.FindProperty("springForce");
            springDampingProp = serializedObject.FindProperty("springDamping");
            maxDampingForceProp = serializedObject.FindProperty("maxDampingForce");
            dampingToleranceProp = serializedObject.FindProperty("dampingTolerance");
            showDampingDebugProp = serializedObject.FindProperty("showDampingDebug");
            dampingDebugColorProp = serializedObject.FindProperty("dampingDebugColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            YemmaMovementProfile profile = (YemmaMovementProfile)target;

            // Header
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Yemma Movement Profile", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Validation
            if (!profile.ValidateProfile())
            {
                EditorGUILayout.HelpBox("Profile has validation errors! Check console for details.", MessageType.Error);
            }

            // Basic Movement Settings
            showBasicSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showBasicSettings, "Basic Movement");
            if (showBasicSettings)
            {
                EditorGUILayout.PropertyField(maxVelocityProp);
                EditorGUILayout.PropertyField(accelerationProp);
                EditorGUILayout.PropertyField(decelerationProp);
                EditorGUILayout.PropertyField(velocityPowerProp);

                // Quick presets
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quick Presets:", EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Slow", GUILayout.Width(60)))
                {
                    maxVelocityProp.floatValue = 3f;
                    accelerationProp.floatValue = 6f;
                    decelerationProp.floatValue = 8f;
                }
                if (GUILayout.Button("Normal", GUILayout.Width(60)))
                {
                    maxVelocityProp.floatValue = 6f;
                    accelerationProp.floatValue = 10f;
                    decelerationProp.floatValue = 15f;
                }
                if (GUILayout.Button("Fast", GUILayout.Width(60)))
                {
                    maxVelocityProp.floatValue = 10f;
                    accelerationProp.floatValue = 15f;
                    decelerationProp.floatValue = 20f;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Rotation & Orientation Settings
            showRotationSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showRotationSettings, "Rotation & Orientation");
            if (showRotationSettings)
            {
                EditorGUILayout.PropertyField(rotationSpeedProp);
                EditorGUILayout.PropertyField(terrainAlignmentSpeedProp);
                EditorGUILayout.PropertyField(tiltAmountProp);
                EditorGUILayout.PropertyField(tiltSpeedProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Ground Detection Settings
            showGroundSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGroundSettings, "Ground Detection");
            if (showGroundSettings)
            {
                EditorGUILayout.PropertyField(groundLayersProp);
                EditorGUILayout.PropertyField(groundCheckDistanceProp);
                EditorGUILayout.PropertyField(groundCheckOffsetProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Physics Settings
            showPhysicsSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showPhysicsSettings, "Physics");
            if (showPhysicsSettings)
            {
                EditorGUILayout.PropertyField(jumpForceProp);
                EditorGUILayout.PropertyField(frictionMultiplierProp);
                EditorGUILayout.PropertyField(airDragProp);
                EditorGUILayout.PropertyField(additionalGravityProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Ground Damping System
            showDampingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showDampingSettings, "Ground Damping System");
            if (showDampingSettings)
            {
                EditorGUILayout.PropertyField(enableGroundDampingProp, new GUIContent("Enable Ground Damping", "Ativar sistema de amortecimento do solo"));
                
                if (enableGroundDampingProp.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(desiredGroundDistanceProp, new GUIContent("Desired Ground Distance", "Distância desejada do player ao chão"));
                    EditorGUILayout.PropertyField(springForceProp, new GUIContent("Spring Force", "Força do spring (rigidez do amortecimento)"));
                    EditorGUILayout.PropertyField(springDampingProp, new GUIContent("Spring Damping", "Amortecimento do spring (reduz oscilações)"));
                    EditorGUILayout.PropertyField(maxDampingForceProp, new GUIContent("Max Damping Force", "Força máxima que pode ser aplicada pelo amortecimento"));
                    EditorGUILayout.PropertyField(dampingToleranceProp, new GUIContent("Damping Tolerance", "Tolerância para considerar na distância correta"));
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Debug Settings:", EditorStyles.miniLabel);
                    EditorGUILayout.PropertyField(showDampingDebugProp, new GUIContent("Show Damping Debug", "Mostrar debug do sistema de amortecimento"));
                    if (showDampingDebugProp.boolValue)
                    {
                        EditorGUILayout.PropertyField(dampingDebugColorProp, new GUIContent("Debug Color", "Cor do debug de amortecimento"));
                    }
                    EditorGUI.indentLevel--;
                    
                    // Visual feedback
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("O sistema de mola mantém o player a uma distância fixa do solo com amortecimento suave.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Sistema de amortecimento desativado. Marque 'Enable Ground Damping' para ativar.", MessageType.Warning);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // State Transition Settings
            showStateSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showStateSettings, "State Transitions");
            if (showStateSettings)
            {
                EditorGUILayout.PropertyField(movementInputThresholdProp);
                EditorGUILayout.PropertyField(minimumWalkSpeedProp);
                
                // Visual feedback
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Transition Zones:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Idle → Walk: Input > {movementInputThresholdProp.floatValue:F3}", EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Walk → Idle: Speed < {minimumWalkSpeedProp.floatValue:F3}", EditorStyles.helpBox);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Advanced Settings
            showAdvancedSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showAdvancedSettings, "Advanced Settings");
            if (showAdvancedSettings)
            {
                EditorGUILayout.PropertyField(inputResponseCurveProp);
                EditorGUILayout.PropertyField(slopeSpeedMultiplierProp);
                EditorGUILayout.PropertyField(maxWalkableAngleProp);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Debug Settings
            showDebugSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showDebugSettings, "Debug");
            if (showDebugSettings)
            {
                EditorGUILayout.PropertyField(showDebugRaysProp);
                if (showDebugRaysProp.boolValue)
                {
                    EditorGUILayout.PropertyField(debugRayColorProp);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Reset to Default"))
            {
                if (EditorUtility.DisplayDialog("Reset Profile", "Reset all values to default?", "Yes", "No"))
                {
                    ResetToDefault();
                }
            }
            
            if (GUILayout.Button("Validate Profile"))
            {
                if (profile.ValidateProfile())
                {
                    EditorUtility.DisplayDialog("Validation", "Profile is valid!", "OK");
                }
            }
            
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void ResetToDefault()
        {
            maxVelocityProp.floatValue = 6f;
            accelerationProp.floatValue = 10f;
            decelerationProp.floatValue = 15f;
            velocityPowerProp.floatValue = 0.96f;
            rotationSpeedProp.floatValue = 12f;
            terrainAlignmentSpeedProp.floatValue = 8f;
            tiltAmountProp.floatValue = 15f;
            tiltSpeedProp.floatValue = 10f;
            groundLayersProp.intValue = 1;
            groundCheckDistanceProp.floatValue = 0.4f;
            groundCheckOffsetProp.vector3Value = Vector3.up * 0.2f;
            jumpForceProp.floatValue = 10f;
            frictionMultiplierProp.floatValue = 1f;
            airDragProp.floatValue = 2f;
            additionalGravityProp.floatValue = 0f;
            movementInputThresholdProp.floatValue = 0.01f;
            minimumWalkSpeedProp.floatValue = 0.1f;
            slopeSpeedMultiplierProp.floatValue = 0.8f;
            maxWalkableAngleProp.floatValue = 45f;
            showDebugRaysProp.boolValue = true;
            debugRayColorProp.colorValue = Color.green;
        }
    }
}
