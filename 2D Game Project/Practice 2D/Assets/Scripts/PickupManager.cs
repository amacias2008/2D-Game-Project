using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public int ItemSpawnRate = 10; // 0 to 1000, item spawn rate
    public int MaxItemCount = 5; // items stop spawning when count reaches this
    public int MinSpawnDistance = 100; // items always spawn at least this far from both players

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
    private float Item9Frequency = 2; // powerup 3: shield
    */

    // Use this for initialization
    void Start()
    {

    }

    // FixedUpdate is called once per physics tick
    void FixedUpdate()
    {
        float random = Random.Range(0, 1000);

        // Check if an item should be spawned
        if (random < ItemSpawnRate && GetNumberOfItems() < MaxItemCount) SpawnItem();
    }

    // Returns number of ItemPickups in scene
    int GetNumberOfItems()
    {
        return GameObject.FindGameObjectsWithTag("Pickup").Length;
    }

    // Add an ItemPickup to the game field
    void SpawnItem()
    {
        GameObject go = (GameObject)Instantiate(Resources.Load("PickupPrefab"));
        go.transform.position = new Vector2(Random.Range(-4f, 4f), Random.Range(0f, 1f));

        // Check spawn location using MinSpawnDistance
        // Set random typeID of item using spawn frequencies
    }
}