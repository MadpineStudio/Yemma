using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de caminhada do Yemma - quando o player está se movendo
    /// </summary>
    public class YemmaHoldingIntemWalkState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;

        public YemmaHoldingIntemWalkState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine)
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            // this.controller.Animator.ChangeState(YemmaAnimationController.YemmaAnimations.Run_00, .02f);
            // controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Run_00);

            // Pode adicionar animação de walk aqui
            // controller.PlayAnimation("Walk");
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            // Transição para Idle se não há input de movimento
            if (!HasMovementInput() && IsGrounded())
            {
                TransitionToIdle();
            }
            if (GetInteractInput() && IsGrounded() && controller.CanDropItem())
            {
                DropItem();
                return;
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            // Aplica movimento baseado no input
            Vector2 movementInput = GetMovementInput();
            controller.ApplyMovement(movementInput);

            // Alinha ao terreno
            controller.AlignToTerrain();
        }

        /// <summary>
        /// Transição para o estado de Idle
        /// </summary>
        private void TransitionToIdle()
        {
            var idleState = new YemmaHoldingItemIdleState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(idleState);
        }
        private void DropItem()
        {
            controller.Interact();
            var walkState = new YemmaWalkState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(walkState);
        }
    }
}
