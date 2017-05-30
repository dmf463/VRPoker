using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class PlayingCardScript : MonoBehaviour {

    Vector3 myRotation;
    Rigidbody rb;
    public float torque;
    public float duration;
    bool startLerping;
    float elapsedTime;


    // Use this for initialization
    void Start () {

        rb = GetComponent<Rigidbody>();

	}
	
	// Update is called once per frame
	void Update () {

        Debug.Log("torque is " + torque);
        myRotation = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0, myRotation.y, myRotation.z);
        if(rb.isKinematic == false)
        {
            transform.Rotate(Vector3.up * (torque * rb.velocity.magnitude));
        }

        if(startLerping == true)
        {
            elapsedTime += Time.deltaTime;
            torque = Mathf.Lerp(torque, 0, elapsedTime / duration);
            if (elapsedTime >= duration) startLerping = false;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        startLerping = true;
        elapsedTime = 0;
    }
}
