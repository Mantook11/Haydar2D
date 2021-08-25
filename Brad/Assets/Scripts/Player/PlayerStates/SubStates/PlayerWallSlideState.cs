using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerTouchingWallState
{
    public PlayerWallSlideState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();


        if (isExitingState) return;

        player.SetVelocityY(-playerData.wallSlideVelocity);

        if (isGrounded)
        {
            stateMachine.ChangeState(player.IdleState);
        }
        else if(grabInput && yInput != -1)
        {
            stateMachine.ChangeState(player.WallGrabState);
        }else if(!grabInput && xInput != player.FacingDirection)
        {
            stateMachine.ChangeState(player.InAirState);
        }
    }
}
