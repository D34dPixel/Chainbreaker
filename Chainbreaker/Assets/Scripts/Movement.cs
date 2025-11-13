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
    
    Vector2 moveVector;

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
            slideTimer -= Time.deltaTime;
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

        
        grounded = Physics.Raycast(transform.position+Vector3.up, Vector3.down, 2.5f, ground);

        moving = (moveVector != Vector2.zero && state != MoveState.sliding && state != MoveState.inAir);
        StateHandler();


        a.SetBool("Grounded", grounded);
        a.SetBool("Moving", moving);
        a.SetBool("Running", sprinting);
        a.SetBool("Jumping", jumping);
        a.SetBool("Sliding", sliding);
        a.SetBool("Falling", falling);

        //if (grounded)
        rb.AddForce(moveVector.normalized * speed, ForceMode.Force);


        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    //basic movement input
    public void OnMove(InputValue moveVal)
    {
        if (grounded && state != MoveState.sliding)
        {
            moveVector = moveVal.Get<Vector2>();

            if (moveVector.x < 0) facingRight = true;
            else if (moveVector.x > 0) facingRight = false;
            GetComponentInChildren<SpriteRenderer>().flipX = facingRight;
        }
    }

    public void OnSprint()
    {
        if (grounded && state != MoveState.inAir && state != MoveState.sliding)
        {
            sprinting = !sprinting;
        }
        else
        {
            sprinting = false;
        }
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
            a.SetTrigger("Slide");
            sliding = true;
            slideTimer = slideTime;
            if (!facingRight) rb.AddForce(new Vector3(slideForce, 0, 0), ForceMode.Impulse);
            else rb.AddForce(new Vector3(-slideForce, 0, 0), ForceMode.Impulse);
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
                sprinting = false;
                state = MoveState.sliding;
            }
        }
        else
        {
            state = MoveState.inAir;
        }

    }

    void EndSlide()
    {
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
