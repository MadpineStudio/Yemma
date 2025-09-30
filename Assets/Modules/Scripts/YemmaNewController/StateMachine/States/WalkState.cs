using UnityEngine;

public class WalkState : IState
{
    private YemmaBaseController controller;

    public WalkState(YemmaBaseController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        EventManager.onStateChange?.Invoke("Walk");
        
        // Interpola para animação Bake-Walk
        controller.GetAnimation().PlayAnimation(YemmaAnimationState.BakeWalk);
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
        
        if (inputManager.movementVector.magnitude == 0 || !controller.IsGrounded())
        {
            controller.GetStateMachine().ChangeState(new IdleState(controller));
        }
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        // Spring force é aplicado automaticamente pelo YemmaBaseController
        // Aplica rotação baseada na direção do movimento
       
    }

    public void Exit()
    {
    }
}