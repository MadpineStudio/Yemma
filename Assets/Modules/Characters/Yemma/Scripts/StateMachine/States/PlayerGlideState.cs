using _Modules.FinalMachine.Machines.PlayerMovement.Events;
using _Modules.Player.Script;
using Unity.Mathematics;
using UnityEngine;
namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerGlideState : PlayerMovementState
    {
        public PlayerGlideState(Player.Script.Player player) : base(player) { }
        private Vector2 inputDirection;
        private Rigidbody playerRb;
        private float _speedModifier = 0;
        private float _forwardSpeed;
        private float _slowedRotationAgle = 0;
        private float _upwardLiftCurveTime = 0;
        private PhysicalProfile _physicalProfile;
        public override void Enter()
        {
            base.Enter();
            _physicalProfile = Player.physicalProfile;
            playerRb = Player.playerRb;
            playerRb.isKinematic = true;

            _forwardSpeed = _physicalProfile.forwardSpeed * 0.01f;
            _speedModifier = 0;
            _slowedRotationAgle = 0;
            _upwardLiftCurveTime = 0;


        }
        public override void Exit()
        {
            base.Exit();
            playerRb.position = Player.transform.position;
            playerRb.isKinematic = false;
            playerRb.WakeUp();
            playerRb.linearVelocity = Vector3.zero;
        }
        public override void UpdatePhysics()
        {
            inputDirection = Player.inputManager.movementVector;
            Fly();
            Turn();
        }
        public override void Update()
        {
            base.Update();
            DetectSurrounds();
            Debug.DrawRay(Player.yemmaBody.position, Player.yemmaBody.forward * 0.33f, Color.red);
            Debug.DrawRay(Player.transform.position, -Player.transform.up * 0.33f, Color.red);
            Debug.DrawRay(Player.transform.position + Vector3.up * .5f, Quaternion.AngleAxis(30f, Player.transform.up) * Player.yemmaBody.forward, Color.cyan);
            Debug.DrawRay(Player.transform.position + Vector3.up * .5f, Quaternion.AngleAxis(-30f, Player.transform.up) * Player.yemmaBody.forward, Color.cyan);

        }
        // transitions
        void ToAirborne()
        {
            Player.exitedGlide = true;
            MovementMachineEventManager.RaiseStateChangeAirborne();
        }
        void Fly()
        {
            Vector3 forceDirection = Player.yemmaBody.forward;
            float playerXRotation = ((Player.yemmaBody.rotation.eulerAngles.x + 180) % 360) - 180;
            float value = _physicalProfile.aoaLiftCurve.Evaluate(playerXRotation);

            if (inputDirection.y != 0)
            {
                _speedModifier += Time.deltaTime * 0.1f * value;
            }
            else if (inputDirection.y == 0)
            {
                _speedModifier -= Time.deltaTime * 0.02f * Mathf.Sign(_speedModifier);
            }
            if (_speedModifier <= (_forwardSpeed * -0.25f))
            {
                Debug.Log(_speedModifier);

                if (_slowedRotationAgle < (20 - playerXRotation)) _slowedRotationAgle += Time.deltaTime * 30;
                forceDirection = Quaternion.AngleAxis(_slowedRotationAgle , Player.yemmaBody.right) * Player.yemmaBody.forward;
            }
            else if(_slowedRotationAgle >= 0)
            {
                _slowedRotationAgle -= Time.deltaTime * 30;
                _slowedRotationAgle = _slowedRotationAgle < 0? 0: _slowedRotationAgle;
            }
            _speedModifier = Mathf.Clamp(_speedModifier, _forwardSpeed * -0.3f, _forwardSpeed);

            Vector3 movement = forceDirection * (_forwardSpeed + _speedModifier);
            Player.playerRb.MovePosition(Player.playerRb.position + movement);

        }

        void Turn()
        {
            float playerXRotation = ((Player.yemmaBody.rotation.eulerAngles.x + 180) % 360) - 180;
            float xAxisRotationValue;
            float yAxisRotationValue = 0;
            LayerMask groundLayer = _physicalProfile.groundLayer;
            bool canRotateNegative = playerXRotation < _physicalProfile.maxGlideInclination;
            bool canRotatePositive = playerXRotation > -_physicalProfile.maxGlideInclination;

            if (Physics.Raycast(Player.transform.position + Vector3.up * .5f, Quaternion.AngleAxis(-30f, Player.transform.up) * Player.yemmaBody.forward, out RaycastHit leftHit, 1f, groundLayer))
            {
                if (leftHit.point != null)
                {
                    yAxisRotationValue = 1;
                }
            }
            else if (Physics.Raycast(Player.transform.position + Vector3.up * .5f, Quaternion.AngleAxis(30f, Player.transform.up) * Player.yemmaBody.forward, out RaycastHit rightHit, 1f, groundLayer))
            {
                if (rightHit.point != null) yAxisRotationValue = -1;
            }
            else
            {
                yAxisRotationValue = inputDirection.x;
            }

            _upwardLiftCurveTime = (inputDirection.y < 0 && playerXRotation < 0) ? _upwardLiftCurveTime + Time.fixedDeltaTime * 2 : 0;

            xAxisRotationValue = canRotateNegative && inputDirection.y > 0 ? 1 : (canRotatePositive && inputDirection.y < 0 ? -1 : 0);
            xAxisRotationValue *= (xAxisRotationValue > 0 || playerXRotation > 0) ? 1 : _physicalProfile.turnCurve.Evaluate(_upwardLiftCurveTime);
            Player.yemmaBody.Rotate(xAxisRotationValue, yAxisRotationValue * _physicalProfile.turnSpeed, 0);

            if (inputDirection.x == 0)
            {
                Vector3 newRotVec = new Vector3(Player.yemmaBody.rotation.eulerAngles.x, Player.yemmaBody.rotation.eulerAngles.y, 0);
                if (inputDirection.y == 0) newRotVec.x = 0;
                Quaternion newRot = Quaternion.Euler(newRotVec);
                Player.yemmaBody.rotation = Quaternion.Slerp(Player.yemmaBody.rotation, newRot, _physicalProfile.rotationVelocity * (.33f * Time.deltaTime));
            }


        }
        void DetectSurrounds()
        {
            bool colided = false;
            if (Physics.Raycast(Player.yemmaBody.position, Player.yemmaBody.forward, out RaycastHit frontHit, .33f, _physicalProfile.groundLayer))
            {
                if (frontHit.point != null) colided = true;
            }
            if (Physics.Raycast(Player.transform.position, -Player.transform.up, out RaycastHit downHit, .33f))
            {
                if (downHit.point != null) colided = true;
            }
            if (colided || !Player.inputManager.glide) ToAirborne();
        }

    }
}