using UnityEngine;

public class PlayerMovementBase : MonoBehaviour
{
    private bool jumped;
    [SerializeField] private PlayerTestMovement playerTestMovement;
    [SerializeField] private Transform mainCam;
    [SerializeField] private Transform yemmaBody;
    [SerializeField] private bool debug;
    [SerializeField] private float rotationRaycastSize;
    [SerializeField] private float frictionCoefficient;
    [SerializeField] private Vector3 rotationRaycastOffset;
    private Vector3 _lastPosition;

    void Start()
    {
        _lastPosition = yemmaBody.position;
    }

    Vector3 Tilt(Vector3 lastPosition)
    {
        //TODO: Otimizar e separar se possivel
        // adjust lookatRotation
        Quaternion tiltRotation = yemmaBody.rotation;
        Quaternion terrainRotation = yemmaBody.rotation;
        Transform currentPlayerTransform = yemmaBody;
        Vector3 direction = currentPlayerTransform.position - lastPosition;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        RaycastHit hit;
        if (Physics.Raycast(yemmaBody.position + rotationRaycastOffset, -yemmaBody.up, out hit, rotationRaycastSize, playerTestMovement.PlayerPhysicalProfile.groundLayer))
        {
            Vector3 groundNormal = hit.normal;
            Quaternion targetGroundRotation = Quaternion.FromToRotation(yemmaBody.up, groundNormal) * yemmaBody.rotation;
            terrainRotation = targetGroundRotation;
        }

        if (playerTestMovement.playerRb.linearVelocity.magnitude > 0.01f)
        {
            direction = Quaternion.Inverse(currentPlayerTransform.rotation) * direction;
            Vector3 newRotation = new Vector3(direction.z * 100 * playerTestMovement.PlayerPhysicalProfile.tiltAmount, targetRotation.eulerAngles.y, -direction.x * 100 * playerTestMovement.PlayerPhysicalProfile.tiltAmount);
            newRotation.x += terrainRotation.eulerAngles.x;
            newRotation.z += terrainRotation.eulerAngles.z;
            tiltRotation = Quaternion.Euler(newRotation);
        }
        
        currentPlayerTransform.rotation = Quaternion.Slerp(currentPlayerTransform.rotation, tiltRotation, playerTestMovement.PlayerPhysicalProfile.rotationVelocity * Time.deltaTime);
        return currentPlayerTransform.position;
    }
    void Move()
    {
        Vector2 inputDirection = playerTestMovement.InputManager.movementVector;
        Vector3 movement = CalculatePlayerMovement(
            playerTestMovement.playerRb.linearVelocity,
            inputDirection,
            mainCam,
            playerTestMovement.PlayerPhysicalProfile
        );

        Vector3 springForce = CalculateSpringForce(
            transform,
            playerTestMovement.playerRb,
            playerTestMovement.PlayerPhysicalProfile
        );
        playerTestMovement.playerRb.AddForce(movement + springForce , ForceMode.Acceleration);
    }
    public Vector3 CalculateCameraRelativeMovement(Vector2 inputDirection, Transform cameraTransform)
    {
        inputDirection = inputDirection.normalized;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 movement = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);
        return movement;
    }
    public Vector3 CalculatePlayerMovement(Vector3 currentVelocity, Vector2 inputDirection, Transform cameraTransform, PhysicalProfile profile)
    {
        Vector3 cameraRelativeMovement = CalculateCameraRelativeMovement(inputDirection, cameraTransform);
        Vector3 targetSpeed = cameraRelativeMovement * profile.velocity;
        Vector3 speedDif = targetSpeed - currentVelocity;
        float accelRate = targetSpeed.magnitude > 0.01f
            ? profile.acceleration
            : profile.decceleration;
        Vector3 movement = new Vector3(
            Mathf.Pow(Mathf.Abs(speedDif.x) * accelRate, profile.velPower) * Mathf.Sign(speedDif.x),
            0,
            Mathf.Pow(Mathf.Abs(speedDif.z) * accelRate, profile.velPower) * Mathf.Sign(speedDif.z)
        );
        return movement;
    }
    public Vector3 CalculateSpringForce(Transform playerTransform, Rigidbody playerRb, PhysicalProfile profile)
    {
        if (Physics.Raycast(playerTransform.position + profile.springOffsset, Vector3.down, out RaycastHit hit, profile.restDistance * 2f, profile.groundLayer) && !jumped)
        {
            if (hit.distance < profile.restDistance * profile.activationMultiplier)
            {
                Vector3 groundNormal = hit.normal;
                float displacement = profile.restDistance - hit.distance;

                if (displacement > 0)
                {
                    Vector3 springForce = Vector3.up * (profile.springConstant * displacement);
                    float verticalVel = Vector3.Dot(playerRb.linearVelocity, groundNormal); // Usamos velocity em vez de linearVelocity
                    Vector3 dampingForce = Vector3.up * (-verticalVel * profile.damping);

                    //atrito? talvez
                    // Vector3 linearVelocity = playerRb.linearVelocity;
                    // Vector3 tangentialVelocity = linearVelocity - Vector3.Project(linearVelocity, groundNormal);
                    // Vector3 frictionForce = -tangentialVelocity * frictionCoefficient;

                    return springForce + dampingForce;
                }
            }
        }
        return Vector3.zero;
    }

    private void Jump()
    {
        jumped = true;
        // Vector3 jumpDir = yemmaBody.forward * playerTestMovement.PlayerPhysicalProfile.jumpForceDirection.y;
        Vector3 jumpDir = transform.up * playerTestMovement.PlayerPhysicalProfile.jumpForceDirection.y;
        jumpDir.y += playerTestMovement.PlayerPhysicalProfile.jumpForceDirection.y;
        playerTestMovement.playerRb.AddForce(jumpDir, ForceMode.Impulse);
    }
    private void FixedUpdate()
    {
        Move();
        _lastPosition = Tilt(_lastPosition);
        if (playerTestMovement.InputManager.jump && !jumped)
        {
            Jump();
        }
        else if (!playerTestMovement.InputManager.jump && jumped)
        {
            jumped = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        // Desenha o raycast na direção da superfície
        Gizmos.DrawLine(yemmaBody.position + rotationRaycastOffset, yemmaBody.position + (-yemmaBody.up * rotationRaycastSize));
        // Desenha uma pequena esfera no ponto de impacto (se houver)
        RaycastHit hit;
        if (Physics.Raycast(yemmaBody.position + rotationRaycastOffset, -yemmaBody.up, out hit, rotationRaycastSize, playerTestMovement.PlayerPhysicalProfile.groundLayer))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.1f); // Desenha uma esfera no ponto de colisão
        }
    }
}
