using UnityEngine;

[System.Serializable]
public class Bone
{
    public Vector3 position;
    public Vector3 previousPosition;
    public bool isFixed;
    public float mass = 1f;
    public Vector3 initialOffset;

    public Bone(Vector3 startPosition, bool fixedBone = false, Vector3 offset = default)
    {
        position = startPosition;
        previousPosition = startPosition;
        isFixed = fixedBone;
        initialOffset = offset;
    }

    public void ResetPosition(Vector3 rootPosition)
    {
        if (!isFixed)
        {
            position = rootPosition + initialOffset;
            previousPosition = position;
        }
        else
        {
            position = rootPosition;
            previousPosition = rootPosition;
        }
    }

    public void Update(float fixedDeltaTime, Vector3 force, float damping)
    {
        if (isFixed) return;
        Vector3 velocity = (position - previousPosition) * damping;
        previousPosition = position;
        position += velocity + (force / mass) * fixedDeltaTime * fixedDeltaTime;
    }
}