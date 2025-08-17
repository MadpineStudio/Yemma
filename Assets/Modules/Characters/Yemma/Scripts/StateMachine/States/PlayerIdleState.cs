using _Modules.FinalMachine.Machines.PlayerMovement.Events;
namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerIdleState : PlayerMovementState
    {
        public PlayerIdleState(Player.Script.Player player) : base(player) { }
        public override void Enter()
        {
            base.Enter();
            Player.exitedGlide = false;
            Player.ChangeAnimations(_Modules.Player.Script.Player.AnimationStates.Idle, .11f, -1f);

        }
        public override void Update()
        {
            base.Update();
            ToWalk();
            // ToAirborne();
        }
        // transitions
        void ToWalk()
        {
            if (Player.Grounded() && Player.inputManager.movementVector.magnitude > .01f)
            {
                MovementMachineEventManager.RaiseStateChangeWalk();
            }
        }
        void ToAirborne(){
            if(!Player.Grounded()) MovementMachineEventManager.RaiseStateChangeAirborne();
        }
    }
}