using UnityEngine;

public class Boss : MonoBehaviour
{
    public int maxHealth;
    public float health;
    public Animator a;

    [System.Serializable]
    public class Attack
    {
        GameObject[] hitBoxes;
        GameObject[] hurtBoxes;
        GameObject[] weakBoxes;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Die();
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
