using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de escalada com root motion animation
    /// </summary>
    public class YemmaEdgeClimbState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;
        private bool climbComplete = false;
        private float climbStartTime;
        private float climbDuration = .75f; // Duração estimada da animação (aumentada)
        
        // Target Matching
        private Vector3 targetPosition;
        private bool hasTargetPosition = false;
        private bool targetMatchingApplied = false; // Flag para aplicar target matching apenas uma vez

        public YemmaEdgeClimbState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            // Verificações de segurança
            if (controller == null)
            {
                Debug.LogError("[EdgeClimb] Controller is null in Enter()");
                return;
            }

            // RESET COMPLETO do target matching antes de iniciar
            ResetTargetMatching();

            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.EdgeClimb);
            
            // Para o movimento manual - root motion vai controlar
            controller.StopMovement();
            
            // DESABILITA FÍSICA E COLISORES durante o climb
            DisablePhysicsAndColliders();
            
            // Calcula posição alvo para target matching
            CalculateTargetPosition();
            
            // Marca o início da escalada
            climbComplete = false;
            climbStartTime = Time.time;
            
            // Habilita root motion
            EnableRootMotion(true);
        }

        public override void Exit()
        {
            // RESET COMPLETO do target matching ao sair
            ResetTargetMatching();
            
            // Reabilita física e colisores
            EnablePhysicsAndColliders();
            
            // Desabilita root motion
            EnableRootMotion(false);
            base.Exit();
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            // Verificações de segurança
            if (controller == null)
            {
                Debug.LogError("[EdgeClimb] Controller is null in UpdateLogic()");
                return;
            }

            if (controller.Animator == null)
            {
                Debug.LogError("[EdgeClimb] Animator is null in UpdateLogic()");
                return;
            }
            
            // Aplica target matching se temos uma posição alvo
            if (hasTargetPosition && !climbComplete)
            {
                ApplyTargetMatching();
            }
            
            // Verifica se a animação de climb terminou
            if (!climbComplete && HasClimbFinished())
            {
                climbComplete = true;
                // Reset target matching antes de finalizar
                ResetTargetMatching();
                // Reabilita física quando a animação terminar
                EnablePhysicsAndColliders();
                TransitionToIdle();
                return;
            }
            
            // Failsafe - se passou muito tempo, força transição
            if (Time.time - climbStartTime > climbDuration + 1.0f) // Mais tempo de tolerância
            {
                climbComplete = true;
                // Reset target matching em caso de failsafe
                ResetTargetMatching();
                // Reabilita física em caso de failsafe
                EnablePhysicsAndColliders();
                TransitionToIdle();
                return;
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            
            // Root motion controla o movimento, apenas mantém estabilidade
            if (!climbComplete)
            {
                // Pode adicionar pequenos ajustes se necessário
            }
        }

        /// <summary>
        /// Verifica se a animação de climb terminou
        /// </summary>
        private bool HasClimbFinished()
        {
            // Primeiro verifica se o tempo máximo foi atingido (failsafe)
            if (Time.time - climbStartTime >= climbDuration)
                return true;
            
            // Verifica pelo normalized time da animação se ela chegou perto do fim
            if (controller != null && controller.Animator != null)
            {
                AnimatorStateInfo stateInfo = controller.Animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("EdgeClimb"))
                {
                    // Só termina quando a animação realmente chegar ao final (0.98)
                    return stateInfo.normalizedTime >= 0.98f;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Controla root motion do animator
        /// </summary>
        private void EnableRootMotion(bool enable)
        {
            if (controller != null && controller.Animator != null)
            {
                controller.Animator.applyRootMotion = enable;
            }
            else
            {
                Debug.LogError("[EdgeClimb] Cannot set root motion - Controller or Animator is null");
            }
        }

        /// <summary>
        /// Reset completo do target matching para evitar conflitos entre climbs
        /// </summary>
        private void ResetTargetMatching()
        {
            if (controller != null && controller.Animator != null)
            {
                // Interrompe qualquer target matching em andamento
                controller.Animator.InterruptMatchTarget(false);
                
                // Força o animator a parar qualquer operação de matching
                if (controller.Animator.isMatchingTarget)
                {
                    controller.Animator.InterruptMatchTarget(true);
                }
            }
            
            // Reset das variáveis de target matching
            hasTargetPosition = false;
            targetPosition = Vector3.zero;
            climbComplete = false;
            targetMatchingApplied = false; // Reset da flag de target matching
        }

        /// <summary>
        /// Desabilita física e colisores durante o climb
        /// </summary>
        private void DisablePhysicsAndColliders()
        {
            if (controller != null && controller.Rigidbody != null)
            {
                // Torna kinematic para desabilitar física
                controller.Rigidbody.isKinematic = true;
            }

            // Desabilita colisores
            if (controller != null && controller.Transform != null)
            {
                Collider[] colliders = controller.Transform.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = false;
                }
            }
        }

        /// <summary>
        /// Reabilita física e colisores após o climb
        /// </summary>
        private void EnablePhysicsAndColliders()
        {
            if (controller != null && controller.Rigidbody != null)
            {
                // Reabilita física
                controller.Rigidbody.isKinematic = false;
            }

            // Reabilita colisores
            if (controller != null && controller.Transform != null)
            {
                Collider[] colliders = controller.Transform.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    col.enabled = true;
                }
            }
        }

        /// <summary>
        /// Calcula a posição alvo onde o player deve ficar após a escalada
        /// </summary>
        private void CalculateTargetPosition()
        {
            // Verificações de segurança
            if (controller == null || controller.Transform == null)
            {
                Debug.LogError("[EdgeClimb] Controller or Transform is null");
                hasTargetPosition = false;
                return;
            }

            // Tenta detectar a edge novamente para calcular posição final
            if (controller.CanGrabEdge(out Vector3 edgePosition, out Vector3 edgeNormal))
            {
                // Posição final: em cima da edge, um pouco para trás da borda
                targetPosition = edgePosition + Vector3.up * 0.1f - edgeNormal * 0.5f;
                hasTargetPosition = true;
            }
            else
            {
                // Fallback: posição um pouco acima e à frente da posição atual
                targetPosition = controller.Transform.position + Vector3.up * 2f + controller.Transform.forward * 1f;
                hasTargetPosition = true;
            }
        }

        /// <summary>
        /// Aplica target matching simples usando MatchTarget
        /// </summary>
        private void ApplyTargetMatching()
        {
            // Só aplica uma vez por climb
            if (targetMatchingApplied) return;
            
            // Verificações de segurança
            if (controller == null || controller.Animator == null)
            {
                Debug.LogError("[EdgeClimb] Controller or Animator is null");
                return;
            }

            if (controller.Transform == null)
            {
                Debug.LogError("[EdgeClimb] Transform is null");
                return;
            }

            // Verifica se não há target matching ativo E se a animação está no estado correto
            if (!controller.Animator.isMatchingTarget)
            {
                // Verifica se estamos na animação de EdgeClimb
                AnimatorStateInfo stateInfo = controller.Animator.GetCurrentAnimatorStateInfo(0);
                
                // Aplica quando a animação começar (após 0.1s) até quase o final (0.95)
                if (stateInfo.IsName("EdgeClimb") && stateInfo.normalizedTime >= 0.1f && stateInfo.normalizedTime < 0.95f)
                {
                    // Target matching mais tardio para dar tempo da animação completar
                    float startTime = 0.2f;  // Começa mais tarde
                    float endTime = 0.9f;    // Termina mais tarde
                    
                    // Usa apenas posição (sem rotação) para simplicidade
                    MatchTargetWeightMask weightMask = new MatchTargetWeightMask(Vector3.one, 0f);
                    
                    try
                    {
                        controller.Animator.MatchTarget(
                            targetPosition,
                            controller.Transform.rotation,
                            AvatarTarget.Root,
                            weightMask,
                            startTime,
                            endTime
                        );
                        
                        // Marca como aplicado para não repetir
                        targetMatchingApplied = true;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[EdgeClimb] Target matching failed: {e.Message}");
                        // Em caso de erro, marca como sem target para evitar loops
                        hasTargetPosition = false;
                    }
                }
            }
        }

        private void TransitionToIdle()
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
    }
}