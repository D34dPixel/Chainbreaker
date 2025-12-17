using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    
    //phase class to allow for multiple phases
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
        public float[] attackVariables; //these are used for whatever the phase needs them for  - there is definitely a more efficient and less annoying way of doing this
        public Vector3[] idlePositions;
    }
    
    //variables have to be public for the subclass to like them

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

    //states!
    public enum BossState
    {
        idle,
        teleporting,
        weakened,
        charging,
        attacking
    }
    private void Start()
    {
        a = GetComponentInChildren<Animator>();
    }

    public void TakeDamage()
    {
        //if the boss is teleporting negate damage
        if (state == BossState.teleporting)
            return;

        //if weakened it will play a different sound effect and deal more damage than if not
        if (state == BossState.weakened)
        {

            SFXManager.instance.PlaySFXClip(hitWeakSFX, transform.position, 1f, transform);
            //immediately recovers from being weakened and is almost ready to instantly attack too
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

    //more of a placeholder example coroutine for the subclasses to do more in depth
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

    //when "dying" it simply restarts the loop instead of ending the game, reloading the scene
    public virtual void Die()
    {
        StartCoroutine(LoopManager.instance.FadeIn());
    }    
}
