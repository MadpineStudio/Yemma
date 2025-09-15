using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de caminhada do Yemma - quando o player está se movendo
    /// </summary>
    public class YemmaWalkState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;

        public YemmaWalkState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            // this.controller.Animator.ChangeState(YemmaAnimationController.YemmaAnimations.Run_00, .02f);
            // controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Run);
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Run);

            // Pode adicionar animação de walk aqui
            // controller.PlayAnimation("Walk");
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            // Transição para PrepareJump se há input de jump
            if (GetJumpInput() && IsGrounded())
            {
                TransitionToPrepareJump();
                return;
            }
            
            // Transição para Fall se não está no chão
            if (!IsGrounded())
            {
                TransitionToFall();
                return;
            }
            
            // Transição para WalkCrouch se detecta obstáculo
            if (controller.ShouldCrouch())
            {
                TransitionToWalkCrouch();
                return;
            }
            
            // Transição para Idle se não há input de movimento
            if (!HasMovementInput() && IsGrounded())
            {
                TransitionToIdle();
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
            var idleState = new YemmaIdleState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(idleState);
        }
        
        /// <summary>
        /// Transição para o estado de PrepareJump
        /// </summary>
        private void TransitionToPrepareJump()
        {
            var prepareJumpState = new YemmaPrepareJumpState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(prepareJumpState);
        }
        
        /// <summary>
        /// Transição para o estado de Jump (mantido para compatibilidade)
        /// </summary>
        private void TransitionToJump()
        {
            var jumpState = new YemmaJumpState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(jumpState);
        }
        
        /// <summary>
        /// Transição para o estado de WalkCrouch
        /// </summary>
        private void TransitionToWalkCrouch()
        {
            var walkCrouchState = new YemmaWalkCrouchState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(walkCrouchState);
        }
        
        /// <summary>
        /// Transição para o estado de Crouch
        /// </summary>
        private void TransitionToCrouch()
        {
            var crouchState = new YemmaCrouchState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(crouchState);
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
