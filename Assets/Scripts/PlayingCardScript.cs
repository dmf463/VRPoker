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
    GameObject newCardDeck;
    Vector3 newCardDeckScale;
    Vector3 currentCardDeckScale;
    Vector3 increaseCardDeckBy;
    bool badThrow;
    float badThrowSpeed;
    ConstantForce constForce;

    //VARIABLES TO CHECK ANGULAR_VELOCITY SO I CAN ADD RANDOM TORQUES IF SOMEONE THROWS LIKE AN IDIOT
    //holds the previous frames rotation
    //Quaternion lastRotation;
    ////references to the relevant axis angle variables
    //float magnitude;
    //Vector3 axis;
    //public Vector3 angularVelocity;
    //{
    //    get
    //    {
    //        //DIVIDED by Time.deltaTime to give you the degrees of rotation per axis per second
    //        return (axis * magnitude) / Time.deltaTime;
    //    }
    //}
    Vector3 angularVelocity;

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
        badThrowSpeed = 0;
        rb = GetComponent<Rigidbody>();
        constForce = GetComponent<ConstantForce>();
        hand1 = GameObject.Find("Hand1").GetComponent<Hand>();
        hand2 = GameObject.Find("Hand2").GetComponent<Hand>();
        //lastRotation = transform.rotation;

	}
	
    //void FixedUpdate()
    //{
    //    //doing the math for rotation
    //    var deltaRot = transform.rotation * Quaternion.Inverse(lastRotation);
    //    var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
    //    angularVelocity = eulerRot / Time.deltaTime;
    //    Debug.Log(angularVelocity);
    //}

    // Update is called once per frame
    void Update () {
        //Debug.Log(rb.transform.rotation);
        //Debug.Log(deckIsDestroyed);
        if (deckIsDestroyed == true && hand1.GetStandardInteractionButton() == true && hand1.GetStandardInteractionButton() == true)
        {
            instantiatingDeck = true;
            //rb.isKinematic = true;
            //float speed = 1f;
            //float step = speed * Time.deltaTime;
            //transform.position = Vector3.MoveTowards(transform.position, hand1.transform.position, step);
            //if(transform.position == hand1.transform.position)
            //{
            //    rb.isKinematic = false;
            //}
        }

        if (instantiatingDeck == true)
        {
            GameObject[] oldCards = GameObject.FindGameObjectsWithTag("PlayingCard");
            foreach (GameObject deadCard in oldCards)
            {
                Destroy(deadCard);
            }
            Destroy(GameObject.Find("PlayingCardDeck"));
            newCardDeck = Instantiate(Resources.Load("Prefabs/PlayingCardDeck"), hand1.transform.position, Quaternion.identity) as GameObject;
            //newCardDeckScale = newCardDeck.transform.localScale;
            //increaseCardDeckBy = new Vector3(newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
            //newCardDeck.transform.localScale = new Vector3(newCardDeck.transform.localScale.x, newCardDeck.transform.localScale.y / 52, newCardDeck.transform.localScale.z);
            //currentCardDeckScale = newCardDeck.transform.localScale;

            instantiatingDeck = false;
            deckIsDestroyed = false;
            hand1.AttachObject(newCardDeck);

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

        if(rb.isKinematic == false && badThrow == true)
        {
            badThrowSpeed += Time.deltaTime;
            constForce.enabled = true;
            rb.drag = 7;
            Vector3 torque;
            torque.x = Random.Range(-200, 200);
            torque.y = Random.Range(-200, 200);
            torque.z = Random.Range(-200, 200);
            constForce.torque = torque;
            transform.rotation = Random.rotation;
            
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
        if(badThrow == true)
        {
            constForce.enabled = false;
            badThrow = false;
        }
    }

    public override void OnTriggerEnterX(Collider other)
    {
        //if (deckIsDestroyed == true)
        //{
        //    if (other.gameObject.tag == "Hand")
        //    {
        //        Destroy(gameObject);
        //        instantiatingDeck = true;
        //        deckIsDestroyed = false;
        //        if (deckExists == true)
        //        {
        //            //currentCardDeckScale.y = currentCardDeckScale.y + increaseCardDeckBy.y;
        //            //Debug.Log("currentCardDeckScale = " + currentCardDeckScale);
        //            //Debug.Log("newCardDeck.transform.localScale = " + newCardDeck.transform.localScale);
        //            //newCardDeck.transform.localScale = currentCardDeckScale;
        //        }
        //    }
        //}
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
        //Debug.Log("angularVelocity for the card is " + rb.angularVelocity);
        angularVelocity = attachedHand.GetTrackedObjectVelocity();
        base.HandAttachedUpdate(attachedHand);
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        if (rb.transform.rotation.y > 0 || rb.transform.rotation.w > 0.4f || rb.transform.rotation.w < -0.4f)
        {
            Debug.Log(this.gameObject.name + " card is facing the wrong way");
            badThrow = true;

        }
        //Debug.Log("playingCardRotation = " + transform.localRotation);
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
