using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    /// <summary>
    /// Estado onde o player fica pendurado na edge aguardando input
    /// VERSÃO SUPER SIMPLIFICADA - apenas congela o player no lugar
    /// Pressione ESPAÇO para fazer climb
    /// </summary>
    public class YemmaEdgeHangState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;
        private bool isAligned = false;
        private float rotationSpeed = 15f; // Velocidade de rotação para alinhamento

        public YemmaEdgeHangState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine) 
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
        }

        public override void Enter()
        {
            base.Enter();
            
            controller.ChangeAnimation(YemmaAnimationController.YemmaAnimations.EdgeHang);
            
            // Para completamente o movimento e física
            controller.StopMovement();
            controller.Rigidbody.isKinematic = true; // Desativa física completamente
            
            // Reseta flag de alinhamento
            isAligned = false;
        }

        public override void Exit()
        {
            // Reativa física quando sair
            controller.Rigidbody.isKinematic = false;
            base.Exit();
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            // Alinha a rotação do player para ficar de frente para a superfície
            if (!isAligned)
            {
                AlignToWallSurface();
            }
            
            // Verifica se o jogador pressionou o botão de pulo para fazer climb
            if (inputManager != null && GetJumpInput())
            {
                stateMachine.ChangeState(stateMachine.EdgeClimbState);
                return;
            }
        }

        public override void UpdatePhysics()
        {
            // SUPER SIMPLES - não faz nada na física
            // Player está com isKinematic = true, então não se move
        }

        /// <summary>
        /// Alinha o player para ficar de frente para a superfície da edge
        /// </summary>
        private void AlignToWallSurface()
        {
            // Tenta detectar a edge novamente para obter o normal da superfície
            if (controller.CanGrabEdge(out Vector3 edgePosition, out Vector3 edgeNormal))
            {
                // Calcula a rotação alvo baseada no normal da parede
                // O normal aponta para fora da parede, então invertemos para ficar de frente
                Vector3 targetDirection = -edgeNormal;
                targetDirection.y = 0; // Remove componente vertical para manter o player em pé
                
                if (targetDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                    
                    // Interpola suavemente para a rotação alvo
                    controller.Transform.rotation = Quaternion.Slerp(
                        controller.Transform.rotation, 
                        targetRotation, 
                        rotationSpeed * Time.deltaTime
                    );
                    
                    // Verifica se o alinhamento está próximo o suficiente
                    float angleDifference = Quaternion.Angle(controller.Transform.rotation, targetRotation);
                    if (angleDifference < 5f) // Tolerância de 5 graus
                    {
                        isAligned = true;
                        controller.Transform.rotation = targetRotation; // Força rotação exata
                    }
                }
            }
            else
            {
                // Se não conseguir detectar edge, marca como alinhado para evitar loop infinito
                isAligned = true;
            }
        }
    }
}