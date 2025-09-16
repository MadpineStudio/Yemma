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
        [SerializeField] private Transform modelo3D; // Referência ao modelo 3D filho
        [SerializeField] private float anguloMaximoDesalinhamento = 30f; // Ângulo máximo antes de perder alinhamento
        
        private bool playerNaArea = false;
        private Vector3 inputAtual;
        private Vector3 inputSuavizado;
        private Monolito monolitoDetectado;
        private bool estaAlinhando = false;
        private Yemma.YemmaController playerController;
        private bool modelo3DTravado = false; // Referência ao controller do player

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
            
            // Controla travamento visual do modelo 3D
            ControlarModelo3D();
        }
        
        private void ControlarModelo3D()
        {
            if (modelo3D == null || monolitoDetectado == null) 
            {
                modelo3DTravado = false;
                return;
            }
            
            // Se alinhado e sem input, trava modelo 3D no alvo
            if (!estaAlinhando && inputAtual.magnitude <= 0.01f && playerNaArea && playerController != null && playerController.IsInInteractionMode)
            {
                Vector3 direcaoAlvo = (monolitoDetectado.transform.position - transform.position).normalized;
                modelo3D.rotation = Quaternion.LookRotation(direcaoAlvo);
                modelo3DTravado = true;
            }
            // Se travado, verifica se pai rotacionou muito para destravar
            else if (modelo3DTravado)
            {
                Vector3 direcaoAlvo = (monolitoDetectado.transform.position - transform.position).normalized;
                Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoAlvo);
                float anguloDesalinhamento = Quaternion.Angle(transform.rotation, rotacaoAlvo);
                
                if (anguloDesalinhamento > anguloMaximoDesalinhamento)
                {
                    modelo3DTravado = false; // Destrava modelo 3D
                }
                else
                {
                    // Mantém travado no alvo
                    modelo3D.rotation = rotacaoAlvo;
                }
            }
            
            // Se não travado, modelo 3D segue rotação do pai
            if (!modelo3DTravado)
            {
                modelo3D.rotation = transform.rotation;
            }
        }

        private void DragRotation()
        {
            Vector3 novoInput = Vector3.zero;
            
            // Input por teclado
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
            
            // Input por joystick (stick esquerdo)
            float joystickX = Input.GetAxis("Horizontal");
            float joystickY = Input.GetAxis("Vertical");
            
            // Adiciona input do joystick ao input do teclado
            novoInput.y += joystickX;
            novoInput.x += -joystickY; // Inverte Y para ficar natural
            
            // Clamp para não passar de -1 a 1
            novoInput.x = Mathf.Clamp(novoInput.x, -1f, 1f);
            novoInput.y = Mathf.Clamp(novoInput.y, -1f, 1f);
            
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
