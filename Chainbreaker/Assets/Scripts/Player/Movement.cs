using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
    public float maxShootCD;
    float shootCD;
    public float maxReloadTime;
    float reloadTime;

    [Header("Ground")]
    public float raycastDist;
    public LayerMask ground;
    public float groundDrag = 2f;
    public float airResistance = 10f;

    Vector3 moveInputs;
    Vector3 moveVector;

    //Action bools
    bool moving,sprinting,grounded,jumping,falling,sliding,aiming,facingRight,invincible,reloading;

    Animator a;
    Rigidbody rb;

    public MoveState state;

    public enum MoveState
    {
        idle,
        walking,
        sprinting,
        sliding,
        inAir,
        reloading
    }

    [Header("Sounds")]
    public AudioClip shootSFX;
    public AudioClip reloadSFX, squeakSFX, jumpSFX, hitSFX, slideSFX;
    private void Start()
    {
        a = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        health = maxHealth;
        bulletCount = maxBullets;
        reloadTime = maxReloadTime;
        DisplayStats();
    }
    private void FixedUpdate()
    {

        shootCD -= Time.fixedDeltaTime;

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
            if (transform.position.y < -25)
            {
                TakeDamage();
                transform.position = Vector3.up;
            }
            if (rb.linearVelocity.y <= 0)
            {
                EndJump();
            }
        }

        if (state == MoveState.reloading)
        {
            if (bulletCount < maxBullets)
            {
                reloadTime -= Time.fixedDeltaTime;
                if (reloadTime <= 0)
                {
                    bulletCount++;
                    DisplayStats();
                    reloadTime = maxReloadTime;
                    SFXManager.instance.PlaySFXClip(reloadSFX, this.transform.position, 1f, this.transform);
                }
            }
            else
            {
                reloading = false;
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
        a.SetBool("Reload", reloading);

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
        moveVector = (moveInputs.y * transform.forward.normalized + transform.right.normalized * moveInputs.x);

        if (moveInputs.x < 0 && !aiming) facingRight = true;
        else if (moveInputs.x > 0) facingRight = false;
        GetComponentInChildren<SpriteRenderer>().flipX = facingRight;
    }

    public void Rotate()
    {
        rb.rotation = Quaternion.Euler(0, orient.transform.eulerAngles.y, 0);
        moveVector = (moveInputs.y * transform.forward.normalized + transform.right.normalized * moveInputs.x);
    }

    public void OnSprint()
    {
        sprinting = !sprinting;
    }

    public void OnAim()
    {
        aiming = !aiming;
        facingRight = false;
        GetComponentInChildren<SpriteRenderer>().flipX = false;
    }

    public void OnReload()
    {
        if (bulletCount < maxBullets)
            reloading = true;
        else
            SFXManager.instance.PlaySFXClip(squeakSFX, this.transform.position, 1f, this.transform);

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

        SFXManager.instance.PlaySFXClip(jumpSFX, this.transform.position, 1f, this.transform);
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
            SFXManager.instance.PlaySFXClip(slideSFX, this.transform.position, 1f, this.transform);
        }
    }

    void StateHandler()
    {
        if (grounded)
        {
            state = MoveState.idle;
            falling = false;
            
            if (reloading)
            {
                speed = 0;
                state = MoveState.reloading;
            }

            else if (moving)
            {
                reloading = false;
                state = MoveState.walking;
                speed = defaultSpeed;

                if (aiming)
                {
                    speed /= 2;
                }
                else if (sprinting)
                {
                    state = MoveState.sprinting;
                    speed = sprintSpeed;
                }
            }

            if (sliding)
            {
                reloading = false;
                moving = false;
                speed = slideSpeed;
                state = MoveState.sliding;
            }
        }
        else
        {
            reloading = false;
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

        SFXManager.instance.PlaySFXClip(hitSFX, this.transform.position, 1f, this.transform);

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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Hurtbox")
            TakeDamage();
    }

    public void OnShoot()
    {
        if (!aiming || shootCD > 0 || !(state == MoveState.idle || state == MoveState.walking))
            return;

        Camera.main.GetComponent<CameraFollow>().Shoot();

        bulletCount--;
        DisplayStats();
        shootCD = maxShootCD;

        SFXManager.instance.PlaySFXClip(shootSFX, this.transform.position, 1f, this.transform);
    }
}
