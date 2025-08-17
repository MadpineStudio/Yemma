using _Modules.FinalMachine.Machines.PlayerMovement;
using _Modules.FinalMachine.Machines.PlayerMovement.States;
using UnityEngine;

namespace _Modules.Player.Script
{

    [RequireComponent(typeof(InputManager))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private bool enableStateDebugging;
        public bool exitedGlide;
        public Transform yemmaBody;
        public Rigidbody playerRb;
        public Animator playerAn;
        public PhysicalProfile physicalProfile;
        public PlayerInventory playerInventory;
        public InputManager inputManager;
        public Vector3 forward;
        public Vector3 Velocity() => playerRb.linearVelocity;
        [SerializeField] private bool grounded;
        int layerMask;
        public bool Grounded() => Physics.Raycast(transform.position + Vector3.up * .2f, Vector3.down, .4f, layerMask);

        public enum AnimationStates
        {
            Idle = 0,
            Walk,
            Jump,
            Roll
        }

        public PlayerMovementStateMachine PlayerMovementStateMachine;
        void OnEnable()
        {
            layerMask = physicalProfile.groundLayer | physicalProfile.interactableLayer;
        }
        private void Start()
        {
            PlayerMovementStateMachine = new PlayerMovementStateMachine(this);
        }

        private void Update()
        {
            grounded = Grounded();
            PlayerMovementStateMachine.enableStateDebugging = enableStateDebugging;
            PlayerMovementStateMachine.HandleInput();
            PlayerMovementStateMachine.Update();
            // WallDetected(yemmaBody, .5f, physicalProfile.groundLayer);
        }

        private void FixedUpdate()
        {
            PlayerMovementStateMachine.UpdatePhysics();
        }

        public void ChangeAnimations(AnimationStates animationState, float transitionTime = 0.15f, float normalizedTimeOffset = 0f)
        {
            if (playerAn == null)
            {
                Debug.LogWarning("Animator is not assigned!");
                return;
            }

            int stateHash = Animator.StringToHash(animationState.ToString());

            // CrossFade com:
            // - stateHash: O hash do estado de animação
            // - transitionTime: Duração da transição em segundos (0.25s padrão)
            // - layer: Camada do Animator (0 = base layer)
            // - normalizedTimeOffset: Reinicia a animação se 0, ou continua de onde parou se negativo
            playerAn.CrossFade(
                stateHash,
                transitionTime,
                0,              // Camada padrão (base layer)
                normalizedTimeOffset
            );
        }

        void OnDrawGizmos()
        {
            // Desenha o raycast na direção da superfície
            // Gizmos.DrawLine(yemmaBody.position, yemmaBody.position + (-yemmaBody.up * 2));
            // Desenha uma pequena esfera no ponto de impacto (se houver)
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * .2f, Vector3.down, out hit, .4f, layerMask))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hit.point, 0.01f); // Desenha uma esfera no ponto de colisão
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position + Vector3.up * .2f, transform.position +  (Vector3.up * .2f) + (Vector3.down * .4f));
            }
            Debug.DrawRay(Vector3.up * .5f + transform.position, forward * physicalProfile.climbWallCheckerLength, Color.green);


        }
    }
}