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
            
            // Transição para Walk se há input de movimento
            if (HasMovementInput() && IsGrounded())
            {
                TransitionToWalk();
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
    }
}
