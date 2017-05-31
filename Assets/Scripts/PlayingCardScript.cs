using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class PlayingCardScript : InteractionSuperClass {

    const float MAGNITUDE_THRESHOLD = 2.5f;
    Vector3 throwingRotation;
    float throwingVelocity;
    Rigidbody rb;
    bool startingTorque;
    public float fastTorque;
    public float slowTorque;
    public float duration;
    bool startLerping;
    float elapsedTime;



    // Use this for initialization
    void Start () {

        rb = GetComponent<Rigidbody>();

	}
	
	// Update is called once per frame
	void Update () {
        if(rb.isKinematic == false && startingTorque == true)
        {
            throwingRotation = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0, throwingRotation.y, 0);
            transform.Rotate(Vector3.up * (fastTorque * throwingVelocity));
        }
        else if (rb.isKinematic == false && startingTorque == false)
        {
            throwingRotation = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0, throwingRotation.y, 0);
            transform.Rotate(Vector3.up * (slowTorque * throwingVelocity));
        }

        if(startLerping == true)
        {
            elapsedTime += Time.deltaTime;
            fastTorque = Mathf.Lerp(fastTorque, 0, elapsedTime / duration);
            slowTorque = Mathf.Lerp(slowTorque, 0, elapsedTime / duration);
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
        StartCoroutine(CheckVelocity(.025f));
        base.OnDetachedFromHand(hand);
    }

    IEnumerator CheckVelocity(float time)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log("rb.velocity.magnitude = " + rb.velocity.magnitude);
        throwingVelocity = rb.velocity.magnitude;
        if (rb.velocity.magnitude > MAGNITUDE_THRESHOLD)
        {
            startingTorque = true;
        }
    }
}
