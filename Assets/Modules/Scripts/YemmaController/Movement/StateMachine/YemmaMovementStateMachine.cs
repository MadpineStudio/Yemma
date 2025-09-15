using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine
{
    /// <summary>
    /// State machine para controlar os estados de movimentação do Yemma
    /// </summary>
    public class YemmaMovementStateMachine
    {
        private IYemmaMovementState currentState;
        private readonly YemmaMovementController controller;

        public IYemmaMovementState CurrentState => currentState;
        public bool EnableDebugging { get; set; }
        
        // Propriedades para acesso aos estados comuns
        public IYemmaMovementState IdleState => CreateIdleState();
        public IYemmaMovementState FallState => CreateFallState();
        public IYemmaMovementState EdgeHangState => CreateEdgeHangState();
        public IYemmaMovementState EdgeClimbState => CreateEdgeClimbState();

        public YemmaMovementStateMachine(YemmaMovementController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Muda para um novo estado
        /// </summary>
        public void ChangeState(IYemmaMovementState newState)
        {
            if (currentState == newState) return;

            if (EnableDebugging)
            {
                UnityEngine.Debug.Log($"[YemmaMovementStateMachine] Changing from {currentState?.GetType().Name ?? "null"} to {newState?.GetType().Name ?? "null"}");
            }

            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        /// <summary>
        /// Processa input do estado atual
        /// </summary>
        public void HandleInput()
        {
            currentState?.HandleInput();
        }

        /// <summary>
        /// Atualiza lógica do estado atual
        /// </summary>
        public void UpdateLogic()
        {
            currentState?.UpdateLogic();
        }

        /// <summary>
        /// Atualiza física do estado atual
        /// </summary>
        public void UpdatePhysics()
        {
            currentState?.UpdatePhysics();
        }

        // Métodos para criar estados (para acesso via propriedades)
        private IYemmaMovementState CreateIdleState()
        {
            return new States.YemmaIdleState(controller, GetInputManager(), this);
        }

        private IYemmaMovementState CreateFallState()
        {
            return new States.YemmaFallState(controller, GetInputManager(), this);
        }

        private IYemmaMovementState CreateEdgeHangState()
        {
            return new States.YemmaEdgeHangState(controller, GetInputManager(), this);
        }

        private IYemmaMovementState CreateEdgeClimbState()
        {
            return new States.YemmaEdgeClimbState(controller, GetInputManager(), this);
        }

        private InputManager GetInputManager()
        {
            return controller.InputManager;
        }
    }
}
