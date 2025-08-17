namespace _Modules.FinalMachine.Events
{
    public abstract class MachineEventManager
    {
        public delegate void OnStateChangeHandler(IState state);
        public static event OnStateChangeHandler OnStateChange;
        public static void RaiseStateChange(IState state) => OnStateChange?.Invoke(state);
    }
}