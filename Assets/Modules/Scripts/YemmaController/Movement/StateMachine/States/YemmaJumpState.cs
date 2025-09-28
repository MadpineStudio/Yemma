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
            
            // CORREÇÃO: Aplica a força de pulo imediatamente ao entrar no estado
            ExecuteJump();
        }

        public override void UpdatePhysics()
        {
            // Não aplica sistema de molas durante o pulo para não interferir na física
            // NÃO chama base.UpdatePhysics() para evitar as molas
        }

        private void ExecuteJump()
        {
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

        /// <summary>
        /// Aplica a força de pulo
        /// </summary>
        private void ApplyJumpForce()
        {
            // Reseta a velocidade vertical primeiro para garantir pulo consistente
            Vector3 velocity = controller.Rigidbody.linearVelocity;
            velocity.y = 0f; // Zera a velocidade Y atual
            controller.Rigidbody.linearVelocity = velocity;
            
            // Aplica força de pulo
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
