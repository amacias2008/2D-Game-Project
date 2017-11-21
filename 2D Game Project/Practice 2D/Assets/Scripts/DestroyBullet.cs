using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBullet : MonoBehaviour 
{
	public float delay;

	// Use this for initialization
	void Start () 
	{
		Destroy (gameObject, delay);	
	}

}
