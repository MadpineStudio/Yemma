using UnityEngine;

public class IdleState : IState
{
    private YemmaBaseController controller;

    public IdleState(YemmaBaseController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        EventManager.onStateChange?.Invoke("Idle");
        
        // Interpola para animação Bake-Idle
        controller.GetAnimation().PlayAnimation(YemmaAnimationState.BakeIdle);
    }

    public void HandleInput()
    {
        YemmaInputManager inputManager = controller.GetInputManager();
        
        // Verifica pulo primeiro (com coyote jump)
        if (inputManager.jump && (controller.IsGrounded() || controller.CanCoyoteJump()))
        {
            controller.GetStateMachine().ChangeState(new JumpState(controller));
            return;
        }
        
        if (inputManager.movementVector.magnitude > 0 && controller.IsGrounded())
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
        // Aqui podemos adicionar modificações específicas do estado se necessário
    }

    public void Exit()
    {
    }
}