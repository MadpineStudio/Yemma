using UnityEngine;
using Yemma.Movement.Core;
using System.Collections.Generic;

namespace Yemma.Movement.Data
{
    /// <summary>
    /// Configuração completa de todos os estados de animação com seus respectivos movement profiles
    /// </summary>
    [CreateAssetMenu(fileName = "YemmaAnimationProfileSet", menuName = "Yemma/Animation Profile Set", order = 1)]
    public class YemmaAnimationProfileSet : ScriptableObject
    {
        [System.Serializable]
        public class AnimationStateConfig
        {
            [Header("Animation Settings")]
            [Space(5)]
            public float blendTime = 0.2f;
            
            [Header("Movement Profile")]
            [Space(5)]
            public YemmaMovementProfile movementProfile;
            
            [Header("Behavior")]
            [Space(5)]
            public bool allowInput = true;
            public bool canBeInterrupted = true;
        }
        
        [Header("Animation States Configuration")]
        [Space(10)]
        
        [SerializeField, Space(5)] 
        private AnimationStateConfig idleConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig collectWalkConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig runConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig jumpConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig jumpPrepareConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig fallConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig walkSimpleConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig walkCrouchConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig crouchConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig landingConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig edgeHangConfig = new AnimationStateConfig();
        [SerializeField, Space(5)] 
        private AnimationStateConfig edgeClimbConfig = new AnimationStateConfig();
        
        /// <summary>
        /// Obtém a configuração para um tipo de animação específico
        /// </summary>
        public AnimationStateConfig GetStateConfig(YemmaAnimationController.YemmaAnimations animationType)
        {
            switch (animationType)
            {
                case YemmaAnimationController.YemmaAnimations.Idle:
                    return idleConfig;
                case YemmaAnimationController.YemmaAnimations.CollectWalk:
                    return collectWalkConfig;
                case YemmaAnimationController.YemmaAnimations.Run:
                    return runConfig;
                case YemmaAnimationController.YemmaAnimations.Jump:
                    return jumpConfig;
                case YemmaAnimationController.YemmaAnimations.JumpPrepare:
                    return jumpPrepareConfig;
                case YemmaAnimationController.YemmaAnimations.Fall:
                    return fallConfig;
                case YemmaAnimationController.YemmaAnimations.WalkSimple:
                    return walkSimpleConfig;
                case YemmaAnimationController.YemmaAnimations.WalkCrouch:
                    return walkCrouchConfig;
                case YemmaAnimationController.YemmaAnimations.Crouch:
                    return crouchConfig;
                case YemmaAnimationController.YemmaAnimations.Landing:
                    return landingConfig;
                case YemmaAnimationController.YemmaAnimations.EdgeHang:
                    return edgeHangConfig;
                case YemmaAnimationController.YemmaAnimations.EdgeClimb:
                    return edgeClimbConfig;
                default:
                    Debug.LogWarning($"Configuração não encontrada para: {animationType}");
                    return null;
            }
        }
        
        /// <summary>
        /// Verifica se tem configuração para todos os estados de animação
        /// </summary>
        public bool ValidateConfiguration()
        {
            var allAnimationTypes = System.Enum.GetValues(typeof(YemmaAnimationController.YemmaAnimations));
            bool isValid = true;
            
            foreach (YemmaAnimationController.YemmaAnimations animType in allAnimationTypes)
            {
                var config = GetStateConfig(animType);
                if (config == null)
                {
                    Debug.LogWarning($"Configuração faltando para: {animType}");
                    isValid = false;
                }
                else if (config.movementProfile == null)
                {
                    Debug.LogWarning($"Movement Profile faltando para: {animType}");
                    isValid = false;
                }
            }
            
            return isValid;
        }
        
        [ContextMenu("Validate All Configurations")]
        private void ValidateInEditor()
        {
            if (ValidateConfiguration())
            {
                Debug.Log("✅ Todas as configurações estão válidas!");
            }
            else
            {
                Debug.LogError("❌ Algumas configurações estão faltando!");
            }
        }
        
        [ContextMenu("Set Default Values")]
        private void SetDefaultValues()
        {
            // Configurações padrão para cada estado
            idleConfig = new AnimationStateConfig { blendTime = 0.3f, allowInput = true, canBeInterrupted = true };
            collectWalkConfig = new AnimationStateConfig { blendTime = 0.2f, allowInput = true, canBeInterrupted = true };
            runConfig = new AnimationStateConfig { blendTime = 0.15f, allowInput = true, canBeInterrupted = true };
            jumpConfig = new AnimationStateConfig { blendTime = 0.1f, allowInput = false, canBeInterrupted = false };
            jumpPrepareConfig = new AnimationStateConfig { blendTime = 0.05f, allowInput = true, canBeInterrupted = false };
            fallConfig = new AnimationStateConfig { blendTime = 0.3f, allowInput = false, canBeInterrupted = false }; // Aumentado de 0.1f para 0.3f
            walkSimpleConfig = new AnimationStateConfig { blendTime = 0.2f, allowInput = true, canBeInterrupted = true };
            walkCrouchConfig = new AnimationStateConfig { blendTime = 0.25f, allowInput = true, canBeInterrupted = true };
            landingConfig = new AnimationStateConfig { blendTime = 0.15f, allowInput = false, canBeInterrupted = true }; // Mudou para interruptível
            
            Debug.Log("✅ Valores padrão definidos!");
        }
    }
}
