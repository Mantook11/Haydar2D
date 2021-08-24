using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookController : MonoBehaviour
{
    //Explode if right click pressed and boost again;

    public float knockbackSpeedX, knockbackSpeedY, knockbackDuration;
    public float explodeForceX = 10f;
    public float explodeForceY = 10f;

    private Rigidbody2D rb;
    private PlayerController pc;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }


    // Update is called once per frame
    void Update()
    {
    }
    private void Damage(float damage)
    {
        rb.isKinematic = false;
        Knockback();
    }

    private void Knockback()
    {
        rb.velocity = new Vector2(knockbackSpeedX * pc.GetFacingDir(), knockbackSpeedY);
    }

    private void Die()
    {
        Destroy(gameObject);
        //anim.SetTrigger("explode");
    }

    public void FinishAnim()
    {
        Destroy(gameObject);
    }
}
