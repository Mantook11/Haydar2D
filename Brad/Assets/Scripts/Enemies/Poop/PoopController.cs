using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoopController : MonoBehaviour
{

    private enum State
    {
        Walking,
        Knockback,
        Dead
    }

    private State currentState;

    private bool groundDetected, wallDetected;

    private int damageDirection;
    private int facingDir = 1;

    [SerializeField]
    private float groundCheckDistance, wallCheckDistance, maxHealth, knockbackDuration, dashCooldown;
    [SerializeField]
    private float touchDamageCooldown, touchDamage, touchDamageWidth, touchDamageHeight;
    private float currentHealth, knockbackStartTime, lastTouchDamageTime;
    private float dashStartTime = Mathf.NegativeInfinity;

    private float[] attackDetails = new float[2];

    [SerializeField]
    private Transform groundCheck, wallCheck, touchDamageCheck;

    [SerializeField]
    private Vector2 knockbackSpeed, moveForceMax, moveForceMin;
    private Vector2 movement = Vector2.one;
    private Vector2 touchDamageTopRight, touchDamageBotLeft;

    [SerializeField]
    private LayerMask whatIsGround, whatIsPlayer;

    [SerializeField]
    private GameObject hitParticle, deathChunkParticle, deathBloodParticle;

    private Animator aliveAnim;
    private GameObject alive;
    private Rigidbody2D aliveRb;

    private void Start()
    {
        currentHealth = maxHealth;
        alive = transform.Find("Alive").gameObject;
        aliveAnim = alive.GetComponent<Animator>();
        aliveRb = alive.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Walking:
                UpdateWalkingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    // WALKING
    private void EnterWalkingState()
    {

    }

    private void UpdateWalkingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, alive.transform.right, wallCheckDistance, whatIsGround);

        CheckTouchDamage();

        if(wallDetected)
        {
            Flip();
        }
        else
        {
            if (Time.time >= dashStartTime + dashCooldown)
            {
                dashStartTime = Time.time + Random.Range(0.0f, 0.5f);
                Dash();
            }
        }
    }

    private void ExitWalkingState()
    {

    }

    // KNOCKBACK
    private void EnterKnockbackState()
    {
        knockbackStartTime = Time.time;
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        aliveRb.velocity = movement;
        aliveAnim.SetBool("knockback", true);
    }

    private void UpdateKnockbackState()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration)
        {
            SwitchState(State.Walking);
        }
    }

    private void ExitKnockbackState()
    {
        aliveAnim.SetBool("knockback", false);
    }

    // DEAD
    private void EnterDeadState()
    {
        Instantiate(deathChunkParticle, alive.transform.position, deathChunkParticle.transform.rotation);
        Instantiate(deathBloodParticle, alive.transform.position, deathBloodParticle.transform.rotation);
        Destroy(gameObject);
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // OTHER

    private void Damage(float[] attackDetails)
    {
        currentHealth -= attackDetails[0];

        Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if(attackDetails[1] > alive.transform.position.x)
        {
            damageDirection = -1;
        }
        else
        {
            damageDirection = 1;
        }

        if(currentHealth > 0.0f)
        {
            SwitchState(State.Knockback);
        }else if(currentHealth <= 0.0f)
        {
            SwitchState(State.Dead);
        }
    }

    private void CheckTouchDamage()
    {
        if(Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {
            touchDamageBotLeft.Set(touchDamageCheck.position.x - touchDamageWidth / 2, touchDamageCheck.position.y - touchDamageHeight / 2);
            touchDamageTopRight.Set(touchDamageCheck.position.x + touchDamageWidth / 2, touchDamageCheck.position.y + touchDamageHeight / 2);

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, whatIsPlayer);
            
            if(hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails[0] = touchDamage;
                attackDetails[1] = alive.transform.position.x;
                hit.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void Flip()
    {
        facingDir *= -1;
        alive.transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void Dash()
    {
        aliveAnim.SetTrigger("dash");
        Vector2 forceToAdd = new Vector2(Random.Range(moveForceMin.x, moveForceMax.x) * facingDir, Random.Range(moveForceMin.y, moveForceMax.y));
        aliveRb.AddForce(forceToAdd, ForceMode2D.Impulse);
    }

    private void SwitchState(State to)
    {
        switch (currentState)
        {
            case State.Walking:
                ExitWalkingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
        }

        switch (to)
        {
            case State.Walking:
                EnterWalkingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
        }
        currentState = to;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
        Vector2 bottomLeft = new Vector2(touchDamageCheck.position.x - touchDamageWidth / 2, touchDamageCheck.position.y - touchDamageHeight / 2);
        Vector2 bottomRight = new Vector2(touchDamageCheck.position.x + touchDamageWidth / 2, touchDamageCheck.position.y - touchDamageHeight / 2);
        Vector2 topRight = new Vector2(touchDamageCheck.position.x + touchDamageWidth / 2, touchDamageCheck.position.y + touchDamageHeight / 2);
        Vector2 topLeft = new Vector2(touchDamageCheck.position.x - touchDamageWidth / 2, touchDamageCheck.position.y + touchDamageHeight / 2);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

}
