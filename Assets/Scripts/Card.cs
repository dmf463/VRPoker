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
    static float cardsInHand;
   

    // Use this for initialization
    void Start () {

        cardDeck = GameObject.FindGameObjectWithTag("CardDeck"); //DEF will need to change this for recoupling purposes.
        deckScript = cardDeck.GetComponent<CardDeckScript>();  //gonna need to rework A LOT
        rb = GetComponent<Rigidbody>();
        elapsedTimeForCardFlip = 0;
        playerHand = GameObject.Find("Hand1").GetComponent<Hand>();
        cardsInHand = 0;

	}
	

    // Update is called once per frame
    void Update () {

        CardForDealingMode();

}

    public void CardForDealingMode()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            InstantiateNewDeck();
        }

        if(throwingHand.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip) || deckHand.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_Grip))
        {
            TableCards.dealerState = DealerState.ShufflingState;
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
        if(GameObject.FindGameObjectWithTag("CardDeck") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("CardDeck")); //maybe could pool the card decks
        }
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
        //Debug.Log("badThrowVelocity from Card is " + badThrowVelocity);
        Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        transform.Rotate(randomRot * badThrowVelocity * Time.deltaTime);
    }

    void OnCollisionEnter(Collision other)
    {
        startLerping = true;
        elapsedTimeForThrowTorque = 0;
        if (cardThrownWrong == true && other.gameObject.tag != "CardDeck" && other.gameObject.tag != "PlayingCard") 
        {
            //Debug.Log("hitting " + other.gameObject.name);
            gameObject.GetComponent<ConstantForce>().enabled = false;
            cardThrownWrong = false;
        }
        if(TableCards.dealerState == DealerState.ShufflingState)
        {
            if (other.gameObject.tag == "PlayingCard")
            {
                if (cardsInHand > 5)
                {
                    //Debug.Log("Turning of some physics");
                    Physics.IgnoreCollision(GetComponent<Collider>(), other.gameObject.GetComponent<Collider>(), true);
                }
            }
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
        if (TableCards.dealerState == DealerState.ShufflingState)
        {
            if (other.gameObject.tag == "Hand" && cardOnTable == true)
            {
                cardsInHand += 1;
                //Debug.Log("cardsInHand = " + cardsInHand);
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

    public override void CheckSwipeDirection()
    {
        base.CheckSwipeDirection();
    }

    public override void OnSwipeTop()
    {
        RotateCard();
        base.OnSwipeTop();
    }

    public void RotateCard()
    {
        flippingCard = true;
        rotationAtFlipStart = transform.localRotation;
    }

}
