using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
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
    bool moving,sprinting,grounded,jumping,falling,sliding,aiming,facingLeft,invincible,reloading;

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
    public AudioClip reloadSFX, squeakSFX, jumpSFX, hitSFX, slideSFX, landSFX;
    private void Start()
    {
        a = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        //set values to defaults and display
        health = maxHealth;
        bulletCount = maxBullets;
        reloadTime = maxReloadTime;
        DisplayStats();
    }
    private void FixedUpdate()
    {
        //take away from shoot cooldown
        shootCD -= Time.fixedDeltaTime;

        //if sliding then take from slide timer
        if (state == MoveState.sliding)
        {
            //when slidetimer hits 0 end slide
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }
        //take from invincible timer, when hits 0, set vincible again
        invcTimer -= Time.fixedDeltaTime;
        if (invcTimer <= 0)
        {
            invincible = false;
        }

        //when in the air
        if (state == MoveState.inAir)
        {
            //check if below the death boundary
            if (transform.position.y < -25)
            {
                //take damage and teleport to 0,1,0
                TakeDamage();
                transform.position = Vector3.up;
            }
            //if going downwards, end jump (doesn't always work for some reason)
            if (rb.linearVelocity.y <= 0)
            {
                EndJump();
            }
        }

        //if reloading
        if (state == MoveState.reloading)
        {
            //only reload while there are less bullets than max
            if (bulletCount < maxBullets)
            {
                //take from reload timer
                reloadTime -= Time.fixedDeltaTime;
                //when hitting zero, reload bullet, reset reload timer and play sound effect
                if (reloadTime <= 0)
                {
                    bulletCount++;
                    DisplayStats(); //display new bullet count
                    reloadTime = maxReloadTime;
                    SFXManager.instance.PlaySFXClip(reloadSFX, this.transform.position, 1f, this.transform);
                }
            }
            //stop reloading
            else
            {
                reloading = false;
            }
        }

        //get moving bool
        moving = (moveVector != Vector3.zero && state != MoveState.inAir && state != MoveState.sliding);

        //sync camera and movement aiming
        cam.GetComponent<CameraFollow>().aiming = aiming;
        
        //makes it so that the player's physical object will only rotate when moving or aiming
        if (moving || aiming)
        {
            Rotate();
        }
        //check grounded
        grounded = Physics.Raycast(transform.position+Vector3.up, Vector3.down, raycastDist, ground);

        //call state handler
        StateHandler();

        //set animator bools
        a.SetBool("Grounded", grounded);
        a.SetBool("Moving", moving);
        a.SetBool("Running", sprinting);
        a.SetBool("Jumping", jumping);
        a.SetBool("Sliding", sliding);
        a.SetBool("Falling", falling);
        a.SetBool("Aiming", aiming);
        a.SetBool("Reload", reloading);

        //if in air then add air resistance to speed and movement
        if (state == MoveState.inAir)
            rb.AddForce(moveVector.normalized * speed * airResistance, ForceMode.Force);
        else
        {
            //keep player grounded
            rb.AddForce(Vector3.down*10, ForceMode.Force);

            //add appropriate force in appropriate direction
            if (state == MoveState.sliding)
                rb.AddForce(slideDir.normalized * speed, ForceMode.Force);
            else
                rb.AddForce(moveVector.normalized * speed, ForceMode.Force);
        }

        //put orient slightly above player for camera to look at
        orient.transform.position = transform.position + Vector3.up*2;

        //add drag if needed
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    //basic movement input
    public void OnMove(InputValue moveVal)
    {
        //takes move inputs and applies them to the move vector
        moveInputs = moveVal.Get<Vector2>();
        moveVector = (moveInputs.y * transform.forward.normalized + transform.right.normalized * moveInputs.x);

        //makes sure the user is facing the right way based on what they pressed
        if (moveInputs.x < 0 && !aiming) facingLeft = true;
        else if (moveInputs.x > 0) facingLeft = false;
        GetComponentInChildren<SpriteRenderer>().flipX = facingLeft;
    }

    public void Rotate()
    {
        //rotates player to the orient rotation
        rb.rotation = Quaternion.Euler(0, orient.transform.eulerAngles.y, 0);
        moveVector = (moveInputs.y * transform.forward.normalized + transform.right.normalized * moveInputs.x);
    }

    //called when lshift is both pressed and released
    public void OnSprint()
    {
        sprinting = !sprinting;
    }

    //called when right click is both pressed and released
    public void OnAim()
    {
        aiming = !aiming;
        facingLeft = false;
        //for user experience you cannot face left when aiming
        GetComponentInChildren<SpriteRenderer>().flipX = false;
    }

    public void OnReload()
    {
        //check to make sure the gun can be reloaded
        if (bulletCount < maxBullets)
            reloading = true;
        else
            SFXManager.instance.PlaySFXClip(squeakSFX, this.transform.position, 1f, this.transform);
        //plays a cute little squeak sound if nothing is needed to be done
    }

    public void OnJump()
    {
        //rotate jump
        Rotate();
        //if not already in the air
        if (state != MoveState.inAir)
        {
            //stop sliding in case sliding
            EndSlide();
            transform.position += (Vector3.up * 0.1f); //update position just slightly to stop player getting caught on ground
            a.SetTrigger("Jump");
            jumping = true;

            //add force upwards
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            SFXManager.instance.PlaySFXClip(jumpSFX, this.transform.position, 1f, this.transform);
        }
        
    }

    public void OnSlide()
    {
        //rotate to face slide direction
        Rotate();
        //if not already sliding or in air
        if (state != MoveState.inAir && state != MoveState.sliding)
        {
            //set the slide direction to the current move vector <- this allows the camera to rotate or inputs to be pressed without changing slide direction mid-slide
            slideDir = moveVector;
            //decrease mass for better slide
            rb.mass = 0.5f;
            a.SetTrigger("Slide");
            sliding = true;
            //set slide and invinciblity timers
            slideTimer = slideTime;
            invcTimer = invincibilityTime;

            //if moving then slide in the moving direction, otherwise slide in the currently faced direction
            if (moving)
                rb.AddForce(moveVector*slideForce, ForceMode.Impulse);
            else
                rb.AddForce(transform.forward * slideForce, ForceMode.Impulse);

            //play slide sound
            SFXManager.instance.PlaySFXClip(slideSFX, this.transform.position, 1f, this.transform);
        }
    }

    //the state handler, called in update
    void StateHandler()
    {
        //check if grounded
        if (grounded)
        {
            //set state to idle
            state = MoveState.idle;
            falling = false;
            
            //if reloading then set state to reloading
            if (reloading)
            {
                speed = 0;
                state = MoveState.reloading;
            }

            //else if moving
            else if (moving)
            {
                //cannot reload while moving
                reloading = false;
                state = MoveState.walking;
                speed = defaultSpeed;

                //aiming halves speed
                if (aiming)
                {
                    speed /= 2;
                }
                //cannot aim while sprinting anymore so speed is unaffected
                else if (sprinting)
                {
                    state = MoveState.sprinting;
                    speed = sprintSpeed;
                }
            }

            //sliding must disable moving to ensure there is no infinite slide chain
            if (sliding)
            {
                reloading = false;
                moving = false;
                speed = slideSpeed;
                state = MoveState.sliding;
            }
        }
        //if not grounded then in air
        else
        {
            reloading = false;
            moving = false;
            state = MoveState.inAir;
        }

    }

    //ends the slide
    void EndSlide()
    {
        rb.mass = 1f;
        a.SetTrigger("End Slide");
        sliding = false;
    }

    //ends the jump
    void EndJump()
    {
        a.SetTrigger("End Jump");
        jumping = false;
        falling = true;
    }

    //take damage
    public void TakeDamage()
    {
        //take damage if vulnerable
        if (!invincible)
            health--;

        //play hit sound no matter what
        SFXManager.instance.PlaySFXClip(hitSFX, this.transform.position, 1f, this.transform);

        //check if health is less than zero
        if (health <= 0)
        {
            //if so, then restart the loop
            StartCoroutine(LoopManager.instance.FadeIn());
        }
        DisplayStats();
    }

    //similar DisplayStats function to DS.cs
    void DisplayStats()
    {
        //make sure nothing is above the maximum value
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        if (bulletCount > maxBullets)
        {
            bulletCount = maxBullets;
        }

        //disable every heart and bullet
        foreach (GameObject health in hearts)
            health.GetComponent<Image>().enabled = false;

        foreach (GameObject bullet in bullets)
            bullet.GetComponent<Image>().enabled = false;

        //enable the correct amount of hearts and bullets

        for (int i = 0; i < health; i++)
            hearts[i].GetComponent<Image>().enabled = true;

        for (int i = 0; i < bulletCount; i++)
            bullets[i].GetComponent<Image>().enabled = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //if the player hits a specific part of the boss known as the hurtbox it will deal damage
        //the hurtbox is significantly smaller than the hitbox
        if (collision.gameObject.tag == "Hurtbox")
            TakeDamage();
    }

    //On left click
    public void OnShoot()
    {
        //many conditions for this not doing anything
        if (!aiming || shootCD > 0 || !(state == MoveState.idle || state == MoveState.walking) || bulletCount <= 0)
            return;

        //Call the camera to shoot too, it does the raycasting and damaging
        Camera.main.GetComponent<CameraFollow>().Shoot();

        //take a bullet and set shooting cooldown
        bulletCount--;
        DisplayStats();
        shootCD = maxShootCD;

        //play sound
        SFXManager.instance.PlaySFXClip(shootSFX, this.transform.position, 1f, this.transform);
    }
    
}
