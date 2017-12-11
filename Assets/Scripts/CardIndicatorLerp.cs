using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardIndicatorLerp : MonoBehaviour {

    public float lerpDistance;
    public float lerpSpeed;
    private float yPos;
	// Use this for initialization
	void Start () {
        yPos = transform.position.y;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(transform.position.x, yPos + Mathf.PingPong(Time.time / lerpSpeed, lerpDistance), transform.position.z); 
		
	}
}
