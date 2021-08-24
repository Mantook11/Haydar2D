using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : MonoBehaviour
{
    [SerializeField]
    private float maxHealth, knockbackSpeedX, knockbackSpeedY, knockbackDuration, knockbackDeathSpeedX, knockbackDeathSpeedY, deathTorque;
    private float currentHealth, knockbackStart;

    private int playerFacingDir;

    [SerializeField]
    private bool applyKnockback;
    private bool playerOnLeft, knockback;

    [SerializeField]
    private GameObject hitParticle;

    private PlayerController pc;
    private GameObject aliveGO, topGO, bottomGO;
    private Rigidbody2D aliveRB, topRB, bottomRB;
    private Animator aliveAnim;

    private void Start()
    {
        currentHealth = maxHealth;

        pc = GameObject.Find("Player").GetComponent<PlayerController>();

        aliveGO = transform.Find("Alive").gameObject;
        topGO = transform.Find("Broken Top").gameObject;
        bottomGO = transform.Find("Broken Bottom").gameObject;

        aliveAnim = aliveGO.GetComponent<Animator>();
        aliveRB = aliveGO.GetComponent<Rigidbody2D>();
        topRB = topGO.GetComponent<Rigidbody2D>();
        bottomRB = bottomGO.GetComponent<Rigidbody2D>();

        aliveGO.SetActive(true);
        bottomGO.SetActive(false);
        topGO.SetActive(false);

    }

    private void Update()
    {
        CheckKnockback();
    }

    private void Damage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;
        playerFacingDir = pc.GetFacingDir();

        Instantiate(hitParticle, aliveGO.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if(playerFacingDir == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }
        aliveAnim.SetBool("playerOnLeft", playerOnLeft);
        aliveAnim.SetTrigger("damage");

        if(applyKnockback && currentHealth > 0.0f)
        {
            Knockback();
        }
        if(currentHealth <= 0.0f)
        {
            Die();
        }
    }

    private void Knockback()
    {
        knockback = true;
        knockbackStart = Time.time;
        aliveRB.velocity = new Vector2(knockbackSpeedX * playerFacingDir, knockbackSpeedY);
    }

    private void CheckKnockback()
    {
        if(Time.time >= knockbackStart + knockbackDuration && knockback)
        {
            knockback = false;
            aliveRB.velocity = new Vector2(0.0f, aliveRB.velocity.y);
        }
    }

    private void Die()
    {
        aliveGO.SetActive(false);
        topGO.SetActive(true);
        bottomGO.SetActive(true);

        topGO.transform.position = aliveGO.transform.position;
        bottomGO.transform.position = aliveGO.transform.position;

        bottomRB.velocity = new Vector2(knockbackSpeedX * playerFacingDir, knockbackSpeedY);
        topRB.velocity = new Vector2(knockbackDeathSpeedX * playerFacingDir, knockbackDeathSpeedY);
        topRB.AddTorque(deathTorque * -playerFacingDir, ForceMode2D.Impulse);
    }

}
