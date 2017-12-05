using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthScript : NetworkBehaviour {

	public float maxHealth = 100.0f;			//Maximum health
	[SyncVar(hook = "OnHealthChanged")]
    public float health;                       //Health of player



	PlayerController player;


	//Set health equal to max health
	void Awake()
	{
		player = GetComponent<PlayerController> ();
	}


	// Use this for initialization
	void Start () 
	{
	}

	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void onEnable()
	{
		health = maxHealth;
	}

    public float GetHealth()
    {
        return health;
    }

	/**********************************************************
	 * updateHealthBar:
	 * 	@params health: New health of player
	 * This method fills the health bar will the new amount
	 * after the damage has been after coming into contact
	 * with the spike.
	 **********************************************************/
	void OnHealthChanged(float health)
	{
		if (isLocalPlayer)
			PlayerCanvas.canvas.SetHealth (health);
	}

    /**********************************************************
	 * TakeDamage:
	 * 	@params val: amount to damage
	 * This method lowers the player health by some value. 
	 * It then updates the health bar UI.
	 **********************************************************/
	[Server]
    public void TakeDamage(float val)
    {
		if (!isServer) 
		{
			return;
		}

        if (player.IsInvulnerable()) return;

        health -= val;
		Debug.Log (health);
        if (health < 0)
        {
            health = maxHealth;
            //dead = true;
        }
        //updateHealthBar(health);
    }
		

    /**********************************************************
	 * Heal:
	 * 	@params val: amount to heal
	 * This method heals the player by some value. 
	 * It then updates the health bar UI.
	 **********************************************************/
    public void Heal(float val)
    {
        health += val;
        if (health > maxHealth)
            health = maxHealth;

        //updateHealthBar (health);
    }

    /**********************************************************
	 * OnTriggerEnter2D():
	 * 	@params : Other RigidBody collider.
	 * This method is used to determine if the player has come
	 * into contact with the spike. If it has, it calls the
	 * function TakeDamage().
	 **********************************************************/
    void OnTriggerEnter2D(Collider2D other)
	{
		if(other.gameObject.name == "Spike")
		{
			TakeDamage (20);
		}
	}
		
}
