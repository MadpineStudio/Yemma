using UnityEngine;
using UnityEngine.InputSystem;

public class InteractorTest : MonoBehaviour
{
    public InputManager inputManager;
    public Transform yemmaBody;
    public LayerMask layerMask;
    void Start()
    {
        inputManager.inputActions.YemmaKeyboard.Interact.performed += Interact;
    }
    void OnDisable()
    {
        inputManager.inputActions.YemmaKeyboard.Interact.performed -= Interact;
    }
    void Interact(InputAction.CallbackContext ctxt){
        if(Physics.Raycast(transform.position + Vector3.up * 0.5f, yemmaBody.forward, out RaycastHit hit, 3, layerMask)){
            if(hit.point != null){
                hit.collider.GetComponent<InteractableBehaviour>().ToggleActivation();
            }
        }
    }
}
