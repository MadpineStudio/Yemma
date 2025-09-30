using UnityEngine;

public class HandEdgeState : IState
{
    private YemmaBaseController controller;
    private bool wasKinematic; // Armazena estado anterior do Rigidbody
    private bool isAligning = false; // Flag para controlar alinhamento
    private Quaternion targetRotation; // Rotação alvo para interpolação
    private float hangTimer = 0f; // Timer para controlar tempo no hang edge

    public HandEdgeState(YemmaBaseController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        EventManager.onStateChange?.Invoke("HandEdge");
        
        // Interpola para animação Bake-HandEdge
        controller.GetAnimation().PlayAnimation(YemmaAnimationState.BakeHandEdge);
        
        // Desativa mola durante hand edge
        controller.GetPhysics().SetSpringEnabled(false);
        
        // Desativa rotação durante hand edge
        controller.GetPhysics().SetRotationEnabled(false);
        
        // Alinha rotação com a parede
        AlignToWall();
        
        // Posiciona player na posição exata para climb
        SnapToHangPosition();
        
        // Torna o Rigidbody kinematic para travar completamente
        Rigidbody rb = controller.GetPhysics().GetRigidbody();
        if (rb != null)
        {
            wasKinematic = rb.isKinematic; // Salva estado anterior
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero; // Zera velocidade
            rb.angularVelocity = Vector3.zero; // Zera rotação
        }
        
        // Reset timer
        hangTimer = 0f;
    }

    public void HandleInput()
    {
        YemmaInputManager inputManager = controller.GetInputManager();
        YemmaSettings settings = controller.GetSettings();
        
        // Input para escalar (pular) - só depois do tempo mínimo
        if (inputManager.jump && hangTimer >= (settings?.minHangTime ?? 0.5f))
        {
            controller.GetStateMachine().ChangeState(new ClimbState(controller));
            return;
        }
        
        // Input para soltar/descer do edge (apenas movimento para baixo)
        if (inputManager.movementVector.y < -0.5f)
        {
            controller.GetStateMachine().ChangeState(new FallState(controller));
            return;
        }
    }

    public void Update()
    {
        // Atualiza timer
        hangTimer += Time.deltaTime;
        
        // Aplica alinhamento suave à parede
        if (isAligning)
        {
            ApplySmoothAlignment();
        }
    }

    public void FixedUpdate()
    {
        // Mantém posição durante hand edge (sem movimento)
    }

    public void Exit()
    {
        // Restaura o estado anterior do Rigidbody
        Rigidbody rb = controller.GetPhysics().GetRigidbody();
        if (rb != null)
        {
            rb.isKinematic = wasKinematic; // Restaura estado anterior
        }
        
        // Reativa mola ao sair do estado
        controller.GetPhysics().SetSpringEnabled(true);
        
        // Reativa rotação ao sair do estado
        controller.GetPhysics().SetRotationEnabled(true);
    }

    private void AlignToWall()
    {
        // Obtém informações da parede detectada
        if (controller.GetClimbPhysics().GetWallHitInfo(out RaycastHit hitInfo, controller.GetSettings()))
        {
            // Usa a normal da parede para calcular a rotação oposta
            Vector3 wallNormal = hitInfo.normal;
            
            // A direção que o player deve encarar é oposta à normal da parede
            Vector3 targetDirection = -wallNormal;
            
            // Remove componente Y para manter rotação apenas no plano horizontal
            targetDirection.y = 0;
            targetDirection.Normalize();
            
            // Calcula rotação alvo
            if (targetDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
                isAligning = true; // Inicia processo de alinhamento suave
            }
        }
    }

    private void SnapToHangPosition()
    {
        // Obtém informações da parede para posicionamento exato
        YemmaSettings settings = controller.GetSettings();
        if (controller.GetClimbPhysics().GetWallHitInfo(out RaycastHit hitInfo, settings) && settings != null)
        {
            Vector3 wallNormal = hitInfo.normal;
            Vector3 wallPoint = hitInfo.point;
            
            // Usa configurações do settings para posicionamento
            Vector3 hangPosition = wallPoint + wallNormal * settings.hangDistance - Vector3.up * settings.hangDepth;
            
            // Aplica posição exata instantaneamente
            Transform playerTransform = controller.transform;
            if (playerTransform != null)
            {
                playerTransform.position = hangPosition;
            }
        }
    }

    private void ApplySmoothAlignment()
    {
        Transform playerTransform = controller.transform;
        YemmaSettings settings = controller.GetSettings();
        
        if (playerTransform != null && settings != null)
        {
            // Interpolação suave em direção à rotação alvo usando settings
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation, 
                targetRotation, 
                settings.alignmentSpeed * Time.deltaTime
            );
            
            // Para o alinhamento quando estiver próximo o suficiente
            if (Quaternion.Angle(playerTransform.rotation, targetRotation) < 1f)
            {
                playerTransform.rotation = targetRotation;
                isAligning = false;
            }
        }
    }
}