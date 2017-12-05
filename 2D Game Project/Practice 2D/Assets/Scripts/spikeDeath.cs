using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class spikeDeath : NetworkBehaviour {

	public Vector2 SpawnPoint;

	void Awake ()
	{
		
	}

	/**********************************************************
	 * Temporarily commented this out so that the spike
	 * does not destroy the game player and we can implement
	 * the health bar.
	 **********************************************************/
	/*void OnTriggerEnter2D(Collider2D other)
	{
		if (other.tag == "Player") {
			other.transform.position = SpawnPoint;
		}
	}*/
}