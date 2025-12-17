using UnityEngine;

public class FallingRock : MonoBehaviour
{
    public GameObject breakParticles;
    public GameObject warningRing; //warning ring is assigned by the Boss when this rock is created
    public AudioClip breakSound;

    //When hitting something
    private void OnTriggerEnter(Collider col)
    {
        //if the object hit is the player deal damage
        if (col.gameObject.tag == "Player")
            col.gameObject.GetComponent<Movement>().TakeDamage();

        Break();
    }


    void Break()
    {
        //create particle object to simulate breaking and play sound
        GameObject particles = Instantiate(breakParticles, gameObject.transform.position, Quaternion.Euler(-90, 0, 0));
        SFXManager.instance.PlaySFXClip(breakSound, this.transform.position, 1f);
        Destroy(particles, 5f);
        Destroy(gameObject);
        Destroy(warningRing);

        //destroy the everything, particles are destroyed after 5 seconds
    }
}
