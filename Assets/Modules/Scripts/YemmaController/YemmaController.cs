using UnityEngine;
using Yemma.Movement.Core;
using Yemma.Movement.StateMachine;
using Yemma.Movement.StateMachine.States;

namespace Yemma
{
    /// <summary>
    /// Controlador principal do Yemma - integra todos os sistemas de movimento
    /// </summary>
    public class YemmaController : MonoBehaviour
    {
        [Header("Movement System")]
        [SerializeField] private YemmaMovementController movementController;
        [SerializeField] private InputManager inputManager;
        
        [Header("Debug")]
        [SerializeField] private bool enableStateDebugging = false;

        // State Machine
        private YemmaMovementStateMachine movementStateMachine;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeStateMachine();
        }

        private void Update()
        {
            // Atualiza state machine
            movementStateMachine.HandleInput();
            movementStateMachine.UpdateLogic();
        }

        private void FixedUpdate()
        {
            // Atualiza física da state machine
            movementStateMachine.UpdatePhysics();
            
            // Aplica sistema de amortecimento do solo
            if (movementController != null)
            {
                movementController.ApplyGroundDamping();
            }
        }

        /// <summary>
        /// Inicializa componentes necessários
        /// </summary>
        private void InitializeComponents()
        {
            if (movementController == null)
                movementController = GetComponent<YemmaMovementController>();

            if (inputManager == null)
                inputManager = GetComponent<InputManager>();

            if (movementController == null || inputManager == null)
            {
                Debug.LogError("YemmaController precisa dos componentes YemmaMovementController e InputManager!");
            }
        }

        /// <summary>
        /// Inicializa a state machine de movimento
        /// </summary>
        private void InitializeStateMachine()
        {
            movementStateMachine = new YemmaMovementStateMachine(movementController);
            movementStateMachine.EnableDebugging = enableStateDebugging;

            // Inicia com o estado Idle
            var idleState = new YemmaIdleState(movementController, inputManager, movementStateMachine);
            movementStateMachine.ChangeState(idleState);
        }

        // Propriedades públicas para acesso externo se necessário
        public YemmaMovementController MovementController => movementController;
        public YemmaMovementStateMachine MovementStateMachine => movementStateMachine;
        
        /// <summary>
        /// Verifica se o player está no chão
        /// </summary>
        public bool IsGrounded => movementController.IsGrounded();
        
        /// <summary>
        /// Obtém a velocidade atual do player
        /// </summary>
        public Vector3 CurrentVelocity => movementController.Velocity;
    }
}
