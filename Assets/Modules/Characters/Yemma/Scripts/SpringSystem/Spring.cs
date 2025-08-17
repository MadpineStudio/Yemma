using UnityEngine;

public class Spring : MonoBehaviour {
    public float restDistance = 1.0f;
    public float springConstant = 50f;
    public float damping = 5f;
    public float frictionCoefficient = 10f;
    public LayerMask groundLayer;
    public float activationMultiplier = 1.2f; 
    public Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }
    void UpdateSpring() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, restDistance * 2f, groundLayer)) {
            if (hit.distance < restDistance * activationMultiplier) {
                Vector3 groundNormal = hit.normal;
                float displacement = restDistance - hit.distance;
                if (displacement > 0) {
                    Vector3 springForce = groundNormal * (springConstant * displacement);
                    float verticalVel = Vector3.Dot(rb.linearVelocity, groundNormal);
                    Vector3 dampingForce = groundNormal * (-verticalVel * damping);

                    Vector3 linearVelocity = rb.linearVelocity;
                    Vector3 tangentialVelocity = linearVelocity - Vector3.Project(linearVelocity, groundNormal);
                    Vector3 frictionForce = -tangentialVelocity * frictionCoefficient;
                    
                    rb.AddForce(springForce + dampingForce + frictionForce);
                }
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.green;
        var position = transform.position;
        Vector3 rayStart = position;
        Vector3 rayEnd = rayStart + Vector3.down * restDistance * 2f;
        Gizmos.DrawLine(rayStart, rayEnd);

        Gizmos.color = Color.blue;
        Vector3 restPos = position + Vector3.down * restDistance;
        Gizmos.DrawSphere(restPos, 0.1f);

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, restDistance * 2f, groundLayer)) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hit.point, restPos);
        }
    }
}
