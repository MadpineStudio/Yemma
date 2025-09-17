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
        private bool dashCompleted = false;
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
            dashProgress = 0f;
            dashCompleted = false;
            
            // Align player instantly to dash direction
            Vector3 direction = (endPosition - startPosition).normalized;
            controller.transform.rotation = Quaternion.LookRotation(direction);
            
            // Disable physics during dash
            controller.Rigidbody.isKinematic = true;
        }

        public override void UpdateLogic()
        {
            base.UpdateLogic();
            
            if (dashCompleted) return;
            
            // Update progress based on speed
            float totalDistance = Vector3.Distance(startPosition, endPosition);
            float progressIncrement = (dashSpeed * Time.deltaTime) / totalDistance;
            dashProgress += progressIncrement;
            
            if (dashProgress >= 1f)
            {
                dashProgress = 1f;
                dashCompleted = true;
            }
            
            // Apply curve to progress and calculate position on bezier curve
            float curvedProgress = dashCurve.Evaluate(dashProgress);
            Vector3 currentPos = CalculateQuadraticBezier(startPosition, controlPoint, endPosition, curvedProgress);
            controller.transform.position = currentPos;
            
            if (dashCompleted)
            {
                ExitDash();
            }
        }
        
        private Vector3 CalculateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            
            Vector3 point = uu * p0 + 2 * u * t * p1 + tt * p2;
            return point;
        }

        private void ExitDash()
        {
            controller.Rigidbody.isKinematic = false;
            
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
}