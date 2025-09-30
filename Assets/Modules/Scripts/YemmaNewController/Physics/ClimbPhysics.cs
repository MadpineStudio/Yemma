using UnityEngine;

public class ClimbPhysics
{
    private Transform transform;
    private LayerMask climbableLayer = 1;
    private float climbDetectionDistance = 1f;
    private Vector3 climbRaycastOffset = Vector3.zero;
    private string climbableTag = "Climbable";

    public ClimbPhysics(Transform ownerTransform)
    {
        transform = ownerTransform;
    }

    public void SetClimbDetectionSettings(float distance, LayerMask layer, Vector3 offset, string tag = "Climbable")
    {
        climbDetectionDistance = distance;
        climbableLayer = layer;
        climbRaycastOffset = offset;
        climbableTag = tag;
    }

    /// <summary>
    /// Verifica se pode entrar no estado de climb
    /// Condições: Não está no chão E detecta superfície climbable à frente
    /// </summary>
    public bool CanClimb(bool isGrounded, YemmaSettings settings = null)
    {
        // Primeira condição: deve estar fora do chão
        if (isGrounded) return false;

        // Segunda condição: detecta superfície climbable à frente
        return settings != null ? DetectClimbableSurface(settings) : DetectClimbableSurface();
    }

    /// <summary>
    /// Detecta superfície climbable à frente usando raycast
    /// </summary>
    public bool DetectClimbableSurface()
    {
        Vector3 raycastOrigin = transform.position + climbRaycastOffset;
        Vector3 forwardDirection = transform.forward;

        if (Physics.Raycast(raycastOrigin, forwardDirection, out RaycastHit hit, climbDetectionDistance, climbableLayer))
        {
            // Verifica se o objeto tem a tag correta
            if (hit.collider.CompareTag(climbableTag))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Detecta superfície climbable usando configurações específicas
    /// </summary>
    public bool DetectClimbableSurface(YemmaSettings settings)
    {
        if (settings == null) return DetectClimbableSurface();
        
        Vector3 raycastOrigin = transform.position + settings.climbRaycastOffset;
        Vector3 forwardDirection = transform.forward;

        if (Physics.Raycast(raycastOrigin, forwardDirection, out RaycastHit hit, settings.climbDetectionDistance, settings.climbableLayer))
        {
            // Verifica se o objeto tem a tag correta
            if (hit.collider.CompareTag(settings.climbableTag))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Detecta parede e retorna informações do hit (incluindo normal)
    /// </summary>
    public bool GetWallHitInfo(out RaycastHit hitInfo, YemmaSettings settings = null)
    {
        hitInfo = new RaycastHit();
        
        if (settings != null)
        {
            Vector3 raycastOrigin = transform.position + settings.climbRaycastOffset;
            Vector3 forwardDirection = transform.forward;

            if (Physics.Raycast(raycastOrigin, forwardDirection, out hitInfo, settings.climbDetectionDistance, settings.climbableLayer))
            {
                if (hitInfo.collider.CompareTag(settings.climbableTag))
                {
                    return true;
                }
            }
        }
        else
        {
            Vector3 raycastOrigin = transform.position + climbRaycastOffset;
            Vector3 forwardDirection = transform.forward;

            if (Physics.Raycast(raycastOrigin, forwardDirection, out hitInfo, climbDetectionDistance, climbableLayer))
            {
                if (hitInfo.collider.CompareTag(climbableTag))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Retorna informações do raycast de climb
    /// </summary>
    public RaycastHit GetClimbInfo()
    {
        Vector3 raycastOrigin = transform.position + climbRaycastOffset;
        Vector3 forwardDirection = transform.forward;
        
        Physics.Raycast(raycastOrigin, forwardDirection, out RaycastHit hit, climbDetectionDistance, climbableLayer);
        return hit;
    }

    /// <summary>
    /// Retorna a origem do raycast de climb para debug
    /// </summary>
    public Vector3 GetClimbRaycastOrigin()
    {
        return transform.position + climbRaycastOffset;
    }

    /// <summary>
    /// Retorna a direção do raycast de climb
    /// </summary>
    public Vector3 GetClimbRaycastDirection()
    {
        return transform.forward;
    }

    /// <summary>
    /// Retorna a distância de detecção
    /// </summary>
    public float GetClimbDetectionDistance()
    {
        return climbDetectionDistance;
    }

    /// <summary>
    /// Debug visual do sistema de climb (Gizmos)
    /// </summary>
    public void DrawClimbDebugGizmos(YemmaSettings settings, Color rayColor, Color hitColor)
    {
        if (settings == null) return;
        
        Vector3 raycastOrigin = transform.position + settings.climbRaycastOffset;
        Vector3 forwardDirection = transform.forward;
        
        // Linha do raycast
        Gizmos.color = rayColor;
        Gizmos.DrawRay(raycastOrigin, forwardDirection * settings.climbDetectionDistance);
        
        // Origem do raycast
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(raycastOrigin, 0.05f);
        
        // Se detectou superfície climbable, mostra ponto de contato
        if (DetectClimbableSurface(settings))
        {
            if (Physics.Raycast(raycastOrigin, forwardDirection, out RaycastHit hit, settings.climbDetectionDistance, settings.climbableLayer))
            {
                if (hit.collider.CompareTag(settings.climbableTag))
                {
                    Gizmos.color = hitColor;
                    Gizmos.DrawSphere(hit.point, 0.1f);
                    Gizmos.DrawLine(raycastOrigin, hit.point);
                }
            }
        }
    }
}