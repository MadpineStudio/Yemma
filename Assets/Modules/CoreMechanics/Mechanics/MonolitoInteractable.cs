using UnityEngine;
using Yemma;

namespace CoreMechanics.Mechanics
{
    public class MonolitoInteractable : InteractableBehaviour
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

        public override void ToggleActivation()
        {
            if (debugInteraction)
            {
                Debug.Log($"MonolitoInteractable: Player interagindo com {gameObject.name}");
            }

            // Só verifica CanInteract para ENTRAR no modo
            if (!CanInteract) return;

            YemmaController.OnPlayerInteractWithMonolith?.Invoke();

            if (monolito != null)
            {
                if (debugInteraction)
                {
                    Debug.Log($"MonolitoInteractable: Monólito {gameObject.name} configurado para interação");
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