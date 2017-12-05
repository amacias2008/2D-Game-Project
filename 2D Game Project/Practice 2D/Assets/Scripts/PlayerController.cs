using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public int playerNumber = 1;

    //Movement variables
    public float baseWalkSpeed = 5;
    private float walkAccelerationGround = 0.6f;
    private float walkAccelerationAir = 0.3f;
    private float stopFrictionGround = 0.6f;
    private float stopFrictionAir = 0.1f;

    private float currentSpeed = 0;
    private float speedMult = 1;
    private float[] previousX = new float[5];

    private float disabledTimeRemaining = 0;
    private float disabledTimeDuration = 0.5f;

    //Jump variables
    public float jumpForce = 6;
    private float maxTime = 0.15f;
    private float timer;
    private int jumpsTotal = 2;
    private int jumpsCurrent;
    private float jumpMult = 1;

    public Transform foot1;
    public Transform foot2;
    public LayerMask groundMask;
    private bool isGrounded;
    private bool canJump;

    //Health
    HealthScript healthScript;
	public GameObject bulletPrefab;

    //Weapon variables
    private int weapon = 0;
    private int previousWeapon = 0;
    private float fireRateMult = 1;
    private float bulletSpeedBase = 0.1f;
    private float bulletSpeedMult = 1;
    private float bulletSpawnDist = 0.5f;

    private Vector2 lungeForce = new Vector2(3, 2);
    private Vector2 spearKnockbackForce = new Vector2(5, 3);
    private float swordReflectRadius = 1.5f;

    //Melee hitboxes
    public LayerMask playerMask;
    public Transform knifeHitboxA;
    public Transform knifeHitboxB;
    public Transform swordHitboxA;
    public Transform swordHitboxB;
    public Transform spearHitboxA;
    public Transform spearHitboxB;
    public Transform chainsawHitboxA;
    public Transform chainsawHitboxB;

    //Aiming variables
    private Vector2 aimVector = new Vector2(1, 0);
    private bool inputUp, inputDown, inputLeft, inputRight;
    private bool facingRight = true;

    //Weapon fire rates (in seconds of cooldown time)
    private float FireRateKnife = 0.5f;
    private float FireRateSword = 0.75f;
    private float FireRateSpear = 1f;
    private float FireRateChainsaw = 0.25f;
    private float FireRatePistol = 0.6f;
    private float FireRatePlasmaRifle = 0.3f;
    private float FireRateMinigun = 0.15f;

    //Powerup strength
    private float FuryFireRateMult = 2f;
    private float FuryBulletSpeedMult = 2f;
    private float AgilitySpeedMult = 1.5f;
    private float AgilityJumpMult = 1.2f;
    private float VigorRegenRate = 0.15f;

    //Powerup durations and weapon max ammo
    private int PlasmaRifleAmmoMax = 20;
    private float MinigunDuration = 10;
    private float ChainsawDuration = 10;
    private float FuryDuration = 10;
    private float AgilityDuration = 10;
    private float InvulnerabilityDuration = 3;
    private float RegenerationDuration = 10;

    //Weapon and Powerup status variables
    private int PlasmaRifleAmmoRemaining = 0;
    private float MinigunTimeRemaining = 0;
    private float ChainsawTimeRemaining = 0;
    private float FuryTimeRemaining = 0;
    private float AgilityTimeRemaining = 0;
    private float InvulnerabilityTimeRemaining = 0;
    private float RegenerationTimeRemaining = 0;
    private float TimeSinceLastAttack = 10;
   
    //Other
    public Text debugText;
    public Text debugText2;
    Vector2 spawnLoc;
    Animator anim;
    RoundManager rm;

    // Use this for initialization
    void Start()
    {
		anim = GetComponent<Animator> ();
        healthScript = GetComponent<HealthScript>();
        rm = GameObject.FindGameObjectsWithTag("RoundManager")[0].GetComponent<RoundManager>();

        previousWeapon = weapon;

        spawnLoc = transform.position;
    }

	public override void OnStartLocalPlayer ()
	{
		base.OnStartLocalPlayer ();
		gameObject.name = "Local";
	}
    // Update is called once per frame
    void Update()
    {
		CmdStartOver ();
        // Only run this Update function if the fight is active
		if (!rm.fightActive) return;

		if (!isLocalPlayer)
			return;
		
        UpdateTimeVariables();
        UpdatePowerupEffects();
        UpdateMovement();
        UpdateAiming();
        UpdateAttack();
		//StartOver ();
        //UpdateDebugText();
    }

	[Command]
	void CmdStartOver()
	{
		if (!rm.fightActive) 
		{

			if (Input.GetKeyDown (KeyCode.R)) 
			{
				Debug.Log ("hello");
				rm.NewRound ();
			} 

			if (Input.GetKeyDown (KeyCode.Escape)) 
			{
				rm.FullReset ();
			}
		}
	}

    // Update variables used for timers of powerups & weapons
    void UpdateTimeVariables()
    {
        float delta = Time.deltaTime;
        
        MinigunTimeRemaining -= delta;
        ChainsawTimeRemaining -= delta;
        FuryTimeRemaining -= delta;
        AgilityTimeRemaining -= delta;
        InvulnerabilityTimeRemaining -= delta;
        RegenerationTimeRemaining -= delta;
        disabledTimeRemaining -= delta;
        
        TimeSinceLastAttack += delta;

        // If Chainsaw/Minigun is equipped and duration is expired, equip previous weapon
        if (weapon == 3 && ChainsawTimeRemaining < 0) RpcEquipPreviousWeapon();
        if (weapon == 6 && MinigunTimeRemaining < 0) RpcEquipPreviousWeapon();
    }

    // Update variables affected by powerup effects
    void UpdatePowerupEffects()
    {
        UpdateFury();
        UpdateAgility();
        UpdateVigor();
    }

    // Update fire rate and bullet speed multipliers
    void UpdateFury()
    {
        // If powerup is active
        if (FuryTimeRemaining > 0)
        {
            fireRateMult = FuryFireRateMult;
            bulletSpeedMult = FuryBulletSpeedMult;
        }
        else
        {
            fireRateMult = 1;
            bulletSpeedMult = 1;
        }
    }

    // Update walk speed and jump power multipliers
    void UpdateAgility()
    {
        // If powerup is active
        if (AgilityTimeRemaining > 0)
        {
            speedMult = AgilitySpeedMult;
            jumpMult = AgilityJumpMult;
        }
        else
        {
            speedMult = 1;
            jumpMult = 1;
        }
    }

    // Health regeneration
    void UpdateVigor()
    {
        if(RegenerationTimeRemaining > 0)
        {
            healthScript.Heal(VigorRegenRate);
        }
    }

    // Returns whether the player is invulnerable
    public bool IsInvulnerable()
    {
        return InvulnerabilityTimeRemaining > 0;
    }

    // Update player movement using input
    void UpdateMovement()
    {
        UpdateHorizontalMovement();
        UpdateJumping();
    }

    // Get input and update X velocity using acceleration
    void UpdateHorizontalMovement()
    {
        // WALK LEFT
        if (playerNumber == 1 && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.LeftShift) ||
            playerNumber == 2 && Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightControl))
        {
            // walking acceleration
			anim.SetBool ("Moving", true);
            if (isGrounded)
                currentSpeed -= walkAccelerationGround;
            else
                currentSpeed -= walkAccelerationAir;
        }
        // WALK RIGHT
        else if (playerNumber == 1 && Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.LeftShift) ||
            playerNumber == 2 && Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.RightControl))
        {
            // walking acceleration
            anim.SetBool ("Moving", true);
            if (isGrounded)
                currentSpeed += walkAccelerationGround;
            else
                currentSpeed += walkAccelerationAir;
        }
        // NO INPUT
        else
        {
            // stopping friction
			anim.SetBool ("Moving", false);
            if (isGrounded) // on ground
            {
                if (currentSpeed > stopFrictionGround) // if moving right
                    currentSpeed -= stopFrictionGround; // decrease velocity by friction amount
                else if (currentSpeed < -stopFrictionGround) // else if moving left
                    currentSpeed += stopFrictionGround; // increase velocity by friction amount
                else // speed has been slowed enough to be less than friction amount, set equal to 0
                    currentSpeed = 0;
            }
            else // in air
            {
                if (currentSpeed > stopFrictionAir) // if moving right
                    currentSpeed -= stopFrictionAir; // decrease velocity by friction amount
                else if (currentSpeed < -stopFrictionAir) // else if moving left
                    currentSpeed += stopFrictionAir; // increase velocity by friction amount
                else // speed has been slowed enough to be less than friction amount, set equal to 0
                    currentSpeed = 0;
            }
        }

        // limit max walk speed
        if (currentSpeed > baseWalkSpeed)
            currentSpeed = baseWalkSpeed;
        else if (currentSpeed < -baseWalkSpeed)
            currentSpeed = -baseWalkSpeed;

        // prevent player movement if disabled by Spear hit
        if (disabledTimeRemaining > 0) currentSpeed = 0;

        // check if player rigidbody hit a wall while in air and should fall to the ground (was bug)
        CheckIfStuckFloating();

        // Return (don't apply X force) if lunging OR movement disabled by spear knockback
        if ((weapon == 0 && TimeSinceLastAttack < FireRateKnife) || (weapon == 2 && TimeSinceLastAttack < FireRateSpear / 2) || disabledTimeRemaining > 0) return;

        // apply X velocity
        if ((weapon == 3 || weapon == 6) && AgilityTimeRemaining < 0) // chainsaw and minigun slow
            GetComponent<Rigidbody2D>().velocity = new Vector2(currentSpeed * 0.5f, GetComponent<Rigidbody2D>().velocity.y);
        else // no slow
            GetComponent<Rigidbody2D>().velocity = new Vector2(currentSpeed * speedMult, GetComponent<Rigidbody2D>().velocity.y);
    }

    // Updates array with X location of player
    // Stores previous 5 locations
    // Used to detect if player is stuck on a wall (this function is a bugfix)
    void CheckIfStuckFloating()
    {
        previousX[4] = previousX[3];
        previousX[3] = previousX[2];
        previousX[2] = previousX[1];
        previousX[1] = previousX[0];
        previousX[0] = transform.position.x;

        float difference = Mathf.Abs(previousX[4] - previousX[0]);

        // if X hasn't changed in 4 updates AND not grounded AND speed variable is not zero
        // set speed to zero in order to fall
        if (difference < 0.1 && !isGrounded && Mathf.Abs(currentSpeed) > 3)
            currentSpeed = 0;
    }

    // Get input and update jumping
    void UpdateJumping()
    {
        isGrounded = Physics2D.OverlapArea(foot1.position, foot1.position, groundMask);

        // Key is pressed in air
        if ((playerNumber == 1 && Input.GetKeyDown(KeyCode.W) && !isGrounded && jumpsCurrent > 1 && AgilityTimeRemaining > 0) ||
            (playerNumber == 2 && Input.GetKeyDown(KeyCode.UpArrow) && !isGrounded && jumpsCurrent > 1 && AgilityTimeRemaining > 0))
        {
            jumpsCurrent--;
            timer = 0;
            canJump = true;
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumpForce * jumpMult); // jump
        }
        // Key is pressed on ground
        else if ((playerNumber == 1 && Input.GetKeyDown(KeyCode.W) && isGrounded && !Input.GetKey(KeyCode.LeftShift)) ||
            (playerNumber == 2 && Input.GetKeyDown(KeyCode.UpArrow) && isGrounded && !Input.GetKey(KeyCode.RightControl)))
        {
            timer = 0;
            canJump = true;
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumpForce * jumpMult); // jump
        }
        // Key is held down and timer hasn't reached maxTime
        else if ((playerNumber == 1 && Input.GetKey(KeyCode.W) && canJump && timer < maxTime) ||
            (playerNumber == 2 && Input.GetKey(KeyCode.UpArrow) && canJump && timer < maxTime))
        {
            timer += Time.deltaTime;
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumpForce * jumpMult); // jump
        }
        // Key is released or timer has reached maxTime
        else
        {
            canJump = false;
        }

        // Reset jump counter when grounded
        if (isGrounded)
            jumpsCurrent = jumpsTotal;
    }

    // Update aiming
    void UpdateAiming()
    {
        Flip();

        inputUp = (playerNumber == 1 && Input.GetKey(KeyCode.W)) || (playerNumber == 2 && Input.GetKey(KeyCode.UpArrow));
        inputDown = (playerNumber == 1 && Input.GetKey(KeyCode.S)) || (playerNumber == 2 && Input.GetKey(KeyCode.DownArrow));
        inputLeft = (playerNumber == 1 && Input.GetKey(KeyCode.A)) || (playerNumber == 2 && Input.GetKey(KeyCode.LeftArrow));
        inputRight = (playerNumber == 1 && Input.GetKey(KeyCode.D)) || (playerNumber == 2 && Input.GetKey(KeyCode.RightArrow));

        if (inputUp)
        {
            if(inputLeft)
            {
                aimVector = new Vector2(-1, 1).normalized; // up-left
            }
            else if(inputRight)
            {
                aimVector = new Vector2(1, 1).normalized; // up-right
            }
            else
            {
                aimVector = new Vector2(0, 1).normalized; // up
            }
        }
        else if (inputDown)
        {
            if (inputLeft)
            {
                aimVector = new Vector2(-1, -1).normalized; // down-left
            }
            else if (inputRight)
            {
                aimVector = new Vector2(1, -1).normalized; // down-right
            }
            else
            {
                aimVector = new Vector2(0, -1).normalized; // down
            }
        }
        else
        {
            if (inputLeft) 
            {
                aimVector = new Vector2(-1, 0).normalized; // left
            }
            else if (inputRight)
            {
                aimVector = new Vector2(1, 0).normalized; // right
            }
            else
            {
                if (facingRight) aimVector = new Vector2(1, 0).normalized; // right
                else aimVector = new Vector2(-1, 0).normalized; // left
            }
        }
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
        if ((playerNumber == 1 && !Input.GetKey(KeyCode.LeftShift)) || (playerNumber == 2 && !Input.GetKey(KeyCode.RightControl)))
        {
            if (currentSpeed > 0 && !facingRight || currentSpeed < 0 && facingRight)
            {
                facingRight = !facingRight;
                Vector3 temp = transform.localScale;
                temp.x *= -1;
                transform.localScale = temp;
            }
        }
        else 
        {
            if (aimVector.x > 0 && !facingRight || aimVector.x < 0 && facingRight)
            {
                facingRight = !facingRight;
                Vector3 temp = transform.localScale;
                temp.x *= -1;
                transform.localScale = temp;
            }
        }
    }

    // Get player input for attacking
    void UpdateAttack()
    {
        // While Fire key is held down, attempt to attack
        if ((playerNumber == 1 && Input.GetKey(KeyCode.Space)) || (playerNumber == 2 && Input.GetKey(KeyCode.Keypad0)))
        {

            AttemptAttack ();
		}
        // If Chainsaw or Minigun is equipped, constantly attack
        else if (weapon == 3 || weapon == 6)
            AttemptAttack();
    }

   // Check if equipped weapon is ready to be fired
    void AttemptAttack()
    {
        switch (weapon)
        {
		case 0:
			if (TimeSinceLastAttack * fireRateMult > FireRateKnife) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
				GetComponent<Animator> ().SetTrigger ("Melee");
			}
                break;
		case 1:
			if (TimeSinceLastAttack * fireRateMult > FireRateSword) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
				GetComponent<Animator> ().SetTrigger ("Melee");
			}
                break;
		case 2:
			if (TimeSinceLastAttack * fireRateMult > FireRateSpear) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
				GetComponent<Animator> ().SetTrigger ("Melee");
			}
                break;
		case 3:
			if (TimeSinceLastAttack * fireRateMult > FireRateChainsaw) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
			}
                break;
		case 4:
			if (TimeSinceLastAttack * fireRateMult > FireRatePistol) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
				GetComponent<Animator> ().SetTrigger ("Shooting1");
			}
                break;
		case 5:
			if (TimeSinceLastAttack * fireRateMult > FireRatePlasmaRifle) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
				GetComponent<Animator> ().SetTrigger ("Shooting1");
			}
                break;
		case 6:
			if (TimeSinceLastAttack * fireRateMult > FireRateMinigun) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
				GetComponent<Animator> ().SetTrigger ("Shooting1");
			}
                break;
            default:
                break;
        }
    }

    // Attack using the equipped weapon
	[Command]
    void CmdAttack()
    {

        // Fire bullet
        if (weapon > 3)
        {
			
			/*GameObject go = (GameObject)Instantiate(Resources.Load("Bullet"));
            go.transform.position = transform.position + new Vector3(aimVector.x, aimVector.y, 0) * bulletSpawnDist;
            BulletController bullet = go.GetComponent<BulletController>();
            bullet.SetVelocity(aimVector * bulletSpeedBase * bulletSpeedMult);*/

			var networkBullet = (GameObject)Instantiate 
				(bulletPrefab, transform.position + new Vector3(aimVector.x, aimVector.y, 0) * bulletSpawnDist, transform.rotation);
			//networkBullet.transform.position = transform.position + new Vector3 (aimVector.x, aimVector.y, 0) * bulletSpawnDist;

			BulletController bullet = networkBullet.GetComponent<BulletController>();
			bullet.SetVelocity (aimVector * bulletSpeedBase * bulletSpeedMult);

			networkBullet.GetComponent<Rigidbody2D> ().velocity = bullet.GetVelocity();

			NetworkServer.Spawn(networkBullet);

            // Plasma Rifle
            if (weapon == 5)
            {
                bullet.SetRicochet(true);
                PlasmaRifleAmmoRemaining--;
                if (PlasmaRifleAmmoRemaining <= 0) RpcEquipPreviousWeapon();
            }
        }
        // Melee attack
        else CheckMeleeHit();
    }

    // Check if melee attack hit enemy
    void CheckMeleeHit()
    {
        if (weapon == 0) // Knife
        {
            // Lunge
            if (facingRight)
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x + lungeForce.x,
                    GetComponent<Rigidbody2D>().velocity.y + lungeForce.y);
            else
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x - lungeForce.x,
                        GetComponent<Rigidbody2D>().velocity.y + lungeForce.y);

            if (Physics2D.OverlapArea(knifeHitboxA.position, knifeHitboxB.position, playerMask))
            {
                Collider2D hit = Physics2D.OverlapAreaAll(knifeHitboxA.position, knifeHitboxB.position, playerMask)[0];
                if (hit.gameObject.tag == "Player")
                {
                    HealthScript h = hit.gameObject.GetComponent<HealthScript>();
                    h.TakeDamage(10);
                }
                
            }
        }
        else if (weapon == 1) // Sword
        {
            if (Physics2D.OverlapArea(swordHitboxA.position, swordHitboxB.position, playerMask))
            {
                Collider2D hit = Physics2D.OverlapAreaAll(swordHitboxA.position, swordHitboxB.position, playerMask)[0];
                if (hit.gameObject.tag == "Player")
                {
                    HealthScript h = hit.gameObject.GetComponent<HealthScript>();
                    h.TakeDamage(20);
                }
            }
            // check if any bullets should be reflected (bullet is close enough AND approaching the player AND the player is facing the bullet)
            var bullets = GameObject.FindGameObjectsWithTag("Bullet");
            foreach (var b in bullets) // loop through all bullets
            {
                if (Vector2.Distance(transform.position, b.transform.position) < swordReflectRadius) // check if within sword radius
                {
                    BulletController bc = b.gameObject.GetComponent<BulletController>();
                    if ((bc.GetVelocity().x > 0 && b.transform.position.x < transform.position.x) ||
                        (bc.GetVelocity().x < 0 && b.transform.position.x > transform.position.x)) // check if bullet is approaching player (X value only)
                    {
                        if ((facingRight && transform.position.x < b.transform.position.x) ||
                            (!facingRight && transform.position.x > b.transform.position.x)) // check if player is facing correct X direction
                        {
                            // Change bullet velocity to move toward enemy player
                            Vector2 thisPLayerLoc = transform.position;
                            Vector2 playerLocA = GameObject.FindGameObjectsWithTag("Player")[0].transform.position;
                            Vector2 playerLocB = GameObject.FindGameObjectsWithTag("Player")[1].transform.position;
                            Vector2 targetAimLoc = new Vector2(0, 0);
            
                            if(thisPLayerLoc == playerLocA) targetAimLoc = playerLocB;
                            else targetAimLoc = playerLocA;

                            float speedOfBullet = bc.GetVelocity().magnitude;
                            Vector2 newVelocity = (targetAimLoc - new Vector2(b.transform.position.x, b.transform.position.y)).normalized * speedOfBullet;
                            bc.SetVelocity(newVelocity);
                        }
                    }
                }
            }
        }
        else if (weapon == 2) // Spear
        {
            // Lunge
            if (facingRight)
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x + lungeForce.x,
                    GetComponent<Rigidbody2D>().velocity.y + lungeForce.y);
            else
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x - lungeForce.x,
                        GetComponent<Rigidbody2D>().velocity.y + lungeForce.y);

            if (Physics2D.OverlapArea(spearHitboxA.position, spearHitboxB.position, playerMask))
            {
                Collider2D hit = Physics2D.OverlapAreaAll(spearHitboxA.position, spearHitboxB.position, playerMask)[0];
                if (hit.gameObject.tag == "Player")
                {
                    HealthScript h = hit.gameObject.GetComponent<HealthScript>();
                    h.TakeDamage(20);

                    // Knockback
                    PlayerController p = hit.gameObject.GetComponent<PlayerController>();
                    bool b = hit.gameObject.transform.position.x > transform.position.x;
                    p.KnockedBack(b);
                }
            }
        }
        else if (weapon == 3) // Chainsaw
        {
            if (Physics2D.OverlapArea(chainsawHitboxA.position, chainsawHitboxB.position, playerMask))
            {
                Collider2D hit = Physics2D.OverlapAreaAll(chainsawHitboxA.position, chainsawHitboxB.position, playerMask)[0];
                if (hit.gameObject.tag == "Player")
                {
                    HealthScript h = hit.gameObject.GetComponent<HealthScript>();
                    h.TakeDamage(100);
                }

            }
        }
    }

    // Hit by enemy Spear
    public void KnockedBack(bool toTheRight)
    {
        disabledTimeRemaining = disabledTimeDuration;
        currentSpeed = 0;

        if(toTheRight) // Knockback
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x + spearKnockbackForce.x, GetComponent<Rigidbody2D>().velocity.y + spearKnockbackForce.y);
        else
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x - spearKnockbackForce.x, GetComponent<Rigidbody2D>().velocity.y + spearKnockbackForce.y);
    }

    // Called when the Player walks over an Item Pickup
    public void EquipItem(int itemType)
    {
        switch (itemType)
        {
            case 1:
                RpcEquipSword();
                break;
            case 2:
                RpcEquipSpear();
                break;
            case 3:
                RpcEquipChainsaw();
                break;
            case 4:
                RpcEquipPistol();
                break;
            case 5:
                RpcEquipPlasmaRifle();
                break;
            case 6:
                RpcEquipMinigun();
                break;
            case 7:
                RpcEquipFury();
                break;
            case 8:
                RpcEquipAgility();
                break;
            case 9:
                RpcEquipVigor();
                break;
            default:
                break;
        }

        // This readies an attack after picking up a weapon
        if(itemType < 7)
            TimeSinceLastAttack = 10;
    }

	// Item 1 Collected
	[ClientRpc]
	void RpcEquipSword()
	{
		//Debug.Log("Player equipped Sword");
		weapon = 1;
	}

	// Item 2 Collected
	[ClientRpc]
	void RpcEquipSpear()
	{
		//Debug.Log("Player equipped Spear");
		weapon = 2;
	}

	// Item 3 Collected
	[ClientRpc]
	void RpcEquipChainsaw()
	{
		//Debug.Log("Player equipped Chainsaw");

		// only update previousWeapon if player is not already holding Chainsaw, Rifle, or Minigun
		if (!(weapon == 3 || weapon == 5 || weapon == 6))
			previousWeapon = weapon;
		weapon = 3;

		ChainsawTimeRemaining = ChainsawDuration;
	}

	// Item 4 Collected
	[ClientRpc]
	void RpcEquipPistol()
	{
		//Debug.Log("Player equipped Pistol");
		weapon = 4;
	}

	// Item 5 Collected
	[ClientRpc]
	void RpcEquipPlasmaRifle()
	{
		//Debug.Log("Player equipped Plasma Rifle");

		// only update previousWeapon if player is not already holding Chainsaw, Rifle, or Minigun
		if (!(weapon == 3 || weapon == 5 || weapon == 6)) previousWeapon = weapon;
		weapon = 5;

		PlasmaRifleAmmoRemaining = PlasmaRifleAmmoMax;
	}

	// Item 6 Collected
	[ClientRpc]
	void RpcEquipMinigun()
	{
		//Debug.Log("Player equipped Minigun");

		// only update previousWeapon if player is not already holding Chainsaw, Rifle, or Minigun
		if (!(weapon == 3 || weapon == 5 || weapon == 6)) previousWeapon = weapon;
		weapon = 6;

		MinigunTimeRemaining = MinigunDuration;
	}

	// Item 7 Collected
	[ClientRpc]
	void RpcEquipFury()
	{
		//Debug.Log("Player equipped Fury");
		FuryTimeRemaining = FuryDuration;
	}

	// Item 8 Collected
	[ClientRpc]
	void RpcEquipAgility()
	{
		//Debug.Log("Player equipped Agility");
		AgilityTimeRemaining = AgilityDuration;
	}

	// Item 9 Collected
	[ClientRpc]
	void RpcEquipVigor()
	{
		//Debug.Log("Player equipped Vigor");
		InvulnerabilityTimeRemaining = InvulnerabilityDuration;
		RegenerationTimeRemaining = RegenerationDuration;
	}

    // Knives do not spawn as items.
    // Only needed for use in EquipPreviousWeapon()
	[ClientRpc]
    void RpcEquipKnife()
    {
        //Debug.Log("Player equipped Knife");
        weapon = 0;
    }

    // After the player's Chainsaw, Rifle, or Minigun expires, equip the previous weapon
	[ClientRpc]
	void RpcEquipPreviousWeapon()
    {
        switch (previousWeapon)
        {
            // previousWeapon should not be Chainsaw, Rifle, or Minigun (3, 5, or 6)
            case 0:
                RpcEquipKnife();
                break;
            case 1:
                RpcEquipSword();
                break;
            case 2:
                RpcEquipSpear();
                break;
            case 4:
                RpcEquipPistol();
                break;
            default:
                break;
        }
    }

    //
    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    //
    public float GetHealth()
    {
        return healthScript.GetHealth();
    }

    //
    public float GetY()
    {
        return transform.position.y;
    }

    // Equip knife, fill health, clear all powerup effects, and move to spawn
    public void NewRound()
    {
        previousWeapon = weapon = 0;
        TimeSinceLastAttack = 10;
        healthScript.Heal(1000);

        FuryTimeRemaining = 0;
        AgilityTimeRemaining = 0;
        InvulnerabilityTimeRemaining = 0;
        RegenerationTimeRemaining = 0;

        transform.position = spawnLoc;
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        currentSpeed = 0;
    }

    //
    public void IdleAnim()
    {
        anim.SetBool("Moving", false);
    }

    // Update Text fields with debug info
    void UpdateDebugText()
    {
        debugText.text = "";
        switch (weapon)
        {
            case 0:
                debugText.text += "Knife";
                break;
            case 1:
                debugText.text += "Sword";
                break;
            case 2:
                debugText.text += "Spear";
                break;
            case 3:
                debugText.text += "Chainsaw";
                break;
            case 4:
                debugText.text += "Pistol";
                break;
            case 5:
                debugText.text += "Plasma Rifle";
                break;
            case 6:
                debugText.text += "Minigun";
                break;
            default:
                break;
        }

        debugText2.text = "";
        if (weapon == 0)
        {
            debugText2.text += "Attack: ";
            if (TimeSinceLastAttack * fireRateMult > FireRateKnife) debugText2.text += "Ready \n";
            else debugText2.text += (FireRateKnife - (TimeSinceLastAttack * fireRateMult)) + " \n";
        }
        else if (weapon == 1)
        {
            debugText2.text += "Attack: ";
            if (TimeSinceLastAttack * fireRateMult > FireRateSword) debugText2.text += "Ready \n";
            else debugText2.text += (FireRateSword - (TimeSinceLastAttack * fireRateMult)) + " \n";
        }
        else if (weapon == 2)
        {
            debugText2.text += "Attack: ";
            if (TimeSinceLastAttack * fireRateMult > FireRateSpear) debugText2.text += "Ready \n";
            else debugText2.text += (FireRateSpear - (TimeSinceLastAttack * fireRateMult)) + " \n";
        }
        else if (weapon == 3)
            debugText2.text += "Chainsaw: " + ChainsawTimeRemaining + "\n";

        else if (weapon == 4)
            debugText2.text += "Ammo: Unlimited \n";

        else if (weapon == 5)
            debugText2.text += "Ammo: " + PlasmaRifleAmmoRemaining + "\n";

        else if (weapon == 6)
            debugText2.text += "Minigun: " + MinigunTimeRemaining + "\n";


        if(weapon == 3 || weapon == 5 || weapon == 6)
        {
            debugText2.text += "Next: ";
            switch (previousWeapon)
            {
                case 0:
                    debugText2.text += "Knife \n";
                    break;
                case 1:
                    debugText2.text += "Sword \n";
                    break;
                case 2:
                    debugText2.text += "Spear \n";
                    break;
                case 4:
                    debugText2.text += "Pistol \n";
                    break;
                default:
                    break;
            }
        }

        if (InvulnerabilityTimeRemaining > 0)
            debugText2.text += "Invuln: " + InvulnerabilityTimeRemaining + "\n";

        if (RegenerationTimeRemaining > 0)
            debugText2.text += "Regen: " + RegenerationTimeRemaining + "\n";

        if (AgilityTimeRemaining > 0)
            debugText2.text += "Agility: " + AgilityTimeRemaining + "\n";

        if (FuryTimeRemaining > 0)
            debugText2.text += "Fury: " + FuryTimeRemaining + "\n";
    }
}