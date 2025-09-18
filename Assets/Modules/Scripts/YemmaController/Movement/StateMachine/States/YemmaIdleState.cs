using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de idle do Yemma - quando o player não está se movendo
    /// </summary>
    public class YemmaIdleState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;

        public YemmaIdleState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            // Pode adicionar animação de idle aqui
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Idle);
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
            if (GetInteractInput() && IsGrounded() && controller.HasClosestPickable())
            {
                TransitionToPickUpItem();
                return;
            }
            // Transição para Crouch se detecta obstáculo
            if (controller.ShouldCrouch() && IsGrounded())
            {
                TransitionToCrouch();
                return;
            }
            
            // Transição para Walk se há input de movimento
            if (HasMovementInput() && IsGrounded())
            {
                TransitionToWalk();
                return;
            }
            
            // Transição para Fall se não está no chão
            if (!IsGrounded())
            {
                TransitionToFall();
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            
            // Aplica desaceleração gradual
            ApplyDeceleration();
            
            // Alinha ao terreno
            controller.AlignToTerrain();
        }

        /// <summary>
        /// Aplica desaceleração gradual para parar o movimento
        /// </summary>
        private void ApplyDeceleration()
        {
            Vector3 velocity = controller.Velocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);

            if (horizontalVelocity.magnitude > controller.MovementProfile.WalkSpeedThreshold)
            {
                // Aplica força de desaceleração
                Vector3 decelerationForce = -horizontalVelocity * controller.MovementProfile.deceleration;
                controller.Rigidbody.AddForce(decelerationForce, ForceMode.Acceleration);

            }
            else
            {
                // Para completamente se a velocidade for muito baixa
                controller.StopMovement();
            }
        }

        /// <summary>
        /// Transição para o estado de Walk
        /// </summary>
        private void TransitionToWalk()
        {
            var walkState = new YemmaWalkState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(walkState);
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
        private void TransitionToPickUpItem()
        {
            var pickupItemState = new YemmaPickingUpItemState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(pickupItemState);
        }
        /// <summary>
        /// Transição para o estado de Fall
        /// </summary>
        private void TransitionToFall()
        {
            var fallState = new YemmaFallState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(fallState);
        }
        
        /// <summary>
        /// Transição para o estado de Crouch
        /// </summary>
        private void TransitionToCrouch()
        {
            var crouchState = new YemmaCrouchState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(crouchState);
        }
    }
}
