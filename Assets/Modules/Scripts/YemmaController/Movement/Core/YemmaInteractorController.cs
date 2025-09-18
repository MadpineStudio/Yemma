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
        public delegate GameObject InteractableDelegate();
        public static InteractableDelegate onGetPickedItem;



        [SerializeField] private Transform yemmaHands;
        private GameObject currentPickedItem = null;
        private YemmaMovementController controller;
        private InputManager inputManager;

        public YemmaInteractorController(YemmaMovementController controller, InputManager inputManager)
        {
            this.controller = controller;
            this.inputManager = inputManager;
        }
        public void AssingController(YemmaMovementController controller, InputManager inputManager)
        {
            this.controller = controller;
            this.inputManager = inputManager;

        }
        [SerializeField] private Transform yemmaBody;
        [SerializeField] private LayerMask layerMask;
        Collider currentClosest = null;
        List<Collider> colliders = new();

        void Start()
        {
            // if (inputManager != null && inputManager.inputActions != null)
            inputManager.inputActions.YemmaKeyboard.Interact.performed += ActivateInteraction;

        }
        void OnEnable()
        {
            onGetPickedItem += GetPickedItem;
        }
        void OnDisable()
        {
            // if (inputManager != null && inputManager.inputActions != null)
            inputManager.inputActions.YemmaKeyboard.Interact.performed -= ActivateInteraction;
            onGetPickedItem -= GetPickedItem;
        }
        void Update()
        {
            float oldDot = 0;

            colliders = Physics.OverlapSphere(yemmaBody.position + Vector3.up * 1.33f + yemmaBody.forward, 1.5f, layerMask).ToList();

            if (colliders.Count == 0) currentClosest = null;
            colliders.ForEach(collider =>
            {
                if (currentClosest == null && !collider.CompareTag("PickupPlaceFull")) { currentClosest = collider; }


                Vector3 dir = collider.transform.position - yemmaBody.position;
                dir.y = 0;
                float dot = Vector3.Dot(new Vector3(yemmaBody.forward.x, 0, yemmaBody.forward.z), dir.normalized);
                if (dot > oldDot)
                {
                    if (!collider.CompareTag("PickupPlaceFull"))
                    {
                        oldDot = dot;
                        currentClosest = collider;
                    }
                }
                if (currentClosest != null) Debug.Log(currentClosest.name);

            });



        }

        public void Interact()
        {

            if (currentPickedItem != null && currentClosest == null)
            {
                PickItem(false);
                currentPickedItem = null;
            }
            else
            {
                InteractableBehaviour itemInteracted;
                Debug.Log("Interagiu");
                if (currentClosest != null)
                {
                    itemInteracted = currentClosest.GetComponent<InteractableBehaviour>();
                    
                    if (itemInteracted.isPickable && currentPickedItem == null)
                    {
                        currentPickedItem = itemInteracted.gameObject;
                        PickItem(true);
                    }
                    else
                    {
                        itemInteracted.ToggleActivation();
                    }
                }
            }
        }
        public bool HasPickableClosestItem()
        {
            if (currentClosest == null) return false;
            return currentClosest.GetComponent<InteractableBehaviour>().isPickable;
        }
        public bool CanDropItem()
        {
            return currentPickedItem != null && ((currentClosest == null) || (currentClosest != null && !currentClosest.GetComponent<InteractableBehaviour>().isPickable));
        }
        private void PickItem(bool pick)
        {
            Vector3 pivotScale = yemmaHands.localScale;
            currentPickedItem.GetComponent<Rigidbody>().isKinematic = pick;
            currentPickedItem.GetComponent<BoxCollider>().enabled = !pick;
            currentPickedItem.transform.parent = pick ? yemmaHands : null;
            currentPickedItem.transform.localScale = Vector3.one;
            currentPickedItem.transform.position = yemmaHands.transform.position;
        }
        private GameObject GetPickedItem()
        {

            GameObject item = currentPickedItem;
            currentPickedItem = null;
            return item;
        }
        public void ActivateInteraction(InputAction.CallbackContext ctxt)
        {
        }

    }
}
