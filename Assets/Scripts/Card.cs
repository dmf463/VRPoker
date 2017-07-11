using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class Card : InteractionSuperClass {

    public CardType cardType;
    bool cardOnTable;

    const float MAGNITUDE_THRESHOLD = 2.5f;
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
    public float flipDuration;
    bool flippingCard = false;
    Quaternion rotationAtFlipStart;
    Hand playerHand;
    GameObject newCardDeck;
    public bool cardThrownWrong;
    bool cardIsFlipped;
    GameObject cardDeck;
    CardDeckScript deckScript;
    bool cardFacingUp = false;

    //VARIABLE FOR CHECKING SWIPE
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

        cardDeck = GameObject.FindGameObjectWithTag("CardDeck"); //DEF will need to change this for recoupling purposes.
        deckScript = cardDeck.GetComponent<CardDeckScript>();  //gonna need to rework A LOT
        rb = GetComponent<Rigidbody>();
        elapsedTimeForCardFlip = 0;
        playerHand = GameObject.Find("Hand1").GetComponent<Hand>();

	}
	

    // Update is called once per frame
    void Update () {

        CardForDealingMode();

}

    public void CardForDealingMode()
    {
        if (deckIsEmpty == true && Input.GetKeyDown(KeyCode.R))
        {
            InstantiateNewDeck();
        }

        if (transform.eulerAngles.x > 89 && transform.eulerAngles.x < 92) cardFacingUp = true;
        else cardFacingUp = false;

        if (flippingCard == true)
        {
            elapsedTimeForCardFlip += Time.deltaTime;
            transform.localRotation = Quaternion.Euler(Mathf.Lerp(rotationAtFlipStart.eulerAngles.x, rotationAtFlipStart.eulerAngles.x + 180, elapsedTimeForCardFlip / flipDuration), rotationAtFlipStart.eulerAngles.y, rotationAtFlipStart.eulerAngles.z);
            if (elapsedTimeForCardFlip >= flipDuration)
            {
                elapsedTimeForCardFlip = 0;
                flippingCard = false;
                cardIsFlipped = true;
            }
        }

        if (rb.isKinematic == false && cardThrownWrong == true && deckScript.deckWasThrown == false)
        {
            ThrewSingleCardBadPhysics();
        }

        if (rb.isKinematic == false && cardThrownWrong == true && deckScript.deckWasThrown == true)
        {
            ThrewWholeDeckPhysics();
        }

        if (rb.isKinematic == false && startingFastTorque == true)
        {
            rb.drag = 0;
            rb.AddForce(Vector3.down * 5);
            transform.Rotate(Vector3.forward * (fastTorque * throwingVelocity));
        }
        else if (rb.isKinematic == false && startingSlowTorque == true)
        {
            rb.drag = 0;
            rb.AddForce(Vector3.down * 5);
            transform.Rotate(Vector3.forward * (slowTorque * throwingVelocity));
        }

        if (startLerping == true)
        {
            elapsedTimeForThrowTorque += Time.deltaTime;
            fastTorque = Mathf.Lerp(fastTorque, 0, elapsedTimeForThrowTorque / torqueDuration);
            slowTorque = Mathf.Lerp(slowTorque, 0, elapsedTimeForThrowTorque / torqueDuration);
            if (elapsedTimeForThrowTorque >= torqueDuration)
            {
                startLerping = false;
            }

        }

        if (rb.isKinematic == true && deckIsEmpty == false)
        {
            CheckSwipeDirection();
        }
    }

    public void InstantiateNewDeck()
    {
        GameObject[] oldCards = GameObject.FindGameObjectsWithTag("PlayingCard");
        foreach (GameObject deadCard in oldCards)
        {
            Destroy(deadCard);
        }
        deckScript.deckWasThrown = false;
        Destroy(GameObject.FindGameObjectWithTag("CardDeck")); //maybe could pool the card decks
        deckScript.deckWasThrown = false;
        newCardDeck = Instantiate(Services.PrefabDB.CardDeck, playerHand.transform.position, Quaternion.identity) as GameObject;
        deckIsEmpty = false;
        deckHand.AttachObject(newCardDeck);
        TableCards.dealerState = DealerState.DealingState;
    }

    public void ThrewSingleCardBadPhysics()
    {
        startingFastTorque = false;
        startingSlowTorque = false;
        rb.drag = 7;
        rb.AddForce(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
        gameObject.GetComponent<ConstantForce>().enabled = true;
        Vector3 torque;
        torque.x = Random.Range(-200, 200);
        torque.y = Random.Range(-200, 200);
        torque.z = Random.Range(-200, 200);
        gameObject.GetComponent<ConstantForce>().torque = torque;
        float badThrowVelocity = 5;
        Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        transform.Rotate(randomRot * badThrowVelocity * Time.deltaTime);
    }

    public void ThrewWholeDeckPhysics()
    {
        //Debug.Log("Calling bad throw at time: " + Time.time);
        startingFastTorque = false;
        startingSlowTorque = false;
        rb.drag = 0.5f;
        rb.AddForce(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
        float badThrowVelocity = deckScript.badThrowVelocity;
        Debug.Log("badThrowVelocity from Card is " + badThrowVelocity);
        Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        transform.Rotate(randomRot * badThrowVelocity * Time.deltaTime);
    }

    void OnCollisionEnter(Collision other)
    {
        startLerping = true;
        elapsedTimeForThrowTorque = 0;
        if (cardThrownWrong == true && other.gameObject.tag != "CardDeck") 
        {
            //Debug.Log("hitting " + other.gameObject.name);
            gameObject.GetComponent<ConstantForce>().enabled = false;
            cardThrownWrong = false;
        }
    }

    void OnCollisionStay(Collision other)
    {
        if(other.gameObject.tag == "Table")
        {
            cardOnTable = true;
        }
    }

    void OnCollisionExit(Collision other)
    {
        cardOnTable = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (TableCards.dealerState == DealerState.ShufflingState)
        {
            if (other.gameObject.tag == "Hand" && cardOnTable == true)
            {
                transform.position = new Vector3 (other.transform.position.x, transform.position.y, other.transform.position.z);
            }
        }
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
        if(TableCards.dealerState == DealerState.DealingState)
        {
            if (cardFacingUp == false)
            {
                transform.rotation = throwingHand.GetAttachmentTransform("CardFaceDown").transform.rotation;
            }
            else if (cardFacingUp == true)
            {
                transform.rotation = throwingHand.GetAttachmentTransform("CardFaceUp").transform.rotation;
            }
            //resetting the torque so they throw properly each time
            fastTorque = 3;
            slowTorque = .25f;
        }

        base.OnAttachedToHand(attachedHand);
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
        base.HandAttachedUpdate(attachedHand);
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        if(TableCards.dealerState == DealerState.DealingState)
        {
            if (rb.transform.rotation.eulerAngles.x > 290 || rb.transform.rotation.eulerAngles.x < 250 && cardIsFlipped == false)
            {
                //Debug.Log(this.gameObject.name + " card is facing the wrong way");
                cardThrownWrong = true;
            }
            StartCoroutine(CheckVelocity(.025f));
        }
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

    public void CheckSwipeDirection()
    {
        Vector2 touch = throwingHand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        var device = throwingHand.GetComponent<Hand>().controller;
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
            if (velocity > MIN_VELOCITY && swipeVector.magnitude > MIN_SWIPE_DIST)
            {
                // if the swipe has enough velocity and enough distance
                swipeVector.Normalize();
                float angleOfSwipe = Vector2.Dot(swipeVector, xAxis);
                angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                // Detect left and right swipe
                if (angleOfSwipe < angleRange)
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
                    if (angleOfSwipe < angleRange)
                    {
                        OnSwipeTop();
                    }
                    else if ((180f - angleOfSwipe) < angleRange)
                    {
                        OnSwipeBottom();
                    }
                    else
                    {
                        //messageIndex = 0;
                    }
                }
            }
        }
    }

    public void RotateCard()
    {
        flippingCard = true;
        rotationAtFlipStart = transform.localRotation;
    }

    private void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
    }

    private void OnSwipeRight()
    {
        Debug.Log("Swipe right");
    }

    private void OnSwipeTop()
    {
        Debug.Log("Swipe Top");
        RotateCard();
    }

    private void OnSwipeBottom()
    {
        Debug.Log("Swipe Bottom");
    }
}
