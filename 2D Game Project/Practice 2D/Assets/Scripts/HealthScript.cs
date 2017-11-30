using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class HealthScript : NetworkBehaviour {

	public float maxHealth = 100.0f;			//Maximum health
    public float health;                       //Health of player

    public Image GreenHealthBar;				//Used as foreground color of health bar
	public Image RedHealthBar;					//Used as background color of health bar

    PlayerController player;

	//Set health equal to max health
	void Awake()
	{
		health = maxHealth;
	}

	// Use this for initialization
	void Start () 
	{
        player = GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		
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
	void updateHealthBar(float health)
	{
		GreenHealthBar.fillAmount = health / maxHealth;
	}

    /**********************************************************
	 * TakeDamage:
	 * 	@params val: amount to damage
	 * This method lowers the player health by some value. 
	 * It then updates the health bar UI.
	 **********************************************************/
    public void TakeDamage(float val)
    {
		if (!isServer) 
		{
			return;
		}
        if (player.IsInvulnerable()) return;

        health -= val;
        if (health < 0)
        {
            health = maxHealth;
            //dead = true;
        }
        updateHealthBar(health);
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

        updateHealthBar (health);
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
