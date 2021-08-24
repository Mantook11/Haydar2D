using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rat : Entity
{
    public Rat_IdleState idleState { get; private set; }
    public Rat_MoveState moveState { get; private set; }
    public Rat_PlayerDetectedState playerDetectedState { get; private set; }
    public Rat_ChargeState chargeState { get; private set; }
    public Rat_LookForPlayerState lookForPlayerState { get; private set; }
    public Rat_MeleeAttackState meleeAttackState { get; private set; }
    public Rat_StunState stunState { get; private set; }
    public Rat_DeadState deadState { get; private set; }

    [SerializeField]
    private Transform meleeAttackPosition;

    private int playerAttackDirection;

    [SerializeField]
    private D_IdleState idleStateData;
    [SerializeField]
    private D_MoveState moveStateData;
    [SerializeField]
    private D_PlayerDetected playerDetectedData;
    [SerializeField]
    private D_ChargeState chargeStateData;
    [SerializeField]
    private D_LookForPlayerState lookForPlayerStateData;
    [SerializeField]
    private D_MeleeAttack meleeAttackStateData;
    [SerializeField]
    private D_StunState stunStateData;
    [SerializeField]
    private D_DeadState deadStateData;

    private bool isAttacking;
    private bool isCharging;

    public override void Start()
    {
        base.Start();

        moveState = new Rat_MoveState(this, stateMachine, "move", moveStateData, this);
        idleState = new Rat_IdleState(this, stateMachine, "idle", idleStateData, this);
        playerDetectedState = new Rat_PlayerDetectedState(this, stateMachine, "playerDetected", playerDetectedData, this);
        chargeState = new Rat_ChargeState(this, stateMachine, "charge", chargeStateData, this);
        lookForPlayerState = new Rat_LookForPlayerState(this, stateMachine, "lookForPlayer", lookForPlayerStateData, this);
        meleeAttackState = new Rat_MeleeAttackState(this, stateMachine, "meleeAttack", meleeAttackPosition, meleeAttackStateData, this);
        stunState = new Rat_StunState(this, stateMachine, "stun", stunStateData, this);
        deadState = new Rat_DeadState(this, stateMachine, "dead", deadStateData, this);

        stateMachine.Initialize(moveState);
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawWireSphere(meleeAttackPosition.position, meleeAttackStateData.attackRadius);
    }

    public override void Damage(AttackDetails attackDetails)
    {

        base.Damage(attackDetails);
        if(attackDetails.position.x > transform.position.x)
        {
            playerAttackDirection = -1;
        }
        else
        {
            playerAttackDirection = 1;
        }

        if (isDead)
        {
            stateMachine.ChangeState(deadState);
        }
        else if (isStunned && stateMachine.currentState != stunState)
        {
            stateMachine.ChangeState(stunState);
        }
        else if (!CheckPlayerInMinAgroRange() && stateMachine.currentState != meleeAttackState && stateMachine.currentState != chargeState )
        {
            lookForPlayerState.SetTurnImmediately(true);
            stateMachine.ChangeState(lookForPlayerState);
        }

    }
}
