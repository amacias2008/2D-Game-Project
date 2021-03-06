﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public bool dynamicCamEnabled = true;

    private float MinCameraSize = 2;
    private float MaxCameraSize = 6;

    private Camera cam;

    // Use this for initialization
    void Start () {
        cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {

        if (!dynamicCamEnabled) return;

        float x, y;
        x = (GameObject.FindGameObjectsWithTag("Player")[0].transform.position.x + GameObject.FindGameObjectsWithTag("Player")[1].transform.position.x) / 2f;
        y = (GameObject.FindGameObjectsWithTag("Player")[0].transform.position.y + GameObject.FindGameObjectsWithTag("Player")[1].transform.position.y) / 2f;
        transform.position = new Vector3(x, y, -1);

        float dist;
        dist = (GameObject.FindGameObjectsWithTag("Player")[0].transform.position - GameObject.FindGameObjectsWithTag("Player")[1].transform.position).magnitude;
        
        //cam.orthographicSize = MinCameraSize + (dist / 4);
    }
}
