using UnityEngine;

[RequireComponent(typeof(Rigidbody))]  // Ou CharacterController, dependendo do movimento
public class GrassInteraction : MonoBehaviour {
    [Header("Grass Displacement Settings")]
    public float effectRadius = 3.0f;       // Raio de influência do jogador
    public float effectIntensity = 0.5f;    // Força do displacement
    public float decayRate = 0.3f;          // Quão rápido o efeito desaparece

    [Header("Debug")]
    public bool debugMask = false;          // Mostrar a textura de interação em tempo real
    public Material debugMaterial;          // Material para visualização (opcional)
    public Material grassMaterial;          // Material para visualização (opcional)

    public ComputeShader _grassMaskShader;
    private RenderTexture _grassMask;
    private Rigidbody _rb;
    private Vector3 _lastPosition;

    void Start() {
        _rb = GetComponent<Rigidbody>();

        // Cria a RenderTexture para o buffer de interação
        _grassMask = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGBFloat);
        _grassMask.enableRandomWrite = true;
        _grassMask.Create();

        // // Passa para o material da grama (opcional)
        // Shader.SetGlobalTexture("_InputMask", _grassMask);
    }

    void Update() {
        if (!_grassMaskShader || !_rb) return;

        // Calcula a velocidade (se usar Rigidbody)
        Vector3 velocity = _rb.linearVelocity;

        // Se não usar física, calcula manualmente:
        // Vector3 velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        // Define os parâmetros no Compute Shader
        int kernel = _grassMaskShader.FindKernel("UpdateMask");
        _grassMaskShader.SetTexture(kernel, "_GrassMaskBuffer", _grassMask);
        _grassMaskShader.SetFloat("_EffectRadius", effectRadius);
        _grassMaskShader.SetFloat("_EffectIntensity", effectIntensity);
        _grassMaskShader.SetFloat("_DecayRate", decayRate);
        _grassMaskShader.SetFloat("_TimeDelta", Time.deltaTime);
        _grassMaskShader.SetVector("_PlayerPosition", transform.position);
        _grassMaskShader.SetVector("_PlayerVelocity", velocity);

        // Executa o Compute Shader (1024x1024 / 8x8 = 128x128 thread groups)
        _grassMaskShader.Dispatch(kernel, 1024 / 8, 1024 / 8, 1);

        // Debug: Mostra a textura em um quad (opcional)
        if (debugMask && debugMaterial) {
            debugMaterial.SetTexture("_InputMask", _grassMask);
            grassMaterial.SetTexture("_InputMask",_grassMask);
        }
    }

    void OnDestroy() {
        if (_grassMask != null) _grassMask.Release();
    }
}