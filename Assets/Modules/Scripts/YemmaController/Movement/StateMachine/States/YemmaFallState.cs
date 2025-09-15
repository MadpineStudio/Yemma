using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de queda do Yemma - quando o player está no ar caindo
    /// </summary>
    public class YemmaFallState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;
        private float fallTimeThreshold = 0.1f; // Tempo mínimo de queda antes de poder aterrissar
        private float fallTimer = 0f;
        private bool canLand = false;

        public YemmaFallState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            // Muda para animação de fall
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Fall);
            
            fallTimer = 0f;
            canLand = false;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            // Atualiza timer de queda
            fallTimer += Time.deltaTime;
            
            // Permite aterrissar após o tempo mínimo
            if (fallTimer >= fallTimeThreshold)
            {
                canLand = true;
            }
            
            // NOVA LÓGICA: Verifica se pode agarrar uma edge durante a queda
            if (controller.IsInEdgeGrabRange() && controller.Velocity.y <= 0)
            {
                TransitionToEdgeHang();
                return;
            }
            
            // Verifica se aterrissou
            if (canLand && IsGrounded() && controller.Velocity.y <= 0.1f)
            {
                // Transiciona para Landing ao invés de ir direto para outros estados
                TransitionToLanding();
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            
            // Permite movimento horizontal limitado no ar
            Vector2 movementInput = GetMovementInput();
            if (movementInput.magnitude > 0.01f)
            {
                // Movimento aéreo reduzido
                Vector2 reducedInput = movementInput * 0.7f; // 70% do movimento normal (um pouco mais que no jump)
                controller.ApplyMovement(reducedInput);
            }
            
            // Aplica gravidade adicional mais forte APENAS quando está no ar
            if (!IsGrounded())
            {
                float additionalGravity = 15f; // Gravidade base adicional
                
                // Se o profile tem gravidade configurada, usa ela, senão usa a padrão
                if (controller.MovementProfile != null && controller.MovementProfile.additionalGravity > 0f)
                {
                    additionalGravity = controller.MovementProfile.additionalGravity;
                }
                
                // Aplica gravidade extra mais forte durante a queda
                Vector3 extraGravity = Vector3.down * additionalGravity * 1.5f; // 50% mais gravidade que o configurado
                controller.Rigidbody.AddForce(extraGravity, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Transição para o estado de EdgeHang
        /// </summary>
        private void TransitionToEdgeHang()
        {
            var edgeHangState = new YemmaEdgeHangState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(edgeHangState);
        }

        /// <summary>
        /// Transição para o estado de Landing
        /// </summary>
        private void TransitionToLanding()
        {
            var landingState = new YemmaLandingState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(landingState);
        }

        /// <summary>
        /// Transição para o estado de Idle (mantido para compatibilidade, mas não usado)
        /// </summary>
        private void TransitionToIdle()
        {
            var idleState = new YemmaIdleState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(idleState);
        }

        /// <summary>
        /// Transição para o estado de Walk (mantido para compatibilidade, mas não usado)
        /// </summary>
        private void TransitionToWalk()
        {
            var walkState = new YemmaWalkState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(walkState);
        }

        /// <summary>
        /// Verifica se a queda é muito rápida (para possível animação de impacto)
        /// </summary>
        public bool IsFastFall()
        {
            return controller.Velocity.y < -15f; // Velocidade de queda considerada "rápida"
        }

        /// <summary> 
        /// Obtém o tempo total de queda
        /// </summary>
        public float GetFallTime()
        {
            return fallTimer;
        }
    }
}
