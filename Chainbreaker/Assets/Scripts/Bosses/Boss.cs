using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    
    [System.Serializable]
    public class BossPhase
    {
        public string name;
        public int maxHealth;
        public float attackTimer;
        public float attackTimerVariation;
        public float recoveryTime;
        public float chargeTimeMult;
        public bool nextPhaseExists;
        public float[] attackVariables;
        public Vector3[] idlePositions;
    }

    [Header("Stats")]
    public float health;
    public float attackTime;
    public float recoveryTime;
    public int hitDamage;
    public int hitDamageWeakened;

    [Header("State")]
    public bool startWeakened;
    public BossState state;
    public BossPhase[] phases;
    public int currentPhase;

    [Header("Sounds")]
    public AudioClip windupSFX;
    public AudioClip hitSFX, hitWeakSFX, tpSFX, weakenSFX, recoverSFX;

    [Header("References")]
    public Animator a;

    public enum BossState
    {
        idle,
        teleporting,
        weakened,
        charging,
        attacking
    }

    public void TakeDamage()
    {
        if (state == BossState.teleporting)
            return;

        if (state == BossState.weakened)
        {

            SFXManager.instance.PlaySFXClip(hitWeakSFX, transform.position, 1f, transform);
            recoveryTime = 0;
            attackTime = 1;
            health -= hitDamageWeakened;
        }
        else
        {
            SFXManager.instance.PlaySFXClip(hitSFX, transform.position, 1f, transform);
            health -= hitDamage;
        }

        if (health <= 0)
        {
            StartCoroutine(AdvancePhase());
        }
    }

    public virtual IEnumerator AdvancePhase()
    {
        if (!phases[currentPhase].nextPhaseExists)
            Die();
        else
        {
            yield return new WaitForSeconds(2f);
            currentPhase++;
        }
    }
    public virtual void Attack()
    {
        Debug.Log("Attack");
    }    

    public virtual void Die()
    {
        StartCoroutine(LoopManager.instance.FadeIn());
    }    

    private void Start()
    {
        a = GetComponentInChildren<Animator>();
    }
}
