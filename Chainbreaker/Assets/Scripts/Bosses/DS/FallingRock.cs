using Unity.VisualScripting;
using UnityEngine;

public class FallingRock : MonoBehaviour
{
    public int damage;
    public float destroyTime;
    bool floored = false;
    public GameObject breakParticles;
    public GameObject warningRing;
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Projectile")
        {
            if (floored || col.gameObject.GetComponent<FallingRock>().floored)
            {
                Break();
                col.gameObject.GetComponent<FallingRock>().Break();
            }
        }

        else if (col.gameObject.tag == "Player")
        {
            if (!floored)
            {
                Break();
                col.gameObject.GetComponent<Movement>().TakeDamage();
            }
        }

        else
            Break();
    }


    void Break()
    {
        GameObject particles = Instantiate(breakParticles, gameObject.transform.position, Quaternion.Euler(-90, 0, 0));
        Destroy(particles, 5f);
        Destroy(gameObject);
        Destroy(warningRing);
    }
}
