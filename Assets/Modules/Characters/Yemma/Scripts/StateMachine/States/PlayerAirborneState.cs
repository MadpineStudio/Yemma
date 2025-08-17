using _Modules.FinalMachine.Machines.PlayerMovement.Events;
using UnityEngine;

namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerAirborneState : PlayerMovementState
    {
        public PlayerAirborneState(Player.Script.Player player) : base(player) { }
        public override void Enter()
        {
            base.Enter();
            Player.inputManager.glide = false;
        }

        public override void Update()
        {
            base.Update();
            ToIdle();
            ToGlide();
        }

        public override void UpdatePhysics()
        {
            // base.UpdatePhysics();
            Move(false);
            CheckHeight();

        }

        public override void Exit()
        {
            base.Exit();
        }

        private void ToIdle()
        {
            if (Player.Grounded())
                MovementMachineEventManager.RaiseStateChangeIdle();
        }

        private void ToGlide()
        {
            if (Player.inputManager.glide && !Player.exitedGlide && Player.playerInventory.para_Glider)
                MovementMachineEventManager.RaiseStateChangeGlide();
        }
        void CheckHeight()
        {
            if (Physics.Raycast(Player.transform.position + Vector3.up, -Player.transform.up, out RaycastHit hit, 1f, Player.physicalProfile.groundLayer))
            {
                if (hit.point != null)
                {
                    Vector3 spring = CalculateSpringForce(Player.transform, Player.playerRb, Player.physicalProfile);
                    Player.playerRb.AddForce(spring, ForceMode.Acceleration);
                }
            }
        }
    }
}
