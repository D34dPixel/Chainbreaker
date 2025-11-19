using UnityEngine;

public class Boss : MonoBehaviour
{
    
    [System.Serializable]
    public class BossPhase
    {
        public string name;
        public float attackTimer;
        public float attackTimerVariation;
        public float recoveryTime;
        public float chargeTimeMult;
        public int healthPercentToNextPhase;
        public float[] attackVariables;
    }

    [Header("Stats")]
    public int maxHealth;
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
        AdvancePhase();
        if (health <= 0) Die();
    }

    public void AdvancePhase()
    {
        if (health <= (maxHealth / 100) * phases[currentPhase].healthPercentToNextPhase)
        {
            if (phases[currentPhase + 1] != null)
                currentPhase++;
        }
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
