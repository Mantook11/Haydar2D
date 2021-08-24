using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{

    [SerializeField]
    private bool combatEnabled;
    private bool gotInput;
    private bool isAttacking;
    private bool isFirstAttack;

    [SerializeField]
    private float inputTimer, attackRadius, attackDamage;
    [SerializeField]
    private float stunDamageAmount = 1f;
    private float lastInputTime = Mathf.NegativeInfinity;

    private AttackDetails attackDetails;

    [SerializeField]
    private Transform attackHitBoxPos;

    [SerializeField]
    private LayerMask whatIsDamageable;

    private PlayerController pc;
    private PlayerStats ps;

    private Animator anim;

    private void Start()
    {
        ps = GetComponent<PlayerStats>();
        pc = GetComponent<PlayerController>();
        anim = GetComponent<Animator>();
        anim.SetBool("canAttack", combatEnabled);
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (combatEnabled)
            {
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
            }
        }

        if (Time.time >= lastInputTime + inputTimer)
        {
            gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attackHitBoxPos.position, attackRadius, whatIsDamageable);

        attackDetails.damageAmount = attackDamage;
        attackDetails.position = transform.position;
        attackDetails.stunDamageAmount = stunDamageAmount;

        foreach (Collider2D collider in detectedObjects)
        {
            if(collider.transform.tag.Equals("Object"))
            {
                collider.transform.SendMessage("Damage", attackDamage);
            }
            else
            {
                collider.transform.parent.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void FinishAttack()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    public void DisableCombat()
    {
        combatEnabled = false;
    }

    public void EnableCombat()
    {
        combatEnabled = true;
    }

    private void Damage(AttackDetails attackDetails)
    {
        if (!pc.GetDashStatus())
        {
            int direction;

            ps.DecreaseHealth(attackDetails.damageAmount);

            if(attackDetails.position.x < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }

            pc.Knockback(direction);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackHitBoxPos.position, attackRadius);
    }

}
