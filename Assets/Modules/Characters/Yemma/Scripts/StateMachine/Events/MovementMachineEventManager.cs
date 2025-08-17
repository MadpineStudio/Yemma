namespace _Modules.FinalMachine.Machines.PlayerMovement.Events
{
    public abstract class MovementMachineEventManager
    {
        // IDLE
        public delegate void OnStateChangeIdleHandler();
        public static event OnStateChangeIdleHandler OnStateChangeIdle;
        public static void RaiseStateChangeIdle() => OnStateChangeIdle?.Invoke();
        //WALK
        public delegate void OnStateChangeWalkHandler();
        public static event OnStateChangeIdleHandler OnStateChangeWalk;
        public static void RaiseStateChangeWalk() => OnStateChangeWalk?.Invoke();

        //JUMP
        public delegate void OnStateChangeJumpHandler();
        public static event OnStateChangeJumpHandler OnStateChangeJump;
        public static void RaiseStateChangeJump() => OnStateChangeJump?.Invoke();

        // CLIMB
        public delegate void OnStateChangeClimbHandler();
        public static event OnStateChangeClimbHandler OnStateChangeClimb;
        public static void RaiseStateChangeClimb() => OnStateChangeClimb?.Invoke();
        // CLIMBWALL
        public delegate void OnStateChangeClimbingWallHandler();
        public static event OnStateChangeClimbingWallHandler OnStateChangeClimbingWall;
        public static void RaiseStateChangeClimbingWall() => OnStateChangeClimbingWall?.Invoke();
        // CLIMBSTAIRS
        public delegate void OnStateChangeClimbingStairsHandler();
        public static event OnStateChangeClimbingStairsHandler OnStateChangeClimbingStairs;
        public static void RaiseStateChangeClimbingStairs() => OnStateChangeClimbingStairs?.Invoke();
        // FALL
        public delegate void OnStateChangeAirborneHandler();
        public static event OnStateChangeAirborneHandler OnStateChangeAirborne;
        public static void RaiseStateChangeAirborne() => OnStateChangeAirborne?.Invoke();
        // ROLL
        public delegate void OnStateChangeRollHandler();
        public static event OnStateChangeRollHandler OnStateChangeRoll;
        public static void RaiseStateChangeRoll() => OnStateChangeRoll?.Invoke();
        // GLIDE
        public delegate void OnStateChangeGlideHandler();
        public static event OnStateChangeGlideHandler OnStateChangeGlide;
        public static void RaiseStateChangeGlide() => OnStateChangeGlide?.Invoke();
    }
}