using UnityEngine;
using System.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

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

    [Header("Attacks")]
    public DSAttack[] attacks;
    public int chosenAttack;

    private void Start()
    {
        a = GetComponentInChildren<Animator>();
        if (!startWeakened) Attack();
        else state = Boss.BossState.weakened;
    }

    private void FixedUpdate()
    {
        if (state != BossState.weakened)
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

        StartCoroutine(Teleport(attacks[chosenAttack].positions[Random.Range(0,attacks[chosenAttack].positions.Length-1)]));
        a.SetTrigger(string.Concat("Attack", chosenAttack - 1));

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
        a.SetTrigger("TPStart");
        yield return new WaitForSeconds(1f);
        transform.position = position;
        a.SetTrigger("TPEnd");
    }

    public IEnumerator ChargeAttack()
    {
        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime);

        a.SetTrigger("Attack");

        ResetAttackTime();
    }

    public IEnumerator SlamAttack()
    {
        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime);
        a.SetTrigger("Attack");

        ResetAttackTime();
    }

    public IEnumerator YellAttack()
    {
        yield return new WaitForSeconds(attacks[chosenAttack].chargeTime);
        a.SetTrigger("Attack");

        ResetAttackTime();
    }

    void ResetAttackTime()
    {
        float timerAddon = Random.Range(-phases[currentPhase].attackTimerVariation, phases[currentPhase].attackTimerVariation);
        attackTime = phases[currentPhase].attackTimer;
    }
}
