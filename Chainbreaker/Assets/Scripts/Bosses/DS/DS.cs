using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DS : Boss
{
    [System.Serializable]
    public class DSAttack
    {
        public string name;
        public float chargeTime, attackTime, minDistFromPlayer, maxDistFromPlayer;
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
    public AudioClip yellSFX;

    [Header("Battery")]
    public int maxBattery;
    int batteryCharge;
    public GameObject[] batteries;

    [Header("Health")]
    public int heartCount = 3;
    public GameObject[] hearts;
    public AudioClip loseHeartSFX;

    private void Start()
    {
        a = GetComponentInChildren<Animator>();
        if (!startWeakened) Attack();
        else state = BossState.weakened;

        attackTime = attackTime = phases[currentPhase].attackTimer;

        batteryCharge = maxBattery;
        health = phases[currentPhase].maxHealth;
        DisplayStats();

    }

    private void FixedUpdate()
    {
        a.SetBool("Teleporting", state == BossState.teleporting);
        a.SetBool("Charging", charging);
        a.SetBool("Weakened", weakened);


        if (weakened)
        {
            state = BossState.weakened;
            transform.rotation = Quaternion.identity;
            recoveryTime -= Time.fixedDeltaTime;
            if (recoveryTime <= 0)
            {
                SFXManager.instance.PlaySFXClip(recoverSFX, transform.position, 1f, transform);
                state = BossState.idle;
                weakened = false;
                batteryCharge = maxBattery;
                DisplayStats();
            }
        }
        if (state == BossState.idle || state == BossState.charging)
        {
            transform.LookAt(player.transform, Vector3.up);
            if (state == BossState.idle)
            {
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
        state = BossState.charging;

        bool validAttack = false;
        while (!validAttack)
        {
            chosenAttack = Random.Range(0, attacks.Length);
            //chosenAttack = 0;
            //check if player in range;
            validAttack = true;
        }

        bool validPlacement = false;
        foreach (Vector3 atkpos in attacks[chosenAttack].positions)
            if (atkpos == transform.position) validPlacement = true;

        if (!validPlacement || Random.Range(0,3) == 3) 
            StartCoroutine(Teleport(attacks[chosenAttack].positions[Random.Range(0,attacks[chosenAttack].positions.Length)]));

        a.SetInteger("Attack", chosenAttack);
        charging = true;

        switch (chosenAttack)
        {
            case 0: StartCoroutine(ChargeAttack()); break;
            case 1: StartCoroutine(SlamAttack()); break;
            case 2: StartCoroutine(YellAttack()); break; 
        }
    }

    IEnumerator Teleport(Vector3 position)
    {
        SFXManager.instance.PlaySFXClip(tpSFX, transform.position, 1f);

        state = BossState.teleporting;
        yield return new WaitForSeconds(1f);
        if (charging)
            state = BossState.charging;
        else if (weakened)
            state = BossState.weakened;
        else
            state = BossState.idle;
        transform.position = position;
    }

    public IEnumerator ChargeAttack() //considered attack 0, uses phase mult 0 and 1 and 5
    {
        int chargeCount = (int)phases[currentPhase].attackVariables[0];
        float chargeSpeed = phases[currentPhase].attackVariables[1];

        for (int i = 0; i < chargeCount; i++)
        {

            SFXManager.instance.PlaySFXClip(attacks[chosenAttack].chargeSFX, transform.position, 1f, transform);

            yield return new WaitForSeconds((attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult)/(2*(i+1)));
            charging = false;


            a.SetTrigger("Attack!");
            yield return new WaitForSeconds(0.25f);
            state = BossState.attacking;

            float chargeTime = 100;
            float chargingTime = 0;
            Vector3 targetPosition = transform.position + transform.forward * phases[currentPhase].attackVariables[5] + player.transform.up;
            Vector3 currentPos = transform.position;

            while (targetPosition.y < 2)
                targetPosition += Vector3.up;

            //Debug.Log(targetPosition.ToString());
            
            SFXManager.instance.PlaySFXClip(attacks[chosenAttack].attackSFX, transform.position, 1f, transform);

            while (chargingTime < chargeTime)
            {
                transform.position = Vector3.Lerp(currentPos, targetPosition, chargingTime/chargeTime);
                chargingTime++;
                yield return new WaitForSeconds(0.01f);
            }


            if (i + 1 < chargeCount)
            {
                a.SetTrigger("Recharge");
                charging = true;
                state = BossState.charging;
            }

        }

        ResetAttackTime();
    }

    public IEnumerator SlamAttack() //attack 1
    {
        SFXManager.instance.PlaySFXClip(attacks[chosenAttack].chargeSFX, transform.position, 1f, transform);

        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);

        charging = false;
        a.SetTrigger("Attack!");
        state = BossState.attacking;

        SFXManager.instance.PlaySFXClip(attacks[chosenAttack].attackSFX, transform.position, 1f, transform);

        yield return new WaitForSeconds(attacks[chosenAttack].attackTime);


        ResetAttackTime();
    }

    public IEnumerator YellAttack() // attack 2, uses phase mult 2, 3 4
    {
        SFXManager.instance.PlaySFXClip(attacks[chosenAttack].chargeSFX, transform.position, 1f, transform);

        int rockCount = (int)phases[currentPhase].attackVariables[2];
        float rockMass = phases[currentPhase].attackVariables[3];
        float rockSpawnSpeed = phases[currentPhase].attackVariables[4];

        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);

        SFXManager.instance.PlaySFXClip(yellSFX, transform.position, 1f, transform);

        charging = false;
        a.SetTrigger("Attack!");
        state = BossState.attacking;

        for (int i = 0; i < rockCount; i++)
        {
            SFXManager.instance.PlaySFXClip(attacks[chosenAttack].attackSFX, transform.position, 1f, transform);

            Vector3 spawnPos = new Vector3(player.transform.position.x, 1, player.transform.position.z);
            Vector3 rockSpawn = (spawnPos + (Vector3.up * 100));
            GameObject rockGO = Instantiate(rock, rockSpawn, Quaternion.identity);
           
            rockGO.GetComponent<Rigidbody>().mass = rockMass;

            if (warningRing != null)
            {
                GameObject ring = Instantiate(warningRing, spawnPos, Quaternion.identity);
                rockGO.GetComponent<FallingRock>().warningRing = ring;
            }
            yield return new WaitForSeconds(rockSpawnSpeed);
        }

        ResetAttackTime();
    }

    void ResetAttackTime()
    {
        a.ResetTrigger("Attack!");
        a.SetTrigger("End Attack");
        batteryCharge--;
        DisplayStats();
        if (batteryCharge <= 0)
        {
            state = BossState.weakened;
            weakened = true;
            recoveryTime = phases[currentPhase].recoveryTime;
            SFXManager.instance.PlaySFXClip(weakenSFX, transform.position, 1f, transform);

        }
        else
        {
            state = BossState.idle;
        }
        if (Random.Range(0,2) != 0)
            StartCoroutine(Teleport(phases[currentPhase].idlePositions[Random.Range(0, phases[currentPhase].idlePositions.Length - 1)]));

        float timerAddon = Random.Range(-phases[currentPhase].attackTimerVariation, phases[currentPhase].attackTimerVariation);
        attackTime = phases[currentPhase].attackTimer + timerAddon;
    }

    void DisplayStats()
    {
        if (health > phases[currentPhase].maxHealth)
        {
            health = phases[currentPhase].maxHealth;
        }
        
        if (batteryCharge > maxBattery)
        {
            batteryCharge = maxBattery;
        }


        foreach (GameObject health in hearts)
            health.GetComponent<Image>().enabled = false;

        for (int i = 0; i < heartCount; i++)
            hearts[i].GetComponent<Image>().enabled = true;

        foreach (GameObject battery in batteries)
            battery.GetComponent<Image>().enabled = false;

        for (int i = 0; i < batteryCharge; i++)
            batteries[i].GetComponent<Image>().enabled = true;
    }
    public override IEnumerator AdvancePhase()
    {
        heartCount--;

        if (phases[currentPhase].nextPhaseExists)
            currentPhase++;

        if (heartCount <= 0)
            Die();
        else
        {
            SFXManager.instance.PlaySFXClip(loseHeartSFX, transform.position, 1f, transform);

            batteryCharge++;
            ResetAttackTime();

            yield return new WaitForSeconds(2f);

            maxBattery = (int)phases[currentPhase].attackVariables[6];

            health = phases[currentPhase].maxHealth;
            batteryCharge = maxBattery;
            DisplayStats();

            attackTime = phases[currentPhase].attackTimer;
        }
        StopAllCoroutines();
    }
}

