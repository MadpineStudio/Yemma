using UnityEngine;
using UnityEngine.InputSystem;
using _Modules.FinalMachine.Machines.PlayerMovement.Events;

    public class InputManager : MonoBehaviour
    {
        public Yemma_Input_Actions inputActions;
        public Vector2 movementVector;
        public Vector2 movementVectorWithoutReset;
        public Vector2 currentDirection;
        public bool glide = false;
        public bool jump;

        void Awake()
        {
            inputActions = new Yemma_Input_Actions();
        }

        void OnEnable()
        {
            inputActions.Enable();

            inputActions.YemmaKeyboard.Movement.started += ReadMovement;
            inputActions.YemmaKeyboard.Movement.performed += ReadMovement;
            inputActions.YemmaKeyboard.Movement.canceled += ResetMovement;
            inputActions.YemmaKeyboard.Glide.performed += ActivateGliderTrigger;
            inputActions.YemmaKeyboard.Glide.canceled += DeactivateGliderTrigger;

            // inputActions.YemmaKeyboard.Jump.performed += Jump;
        }

        void OnDisable()
        {
            inputActions.YemmaKeyboard.Movement.started -= ReadMovement;
            inputActions.YemmaKeyboard.Movement.performed -= ReadMovement;
            inputActions.YemmaKeyboard.Movement.canceled -= ResetMovement;
            inputActions.YemmaKeyboard.Glide.performed -= ActivateGliderTrigger;
            inputActions.YemmaKeyboard.Glide.canceled -= DeactivateGliderTrigger;

            // inputActions.YemmaKeyboard.Jump.performed -= Jump;
            inputActions.YemmaKeyboard.Jump.Dispose();

            inputActions.Disable();
        }

        private void ReadMovement(InputAction.CallbackContext cntx)
        {
            Vector2 input = cntx.ReadValue<Vector2>();

            if (input != Vector2.zero)
            {
                currentDirection = input;
            }

            movementVector = input;
        }

        private void ResetMovement(InputAction.CallbackContext cntx)
        {
            movementVector = Vector2.zero;
        }

        private void ActivateGliderTrigger(InputAction.CallbackContext cntx)
        {
            glide = true;
        }
        private void DeactivateGliderTrigger(InputAction.CallbackContext cntx)
        {
            glide = false;
        }
        // private void Jump(InputAction.CallbackContext cntx)
        // {
        //     MovementMachineEventManager.RaiseStateChangeJump();
        // }
    }