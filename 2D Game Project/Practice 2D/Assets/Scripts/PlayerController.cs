using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
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

    //Weapon variables
    public int weapon = 0;
    private int previousWeapon = 0;
    private float fireRateMult = 1;
    private float bulletSpeedMult = 1;

    //Weapon fire rates (in seconds of cooldown time)
    private float FireRateKnife = 1f;
    private float FireRateSword = 2f;
    private float FireRateSpear = 2f;
    private float FireRateChainsaw = 0.1f;
    private float FireRatePistol = 0.5f;
    private float FireRatePlasmaRifle = 0.3f;
    private float FireRateMinigun = 0.15f;

    //Powerup strength
    private float FuryFireRateMult = 1.2f;
    private float FuryBulletSpeedMult = 1.5f;
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
    private float RegenerationDuration = 13;

    //Weapon and Powerup status variables
    private int PlasmaRifleAmmoRemaining = 0;
    private float MinigunTimeRemaining = 0;
    private float ChainsawTimeRemaining = 0;
    private float FuryTimeRemaining = 0;
    private float AgilityTimeRemaining = 0;
    private float InvulnerabilityTimeRemaining = 0;
    private float RegenerationTimeRemaining = 0;
    private float TimeSinceLastAttack = 0;

    //Animation
    Animator anim;

    //public AudioClip footstep;

    public Text debugText;


    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        healthScript = GetComponent<HealthScript>();
        previousWeapon = weapon;
        debugText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimeVariables();
        UpdatePowerupEffects();
        UpdateMovement();
        UpdateAttack();

        UpdateDebugText();
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
        if (weapon == 3 && ChainsawTimeRemaining < 0) EquipPreviousWeapon();
        if (weapon == 6 && MinigunTimeRemaining < 0) EquipPreviousWeapon();
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
        if (Input.GetKey(KeyCode.A))
        {
            // walking acceleration
            if (isGrounded)
                currentSpeed -= walkAccelerationGround;
            else
                currentSpeed -= walkAccelerationAir;
        }
        // WALK RIGHT
        else if (Input.GetKey(KeyCode.D))
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
        else if (Input.GetKeyDown(KeyCode.W) && isGrounded)
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

    // Get player input for attacking
    void UpdateAttack()
    {
        // While Fire key is held down, attempt to attack
        if (Input.GetKey(KeyCode.Space))
            AttemptAttack();

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
                if (TimeSinceLastAttack * fireRateMult > FireRateKnife)
                    Attack();
                break;
            case 1:
                if (TimeSinceLastAttack * fireRateMult > FireRateSword)
                    Attack();
                break;
            case 2:
                if (TimeSinceLastAttack * fireRateMult > FireRateSpear)
                    Attack();
                break;
            case 3:
                if (TimeSinceLastAttack * fireRateMult > FireRateChainsaw)
                    Attack();
                break;
            case 4:
                if (TimeSinceLastAttack * fireRateMult > FireRatePistol)
                    Attack();
                break;
            case 5:
                if (TimeSinceLastAttack * fireRateMult > FireRatePlasmaRifle)
                    Attack();
                break;
            case 6:
                if (TimeSinceLastAttack * fireRateMult > FireRateMinigun)
                    Attack();
                break;
            default:
                break;
        }
    }

    // Attack using the equipped weapon
    void Attack()
    {
        TimeSinceLastAttack = 0;

        // if Plasma Rifle is equipped, update ammo
        if (weapon == 5)
        {
            PlasmaRifleAmmoRemaining--;
            if (PlasmaRifleAmmoRemaining <= 0)
                EquipPreviousWeapon();
        }
    }

    // Called when the Player walks over an Item Pickup
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

    // Item 1 Collected
    void EquipSword()
    {
        Debug.Log("Player equipped Sword");
        weapon = 1;
    }

    // Item 2 Collected
    void EquipSpear()
    {
        Debug.Log("Player equipped Spear");
        weapon = 2;
    }

    // Item 3 Collected
    void EquipChainsaw()
    {
        Debug.Log("Player equipped Chainsaw");

        // only update previousWeapon if player is not already holding Chainsaw, Rifle, or Minigun
        if (!(weapon == 3 || weapon == 5 || weapon == 6))
            previousWeapon = weapon;
        weapon = 3;

        ChainsawTimeRemaining = ChainsawDuration;
    }

    // Item 4 Collected
    void EquipPistol()
    {
        Debug.Log("Player equipped Pistol");
        weapon = 4;
    }

    // Item 5 Collected
    void EquipPlasmaRifle()
    {
        Debug.Log("Player equipped Plasma Rifle");

        // only update previousWeapon if player is not already holding Chainsaw, Rifle, or Minigun
        if (!(weapon == 3 || weapon == 5 || weapon == 6)) previousWeapon = weapon;
        weapon = 5;

        PlasmaRifleAmmoRemaining = PlasmaRifleAmmoMax;
    }

    // Item 6 Collected
    void EquipMinigun()
    {
        Debug.Log("Player equipped Minigun");

        // only update previousWeapon if player is not already holding Chainsaw, Rifle, or Minigun
        if (!(weapon == 3 || weapon == 5 || weapon == 6)) previousWeapon = weapon;
        weapon = 6;

        MinigunTimeRemaining = MinigunDuration;
    }

    // Item 7 Collected
    void EquipFury()
    {
        Debug.Log("Player equipped Fury");
        FuryTimeRemaining = FuryDuration;
    }

    // Item 8 Collected
    void EquipAgility()
    {
        Debug.Log("Player equipped Agility");
        AgilityTimeRemaining = AgilityDuration;
    }

    // Item 9 Collected
    void EquipVigor()
    {
        Debug.Log("Player equipped Vigor");
        InvulnerabilityTimeRemaining = InvulnerabilityDuration;
        RegenerationTimeRemaining = RegenerationDuration;
    }

    // Knives do not spawn as items.
    // Only needed for use in EquipPreviousWeapon()
    void EquipKnife()
    {
        Debug.Log("Player equipped Knife");
        weapon = 0;
    }

    // After the player's Chainsaw, Rifle, or Minigun expires, equip the previous weapon
    void EquipPreviousWeapon()
    {
        switch (previousWeapon)
        {
            // previousWeapon should not be Chainsaw, Rifle, or Minigun (3, 5, or 6)
            case 0:
                EquipKnife();
                break;
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

    // Update Text field with weapon info
    void UpdateDebugText()
    {
        debugText.text = "Weapon: ";

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
    }
}