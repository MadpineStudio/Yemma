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
    }
}
