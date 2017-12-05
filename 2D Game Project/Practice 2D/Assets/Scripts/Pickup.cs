using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class Pickup : NetworkBehaviour {

    private int typeID;
    //private Rigidbody rb;
    private PlayerController player;

    // Use this for initialization
    void Start () {
        //rb = GetComponent<Rigidbody>();
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
