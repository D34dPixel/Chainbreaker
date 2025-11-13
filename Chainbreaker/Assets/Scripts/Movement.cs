using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class Movement : MonoBehaviour
{
    [Header("Stats")]
    public int defaultSpeed = 20;
    public int sprintSpeed = 50;
    public int slideSpeed = 25;
    public float slideTime = 5;
    public int defaultHealth = 5;
    public int jumpForce = 10;

    int health;
    int speed;
    float slideTimer;
    float jumpTimer;

    [Header("More")]
    public LayerMask ground;
    Vector2 moveVector;

    //Action bools
    bool moving;
    bool sprinting;
    bool grounded;
    bool jumping;
    bool falling;
    bool shooting;
    bool sliding;

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
        if (sliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }

        
        grounded = Physics.Raycast(transform.position+Vector3.up, Vector3.down, 2.5f, ground);
        moving = (moveVector != Vector2.zero);
        StateHandler();


        a.SetBool("Grounded", grounded);
        a.SetBool("Moving", moving);
        a.SetBool("Running", sprinting);
        a.SetBool("Jumping", jumping);
        a.SetBool("Sliding", sliding);
        a.SetBool("Falling", falling);

        if (grounded)
            rb.linearVelocity = new Vector3(moveVector.x * speed, 0, moveVector.y * speed);

    }

    //basic movement input
    public void OnMove(InputValue moveVal)
    {
        if (grounded)
        {
            moveVector = moveVal.Get<Vector2>();
            if (moveVector.x < 0) GetComponentInChildren<SpriteRenderer>().flipX = true;
            else if (moveVector.x > 0) GetComponentInChildren<SpriteRenderer>().flipX = false;
        }
    }

    public void OnSprint()
    {
        if (grounded)
        {
            sprinting = !sprinting;
        }
    }

    public void OnJump()
    {
        if (grounded && state != MoveState.inAir)
        {
            a.SetTrigger("Jump");
            jumping = true;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
    }

    public void OnSlide()
    {
        if (grounded && state != MoveState.inAir)
        {
            a.SetTrigger("Slide");
            sliding = true;
            slideTimer = slideTime;
        }
    }

    void StateHandler()
    {
        if (grounded)
        {
            state = MoveState.idle;

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
                state = MoveState.sliding;
            }
        }
        else
        {
            state = MoveState.inAir;
            falling = !jumping;
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
    }
}
