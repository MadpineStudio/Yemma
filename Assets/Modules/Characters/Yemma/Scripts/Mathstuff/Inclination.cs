using UnityEngine;

public class Inclination : MonoBehaviour
{
    public float raycastDistance = 1.5f;
    public float rotationSpeed = 5f;

    private Quaternion GetDesiredRotation()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, raycastDistance))
        {
            // Calcula a rotação desejada baseada na normal da superfície
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            return targetRotation;
        }
        // Se não houver colisão, retorna a rotação atual
        return transform.rotation;
    }

    private void Update()
    {
        // Obtém a rotação desejada
        Quaternion desiredRotation = GetDesiredRotation();
        // Aplica a rotação suavemente
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime * 20f);
    }
    private void OnDrawGizmos()
    {
        // Define a cor do Gizmo para verde
        Gizmos.color = Color.green;

        // Desenha o raycast na direção da superfície
        Gizmos.DrawLine(transform.position, transform.position + (-transform.up * raycastDistance));

        // Desenha uma pequena esfera no ponto de impacto (se houver)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, raycastDistance))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.1f); // Desenha uma esfera no ponto de colisão
        }
    }
}