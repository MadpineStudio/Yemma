using UnityEngine;

public class YemmaDebug : MonoBehaviour
{
    private YemmaBaseController controller;

    void Start()
    {
        controller = GetComponent<YemmaBaseController>();
    }

    void OnDrawGizmos()
    {
        // Se não tem controller em runtime, pega os valores padrão
        if (controller == null)
            controller = GetComponent<YemmaBaseController>();

        YemmaSettings settings = controller?.GetSettings();
        YemmaPhysics physics = controller?.GetPhysics();
        
        // Se não tem settings, não desenha
        if (settings == null) return;
        
        // Ground check gizmos - agora usa as mesmas configurações do spring
        if (settings.showGroundCheck)
        {
            float distance = settings.springRaycastDistance;
            bool isGrounded = physics?.IsGrounded() ?? false;

            Gizmos.color = isGrounded ? settings.groundedColor : settings.groundCheckColor;

            Vector3 origin = physics?.GetSpringRaycastOrigin(settings) ?? transform.position;
            Vector3 end = origin + Vector3.down * distance;

            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, 0.05f);
        }
        
        // Spring force gizmos
        if (settings.showSpringForce && physics != null)
        {
            Vector3 raycastOrigin = physics.GetSpringRaycastOrigin(settings);
            float springDistance = settings.springRaycastDistance;
            
            // Linha do raycast
            Gizmos.color = settings.springRayColor;
            Gizmos.DrawRay(raycastOrigin, Vector3.down * springDistance);
            
            // Posição do raycast offset
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(raycastOrigin, 0.05f);
            
            RaycastHit hit = physics.GetSpringRaycastInfo(settings);
            if (hit.collider != null)
            {
                // Ponto de contato no chão
                Gizmos.color = settings.springHitColor;
                Gizmos.DrawSphere(hit.point, 0.1f);
                
                // Altura da posição de repouso
                Vector3 restPosition = hit.point + Vector3.up * settings.restLength;
                Gizmos.color = settings.enableSpringForce ? settings.springRestColor : Color.gray;
                Gizmos.DrawSphere(restPosition, 0.15f);
                Gizmos.DrawLine(hit.point, restPosition);
            }
        }
        
        // Climb detection gizmos
        if (settings.showClimbDetection)
        {
            ClimbPhysics climbPhysics = controller?.GetClimbPhysics();
            if (climbPhysics != null)
            {
                climbPhysics.DrawClimbDebugGizmos(settings, settings.climbRayColor, settings.climbHitColor);
            }
        }
    }
}