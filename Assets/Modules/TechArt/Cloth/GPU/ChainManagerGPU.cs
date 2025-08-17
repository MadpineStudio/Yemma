using UnityEngine;

[ExecuteAlways]
public class ChainManagerGPU : MonoBehaviour
{
    [Header("Configurações da Corrente")]
    public float boneLength = 0.5f;
    [Range(0.01f, 5f)] public float stiffness = 1.5f;
    [Range(0.1f, 2f)] public float stiffnessFalloff = 1f;
    public Vector3 gravity = new Vector3(0, -9.81f, 0);
    [Range(0.9f, 0.999f)] public float damping = 0.995f;
    [Range(1, 100)] public int boneCount = 10;
    [Range(1, 10)] public int solverIterations = 5;

    [Header("Qualidade Física")]
    [Range(0.1f, 5f)] public float massMultiplier = 1f;
    [Range(0f, 2f)] public float inertia = 0.7f;
    public float maxMovementPerStep = 0.5f;
    public float timeScale = 1f;
    public float equilibriumThreshold = 0.001f;

    [Header("Visualização")]
    public bool drawGizmos = true;
    public Color fixedColor = Color.red;
    public Color dynamicColor = Color.cyan;
    public float gizmoSize = 0.1f;

    public ComputeShader chainComputeShader;
    
    private ComputeBuffer bonesBuffer;
    private BoneGPU[] bonesArray;
    private Vector3 lastRootPosition;
    private float accumulator;
    private int updatePhysicsKernel;
    private int solveConstraintsKernel;
    private bool isInEquilibrium;

    private struct BoneGPU
    {
        public Vector3 position;        // 12 bytes
        public Vector3 previousPosition;// 12 bytes
        public Vector3 velocity;        // 12 bytes
        public Vector3 initialOffset;   // 12 bytes
        public float mass;              // 4 bytes
        public int isFixed;            // 4 bytes
        // Total: 56 bytes
    }


    private void OnEnable()
    {
        InitializeChain();
        updatePhysicsKernel = chainComputeShader.FindKernel("UpdatePhysics");
        solveConstraintsKernel = chainComputeShader.FindKernel("SolveConstraints");
    }

    private void InitializeChain()
    {
        lastRootPosition = transform.position;
        bonesArray = new BoneGPU[boneCount];
        
        // Osso fixo (raiz)
        bonesArray[0] = new BoneGPU
        {
            position = lastRootPosition,
            previousPosition = lastRootPosition,
            velocity = Vector3.zero,
            initialOffset = Vector3.zero,
            mass = 1f * massMultiplier,
            isFixed = 1
        };
        
        // Ossos dinâmicos
        for (int i = 1; i < boneCount; i++)
        {
            Vector3 offset = Vector3.down * i * boneLength;
            bonesArray[i] = new BoneGPU
            {
                position = lastRootPosition + offset,
                previousPosition = lastRootPosition + offset,
                velocity = Vector3.zero,
                initialOffset = offset,
                mass = Mathf.Lerp(1f, 3f, (float)i / boneCount) * massMultiplier,
                isFixed = 0
            };
        }
        
        CreateComputeBuffer();
    }

    private void CreateComputeBuffer()
    {
        if (bonesBuffer != null) bonesBuffer.Release();
        
        // Tamanho explícito correspondente à estrutura BoneGPU
        bonesBuffer = new ComputeBuffer(Mathf.Max(1, boneCount), 56);
        bonesBuffer.SetData(bonesArray);
    }
    private void Update()
    {
        if (!Application.isPlaying)
        {
            ResetSimulation(transform.position);
        }
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            accumulator += Mathf.Min(Time.fixedDeltaTime * timeScale, 0.1f);
            const float fixedDeltaTime = 0.0166666f;
            while (accumulator >= fixedDeltaTime)
            {
                accumulator -= fixedDeltaTime;
                FixedStepUpdate(transform.position, fixedDeltaTime);
            }
        }
    }

    private void FixedStepUpdate(Vector3 currentRootPosition, float fixedDeltaTime)
    {
        Vector3 rootMovement = currentRootPosition - lastRootPosition;
        rootMovement = Vector3.ClampMagnitude(rootMovement, maxMovementPerStep);
        lastRootPosition = currentRootPosition;

        // Atualiza osso raiz
        bonesArray[0].position = currentRootPosition;
        bonesArray[0].previousPosition = currentRootPosition;
        bonesBuffer.SetData(bonesArray, 0, 0, 1);

        // Configura parâmetros da física
        chainComputeShader.SetBuffer(updatePhysicsKernel, "Bones", bonesBuffer);
        chainComputeShader.SetInt("boneCount", boneCount);
        chainComputeShader.SetFloat("damping", damping);
        chainComputeShader.SetFloat("fixedDeltaTime", fixedDeltaTime);
        chainComputeShader.SetVector("gravity", gravity);
        chainComputeShader.SetFloat("inertia", inertia);
        chainComputeShader.SetFloat("equilibriumThreshold", equilibriumThreshold);

        // Executa simulação física
        chainComputeShader.Dispatch(updatePhysicsKernel, Mathf.CeilToInt(boneCount / 64f), 1, 1);

        // Configura restrições
        chainComputeShader.SetBuffer(solveConstraintsKernel, "Bones", bonesBuffer);
        chainComputeShader.SetFloat("boneLength", boneLength);
        chainComputeShader.SetFloat("stiffness", stiffness);
        chainComputeShader.SetFloat("stiffnessFalloff", stiffnessFalloff);
        
        // Executa solver de restrições
        for (int i = 0; i < solverIterations; i++)
        {
            chainComputeShader.Dispatch(solveConstraintsKernel, Mathf.CeilToInt((boneCount - 1) / 64f), 1, 1);
        }

        // Verificação de equilíbrio e visualização
        if (Application.isPlaying || drawGizmos)
        {
            bonesBuffer.GetData(bonesArray);
            if (Application.isPlaying)
            {
                isInEquilibrium = CheckEquilibrium();
            }
        }
    }

    private bool CheckEquilibrium()
    {
        float totalMovement = 0f;
        for (int i = 1; i < bonesArray.Length; i++)
        {
            totalMovement += bonesArray[i].velocity.magnitude;
            if (totalMovement > equilibriumThreshold * bonesArray.Length)
                return false;
        }
        return true;
    }

    private void ResetSimulation(Vector3 rootPosition)
    {
        lastRootPosition = rootPosition;
        for (int i = 0; i < boneCount; i++)
        {
            if (bonesArray[i].isFixed == 1)
            {
                bonesArray[i].position = rootPosition;
                bonesArray[i].previousPosition = rootPosition;
                bonesArray[i].velocity = Vector3.zero;
            }
            else
            {
                bonesArray[i].position = rootPosition + bonesArray[i].initialOffset;
                bonesArray[i].previousPosition = rootPosition + bonesArray[i].initialOffset;
                bonesArray[i].velocity = Vector3.zero;
            }
        }
        bonesBuffer.SetData(bonesArray);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || bonesArray == null || bonesArray.Length == 0) return;
        
        Gizmos.color = fixedColor;
        Gizmos.DrawWireSphere(bonesArray[0].position, gizmoSize * 1.5f);
        
        Gizmos.color = dynamicColor;
        for (int i = 1; i < bonesArray.Length; i++)
        {
            Gizmos.DrawSphere(bonesArray[i].position, gizmoSize);
            Gizmos.DrawLine(bonesArray[i - 1].position, bonesArray[i].position);
        }
    }

    private void OnDisable()
    {
        if (bonesBuffer != null)
        {
            bonesBuffer.Release();
            bonesBuffer = null;
        }
    }

    private void OnValidate()
    {
        if (isActiveAndEnabled)
        {
            InitializeChain();
        }
    }
}