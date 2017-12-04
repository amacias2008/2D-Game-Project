using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public float ItemSpawnRate = 2; // item spawn chance each update
    public int MaxItemCount = 5; // items stop spawning when count reaches this
    public float MinSpawnDistance = 1; // items always spawn at least this far from both players and other items

    /*
    // players start with a knife equipped
    private float Item1Frequency = 3; // melee 1: sword
    private float Item2Frequency = 3; // melee 2: spear
    private float Item3Frequency = 1; // melee 3: chainsaw
    private float Item4Frequency = 3; // ranged 1: pistol
    private float Item5Frequency = 3; // ranged 2: plasma rifle
    private float Item6Frequency = 1; // ranged 3: minigun
    private float Item7Frequency = 2; // powerup 1: fury
    private float Item8Frequency = 2; // powerup 2: agility
    private float Item9Frequency = 2; // powerup 3: vigor
    */

    // Use this for initialization
    void Start()
    {

    }

    // FixedUpdate is called once per physics tick
    void FixedUpdate()
    {
        float random = Random.Range(0f, 100f);

        // Check if an item should be spawned
        if (random < ItemSpawnRate && GetNumberOfItems() < MaxItemCount) SpawnItem();
    }

    // Returns number of Pickups in scene
    int GetNumberOfItems()
    {
        return GameObject.FindGameObjectsWithTag("Pickup").Length;
    }

    // Adds a Pickup to the game field
    void SpawnItem()
    {
        int randomType = Random.Range(1, 10);

        // Choose spawn location using MinSpawnDistance
        Vector2 loc = ChooseSpawnLocation();

        // Create Pickup and set location
        GameObject go = (GameObject)Instantiate(Resources.Load("PickupPrefab" + randomType));
        Pickup p = go.GetComponent<Pickup>();
        go.transform.position = loc;

		// TODO: Use item spawn frequencies
		p.SetTypeID(randomType);
    }

	// Choose spawn location using MinSpawnDistance
	Vector2 ChooseSpawnLocation()
	{
		Vector2 TestLoc = new Vector2 (Random.Range(-8f, 8f), Random.Range(-1, 4));
		int attempts = 0;

		// Try to move the location up to 10 times to find sufficient open space
		while (!IsThisLocationAcceptable(TestLoc) && attempts < 10) {
            TestLoc = new Vector2 (Random.Range(-8f, 8f), Random.Range(-1, 4));
			attempts++;
		}

        //Debug.Log("Item location selection took " + attempts + " attempts.");

		return TestLoc;
	}

	// Check if this spawn location is far enough away from Players and Pickups
	bool IsThisLocationAcceptable(Vector2 loc)
	{
		bool acceptable = true;

		var players = GameObject.FindGameObjectsWithTag("Player");
		foreach (var obj in players) {
			if (Vector2.Distance (loc, obj.transform.position) < MinSpawnDistance) {
                acceptable = false;
			}
		}

		var pickups = GameObject.FindGameObjectsWithTag("Pickup");
		foreach (var obj in pickups) {
			if (Vector2.Distance (loc, obj.transform.position) < MinSpawnDistance) {
				acceptable = false;
			}
		}

		return acceptable;
	}
}