using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField] private YemmaInteractorController interactorController;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private YemmaInteractionSystem interactionSystem;
        
        [Header("Interaction System")]
        [SerializeField] private bool isInInteractionMode = false;
        [SerializeField] private bool blockInputsInInteractionMode = true;
        [SerializeField] private bool blockMovementInInteractionMode = true;
        
        [Header("Interaction Events")]
        public UnityEvent OnEnterInteractionMode;
        public UnityEvent OnExitInteractionMode;
        public UnityEvent<IInteractable> OnInteractWithObject;
        
        [Header("Light Dash Events")]
        public UnityEvent<Transform, float> OnLightDashActivated;
        
        [Header("Dash Configuration")]
        [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Debug")]
        [SerializeField] private bool enableStateDebugging = false;
        [SerializeField] private bool debugInteractionMode = true;

        // State Machine
        private YemmaMovementStateMachine movementStateMachine;
        
        // Interaction Mode Properties
        public bool IsInInteractionMode => isInInteractionMode;
        public bool ShouldBlockInputs => isInInteractionMode && blockInputsInInteractionMode;
        public bool ShouldBlockMovement => isInInteractionMode && blockMovementInInteractionMode;

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
            // Só atualiza state machine se não estiver no modo de interação ou se permitir inputs
            if (!ShouldBlockInputs)
            {
                movementStateMachine.HandleInput();
            }
            
            if (!ShouldBlockMovement)
            {
                movementStateMachine.UpdateLogic();
            }
        }

        private void FixedUpdate()
        {
            // Só atualiza física se não estiver bloqueando movimento
            if (!ShouldBlockMovement)
            {
                movementStateMachine.UpdatePhysics();
                
                // Aplica sistema de amortecimento do solo
                if (movementController != null)
                {
                    movementController.ApplyGroundDamping();
                }
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
                
            if (interactionSystem == null)
                interactionSystem = GetComponent<YemmaInteractionSystem>();
                
            if (interactorController == null)
            {
                interactorController = GetComponent<YemmaInteractorController>();
            }
                interactorController.AssingController(movementController ,inputManager); 
            if (movementController == null || inputManager == null)
            {
                Debug.LogError("YemmaController precisa dos componentes YemmaMovementController e InputManager!");
            }
            
            // Configura o InputManager no MovementController para o sistema de crouch
            movementController.SetInputManager(inputManager);
            
            // Configura eventos do sistema de interação
            SetupInteractionEvents();
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
        public InputManager InputManager => inputManager;
        public YemmaMovementStateMachine MovementStateMachine => movementStateMachine;
        
        /// <summary>
        /// Verifica se o player está no chão
        /// </summary>
        public bool IsGrounded => movementController.IsGrounded();
        
        /// <summary>
        /// Obtém a velocidade atual do player
        /// </summary>
        public Vector3 CurrentVelocity => movementController.Velocity;
        
        /// <summary>
        /// Verifica se deve agachar (obstáculo detectado à frente)
        /// </summary>
        public bool ShouldCrouch => movementController.ShouldCrouch();
        
        /// <summary>
        /// Verifica se pode levantar (espaço livre acima)
        /// </summary>
        public bool CanStandUp => movementController.CanStandUp();
        
        // === INTERACTION MODE SYSTEM ===
        
        /// <summary>
        /// Ativa o modo de interação, bloqueando inputs e movimento
        /// </summary>
        public void EnterInteractionMode()
        {
            if (!isInInteractionMode)
            {
                isInInteractionMode = true;
                
                // Dispara evento
                OnEnterInteractionMode?.Invoke();
                
                if (debugInteractionMode)
                {
                    Debug.Log("YemmaController: Entrando no modo de interação");
                }
            }
        }
        
        /// <summary>
        /// Desativa o modo de interação, restaurando inputs e movimento
        /// </summary>
        public void ExitInteractionMode()
        {
            if (isInInteractionMode)
            {
                isInInteractionMode = false;
                
                // Dispara evento
                OnExitInteractionMode?.Invoke();
                
                if (debugInteractionMode)
                {
                    Debug.Log("YemmaController: Saindo do modo de interação");
                }
            }
        }
        
        /// <summary>
        /// Alterna entre modo de interação ativo/inativo
        /// </summary>
        public void ToggleInteractionMode()
        {
            if (isInInteractionMode)
            {
                ExitInteractionMode();
            }
            else
            {
                EnterInteractionMode();
            }
        }
        
        /// <summary>
        /// Define o modo de interação diretamente
        /// </summary>
        /// <param name="active">Se true, ativa o modo; se false, desativa</param>
        public void SetInteractionMode(bool active)
        {
            if (active)
            {
                EnterInteractionMode();
            }
            else
            {
                ExitInteractionMode();
            }
        }
        
        /// <summary>
        /// Configura as opções de bloqueio do modo de interação
        /// </summary>
        /// <param name="blockInputs">Se deve bloquear inputs</param>
        /// <param name="blockMovement">Se deve bloquear movimento</param>
        public void ConfigureInteractionMode(bool blockInputs, bool blockMovement)
        {
            blockInputsInInteractionMode = blockInputs;
            blockMovementInInteractionMode = blockMovement;
            
            if (debugInteractionMode)
            {
                Debug.Log($"YemmaController: Configuração do modo de interação - Inputs: {blockInputs}, Movimento: {blockMovement}");
            }
        }
        
        /// <summary>
        /// Configura os eventos do sistema de interação
        /// </summary>
        private void SetupInteractionEvents()
        {
            if (interactionSystem != null)
            {
                // Se inscreve nos eventos do sistema de interação
                interactionSystem.OnInteraction.AddListener(HandleObjectInteraction);
            }
            
            // Configura o evento de light dash para chamar o método de dash
            OnLightDashActivated.AddListener(StartLightDash);
        }
        
        /// <summary>
        /// Manipula a interação com objetos
        /// </summary>
        /// <param name="interactable">Objeto com o qual foi interagido</param>
        private void HandleObjectInteraction(IInteractable interactable)
        {
            // Dispara evento personalizado
            OnInteractWithObject?.Invoke(interactable);
            
            if (debugInteractionMode)
            {
                Debug.Log($"YemmaController: Interação com objeto: {interactable.InteractionPrompt}");
            }
        }
        
        /// <summary>
        /// Força uma interação com um objeto específico (para uso externo)
        /// </summary>
        /// <param name="interactable">Objeto para interagir</param>
        public void ForceInteraction(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract)
            {
                HandleObjectInteraction(interactable);
                interactable.OnInteract(this);
            }
        }
        
        // Propriedades públicas para o sistema de interação
        public YemmaInteractionSystem InteractionSystem => interactionSystem;
        public bool HasCurrentInteractable => interactionSystem != null && interactionSystem.HasInteractable;
        public IInteractable CurrentInteractable => interactionSystem?.CurrentInteractable;
        
        // === LIGHT DASH SYSTEM ===
        
        public void StartLightDash(Transform dashPoint, float dashSpeed)
        {
            if (ShouldBlockMovement) return;
            
            // Get the specific LightDashManager that triggered this
            LightDashManager dashManager = null;
            LightDashManager[] managers = FindObjectsOfType<LightDashManager>();
            foreach (var manager in managers)
            {
                if (manager.DashPoint == dashPoint)
                {
                    dashManager = manager;
                    break;
                }
            }
            
            Vector3 controlPoint = dashManager != null ? 
                dashManager.GetControlPoint(transform.position, dashPoint.position) :
                (transform.position + dashPoint.position) / 2f + Vector3.up * 2f;
            
            var dashState = new YemmaDashState(movementController, inputManager, movementStateMachine, dashPoint, dashSpeed, controlPoint, dashCurve);
            movementStateMachine.ChangeState(dashState);
        }
    }
}
