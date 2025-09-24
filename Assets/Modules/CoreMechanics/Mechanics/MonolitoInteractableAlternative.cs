using UnityEngine;
using Yemma;

namespace CoreMechanics.Mechanics
{
    public class MonolitoInteractableAlternative : InteractableBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private bool canInteract = true;
        [SerializeField] private bool alignPlayerToObject = true; // Toggle para alinhar player
        private float lastRotationTime = 0f;
        private Vector3 currentAngles;

        [Header("Debug")]
        [SerializeField] private bool debugInteraction = true;

        public bool CanInteract => canInteract;


        [Header("Crystal Light Path")]
        [SerializeField] private CrystalLightPathInterpolator crystalLightPath;

        [Header("Objetos Ativados")]
        [SerializeField] private GameObject[] objectsToActivate;


        [Header("Configurações")]

        [SerializeField] private float distanciaRaycast = 10f;
        [SerializeField] private LayerMask layersDeteccao = -1;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float comprimentoLaserPadrao = 5f; // Comprimento do laser quando não detecta monólito

        [Header("Rotação Controlada")]
        [SerializeField] private float rotationAmount = 45f; // Graus por passo
        [SerializeField] private float rotationCooldown = 0.3f; // Tempo mínimo entre rotações
        [SerializeField] private float pitchLimit = 60f; // Limite para rotação X (cima/baixo)
        [SerializeField] private float yawLimit = 180f; // Limite para rotação Y (esquerda/direita)

        private bool playerNaArea = false;
        private MonolitoInteractableAlternative lastDetectedMonolith;

        private void Awake() { }
        private void Start()
        {
            currentAngles = transform.eulerAngles;
        }
        public override void Update()
        {
            UpdateLineRenderer();
        }

        public override void ToggleActivation()
        {
            if (!CanInteract) return;
            if (lastDetectedMonolith != null)
            {
                lastDetectedMonolith.OnUndetected();
                if (crystalLightPath != null) crystalLightPath.NaoDetectado();

                lastDetectedMonolith = null;
            }
            DragRotation();

        }
        private void ExecuteRaycast()
        {
            Vector3 direction = transform.forward;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distanciaRaycast, layersDeteccao))
            {
                MonolitoInteractableAlternative foundMonolith = hit.collider.GetComponent<MonolitoInteractableAlternative>();
                

                if (foundMonolith != null && foundMonolith != this)
                {
                    Debug.Log(foundMonolith.name);
                    if (lastDetectedMonolith != foundMonolith)
                    {
                        lastDetectedMonolith = foundMonolith;
                        lastDetectedMonolith.OnDetected();

                        // Dispara evento de detectado no Crystal Light Path
                        if (crystalLightPath != null)
                        {
                            crystalLightPath.Detectado();
                        }
                    }
                }

            }
            else
            {
                if (lastDetectedMonolith != null)
                {
                    lastDetectedMonolith.OnUndetected();

                    // Dispara evento de não detectado no Crystal Light Path
                    if (crystalLightPath != null)
                        crystalLightPath.NaoDetectado();
                }
                lastDetectedMonolith = null;
            }
        }

        private void UpdateLineRenderer()
        {
            if (lineRenderer == null) return;

            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);

            if (lastDetectedMonolith != null)
            {
                // Se detectou outro monólito, vai até ele
                lineRenderer.SetPosition(1, lastDetectedMonolith.transform.position);
            }
            else
            {
                // Se não detectou, vai na direção do raycast com comprimento padrão
                Vector3 endPosition = transform.position + transform.forward * comprimentoLaserPadrao;
                lineRenderer.SetPosition(1, endPosition);
            }
        }


        private void ActivateObjects(bool setActive)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(setActive);
                }
            }
        }

        public void OnDetected()
        {
            ActivateObjects(true);
        }

        public void OnUndetected()
        {
            ActivateObjects(false);

        }
        private void DragRotation()
        {
            // Verifica se passou tempo suficiente desde a última rotação
            if (Time.time - lastRotationTime < rotationCooldown)
                return;

            Vector3 desiredRotation = Vector3.zero;


            desiredRotation.y = rotationAmount;

            // Se há input válido, aplica rotação
            // Calcula novos ângulos
            Vector3 newAngles = currentAngles + desiredRotation;

            // Normaliza ângulos
            newAngles.x = NormalizeAngles(newAngles.x);
            newAngles.y = NormalizeAngles(newAngles.y);

            // Aplica limites
            newAngles.x = Mathf.Clamp(newAngles.x, -pitchLimit, pitchLimit);
            newAngles.y = Mathf.Clamp(newAngles.y, -yawLimit, yawLimit);

            // Atualiza ângulos atuais
            currentAngles = newAngles;

            // Aplica rotação suavemente
            StartCoroutine(SmoothRotateMonolith(newAngles));


            // Atualiza tempo da última rotação
            lastRotationTime = Time.time;
        }

        private float NormalizeAngles(float angulo)
        {
            while (angulo > 180f) angulo -= 360f;
            while (angulo < -180f) angulo += 360f;
            return angulo;
        }

        private System.Collections.IEnumerator SmoothRotateMonolith(Vector3 angulosAlvo)
        {
            Quaternion initialRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(angulosAlvo);

            float time = 0f;
            float duration = 0.2f; // Duração da rotação suave

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                // Curve para suavizar movimento
                t = Mathf.SmoothStep(0f, 1f, t);

                transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t);
                yield return null;
            }

            transform.rotation = targetRotation;
            ExecuteRaycast();
        }
        private void OnDrawGizmos()
        {
            // Desenha direção do raycast
            Gizmos.color = lastDetectedMonolith != null ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * distanciaRaycast);

            // Desenha esfera no centro do monólito
            Gizmos.color = playerNaArea ? Color.yellow : Color.blue;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // Se está alinhando, desenha linha para o alvo
            if (lastDetectedMonolith != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, lastDetectedMonolith.transform.position);
            }
        }
    }
}