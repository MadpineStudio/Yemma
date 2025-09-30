using UnityEngine;

public class YemmaPhysics
{
    private Transform transform;
    private Rigidbody rb;
    private float groundCheckDistance = 0.1f;
    private LayerMask groundLayer = 1;
    private Transform groundCheckPoint;
    
    // Spring Force settings
    private bool enableSpringForce = true;
    private float springConstant = 100f;
    private float dampingConstant = 10f;
    private float restLength = 2f;
    private float springRaycastDistance = 5f;
    private Vector3 springRaycastOffset = Vector3.zero;
    private bool springForceActive = true;
    
    // Rotation settings
    private bool rotationEnabled = true;

    public YemmaPhysics(Transform ownerTransform, Rigidbody rigidbody = null)
    {
        transform = ownerTransform;
        rb = rigidbody ?? ownerTransform.GetComponent<Rigidbody>();
        groundCheckPoint = transform;
    }

    public void SetGroundCheckSettings(float distance, LayerMask layer, Transform checkPoint = null)
    {
        groundCheckDistance = distance;
        groundLayer = layer;
        if (checkPoint != null)
            groundCheckPoint = checkPoint;
    }

    public void SetSpringForceSettings(bool enable, float spring, float damping, float rest, float raycastDist, Vector3 offset)
    {
        enableSpringForce = enable;
        springConstant = spring;
        dampingConstant = damping;
        restLength = rest;
        springRaycastDistance = raycastDist;
        springRaycastOffset = offset;
    }

    public float GetGroundCheckDistance()
    {
        return springRaycastDistance;
    }

    public void ApplyMovement(Vector3 movement, float speed, bool useFixedDeltaTime = true)
    {
        float deltaTime = useFixedDeltaTime ? Time.fixedDeltaTime : Time.deltaTime;
        transform.Translate(movement * speed * deltaTime);
    }

    public Vector3 CalculatePlayerMovement(Vector2 inputDirection, Vector3 currentVelocity, float maxVelocity, float acceleration, float deceleration, float inputThreshold = 0.1f)
    {
        Vector3 cameraRelativeMovement = CalculateCameraRelativeMovement(inputDirection, Camera.main.transform);
        Vector3 targetVelocity = cameraRelativeMovement * maxVelocity;
        
        Vector3 velocityDifference = targetVelocity - new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        // Determina se está acelerando ou desacelerando
        float accelRate = targetVelocity.magnitude > inputThreshold 
            ? acceleration 
            : deceleration;

        // Reduz apenas a desaceleração no ar para manter controle
        if (!IsGrounded() && targetVelocity.magnitude <= inputThreshold)
        {
            accelRate *= 0.3f; // Só reduz desaceleração quando não há input
        }

        // Aplica força baseada na diferença de velocidade
        Vector3 force = new Vector3(
            CalculateAxisForce(velocityDifference.x, accelRate),
            0,
            CalculateAxisForce(velocityDifference.z, accelRate)
        );

        return force;
    }

    private Vector3 CalculateCameraRelativeMovement(Vector2 inputDirection, Transform cameraTransform)
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        
        forward.y = 0;
        right.y = 0;
        
        forward.Normalize();
        right.Normalize();
        
        return (forward * inputDirection.y + right * inputDirection.x);
    }

    private float CalculateAxisForce(float velocityDifference, float accelRate)
    {
        return velocityDifference * accelRate;
    }

    public void ApplyForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
    {
        if (rb != null)
            rb.AddForce(force, forceMode);
    }

    public void ApplySpringForceToGround(YemmaSettings settings)
    {
        if (settings == null || !settings.enableSpringForce || !springForceActive || rb == null) return;
        
        Vector3 raycastOrigin = transform.position + settings.springRaycastOffset;
        
        if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, settings.springRaycastDistance, groundLayer))
        {
            float currentDistance = hit.distance;
            float displacement = currentDistance - settings.restLength;
            
            // F = -kx - cv (Lei de Hooke com amortecimento)
            float springForce = -settings.springConstant * displacement;
            float dampingForce = -settings.dampingConstant * rb.linearVelocity.y;
            float totalForce = springForce + dampingForce;
            
            rb.AddForce(Vector3.up * totalForce);
        }
    }

    public void SetSpringEnabled(bool enabled)
    {
        springForceActive = enabled;
    }

    public void SetRotationEnabled(bool enabled)
    {
        rotationEnabled = enabled;
    }

    public void ApplySpringForce(Vector3 targetPosition, float springStrength, float damping)
    {
        if (rb == null) return;
        
        Vector3 displacement = targetPosition - transform.position;
        Vector3 springForce = displacement * springStrength;
        Vector3 dampingForce = -rb.linearVelocity * damping;
        
        rb.AddForce(springForce + dampingForce);
    }

    public Rigidbody GetRigidbody()
    {
        return rb;
    }

    public Vector3 GetVelocity()
    {
        return rb != null ? rb.linearVelocity : Vector3.zero;
    }

    public void SetVelocity(Vector3 velocity)
    {
        if (rb != null)
            rb.linearVelocity = velocity;
    }

    public bool IsGrounded()
    {
        Vector3 raycastOrigin = transform.position + springRaycastOffset;
        return Physics.Raycast(raycastOrigin, Vector3.down, springRaycastDistance, groundLayer);
    }

    public RaycastHit GetGroundInfo()
    {
        Vector3 raycastOrigin = transform.position + springRaycastOffset;
        Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, springRaycastDistance, groundLayer);
        return hit;
    }

    public RaycastHit GetSpringRaycastInfo(YemmaSettings settings)
    {
        if (settings == null) return new RaycastHit();
        
        Vector3 raycastOrigin = transform.position + settings.springRaycastOffset;
        Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, settings.springRaycastDistance, groundLayer);
        return hit;
    }

    public Vector3 GetSpringRaycastOrigin(YemmaSettings settings)
    {
        if (settings == null) return transform.position;
        return transform.position + settings.springRaycastOffset;
    }

    public void ApplyRotation(Vector2 inputDirection, float inputThreshold, float rotationSpeed, Transform modelTransform)
    {
        // Se rotação está desabilitada, não faz nada
        if (!rotationEnabled) return;
        
        if (inputDirection.magnitude < inputThreshold || modelTransform == null) return;

        // Calcula movimento relativo à câmera
        Vector3 cameraRelativeMovement = CalculateCameraRelativeMovement(inputDirection, Camera.main.transform);
        
        if (cameraRelativeMovement.magnitude < inputThreshold) return;

        // Aplica rotação direta baseada na direção do movimento
        Quaternion targetRotation = Quaternion.LookRotation(cameraRelativeMovement, Vector3.up);
        
        // Interpolação mais suave com curva
        float smoothRotationSpeed = rotationSpeed * Time.fixedDeltaTime;
        float smoothFactor = 1f - Mathf.Exp(-smoothRotationSpeed);
        
        modelTransform.rotation = Quaternion.Slerp(
            modelTransform.rotation, 
            targetRotation, 
            smoothFactor
        );
    }
}