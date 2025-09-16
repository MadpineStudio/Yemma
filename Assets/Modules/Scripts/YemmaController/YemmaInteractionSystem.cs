using UnityEngine;
using UnityEngine.Events;

namespace Yemma
{
    // Sistema simples de interação - apenas gerencia eventos
    public class YemmaInteractionSystem : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionLayers = -1;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        
        [Header("Events")]
        public UnityEvent<IInteractable> OnInteraction;
        
        private YemmaController yemmaController;
        private IInteractable currentInteractable;
        
        public IInteractable CurrentInteractable => currentInteractable;
        public bool HasInteractable => currentInteractable != null;

        private void Awake()
        {
            yemmaController = GetComponent<YemmaController>();
        }

        private void Update()
        {
            DetectInteractable();
            HandleInput();
        }

        private void DetectInteractable()
        {
            var colliders = Physics.OverlapSphere(transform.position, interactionDistance, interactionLayers);
            
            IInteractable closest = null;
            float closestDist = float.MaxValue;
            
            foreach (var col in colliders)
            {
                var interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = interactable;
                    }
                }
            }
            
            currentInteractable = closest;
        }
        
        private void HandleInput()
        {
            // Permite tecla E mesmo em modo de interação (para sair do modo)
            if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
            {
                OnInteraction?.Invoke(currentInteractable);
                currentInteractable.OnInteract(yemmaController);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}