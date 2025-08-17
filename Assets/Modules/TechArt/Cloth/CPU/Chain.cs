using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Chain
{
    public float boneLength = 0.5f;
    [Range(0.1f, 1f)] public float stiffness = 1f; // Valor fixo para corda inextensível
    public Vector3 gravity = new (0, -9.81f, 0);
    [Range(0.9f, 0.999f)] public float damping = 0.98f;
    public int boneCount = 10;
    public int solverIterations = 3;

    public float maxMovementPerStep = 1f;
    public float timeScale = 1f;

    [System.NonSerialized] public List<Bone> Bones = new();
    private Vector3 _lastRootPosition;
    private float _accumulator;

    public void Initialize(Vector3 rootPosition)
    {
        Bones.Clear();
        _lastRootPosition = rootPosition;
        Bones.Add(new Bone(rootPosition, true));
        for (int i = 1; i < boneCount; i++)
        {
            Vector3 offset = Vector3.down * i * boneLength;
            Bone bone = new Bone(rootPosition + offset, false, offset);
            bone.mass = Mathf.Lerp(1f, 5f, (float)i / boneCount);
            Bones.Add(bone);
        }
    }

    public void ResetSimulation(Vector3 rootPosition)
    {
        _lastRootPosition = rootPosition;
        for (int i = 0; i < Bones.Count; i++)
        {
            Bones[i].ResetPosition(rootPosition);
        }
    }

    public void Simulate(float deltaTime, Vector3 currentRootPosition)
    {
        _accumulator += Mathf.Min(deltaTime * timeScale, 0.1f);
        const float fixedDeltaTime = 0.0166666f;
        while (_accumulator >= fixedDeltaTime)
        {
            _accumulator -= fixedDeltaTime;
            FixedStepUpdate(currentRootPosition, fixedDeltaTime);
        }
    }

    private void FixedStepUpdate(Vector3 currentRootPosition, float fixedDeltaTime)
    {
        Vector3 rootMovement = currentRootPosition - _lastRootPosition;
        rootMovement = Vector3.ClampMagnitude(rootMovement, maxMovementPerStep);
        _lastRootPosition = currentRootPosition;

        Bones[0].position = currentRootPosition;
        Bones[0].previousPosition = currentRootPosition;

        for (int i = 1; i < Bones.Count; i++)
        {
            Bones[i].Update(fixedDeltaTime, gravity, damping);
        }
        for (int iteration = 0; iteration < solverIterations; iteration++)
        {
            for (int i = 1; i < Bones.Count; i++)
            {
                SolveConstraint(Bones[i - 1], Bones[i]);
            }
        }
    }

    private void SolveConstraint(Bone a, Bone b)
    {
        Vector3 delta = b.position - a.position;
        float distance = delta.magnitude;
        float diff = distance - boneLength;
        if (Mathf.Abs(diff) > 0.001f)
        {
            Vector3 correction = delta.normalized * diff;
            if (!a.isFixed && !b.isFixed)
            {
                a.position += 0.5f * correction;
                b.position -= 0.5f * correction;
            }
            else if (!a.isFixed)
            {
                a.position += correction;
            }
            else if (!b.isFixed)
            {
                b.position -= correction;
            }
        }
    }
}
