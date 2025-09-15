using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de caminhada agachada do Yemma
    /// </summary>
    public class YemmaWalkCrouchState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;

        public YemmaWalkCrouchState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.WalkCrouch);
            // Ativa collider de crouch
            controller.SetCrouchCollider(true);
            // Ativa o toggle de crouch ao entrar no estado
            controller.SetCrouchToggle(true);
        }
        
        public override void Exit()
        {
            // Restaura collider normal ao sair
            controller.SetCrouchCollider(false);
            // Restaura velocidade normal da animação ao sair
            controller.SetAnimationSpeed(1f);
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
            
            // Transição para Walk se:
            // 1. Não há mais obstáculos E pode levantar E há movimento
            // 2. OU se toggle foi desativado E pode levantar
            Vector2 movementInput = GetMovementInput();
            bool hasMovement = movementInput.magnitude > 0.1f;
            bool togglePressed = controller.WasInteractButtonPressed();
            
            bool canExitWithMovement = !controller.HasCrouchObstacles() && controller.CanStandUp() && hasMovement;
            bool toggleOut = togglePressed && controller.CanStandUp();
                          
            if (canExitWithMovement || toggleOut)
            {
                TransitionToWalk();
                return;
            }
            
            // Não transita para Crouch - fica em WalkCrouch mesmo parado
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            
            Vector2 movementInput = GetMovementInput();
            controller.ApplyMovement(movementInput);
            controller.AlignToTerrain();
            
            // Controla velocidade da animação baseada no movimento
            UpdateAnimationSpeed();
        }
        
        private void UpdateAnimationSpeed()
        {
            // Verifica se há input de movimento
            bool hasInput = HasMovementInput();
            
            // Define velocidade da animação baseada no input
            if (hasInput)
            {
                // Com input = animação normal
                controller.SetAnimationSpeed(1f);
            }
            else
            {
                // Sem input = animação pausada (congela no frame atual)
                controller.SetAnimationSpeed(0f);
            }
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