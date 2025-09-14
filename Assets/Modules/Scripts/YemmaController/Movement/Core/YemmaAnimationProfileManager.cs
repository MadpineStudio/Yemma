using UnityEngine;
using System.Collections.Generic;
using Yemma.Movement.Data;

namespace Yemma.Movement.Core
{
    /// <summary>
    /// Manager que usa o Animation Profile Set para gerenciar estados
    /// </summary>
    public class YemmaAnimationProfileManager : MonoBehaviour
    {
        [Header("Animation Profile Set")]
        [SerializeField] private YemmaAnimationProfileSet profileSet;
        
        private Dictionary<YemmaAnimationController.YemmaAnimations, YemmaAnimationProfileSet.AnimationStateConfig> configMap;
        private YemmaMovementController controller;
        private YemmaAnimationController.YemmaAnimations currentState;
        private YemmaMovementProfile originalProfile; // Backup do profile original
        
        public void Initialize(YemmaMovementController movementController)
        {
            controller = movementController;
            originalProfile = controller.MovementProfile; // Salva o profile original
            BuildConfigMap();
            
            if (profileSet != null)
            {
                profileSet.ValidateConfiguration();
            }
        }
        
        private void BuildConfigMap()
        {
            configMap = new Dictionary<YemmaAnimationController.YemmaAnimations, YemmaAnimationProfileSet.AnimationStateConfig>();
            
            if (profileSet?.animationStates != null)
            {
                foreach (var config in profileSet.animationStates)
                {
                    if (config != null)
                    {
                        configMap[config.animationType] = config;
                    }
                }
            }
        }
        
        public bool ChangeToState(YemmaAnimationController.YemmaAnimations newState)
        {
            if (!configMap.TryGetValue(newState, out var config))
            {
                Debug.LogWarning($"Configuração não encontrada para: {newState}");
                return false;
            }
            
            var currentConfig = GetCurrentConfig();
            if (currentConfig != null && !currentConfig.canBeInterrupted)
            {
                return false;
            }
            
            ApplyStateConfig(config);
            currentState = newState;
            return true;
        }
        
        private void ApplyStateConfig(YemmaAnimationProfileSet.AnimationStateConfig config)
        {
            // Troca o movement profile se especificado
            if (config.movementProfile != null)
            {
                controller.SetMovementProfile(config.movementProfile);
            }
            else
            {
                // Usa o profile original se não houver um específico
                controller.SetMovementProfile(originalProfile);
            }
            
            // Executa animação
            if (controller.Animator != null)
            {
                controller.Animator.CrossFade("Bake-" + config.animationType.ToString(), config.blendTime);
            }
        }
        
        public YemmaAnimationProfileSet.AnimationStateConfig GetCurrentConfig()
        {
            return configMap.TryGetValue(currentState, out var config) ? config : null;
        }
        
        public bool CanReceiveInput()
        {
            var config = GetCurrentConfig();
            return config == null || config.allowInput;
        }
        
        public YemmaAnimationController.YemmaAnimations GetCurrentState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Define o profile set em runtime
        /// </summary>
        public void SetProfileSet(YemmaAnimationProfileSet newProfileSet)
        {
            profileSet = newProfileSet;
            BuildConfigMap();
        }
    }
}
