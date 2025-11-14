using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class Movement : MonoBehaviour
{
    [Header("Stats")]
    public int defaultSpeed = 20;
    public int sprintSpeed = 50;
    public int slideForce = 25;
    public float slideTime = 5;
    public int defaultHealth = 5;
    public int jumpForce = 10;

    int health;
    int speed;
    float slideTimer;
    float jumpTimer;

    [Header("Drag")]
    public LayerMask ground;
    public float groundDrag = 2f;
    public float airResistance = 10f;
    
    Vector3 moveVector;

    //Action bools
    bool moving;
    bool sprinting;
    bool grounded;
    bool jumping;
    bool falling;
    bool shooting;
    bool sliding;
    bool facingRight;

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
        rb.freezeRotation = true;
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

        if (state == MoveState.inAir)
        {
            if (rb.linearVelocity.y <= 0)
            {
                EndJump();
            }
        }

        moving = (moveVector != Vector3.zero && state != MoveState.inAir && state != MoveState.sliding);
        grounded = Physics.Raycast(transform.position+Vector3.up, Vector3.down, 1f, ground);
        StateHandler();


        a.SetBool("Grounded", grounded);
        a.SetBool("Moving", moving);
        a.SetBool("Running", sprinting);
        a.SetBool("Jumping", jumping);
        a.SetBool("Sliding", sliding);
        a.SetBool("Falling", falling);

        if (state == MoveState.inAir)
            rb.AddForce(moveVector.normalized * speed*airResistance, ForceMode.Force);
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
        if (state != MoveState.sliding)
        {
            Vector2 moveVec = moveVal.Get<Vector2>();
            moveVector = new Vector3(moveVec.x, 0, moveVec.y);

            if (moveVector.x < 0) facingRight = true;
            else if (moveVector.x > 0) facingRight = false;
            GetComponentInChildren<SpriteRenderer>().flipX = facingRight;
        }
    }

    public void OnSprint()
    {
        sprinting = !sprinting;
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
            rb.mass = 0.5f;
            a.SetTrigger("Slide");
            sliding = true;
            slideTimer = slideTime;
            if (moving) rb.AddForce(moveVector*slideForce, ForceMode.Impulse);
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

                if (sprinting)
                {
                    state = MoveState.sprinting;
                    speed = sprintSpeed;
                }
            }

            if (sliding)
            {
                moving = false;
                speed = 0;
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
}
