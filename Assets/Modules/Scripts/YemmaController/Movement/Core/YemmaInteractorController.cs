using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Yemma.Movement.Core
{
    /// <summary>
    /// Controlador de animações do Yemma.
    /// </summary>

    public class YemmaInteractorController : MonoBehaviour
    {

        private YemmaMovementController controller;
        private InputManager inputManager;

        public YemmaInteractorController(YemmaMovementController controller, InputManager inputManager)
        {
            this.controller = controller;
            this.inputManager = inputManager;
        }
        public void AssingController( InputManager inputManager) {
            this.inputManager = inputManager;
            
            // Se o GameObject está ativo, subscreve os eventos agora
            if (gameObject.activeInHierarchy && inputManager != null && inputManager.inputActions != null)
            {
                inputManager.inputActions.YemmaKeyboard.Interact.performed += Interact;
            }
        }
        [SerializeField] private Transform yemmaBody;
        [SerializeField] private LayerMask layerMask;
        Collider currentClosest = null;
        List<Collider> colliders = new();

        void OnEnable()
        {
            if (inputManager != null && inputManager.inputActions != null)
            {
                inputManager.inputActions.YemmaKeyboard.Interact.performed += Interact;
            }
        }
        void OnDisable()
        {
            if (inputManager != null && inputManager.inputActions != null)
            {
                inputManager.inputActions.YemmaKeyboard.Interact.performed -= Interact;
            }
        }
        void Update()
        {
            float oldDot = 0;

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

        }

        void Interact(InputAction.CallbackContext ctxt)
        {
            if (currentClosest != null) currentClosest.GetComponent<InteractableBehaviour>().ToggleActivation();
        }
       
    }
}
