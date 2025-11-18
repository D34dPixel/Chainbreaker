using UnityEngine;
using System.Collections;

public class DS : Boss
{

    private void Start()
    {
        if (!startWeakened) StartCoroutine(Teleport());
        else state = Boss.BossState.weakened;
    }

    public void OnHit(float damage, bool weakSpot)
    {
        if (state == BossState.weakened)
        {
            StartCoroutine(Teleport());
            damage *= 2;
        }

        if (weakSpot) damage *= 1.5f;

        TakeDamage(damage);
    }

    IEnumerator Teleport()
    {
        state = BossState.teleporting;
        a.SetTrigger("TPStart");
        yield return new WaitForSeconds(1f);
        //set new position elsewhere
        a.SetTrigger("TPEnd");
    }
}
