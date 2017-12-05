using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BulletController : NetworkBehaviour {

    public float bulletDamage = 10;
    public float lifespan = 10;
    public Vector2 vel;
    public bool ricochet = false;

    private float activationDelay = 0.05f;

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.position = transform.position + new Vector3(vel.x, vel.y, 0);

        lifespan -= Time.deltaTime;
        if (lifespan < 0) Destroy(gameObject);

        activationDelay -= Time.deltaTime;
	}

    // Set velocity vector
    public void SetVelocity(Vector2 v)
    {
        vel = v;
    }

    public Vector2 GetVelocity()
    {
        return vel;
    }

    // Set ricochet boolean
    public void SetRicochet(bool r)
    {
        ricochet = r;
    }

    // Collisions
	[ClientCallback]
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Player" && activationDelay < 0)
        {
            HealthScript h = coll.gameObject.GetComponent<HealthScript>();
            h.TakeDamage(bulletDamage);
            Destroy(gameObject);
        }

        if (coll.gameObject.tag == "Ground")
        {
            if (ricochet) vel.y *= -1; // temporary
            else Destroy(gameObject);
        }
    }
}
