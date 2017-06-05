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
    bool startingFastTorque;
    bool startingSlowTorque;
    public float fastTorque;
    public float slowTorque;
    public float torqueDuration;
    bool startLerping;
    float elapsedTimeForThrowTorque;
    float elapsedTimeForCardFlip;
    //public float flipSpeed;
    public float flipDuration;
    bool flippingCard = false;
    Quaternion rotationAtFlipStart;
    Hand hand1;
    Hand hand2;

    //VARIABLE FOR CHECKING SWIPE
    private int messageIndex = 0;
    private bool trackingSwipe;
    private bool checkSwipe;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float swipeStartTime;
    // To recognize as swipe user should at lease swipe for this many pixels
    private const float MIN_SWIPE_DIST = .2f;
    // To recognize as a swipe the velocity of the swipe
    // should be at least mMinVelocity
    // Reduce or increase to control the swipe speed
    private const float MIN_VELOCITY = 3f;
    private readonly Vector2 xAxis = new Vector2(1, 0);
    private readonly Vector2 yAxis = new Vector2(0, 1);
    // The angle range for detecting swipe
    private const float angleRange = 30;



    // Use this for initialization
    void Start () {

        elapsedTimeForCardFlip = 0;
        rb = GetComponent<Rigidbody>();
        hand1 = GameObject.Find("Hand1").GetComponent<Hand>();
        hand2 = GameObject.Find("Hand2").GetComponent<Hand>();

	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(deckIsDestroyed);
        if (deckIsDestroyed == true && hand1.GetStandardInteractionButton() == true && hand1.GetStandardInteractionButton() == true)
        {
            rb.isKinematic = true;
            float speed = 2f;
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, hand1.transform.position, step);
            if(transform.position == hand1.transform.position)
            {
                rb.isKinematic = false;
            }
        }

        if (flippingCard == true)
        {
            Debug.Log("flippingCard!");
            elapsedTimeForCardFlip += Time.deltaTime;
            //Quaternion startRotation = transform.rotation;
            //Quaternion endRotation = startRotation + Quaternion.Euler
            //Quaternion newRotation = new Quaternion(myRotation.x + 180, myRotation.y, myRotation.z, myRotation.w);
            //Quaternion.Lerp(myRotation, newRotation, Time.deltaTime / flipSpeed);
            transform.localRotation = Quaternion.Euler(Mathf.Lerp(rotationAtFlipStart.eulerAngles.x, rotationAtFlipStart.eulerAngles.x + 180, elapsedTimeForCardFlip / flipDuration), rotationAtFlipStart.eulerAngles.y, rotationAtFlipStart.eulerAngles.z);
            if (elapsedTimeForCardFlip >= flipDuration)  flippingCard = false;
        }

        if (rb.isKinematic == false && startingFastTorque == true)
        {
            //throwingRotation = transform.eulerAngles;
            //transform.rotation = Quaternion.Euler(throwingRotation.x, 0, 0);
            //rb.AddForce(Vector3.up * 1);
            rb.AddForce(Vector3.down * 5);
            transform.Rotate(Vector3.forward * (fastTorque * throwingVelocity));
        }
        else if (rb.isKinematic == false && startingSlowTorque == true)
        {
            //throwingRotation = transform.eulerAngles;
            //transform.rotation = Quaternion.Euler(throwingRotation.x, 0, 0);
            //rb.AddForce(Vector3.up * 1);
            rb.AddForce(Vector3.down * 5);
            transform.Rotate(Vector3.forward * (slowTorque * throwingVelocity));
        }

        if (startLerping == true)
        {
            elapsedTimeForThrowTorque += Time.deltaTime;
            fastTorque = Mathf.Lerp(fastTorque, 0, elapsedTimeForThrowTorque / torqueDuration);
            slowTorque = Mathf.Lerp(slowTorque, 0, elapsedTimeForThrowTorque / torqueDuration);
            if (elapsedTimeForThrowTorque >= torqueDuration) startLerping = false;
        }

        //we want to be able to flip the card if we're holding in it our hands. the only time the card is in our hands is if it's kinematic.
        if (rb.isKinematic == true && deckIsDestroyed == false)
        {
            Vector2 touch = transform.parent.gameObject.GetComponent<Hand>().controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
            var device = transform.parent.gameObject.GetComponent<Hand>().controller;
            if (device.GetTouchDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                //Debug.Log("touching Trackpad");
                trackingSwipe = true;
                startPosition = new Vector2(touch.x, touch.y);
                swipeStartTime = Time.time;
            }
            else if (device.GetTouchUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
            {
                //Debug.Log("not touching trackpad");
                trackingSwipe = false;
                trackingSwipe = true;
                checkSwipe = true;
                //Debug.Log("Tracking Finish");
            }
            else if (trackingSwipe)
            {
                endPosition = new Vector2(touch.x, touch.y);
            }
            if (checkSwipe)
            {
                checkSwipe = false;
                float deltaTime = Time.time - swipeStartTime;
                Vector2 swipeVector = endPosition - startPosition;
                float velocity = swipeVector.magnitude / deltaTime;
                //Debug.Log("velocity is " + velocity);
                if(velocity > MIN_VELOCITY && swipeVector.magnitude > MIN_SWIPE_DIST)
                {
                    // if the swipe has enough velocity and enough distance
                    swipeVector.Normalize();
                    float angleOfSwipe = Vector2.Dot(swipeVector, xAxis);
                    angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                    // Detect left and right swipe
                    if(angleOfSwipe < angleRange)
                    {
                        OnSwipeRight();
                    }
                    else if ((180f - angleOfSwipe) < angleRange)
                    {
                        OnSwipeLeft();
                    }
                    else
                    {
                        // Detect top and bottom swipe
                        angleOfSwipe = Vector2.Dot(swipeVector, yAxis);
                        angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                        if(angleOfSwipe < angleRange)
                        {
                            OnSwipeTop();
                        }
                        else if((180f - angleOfSwipe) < angleRange)
                        {
                            OnSwipeBottom();
                        }
                        else
                        {
                            messageIndex = 0;
                        }
                    }
                }
            }
        }

    }

    void OnCollisionEnter(Collision other)
    {
        startLerping = true;
        elapsedTimeForThrowTorque = 0;
    }

    public override void OnTriggerEnterX(Collider other)
    {
        if (deckIsDestroyed == true)
        {
            if (other.gameObject.tag == "Hand")
            {
                Destroy(gameObject);
            }
        }
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
        //transform.rotation = Quaternion.Euler(270, 0, 0);
        transform.rotation = transform.parent.GetComponent<Hand>().GetAttachmentTransform("Attach_ControllerTip").transform.rotation;
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
            startingFastTorque = true;
        }
        else
        {
            startingSlowTorque = true;
        }
    }

    public void RotateCard()
    {
        flippingCard = true;
        rotationAtFlipStart = transform.localRotation;
        //Vector2 touch = transform.parent.gameObject.GetComponent<Hand>().controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        //transform.Rotate(Vector3.forward * Mathf.Clamp01(touch.y) * flipSpeed * Time.deltaTime);
        //Debug.Log("touch.y = " + touch.y);
    }

    private void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
        messageIndex = 1;
    }

    private void OnSwipeRight()
    {
        Debug.Log("Swipe right");
        messageIndex = 2;
    }

    private void OnSwipeTop()
    {
        Debug.Log("Swipe Top");
        RotateCard();
        messageIndex = 3;
    }

    private void OnSwipeBottom()
    {
        Debug.Log("Swipe Bottom");
        messageIndex = 4;
    }
}
