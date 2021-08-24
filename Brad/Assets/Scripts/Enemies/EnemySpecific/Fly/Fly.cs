using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : Entity
{
    public Fly_MoveState moveState { get; private set; }
    public Fly_IdleState IdleState { get; private set; }
    public Fly_PlayerDetectedState playerDetectedState { get; private set; }
    public Fly_MeleeAttackState meleeAttackState { get; private set; }
    public Fly_LookForPlayerState lookForPlayerState { get; private set; }
    public Fly_StunState stunState { get; private set; }
    public Fly_DeadState deadState { get; private set; }
    public Fly_DodgeState dodgeState { get; private set; }
    public Fly_RangedAttackState rangedAttackState { get; private set; }

    [SerializeField]
    private D_MoveState moveStateData;
    [SerializeField]
    private D_IdleState idleStateData;
    [SerializeField]
    private D_PlayerDetected playerDetectedData;
    [SerializeField]
    private D_MeleeAttack meleeAttackStateData;
    [SerializeField]
    private D_LookForPlayerState lookForPlayerStateData;
    [SerializeField]
    private D_StunState stunStateData;
    [SerializeField]
    private D_DeadState deadStateData;
    [SerializeField]
    public D_DodgeState dodgeStateData;
    [SerializeField]
    public D_RangedAttackState rangedAttackStateData;

    [SerializeField]
    private Transform meleeAttackPosition, rangedAttackPosition;

    public override void Start()
    {
        base.Start();

        moveState = new Fly_MoveState(this, stateMachine, "move", moveStateData, this);
        IdleState = new Fly_IdleState(this, stateMachine, "idle", idleStateData, this);
        playerDetectedState = new Fly_PlayerDetectedState(this, stateMachine, "playerDetected", playerDetectedData, this);
        meleeAttackState = new Fly_MeleeAttackState(this, stateMachine, "meleeAttack", meleeAttackPosition, meleeAttackStateData, this);
        lookForPlayerState = new Fly_LookForPlayerState(this, stateMachine, "lookForPlayer", lookForPlayerStateData, this);
        stunState = new Fly_StunState(this, stateMachine, "stun", stunStateData, this);
        deadState = new Fly_DeadState(this, stateMachine, "dead", deadStateData, this);
        dodgeState = new Fly_DodgeState(this, stateMachine, "dodge", dodgeStateData, this);
        rangedAttackState = new Fly_RangedAttackState(this, stateMachine, "rangedAttack", rangedAttackPosition, rangedAttackStateData, this);

        stateMachine.Initialize(moveState);
    }

    public override void Damage(AttackDetails attackDetails)
    {
        base.Damage(attackDetails);

        if (isDead)
        {
            stateMachine.ChangeState(deadState);
        }
        else if(isStunned && stateMachine.currentState != stunState)
        {
            stateMachine.ChangeState(stunState);
        }
        else if (CheckPlayerInMinAgroRange())
        {
            stateMachine.ChangeState(rangedAttackState);
        }
        else if (!CheckPlayerInMinAgroRange() && stateMachine.currentState != meleeAttackState)
        {
            lookForPlayerState.SetTurnImmediately(true);
            stateMachine.ChangeState(lookForPlayerState);
        }
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawWireSphere(meleeAttackPosition.position, meleeAttackStateData.attackRadius);
    }
}
