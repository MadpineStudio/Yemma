using UnityEngine;

public class JumpState : IState
{
    private YemmaBaseController controller;
    private float jumpCooldown = 0.2f; // Tempo para evitar detecção imediata do chão
    private float jumpTimer;
    private bool hasReachedPeak = false; // Verifica se já chegou no pico do pulo

    public JumpState(YemmaBaseController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        EventManager.onStateChange?.Invoke("Jump");
        
        // Interpola para animação Bake-Jump
        controller.GetAnimation().PlayAnimation(YemmaAnimationState.BakeJump);
        
        // Desativa mola no primeiro frame do pulo
        controller.GetPhysics().SetSpringEnabled(false);
        
        // Inicia timer de cooldown e reset do pico
        jumpTimer = 0f;
        hasReachedPeak = false;
        
        // Aplica força de pulo
        YemmaSettings settings = controller.GetSettings();
        if (settings != null)
        {
            Vector3 jumpForce = Vector3.up * settings.jumpForce;
            Vector3 velocity = controller.GetPhysics().GetVelocity();   
            velocity.y = 0;
            controller.GetPhysics().SetVelocity(velocity);
            controller.GetPhysics().ApplyForce(jumpForce, ForceMode.Impulse);
        }
    }

    public void HandleInput()
    {
        // Atualiza timer
        jumpTimer += Time.deltaTime;
        
        // Verifica se já chegou no pico (velocidade Y negativa)
        Vector3 velocity = controller.GetPhysics().GetVelocity();
        if (velocity.y < 0)
        {
            hasReachedPeak = true;
        }
        
        // Verifica se pode fazer climb durante o pulo (só na descida)
        ClimbPhysics climbPhysics = controller.GetClimbPhysics();
        YemmaSettings settings = controller.GetSettings();
        if (climbPhysics != null && settings != null && velocity.y < 0)
        {
            if (climbPhysics.CanClimb(controller.IsGrounded(), settings))
            {
                controller.GetStateMachine().ChangeState(new HandEdgeState(controller));
                return;
            }
        }
        
        // Só verifica aterrissagem após cooldown E se já passou do pico E se realmente está no chão
        if (jumpTimer >= jumpCooldown && hasReachedPeak && controller.IsGrounded())
        {
            // Reativa mola quando toca o chão
            controller.GetPhysics().SetSpringEnabled(true);
            YemmaInputManager inputManager = controller.GetInputManager();
            controller.GetStateMachine().ChangeState(new IdleState(controller));
        }
    }

    public void Update()
    {
    }
    public void FixedUpdate()
    {   
        // Spring force é controlado pelo estado (desativado durante pulo)
        // Permite rotação durante o pulo
        YemmaInputManager inputManager = controller.GetInputManager();
        YemmaSettings settings = controller.GetSettings();
        
      
    }
    public void Exit()
    {
    }
}