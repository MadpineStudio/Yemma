using System;
using UnityEngine;

public class Climb : MonoBehaviour
{
    [SerializeField] private Transform climbEmissor;
    [SerializeField] private float climbEmissorLength;
    [SerializeField] private float climbEmissorHitLength;
    [SerializeField] private float minLengthToClimb;
    [SerializeField] private LayerMask groundLayer;
    private void LateUpdate()
    {
        ClimbLedge();
    }

    public void ClimbLedge()
    {
            
    }

    private void OnDrawGizmos()
    {
        if(!climbEmissor) return;
        
        // climb forward ray
        Gizmos.color = Color.green;
        var position = climbEmissor.position;
        var forward = climbEmissor.forward;
       
        Vector3 origin = position + forward *climbEmissorLength;
        Gizmos.DrawRay(position, forward * climbEmissorLength);
        
        //climb hit world up ray
        Physics.queriesHitBackfaces = true;
        RaycastHit hit;
        if (Physics.Raycast(origin, Vector3.up, out hit, Mathf.Infinity, groundLayer))
        {
            if (hit.point.y - origin.y < minLengthToClimb)
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawWireSphere(hit.point, .1f);
            Gizmos.DrawRay(position + forward * climbEmissorLength, Vector3.up * climbEmissorHitLength);
        }
    }
}
