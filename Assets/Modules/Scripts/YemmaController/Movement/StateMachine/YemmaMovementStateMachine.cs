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
    }
}
