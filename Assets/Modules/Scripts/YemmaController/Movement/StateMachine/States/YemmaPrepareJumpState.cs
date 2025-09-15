using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado de preparação para pulo - animação curta antes do pulo real
    /// </summary>
    public class YemmaPrepareJumpState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;
        private float prepareTimer = 0f;
        private float prepareDuration = 0.15f; // Tempo curto de preparação

        public YemmaPrepareJumpState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.JumpPrepare);
            prepareTimer = 0f;
            
            // Para o movimento horizontal para garantir pulo consistente
            Vector3 velocity = controller.Velocity;
            velocity.x = 0f;
            velocity.z = 0f;
            // Mantém velocity.y para não interferir na física vertical
            controller.Rigidbody.linearVelocity = velocity;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            prepareTimer += Time.deltaTime;
            
            // Transiciona automaticamente para Jump após o tempo de preparação
            if (prepareTimer >= prepareDuration)
            {
                TransitionToJump();
            }
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            
            // Permite movimento horizontal limitado durante preparação
            Vector2 movementInput = GetMovementInput();
            if (movementInput.magnitude > 0.01f)
            {
                Vector2 reducedInput = movementInput * 0.5f; // 50% do movimento normal
                controller.ApplyMovement(reducedInput);
            }
        }

        private void TransitionToJump()
        {
            var jumpState = new YemmaJumpState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(jumpState);
        }
    }
}