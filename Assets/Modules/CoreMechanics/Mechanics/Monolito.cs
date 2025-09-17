using UnityEngine;
using UnityEngine.Events;

namespace CoreMechanics.Mechanics
{
    public class Monolito : MonoBehaviour
    {
        [Header("Crystal Light Path")]
        [SerializeField] private CrystalLightPathInterpolator crystalLightPath;
        
        [Header("Objetos Ativados")]
        [SerializeField] private GameObject[] objetosParaAtivar;
        
        [Header("Eventos")]
        public UnityEvent OnDetectado;
        
        [Header("Configurações")]
        [SerializeField] private float velocidadeRotacao = 90f;
        [SerializeField] private float suavidadeRotacao = 5f;
        [SerializeField] private float distanciaRaycast = 10f;
        [SerializeField] private LayerMask layersDeteccao = -1;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float velocidadeAlinhamento = 2f;
        [SerializeField] private Transform modelo3D; // Referência ao modelo 3D filho
        [SerializeField] private float anguloMaximoDesalinhamento = 30f; // Ângulo máximo antes de perder alinhamento
        [SerializeField] private float comprimentoLaserPadrao = 5f; // Comprimento do laser quando não detecta monólito
        
        [Header("Rotação Controlada")]
        [SerializeField] private float incrementoRotacao = 15f; // Graus por passo
        [SerializeField] private float tempoEntrePasso = 0.3f; // Tempo mínimo entre rotações
        [SerializeField] private float limitePitch = 60f; // Limite para rotação X (cima/baixo)
        [SerializeField] private float limiteYaw = 180f; // Limite para rotação Y (esquerda/direita)
        
        private bool playerNaArea = false;
        private Vector3 inputAtual;
        private Vector3 inputSuavizado;
        private Monolito monolitoDetectado;
        private bool estaAlinhando = false;
        private Yemma.YemmaController playerController;
        private bool modelo3DTravado = false; // Referência ao controller do player
        
        // Variáveis para rotação controlada
        private float ultimoTempoRotacao = 0f;
        private bool inputProcessado = false;
        private Vector3 angulosAtuais; // Para rastrear ângulos acumulados

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

        void Start()
        {
            // Inicializa os ângulos atuais com a rotação inicial
            angulosAtuais = transform.eulerAngles;
            
            // Busca o componente CrystalLightPathInterpolator se não foi atribuído
            if (crystalLightPath == null)
                crystalLightPath = GetComponent<CrystalLightPathInterpolator>();
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
            
            // Atualiza o line renderer
            AtualizarLineRenderer();
            
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
            // Verifica se passou tempo suficiente desde a última rotação
            if (Time.time - ultimoTempoRotacao < tempoEntrePasso)
                return;
            
            bool inputDetectado = false;
            Vector3 rotacaoDesejada = Vector3.zero;
            
            // Input por teclado - apenas uma direção por vez
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                rotacaoDesejada.y = -incrementoRotacao;
                inputDetectado = true;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                rotacaoDesejada.y = incrementoRotacao;
                inputDetectado = true;
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                rotacaoDesejada.x = -incrementoRotacao;
                inputDetectado = true;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                rotacaoDesejada.x = incrementoRotacao;
                inputDetectado = true;
            }
            
            // Input por joystick - apenas quando passa de um threshold
            float joystickX = Input.GetAxis("Horizontal");
            float joystickY = Input.GetAxis("Vertical");
            
            if (!inputProcessado)
            {
                if (Mathf.Abs(joystickX) > 0.7f)
                {
                    rotacaoDesejada.y = joystickX > 0 ? incrementoRotacao : -incrementoRotacao;
                    inputDetectado = true;
                    inputProcessado = true;
                }
                else if (Mathf.Abs(joystickY) > 0.7f)
                {
                    rotacaoDesejada.x = joystickY > 0 ? -incrementoRotacao : incrementoRotacao;
                    inputDetectado = true;
                    inputProcessado = true;
                }
            }
            
            // Reset flag quando joystick volta ao centro
            if (Mathf.Abs(joystickX) < 0.3f && Mathf.Abs(joystickY) < 0.3f)
            {
                inputProcessado = false;
            }
            
            // Se há input válido, aplica rotação
            if (inputDetectado)
            {
                // Calcula novos ângulos
                Vector3 novosAngulos = angulosAtuais + rotacaoDesejada;
                
                // Normaliza ângulos
                novosAngulos.x = NormalizarAngulo(novosAngulos.x);
                novosAngulos.y = NormalizarAngulo(novosAngulos.y);
                
                // Aplica limites
                novosAngulos.x = Mathf.Clamp(novosAngulos.x, -limitePitch, limitePitch);
                novosAngulos.y = Mathf.Clamp(novosAngulos.y, -limiteYaw, limiteYaw);
                
                // Atualiza ângulos atuais
                angulosAtuais = novosAngulos;
                
                // Aplica rotação suavemente
                StartCoroutine(RotacionarSuavemente(novosAngulos));
                
                // Para o alinhamento automático
                estaAlinhando = false;
                
                // Atualiza tempo da última rotação
                ultimoTempoRotacao = Time.time;
            }
        }
        
        private float NormalizarAngulo(float angulo)
        {
            while (angulo > 180f) angulo -= 360f;
            while (angulo < -180f) angulo += 360f;
            return angulo;
        }
        
        private System.Collections.IEnumerator RotacionarSuavemente(Vector3 angulosAlvo)
        {
            Quaternion rotacaoInicial = transform.rotation;
            Quaternion rotacaoAlvo = Quaternion.Euler(angulosAlvo);
            
            float tempo = 0f;
            float duracao = 0.2f; // Duração da rotação suave
            
            while (tempo < duracao)
            {
                tempo += Time.deltaTime;
                float t = tempo / duracao;
                
                // Curve para suavizar movimento
                t = Mathf.SmoothStep(0f, 1f, t);
                
                transform.rotation = Quaternion.Lerp(rotacaoInicial, rotacaoAlvo, t);
                yield return null;
            }
            
            transform.rotation = rotacaoAlvo;
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
                        monolitoDetectado.SerDetectado();
                        
                        // Dispara evento de detectado no Crystal Light Path
                        if (crystalLightPath != null)
                            crystalLightPath.Detectado();
                    }
                }
                else
                {
                    if (monolitoDetectado != null)
                    {
                        monolitoDetectado.SerDesdetectado();
                        
                        // Dispara evento de não detectado no Crystal Light Path
                        if (crystalLightPath != null)
                            crystalLightPath.NaoDetectado();
                    }
                    monolitoDetectado = null;
                }
            }
            else
            {
                if (monolitoDetectado != null)
                {
                    monolitoDetectado.SerDesdetectado();
                    
                    // Dispara evento de não detectado no Crystal Light Path
                    if (crystalLightPath != null)
                        crystalLightPath.NaoDetectado();
                }
                monolitoDetectado = null;
            }
        }
        
        private void AtualizarLineRenderer()
        {
            if (lineRenderer == null) return;
            
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            
            if (monolitoDetectado != null)
            {
                // Se detectou outro monólito, vai até ele
                lineRenderer.SetPosition(1, monolitoDetectado.transform.position);
            }
            else
            {
                // Se não detectou, vai na direção do raycast com comprimento padrão
                Vector3 endPosition = transform.position + transform.forward * comprimentoLaserPadrao;
                lineRenderer.SetPosition(1, endPosition);
            }
        }
        
        public bool EstaAlinhando => estaAlinhando;
        public bool PlayerNaArea => playerNaArea;
        
        private void AtivarObjetos()
        {
            foreach (GameObject obj in objetosParaAtivar)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        
        private void DesativarObjetos()
        {
            foreach (GameObject obj in objetosParaAtivar)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
        
        public void SerDetectado()
        {
            AtivarObjetos();
            OnDetectado?.Invoke();
        }
        
        public void SerDesdetectado()
        {
            DesativarObjetos();
        }
        
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
