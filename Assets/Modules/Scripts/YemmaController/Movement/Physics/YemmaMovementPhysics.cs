using UnityEngine;
using Yemma.Movement.Data;
using Yemma.Movement.Core;

namespace Yemma
{
    /// <summary>
    /// Classe responsável por gerenciar todos os cálculos de física de movimentação do Yemma
    /// </summary>
    public class YemmaMovementPhysics
    {
        private readonly YemmaMovementController controller;
        private YemmaMovementProfile Profile => controller.MovementProfile;
        
        // Cache da última direção válida para evitar flicking
        private Vector3 lastValidMovementDirection = Vector3.forward;
        private float directionStabilityThreshold = 0.1f;

        public YemmaMovementPhysics(YemmaMovementController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Calcula movimento relativo à câmera
        /// </summary>
        public Vector3 CalculateCameraRelativeMovement(Vector2 inputDirection, Transform cameraTransform)
        {
            if (inputDirection.magnitude < 0.01f) return Vector3.zero;

            inputDirection = inputDirection.normalized;

            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Remove componente Y para movimento no plano horizontal
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 movement = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);

            // Ajusta movimento baseado na inclinação do terreno
            if (controller.IsGrounded())
            {
                movement = ProjectMovementOnSlope(movement);
            }

            return movement;
        }

        /// <summary>
        /// Calcula movimento do player com aceleração e desaceleração
        /// </summary>
        public Vector3 CalculatePlayerMovement(Vector2 inputDirection, Vector3 currentVelocity)
        {
            // Aplica curva de responsividade ao input
            Vector2 processedInput = new Vector2(
                Profile.ApplyInputCurve(inputDirection.x),
                Profile.ApplyInputCurve(inputDirection.y)
            );

            Vector3 cameraRelativeMovement = CalculateCameraRelativeMovement(processedInput, Camera.main.transform);
            Vector3 targetVelocity = cameraRelativeMovement * Profile.maxVelocity;
            
            Vector3 velocityDifference = targetVelocity - new Vector3(currentVelocity.x, 0, currentVelocity.z);
            
            // Determina se está acelerando ou desacelerando
            float accelRate = targetVelocity.magnitude > Profile.InputThreshold 
                ? Profile.acceleration 
                : Profile.deceleration;

            // Aplica força baseada na diferença de velocidade
            Vector3 force = new Vector3(
                CalculateAxisForce(velocityDifference.x, accelRate),
                0,
                CalculateAxisForce(velocityDifference.z, accelRate)
            );

            return force;
        }

        /// <summary>
        /// Projeta movimento na inclinação do terreno
        /// </summary>
        private Vector3 ProjectMovementOnSlope(Vector3 movement)
        {
            if (Physics.Raycast(controller.Transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 1.5f))
            {
                Vector3 slopeNormal = hit.normal;
                return Vector3.ProjectOnPlane(movement, slopeNormal).normalized;
            }

            return movement;
        }

        /// <summary>
        /// Calcula força para um eixo específico
        /// </summary>
        private float CalculateAxisForce(float speedDifference, float accelRate)
        {
            return Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, Profile.velocityPower) 
                   * Mathf.Sign(speedDifference);
        }

        /// <summary>
        /// Aplica rotação suave para o player
        /// </summary>
        public void ApplyRotation(Vector3 movementDirection)
        {
            if (movementDirection.magnitude < Profile.InputThreshold) return;

            movementDirection.y = 0;
            
            // Verifica se a direção atual é suficientemente diferente da última direção válida
            // para evitar flicking quando a velocidade é baixa
            Vector3 currentVelocity = controller.Velocity;
            currentVelocity.y = 0;
            
            // Se a velocidade atual é baixa, usa a velocidade real em vez do input para determinar direção
            if (currentVelocity.magnitude > directionStabilityThreshold)
            {
                // Usa a direção da velocidade atual se ela for significativa
                Vector3 velocityDirection = currentVelocity.normalized;
                
                // Só atualiza a direção se a mudança for significativa o suficiente
                if (Vector3.Angle(velocityDirection, lastValidMovementDirection) > 15f || 
                    Vector3.Dot(movementDirection.normalized, velocityDirection) > 0.7f)
                {
                    lastValidMovementDirection = movementDirection.normalized;
                }
            }
            else if (movementDirection.magnitude > Profile.InputThreshold * 2f) // Threshold maior para mudanças quando parado
            {
                // Só permite mudança de direção quando o input for significativo
                lastValidMovementDirection = movementDirection.normalized;
            }
            
            // Usa a última direção válida para a rotação
            Quaternion targetRotation = Quaternion.LookRotation(lastValidMovementDirection, Vector3.up);
            
            controller.Transform.rotation = Quaternion.Slerp(
                controller.Transform.rotation, 
                targetRotation, 
                Profile.rotationSpeed * Time.fixedDeltaTime
            );
        }

        /// <summary>
        /// Alinha player ao terreno
        /// </summary>
        public void AlignToTerrain()
        {
            Vector3 rayStart = controller.Transform.position + Profile.groundCheckOffset;
            
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Profile.groundCheckDistance * 2f, Profile.groundLayers))
            {
                Vector3 terrainNormal = hit.normal;
                
                // Verifica se a superfície é caminhável
                if (Profile.IsSurfaceWalkable(terrainNormal))
                {
                    Quaternion terrainRotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);
                    
                    // Mantém a rotação Y do player
                    Vector3 currentEuler = controller.Transform.eulerAngles;
                    Quaternion targetRotation = terrainRotation * Quaternion.Euler(0, currentEuler.y, 0);
                    
                    // Aplica rotação gradual para alinhar com o terreno
                    controller.Transform.rotation = Quaternion.Slerp(
                        controller.Transform.rotation,
                        targetRotation,
                        Profile.terrainAlignmentSpeed * Time.deltaTime
                    );
                }
            }
        }

        /// <summary>
        /// Aplica sistema de molas para manter distância do chão
        /// </summary>
        public void ApplySpringForce(Rigidbody rb)
        {
            if (Profile.springProfile == null || !Profile.springProfile.enableSpringForce) return;
            
            var spring = Profile.springProfile;
            Vector3 raycastOrigin = controller.Transform.position + spring.raycastOffset;
            
            if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, spring.raycastDistance, spring.groundMask))
            {
                float currentDistance = hit.distance;
                float displacement = currentDistance - spring.restLength;
                
                // F = -kx - cv (Lei de Hooke com amortecimento)
                float springForce = -spring.springConstant * displacement;
                float dampingForce = -spring.dampingConstant * rb.linearVelocity.y;
                float totalForce = springForce + dampingForce;
                
                rb.AddForce(Vector3.up * totalForce);
            }
        }

        /// <summary>
        /// Debug visual do sistema de molas (Gizmos)
        /// </summary>
        public void DrawSpringDebugGizmos()
        {
            if (Profile.springProfile == null || !Profile.springProfile.showSpringDebug) return;

            var spring = Profile.springProfile;
            Vector3 raycastOrigin = controller.Transform.position + spring.raycastOffset;
            
            // Linha do raycast
            Gizmos.color = Color.red;
            Gizmos.DrawRay(raycastOrigin, Vector3.down * spring.raycastDistance);
            
            // Posição do raycast offset
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(raycastOrigin, 0.05f);
            
            if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit, spring.raycastDistance, spring.groundMask))
            {
                // Ponto de contato no chão
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(hit.point, 0.1f);
                
                // Altura da posição de repouso
                Vector3 restPosition = hit.point + Vector3.up * spring.restLength;
                Gizmos.color = spring.enableSpringForce ? Color.green : Color.gray;
                Gizmos.DrawSphere(restPosition, 0.15f);
                Gizmos.DrawLine(hit.point, restPosition);
            }
        }
    }
}
