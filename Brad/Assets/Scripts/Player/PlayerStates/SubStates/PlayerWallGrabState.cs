using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallGrabState : PlayerTouchingWallState
{
    private Vector2 holdPosition;

    public PlayerWallGrabState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        holdPosition = player.transform.position;

        HoldPosition();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (isExitingState) return;

        HoldPosition();

        if (yInput > 0)
        {
            stateMachine.ChangeState(player.WallClimbState);
        }else if((yInput < 0 || !grabInput) && !isGrounded)
        {
            stateMachine.ChangeState(player.WallSlideState);
        }
    }

    private void HoldPosition()
    {
        player.transform.position = holdPosition;
        player.SetVelocityX(0f);
        player.SetVelocityY(0f);
    }
}
