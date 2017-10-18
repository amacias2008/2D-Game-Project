using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour {

	public float health;						//Health of player
	public float maxHealth = 100.0f;			//Maximum health

	public Image GreenHealthBar;				//Used as foreground color of health bar
	public Image RedHealthBar;					//Used as background color of health bar


	//Set health equal to max health
	void Awake()
	{
		health = maxHealth;
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
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
	 * 	@params none
	 * This method takes away health by 20 when it comes into
	 * contact with the spike. 
	 * It then updates the health bar UI.
	 **********************************************************/
	void TakeDamage()
	{
		health -= 20;
		if (health == 0)
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
			TakeDamage ();
		}
	}
		
}
