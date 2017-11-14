using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour {
	public float speedForce = 50f;
	public Vector2 jumpVector;
	public Vector2 jumpVector2;
	public bool isGrounded;
	float speed;

	public Transform grounder;
	public float radiuss;
	public LayerMask ground;

	Animator anim;

	public float health;

    public int weapon = 0; 
	private int previousWeapon = 0;

	private float FireRateKnife = 1f;
	private float FireRateSword = 2f;
	private float FireRateSpear = 2f;
	private float FireRateChainsaw = 0f;
	private float FireRatePistol = 0.5f;
	private float FireRatePlasmaRifle = 0.25f;
	private float FireRateMinigun = 0.1f;

	private int PlasmaRifleAmmoMax = 20;
	private float MinigunDuration = 10;
	private float ChainsawDuration = 10;
	private float FuryDuration = 10;
	private float AgilityDuration = 10;
	private float InvincibilityDuration = 3;
	private float RegenerationDuration = 13;

	private int PlasmaRifleAmmoRemaining = 0;
	private float MinigunTimeRemaining = 0;
	private float ChainsawTimeRemaining = 0;
	private float FuryTimeRemaining = 0;
	private float AgilityTimeRemaining = 0;
	private float InvincibilityTimeRemaining = 0;
	private float RegenerationTimeRemaining = 0;
	private float TimeSinceLastAttack = 0;

	void Awake()
	{

	}

	// Use this for initialization
	void Start () 
	{
		anim = GetComponent<Animator> ();
		health = GetComponent<HealthScript> ().health;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Movement ();
		UpdateTimeVariables (Time.deltaTime);
	}
		
	/**********************************************************
	 * This following method is in charge of all movement
	 * from the player as well animation.
	 * A = Left
	 * D = Right
	 * W = Up
	 * ******************************************************/
	void Movement()
	{
		if (Input.GetKey (KeyCode.A)) {
			speed = isGrounded ? speedForce : speedForce*0.8f;
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (-speed, GetComponent<Rigidbody2D> ().velocity.y);
			transform.localScale = new Vector3 (-3, 4, 1);
			anim.SetInteger ("AnimationState", 1);
		} else if (Input.GetKey (KeyCode.D)) {
			speed = isGrounded ? speedForce : speedForce*0.8f;
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (speed, GetComponent<Rigidbody2D> ().velocity.y);
			transform.localScale = new Vector3 (3, 4, 1);
			anim.SetInteger ("AnimationState", 1);
		} else if (isGrounded)
			GetComponent<Rigidbody2D> ().velocity = new Vector2 (0, GetComponent<Rigidbody2D> ().velocity.y);
		anim.SetInteger ("AnimationState", 0);


		isGrounded = Physics2D.OverlapCircle (grounder.transform.position, radiuss, ground);

		if(Input.GetKey(KeyCode.W) && isGrounded==true) 
		{
			GetComponent<Rigidbody2D>().AddForce (jumpVector, ForceMode2D.Force);
		}
		else if (Input.GetKey(KeyCode.W))
		{
			GetComponent<Rigidbody2D>().AddForce (jumpVector2, ForceMode2D.Force);
		}
	}

	/**********************************************************
	 * Update all timing variables using deltaTime.
	 * *******************************************************/
	void UpdateTimeVariables(float delta)
	{
		MinigunTimeRemaining -= delta;
		ChainsawTimeRemaining -= delta;
		FuryTimeRemaining -= delta;
		AgilityTimeRemaining -= delta;
		InvincibilityTimeRemaining -= delta;
		RegenerationTimeRemaining -= delta;

		TimeSinceLastAttack += delta;

		if (weapon == 3 && ChainsawTimeRemaining < 0) EquipPreviousWeapon ();
		if (weapon == 6 && MinigunTimeRemaining < 0) EquipPreviousWeapon ();
	}

	/**********************************************************
	 * updateHealth: Sets the current health equal to the
	 * health located in HealthScript.
	 * *******************************************************/
	void updateHealth()
	{
		health = GetComponent<HealthScript> ().health;
	}

	/**********************************************************
	 * Triggers updateHealth when the player comes into contact
	 * with spike.
	 **********************************************************/
	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.gameObject.name == "Spike")
		{
            updateHealth ();
		}
    }

	/**********************************************************
	 * Check if weapon is ready to be fired.
	 **********************************************************/
	void AttemptAttack()
	{
		switch (weapon) {
		case 0:
			if(TimeSinceLastAttack > FireRateKnife) Attack();
			break;
		case 1:
			if(TimeSinceLastAttack > FireRateSword) Attack();
			break;
		case 2:
			if(TimeSinceLastAttack > FireRateSpear) Attack();
			break;
		case 3:
			if(TimeSinceLastAttack > FireRateChainsaw) Attack();
			break;
		case 4:
			if(TimeSinceLastAttack > FireRatePistol) Attack();
			break;
		case 5:
			if(TimeSinceLastAttack > FireRatePlasmaRifle) Attack();
			break;
		case 6:
			if(TimeSinceLastAttack > FireRateMinigun) Attack();
			break;
		}
	}

	/**********************************************************
	 * Attack using the current weapon.
	 **********************************************************/
	void Attack()
	{
		TimeSinceLastAttack = 0;

		if (weapon == 5) {
			PlasmaRifleAmmoRemaining--;
			if (PlasmaRifleAmmoRemaining <= 0) EquipPreviousWeapon ();
		}
	}

    /**********************************************************
	 * Sets the current weapon equal to the collected weapon.
	 **********************************************************/
	public void EquipItem(int itemType)
	{
		switch (itemType)
		{
		case 1:
			EquipSword();
			break;
		case 2:
			EquipSpear();
			break;
		case 3:
			EquipChainsaw();
			break;
		case 4:
			EquipPistol();
			break;
		case 5:
			EquipPlasmaRifle();
			break;
		case 6:
			EquipMinigun();
			break;
		case 7:
			EquipFury();
			break;
		case 8:
			EquipAgility();
			break;
		case 9:
			EquipVigor();
			break;
		default:
			break;
		}
    }

	void EquipSword()
	{
		Debug.Log ("Player equipped Sword");
		weapon = 1;
	}		

	void EquipSpear()
	{
		Debug.Log ("Player equipped Spear");
		weapon = 2;
	}

	void EquipChainsaw()
	{
		Debug.Log ("Player equipped Chainsaw");

		if(!(weapon == 3 || weapon == 5 || weapon == 6)) previousWeapon = weapon;
		weapon = 3;

		ChainsawTimeRemaining = ChainsawDuration;
	}

	void EquipPistol()
	{
		Debug.Log ("Player equipped Pistol");
		weapon = 4;
	}

	void EquipPlasmaRifle()
	{
		Debug.Log ("Player equipped Plasma Rifle");

		if(!(weapon == 3 || weapon == 5 || weapon == 6)) previousWeapon = weapon;
		weapon = 5;

		PlasmaRifleAmmoRemaining = PlasmaRifleAmmoMax;
	}

	void EquipMinigun()
	{
		Debug.Log ("Player equipped Minigun");

		if(!(weapon == 3 || weapon == 5 || weapon == 6)) previousWeapon = weapon;
		weapon = 6;

		MinigunTimeRemaining = MinigunDuration;
	}

	void EquipFury()
	{
		Debug.Log ("Player equipped Fury");
		
		FuryTimeRemaining = FuryDuration;
	}

	void EquipAgility()
	{
		Debug.Log ("Player equipped Agility");
		
		AgilityTimeRemaining = AgilityDuration;
	}

	void EquipVigor()
	{
		Debug.Log ("Player equipped Vigor");
		
		InvincibilityTimeRemaining = InvincibilityDuration;
		RegenerationTimeRemaining = RegenerationDuration;
	}

	void EquipPreviousWeapon()
	{
		switch (previousWeapon)
		{
		case 1:
			EquipSword();
			break;
		case 2:
			EquipSpear();
			break;
		case 4:
			EquipPistol();
			break;
		default:
			break;
		}
	}
}