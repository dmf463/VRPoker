using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayingCardScript : MonoBehaviour {

    Vector3 myRotation;
    public float torque;
    Rigidbody rb;


	// Use this for initialization
	void Start () {

        rb = GetComponent<Rigidbody>();

	}
	
	// Update is called once per frame
	void Update () {

        myRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0, myRotation.y, myRotation.z);

    }
}
