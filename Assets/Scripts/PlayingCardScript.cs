using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //float turn = Input.GetAxis("Vertical");
        //rb.AddTorque(transform.up * torque);
        if(rb.isKinematic == false)
        {
            transform.Rotate(Vector3.up * torque);
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
