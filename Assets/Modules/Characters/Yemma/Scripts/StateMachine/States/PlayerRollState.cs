using UnityEngine;
using _Modules.FinalMachine.Machines.PlayerMovement.Events;
using _Modules.Player.Script;
namespace _Modules.FinalMachine.Machines.PlayerMovement.States
{
    public class PlayerRollState : PlayerMovementState
    {
        public PlayerRollState(Player.Script.Player player) : base(player) { }
        private float _timer;
        public override void Enter()
        {
            base.Enter();
            Player.ChangeAnimations(_Modules.Player.Script.Player.AnimationStates.Roll, .05f, .1f);

            _timer = 0;
            Roll();
            // Player.playerRb.isKinematic = true;
        }
        public override void Exit()
        {
            base.Exit();
            // Player.playerRb.isKinematic = false;
        }
        public override void Update()
        {
            base.Update();
            if (_timer >= Player.physicalProfile.rollDuration)
            {
                ToIdle();
                ToAirborne();
            }
            else
            {
                _timer += Time.deltaTime;
            }
            // ToIdle();
            // ToAirborne();
        }
        public override void UpdatePhysics()
        {
            UseSpring();
        }
        // transitions
        void ToIdle()
        {
            if (Player.Grounded())
                MovementMachineEventManager.RaiseStateChangeIdle();

        }
        void ToAirborne()
        {
            if (!Player.Grounded())
                MovementMachineEventManager.RaiseStateChangeAirborne();
        }

        void RollKinnematic()
        {
            Transform playerTransform = Player.transform;
            Vector3 direction = Player.yemmaBody.forward;

            Vector3 targetPosition = playerTransform.position +
                                     (direction * (Player.physicalProfile.rollDistance * Time.fixedDeltaTime));

            RaycastHit hit;
            if (Physics.Raycast(targetPosition + Vector3.up * 1f, Vector3.down, out hit, 2f, Player.physicalProfile.groundLayer))
            {
                if (targetPosition.y < hit.point.y) targetPosition.y = hit.point.y;
            }

            // Move o Rigidbody para a nova posição ajustada
            Player.playerRb.MovePosition(targetPosition);
        }
        void UseSpring()
        {
            Vector3 spring = CalculateSpringForce(Player.transform, Player.playerRb, Player.physicalProfile);
            Player.playerRb.AddForce(spring, ForceMode.Acceleration);
        }
        void Roll()
        {
            Vector3 direction = Player.yemmaBody.forward;
            Player.playerRb.AddForce(direction * Player.physicalProfile.rollDistance, ForceMode.Impulse);
        }
    }
}