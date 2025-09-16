using UnityEngine;

namespace CoreMechanics
{
    [RequireComponent(typeof(LineRenderer))]
    public class LightShaftController : MonoBehaviour
    {
        [Header("Light Shaft Settings")]
        [SerializeField] private Material lightShaftMaterial;
        [SerializeField] private bool autoUpdatePositions = true;
        [SerializeField] private bool updateInRealTime = true;
        
        private LineRenderer lineRenderer;
        private Vector3 lastStartPos;
        private Vector3 lastEndPos;
        
        // Property IDs para otimização
        private static readonly int StartPointID = Shader.PropertyToID("_StartPoint");
        private static readonly int EndPointID = Shader.PropertyToID("_EndPoint");
        private static readonly int LineLengthID = Shader.PropertyToID("_LineLength");

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            // Se não há material definido, tenta usar o material atual do LineRenderer
            if (lightShaftMaterial == null)
            {
                lightShaftMaterial = lineRenderer.material;
            }
            
            // Aplica o material ao LineRenderer
            if (lightShaftMaterial != null)
            {
                lineRenderer.material = lightShaftMaterial;
            }
            
            // Atualiza posições iniciais
            if (autoUpdatePositions)
            {
                UpdateShaderPositions();
            }
        }

        void Update()
        {
            if (updateInRealTime && autoUpdatePositions)
            {
                // Verifica se as posições mudaram para otimizar
                Vector3 currentStart = GetStartPosition();
                Vector3 currentEnd = GetEndPosition();
                
                if (currentStart != lastStartPos || currentEnd != lastEndPos)
                {
                    UpdateShaderPositions();
                    lastStartPos = currentStart;
                    lastEndPos = currentEnd;
                }
            }
        }

        public void UpdateShaderPositions()
        {
            if (lightShaftMaterial == null || lineRenderer == null)
            {
                Debug.LogWarning("LightShaftController: Material ou LineRenderer não encontrado!");
                return;
            }

            Vector3 startPos = GetStartPosition();
            Vector3 endPos = GetEndPosition();
            float lineLength = Vector3.Distance(startPos, endPos);

            // Atualiza propriedades do shader
            lightShaftMaterial.SetVector(StartPointID, startPos);
            lightShaftMaterial.SetVector(EndPointID, endPos);
            lightShaftMaterial.SetFloat(LineLengthID, lineLength);
        }

        public Vector3 GetStartPosition()
        {
            if (lineRenderer != null && lineRenderer.positionCount > 0)
            {
                return lineRenderer.GetPosition(0);
            }
            return Vector3.zero;
        }

        public Vector3 GetEndPosition()
        {
            if (lineRenderer != null && lineRenderer.positionCount > 1)
            {
                return lineRenderer.GetPosition(1);
            }
            return Vector3.forward;
        }

        public void SetPositions(Vector3 start, Vector3 end)
        {
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, end);
                
                if (autoUpdatePositions)
                {
                    UpdateShaderPositions();
                }
            }
        }

        public void SetMaterial(Material newMaterial)
        {
            lightShaftMaterial = newMaterial;
            if (lineRenderer != null)
            {
                lineRenderer.material = lightShaftMaterial;
                UpdateShaderPositions();
            }
        }

        // Método público para forçar atualização
        public void ForceUpdate()
        {
            UpdateShaderPositions();
        }

        void OnValidate()
        {
            // Atualiza no editor quando propriedades mudarem
            if (Application.isPlaying && autoUpdatePositions)
            {
                UpdateShaderPositions();
            }
        }

        void OnDrawGizmosSelected()
        {
            // Visualiza as posições no editor
            if (lineRenderer != null && lineRenderer.positionCount >= 2)
            {
                Vector3 start = GetStartPosition();
                Vector3 end = GetEndPosition();
                
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(start, 0.1f);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(end, 0.1f);
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(start, end);
                
                // Mostra o centro
                Vector3 center = (start + end) * 0.5f;
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(center, 0.05f);
            }
        }
    }
}