using UnityEngine;
using _Modules.FinalMachine.Machines.PlayerMovement.Events;

namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerClimbingWallState : PlayerClimbState
    {
        public PlayerClimbingWallState(Player.Script.Player player) : base(player) { }
        private bool _hitWall;
        public override void Enter()
        {
            base.Enter();
            _hitWall = true;
            AlignPlayerToWall(WallChecker().normal, 200);
            if (_hitWall)
            {
                Player.playerRb.isKinematic = true;
            }
        }

        public override void Update()
        {
            ToAirborne();
        }

        public override void UpdatePhysics()
        {
            Climb();
        }

        public override void Exit()
        {
            base.Exit();
            Player.playerRb.isKinematic = false;
        }

        void ToAirborne()
        {
            if (!_hitWall)
            {
                MovementMachineEventManager.RaiseStateChangeAirborne();
            }
        }
        public void Climb()
        {
            Vector2 inputDirection = Player.inputManager.movementVector;
            Transform playerTransform = Player.transform;
            Vector3 position = playerTransform.position;

            RaycastHit hitInfo = WallChecker();
            _hitWall = hitInfo.transform != null;
            if (_hitWall)
            {
                AlignPlayerToWall(hitInfo.normal);
                Vector3 climbMovement = CalculateClimbMovementOnSurface(inputDirection, hitInfo.normal, Player.physicalProfile);
                Vector3 targetPos = hitInfo.normal * 0.2f;
                targetPos += hitInfo.point;
                targetPos.y = playerTransform.position.y;
                Vector3 newPos = Vector3.Lerp(position, targetPos + climbMovement, Time.deltaTime);
                Player.playerRb.MovePosition(newPos);
            }
        }
        private void AlignPlayerToWall(Vector3 wallNormal)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-wallNormal, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(Player.yemmaBody.rotation, targetRotation, Player.physicalProfile.climbTurnVelocity * Time.fixedDeltaTime);
            Player.yemmaBody.transform.rotation = Quaternion.Slerp(Player.yemmaBody.transform.rotation, newRotation, Player.physicalProfile.climbTurnVelocity * Time.deltaTime);
        }
        private void AlignPlayerToWall(Vector3 wallNormal, float rotationVelocity)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-wallNormal, Vector3.up);
            Quaternion newRotation = Quaternion.Slerp(Player.yemmaBody.rotation, targetRotation, rotationVelocity * Time.fixedDeltaTime);
            Player.yemmaBody.transform.rotation = Quaternion.Slerp(Player.yemmaBody.transform.rotation, newRotation, rotationVelocity * Time.deltaTime);
        }
        public Vector3 CalculateClimbMovementOnSurface(Vector2 inputDirection, Vector3 wallNormal, PhysicalProfile profile)
        {
            // Movimento lateral independente da rotação da câmera
            Vector3 horizontal = Vector3.Cross(wallNormal, Vector3.up).normalized;
            // Movimento vertical (subida/descida)
            Vector3 vertical = Vector3.up;
            Vector3 movement = horizontal * inputDirection.x + vertical * inputDirection.y;
            return movement * profile.climbXVelocity;
        }
    }
}
