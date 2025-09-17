using UnityEngine;

namespace Yemma.Movement.Data
{
    /// <summary>
    /// Configurações de movimentação para o sistema Yemma
    /// ScriptableObject modular e extensível para diferentes tipos de movimento
    /// </summary>
    [CreateAssetMenu(fileName = "YemmaMovementProfile", menuName = "Yemma/Movement Profile", order = 0)]
    public class YemmaMovementProfile : ScriptableObject
    {
        [Header("Basic Movement")]
        [Tooltip("Velocidade máxima de movimento")]
        [Range(1f, 20f)]
        public float maxVelocity = 6f;
        
        [Tooltip("Taxa de aceleração")]
        [Range(1f, 30f)]
        public float acceleration = 10f;
        
        [Tooltip("Taxa de desaceleração")]
        [Range(1f, 30f)]
        public float deceleration = 15f;
        
        [Tooltip("Curva de potência da velocidade (suavidade do movimento)")]
        [Range(0.5f, 2f)]
        public float velocityPower = 0.96f;

        [Header("Rotation & Orientation")]
        [Tooltip("Velocidade de rotação do player")]
        [Range(1f, 30f)]
        public float rotationSpeed = 12f;
        
        [Tooltip("Velocidade de alinhamento ao terreno")]
        [Range(1f, 20f)]
        public float terrainAlignmentSpeed = 8f;
        
        [Tooltip("Quantidade de inclinação baseada no movimento")]
        [Range(0f, 45f)]
        public float tiltAmount = 15f;
        
        [Tooltip("Velocidade da inclinação")]
        [Range(1f, 20f)]
        public float tiltSpeed = 10f;

        [Header("Ground Detection")]
        [Tooltip("Camadas consideradas como chão")]
        public LayerMask groundLayers = 1;
        
        [Tooltip("Distância máxima de detecção do chão")]
        [Range(0.1f, 2f)]
        public float groundCheckDistance = 0.4f;
        
        [Tooltip("Offset para raycast de detecção do chão")]
        public Vector3 groundCheckOffset = Vector3.up * 0.2f;

        [Header("Physics")]
        [Tooltip("Multiplicador de força de atrito")]
        [Range(0f, 2f)]
        public float frictionMultiplier = 1f;

        [Tooltip("Força aplicada no rigidbody ao executar um pulo")]
        [Range(0f, 100f)]
        public float jumpForce = 10f;

        [Tooltip("Drag aplicado quando não há input")]
        [Range(0f, 10f)]
        public float airDrag = 2f;
        
        [Tooltip("Força gravitacional adicional")]
        [Range(0f, 20f)]
        public float additionalGravity = 0f;

        [Header("Ground Damping System")]
        [SerializeField]
        [Tooltip("Ativar sistema de amortecimento do solo")]
        public bool enableGroundDamping = true;
        
        [SerializeField]
        [Tooltip("Distância desejada do player ao chão")]
        [Range(0.1f, 2f)]
        public float desiredGroundDistance = 0.5f;
        
        [SerializeField]
        [Tooltip("Força do spring (rigidez do amortecimento)")]
        [Range(10f, 1000f)]
        public float springForce = 300f;
        
        [SerializeField]
        [Tooltip("Amortecimento do spring (reduz oscilações)")]
        [Range(1f, 50f)]
        public float springDamping = 20f;
        
        [SerializeField]
        [Tooltip("Força máxima que pode ser aplicada pelo amortecimento")]
        [Range(10f, 100f)]
        public float maxDampingForce = 50f;
        
        [SerializeField]
        [Tooltip("Tolerância para considerar na distância correta")]
        [Range(0.01f, 0.2f)]
        public float dampingTolerance = 0.05f;

        [Header("State Transition")]
        [Tooltip("Threshold mínimo de input para transição Idle -> Walk")]
        [Range(0.01f, 0.5f)]
        public float movementInputThreshold = 0.01f;
        
        [Tooltip("Velocidade mínima para manter estado Walk")]
        [Range(0.01f, 1f)]
        public float minimumWalkSpeed = 0.1f;

        [Header("Advanced Settings")]
        [Tooltip("Curva de responsividade do input")]
        public AnimationCurve inputResponseCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        [Tooltip("Multiplier de velocidade em superfícies inclinadas")]
        [Range(0.5f, 1.5f)]
        public float slopeSpeedMultiplier = 0.8f;
        
        [Tooltip("Ângulo máximo considerado caminhável")]
        [Range(30f, 60f)]
        public float maxWalkableAngle = 45f;

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Mostrar raios de debug no Scene")]
        public bool showDebugRays = true;
        
        [SerializeField]
        [Tooltip("Cor dos raios de debug")]
        public Color debugRayColor = Color.green;
        
        [SerializeField]
        [Tooltip("Mostrar debug do sistema de amortecimento")]
        public bool showDampingDebug = true;
        
        [SerializeField]
        [Tooltip("Cor do debug de amortecimento")]
        public Color dampingDebugColor = Color.cyan;

        // Propriedades calculadas
        public float InputThreshold => movementInputThreshold;
        public float WalkSpeedThreshold => minimumWalkSpeed;
        public bool IsDebugEnabled => showDebugRays;

        /// <summary>
        /// Aplica curva de responsividade ao input
        /// </summary>
        public float ApplyInputCurve(float inputValue)
        {
            return inputResponseCurve.Evaluate(Mathf.Abs(inputValue)) * Mathf.Sign(inputValue);
        }

        /// <summary>
        /// Calcula multiplicador de velocidade baseado na inclinação
        /// </summary>
        public float CalculateSlopeMultiplier(float slopeAngle)
        {
            if (slopeAngle > maxWalkableAngle)
                return 0f;
            
            float normalizedAngle = slopeAngle / maxWalkableAngle;
            return Mathf.Lerp(1f, slopeSpeedMultiplier, normalizedAngle);
        }

        /// <summary>
        /// Verifica se a superfície é caminhável
        /// </summary>
        public bool IsSurfaceWalkable(Vector3 surfaceNormal)
        {
            float angle = Vector3.Angle(Vector3.up, surfaceNormal);
            return angle <= maxWalkableAngle;
        }

        /// <summary>
        /// Valida se o perfil está corretamente configurado
        /// </summary>
        public bool ValidateProfile()
        {
            bool isValid = true;

            if (maxVelocity <= 0)
            {
                Debug.LogError($"[{name}] maxVelocity deve ser maior que 0!");
                isValid = false;
            }

            if (acceleration <= 0 || deceleration <= 0)
            {
                Debug.LogError($"[{name}] acceleration e deceleration devem ser maiores que 0!");
                isValid = false;
            }

            if (groundLayers == 0)
            {
                Debug.LogWarning($"[{name}] groundLayers não configurado!");
            }

            return isValid;
        }

        // Métodos de conveniência para backward compatibility
        public float velocity => maxVelocity;
        public float decceleration => deceleration;
        public float velPower => velocityPower;
        public float rotationVelocity => rotationSpeed;
        public float tiltVelocity => terrainAlignmentSpeed;
        public LayerMask groundLayer => groundLayers;

        private void OnValidate()
        {
            // Garante que deceleration seja sempre >= acceleration para comportamento natural
            if (deceleration < acceleration)
            {
                deceleration = acceleration;
            }
            
            // Força atualização do Inspector
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
}
