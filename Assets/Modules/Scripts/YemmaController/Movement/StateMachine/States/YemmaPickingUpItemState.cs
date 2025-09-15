using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de idle do Yemma - quando o player não está se movendo
    /// </summary>
    public class YemmaPickingUpItemState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;

        public YemmaPickingUpItemState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            // Pode adicionar animação de idle aqui
            // controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.Idle_01);
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            // Transição para Walk se há input de movimento
            if (HasMovementInput() && IsGrounded())
            {
                TransitionToIdle();
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            
            // Alinha ao terreno
            controller.AlignToTerrain();
        }

  
        /// <summary>
        /// Transição para o estado de Holding Item Idle
        /// </summary>
        private void TransitionToIdle()
        {
            var idleState = new YemmaHoldingItemIdleState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(idleState);
        }
    }
}
