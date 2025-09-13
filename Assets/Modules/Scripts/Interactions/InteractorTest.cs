using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class InteractorTest : MonoBehaviour
{
    public InputManager inputManager;
    public Transform yemmaBody;
    public LayerMask layerMask;
    Collider currentClosest = null;

    List<Collider> colliders = new();

    void OnEnable()
    {
        inputManager.inputActions.YemmaKeyboard.Interact.performed += Interact;
    }
    void OnDisable()
    {
        inputManager.inputActions.YemmaKeyboard.Interact.performed -= Interact;
    }
    void Update()
    {
        float oldDot = 0;

        Debug.Log("A");
        colliders = Physics.OverlapSphere(yemmaBody.position + Vector3.up * 1.33f + yemmaBody.forward, 1.5f, layerMask).ToList();
        if (colliders.Count == 0) currentClosest = null;
        colliders.ForEach(collider =>
        {
            if (currentClosest == null) { currentClosest = collider; }

            Vector3 dir = collider.transform.position - yemmaBody.position;
            dir.y = 0;
            float dot = Vector3.Dot(new Vector3(yemmaBody.forward.x, 0, yemmaBody.forward.z), dir.normalized);
            if (dot > oldDot)
            {
                oldDot = dot;
                currentClosest = collider;
            }

        });
        if (currentClosest != null) Debug.Log(currentClosest.gameObject.name);

    }
    
    void Interact(InputAction.CallbackContext ctxt)
    {
        if (currentClosest != null) currentClosest.GetComponent<InteractableBehaviour>().ToggleActivation();
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(yemmaBody.position + Vector3.up * 1.33f + yemmaBody.forward, 1.5f);
        float tiniest = 0;
        colliders.ForEach(collider =>
        {
            Vector3 dir = collider.transform.position - yemmaBody.position;
            dir.y = 0;
            float dot = Vector3.Dot(new Vector3(yemmaBody.forward.x, 0, yemmaBody.forward.z), dir.normalized);
            Gizmos.color = Color.aliceBlue;
            Gizmos.DrawLine(yemmaBody.position, collider.transform.position);
            if (dot < tiniest)
            {
                tiniest = dot;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(yemmaBody.position, collider.transform.position);
            }


        });
    }
}
