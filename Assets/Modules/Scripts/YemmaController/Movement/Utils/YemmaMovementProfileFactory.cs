using UnityEngine;
using Yemma.Movement.Data;

namespace Yemma.Movement.Utils
{
    /// <summary>
    /// Factory para criar perfis de movimento pré-configurados
    /// </summary>
    public static class YemmaMovementProfileFactory
    {
        /// <summary>
        /// Cria um perfil padrão equilibrado
        /// </summary>
        public static YemmaMovementProfile CreateDefault()
        {
            var profile = ScriptableObject.CreateInstance<YemmaMovementProfile>();
            profile.name = "Default Movement Profile";
            
            // Basic Movement
            profile.maxVelocity = 6f;
            profile.acceleration = 10f;
            profile.deceleration = 15f;
            profile.velocityPower = 0.96f;

            // Rotation & Orientation
            profile.rotationSpeed = 12f;
            profile.terrainAlignmentSpeed = 8f;
            profile.tiltAmount = 15f;
            profile.tiltSpeed = 10f;

            // Ground Detection
            profile.groundLayers = 1;
            profile.groundCheckDistance = 0.4f;
            profile.groundCheckOffset = Vector3.up * 0.2f;

            // Physics
            profile.frictionMultiplier = 1f;
            profile.airDrag = 2f;
            profile.additionalGravity = 0f;

            // State Transition
            profile.movementInputThreshold = 0.01f;
            profile.minimumWalkSpeed = 0.1f;

            // Advanced Settings
            profile.inputResponseCurve = AnimationCurve.Linear(0, 0, 1, 1);
            profile.slopeSpeedMultiplier = 0.8f;
            profile.maxWalkableAngle = 45f;

            // Debug
            profile.showDebugRays = true;
            profile.debugRayColor = Color.green;

            return profile;
        }

        /// <summary>
        /// Cria um perfil para movimento lento e preciso
        /// </summary>
        public static YemmaMovementProfile CreateSlow()
        {
            var profile = CreateDefault();
            profile.name = "Slow Movement Profile";
            
            profile.maxVelocity = 3f;
            profile.acceleration = 6f;
            profile.deceleration = 8f;
            profile.rotationSpeed = 8f;
            
            return profile;
        }

        /// <summary>
        /// Cria um perfil para movimento rápido e responsivo
        /// </summary>
        public static YemmaMovementProfile CreateFast()
        {
            var profile = CreateDefault();
            profile.name = "Fast Movement Profile";
            
            profile.maxVelocity = 10f;
            profile.acceleration = 15f;
            profile.deceleration = 20f;
            profile.rotationSpeed = 18f;
            profile.velocityPower = 0.90f; // Mais responsivo
            
            return profile;
        }

        /// <summary>
        /// Cria um perfil para movimento fluido e suave
        /// </summary>
        public static YemmaMovementProfile CreateSmooth()
        {
            var profile = CreateDefault();
            profile.name = "Smooth Movement Profile";
            
            profile.acceleration = 8f;
            profile.deceleration = 12f;
            profile.velocityPower = 1.1f; // Mais suave
            profile.rotationSpeed = 10f;
            profile.terrainAlignmentSpeed = 6f;
            
            // Curva suave para input
            profile.inputResponseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            
            return profile;
        }

        /// <summary>
        /// Cria um perfil para movimento em terrenos acidentados
        /// </summary>
        public static YemmaMovementProfile CreateRoughTerrain()
        {
            var profile = CreateDefault();
            profile.name = "Rough Terrain Movement Profile";
            
            profile.maxVelocity = 4f;
            profile.terrainAlignmentSpeed = 12f;
            profile.tiltAmount = 25f;
            profile.maxWalkableAngle = 35f;
            profile.slopeSpeedMultiplier = 0.6f;
            profile.groundCheckDistance = 0.6f;
            
            return profile;
        }

        /// <summary>
        /// Cria um perfil para movimento preciso (plataformas)
        /// </summary>
        public static YemmaMovementProfile CreatePrecise()
        {
            var profile = CreateDefault();
            profile.name = "Precise Movement Profile";
            
            profile.maxVelocity = 4f;
            profile.acceleration = 12f;
            profile.deceleration = 18f;
            profile.velocityPower = 0.85f; // Muito responsivo
            profile.movementInputThreshold = 0.005f; // Threshold menor
            profile.minimumWalkSpeed = 0.05f;
            
            return profile;
        }

        /// <summary>
        /// Cria um perfil customizado baseado em parâmetros
        /// </summary>
        public static YemmaMovementProfile CreateCustom(
            string name,
            float velocity = 6f,
            float accel = 10f,
            float decel = 15f,
            float rotSpeed = 12f)
        {
            var profile = CreateDefault();
            profile.name = name;
            
            profile.maxVelocity = velocity;
            profile.acceleration = accel;
            profile.deceleration = decel;
            profile.rotationSpeed = rotSpeed;
            
            return profile;
        }
    }
}
