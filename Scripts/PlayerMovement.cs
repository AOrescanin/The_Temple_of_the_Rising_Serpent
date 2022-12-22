using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private CapsuleCollider2D playerCollider;
    private Animator playerAnimator;
    

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Run")]
    //[SerializeField] bool canMove = true;
    [SerializeField] private bool isMoving;
    [SerializeField] private float maxSpeed = 9f;
    private Vector2 movementInput;
    private float targetSpeed;
    private float speedDif;
    private float movement;

    [Header("Acceleration/Deceleration")]
    [SerializeField] private float timeZeroToMax = 0.9f;
    [SerializeField] private float frictionAmount = 0.9f;
    private float accelRatePerSec;

    [Header("Jump")]
    [SerializeField] private bool isJumping;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallMuliplier = 2.7f;
    [SerializeField] private float lowJumpMultiplier = 6f;

    [Header("Wall Jump/Slide")]
    [SerializeField] private bool wallJumped;
    [SerializeField] private bool isWallSliding;
    [SerializeField] private float wallJumpTime = 0.15f;
    [SerializeField] private float WallSlideSpeed = -1.5f;
    [SerializeField] private float wallDistance = 0.3f;
    // [SerializeField] float wallJumpLerp = 9f;
    private float jumpTime;
    private bool isWallSlidingLastFrame = false;

    [Header("Dash")]
    [SerializeField] private bool canDash = true;
    [SerializeField] public bool isDashing = false;
    [SerializeField] private float dashingForce = 24f;
    [SerializeField] public float dashingTime = 0.2f;
    [SerializeField] public float dashingCooldown = 1f;
    [SerializeField] private TrailRenderer tr;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Collision")]
    [SerializeField] private bool onGround;
    [SerializeField] private bool isFacingRight;
    [SerializeField] private bool jumpBufferIsTouching;
    [SerializeField] private float collisionRadius = 0.12f;
    [SerializeField] private Vector2 bottomOffset, rightOffset, leftOffset, boxDimension;
    [SerializeField] private RaycastHit2D wallCheckHit;
    private bool onGroundLastFrame = false;

    [Header("Death")]
    [SerializeField] float respawnDelay = 1.5f;
    public bool isAlive = true;
    private Vector3 respawnPoint;

    [Header("SFX")]
    [SerializeField] AudioSource footstepSound;
    [SerializeField] AudioSource jumpSound;
    [SerializeField] AudioSource landSound;
    [SerializeField] AudioSource dashSound;
    [SerializeField] AudioSource dashRechargeSound;
    [SerializeField] AudioSource wallSlideSound;
    [SerializeField] AudioSource deathSound;
    [SerializeField] AudioSource respawnSound;

    [Header("Particles")]
    [SerializeField] ParticleSystem dust;
    [SerializeField] ParticleSystem jumpDust;
    [SerializeField] ParticleSystem deathDust;
    [SerializeField] TrailRenderer dashTrail;
    

    void Awake() 
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        playerAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        accelRatePerSec = maxSpeed / timeZeroToMax;
        respawnPoint = transform.position;
        respawnPoint.y += .21f;
    }

    void Update()
    {
        if(!isAlive || isDashing) {return;}

        CoyoteTime();
        CheckWallSlide();
        CheckFacing();
        FlipSprite();
        JumpAnimations();
        SoundEffects();
    }

    void FixedUpdate() 
    {
        if(!isAlive || isDashing) {return;}

        Run();
        BetterJump();
        Collisions();

        if(isWallSliding)
        {
            WallSlide();
        }
    }

    // input handler
    void OnMove(InputValue inputValue)
    {
        if(!isAlive || isDashing) {return;}

        movementInput = inputValue.Get<Vector2>();
    }

    void OnJump(InputValue inputValue)
    {
        if(!isAlive || isDashing) {return;}

        float force = jumpForce;

        // this will normalize the jump if the player jumps while falling due to coyote time
		if (rb.velocity.y < 0)
        {
			force -= rb.velocity.y;
        }

        // jump input that implements coyote time(allows player to jump a little bit after leaving the ground)
        // and a jump buffer(allows player to jump a little bit before they hit the ground), 
        // also handles wall jumps
        if(inputValue.isPressed && (coyoteTimeCounter > 0 || jumpBufferIsTouching) && rb.velocity.y <= 0.1f)
        {
            coyoteTimeCounter = 0;
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
            jumpSound.Play();
            jumpDust.Play();
        }
        else if(inputValue.isPressed && isWallSliding && !wallJumped)
        {
            rb.AddForce(Vector2.up * force * 1.2f, ForceMode2D.Impulse);
            wallJumped = true;
            jumpSound.Play();
            jumpDust.Play();
        }
    }

    void OnDash()
    {
        if(!isAlive) {return;}

        if(canDash)
        {
            dashTrail.emitting = true;
            StartCoroutine(Dash());
            dashSound.Play();
        }
    }

    void OnCollisionEnter2D(Collision2D other) 
    {
        // prevents player from wall jumping on the same wall without leaving it first
        if(!onGround)
        {
            wallJumped = false;
        }

        // makes the player stick to moving platforms
        if(other.gameObject.CompareTag("Platforms"))
        {
            transform.parent = other.gameObject.transform;
        }
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        // resets player after leaving moving platforms
        if(other.gameObject.CompareTag("Platforms"))
        {
            transform.parent = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(!isAlive) {return;}

        if(other.gameObject.CompareTag("Checkpoint"))
        {
            respawnPoint = transform.position;
            respawnPoint.y += .21f;
        }

        if(other.gameObject.CompareTag("Platforms"))
        {
            jumpDust.Play();
        }

        if(other.gameObject.CompareTag("Hazards"))
        {
            Die();
        }

        if(other.gameObject.CompareTag("Exit"))
        {
            LvlUp();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position  + bottomOffset, collisionRadius);
        Gizmos.DrawWireCube(transform.position + (Vector3)bottomOffset, boxDimension);
    }

    void Run()
    {
        isMoving = Mathf.Abs(movementInput.x) > 0;
        playerAnimator.SetBool("isRunning", isMoving);
        
        // artificial friction to stop player from sliding
        if(!isMoving)
        {
            float friction = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            friction *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -friction, ForceMode2D.Impulse);
        }

        // adds force to player based on their velocity to have smooth acceleration
        targetSpeed = maxSpeed * movementInput.x;
        speedDif = targetSpeed - rb.velocity.x;
        movement = speedDif * accelRatePerSec;

        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    // smoothens jump curve and allows variable jump heights
    void BetterJump()
    {
        if(rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMuliplier - 1) * Time.deltaTime;
        }
        else if(rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

    }

    // handles dash
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingForce, 0f);
        yield return new WaitForSeconds(dashingTime);
        // getting movement input again prevents input collected at the start of the dash effecting player
        movementInput.x = Input.GetAxisRaw("Horizontal");
        rb.gravityScale = originalGravity;
        isDashing = false;
        dashTrail.emitting = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        dashRechargeSound.Play();
    }

    // collision handler
    void Collisions()
    {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);
        jumpBufferIsTouching = Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, boxDimension, 0f, groundLayer);

        if(isFacingRight)
        {
            wallCheckHit = Physics2D.Raycast((Vector2)transform.position + rightOffset, 
                                             new Vector2(wallDistance, 0), wallDistance, groundLayer);
            Debug.DrawRay((Vector2)transform.position + rightOffset, new Vector2(wallDistance, 0), Color.blue);
        }
        else
        {
            wallCheckHit = Physics2D.Raycast((Vector2)transform.position + leftOffset, 
                                             new Vector2(-wallDistance, 0), wallDistance, groundLayer);
            Debug.DrawRay((Vector2)transform.position + leftOffset, new Vector2(-wallDistance, 0), Color.blue);
        }
    }

    // timer to coyote time(allows player to jump a little bit after leaving the ground)
    void CoyoteTime()
    {
        if(onGround)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    // checks whether wallsliding should be enabled
    void CheckWallSlide()
    {
        if(wallCheckHit && !onGround && movementInput.x !=0)
        {
            isWallSliding = true;
            jumpTime = Time.time + wallJumpTime;
        }
        else if(jumpTime < Time.time)
        {
            isWallSliding = false;
        }
    }

    // reduces player y velocity if they are on the wall to simulate a slide
    void WallSlide()
    {
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, WallSlideSpeed, float.MaxValue));
        dust.Play();
    }

    void CheckFacing()
    {
        if(movementInput.x < 0)
        {
            isFacingRight = false;
        }
        else
        {
            isFacingRight = true;
        }
    }

    void FlipSprite()
    {
        if(movementInput.x < 0)
        {
            transform.localScale = new Vector2(-1f, 1f);
        }

        if(movementInput.x > 0)
        {
            transform.localScale = new Vector2(1f, 1f);
        }
    }

    void JumpAnimations()
    {
        if(onGround)
        {
            playerAnimator.SetBool("isJumping", false);
            playerAnimator.SetBool("isFalling", false);
            wallJumped = false;
        }

        if(rb.velocity.y > 0 && !onGround)
        {
            playerAnimator.SetBool("isJumping", true);
        }

        if(rb.velocity.y < 0  && !onGround)
        {
            playerAnimator.SetBool("isJumping", false);
            playerAnimator.SetBool("isFalling", true);
        }
    }

    void SoundEffects()
    {
        if (isMoving && onGround)
        {
            footstepSound.enabled = true;
        }
        else
        {
            footstepSound.enabled = false;
        }

        if((onGround && !onGroundLastFrame) || (isWallSliding && !isWallSlidingLastFrame))
        {
            landSound.Play();
        }
        onGroundLastFrame = onGround;
        isWallSlidingLastFrame = isWallSliding;

        if (isWallSliding && rb.velocity.y < 0)
        {
            wallSlideSound.enabled = true;
        }
        else
        {
            wallSlideSound.enabled = false;
        }
    }
    void Die()
    {
        deathDust.Play();
        deathSound.Play();
        isAlive = false;
        playerAnimator.SetBool("isJumping", false);
        playerAnimator.SetBool("isFalling", false);
        playerAnimator.SetTrigger("Dying");
        RespawnPlayer();
    }

     public void RespawnPlayer()
    {   
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        respawnSound.enabled = true;
        yield return new WaitForSecondsRealtime(respawnDelay);
        isAlive = true;
        playerAnimator.ResetTrigger("Dying");
        movementInput.x = 0;
        isMoving = false;
        transform.position = respawnPoint;
        respawnSound.enabled = false;
    }

    void LvlUp()
    {
        isAlive = false;
        playerAnimator.SetBool("isJumping", false);
        playerAnimator.SetBool("isFalling", false);
        playerAnimator.SetTrigger("Meditating");
    }
}
