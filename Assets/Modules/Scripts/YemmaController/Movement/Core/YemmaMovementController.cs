using UnityEngine;
using Yemma.Movement.Data;

namespace Yemma.Movement.Core
{
    /// <summary>
    /// Controlador principal de movimentação do Yemma
    /// Centraliza todas as funcionalidades de movimento e física
    /// </summary>
    
    public class YemmaMovementController : MonoBehaviour
    {
        [Header("Movement Profile")]
        [SerializeField] private YemmaMovementProfile movementProfile;
        
        [Header("Physics Components")]
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private Transform yemmaTransform;
        [SerializeField] private Animator yemmaAnimator;

        // Propriedades públicas
        public YemmaMovementProfile MovementProfile => movementProfile;
        public PhysicalProfile PhysicalProfile => null; // Backward compatibility - será removido gradualmente
        public Rigidbody Rigidbody => playerRigidbody;
        public Transform Transform => yemmaTransform;
        public Animator Animator => yemmaAnimator;
        public Vector3 Velocity => playerRigidbody.linearVelocity;

        // Sistema de física de movimento
        private YemmaMovementPhysics movementPhysics;
        public YemmaAnimationController animationController;

        // Direção de movimento estável (evita flick quando input ~ 0)
        private Vector3 stableMovementDirection = Vector3.forward;
        // Tempo para decair a direção estável quando realmente para
        private float stableDirectionDecayTime = 0.35f;
        private float lastNonZeroInputTime;
        private float minInputThreshold => movementProfile != null ? movementProfile.InputThreshold : 0.01f;
        private float minSpeedThreshold = 0.05f; // velocidade horizontal mínima para atualizar direção pela velocidade

        private void Awake()
        {
            InitializeComponents();
            movementPhysics = new YemmaMovementPhysics(this);
            animationController = new YemmaAnimationController(this);
        }

        private void InitializeComponents()
        {
            if (playerRigidbody == null)
                playerRigidbody = GetComponent<Rigidbody>();

            if (yemmaTransform == null)
                yemmaTransform = transform;

            if (movementProfile == null)
            {
                Debug.LogError("YemmaMovementProfile não foi atribuído ao YemmaMovementController!");
                // Cria um perfil temporário para evitar erros
                movementProfile = CreateDefaultProfile();
            }
            else
            {
                // Valida o perfil
                movementProfile.ValidateProfile();
            }
        }

        /// <summary>
        /// Cria um perfil padrão temporário
        /// </summary>
        private YemmaMovementProfile CreateDefaultProfile()
        {
            var profile = ScriptableObject.CreateInstance<YemmaMovementProfile>();
            Debug.LogWarning("Usando YemmaMovementProfile padrão temporário. Configure um perfil personalizado!");
            return profile;
        }

        /// <summary>
        /// Verifica se o player está no chão
        /// </summary>
        public bool IsGrounded()
        {
            return Physics.Raycast(
                yemmaTransform.position + movementProfile.groundCheckOffset, 
                Vector3.down, 
                movementProfile.groundCheckDistance, 
                movementProfile.groundLayers
            );
        }

        /// <summary>
        /// Aplica movimento baseado no input
        /// </summary>
        public void ApplyMovement(Vector2 inputDirection)
        {
            Vector3 movementForce = movementPhysics.CalculatePlayerMovement(inputDirection, Velocity);
            playerRigidbody.AddForce(movementForce, ForceMode.Acceleration);

            UpdateStableDirection(inputDirection);
            movementPhysics.ApplyRotation(stableMovementDirection);
        }

        private void UpdateStableDirection(Vector2 inputDirection)
        {
            float inputMag = inputDirection.magnitude;
            Vector3 horizontalVelocity = new Vector3(Velocity.x, 0, Velocity.z);

            // Se há input significativo atualiza pela câmera
            if (inputMag > minInputThreshold * 2f)
            {
                Vector3 newDir = movementPhysics.CalculateCameraRelativeMovement(inputDirection, Camera.main.transform);
                if (newDir.sqrMagnitude > 0.0001f)
                {
                    stableMovementDirection = Vector3.Slerp(stableMovementDirection, newDir.normalized, 0.25f);
                    lastNonZeroInputTime = Time.time;
                }
                return;
            }

            // Sem input relevante: se ainda há velocidade usa ela como referência
            if (horizontalVelocity.magnitude > minSpeedThreshold)
            {
                Vector3 velDir = horizontalVelocity.normalized;
                // Evita flip brusco: só aceita nova direção se ângulo > 150° descartando ruído pequeno
                float angle = Vector3.Angle(stableMovementDirection, velDir);
                if (angle < 150f)
                {
                    stableMovementDirection = Vector3.Slerp(stableMovementDirection, velDir, 0.15f);
                }
                lastNonZeroInputTime = Time.time; // ainda consideramos em movimento
            }
            else
            {
                // Está praticamente parado: após tempo de decaimento, trava direção em forward local
                if (Time.time - lastNonZeroInputTime > stableDirectionDecayTime)
                {
                    // Mantém a última direção; opcionalmente poderia alinhar ao forward atual
                    // stableMovementDirection = yemmaTransform.forward; // se quiser alinhar ao modelo
                }
            }
        }

        /// <summary>
        /// Aplica sistema de amortecimento do solo (chamado no FixedUpdate)
        /// </summary>
        public void ApplyGroundDamping()
        {
            if (movementProfile != null && movementProfile.enableGroundDamping)
            {
                Vector3 dampingForce = movementPhysics.CalculateGroundDampingForce();
                if (dampingForce != Vector3.zero)
                {
                    playerRigidbody.AddForce(dampingForce, ForceMode.Force);
                }
            }
        }

        /// <summary>
        /// Alinha o player ao terreno
        /// </summary>
        public void AlignToTerrain()
        {
            movementPhysics.AlignToTerrain();
        }

        /// <summary>
        /// Para o movimento do player
        /// </summary>
        public void StopMovement()
        {
            Vector3 velocity = playerRigidbody.linearVelocity;
            velocity.x = 0;
            velocity.z = 0;
            playerRigidbody.linearVelocity = velocity;
        }

        /// <summary>
        /// Retorna a magnitude da velocidade horizontal
        /// </summary>
        public float GetHorizontalSpeed()
        {
            Vector3 horizontalVelocity = new Vector3(Velocity.x, 0, Velocity.z);
            return horizontalVelocity.magnitude;
        }

        public void ChangeAnimation(YemmaAnimationController.YemmaAnimations newState, float blendTime = 0.2f)
        {
            this.animationController.ChangeState(newState, blendTime);
        }
        // Debug Gizmos
        private void OnDrawGizmosSelected()
        {
            if (yemmaTransform == null || movementProfile == null) return;

            if (movementProfile.IsDebugEnabled)
            {
                // Desenha raycast de detecção de chão
                Gizmos.color = IsGrounded() ? Color.green : movementProfile.debugRayColor;
                Vector3 rayStart = yemmaTransform.position + movementProfile.groundCheckOffset;
                Vector3 rayEnd = rayStart + Vector3.down * movementProfile.groundCheckDistance;
                
                Gizmos.DrawLine(rayStart, rayEnd);
                Gizmos.DrawWireSphere(rayEnd, 0.1f);

                // Desenha informações de velocidade
                if (Application.isPlaying)
                {
                    // Vetor de velocidade (amarelo claro / ciano)
                    Vector3 velocityVector = new Vector3(Velocity.x, 0, Velocity.z);
                    if (velocityVector.sqrMagnitude > 0.0001f)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawRay(yemmaTransform.position, velocityVector.normalized * 1.6f);
                    }

                    // Vetor estável usado para rotação (azul)
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(yemmaTransform.position, stableMovementDirection.normalized * 2.2f);
                }
            }

            // Debug do sistema de amortecimento
            if (movementProfile.showDampingDebug && Application.isPlaying && movementPhysics != null)
            {
                var dampingInfo = movementPhysics.GetDampingDebugInfo();
                
                if (dampingInfo.isGrounded)
                {
                    Vector3 playerPos = yemmaTransform.position;
                    
                    // Desenha linha da distância atual
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(playerPos, dampingInfo.groundPoint);
                    
                    // Desenha distância desejada
                    Vector3 desiredPos = dampingInfo.groundPoint + Vector3.up * dampingInfo.desiredDistance;
                    Gizmos.color = movementProfile.dampingDebugColor;
                    Gizmos.DrawWireSphere(desiredPos, 0.15f);
                    
                    // Desenha força aplicada
                    if (dampingInfo.currentForce.magnitude > 0.1f)
                    {
                        Gizmos.color = dampingInfo.currentForce.y > 0 ? Color.green : Color.red;
                        Gizmos.DrawRay(playerPos, dampingInfo.currentForce.normalized * 0.5f);
                    }
                    
                    // Desenha normal do terreno
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(dampingInfo.groundPoint, dampingInfo.groundNormal * 0.3f);
                }
            }
        }
    }
}
