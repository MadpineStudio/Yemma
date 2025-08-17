using _Modules.FinalMachine.Machines.PlayerMovement.Events;
using UnityEngine;

namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerClimbState : PlayerMovementState
    {
        public PlayerClimbState(Player.Script.Player player) : base(player) { }
        private bool _hitWall;
        public override void Enter()
        {
            base.Enter();
            RaycastHit hit = WallChecker();
            _hitWall = false;
            if (hit.transform != null)
            {
                Vector3 playerPos = Player.transform.position;
                Vector3 hitPos = hit.point;

                playerPos.y = 0;
                hitPos.y = 0;

                _hitWall = (playerPos - hitPos).magnitude < 0.2f;
            }
        }

        public override void Update()
        {
            base.Update();
            RaycastHit hit = WallChecker();
            if (hit.transform != null)
            {
                Vector3 playerPos = Player.transform.position;
                Vector3 hitPos = hit.point;

                playerPos.y = 0;
                hitPos.y = 0;

                _hitWall = (playerPos - hitPos).magnitude < 0.5f;
            }



            ToIdle();
            ToAirborne();
            ToClimbingWall();
            ToClimbingStairs();
        }

        public override void UpdatePhysics()
        {
        }

        public override void Exit()
        {
            base.Exit();
            // Player.playerRb.isKinematic = false;
        }
        void ToIdle()
        {
            if (!_hitWall && Player.Grounded())
            {
                MovementMachineEventManager.RaiseStateChangeIdle();
            }
        }
        void ToAirborne()
        {
            if (!_hitWall)
                MovementMachineEventManager.RaiseStateChangeAirborne();
        }
        void ToClimbingStairs()
        {
            if (_hitWall)
            {
                bool isStairs = (Player.physicalProfile.climbStairsLayer & (1 << WallChecker().collider.gameObject.layer)) != 0;
                if (isStairs) MovementMachineEventManager.RaiseStateChangeClimbingStairs();
            }
        }
        void ToClimbingWall()
        {
            if (_hitWall)
            {
                bool isWall = (Player.physicalProfile.climbLayer & (1 << WallChecker().collider.gameObject.layer)) != 0;
                if (isWall) MovementMachineEventManager.RaiseStateChangeClimbingWall();
            }
        }
    }
}
