using UnityEngine;

[ExecuteAlways]
public class ChainManager : MonoBehaviour
{
    public Chain chain = new();
    public bool drawGizmos = true;
    public Color fixedColor = Color.red;
    public Color dynamicColor = Color.cyan;
    public float gizmoSize = 0.1f;

    private void OnEnable()
    {
        InitializeChain();
    }

    private void InitializeChain()
    {
        var position = transform.position;
        chain.Initialize(position);
        chain.ResetSimulation(position);
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            chain.ResetSimulation(transform.position);
        }
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            chain.Simulate(Time.fixedDeltaTime, transform.position);
        }
    }
    private void OnDrawGizmos()
    {
        if (!drawGizmos || chain.Bones == null || chain.Bones.Count == 0) return;
        Gizmos.color = fixedColor;
        Gizmos.DrawWireSphere(chain.Bones[0].position, gizmoSize * 1.5f);
        Gizmos.color = dynamicColor;
        for (int i = 1; i < chain.Bones.Count; i++)
        {
            Gizmos.DrawSphere(chain.Bones[i].position, gizmoSize);
            Gizmos.DrawLine(chain.Bones[i - 1].position, chain.Bones[i].position);
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