using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poop : Entity
{
    public Poop_IdleState idleState { get; private set; }
    public Poop_MoveState moveState { get; private set; }
    public Poop_PlayerDetectedState playerDetectedState { get; private set; }

    [SerializeField]
    private D_IdleState idleStateData;
    [SerializeField]
    private D_MoveState moveStateData;
    [SerializeField]
    private D_PlayerDetected playerDetectedData;

    public override void Start()
    {
        base.Start();

        moveState = new Poop_MoveState(this, stateMachine, "move", moveStateData, this);
        idleState = new Poop_IdleState(this, stateMachine, "idle", idleStateData, this);
        playerDetectedState = new Poop_PlayerDetectedState(this, stateMachine, "playerDetected", playerDetectedData, this);

        stateMachine.Initialize(moveState);
    }
}
