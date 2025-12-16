using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
public class Movement : MonoBehaviour
{
    public GameObject orient, chara;
    [Header("Stats")]
    public int defaultSpeed = 20;
    public int sprintSpeed = 50;
    public int jumpForce = 10;
    public int maxHealth = 3;
    int health;
    int speed;
    public GameObject[] hearts;

    [Header("Sliding")]
    public int slideSpeed = 20;
    public int slideForce = 25;
    public float slideTime = 5;
    float slideTimer;
    float invcTimer;
    public float invincibilityTime = 2;
    Vector3 slideDir;

    [Header("Camera")]
    public GameObject cam;
    public Vector3 standardOffset;

    [Header("Bullets")]
    public int maxBullets;
    int bulletCount;
    public GameObject[] bullets;

    [Header("Ground")]
    public float raycastDist;
    public LayerMask ground;
    public float groundDrag = 2f;
    public float airResistance = 10f;
    float jumpTimer;

    Vector3 moveInputs;
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
        health = maxHealth;
        bulletCount = maxBullets;
        DisplayStats();
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
            if (transform.position.y < -10)
            {
                TakeDamage();
            }
            if (rb.linearVelocity.y <= 0)
            {
                EndJump();
            }
        }

        moving = (moveVector != Vector3.zero && state != MoveState.inAir && state != MoveState.sliding);
        cam.GetComponent<CameraFollow>().aiming = aiming;
        if (moving || aiming)
        {
            Rotate();
        }
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
            rb.AddForce(moveVector.normalized * speed * airResistance, ForceMode.Force);
        else
        {
            rb.AddForce(Vector3.down*10, ForceMode.Force);
            if (state == MoveState.sliding)
                rb.AddForce(slideDir.normalized * speed, ForceMode.Force);
            else
                rb.AddForce(moveVector.normalized * speed, ForceMode.Force);
        }

        orient.transform.position = transform.position + Vector3.up*2;

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    //basic movement input
    public void OnMove(InputValue moveVal)
    {
        moveInputs = moveVal.Get<Vector2>();
        moveVector = (moveInputs.y*orient.transform.forward + orient.transform.right * moveInputs.x);

        if (moveInputs.x < 0) facingRight = true;
        else if (moveInputs.x > 0) facingRight = false;
        GetComponentInChildren<SpriteRenderer>().flipX = facingRight;
    }

    public void Rotate()
    {
        rb.rotation = orient.transform.rotation;
        //orient.transform.localRotation = Quaternion.identity;
        moveVector = (moveInputs.y * orient.transform.forward + orient.transform.right * moveInputs.x);
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
        Rotate();
        if (state != MoveState.inAir)
        {
            EndSlide();
            transform.position += (Vector3.up * 0.1f);
            a.SetTrigger("Jump");
            jumping = true;

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        
    }

    public void OnSlide()
    {
        Rotate();
        if (state != MoveState.inAir && state != MoveState.sliding)
        {
            slideDir = moveVector;
            rb.mass = 0.5f;
            a.SetTrigger("Slide");
            sliding = true;
            slideTimer = slideTime;
            invcTimer = invincibilityTime;
            if (moving) rb.AddForce(moveVector*slideForce, ForceMode.Impulse);
            else rb.AddForce(transform.forward * slideForce, ForceMode.Impulse);
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

                if (aiming)
                {
                    speed /= 2;
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

    public void TakeDamage()
    {
        if (!invincible)
            health--;
        if (health <= 0)
        {
            StartCoroutine(LoopManager.instance.FadeIn());
        }
        DisplayStats();
    }

    void DisplayStats()
    {
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        if (bulletCount > maxBullets)
        {
            bulletCount = maxBullets;
        }


        foreach (GameObject health in hearts)
            health.GetComponent<Image>().enabled = false;

        for (int i = 0; i < health; i++)
            hearts[i].GetComponent<Image>().enabled = true;

        foreach (GameObject battery in bullets)
            battery.GetComponent<Image>().enabled = false;

        for (int i = 0; i < bulletCount; i++)
            bullets[i].GetComponent<Image>().enabled = true;
    }
}
