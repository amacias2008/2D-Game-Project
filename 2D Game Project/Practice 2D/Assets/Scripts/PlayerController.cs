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
	 * Sets the current weapon equal to the collected weapon.
	 **********************************************************/
     public void EquipItem(int itemType)
    {
        weapon = itemType;
    }
}
