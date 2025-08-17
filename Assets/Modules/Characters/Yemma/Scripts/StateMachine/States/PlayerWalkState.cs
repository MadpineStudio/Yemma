using _Modules.FinalMachine.Machines.PlayerMovement.Events;

namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerWalkState : PlayerMovementState
    {
        public PlayerWalkState(Player.Script.Player player) : base(player) { }
        public override void Enter()
        {
            base.Enter();
            Player.ChangeAnimations(_Modules.Player.Script.Player.AnimationStates.Walk, 0.1f, -1f);

        }

        public override void Update()
        {
            base.Update();
            ToIdle();
            ToAirborne();
        }
        
        // transitions
        void ToIdle(){
            if(Player.Grounded() && Player.inputManager.movementVector.magnitude < .001f) {
                MovementMachineEventManager.RaiseStateChangeIdle();
            }
        }
        void ToAirborne(){
            if(!Player.Grounded())
            MovementMachineEventManager.RaiseStateChangeAirborne();
        }
       
    }
}