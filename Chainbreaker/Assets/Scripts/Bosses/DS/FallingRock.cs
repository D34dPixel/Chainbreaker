using UnityEngine;

public class FallingRock : MonoBehaviour
{
    public int damage;
    public Vector3 target;
    public float destroyTime;

    void Update()
    {
        if (transform.position.y == GetComponent<SphereCollider>().radius)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            destroyTime -= Time.deltaTime;
        }

        if (destroyTime <= 0)
            Break();
            
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Projectile")
        {
            Break();
            col.gameObject.GetComponent<FallingRock>().Break();
        }

        else if (col.gameObject.tag == "Player")
        {
            Break();
            col.gameObject.GetComponent<Movement>().TakeDamage(damage);
        }
    }

    void Break()
    {
        Destroy(this.gameObject);
    }
}
