using _Modules.FinalMachine.Machines.PlayerMovement.Events;
using UnityEngine;
namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerJumpState : PlayerMovementState
    {
        public PlayerJumpState(Player.Script.Player player) : base(player) { }
        private float delay = 0;
        public override void Enter()
        {
            base.Enter();
            if (Player.Grounded() || WallChecker().transform || !Player.exitedGlide)
            {
                Vector3 jumpDir = Vector3.up * Player.physicalProfile.jumpForceDirection.y;

                if (Player.PlayerMovementStateMachine.PreviousState == Player.PlayerMovementStateMachine.playerGlideState)
                {
                    Player.exitedGlide = true;
                    // jumpDir = Quaternion.Inverse(YemmaBody.rotation) * new Vector3(0f, 0.7777f, 0.777f);
                    jumpDir = (YemmaBody.forward + Vector3.up) * Player.physicalProfile.jumpForceDirection.y;
                }
                Vector3 newVelocity = Player.playerRb.linearVelocity;
                newVelocity.y = 0;
                Player.playerRb.linearVelocity = newVelocity;
                delay = Time.time + .2f;
                Player.playerRb.WakeUp();
                Player.playerRb.AddForce(jumpDir, ForceMode.Impulse);
            }
            Player.ChangeAnimations(_Modules.Player.Script.Player.AnimationStates.Jump, 0.01f, -1);
        }
        public override void Update()
        {
            base.Update();
            ToAirborne();
        }
        public override void UpdatePhysics()
        {
            // base.UpdatePhysics();
            // Move(false);
        }
        void ToAirborne()
        {
            if (delay < Time.time)
            {
                MovementMachineEventManager.RaiseStateChangeAirborne();
            }
        }

    }
}