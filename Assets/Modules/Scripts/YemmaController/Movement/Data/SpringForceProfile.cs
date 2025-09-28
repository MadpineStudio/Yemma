using UnityEngine;

[CreateAssetMenu(fileName = "SpringForceProfile", menuName = "Yemma/Spring Force Profile", order = 1)]
public class SpringForceProfile : ScriptableObject
{
    [Header("Spring Force Configuration")]
    [Tooltip("Constante da mola")]
    public float springConstant = 100f;
    
    [Tooltip("Constante de amortecimento")]
    public float dampingConstant = 10f;
    
    [Tooltip("Distância de repouso da mola")]
    public float restLength = 2f;
    
    [Tooltip("Distância máxima do raycast")]
    public float raycastDistance = 5f;
    
    [Tooltip("Máscara de camadas do chão")]
    public LayerMask groundMask = 1;
    
    [Tooltip("Habilitar sistema de molas")]
    public bool enableSpringForce = true;
    
    [Tooltip("Offset do raycast")]
    public Vector3 raycastOffset = Vector3.zero;
    
    [Tooltip("Mostrar debug visual do sistema de molas")]
    public bool showSpringDebug = true;
}