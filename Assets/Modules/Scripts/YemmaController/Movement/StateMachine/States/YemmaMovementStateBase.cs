using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Classe base para todos os estados de movimentação do Yemma
    /// </summary>
    public abstract class YemmaMovementStateBase : IYemmaMovementState
    {
        protected readonly YemmaMovementController controller;
        protected readonly InputManager inputManager;

        protected YemmaMovementStateBase(YemmaMovementController controller, InputManager inputManager)
        {
            this.controller = controller;
            this.inputManager = inputManager;
        }

        public virtual void Enter()
        {
            if (EnableDebugging())
            {
                Debug.Log($"[{GetType().Name}] Entered state");
            }
        }

        public virtual void Exit()
        {
            if (EnableDebugging())
            {
                Debug.Log($"[{GetType().Name}] Exited state");
            }
        }

        public virtual void HandleInput()
        {
            // Base implementation - pode ser sobrescrita
        }

        public virtual void UpdateLogic()
        {
            // Base implementation - pode ser sobrescrita
        }

        public virtual void UpdatePhysics()
        {
            // Base implementation - pode ser sobrescrita
        }

        /// <summary>
        /// Verifica se debugging está ativado
        /// </summary>
        protected bool EnableDebugging()
        {
            // Implementar lógica de debugging aqui se necessário
            return false;
        }

        /// <summary>
        /// Obtém o input de movimento atual
        /// </summary>
        protected Vector2 GetMovementInput()
        {
            return inputManager.movementVector;
        }

        /// <summary>
        /// Verifica se há input de movimento
        /// </summary>
        protected bool HasMovementInput()
        {
            return GetMovementInput().magnitude > controller.MovementProfile.InputThreshold;
        }

        /// <summary>
        /// Verifica se o player está no chão
        /// </summary>
        protected bool IsGrounded()
        {
            return controller.IsGrounded();
        }

        /// <summary>
        /// Obtém a velocidade horizontal atual
        /// </summary>
        protected float GetHorizontalSpeed()
        {
            return controller.GetHorizontalSpeed();
        }
    }
}
