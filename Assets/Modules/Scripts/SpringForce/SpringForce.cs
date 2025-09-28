using UnityEngine;

public class SpringForce : MonoBehaviour
{
    [SerializeField] private float springConstant = 100f;
    [SerializeField] private float dampingConstant = 10f;
    [SerializeField] private float restLength = 2f;
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private LayerMask groundMask = 1;
    [SerializeField] private bool enableSpringForce = true;
    [SerializeField] private Vector3 raycastOffset = Vector3.zero;
    
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!enableSpringForce) return;
        
        Vector3 raycastOrigin = transform.position + raycastOffset;
        
        if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            float currentDistance = hit.distance;
            float displacement = currentDistance - restLength;
            
            // F = -kx - cv (Lei de Hooke com amortecimento)
            float springForce = -springConstant * displacement;
            float dampingForce = -dampingConstant * rb.linearVelocity.y;
            float totalForce = springForce + dampingForce;
            
            rb.AddForce(Vector3.up * totalForce);
        }
    }
    
    void OnDrawGizmos()
    {
        Vector3 raycastOrigin = transform.position + raycastOffset;
        
        // Linha do raycast
        Gizmos.color = Color.red;
        Gizmos.DrawRay(raycastOrigin, Vector3.down * raycastDistance);
        
        // Posição do raycast offset
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(raycastOrigin, 0.05f);
        
        if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            // Ponto de contato no chão
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(hit.point, 0.1f);
            
            // Altura da posição de repouso
            Vector3 restPosition = hit.point + Vector3.up * restLength;
            Gizmos.color = enableSpringForce ? Color.green : Color.gray;
            Gizmos.DrawSphere(restPosition, 0.15f);
            Gizmos.DrawLine(hit.point, restPosition);
        }
    }
}
