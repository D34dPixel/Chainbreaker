using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DS : Boss
{
    [System.Serializable]
    public class DSAttack //attack class for the attacks
    {
        public string name;
        public float chargeTime, attackTime;
        public Vector3 hitboxSize;
        public Vector3[] positions;
        public AudioClip attackSFX, chargeSFX;
    }

    bool charging, weakened;
    public GameObject player;

    [Header("Attacks")]
    public DSAttack[] attacks;
    public int chosenAttack;
    public GameObject rock;
    public GameObject warningRing;
    public AudioClip yellSFX; //yell needed three sfx instead of two so this is here now. there was a much easier solution to this

    [Header("Battery")]
    public int maxBattery;
    int batteryCharge;
    public GameObject[] batteries;

    [Header("Health")] //hearts are different to health, hearts are more of an indicator of phase
    public int heartCount = 3;
    public GameObject[] hearts;
    public AudioClip loseHeartSFX;

    private void Start()
    {
        a = GetComponentInChildren<Animator>();

        //if start weakened, then start weakened, otherwise immediately attack
        if (!startWeakened) Attack();
        else state = BossState.weakened;

        //set necessary stats to max
        attackTime = phases[currentPhase].attackTimer;

        batteryCharge = maxBattery;
        health = phases[currentPhase].maxHealth;
        DisplayStats();

    }

    private void FixedUpdate()
    {
        //set necessary animator bools
        a.SetBool("Teleporting", state == BossState.teleporting);
        a.SetBool("Charging", charging);
        a.SetBool("Weakened", weakened);

        //check if weakened and take from the recovery countdown if true
        if (weakened)
        {
            state = BossState.weakened;
            transform.rotation = Quaternion.identity;
            recoveryTime -= Time.fixedDeltaTime;

            //when recovery countdown is 0, reset batteries and go back to idle state
            if (recoveryTime <= 0)
            {
                SFXManager.instance.PlaySFXClip(recoverSFX, transform.position, 1f, transform);
                state = BossState.idle;
                weakened = false;
                batteryCharge = maxBattery;
                DisplayStats();
            }
        }
        //when idle or charging the boss will always face player position
        if (state == BossState.idle || state == BossState.charging)
        {
            transform.LookAt(player.transform, Vector3.up);
            if (state == BossState.idle)
            {
                //if idle then take from attack countdown and attack on 0
                attackTime -= Time.fixedDeltaTime;
                if (attackTime <= 0)
                {
                    Attack();
                }
            }
        }
    }

    public override void Attack()
    {
        //set state to charging
        state = BossState.charging;

        //select random attack
        chosenAttack = Random.Range(0, attacks.Length);

        //check if already in a valid location
        bool validPlacement = false;
        foreach (Vector3 atkpos in attacks[chosenAttack].positions)
            if (atkpos == transform.position) validPlacement = true;

        //teleport to a random valid location if either not already in a valid location or if the randomiser hits a one in four
        if (!validPlacement || Random.Range(0,3) == 3) 
            StartCoroutine(Teleport(attacks[chosenAttack].positions[Random.Range(0,attacks[chosenAttack].positions.Length)]));

        //set bools
        a.SetInteger("Attack", chosenAttack);
        charging = true;

        //switch case for the attacks
        switch (chosenAttack)
        {
            case 0: StartCoroutine(ChargeAttack()); break;
            case 1: StartCoroutine(SlamAttack()); break;
            case 2: StartCoroutine(YellAttack()); break; 
        }
    }

    //Teleporting coroutine
    IEnumerator Teleport(Vector3 position)
    {
        //play tp sound
        SFXManager.instance.PlaySFXClip(tpSFX, transform.position, 1f);

        //set state and wait to return to previous state - probably would have been better to store previous state
        state = BossState.teleporting;
        yield return new WaitForSeconds(1f);
        if (charging)
            state = BossState.charging;
        else if (weakened)
            state = BossState.weakened;
        else
            state = BossState.idle;

        //move to new position
        transform.position = position;
    }

    public IEnumerator ChargeAttack() //considered attack 0, uses phase mult 0 and 1 and 5 <----- again: really awful way of doing this, it should have just been an array of arrays per attack
    {
        //see what I mean about the attackVariables thing being dumb, they literally get named right here
        int chargeCount = (int)phases[currentPhase].attackVariables[0];
        float chargeSpeed = phases[currentPhase].attackVariables[1];

        //for each charge)
        for (int i = 0; i < chargeCount; i++)
        {
            //play the charge sfx and wait to attack
            SFXManager.instance.PlaySFXClip(attacks[chosenAttack].chargeSFX, transform.position, 1f, transform);

            yield return new WaitForSeconds((attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult)/(2*(i+1)));
            charging = false;

            //set attack variables and animator stuff
            a.SetTrigger("Attack!");
            yield return new WaitForSeconds(0.25f);
            state = BossState.attacking;

            //create temp variables to use in the lerp
            float chargeTime = 100;
            float chargingTime = 0;
            Vector3 targetPosition = transform.position + transform.forward * phases[currentPhase].attackVariables[5] + player.transform.up;
            Vector3 currentPos = transform.position;

            //make sure the target isn't in the ground
            while (targetPosition.y < 2)
                targetPosition += Vector3.up;

            //Debug.Log(targetPosition.ToString());
            
            //play attack sound
            SFXManager.instance.PlaySFXClip(attacks[chosenAttack].attackSFX, transform.position, 1f, transform);

            while (chargingTime < chargeTime) //I really wish I'd used dash attack instead of charge attack
            {
                //lerp position to get to the target position
                transform.position = Vector3.Lerp(currentPos, targetPosition, chargingTime/chargeTime);
                chargingTime++;
                yield return new WaitForSeconds(0.01f);
            }

            //if there are more charges to come, initiate recharge stuff
            if (i + 1 < chargeCount)
            {
                a.SetTrigger("Recharge");
                charging = true;
                state = BossState.charging;
            }

        }

        //reset attack stuff
        ResetAttackTime();
    }

    //this attack doesn't work yet, it just plays an animation
    public IEnumerator SlamAttack() //attack 1
    {
        //charge sfx and wait
        SFXManager.instance.PlaySFXClip(attacks[chosenAttack].chargeSFX, transform.position, 1f, transform);

        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);

        //attakc anims and bools
        charging = false;
        a.SetTrigger("Attack!");
        state = BossState.attacking;

        //attack sfx and wait
        SFXManager.instance.PlaySFXClip(attacks[chosenAttack].attackSFX, transform.position, 1f, transform);

        yield return new WaitForSeconds(attacks[chosenAttack].attackTime);

        //reset attack stuff
        ResetAttackTime();
    }

    public IEnumerator YellAttack() // attack 2, uses phase mult 2, 3 4
    {
        SFXManager.instance.PlaySFXClip(attacks[chosenAttack].chargeSFX, transform.position, 1f, transform);

        //take the dumb attackvariables into nice named local variables
        int rockCount = (int)phases[currentPhase].attackVariables[2];
        float rockMass = phases[currentPhase].attackVariables[3];
        float rockSpawnSpeed = phases[currentPhase].attackVariables[4];

        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);

        //play the yell sound
        SFXManager.instance.PlaySFXClip(yellSFX, transform.position, 1f, transform);

        //do attack stuff
        charging = false;
        a.SetTrigger("Attack!");
        state = BossState.attacking;

        //for the amount of rocks
        for (int i = 0; i < rockCount; i++)
        {
            //play rock spawn sound <- could have been done in the actual rock but whatever
            SFXManager.instance.PlaySFXClip(attacks[chosenAttack].attackSFX, transform.position, 1f, transform);

            //rock local variables
            Vector3 spawnPos = new Vector3(player.transform.position.x, 1, player.transform.position.z);
            Vector3 rockSpawn = (spawnPos + (Vector3.up * 100));
            GameObject rockGO = Instantiate(rock, rockSpawn, Quaternion.identity);
            
            //set mass of rock's rigidbody
            rockGO.GetComponent<Rigidbody>().mass = rockMass;

            //make sure the prefab exists
            if (warningRing != null)
            {
                //if the prefab DOES exist then instantiate one under the rock and assign it to the rock
                GameObject ring = Instantiate(warningRing, spawnPos, Quaternion.identity);
                rockGO.GetComponent<FallingRock>().warningRing = ring;
            }
            yield return new WaitForSeconds(rockSpawnSpeed);
        }

        //reset attack stuff
        ResetAttackTime();
    }

    void ResetAttackTime()
    {
        //reset animation triggers
        a.ResetTrigger("Attack!");
        a.SetTrigger("End Attack");

        //take a battery
        batteryCharge--;
        DisplayStats();

        //check if there is no battery
        if (batteryCharge <= 0)
        {
            //set state to weakened and play appropriate sound effect
            state = BossState.weakened;
            weakened = true;
            recoveryTime = phases[currentPhase].recoveryTime;
            SFXManager.instance.PlaySFXClip(weakenSFX, transform.position, 1f, transform);

        }
        else
        {
            //reset state if not weakened
            state = BossState.idle;
        }

        //if the random hits the one in three it will, otherwise it will remain where it finished the attack
        if (Random.Range(0,2) != 0)
            StartCoroutine(Teleport(phases[currentPhase].idlePositions[Random.Range(0, phases[currentPhase].idlePositions.Length - 1)]));

        //randomise a timer addon for a slight dash of unpredictability and add it to the base timer
        float timerAddon = Random.Range(-phases[currentPhase].attackTimerVariation, phases[currentPhase].attackTimerVariation);
        attackTime = phases[currentPhase].attackTimer + timerAddon;
    }

    //this makes sure hearts and batteries are displayed properly
    void DisplayStats()
    {
        //make sure health and battery don't go past their max
        if (health > phases[currentPhase].maxHealth)
        {
            health = phases[currentPhase].maxHealth;
        }
        
        if (batteryCharge > maxBattery) //<- I should've done it like this for all the phase variables
        {
            batteryCharge = maxBattery;
        }

        //disable every heart
        foreach (GameObject health in hearts)
            health.GetComponent<Image>().enabled = false;

        //activate as many hearts as possible
        for (int i = 0; i < heartCount; i++)
            hearts[i].GetComponent<Image>().enabled = true;

        //disable every battery
        foreach (GameObject battery in batteries)
            battery.GetComponent<Image>().enabled = false;

        //activate as many batteries as possible
        for (int i = 0; i < batteryCharge; i++)
            batteries[i].GetComponent<Image>().enabled = true;

        //it is important to disable the image component rather than the gameobject as the layout groups will put things in the wrong places otherwise
    }
    public override IEnumerator AdvancePhase()
    {
        //take a heart
        heartCount--;

        //advance phase if possible
        if (phases[currentPhase].nextPhaseExists)
            currentPhase++;

        //die if there are no hearts left
        if (heartCount <= 0)
            Die();
        else
        {
            //otherwise teleport to the idle position
            StartCoroutine(Teleport(phases[currentPhase].idlePositions[Random.Range(0, phases[currentPhase].idlePositions.Length - 1)] - Vector3.up*100));

            //play the lost heart sound
            SFXManager.instance.PlaySFXClip(loseHeartSFX, transform.position, 1f, transform);

            //add a battery because if the battery is zero when ResetAttacktime is called it will put the boss into its weakened state
            batteryCharge++;
            //reset attack stuff
            ResetAttackTime();

            yield return new WaitForSeconds(2f);


            //update health and battery to their new values
            maxBattery = (int)phases[currentPhase].attackVariables[6];

            health = phases[currentPhase].maxHealth;
            batteryCharge = maxBattery;
            DisplayStats();

            attackTime = phases[currentPhase].attackTimer;
        }
    }
}

