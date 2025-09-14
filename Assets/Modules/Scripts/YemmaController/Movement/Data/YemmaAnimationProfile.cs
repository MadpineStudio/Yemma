using UnityEngine;
using Yemma.Movement.Core;

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
            public YemmaAnimationController.YemmaAnimations animationType;
            public float blendTime = 0.2f;
            
            [Header("Movement Profile")]
            public YemmaMovementProfile movementProfile;
            
            [Header("Behavior")]
            public bool allowInput = true;
            public bool canBeInterrupted = true;
        }
        
        [Header("Animation States Configuration")]
        public AnimationStateConfig[] animationStates;
        
        /// <summary>
        /// Obtém a configuração para um tipo de animação específico
        /// </summary>
        public AnimationStateConfig GetStateConfig(YemmaAnimationController.YemmaAnimations animationType)
        {
            foreach (var config in animationStates)
            {
                if (config.animationType == animationType)
                    return config;
            }
            return null;
        }
        
        /// <summary>
        /// Verifica se tem configuração para todos os estados de animação
        /// </summary>
        public bool ValidateConfiguration()
        {
            var allAnimationTypes = System.Enum.GetValues(typeof(YemmaAnimationController.YemmaAnimations));
            
            foreach (YemmaAnimationController.YemmaAnimations animType in allAnimationTypes)
            {
                if (GetStateConfig(animType) == null)
                {
                    Debug.LogWarning($"Configuração faltando para: {animType}");
                    return false;
                }
            }
            
            return true;
        }
    }
}
