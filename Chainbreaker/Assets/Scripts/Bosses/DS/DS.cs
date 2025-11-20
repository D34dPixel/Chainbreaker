using UnityEngine;
using System.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEngine.LightTransport;

public class DS : Boss
{
    [System.Serializable]
    public class DSAttack
    {
        public string name;
        public float chargeTime;
        public float attackTime;
        public float minDistFromPlayer;
        public float maxDistFromPlayer;
        public GameObject[] hitboxes; //boxes where the player can hit
        public GameObject[] hurtBoxes; //boxes where the player can GET hit
        public GameObject[] weakBoxes; //boxes where double damage is applied
        public Vector3[] positions;
    }

    bool charging;
    public GameObject player;

    [Header("Attacks")]
    public DSAttack[] attacks;
    public int chosenAttack;
    public GameObject rock;
    

    private void Start()
    {
        a = GetComponentInChildren<Animator>();
        if (!startWeakened) Attack();
        else state = Boss.BossState.weakened;

        attackTime = attackTime = phases[currentPhase].attackTimer;

    }

    private void FixedUpdate()
    {
        a.SetBool("Teleporting", state == BossState.teleporting);
        a.SetBool("Charging", charging);
        a.SetBool("Weakened", state == BossState.weakened);

        if (state == BossState.weakened)
        {
            transform.rotation = Quaternion.identity;
            recoveryTime -= Time.fixedDeltaTime;
            if (recoveryTime <= 0)
            {
                state = BossState.idle;
            }
        }
        if (state == BossState.idle || state == BossState.charging)
        {
            transform.LookAt(player.transform, Vector3.up);
            attackTime -= Time.fixedDeltaTime;
            if (attackTime <= 0)
            {
                Attack();
            }
        }
    }

    public override void Attack()
    {
        bool validAttack = false;
        while (!validAttack)
        {
            //chosenAttack = Random.Range(0, attacks.Length);
            chosenAttack = 2;
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

    public void OnHit(float damage, bool weakSpot)
    {
        if (state == BossState.weakened)
        {
            Attack();
            damage *= 2;
        }

        if (weakSpot) damage *= 1.5f;

        TakeDamage(damage);
    }

    IEnumerator Teleport(Vector3 position)
    {
        state = BossState.teleporting;
        yield return new WaitForSeconds(1f);
        if (charging)
            state = BossState.charging;
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
            yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);
            charging = false;


            a.SetTrigger("Attack!");
            state = BossState.attacking;

            yield return new WaitForSeconds(attacks[0].attackTime * phases[currentPhase].attackVariables[5]);

            if (i + 1 < chargeCount)
            {
                a.SetTrigger("Recharge");
                charging = true;
            }

        }

        ResetAttackTime();
    }

    public IEnumerator SlamAttack() //attack 1
    {
        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);

        charging = false;
        a.SetTrigger("Attack!");
        state = BossState.attacking;

        yield return new WaitForSeconds(attacks[1].attackTime);

        ResetAttackTime();
    }

    public IEnumerator YellAttack() // attack 2, uses phase mult 2, 3 4
    {
        int rockCount = (int)phases[currentPhase].attackVariables[2];
        float rockSpeed = phases[currentPhase].attackVariables[3];
        float rockSpawnSpeed = phases[currentPhase].attackVariables[4];

        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);

        charging = false;
        a.SetTrigger("Attack!");
        state = BossState.attacking;

        for (int i = 0; i < rockCount; i++)
        {
            yield return new WaitForSeconds(rockSpawnSpeed);
            Vector3 rockSpawn = (player.transform.position + (Vector3.up * 100));
            GameObject rockGO = Instantiate(rock, rockSpawn, Quaternion.identity);
            
        }

        yield return new WaitForSeconds(attacks[2].attackTime);

        ResetAttackTime();
    }

    void ResetAttackTime()
    {
        a.SetTrigger("End Attack");
        attackCount++;
        if (attackCount >= phases[currentPhase].attacksToWeaken)
        {
            attackCount = 0;
            state = BossState.weakened;
        }
        else
        {
            state = BossState.idle;
            StartCoroutine(Teleport(phases[currentPhase].idlePositions[Random.Range(0, phases[currentPhase].idlePositions.Length - 1)]));
        }
        float timerAddon = Random.Range(-phases[currentPhase].attackTimerVariation, phases[currentPhase].attackTimerVariation);
        attackTime = phases[currentPhase].attackTimer + timerAddon;
    }
}
