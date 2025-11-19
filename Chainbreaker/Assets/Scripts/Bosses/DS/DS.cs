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
        public float minDistFromPlayer;
        public float maxDistFromPlayer;
        public GameObject[] hitboxes; //boxes where the player can hit
        public GameObject[] hurtBoxes; //boxes where the player can GET hit
        public GameObject[] weakBoxes; //boxes where double damage is applied
        public Vector3[] positions;
    }

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
    }

    private void FixedUpdate()
    {
        a.SetBool("Teleporting", state == BossState.teleporting);
        a.SetBool("Weakened", state == BossState.weakened);

        if (state == BossState.weakened)
        {
            recoveryTime -= Time.fixedDeltaTime;
            if (recoveryTime <= 0)
            {
                state = BossState.idle;
            }
        }
        if (state == BossState.idle)
        {
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
            chosenAttack = Random.Range(0, attacks.Length-1);
            //check if player in range;
            validAttack = true;
        }

        bool validPlacement = false;
        foreach (Vector3 atkpos in attacks[chosenAttack].positions)
            if (atkpos == transform.position) validPlacement = true;

        if (!validPlacement || Random.Range(0,3) == 3) 
            StartCoroutine(Teleport(attacks[chosenAttack].positions[Random.Range(0,attacks[chosenAttack].positions.Length-1)]));

        a.SetInteger("Attack", chosenAttack);

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
        state = BossState.idle;
        transform.position = position;
    }

    public IEnumerator ChargeAttack() //considered attack 0, uses phase mult 0 and 1
    {
        int chargeCount = (int)phases[currentPhase].attackVariables[0];
        float chargeSpeed = (int)phases[currentPhase].attackVariables[1];

        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);

        a.SetTrigger("Attack!");
        ResetAttackTime();
    }

    public IEnumerator SlamAttack() //attack 1
    {
        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);
        a.SetTrigger("Attack!");

        ResetAttackTime();
    }

    public IEnumerator YellAttack() // attack 2, uses phase mult 2, 3 4
    {
        int rockCount = (int)phases[currentPhase].attackVariables[2];
        float rockSpeed = phases[currentPhase].attackVariables[3];
        float rockSpawnSpeed = phases[currentPhase].attackVariables[4];

        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime * phases[currentPhase].chargeTimeMult);
        a.SetTrigger("Attack!");

        for (int i = 0; i < rockCount; i++)
        {
            Vector3 rockTarget = (player.transform.position + player.transform.up * 100);
            GameObject rockGO = Instantiate(rock, rockTarget, Quaternion.identity);
            rockGO.GetComponent<FallingRock>().target = rockTarget;
            yield return new WaitForSeconds(rockSpawnSpeed);
        }

        ResetAttackTime();
    }

    void ResetAttackTime()
    {
        float timerAddon = Random.Range(-phases[currentPhase].attackTimerVariation, phases[currentPhase].attackTimerVariation);
        attackTime = phases[currentPhase].attackTimer;
    }
}
