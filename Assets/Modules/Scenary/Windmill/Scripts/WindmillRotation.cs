using System;
using UnityEngine;

public class WindmillRotation : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private float velocity;

    private Vector3 pivotRotation;
    private void Start()
    {
        pivotRotation = pivot.localRotation.eulerAngles;
    }

    private void FixedUpdate()
    {
        pivot.RotateAround(pivotRotation, Time.fixedDeltaTime * velocity);
    }
}
