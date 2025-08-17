using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class PlayerTestMovement : MonoBehaviour
{
    public InputManager InputManager;
    public Rigidbody playerRb;
    public PhysicalProfile PlayerPhysicalProfile;
}
