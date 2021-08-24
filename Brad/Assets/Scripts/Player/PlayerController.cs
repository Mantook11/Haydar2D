using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius = 2f;
    public float wallCheckDistance = 1f;
    public float wallSlideSpeed = 5.0f;
    public float movementForceInAir;
    public float airDragMult = 0.95f;
    public float variableJumpHeightMult = 0.5f;
    public float wallJumpForce;
    public float wallHopForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.5f;
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCooldown;
    public float hookCooldown;
    public float maxHookDist;
    public float hookSpeedX;
    public float hookSpeedY;
    public float hookLiftOffTime;
    public float hookOffsetX, hookOffsetY;

    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    public LayerMask whatIsGround;

    public Vector2 wallHopDir;
    public Vector2 wallJumpDir;

    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ceilingCheck;
    public Transform ledgeCheck;

    private bool canStartDialogue;
    private bool isHooking = false;
    private bool isDashing = false;
    private bool canClimbLedge = false;
    private bool ledgeDetected;
    private bool isTouchingLedge;
    private bool isTouchingCeiling;
    private bool hasWallJumped;
    private bool canMove;
    private bool canFlip;
    private bool checkJumpMultiplier;
    private bool isAttemptingToJump;
    private bool isWallSliding;
    private bool isTouchingWall;
    private bool isHiding = false;
    private bool canHide;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isWalking;
    private bool startDialogueASAP;
    private bool disabledInput;
    private bool knockback;

    [SerializeField]
    private Vector2 knockbackSpeed;
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;
    private Vector2 hookDestination;

    private int facingDir = 1;
    private int lastWallJumpDir;

    private float movementDirection;
    private float knockbackStartTime;
    [SerializeField]
    private float knockbackDuration;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    private float dashTimeLeft;
    private float lastImageXpos;
    private float lastDash = Mathf.NegativeInfinity;
    private float lastHook = Mathf.NegativeInfinity;

    [SerializeField]
    private GameObject hookParticle;
    [SerializeField]
    private GameObject hook;
    private GameObject realHook;
    private GameObject dialogueZone;

    [SerializeField]
    private CinemachineVirtualCamera cam;

    private Rigidbody2D rb;
    private Animator anim;
    private LineRenderer lr;

    private PlayerCombatController pcc;

    public float camZoomLoss;

    // Start is called before the first frame update
    void Start()
    {
        pcc = GetComponent<PlayerCombatController>();
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        wallHopDir.Normalize();
        wallJumpDir.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        if (!disabledInput)
        {
            CheckInput();
            CheckJump();
            CorrectCamZoom();
        }
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfCanHide();
        CheckIfWallSliding();
        CheckLedgeClimb();
        CheckDash();
        CheckHooking();
        CheckDialogue();
        CheckKnockback();
    }

    private void FixedUpdate()
    {
        if(!disabledInput) ApplyMovement();
        CheckSurroundings();
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isHiding", isHiding);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    public void Knockback(int direction)
    {
        knockback = true;
        knockbackStartTime = Time.time;
        rb.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);
    }

    private void CheckKnockback()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
        }
    }

    public bool GetDashStatus()
    {
        return isDashing;
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && movementDirection > 0)
        {
            Flip();
        }

        if (Mathf.Abs(rb.velocity.x) >= 0.01f)
        {
            isWalking = true;
        }
        else { isWalking = false; }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y <= 0.01f)
        {
            canNormalJump = true;
        }
        else { canNormalJump = false; }

        if (isTouchingWall)
        {
            canWallJump = true;
        }
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingCeiling = Physics2D.Raycast(ceilingCheck.position, transform.up, wallCheckDistance, whatIsGround);
        if (isTouchingWall && !isTouchingLedge && !ledgeDetected && !isTouchingCeiling)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }

    private void CheckLedgeClimb()
    {
        if (ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if (isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            anim.SetBool("canClimbLedge", canClimbLedge);
        }
        if (canClimbLedge)
        {
            transform.position = ledgePos1;
        }
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && movementDirection == facingDir && rb.velocity.y < 0 && !canClimbLedge)
        {
            isWallSliding = true;
        }
        else { isWallSliding = false; }
    }

    private void CheckIfCanHide()
    {
        if (isGrounded && rb.velocity.y == 0 && rb.velocity.x == 0)
        {
            canHide = true;
        }
        else { canHide = false; isHiding = false; }
    }

    private void CheckHooking()
    {
        if (isHooking)
        {
            canMove = false;

            int hookDirection = Mathf.FloorToInt(Mathf.Sign(hookDestination.x - wallCheck.position.x));

            Vector2 dir = (hookDestination - new Vector2(wallCheck.position.x, wallCheck.position.y + 0.3f)).normalized;
            if (hookDirection != facingDir && !isTouchingWall)
            {
                Flip();
                canFlip = false;
            }

            rb.gravityScale = 0f;

            rb.velocity = new Vector2(rb.velocity.x * 0.7f + hookSpeedX * dir.x, rb.velocity.y / 2 + hookSpeedY * dir.y);

            lr.SetPosition(0, wallCheck.position);
            lr.SetPosition(1, hookDestination);

            if ((Time.time >= lastHook + hookLiftOffTime) && (isTouchingWall || isTouchingLedge || isTouchingCeiling || isGrounded))
            {
                lr.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                rb.gravityScale = 4f;
                isHooking = false;
                canMove = true;
                canFlip = true;
            }
        }
    }

    private void CheckDash()
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {

                canMove = false;
                canFlip = false;
                rb.velocity = new Vector2(dashSpeed * facingDir, 0.0f);
                dashTimeLeft -= Time.deltaTime;

                if (Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
            }
            if (dashTimeLeft <= 0 || isTouchingWall || isHooking)
            {
                rb.velocity = new Vector2(0.0f, rb.velocity.y);
                isDashing = false;
                canMove = true;
                canFlip = true;
            }
        }
    }

    private void CheckInput()
    {
        movementDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || (canNormalJump && isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if (!isGrounded && movementDirection != facingDir)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (turnTimer >= 0f)
        {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMult);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (Time.time >= (lastDash + dashCooldown))
            {
                AttemptToDash();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (Time.time >= (lastHook + hookCooldown) && !isHooking)
            {
                AttemptToHook();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (canStartDialogue)
            {
                rb.velocity = Vector2.zero;
                DisablePlayerControl();
                dialogueZone.SendMessage("StartDialogue");
                canStartDialogue = false;
            }
        }

        if (rb.velocity.x == 0 && rb.velocity.y == 0 && Input.GetKeyDown(KeyCode.F))
        {
            Hide();
        }
    }

    private void AttemptToHook()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = worldPosition;
        Vector2 start = wallCheck.position;

        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, (mousePos - start).normalized, maxHookDist, whatIsGround);
        if (hit != false)
        {
            if (realHook != null)
            {
                Destroy(realHook);
            }
            hookDestination = hit.point;
            realHook = Instantiate(hook, new Vector2(hit.point.x + (hookOffsetX * hit.normal.x), hit.point.y + (hookOffsetY * hit.normal.y)), Quaternion.Euler(0.0f, 0.0f, 0.0f));
            Instantiate(hookParticle, new Vector2(hit.point.x, hit.point.y), Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
            realHook.transform.right = (hit.point - start).normalized;
            isHooking = true;
            lastHook = Time.time;
        }

    }

    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    private void ApplyMovement()
    {
        //AirDrag
        if (!isGrounded && !isWallSliding && movementDirection == 0 && !knockback)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMult, rb.velocity.y);
        }

        //Running
        else if (canMove && !knockback)
        {
            rb.velocity = new Vector2(movementSpeed * movementDirection * Time.fixedDeltaTime, rb.velocity.y);
        }

        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed * Time.fixedDeltaTime);
            }
        }

    }

    private void DisableFlip()
    {
        canFlip = false;
    }

    private void EnableFlip()
    {
        canFlip = true;
    }

    private void Flip()
    {
        if (!isWallSliding && canFlip && !knockback)
        {
            facingDir *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    private void Hide()
    {
        if (canHide)
        {
            isHiding = !isHiding;
        }
    }


    private void CheckJump()
    {
        if (jumpTimer > 0)
        {
            //WallJump
            if (!isGrounded && isTouchingWall && movementDirection != 0 && movementDirection != facingDir)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }
        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }
        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && movementDirection == -lastWallJumpDir)
            {
                rb.velocity = new Vector2(-rb.velocity.x * 0.8f, -10f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
    }

    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.fixedDeltaTime);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            canNormalJump = false;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDir.x * movementDirection, wallJumpForce * wallJumpDir.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDir = -facingDir;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Dialogue"))
        {
            dialogueZone = collision.gameObject;
            bool onInput = collision.gameObject.GetComponent<DialogueZoneController>().onInput;

            if (onInput)
            {
                canStartDialogue = true;
            }
            else if(!onInput && ledgeDetected)
            {
                startDialogueASAP = true;
            }
            else
            {
                DisablePlayerControl();
                dialogueZone.SendMessage("StartDialogue");
            }
        }
    }

    private void CorrectCamZoom()
    {
        float size = cam.m_Lens.OrthographicSize;
        if (size <= 6.532f)
        {
            size += camZoomLoss * Time.deltaTime;
            if(size >= 6.532f)
            {
                size = 6.532f;
            }
            cam.m_Lens.OrthographicSize = size;
        }
    }

    private void CheckDialogue()
    {
        if (dialogueZone != null && startDialogueASAP && !ledgeDetected)
        {
            startDialogueASAP = false;
            DisablePlayerControl();
            dialogueZone.SendMessage("StartDialogue");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Dialogue"))
        {
            dialogueZone = null;
            canStartDialogue = false;
        }
    }

    public void DisablePlayerControl()
    {
        disabledInput = true;
        canMove = false;
        canFlip = false;
        pcc.DisableCombat();
        rb.velocity = Vector2.zero;
    }

    public void EnablePlayerControl()
    {
        pcc.EnableCombat();
        disabledInput = false;
        canMove = true;
        canFlip = true;
    }

    public int GetFacingDir()
    {
        return facingDir;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
        Gizmos.DrawLine(ledgeCheck.position, new Vector3(ledgeCheck.position.x + wallCheckDistance, ledgeCheck.position.y, ledgeCheck.position.z));
        Gizmos.DrawLine(ceilingCheck.position, new Vector3(ceilingCheck.position.x, ceilingCheck.position.y + wallCheckDistance, wallCheck.position.z));
        Gizmos.DrawLine(ledgePos1, ledgePos2);
        Gizmos.DrawWireSphere(wallCheck.position, maxHookDist);
    }
}
