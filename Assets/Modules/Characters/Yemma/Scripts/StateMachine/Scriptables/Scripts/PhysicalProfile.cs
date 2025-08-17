using UnityEngine;

[CreateAssetMenu(fileName = "NewPhysicalProfile", menuName = "Player/Profiles", order = 0)]
public class PhysicalProfile : ScriptableObject
{
    public float velocity;
    public float acceleration;
    public float decceleration;
    public float maxSpeed;
    public float velPower;
    public float rotationVelocity;
    
    [Header("Spring System")]
    public float restDistance = 1.0f;
    public float springConstant = 50f;
    public float damping = 5f;
    public float frictionCoefficient = 10f;
    public LayerMask groundLayer;
    public LayerMask interactableLayer;
    public float activationMultiplier = 1.2f; 
    public Vector3 springOffsset;

    [Header("TiltProperty")]
    public float tiltVelocity;
    public float tiltAmount;
    public float maxTiltAmount;
    [Header("Jump")]
    public Vector3 jumpForceDirection;

    [Header("Climb")] 
    public LayerMask climbLayer;
    public LayerMask climbStairsLayer;
    public float climbTurnVelocity;
    public float climbXVelocity;
    public float climbWallCheckerLength;
    [Header("Roll")]
    public float rollDistance; 
    public float rollDuration;
    public float rollDelay;
    [Header("Glide")]
    public float forwardSpeed;
    public float turnSpeed;
    public float maxGlideInclination;
    public AnimationCurve aoaLiftCurve;
    public AnimationCurve turnCurve;
   
}
