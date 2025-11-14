using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class Movement : MonoBehaviour
{
    public GameObject orient;
    [Header("Stats")]
    public int defaultSpeed = 20;
    public int sprintSpeed = 50;
    public int defaultHealth = 5;
    public int jumpForce = 10;

    int health;
    int speed;
    float jumpTimer;

    [Header("Sliding")]
    public int slideSpeed = 20;
    public int slideForce = 25;
    public float slideTime = 5;
    float slideTimer;
    float invcTimer;
    public float invincibilityTime = 2;
    Vector3 slideDir;

    [Header("Ground")]
    public GameObject Camera;
    public Vector3 standardOffeset;


    [Header("Ground")]
    public float raycastDist;
    public LayerMask ground;
    public float groundDrag = 2f;
    public float airResistance = 10f;
    
    Vector3 moveVector;

    //Action bools
    bool moving,sprinting,grounded,jumping,falling,shooting,sliding,aiming,facingRight,invincible;

    Animator a;
    Rigidbody rb;

    public MoveState state;

    public enum MoveState
    {
        idle,
        walking,
        sprinting,
        sliding,
        inAir
    }
    private void Start()
    {
        a = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        //rb.freezeRotation = true;
        health = defaultHealth;
    }
    private void FixedUpdate()
    {
        if (state == MoveState.sliding)
        {
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }

        invcTimer -= Time.fixedDeltaTime;
        if (invcTimer <= 0)
        {
            invincible = false;
        }

        if (state == MoveState.inAir)
        {
            if (rb.linearVelocity.y <= 0)
            {
                EndJump();
            }
        }

        moving = (moveVector != Vector3.zero && state != MoveState.inAir && state != MoveState.sliding);
        grounded = Physics.Raycast(transform.position+Vector3.up, Vector3.down, raycastDist, ground);
        StateHandler();


        a.SetBool("Grounded", grounded);
        a.SetBool("Moving", moving);
        a.SetBool("Running", sprinting);
        a.SetBool("Jumping", jumping);
        a.SetBool("Sliding", sliding);
        a.SetBool("Falling", falling);
        a.SetBool("Aiming", aiming);

        if (state == MoveState.inAir)
            rb.AddForce(moveVector.normalized * speed*airResistance, ForceMode.Force);
        else if (state == MoveState.sliding)
            rb.AddForce(slideDir.normalized * speed, ForceMode.Force);
        else
            rb.AddForce(moveVector.normalized * speed, ForceMode.Force);


        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    //basic movement input
    public void OnMove(InputValue moveVal)
    {
        Vector2 moveVec = moveVal.Get<Vector2>();
        moveVector = new Vector3(moveVec.x, 0, moveVec.y);

        if (moveVector.x < 0) facingRight = true;
        else if (moveVector.x > 0) facingRight = false;
        GetComponentInChildren<SpriteRenderer>().flipX = facingRight;
    }

    public void OnSprint()
    {
        sprinting = !sprinting;
    }

    public void OnAim()
    {
        aiming = !aiming;
    }

    public void OnJump()
    {
        if (state != MoveState.inAir)
        {
            transform.position += (Vector3.up * 0.1f);
            a.SetTrigger("Jump");
            jumping = true;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
    }

    public void OnSlide()
    {
        if (state != MoveState.inAir && state != MoveState.sliding)
        {
            slideDir = moveVector;
            rb.mass = 0.5f;
            a.SetTrigger("Slide");
            sliding = true;
            slideTimer = slideTime;
            invcTimer = invincibilityTime;
            if (moving) rb.AddForce(moveVector*slideForce, ForceMode.Impulse);
            else rb.AddForce(orient.transform.forward * slideForce, ForceMode.Impulse);
        }
    }

    void StateHandler()
    {
        if (grounded)
        {
            state = MoveState.idle;
            falling = false;

            if (moving)
            {
                state = MoveState.walking;
                speed = defaultSpeed;

                if (aiming)
                {
                    speed /= 2;
                }

                if (sprinting)
                {
                    state = MoveState.sprinting;
                    speed = sprintSpeed;
                }
            }

            if (sliding)
            {
                moving = false;
                speed = slideSpeed;
                state = MoveState.sliding;
            }
        }
        else
        {
            moving = false;
            state = MoveState.inAir;
        }

    }

    void EndSlide()
    {
        rb.mass = 1f;
        a.SetTrigger("End Slide");
        sliding = false;
    }

    void EndJump()
    {
        a.SetTrigger("End Jump");
        jumping = false;
        falling = true;
    }

    public void TakeDamage(int damage)
    {
        if (!invincible)
            health--;
    }
}
