using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de crouch parado do Yemma
    /// </summary>
    public class YemmaCrouchState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;

        public YemmaCrouchState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Crouch);
            // Ativa collider de crouch
            controller.SetCrouchCollider(true);
            // Ativa o toggle de crouch ao entrar no estado
            controller.SetCrouchToggle(true);
        }
        
        public override void Exit()
        {
            // Restaura collider normal ao sair
            controller.SetCrouchCollider(false);
            // Desativa o toggle de crouch ao sair do estado
            controller.SetCrouchToggle(false);
            base.Exit();
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
            
            // Transição para Idle se o toggle foi pressionado E pode levantar
            bool togglePressed = controller.WasInteractButtonPressed();
            
            if (togglePressed && controller.CanStandUp())
            {
                TransitionToIdle();
                return;
            }
            
            // Transição para WalkCrouch se tem movimento
            if (HasMovementInput())
            {
                TransitionToWalkCrouch();
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            
            // No crouch parado, aplicamos apenas um pouco de resistência para parar
            controller.ApplyMovement(Vector2.zero);
            controller.AlignToTerrain();
        }

        private void TransitionToIdle()
        {
            var idleState = new YemmaIdleState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(idleState);
        }

        private void TransitionToWalk()
        {
            var walkState = new YemmaWalkState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(walkState);
        }

        private void TransitionToWalkCrouch()
        {
            var walkCrouchState = new YemmaWalkCrouchState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(walkCrouchState);
        }

        private void TransitionToPrepareJump()
        {
            var prepareJumpState = new YemmaPrepareJumpState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(prepareJumpState);
        }

        private void TransitionToFall()
        {
            var fallState = new YemmaFallState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(fallState);
        }
    }
}