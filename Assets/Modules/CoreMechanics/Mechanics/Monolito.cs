using UnityEngine;

namespace CoreMechanics.Mechanics
{
    public class Monolito : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private float velocidadeRotacao = 90f;
        [SerializeField] private float suavidadeRotacao = 5f;
        [SerializeField] private float distanciaRaycast = 10f;
        [SerializeField] private LayerMask layersDeteccao = -1;
        [SerializeField] private Transform lightPathTransform;
        [SerializeField] private float velocidadeAlinhamento = 2f;
        
        private bool playerNaArea = false;
        private Vector3 inputAtual;
        private Vector3 inputSuavizado;
        private Monolito monolitoDetectado;
        private bool estaAlinhando = false;
        private Yemma.YemmaController playerController; // Referência ao controller do player

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerNaArea = true;
                playerController = other.GetComponent<Yemma.YemmaController>();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerNaArea = false;
                estaAlinhando = false;
                playerController = null;
            }
        }

        void Update()
        {
            if (playerNaArea && playerController != null && playerController.IsInInteractionMode)
            {
                DragRotation();
                VerificarAlinhamentoAutomatico();
            }
            else if (estaAlinhando)
            {
                ProcessarAlinhamentoAutomatico();
            }
            
            ExecutarRaycast();
            
            // Se alinhado e não há input, trava rotação como LookAt
            if (!estaAlinhando && monolitoDetectado != null && inputAtual.magnitude <= 0.01f && playerNaArea && playerController != null && playerController.IsInInteractionMode)
            {
                Vector3 direcaoAlvo = (monolitoDetectado.transform.position - transform.position).normalized;
                transform.rotation = Quaternion.LookRotation(direcaoAlvo);
            }
        }

        private void DragRotation()
        {
            Vector3 novoInput = Vector3.zero;
            
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                novoInput.y = -1f;
            }
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                novoInput.y = 1f;
            }
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                novoInput.x = -1f;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                novoInput.x = 1f;
            }
            
            inputAtual = novoInput;
            
            // Para o alinhamento automático se há input
            if (inputAtual.magnitude > 0.01f)
            {
                estaAlinhando = false;
            }
            
            // Suaviza o input para transições mais fluidas
            inputSuavizado = Vector3.Lerp(inputSuavizado, inputAtual, suavidadeRotacao * Time.deltaTime);
            
            // Se há input, rotaciona
            if (inputSuavizado.magnitude > 0.01f)
            {
                Vector3 rotacao = new Vector3(
                    inputSuavizado.x * velocidadeRotacao * Time.deltaTime,
                    inputSuavizado.y * velocidadeRotacao * Time.deltaTime,
                    0f
                );
                
                transform.Rotate(rotacao);
            }
        }
        
        private void VerificarAlinhamentoAutomatico()
        {
            // Se não há input de movimento e há monólito detectado, inicia alinhamento
            if (inputAtual.magnitude <= 0.01f && monolitoDetectado != null && !estaAlinhando)
            {
                IniciarAlinhamentoAutomatico();
            }
        }
        
        private void IniciarAlinhamentoAutomatico()
        {
            if (monolitoDetectado == null) return;
            
            estaAlinhando = true;
        }
        
        private void ProcessarAlinhamentoAutomatico()
        {
            if (monolitoDetectado == null)
            {
                estaAlinhando = false;
                return;
            }
            
            // Calcula a direção para o centro do monólito detectado
            Vector3 direcaoAlvo = (monolitoDetectado.transform.position - transform.position).normalized;
            Quaternion rotacaoAlinhamento = Quaternion.LookRotation(direcaoAlvo);
            
            // Rotaciona suavemente em direção ao alvo
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlinhamento, velocidadeAlinhamento * Time.deltaTime);
            
            // Verifica se terminou o alinhamento - para completamente quando alinhado
            float anguloRestante = Quaternion.Angle(transform.rotation, rotacaoAlinhamento);
            if (anguloRestante < 0.1f)
            {
                transform.rotation = rotacaoAlinhamento; // Força rotação exata
                estaAlinhando = false;
            }
        }
        
        private void ExecutarRaycast()
        {
            Vector3 direction = transform.forward;
            
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distanciaRaycast, layersDeteccao))
            {
                Monolito monolitoEncontrado = hit.collider.GetComponent<Monolito>();
                
                if (monolitoEncontrado != null && monolitoEncontrado != this)
                {
                    if (monolitoDetectado != monolitoEncontrado)
                    {
                        monolitoDetectado = monolitoEncontrado;
                    }
                }
                else
                {
                    monolitoDetectado = null;
                }
            }
            else
            {
                monolitoDetectado = null;
            }
        }
        
        public bool EstaAlinhando => estaAlinhando;
        public bool PlayerNaArea => playerNaArea;
        
        private void OnDrawGizmos()
        {
            // Desenha direção do raycast
            Gizmos.color = monolitoDetectado != null ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * distanciaRaycast);
            
            // Desenha esfera no centro do monólito
            Gizmos.color = playerNaArea ? Color.yellow : Color.blue;
            Gizmos.DrawWireSphere(transform.position, 1f);
            
            // Se está alinhando, desenha linha para o alvo
            if (estaAlinhando && monolitoDetectado != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, monolitoDetectado.transform.position);
            }
        }
    }
}
