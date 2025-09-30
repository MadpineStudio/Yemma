using UnityEngine;

public class RunState : IState
{
    private YemmaBaseController controller;

    public RunState(YemmaBaseController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        EventManager.onStateChange?.Invoke("Run");
        
        // Interpola para animação Bake-Run
        controller.GetAnimation().PlayAnimation(YemmaAnimationState.BakeRun);
    }

    public void HandleInput()
    {
        YemmaInputManager inputManager = controller.GetInputManager();
        Vector2 movementInput = inputManager.movementVector;

        // Verifica pulo primeiro (com coyote jump)
        if (inputManager.jump && (controller.IsGrounded() || controller.CanCoyoteJump()))
        {
            controller.GetStateMachine().ChangeState(new JumpState(controller));
            return;
        }

        // Transições para outros estados
        if (movementInput.magnitude == 0 || !controller.IsGrounded())
        {
            controller.GetStateMachine().ChangeState(new IdleState(controller));
        }
        else if (!inputManager.glide)
        {
            controller.GetStateMachine().ChangeState(new WalkState(controller));
        }
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        // Spring force é aplicado automaticamente pelo YemmaBaseController
        // Aplica rotação baseada na direção do movimento
        YemmaInputManager inputManager = controller.GetInputManager();
        YemmaSettings settings = controller.GetSettings();
        
   
    }

    public void Exit()
    {
    }
}