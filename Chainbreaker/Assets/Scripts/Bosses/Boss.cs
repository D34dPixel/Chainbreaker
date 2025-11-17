using UnityEngine;

public class Boss : MonoBehaviour
{
    public int maxHealth;
    public int health;
    public Animator a;

    private void Start()
    {
        a = GetComponentInChildren<Animator>();
    }
}
