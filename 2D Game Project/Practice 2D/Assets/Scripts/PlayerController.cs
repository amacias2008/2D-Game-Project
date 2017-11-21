using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour {
	public float speedForce = 50f;
	public Vector2 jumpVector;
	public Vector2 jumpVector2;
	bool facingRight;
	public bool isGrounded;
	public float speed;

	public Transform grounder;
	public float radiuss;
	public LayerMask ground;

	Animator anim;
	Rigidbody2D rb;
	public float health;
	public GameObject bulletRight, bulletLeft;
	Transform firePosition;

	void Awake()
	{

	}
	// Use this for initialization
	void Start () 
	{
		rb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		health = GetComponent<HealthScript> ().health;
		firePosition = transform.Find ("firePosition");
		facingRight = true;
		isGrounded = true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Movement ();
		Flip ();
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
		if (Input.GetKey (KeyCode.A)) 
		{
			speed = isGrounded ? -speedForce : -speedForce*0.8f;
			rb.velocity = new Vector2 (speed, rb.velocity.y);
			transform.localScale = new Vector3 (-3, 4, 1);
			anim.SetInteger ("AnimationState", 1);
		} 
		else if (Input.GetKey (KeyCode.D)) 
		{
			speed = isGrounded ? speedForce : speedForce*0.8f;
			rb.velocity = new Vector2 (speed, rb.velocity.y);
			transform.localScale = new Vector3 (3, 4, 1);
			anim.SetInteger ("AnimationState", 1);
		} 
		else if (isGrounded)
			rb.velocity = new Vector2 (0, rb.velocity.y);

		anim.SetInteger ("AnimationState", 0);


		isGrounded = Physics2D.OverlapCircle (grounder.transform.position, radiuss, ground);

		if(Input.GetKey(KeyCode.W) && isGrounded==true) 
		{
			rb.AddForce (jumpVector, ForceMode2D.Force);
		}
		else if (Input.GetKey(KeyCode.W))
		{
			rb.AddForce (jumpVector2, ForceMode2D.Force);
		}

		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			Fire ();
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
	 * Uses the facingRight boolean to determine if we are
	 * instantiating a bullet going left or going right.
	 * *******************************************************/
	void Fire()
	{
		if(facingRight) 
			Instantiate (bulletRight, firePosition.position, Quaternion.identity);
		else
			Instantiate (bulletLeft, firePosition.position, Quaternion.identity);
	}

	/**********************************************************
	 * This flip method is used to determine if we are facing
	 * right or facing left. Facing left is set when
	 * 1) Speed > 0 and we are facing left OR
	 * 2) Speed < 0 and we are facing right
	 * Uses a temp Vector variable to temporarily store the 
	 * local transform variables and then set it to the 
	 * opposite value.
	 * ********************************************************/
	void Flip()
	{
		if (speed > 0 && !facingRight || speed < 0 && facingRight) 
		{
			facingRight = !facingRight;
			Vector3 temp = transform.localScale;
			temp.x *= -1;
			transform.localScale = temp;
		}
	}
}
