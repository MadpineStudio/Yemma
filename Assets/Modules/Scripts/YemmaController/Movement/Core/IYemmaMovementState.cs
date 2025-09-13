namespace Yemma.Movement.Core
{
    /// <summary>
    /// Interface base para estados de movimentação do Yemma
    /// </summary>
    public interface IYemmaMovementState
    {
        void Enter();
        void Exit();
        void HandleInput();
        void UpdateLogic();
        void UpdatePhysics();
    }
}
