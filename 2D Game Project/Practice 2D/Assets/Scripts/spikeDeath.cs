using UnityEngine;
using System.Collections;

public class spikeDeath : MonoBehaviour {

	public Vector2 SpawnPoint;

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player") {
			other.transform.position = SpawnPoint;
		}
	}
}