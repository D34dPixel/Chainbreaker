using UnityEngine;

public class Boss : MonoBehaviour
{
    [System.Serializable]
    public class BossAttack
    {
        public string name;
        public bool useable;
        public float chargeTime;
        public float minDistFromPlayer;
        public float maxDistFromPlayer;
        public GameObject[] hitboxes; //boxes where the player can hit
        public GameObject[] hurtBoxes; //boxes where the player can GET hit
        public GameObject[] weakBoxes; //boxes where double damage is applied
    }

    [System.Serializable]
    public class BossPhase
    {
        public string name;
        public float recoveryTime;
        public int healthPercentToNextPhase;
        public BossAttack[] attacks;
    }

    [Header("Stats")]
    public int maxHealth;
    public float health;

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
