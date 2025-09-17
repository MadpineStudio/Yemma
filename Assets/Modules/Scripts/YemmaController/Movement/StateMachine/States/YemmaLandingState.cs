using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de aterrissagem simples do Yemma
    /// </summary>
    public class YemmaLandingState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;
        private float landingTimer = 0f;
        private float landingDuration = 0.25f; // Duração fixa simples

        public YemmaLandingState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine)
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Landing);
            landingTimer = 0f;

            // Para a velocidade horizontal para evitar deslizamento
            // Vector3 velocity = controller.Velocity;
            // velocity.x = 0f;
            // velocity.z = 0f;
            // controller.Rigidbody.linearVelocity = velocity;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();

            landingTimer += Time.deltaTime;

            // Permite pular durante o landing para responsividade
            if (GetJumpInput() && IsGrounded())
            {
                TransitionToPrepareJump();
                return;
            }

            // Sai do landing após a duração

            if (HasMovementInput())
            {
                TransitionToWalk();
            }
            else if (landingTimer >= landingDuration)
            {
                TransitionToIdle();
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            // Nenhuma física especial - apenas aplica ground damping
            if (!HasMovementInput())
            {
                ApplyDeceleration();
            }
            controller.ApplyGroundDamping();
        }
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
    }
}