using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {

    private int typeID;
    private Rigidbody rb;
    private PlayerController player;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        typeID = 1;
    }
	
	// Update is called once per frame
	void Update () 
	{
        // Item pickups move?
        // Despawn timer?
	}

    // Get the collided player and apply the item
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            player.EquipItem(typeID);
            Destroy(gameObject);
        }
    }

	public void SetTypeID(int id)
	{
		typeID = id;
	}
}
