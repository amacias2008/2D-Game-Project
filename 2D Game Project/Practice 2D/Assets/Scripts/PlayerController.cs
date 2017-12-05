using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    //Movement variables
    public float baseWalkSpeed = 5;
    public float walkAccelerationGround = 0.6f;
    public float walkAccelerationAir = 0.3f;
    public float stopFrictionGround = 0.6f;
    public float stopFrictionAir = 0;

    private float currentSpeed = 0;
    private float speedMult = 1;

    //Jump variables
    public float jumpForce = 6;
    public float maxTime = 0.15f;
    private float timer;
    public int jumpsTotal = 2;
    private int jumpsCurrent;
    private float jumpMult = 1;

    public Transform point1;
    public Transform point2;
    public LayerMask groundMask;
    private bool isGrounded;
    private bool canJump;

    //Health
    HealthScript healthScript;
	public GameObject bulletPrefab;


    //Weapon variables
    public int weapon = 0;
    private int previousWeapon = 0;
    private float fireRateMult = 1;
    private float bulletSpeedBase = 5.0f;
    private float bulletSpeedMult = 1;
    private float bulletSpawnDist = 0.5f;

    //Aiming variables
    private Vector2 aimVector = new Vector2(1, 0);
    private bool inputUp, inputDown, inputLeft, inputRight;
    private bool facingRight = true;

    //Weapon fire rates (in seconds of cooldown time)
    private float FireRateKnife = 1f;
    private float FireRateSword = 2f;
    private float FireRateSpear = 2f;
    private float FireRateChainsaw = 0.1f;
    private float FireRatePistol = 0.5f;
    private float FireRatePlasmaRifle = 0.3f;
    private float FireRateMinigun = 0.15f;

    //Powerup strength
    private float FuryFireRateMult = 2f;
    private float FuryBulletSpeedMult = 2f;
    private float AgilitySpeedMult = 1.5f;
    private float AgilityJumpMult = 1.2f;
    private float VigorRegenRate = 0.1f;

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
   
	public Text debugText;
	public Text debugText2;

    // Use this for initialization
    void Start()
    {
        healthScript = GetComponent<HealthScript>();
        previousWeapon = weapon;
    }

    // Update is called once per frame
    void Update()
    {
		if (!isLocalPlayer) 
		{
			return;
		}
        UpdateTimeVariables();
        UpdatePowerupEffects();
        UpdateMovement();
        UpdateAiming();
        UpdateAttack();

        //UpdateDebugText();
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
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.LeftShift))
        {
            // walking acceleration
            if (isGrounded)
                currentSpeed -= walkAccelerationGround;
            else
                currentSpeed -= walkAccelerationAir;
        }
        // WALK RIGHT
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.LeftShift))
        {
            // walking acceleration
            if (isGrounded)
                currentSpeed += walkAccelerationGround;
            else
                currentSpeed += walkAccelerationAir;
        }
        // NO INPUT
        else
        {
            // stopping friction
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

        // apply X velocity using speed multiplier
        GetComponent<Rigidbody2D>().velocity = new Vector2(currentSpeed * speedMult, GetComponent<Rigidbody2D>().velocity.y);
    }

    // Get input and update jumping
    void UpdateJumping()
    {
        isGrounded = Physics2D.OverlapArea(point1.position, point2.position, groundMask);

        // Key is pressed in air
        if (Input.GetKeyDown(KeyCode.W) && !isGrounded && jumpsCurrent > 1 && AgilityTimeRemaining > 0)
        {
            jumpsCurrent--;
            timer = 0;
            canJump = true;
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumpForce * jumpMult); // jump
        }
        // Key is pressed on ground
        else if (Input.GetKeyDown(KeyCode.W) && isGrounded && !Input.GetKey(KeyCode.LeftShift))
        {
            timer = 0;
            canJump = true;
            GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, jumpForce * jumpMult); // jump
        }
        // Key is held down and timer hasn't reached maxTime
        else if (Input.GetKey(KeyCode.W) && canJump && timer < maxTime)
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

        inputUp = Input.GetKey(KeyCode.W);
        inputDown = Input.GetKey(KeyCode.S);
        inputLeft = Input.GetKey(KeyCode.A);
        inputRight = Input.GetKey(KeyCode.D);

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
        if (!Input.GetKey(KeyCode.LeftShift))
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
		if (Input.GetKey (KeyCode.Space)) {
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
			}
				break;
		case 1:
			if (TimeSinceLastAttack * fireRateMult > FireRateSword) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
			}
                break;
		case 2:
			if (TimeSinceLastAttack * fireRateMult > FireRateSpear) {
				TimeSinceLastAttack = 0;    
				CmdAttack ();
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
			}
                break;
		case 5:
			if (TimeSinceLastAttack * fireRateMult > FireRatePlasmaRifle) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
			}
                break;
		case 6:
			if (TimeSinceLastAttack * fireRateMult > FireRateMinigun) {
				TimeSinceLastAttack = 0;
				CmdAttack ();
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
		Debug.Log ("FIRE2");
		//Debug.Log (TimeSinceLastAttack);
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

			networkBullet.GetComponent<Rigidbody2D> ().velocity = bullet.vel;


			NetworkServer.Spawn(networkBullet);

            if (weapon == 5)
                bullet.SetRicochet (true);
        }

        // if Plasma Rifle is equipped, update ammo
        if (weapon == 5)
        {
            PlasmaRifleAmmoRemaining--;
            if (PlasmaRifleAmmoRemaining <= 0)
                RpcEquipPreviousWeapon();
        }
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

    // Update Text fields with debug info
    void UpdateDebugText()
    {
		PlayerCanvas.canvas.WriteDebugText("");
        switch (weapon)
        {
            case 0:
				PlayerCanvas.canvas.WriteDebugText("Knife");
                break;
            case 1:
				PlayerCanvas.canvas.WriteDebugText("Sword");
                break;
            case 2:
				PlayerCanvas.canvas.WriteDebugText("Spear");
                break;
            case 3:
				PlayerCanvas.canvas.WriteDebugText("Chainsaw");
                break;
            case 4:
				PlayerCanvas.canvas.WriteDebugText("Pistol");
                break;
            case 5:
				PlayerCanvas.canvas.WriteDebugText("Plasma Rifle");
                break;
            case 6:
				PlayerCanvas.canvas.WriteDebugText("Minigun");
                break;
            default:
                break;
        }

		PlayerCanvas.canvas.WriteDebugTextTwo("");
        if (weapon == 0)
        {
			PlayerCanvas.canvas.WriteDebugTextTwo("Attack: ");
			if (TimeSinceLastAttack * fireRateMult > FireRateKnife) PlayerCanvas.canvas.WriteDebugTextTwo("Ready \n");
			else PlayerCanvas.canvas.WriteDebugTextTwo( (FireRateKnife - (TimeSinceLastAttack * fireRateMult)) + " \n" );
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