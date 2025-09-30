using UnityEngine;

public class FallState : IState
{
    private YemmaBaseController controller;

    public FallState(YemmaBaseController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        EventManager.onStateChange?.Invoke("Fall");
        
        // Interpola para animação Bake-Fall
        controller.GetAnimation().PlayAnimation(YemmaAnimationState.BakeFall);
        
        // Reativa mola durante queda
        controller.GetPhysics().SetSpringEnabled(true);
    }

    public void HandleInput()
    {
        YemmaInputManager inputManager = controller.GetInputManager();
        
        // Verifica se aterrissou
        if (controller.IsGrounded())
        {
            // Se tem movimento, vai para walk, senão idle
            if (inputManager.movementVector.magnitude > 0)
            {
                controller.GetStateMachine().ChangeState(new WalkState(controller));
            }
            else
            {
                controller.GetStateMachine().ChangeState(new IdleState(controller));
            }
        }
    }

    public void Update()
    {
    }

    public void FixedUpdate()
    {
        // Permite movimento no ar durante queda (similar ao JumpState)
    }

    public void Exit()
    {
    }
}
