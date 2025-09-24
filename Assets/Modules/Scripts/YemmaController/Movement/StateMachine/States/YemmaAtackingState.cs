using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de idle do Yemma - quando o player não está se movendo
    /// </summary>
    public class YemmaAtackingState : YemmaMovementStateBase
    {
        private float atackTimer = 0;
        private readonly YemmaMovementStateMachine stateMachine;
        public YemmaAtackingState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine)
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            StopMovement();
            AttackItemBehaviour.OnAtackInitiated?.Invoke(true);
            // Pode adicionar animação de idle aqui
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Atack);
        }
        public override void Exit()
        {
            base.Exit();
            AttackItemBehaviour.OnAtackInitiated?.Invoke(false);
        }
        public override void UpdateLogic()
        {
            base.UpdateLogic();

            if (IsGrounded())
            {
                if (HasMovementInput() && atackTimer >= 0.46f)
                {
                    TransitionToWalk();
                    return;
                }
                else if(atackTimer >= 0.66f)
                {
                    TransitionToIdle();
                }
            }

        }

        private void StopMovement()
        {
            controller.Rigidbody.linearVelocity = Vector3.zero;
        }
        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            atackTimer += Time.fixedDeltaTime;

            // Alinha ao terreno
            controller.AlignToTerrain();
        }

        private void TransitionToWalk()
        {
            var walkState = new YemmaWalkState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(walkState);
        }

        private void TransitionToIdle()
        {
            var idleState = new YemmaIdleState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(idleState);
        }





    }
}
