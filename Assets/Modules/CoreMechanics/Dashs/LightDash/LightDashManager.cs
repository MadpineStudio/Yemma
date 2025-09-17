using UnityEngine;
using UnityEngine.Events;
using Yemma;
using Yemma.Movement.StateMachine.States;

public class LightDashManager : MonoBehaviour
{
    [Header("Light Dash Configuration")]
    [SerializeField] private Transform dashPoint;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private bool normalizeDirection = true;
    
    [Header("Visual")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int curveResolution = 20;
    [SerializeField] private float curveHeight = 2f;
    
    [Header("Detection")]
    [SerializeField] private LayerMask playerLayerMask = 1;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Input Detection")]
    [SerializeField] private bool requireJumpFallStates = true;
    
    [Header("Events")]
    public UnityEvent<Transform, float> OnLightDashTriggered;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebug = true;
    
    // Private variables
    private YemmaController playerInArea;
    private bool isPlayerInArea = false;
    
    // Properties
    public Transform DashPoint => dashPoint;
    public float DashSpeed => dashSpeed;
    public bool IsPlayerInArea => isPlayerInArea;

    private void Start()
    {
        // Ensure we have a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"LightDashManager '{name}' precisa de um Collider component!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"LightDashManager '{name}': Collider deve estar marcado como Trigger!");
        }
        
        UpdateLineRenderer();
    }

    private void Update()
    {
        // Check for jump input when player is in area
        if (isPlayerInArea && playerInArea != null)
        {
            CheckForLightDashInput();
        }
        
        UpdateLineRenderer();
    }
    
    private void UpdateLineRenderer()
    {
        if (lineRenderer != null && dashPoint != null)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = dashPoint.position;
            Vector3 midPoint = (startPos + endPos) / 2f + Vector3.up * curveHeight;
            
            lineRenderer.positionCount = curveResolution;
            
            for (int i = 0; i < curveResolution; i++)
            {
                float t = (float)i / (curveResolution - 1);
                Vector3 curvePoint = CalculateQuadraticBezier(startPos, midPoint, endPos, t);
                lineRenderer.SetPosition(i, curvePoint);
            }
        }
    }
    
    private Vector3 CalculateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 point = uu * p0 + 2 * u * t * p1 + tt * p2;
        return point;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (IsPlayer(other))
        {
            YemmaController yemma = other.GetComponent<YemmaController>();
            if (yemma != null)
            {
                playerInArea = yemma;
                isPlayerInArea = true;
                
                if (enableDebug)
                {
                    Debug.Log($"LightDashManager '{name}': Player entrou na área");
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if it's the player leaving
        if (IsPlayer(other) && playerInArea != null)
        {
            if (enableDebug)
            {
                Debug.Log($"LightDashManager '{name}': Player saiu da área");
            }
            
            playerInArea = null;
            isPlayerInArea = false;
        }
    }
    
    private bool IsPlayer(Collider other)
    {
        // Check by tag and layer mask
        bool isCorrectTag = other.CompareTag(playerTag);
        bool isCorrectLayer = ((1 << other.gameObject.layer) & playerLayerMask) != 0;
        
        return isCorrectTag || isCorrectLayer;
    }
    
    private void CheckForLightDashInput()
    {
        // Get the current state to check if we're in jump/fall
        if (requireJumpFallStates && !IsInValidState())
        {
            return;
        }
        
        // Check for jump input using the InputManager directly
        if (playerInArea.InputManager.inputActions.YemmaKeyboard.Jump.WasPressedThisFrame())
        {
            TriggerLightDash();
        }
    }
    
    private bool IsInValidState()
    {
        if (playerInArea?.MovementStateMachine?.CurrentState == null)
        {
            return false;
        }
        
        var currentState = playerInArea.MovementStateMachine.CurrentState;
        
        // Check if current state is jump or fall
        bool isJumpState = currentState is YemmaJumpState;
        bool isFallState = currentState is YemmaFallState;
        
        return isJumpState || isFallState;
    }
    
    private void TriggerLightDash()
    {
        if (dashPoint == null)
        {
            if (enableDebug)
            {
                Debug.LogWarning($"LightDashManager '{name}': DashPoint não configurado!");
            }
            return;
        }
        
        if (enableDebug)
        {
            Debug.Log($"LightDashManager '{name}': Light Dash ativado! Destino: {dashPoint.position}, Velocidade: {dashSpeed}");
        }
        
        OnLightDashTriggered?.Invoke(dashPoint, dashSpeed);
        
        if (playerInArea != null)
        {
            playerInArea.OnLightDashActivated?.Invoke(dashPoint, dashSpeed);
        }
    }
    
    public Vector3 GetControlPoint(Vector3 start, Vector3 end)
    {
        return (start + end) / 2f + Vector3.up * curveHeight;
    }
    
    public float GetCurveHeight()
    {
        return curveHeight;
    }
    
    // Public methods for external configuration
    public void SetDashConfiguration(Transform point, float speed)
    {
        dashPoint = point;
        dashSpeed = Mathf.Max(0f, speed);
    }
}
