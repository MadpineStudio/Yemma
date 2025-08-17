using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class SpringBone : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)] // 3 * 4 = 12bytes
    struct Bone
    {
        public Vector3 Pos;
    }
    
    [SerializeField] private ComputeShader csBones;
    [SerializeField] private int boneCount;
    [SerializeField] private float boneSize;
    [SerializeField] private Vector3 gravity;

    private ComputeBuffer _bonesBuffer;
    private Bone[] _bones;

    private int _kernelId;

    // Parâmetros da simulação
    [Header("Timestep Settings")]
    public float timeStep = 0.01f;
    public int iterationsPerFrame = 3;
    private float _accumulatedTime = 0f;
    private static readonly int DeltaTime = Shader.PropertyToID("_deltaTime");
    private static readonly int BoneCount = Shader.PropertyToID("_boneCount");
    private static readonly int BonesBuffer = Shader.PropertyToID("_bonesBuffer");

    private void Start()
    {
        Init();
    }

    void Init()
    {
        _bonesBuffer = new ComputeBuffer(boneCount, Marshal.SizeOf(typeof(Bone)));
        csBones.SetInt(BoneCount, boneCount);
        
        _kernelId = csBones.FindKernel("CSPhysics");
        csBones.GetKernelThreadGroupSizes(_kernelId, x: out var threadX, out var threadY, out _);
        Debug.Log(threadX);
        
        _bones = new Bone[boneCount];
        Debug.Log($"Size ->  {boneCount}");
        for(int i = 0; i < boneCount; i++)
        {
            Bone bone = new Bone
            {
                Pos = transform.position
            };
            _bones[i] = bone;
        }
        
        Debug.Log($"Bone array 0 ->  {_bones[0].Pos}");
        _bonesBuffer.SetData(_bones);
    }

    private void Update()
    {
        // Passo de tempo fixo para estabilidade
        _accumulatedTime += Time.deltaTime;
        
        // Executar várias iterações para manter a estabilidade
        while (_accumulatedTime >= timeStep)
        {
            _accumulatedTime -= timeStep;
            // Executar a simulação
            RunSimulationStep();
        }
    }

    void RunSimulationStep()
    {
        // Definir parâmetros para o frame atual
        csBones.SetFloat(DeltaTime, timeStep);
        csBones.SetBuffer(_kernelId, BonesBuffer, _bonesBuffer);
        
        // Dispatch do compute shader
        csBones.Dispatch(_kernelId, 1, 1, 1);
        _bonesBuffer.GetData(_bones);
        
        Debug.Log($"Bone array 0 ->  {_bones[0].Pos}");
    }
    private void OnDestroy()
    {
        if(_bonesBuffer != null)
            _bonesBuffer?.Release();
    }

    private void OnDrawGizmos()
    {
        if (_bones == null) return;
        
        foreach (var bone in _bones)
        {
            Gizmos.color = new Color(0, .2f, .4f, .75f);
            Gizmos.DrawSphere(bone.Pos, boneSize);
        }
    }
}
