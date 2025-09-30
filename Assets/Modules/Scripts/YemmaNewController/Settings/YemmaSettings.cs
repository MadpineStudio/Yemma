using UnityEngine;

[CreateAssetMenu(fileName = "YemmaSettings", menuName = "Yemma/Settings")]
public class YemmaSettings : ScriptableObject
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float inputThreshold = 0.1f;
    public float jumpForce = 10f;
    public float rotationSpeed = 8f;
    public float coyoteTime = 0.2f; // Tempo para permitir pulo após sair do chão
    
    [Header("Ground Check")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer = 1;
    
    [Header("Spring Force")]
    public bool enableSpringForce = true;
    public float springConstant = 100f;
    public float dampingConstant = 10f;
    public float restLength = 2f;
    public float springRaycastDistance = 5f;
    public Vector3 springRaycastOffset = Vector3.zero;
    
    [Header("Climb Detection")]
    public float climbDetectionDistance = 1f;
    public LayerMask climbableLayer = 1;
    public Vector3 climbRaycastOffset = Vector3.zero;
    public string climbableTag = "Climbable";
    
    [Header("Hang Edge")]
    public float hangDistance = 0.8f; // Distância da parede no hang edge
    public float hangDepth = 1.5f; // Profundidade abaixo do ponto de detecção
    public float minHangTime = 0.5f; // Tempo mínimo antes de poder escalar
    public float alignmentSpeed = 75f; // Velocidade do alinhamento à parede
    
    [Header("Debug")]
    public bool showGroundCheck = true;
    public bool showSpringForce = true;
    public bool showClimbDetection = true;
    public Color groundCheckColor = Color.red;
    public Color groundedColor = Color.green;
    public Color springRayColor = Color.red;
    public Color springHitColor = Color.blue;
    public Color springRestColor = Color.green;
    public Color climbRayColor = Color.yellow;
    public Color climbHitColor = Color.orange;
}