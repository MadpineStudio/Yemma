using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de pulo do Yemma - quando o player está no ar subindo
    /// </summary>
    public class YemmaJumpState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;
        private bool hasJumped = false;
        private float coyoteTime = 0.1f; // Tempo extra para pular após sair do chão
        private float coyoteTimeCounter = 0f;

        public YemmaJumpState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine)
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();

            // Reseta o coyote time
            coyoteTimeCounter = coyoteTime;

            // Muda para animação de jump
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Jump);

            // Aplica força de pulo se ainda estiver no chão ou dentro do coyote time
            if (IsGrounded() || coyoteTimeCounter > 0f)
            {
                ApplyJumpForce();
                hasJumped = true;
            }
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            // Removido o cancelamento de pulo para evitar redução de altura em pulos rápidos

            // Atualiza coyote time
            if (!IsGrounded())
            {
                coyoteTimeCounter -= Time.deltaTime;
            }
            else
            {
                coyoteTimeCounter = coyoteTime;
            }

            // NOVA LÓGICA: Verifica se pode agarrar uma edge durante o pulo
            if (controller.IsInEdgeGrabRange() && controller.Velocity.y <= 0.5f)
            {
                TransitionToEdgeHang();
                return;
            }

            // Transição para Fall se a velocidade vertical for negativa ou muito baixa
            if (hasJumped && controller.Velocity.y <= 0.5f) // Mudou de 0f para 0.5f para transição mais suave
            {
                TransitionToFall();
            }

            // Se ainda não pulou mas está no ar (caiu), vai direto para Fall
            if (!hasJumped && !IsGrounded() && coyoteTimeCounter <= 0f)
            {
                TransitionToFall();
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
                Vector2 reducedInput = movementInput * 0.6f; // 60% do movimento normal
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

                // Aplica gravidade extra mais forte durante o jump (mesma intensidade do Fall)
                Vector3 extraGravity = Vector3.down * additionalGravity * 1.5f; // 50% mais gravidade que o configurado
                controller.Rigidbody.AddForce(extraGravity, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Aplica a força de pulo
        /// </summary>
        private void ApplyJumpForce()
        {
            // Força a velocidade vertical para o valor do pulo, garantindo consistência
            controller.Rigidbody.AddForce(new Vector3(0, controller.MovementProfile.jumpForce, 0), ForceMode.Impulse);


            // Vector3 velocity = controller.Rigidbody.linearVelocity;
            // velocity.y = jumpForce; // Força diretamente o valor do pulo
            // controller.Rigidbody.linearVelocity = velocity;
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
        /// Transição para o estado de Fall
        /// </summary>
        private void TransitionToFall()
        {
            var fallState = new YemmaFallState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(fallState);
        }

    }
}
