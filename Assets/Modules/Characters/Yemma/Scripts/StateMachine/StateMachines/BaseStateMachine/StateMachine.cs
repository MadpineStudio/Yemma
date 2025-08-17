using _Modules.FinalMachine.Events;
using UnityEngine;

namespace _Modules.FinalMachine
{
    public abstract class StateMachine
    {
        public IState CurrentState;
        public IState PreviousState;
        public bool enableStateDebugging;
        protected StateMachine()
        {
            MachineEventManager.OnStateChange += ChangeState;
        }

        ~StateMachine()
        {
            MachineEventManager.OnStateChange -= ChangeState;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public void ChangeState(IState nextState)
        {
            if (PreviousState != null)
                PreviousState = CurrentState;
            else
                PreviousState = nextState;

            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState?.Enter();

            if (enableStateDebugging) Debug.Log("In: " + CurrentState.GetType().Name);
        }
        public void HandleInput()
        {
            CurrentState?.HandleInput();
        }
        public void Update()
        {
            CurrentState?.Update();
        }

        public void UpdatePhysics()
        {
            CurrentState?.UpdatePhysics();
        }
    }
}