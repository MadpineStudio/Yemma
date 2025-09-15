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
        [Header("Physics Components")]
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private Transform yemmaTransform;
        [SerializeField] private Animator yemmaAnimator;
        [SerializeField] private InputManager inputManager;

        [Header("Colliders")]
        [SerializeField] private CapsuleCollider normalCollider;
        [SerializeField] private CapsuleCollider crouchCollider;
        
        [Header("Crouch Detection")]
        [SerializeField] private float crouchDetectionDistance;
        [SerializeField] private float crouchDetectionHeight;
        [SerializeField] private float standUpDetectionHeight;
        [SerializeField] private LayerMask crouchDetectionLayers;
        
        [Header("Head Sphere Detection")]
        [SerializeField] private Vector3 headSphereOffset = Vector3.zero;
        [SerializeField] private float headSphereRadius = 0.3f;
        
        [Header("Edge Detection")]
        [SerializeField] private float edgeDetectionDistance = 1.5f;
        [SerializeField] private float edgeDetectionHeight = 1.8f;
        [SerializeField] private float edgeHeightTolerance = 0.3f;
        [SerializeField] private LayerMask edgeDetectionLayers = -1;
        
        [Header("Crouch Debug")]
        [SerializeField] private bool showCrouchDebugGizmos = true;
        
        [Header("Edge Debug")]
        [SerializeField] private bool showEdgeDebugGizmos = true;
        
        // Estado do botão de crouch para controle de toggle
        private bool lastInteractButtonState = false;
        private bool crouchToggled = false;

        // Propriedades públicas
        public YemmaMovementProfile MovementProfile => movementProfile;
        public PhysicalProfile PhysicalProfile => null; // Backward compatibility - será removido gradualmente
        public Rigidbody Rigidbody => playerRigidbody;
        public Transform Transform => yemmaTransform;
        public Animator Animator => yemmaAnimator;
        public Vector3 Velocity => playerRigidbody.linearVelocity;
        public InputManager InputManager => inputManager;

        public bool pickItem = false;
        // Sistema de física de movimento
        private YemmaMovementPhysics movementPhysics;
        public YemmaAnimationController animationController;
        
        // Manager de profiles de animação
        private YemmaAnimationProfileManager profileManager;
        
        // Movement profile atual (gerenciado internamente pelo ProfileSet)
        private YemmaMovementProfile movementProfile;

        // Direção de movimento estável (evita flick quando input ~ 0)
        private Vector3 stableMovementDirection = Vector3.forward;
        // Tempo para decair a direção estável quando realmente para
        private float stableDirectionDecayTime = 0.35f;
        private float lastNonZeroInputTime;
        private float minInputThreshold => movementProfile != null ? movementProfile.InputThreshold : 0.01f;
        private float minSpeedThreshold = 0.05f; // velocidade horizontal mínima para atualizar direção pela velocidade

        private void Awake()
        {
            // Primeiro busca o manager de profiles
            profileManager = GetComponent<YemmaAnimationProfileManager>();
            
            InitializeComponents();
            movementPhysics = new YemmaMovementPhysics(this);
            animationController = new YemmaAnimationController(this);
            
            // Inicializa o manager de profiles se encontrado
            if (profileManager != null)
            {
                profileManager.Initialize(this);
            }
        }

        private void InitializeComponents()
        {
            if (playerRigidbody == null)
                playerRigidbody = GetComponent<Rigidbody>();

            if (yemmaTransform == null)
                yemmaTransform = transform;

            // O movementProfile será definido pelo ProfileManager
            // Cria um temporário apenas se não houver ProfileManager
            if (profileManager == null)
            {
                Debug.LogWarning("YemmaAnimationProfileManager não encontrado! Criando profile temporário.");
                movementProfile = CreateDefaultProfile();
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
        /// Configura o InputManager para o sistema de crouch
        /// </summary>
        public void SetInputManager(InputManager manager)
        {
            inputManager = manager;
        }

        /// <summary>
        /// Verifica se o botão de interação foi pressionado (toggle)
        /// </summary>
        public bool WasInteractButtonPressed()
        {
            if (inputManager == null) return false;
            
            bool currentButtonState = inputManager.inputActions.YemmaKeyboard.Interact.IsPressed();
            bool wasPressed = currentButtonState && !lastInteractButtonState;
            lastInteractButtonState = currentButtonState;
            
            if (wasPressed)
            {
                crouchToggled = !crouchToggled;
            }
            
            return wasPressed;
        }

        /// <summary>
        /// Retorna se o crouch está ativo via toggle
        /// </summary>
        public bool IsCrouchToggled()
        {
            return crouchToggled;
        }

        /// <summary>
        /// Força o estado do toggle de crouch
        /// </summary>
        public void SetCrouchToggle(bool active)
        {
            crouchToggled = active;
        }

        /// <summary>
        /// Detecta se há uma edge/quina que pode ser escalada
        /// </summary>
        public bool CanGrabEdge(out Vector3 edgePosition, out Vector3 edgeNormal)
        {
            edgePosition = Vector3.zero;
            edgeNormal = Vector3.zero;
            
            Vector3 forwardDirection = yemmaTransform.forward;
            Vector3 rayStart = yemmaTransform.position + Vector3.up * edgeDetectionHeight;
            
            // Raycast horizontal para detectar parede
            if (Physics.Raycast(rayStart, forwardDirection, out RaycastHit wallHit, edgeDetectionDistance, edgeDetectionLayers))
            {
                // Raycast para baixo a partir da parede para encontrar a edge
                Vector3 edgeSearchStart = wallHit.point + Vector3.up * edgeHeightTolerance;
                
                if (Physics.Raycast(edgeSearchStart, Vector3.down, out RaycastHit edgeHit, edgeHeightTolerance * 2f, edgeDetectionLayers))
                {
                    edgePosition = edgeHit.point;
                    edgeNormal = wallHit.normal;
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Verifica se está na posição correta para agarrar uma edge
        /// </summary>
        public bool IsInEdgeGrabRange()
        {
            return CanGrabEdge(out _, out _);
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
        /// Verifica se há obstáculo à frente que força agachamento E se o botão de ação foi pressionado
        /// </summary>
        public bool ShouldCrouch()
        {
            // Atualiza o estado do botão
            WasInteractButtonPressed();
            
            // Se não tem InputManager, não agacha
            if (inputManager == null) return false;
                
            // Só agacha se o toggle está ativo E há obstáculos
            return crouchToggled && HasCrouchObstacles();
        }

        /// <summary>
        /// Verifica se há obstáculos que justificam estar agachado
        /// </summary>
        public bool HasCrouchObstacles()
        {
            // Raycast para frente (detecta obstáculos baixos à frente)
            Vector3 backOffset = -yemmaTransform.forward * 0.3f;
            Vector3 forwardRayStart = yemmaTransform.position + Vector3.up * crouchDetectionHeight + backOffset;
            Vector3 forwardRayDirection = yemmaTransform.forward;
            float totalForwardDistance = crouchDetectionDistance + 0.3f;
            bool hasObstacleForward = Physics.Raycast(forwardRayStart, forwardRayDirection, totalForwardDistance, crouchDetectionLayers);
            
            // CheckSphere na altura da cabeça para detectar se está em espaço baixo
            Vector3 baseHeadPosition = yemmaTransform.position + Vector3.up * crouchDetectionHeight;
            Vector3 headPosition = baseHeadPosition + headSphereOffset;
            bool hasObstacleAtHead = Physics.CheckSphere(headPosition, headSphereRadius, crouchDetectionLayers);
            
            // Há obstáculo se há obstáculo à frente OU se a cabeça está tocando algo
            return hasObstacleForward || hasObstacleAtHead;
        }

        /// <summary>
        /// Verifica se pode sair do agachamento (sem obstáculo acima)
        /// </summary>
        public bool CanStandUp()
        {
            Vector3 rayStart = yemmaTransform.position + Vector3.up * 0.5f; // Altura agachado
            Vector3 rayDirection = Vector3.up;
            
            return !Physics.Raycast(rayStart, rayDirection, standUpDetectionHeight, crouchDetectionLayers);
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
            // Usa o profile manager se disponível
            if (profileManager != null)
            {
                profileManager.ChangeToState(newState);
            }
            else
            {
                // Fallback para o sistema antigo
                this.animationController.ChangeState(newState, blendTime);
            }
        }
        
        /// <summary>
        /// Define a velocidade da animação atual
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            if (yemmaAnimator != null)
            {
                yemmaAnimator.speed = speed;
            }
        }
        
        /// <summary>
        /// Alterna entre collider normal e collider de crouch
        /// </summary>
        public void SetCrouchCollider(bool useCrouchCollider)
        {
            if (normalCollider != null && crouchCollider != null)
            {
                normalCollider.enabled = !useCrouchCollider;
                crouchCollider.enabled = useCrouchCollider;
            }
        }
        
        /// <summary>
        /// Configura os parâmetros de detecção de crouch
        /// </summary>
        public void SetCrouchDetectionSettings(float detectionDistance, float detectionHeight, float standUpHeight, LayerMask layers)
        {
            crouchDetectionDistance = detectionDistance;
            crouchDetectionHeight = detectionHeight;
            standUpDetectionHeight = standUpHeight;
            crouchDetectionLayers = layers;
        }
        
        /// <summary>
        /// Troca o movement profile em runtime (usado pelo ProfileManager)
        /// </summary>
        public void SetMovementProfile(YemmaMovementProfile newProfile)
        {
            if (newProfile != null)
            {
                movementProfile = newProfile;
                movementProfile.ValidateProfile();
            }
            else
            {
                Debug.LogWarning("Tentativa de definir MovementProfile nulo!");
            }
        }
        
        /// <summary>
        /// Verifica se pode receber input baseado no profile atual
        /// </summary>
        public bool CanReceiveMovementInput()
        {
            if (profileManager != null)
            {
                return profileManager.CanReceiveInput();
            }
            return true;
        }
        
        /// <summary>
        /// Obtém o estado atual da animação
        /// </summary>
        public YemmaAnimationController.YemmaAnimations GetCurrentAnimationState()
        {
            if (profileManager != null)
            {
                return profileManager.GetCurrentState();
            }
            return YemmaAnimationController.YemmaAnimations.Idle;
        }
        // Debug Gizmos
        private void OnDrawGizmosSelected()
        {
            if (!showCrouchDebugGizmos) return;
            
            if (yemmaTransform == null || movementProfile == null) return;

            if (movementProfile.IsDebugEnabled)
            {
                // Desenha raycast de detecção de chão
                Gizmos.color = IsGrounded() ? Color.green : movementProfile.debugRayColor;
                Vector3 rayStart = yemmaTransform.position + movementProfile.groundCheckOffset;
                Vector3 rayEnd = rayStart + Vector3.down * movementProfile.groundCheckDistance;
                
                Gizmos.DrawLine(rayStart, rayEnd);
                Gizmos.DrawWireSphere(rayEnd, 0.1f);

                // Desenha raycasts de crouch
                if (Application.isPlaying)
                {
                    // Testa cada detecção individualmente para debug
                    Vector3 backOffset = -yemmaTransform.forward * 0.3f;
                    Vector3 crouchRayStart = yemmaTransform.position + Vector3.up * crouchDetectionHeight + backOffset;
                    Vector3 crouchRayEnd = crouchRayStart + yemmaTransform.forward * (crouchDetectionDistance + 0.3f);
                    bool forwardHit = Physics.Raycast(crouchRayStart, yemmaTransform.forward, crouchDetectionDistance + 0.3f, crouchDetectionLayers);
                    
                    Vector3 headPosition = yemmaTransform.position + Vector3.up * crouchDetectionHeight + headSphereOffset;
                    bool sphereHit = Physics.CheckSphere(headPosition, headSphereRadius, crouchDetectionLayers);
                    
                    // Raycast para detectar obstáculo à frente (ShouldCrouch - Forward)
                    Gizmos.color = forwardHit ? Color.red : Color.yellow;
                    Gizmos.DrawLine(crouchRayStart, crouchRayEnd);
                    Gizmos.DrawWireCube(crouchRayEnd, Vector3.one * 0.1f);
                    
                    // CheckSphere na altura da cabeça (ShouldCrouch - Head Detection)
                    Gizmos.color = sphereHit ? Color.red : Color.cyan;
                    Gizmos.DrawWireSphere(headPosition, headSphereRadius);

                    // Raycast para detectar se pode levantar (CanStandUp)
                    Vector3 standRayStart = yemmaTransform.position + Vector3.up * 0.5f;
                    Vector3 standRayEnd = standRayStart + Vector3.up * standUpDetectionHeight;
                    Gizmos.color = CanStandUp() ? Color.green : Color.red;
                    Gizmos.DrawLine(standRayStart, standRayEnd);
                    Gizmos.DrawWireCube(standRayEnd, Vector3.one * 0.1f);
                }

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
            
            // Debug do sistema de edge detection
            if (showEdgeDebugGizmos && Application.isPlaying)
            {
                Vector3 forwardDirection = yemmaTransform.forward;
                Vector3 rayStart = yemmaTransform.position + Vector3.up * edgeDetectionHeight;
                
                // Raycast horizontal para detectar parede
                bool wallHit = Physics.Raycast(rayStart, forwardDirection, out RaycastHit wallHitInfo, edgeDetectionDistance, edgeDetectionLayers);
                
                // Desenha raycast horizontal
                Gizmos.color = wallHit ? Color.magenta : Color.gray;
                Gizmos.DrawLine(rayStart, rayStart + forwardDirection * edgeDetectionDistance);
                Gizmos.DrawWireSphere(rayStart, 0.05f);
                
                if (wallHit)
                {
                    // Desenha ponto de impacto na parede
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireSphere(wallHitInfo.point, 0.1f);
                    
                    // Raycast para baixo a partir da parede para encontrar a edge
                    Vector3 edgeSearchStart = wallHitInfo.point + Vector3.up * edgeHeightTolerance;
                    bool edgeFound = Physics.Raycast(edgeSearchStart, Vector3.down, out RaycastHit edgeHitInfo, edgeHeightTolerance * 2f, edgeDetectionLayers);
                    
                    // Desenha busca pela edge
                    Gizmos.color = edgeFound ? Color.green : Color.red;
                    Gizmos.DrawLine(edgeSearchStart, edgeSearchStart + Vector3.down * (edgeHeightTolerance * 2f));
                    
                    if (edgeFound)
                    {
                        // Desenha a edge encontrada
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(edgeHitInfo.point, 0.15f);
                        
                        // Desenha a posição onde o player ficaria pendurado
                        Vector3 hangPosition = edgeHitInfo.point - wallHitInfo.normal * 0.3f;
                        hangPosition.y = edgeHitInfo.point.y - 1.2f;
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(hangPosition, Vector3.one * 0.2f);
                        
                        // Desenha linha conectando edge e posição de hang
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(edgeHitInfo.point, hangPosition);
                    }
                }
            }
        }
    }
}
