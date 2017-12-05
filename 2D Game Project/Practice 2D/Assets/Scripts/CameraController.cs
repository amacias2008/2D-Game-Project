using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraController : NetworkBehaviour {

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

		if (!isLocalPlayer)
			return;

        float x, y;
        x = (GameObject.FindGameObjectsWithTag("Local")[0].transform.position.x + GameObject.FindGameObjectsWithTag("Local")[1].transform.position.x) / 2f;
        y = (GameObject.FindGameObjectsWithTag("Local")[0].transform.position.y + GameObject.FindGameObjectsWithTag("Local")[1].transform.position.y) / 2f;
        transform.position = new Vector3(x, y, -1);

        float dist;
        dist = (GameObject.FindGameObjectsWithTag("Local")[0].transform.position - GameObject.FindGameObjectsWithTag("Local")[1].transform.position).magnitude;
        
        //cam.orthographicSize = MinCameraSize + (dist / 4);
    }
}
