using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class PlayingCardScript : InteractionSuperClass {

    Vector3 throwingRotation;
    Quaternion myRotation;
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
        myRotation = transform.rotation;
        //Debug.Log("torque is " + torque);
        //throwingRotation = transform.eulerAngles;
        //transform.rotation = Quaternion.Euler(0, throwingRotation.y, throwingRotation.z);
        if(rb.isKinematic == false)
        {
            throwingRotation = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0, throwingRotation.y, 0);
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

    public override void OnTriggerEnterX(Collider other)
    {
        base.OnTriggerEnterX(other);
    }

    public override void OnTriggerExitX(Collider other)
    {
        base.OnTriggerExitX(other);
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        transform.rotation = Quaternion.Euler(0, 90, 0);
        base.OnAttachedToHand(attachedHand);
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
        base.HandAttachedUpdate(attachedHand);
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        base.OnDetachedFromHand(hand);
    }
}
