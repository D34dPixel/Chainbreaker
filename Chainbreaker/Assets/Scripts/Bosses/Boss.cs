using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    
    [System.Serializable]
    public class BossPhase
    {
        public int maxHealth;
        public string name;
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

    [Header("State")]
    public bool startWeakened;
    public BossState state;
    public BossPhase[] phases;
    public int currentPhase;

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

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if (phases[currentPhase].nextPhaseExists)
                StartCoroutine(AdvancePhase());
            else
                Die();
        }
    }

    public virtual IEnumerator AdvancePhase()
    {
        yield return new WaitForSeconds(2f);
        currentPhase++;
    }
    public virtual void Attack()
    {
        Debug.Log("Attack");
    }    

    public virtual void Die()
    {
        Destroy(gameObject);
    }    

    private void Start()
    {
        a = GetComponentInChildren<Animator>();
    }
}
