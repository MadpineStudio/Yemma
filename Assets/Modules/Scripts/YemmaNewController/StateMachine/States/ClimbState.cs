using UnityEngine;

public class ClimbState : IState
{
    private YemmaBaseController controller;
    private float climbDuration = 0.8f; // Duração da animação de escalada
    private float climbTimer = 0f;
    private bool wasKinematic; // Armazena estado anterior do Rigidbody
    private bool wasApplyRootMotion; // Armazena estado anterior do root motion
    private Vector3 targetPosition; // Posição alvo para target matching
    private bool hasCalculatedTarget = false;

    public ClimbState(YemmaBaseController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        EventManager.onStateChange?.Invoke("Climb");
        
        // Interpola para animação Bake-Climb
        controller.GetAnimation().PlayAnimation(YemmaAnimationState.BakeClimb);
        
        // Desativa mola durante climb
        controller.GetPhysics().SetSpringEnabled(false);
        
        // Desativa rotação durante climb
        controller.GetPhysics().SetRotationEnabled(false);
        
        // Ativa root motion no animator
        Animator animator = controller.GetComponent<Animator>();
        if (animator != null)
        {
            wasApplyRootMotion = animator.applyRootMotion;
            animator.applyRootMotion = true;
        }
        
        // Calcula posição alvo para target matching
        CalculateTargetPosition();
        
        // Torna o Rigidbody kinematic durante a escalada
        Rigidbody rb = controller.GetPhysics().GetRigidbody();
        if (rb != null)
        {
            wasKinematic = rb.isKinematic; // Salva estado anterior
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero; // Zera velocidade
            rb.angularVelocity = Vector3.zero; // Zera rotação
        }
        
        // Reset timer
        climbTimer = 0f;
    }

    public void HandleInput()
    {
        // Durante a escalada, não aceita inputs (animação locked)
    }

    public void Update()
    {
        // Atualiza timer da escalada
        climbTimer += Time.deltaTime;
        
        // Aplica target matching no final da animação
        if (climbTimer >= climbDuration - 0.1f && hasCalculatedTarget)
        {
            ApplyTargetMatching();
        }
        
        // Quando termina a escalada, volta para Idle
        if (climbTimer >= climbDuration)
        {
            controller.GetStateMachine().ChangeState(new IdleState(controller));
        }
    }

    public void FixedUpdate()
    {
        // Root motion se encarrega do movimento durante a escalada
        // Não aplicamos força manual
    }

    public void Exit()
    {
        EventManager.onStateChange?.Invoke("ExitClimb");
        
        // Restaura root motion anterior
        Animator animator = controller.GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = wasApplyRootMotion;
        }
        
        // Restaura o estado kinematic anterior
        Rigidbody rb = controller.GetPhysics().GetRigidbody();
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
        
        // Reativa rotação ao terminar de escalar
        controller.GetPhysics().SetRotationEnabled(true);
        
        // Reativa mola ao terminar de escalar
        controller.GetPhysics().SetSpringEnabled(true);
    }

    private void CalculateTargetPosition()
    {
        // Calcula posição final baseada na parede detectada
        if (controller.GetClimbPhysics().GetWallHitInfo(out RaycastHit hitInfo, controller.GetSettings()))
        {
            // Posição alvo é ligeiramente acima e atrás da superfície da parede
            Vector3 wallNormal = hitInfo.normal;
            Vector3 wallPoint = hitInfo.point;
            
            // Calcula posição final do climb (em cima da superfície)
            targetPosition = wallPoint + wallNormal * 0.5f + Vector3.up * 2f;
            hasCalculatedTarget = true;
        }
    }

    private void ApplyTargetMatching()
    {
        if (!hasCalculatedTarget) return;
        
        // Aplica target matching nos últimos frames da animação
        Transform playerTransform = controller.transform;
        if (playerTransform != null)
        {
            // Interpolação suave para a posição alvo
            float matchStrength = 5f * Time.deltaTime;
            playerTransform.position = Vector3.Lerp(
                playerTransform.position, 
                targetPosition, 
                matchStrength
            );
        }
    }
}