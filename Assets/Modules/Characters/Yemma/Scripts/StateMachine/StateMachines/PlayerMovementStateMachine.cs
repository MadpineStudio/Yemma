using _Modules.FinalMachine.Machines.PlayerMovement.Events;
using _Modules.FinalMachine.Machines.PlayerMovement.States;
using UnityEngine;
using UnityEngine.InputSystem;


namespace _Modules.FinalMachine.Machines.PlayerMovement
{
    public class PlayerMovementStateMachine : StateMachine
    {
        public Player.Script.Player Player;

        private float _rollTimer = 0f;
        
        PlayerIdleState playerIdleState;
        PlayerJumpState playerJumpState;
        PlayerWalkState playerWalkState;
        PlayerClimbState playerClimbState;
        PlayerAirborneState playerAirborneState;
        PlayerClimbingWallState playerClimbingWallState;
        PlayerClimbingStairsState playerClimbingStairsState;
        PlayerRollState playerRollState;
        public PlayerGlideState playerGlideState;
        public PlayerMovementStateMachine(Player.Script.Player player)
        {
            Player = player;

            playerIdleState = new PlayerIdleState(player);
            playerJumpState = new PlayerJumpState(player);
            playerWalkState = new PlayerWalkState(player);
            playerClimbState = new PlayerClimbState(player);
            playerClimbingWallState = new PlayerClimbingWallState(player);
            playerClimbingStairsState = new PlayerClimbingStairsState(player);
            playerAirborneState = new PlayerAirborneState(player);
            playerRollState = new PlayerRollState(player);
            playerGlideState = new PlayerGlideState(player);

            ChangeState(playerIdleState);

            MovementMachineEventManager.OnStateChangeIdle += () =>
            {
                ChangeState(playerIdleState);
            };

            MovementMachineEventManager.OnStateChangeJump += () =>
            {
                ChangeState(playerJumpState);
            };

            MovementMachineEventManager.OnStateChangeWalk += () =>
            {
                ChangeState(playerWalkState);
            };
            MovementMachineEventManager.OnStateChangeClimb += () =>
            {
                ChangeState(playerClimbState);
            };
            MovementMachineEventManager.OnStateChangeClimbingWall += () =>
            {
                ChangeState(playerClimbingWallState);
            };
            MovementMachineEventManager.OnStateChangeClimbingStairs += () =>
            {
                ChangeState(playerClimbingStairsState);
            };
            MovementMachineEventManager.OnStateChangeAirborne += () =>
            {
               ChangeState(playerAirborneState);
            };
            MovementMachineEventManager.OnStateChangeRoll += () =>
            {
                ChangeState(playerRollState);
            };
            MovementMachineEventManager.OnStateChangeGlide += () =>
            {
                ChangeState(playerGlideState);
            };

            Player.inputManager.inputActions.YemmaKeyboard.Jump.performed += JumpHandler;
            Player.inputManager.inputActions.YemmaKeyboard.Roll.performed += RollHandler;
        }
        private void JumpHandler(InputAction.CallbackContext ctxt)
        {
            if (CurrentState == playerAirborneState)
            {

                MovementMachineEventManager.RaiseStateChangeClimb();
            }
            else if (CurrentState != playerJumpState && CurrentState != playerRollState) MovementMachineEventManager.RaiseStateChangeJump();

        }

        private void RollHandler(InputAction.CallbackContext ctxt)
        {
            if ((CurrentState == playerAirborneState || Player.Grounded()) && _rollTimer < Time.time)
            {
                _rollTimer = Time.time + Player.physicalProfile.rollDelay;
                MovementMachineEventManager.RaiseStateChangeRoll();
            }
        }
        // protected void UpdateAnimationDebugTexInfo(string currentStateName, string previousStateName)
        // {
        //     Player.animatorDebugTex.text = $"<color=#00ff00>{currentStateName}</color>\n" +
        //                                  $"<color=#ff0000>{previousStateName}</color>";
        //     Player.infoStateDebugTex.SetText("");

        // }

    }
}
