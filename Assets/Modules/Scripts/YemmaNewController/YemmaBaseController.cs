using UnityEngine;
using TMPro;

public class YemmaBaseController : MonoBehaviour
{
    private StateMachine stateMachine;
    private YemmaInputManager inputManager;
    private YemmaPhysics yemmaPhysics;
    private YemmaAnim yemmaAnim;
    private ClimbPhysics climbPhysics;
    [SerializeField] private TextMeshProUGUI debugStateText;
    [SerializeField] private YemmaSettings settings;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Animator playerAnimator;
    
    [Header("Debug Info")]
    [SerializeField] private bool isGrounded;
    
    // Coyote jump variables
    private float lastGroundedTime;
    private bool wasGroundedLastFrame;

    void Start()
    {
        // Inicializa o input manager
        inputManager = GetComponent<YemmaInputManager>();
        if (inputManager == null)
            inputManager = gameObject.AddComponent<YemmaInputManager>();

        // Inicializa o physics
        yemmaPhysics = new YemmaPhysics(transform, GetComponent<Rigidbody>());
        if (settings != null)
        {
            yemmaPhysics.SetGroundCheckSettings(settings.springRaycastDistance, settings.groundLayer);
            yemmaPhysics.SetSpringForceSettings(
                settings.enableSpringForce,
                settings.springConstant,
                settings.dampingConstant,
                settings.restLength,
                settings.springRaycastDistance,
                settings.springRaycastOffset
            );
        }

        // Inicializa o sistema de animação
        yemmaAnim = new YemmaAnim(playerAnimator != null ? playerAnimator : 
                                 (playerModel != null ? playerModel.GetComponent<Animator>() : 
                                  GetComponent<Animator>()));

        // Inicializa o sistema de climb
        climbPhysics = new ClimbPhysics(transform);
        if (settings != null)
        {
            climbPhysics.SetClimbDetectionSettings(
                settings.climbDetectionDistance,
                settings.climbableLayer,
                settings.climbRaycastOffset,
                settings.climbableTag
            );
        }

        // Adiciona debug se não existir
        if (GetComponent<YemmaDebug>() == null)
            gameObject.AddComponent<YemmaDebug>();

        // Inicializa a máquina de estados com o estado Idle
        stateMachine = new StateMachine();
        stateMachine.ChangeState(new IdleState(this));

        // Inscreve no evento de mudança de estado
        EventManager.onStateChange += UpdateDebugText;
    }

    void Update()
    {
        // Atualiza status do chão sempre
        bool grounded = yemmaPhysics.IsGrounded();
        UpdateGroundedStatus(grounded);
        
        // Atualiza coyote time
        UpdateCoyoteTime(grounded);
        
        // Atualiza a máquina de estados
        stateMachine.Update();
        
        // Reset jump input após processamento
        if (inputManager.jump)
            inputManager.jump = false;
    }

    void FixedUpdate()
    {
        // Aplica spring force primeiro usando configurações atuais
        yemmaPhysics.ApplySpringForceToGround(settings);
        
        // Aplica rotação baseada no input
        ApplyRotation();
        
        // Aplica movimento automaticamente baseado no estado atual
        ApplyMovement();
        
        // Atualiza física da máquina de estados
        stateMachine.FixedUpdate();
    }

    private void ApplyMovement()
    {
        if (settings == null) return;
        
        Vector2 movementInput = inputManager.movementVector;
        Vector3 currentVelocity = yemmaPhysics.GetVelocity();
        float targetSpeed = GetCurrentStateSpeed();
        
        Vector3 force = yemmaPhysics.CalculatePlayerMovement(
            movementInput,
            currentVelocity,
            targetSpeed,
            settings.acceleration,
            settings.deceleration,
            settings.inputThreshold
        );
        
        yemmaPhysics.ApplyForce(force, ForceMode.Acceleration);
    }

    private void ApplyRotation()
    {
        if (settings == null || playerModel == null) return;
        
        Vector2 movementInput = inputManager.movementVector;
        yemmaPhysics.ApplyRotation(
            movementInput,
            settings.inputThreshold,
            settings.rotationSpeed,
            playerModel
        );
    }

    private float GetCurrentStateSpeed()
    {
        IState currentState = stateMachine.GetCurrentState();
        
        if (currentState is WalkState)
            return settings.walkSpeed;
        else if (currentState is RunState)
            return settings.runSpeed;
        else if (currentState is JumpState)
            return settings.walkSpeed; // Permite movimento no ar
        else
            return 0f; // Idle ou outros estados param
    }

    public StateMachine GetStateMachine()
    {
        return stateMachine;
    }

    public YemmaInputManager GetInputManager()
    {
        return inputManager;
    }

    public YemmaPhysics GetPhysics()
    {
        return yemmaPhysics;
    }

    public YemmaSettings GetSettings()
    {
        return settings;
    }

    public Transform GetPlayerModel()
    {
        return playerModel;
    }

    public YemmaAnim GetAnimation()
    {
        return yemmaAnim;
    }

    public ClimbPhysics GetClimbPhysics()
    {
        return climbPhysics;
    }

    public Animator GetAnimator()
    {
        return playerAnimator;
    }

    public void UpdateGroundedStatus(bool grounded)
    {
        isGrounded = grounded;
    }

    private void UpdateCoyoteTime(bool grounded)
    {
        // Se estava no chão no frame anterior e agora não está mais
        if (wasGroundedLastFrame && !grounded)
        {
            lastGroundedTime = Time.time;
        }
        
        wasGroundedLastFrame = grounded;
    }

    public bool CanCoyoteJump()
    {
        if (settings == null) return false;
        return Time.time - lastGroundedTime <= settings.coyoteTime;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void UpdateDebugText(string stateName)
    {
        if (debugStateText != null)
            debugStateText.text = $"State: {stateName}";
    }

    void OnDestroy()
    {
        EventManager.onStateChange -= UpdateDebugText;
    }
}
