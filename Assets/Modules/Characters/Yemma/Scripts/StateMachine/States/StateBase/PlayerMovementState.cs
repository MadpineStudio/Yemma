using UnityEngine;

namespace _Modules.FinalMachine.Machines.PlayerMovement
{
    public class PlayerMovementState : IState
    {
        protected Vector3 lastPosition;
        protected Transform YemmaBody;
        protected PhysicalProfile physicalProfile;
        protected readonly Player.Script.Player Player;
        protected Vector3 MovementInput;
        private Camera camera;
        protected PlayerMovementState(Player.Script.Player player)
        {
            Player = player;
        }
        public virtual void Enter()
        {
            camera = Camera.main;
            physicalProfile = Player.physicalProfile;
            YemmaBody = Player.yemmaBody;
            lastPosition = YemmaBody.position;
        }
        public virtual void Exit() { }
        public virtual void HandleInput()
        {
            ReadMovementInput();
        }
        public virtual void Update() { }
        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void UpdatePhysics()
        {
            Move(true);
            AlignPlayerToTerrain();
        }
        #region Main Methods
        private void ReadMovementInput()
        {
            MovementInput = Player.inputManager.movementVector;
        }
        #endregion

        #region PHYSICAL MOVEMENT

        public void Move(bool calculateSpring)
        {
            if (YemmaBody == null) YemmaBody = Player.yemmaBody;

            Vector2 inputDirection = Player.inputManager.movementVector;
            Vector3 movement = CalculatePlayerMovement(
                Player.playerRb.linearVelocity,
                inputDirection,
                camera.transform,
                Player.physicalProfile
            );
            Vector3 playerLinearVelocity = Player.playerRb.linearVelocity;
            playerLinearVelocity.y = 0;
            Vector3 springForce = Vector3.zero;
            if (calculateSpring)
            {
                springForce = CalculateSpringForce(
                Player.transform,
                Player.playerRb,
                Player.physicalProfile
            );
            }
            else if (playerLinearVelocity.magnitude > 0.01f)
            {
                Vector3 direction = CalculateCameraRelativeMovement(MovementInput, camera.transform);
                Quaternion tiltRotation = GetTargerRotation(direction);
                tiltRotation = Quaternion.Euler(0, tiltRotation.eulerAngles.y, 0);
                if (tiltRotation.y != YemmaBody.rotation.y) YemmaBody.rotation = Quaternion.Slerp(YemmaBody.rotation, tiltRotation, Player.physicalProfile.rotationVelocity * Time.deltaTime);
            }
            Player.playerRb.AddForce(movement + springForce, ForceMode.Acceleration);

        }
        public Vector3 CalculateCameraRelativeMovement(Vector2 inputDirection, Transform cameraTransform)
        {
            inputDirection = inputDirection.normalized;

            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 movement = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);


            RaycastHit hit;
            if (Physics.Raycast(Player.transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 1.5f))
            {
                Vector3 slopeNormal = hit.normal;
                movement = Vector3.ProjectOnPlane(movement, slopeNormal).normalized;

            }
            return movement;
        }
        public Vector3 CalculatePlayerMovement(Vector3 currentVelocity, Vector2 inputDirection, Transform cameraTransform, PhysicalProfile profile)
        {
            Vector3 cameraRelativeMovement = CalculateCameraRelativeMovement(inputDirection, cameraTransform);
            Vector3 targetSpeed = cameraRelativeMovement * profile.velocity;
            Vector3 speedDif = targetSpeed - currentVelocity;
            float accelRate = targetSpeed.magnitude > 0.01f
                ? profile.acceleration
                : profile.decceleration;
            Vector3 movement = new Vector3(
                Mathf.Pow(Mathf.Abs(speedDif.x) * accelRate, profile.velPower) * Mathf.Sign(speedDif.x),
                0,
                Mathf.Pow(Mathf.Abs(speedDif.z) * accelRate, profile.velPower) * Mathf.Sign(speedDif.z)
            );
            return movement;
        }
        public Vector3 CalculateSpringForce(Transform playerTransform, Rigidbody playerRb, PhysicalProfile profile)
        {
            int layerMask = profile.groundLayer | profile.interactableLayer;
            if (Physics.Raycast(playerTransform.position + profile.springOffsset, Vector3.down, out RaycastHit hit, profile.restDistance * 5f, layerMask))
            {
                if (hit.distance < profile.restDistance * profile.activationMultiplier)
                {
                    Vector3 groundNormal = hit.normal;
                    float displacement = profile.restDistance - hit.distance;

                    if (displacement > 0)
                    {
                        Vector3 springForce = Vector3.up * (profile.springConstant * displacement);
                        float verticalVel = Vector3.Dot(playerRb.linearVelocity, groundNormal);
                        Vector3 dampingForce = Vector3.up * (-verticalVel * profile.damping);

                        return springForce + dampingForce;
                    }
                }
            }
            return Vector3.zero;
        }
        Quaternion GetTargerRotation(Vector3 direction)
        {
            Vector3 targetRotationVector = YemmaBody.eulerAngles;
            if (direction != Vector3.zero) targetRotationVector.y = Quaternion.LookRotation(direction, YemmaBody.up).eulerAngles.y;
            return Quaternion.Euler(targetRotationVector);
        }

        private void AlignPlayerToTerrain()
        {
            Quaternion terrainRotation;
            Vector3 direction = CalculateCameraRelativeMovement(MovementInput, camera.transform);

            RaycastHit hit;
            if (Physics.Raycast(Player.transform.position + Vector3.up, -Player.transform.up, out hit, 2, Player.physicalProfile.groundLayer))
            {
                Vector3 groundNormal = hit.normal;
                Vector3 tiltEuler = CalculateTiltRotation(direction);
                Vector3 forward = Vector3.ProjectOnPlane(YemmaBody.forward, groundNormal);

                Quaternion tiltRotation = Quaternion.Euler(tiltEuler);

                terrainRotation = Quaternion.LookRotation(forward, groundNormal);

                if (IsRotationInRange(terrainRotation, -Player.physicalProfile.maxTiltAmount, Player.physicalProfile.maxTiltAmount))
                {
                    terrainRotation *= tiltRotation;
                    YemmaBody.rotation = ApplyRotation(terrainRotation, physicalProfile.tiltVelocity);
                }
                else
                {
                    tiltEuler.y = YemmaBody.eulerAngles.y;
                    YemmaBody.rotation = ApplyRotation(Quaternion.Euler(tiltEuler), physicalProfile.tiltVelocity);
                }

            }
            RotatePlayer(direction);
        }

        public void RotatePlayer(Vector3 direction)
        {
            direction.y = 0;
            Vector3 targetRotationVector = YemmaBody.eulerAngles;
            if (direction != Vector3.zero) targetRotationVector.y = Quaternion.LookRotation(direction, YemmaBody.up).eulerAngles.y;
            YemmaBody.rotation = ApplyRotation(Quaternion.Euler(targetRotationVector), physicalProfile.rotationVelocity);
        }

        private Vector3 CalculateTiltRotation(Vector3 direction)
        {
            Vector3 newRotation = Vector3.zero;
            Vector3 playerLinearVelocity = Player.playerRb.linearVelocity;
            playerLinearVelocity.y = 0;
            if (playerLinearVelocity.magnitude > 0.01f)
            {
                direction = Quaternion.Inverse(YemmaBody.rotation) * direction;
                newRotation = new Vector3(direction.z * Player.physicalProfile.tiltAmount, 0, direction.x * Player.physicalProfile.tiltAmount);

            }
            lastPosition = YemmaBody.position;
            return newRotation;
        }

        private bool IsRotationInRange(Quaternion rotation, float min, float max)
        {
            rotation *= Quaternion.Inverse(Quaternion.Euler(0, rotation.eulerAngles.y, 0));
            float xAngle = ((rotation.eulerAngles.x + 180) % 360) - 180;
            float zAngle = ((rotation.eulerAngles.z + 180) % 360) - 180;
            if (xAngle > min && xAngle < max)
            {
                if (zAngle > min && zAngle < max) return true;
            }
            return false;

        }
        private Quaternion ApplyRotation(Quaternion newRotation, float rotationVelocity)
        {
            return Quaternion.Slerp(YemmaBody.rotation, newRotation, rotationVelocity * Time.deltaTime);
        }

        #endregion

        public RaycastHit WallChecker()
        {
            int layerMask = Player.physicalProfile.climbLayer | Player.physicalProfile.climbStairsLayer;
            Physics.Raycast(Player.transform.position, Player.yemmaBody.transform.forward, out RaycastHit hitInfo, Player.physicalProfile.climbWallCheckerLength, layerMask);
            DrawWallDetectionRay(!hitInfo.transform);
            return hitInfo;
        }
       
        private void DrawWallDetectionRay(bool hitWall)
        {
            Debug.DrawRay(Vector3.up * .5f + Player.transform.position, Player.yemmaBody.transform.forward * Player.physicalProfile.climbWallCheckerLength, hitWall ? Color.green : Color.red);
        }
        void OnDrawGizmos()
        {
            DrawWallDetectionRay(WallChecker().transform);
        }
    }
}
