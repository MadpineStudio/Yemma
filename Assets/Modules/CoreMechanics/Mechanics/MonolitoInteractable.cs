using UnityEngine;

namespace CoreMechanics.Mechanics
{
    // Permite que o player interaja com o monólito usando o sistema de eventos
    public class MonolitoInteractable : MonoBehaviour, Yemma.IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private string interactionPrompt = "Pressione E para interagir com o Monólito";
        [SerializeField] private bool canInteract = true;
        [SerializeField] private bool alignPlayerToObject = true; // Toggle para alinhar player
        [SerializeField] private float alignmentSpeed = 5f; // Velocidade do alinhamento
        
        [Header("Monolito Reference")]
        [SerializeField] private Monolito monolito;
        
        [Header("Debug")]
        [SerializeField] private bool debugInteraction = true;
        
        public bool CanInteract => canInteract && monolito != null;
        public float InteractionDistance => interactionDistance;
        public string InteractionPrompt => interactionPrompt;

        private void Awake()
        {
            if (monolito == null)
            {
                monolito = GetComponent<Monolito>();
            }
            
            if (monolito == null)
            {
                Debug.LogError($"MonolitoInteractable em {gameObject.name} não encontrou componente Monolito!");
            }
        }

        public void OnInteract(Yemma.YemmaController player)
        {
            if (debugInteraction)
            {
                Debug.Log($"MonolitoInteractable: Player interagindo com {gameObject.name}");
            }
            
            // Se já está em modo de interação, sai do modo (permite sair sempre)
            if (player.IsInInteractionMode)
            {
                ExitInteraction(player);
                return;
            }
            
            // Só verifica CanInteract para ENTRAR no modo
            if (!CanInteract) return;
            
            // Zera velocidades do player
            var movementController = player.MovementController;
            if (movementController != null)
            {
                movementController.StopMovement();
            }
            
            // Alinha player para olhar para o objeto se habilitado
            if (alignPlayerToObject)
            {
                Vector3 directionToObject = (transform.position - player.transform.position).normalized;
                directionToObject.y = 0; // Mantém apenas rotação Y
                
                if (directionToObject != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToObject);
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, alignmentSpeed * Time.deltaTime);
                }
            }
            
            // Ativa o modo de interação do player
            player.EnterInteractionMode();
            
            if (monolito != null)
            {
                if (debugInteraction)
                {
                    Debug.Log($"MonolitoInteractable: Monólito {gameObject.name} configurado para interação");
                }
            }
        }
        
        // Método para sair do modo de interação
        public void ExitInteraction(Yemma.YemmaController player)
        {
            if (player != null)
            {
                player.ExitInteractionMode();
                
                if (debugInteraction)
                {
                    Debug.Log($"MonolitoInteractable: Player saiu da interação com {gameObject.name}");
                }
            }
        }
        
        public void SetCanInteract(bool canInteract)
        {
            this.canInteract = canInteract;
        }
        
        public void SetInteractionDistance(float distance)
        {
            interactionDistance = distance;
        }
        
        public void SetInteractionPrompt(string prompt)
        {
            interactionPrompt = prompt;
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = CanInteract ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
            
            if (CanInteract)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
            }
        }
    }
}