using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yemma.Movement.Core;

namespace Yemma.Movement.StateMachine.States
{
    public class YemmaDashState : YemmaMovementStateBase
    {
        private readonly YemmaMovementStateMachine stateMachine;
        private Transform dashTarget;
        private float dashSpeed;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private Vector3 controlPoint;
        private float dashProgress = 0f;
        private float dashDelay = 0f;
        private bool dashCompleted = false;
        private bool dash = false;
        private LayerMask layerMask;
        private List<Collider> colliders;
        private GameObject currentClosest = null;
        private GameObject lastJumpedClosest = null;
        private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public YemmaDashState(YemmaMovementController controller, InputManager inputManager, YemmaMovementStateMachine stateMachine, Transform target, float speed, Vector3 controlPoint, AnimationCurve curve)
            : base(controller, inputManager)
        {
            this.stateMachine = stateMachine;
            this.dashTarget = target;
            this.dashSpeed = speed;
            this.dashCurve = curve;

            // Use the exact control point from LightDashManager
            startPosition = controller.transform.position;
            endPosition = target.position;
            this.controlPoint = controlPoint;
        }

        public override void Enter()
        {
            base.Enter();
            layerMask = controller.jumpPadsLayerMask;
            startPosition = Vector3.zero;
            Jump();
            // dashProgress = 0f;
            // dashCompleted = false;

            // // Align player instantly to dash direction (only Y rotation)
            // Vector3 direction = (endPosition - startPosition).normalized;
            // direction.y = 0; // Remove vertical component
            // direction = direction.normalized;

            // Vector3 currentEuler = controller.transform.eulerAngles;
            // Quaternion targetRotation = Quaternion.LookRotation(direction);
            // Vector3 targetEuler = targetRotation.eulerAngles;

            // // Keep X and Z rotation, only change Y
            // controller.transform.rotation = Quaternion.Euler(currentEuler.x, targetEuler.y, currentEuler.z);

            // // Disable physics during dash
        }
        public void Jump()
        {
            dashCompleted = false;
            dashProgress = 0;
            controller.Rigidbody.linearVelocity = Vector3.zero;
            controller.Rigidbody.AddForce(Vector3.up * 170, ForceMode.Impulse);
        }
        public override void HandleInput()
        {
            base.HandleInput();
            Debug.Log(GetJumpInput());
            if ((GetJumpInput() && currentClosest != null && dashDelay >= 0.5f) || dash)
            {
                if (!dash) startPosition = controller.transform.position;
                dash = true;
                lastJumpedClosest = currentClosest;
                DashTowardsJumpPad();
            }
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            dashDelay += Time.deltaTime;
            // NOVA LÓGICA: Verifica se pode agarrar uma edge durante a queda
            if (controller.IsInEdgeGrabRange() && controller.Velocity.y <= 0)
            {
                TransitionToEdgeHang();
                return;
            }

            if (!dash) GetClosestJumpPad();

            if (dashCompleted || IsGrounded()) ExitDash();

            // // Update progress based on speed
            // float totalDistance = Vector3.Distance(startPosition, endPosition);
            // float progressIncrement = (dashSpeed * Time.deltaTime) / totalDistance;
            // dashProgress += progressIncrement;

            // if (dashProgress >= 1f)
            // {
            //     dashProgress = 1f;
            //     dashCompleted = true;
            // }

            // // Apply curve to progress and calculate position on bezier curve
            // float curvedProgress = dashCurve.Evaluate(dashProgress);
            // Vector3 currentPos = CalculateQuadraticBezier(startPosition, controlPoint, endPosition, curvedProgress);
            // Yemmabody.position = currentPos;

            // if (dashCompleted)
            // {
            //     ExitDash();
            // }
        }
        public override void UpdatePhysics()
        {
            base.UpdatePhysics();

            // Permite movimento horizontal limitado no ar
            Vector2 movementInput = GetMovementInput();
            if (movementInput.magnitude > 0.01f)
            {
                // Movimento aéreo reduzido
                Vector2 reducedInput = movementInput * 0.6f; // 60% do movimento normal
                controller.ApplyMovement(reducedInput);
            }

            // Aplica gravidade adicional mais forte APENAS quando está no ar
            if (!IsGrounded())
            {
                float additionalGravity = 15f; // Gravidade base adicional

                // Se o profile tem gravidade configurada, usa ela, senão usa a padrão
                if (controller.MovementProfile != null && controller.MovementProfile.additionalGravity > 0f)
                {
                    additionalGravity = controller.MovementProfile.additionalGravity;
                }

                // Aplica gravidade extra mais forte durante o jump (mesma intensidade do Fall)
                Vector3 extraGravity = Vector3.down * additionalGravity * 1.5f; // 50% mais gravidade que o configurado
                controller.Rigidbody.AddForce(extraGravity, ForceMode.Acceleration);
            }
        }
        private void GetClosestJumpPad()
        {
            Transform yemmaBody = controller.transform;
            float oldDot = 0;

            colliders = Physics.OverlapSphere(yemmaBody.position + Vector3.up * 1.33f, 8f, layerMask).ToList();

            if (colliders.Count == 0) currentClosest = null;
            colliders.ForEach(collider =>
            {
                if (currentClosest == null) { currentClosest = collider.gameObject; }

                Vector3 dir = collider.transform.position - yemmaBody.position;

                float dot = Vector3.Dot(new Vector3(yemmaBody.forward.x, 0, yemmaBody.forward.z), dir.normalized);
                if (dot > oldDot)
                {
                    oldDot = dot;
                    currentClosest = collider.gameObject;
                }
                if (currentClosest == lastJumpedClosest) currentClosest = null;
                if (currentClosest != null) Debug.Log(currentClosest.name);

            });
        }
        private void DashTowardsJumpPad()
        {
            if (currentClosest == null || !dash) return;
            controller.Rigidbody.isKinematic = true;
            Transform yemmaTransform = controller.transform;
            yemmaTransform.position = Vector3.Lerp(startPosition, currentClosest.transform.position, dashProgress);
            controller.Rigidbody.position = yemmaTransform.position;
            Vector3 positionDiference = currentClosest.transform.position - yemmaTransform.position;
            dashProgress += Time.fixedDeltaTime * 6.33f / Mathf.Min(positionDiference.magnitude, 1f);
            if (positionDiference.magnitude < 0.07f) dashCompleted = true;
        }

        private void ExitDash()
        {
            controller.Rigidbody.isKinematic = false;
            if (dash)
            {
                dash = false;
                dashDelay = 0;
                Jump();
            }
            else
            {
                if (IsGrounded())
                {
                    var idleState = new YemmaIdleState(controller, inputManager, stateMachine);
                    stateMachine.ChangeState(idleState);
                }
                else
                {
                    var fallState = new YemmaFallState(controller, inputManager, stateMachine);
                    stateMachine.ChangeState(fallState);
                }
            }

        }
        private void TransitionToEdgeHang()
        {
            var edgeHangState = new YemmaEdgeHangState(controller, inputManager, stateMachine);
            stateMachine.ChangeState(edgeHangState);
        }

    }
}