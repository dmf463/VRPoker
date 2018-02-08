using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

//this script pretty much holds two things
//1) the actual card value of the card
//2) all the physics of the card
public class Card : InteractionSuperClass {

    public float flying_start_time, flight_journey_distance;
    public Vector3 flying_start_position;

    public bool is_flying = false;
    public float rotSpeed;

    //this is the actual information for the card
    public CardType cardType;
    private CardRaycast cardRay;

    private Vector3 cardPosHeld;
    private Vector3 cardPosOnRelease;

    //if the card is touching the table, this is true
    bool cardHitPlayer;
    bool cardOnTable;

    public bool cardMarkedForDestruction;

    //this is the threshold by which we check whether we have a fast spin, or a slow spin
    //it COULD be proportional to the velocity of the throw
    //but people really liked the card spin, and I think it's okay to dramaticize some things for the game
    const float MAGNITUDE_THRESHOLD = 2.5f;

    //speaks for itself
    float throwingVelocity;
    Rigidbody rb;
    
    //the bools used so that they only start the torque once
    bool startingFastTorque;
    bool startingSlowTorque;

    //how much the torque should actually be
    public float fastTorque;
    public float slowTorque;

    //how long it should spin for
    public float torqueDuration;

    //the bool to start the lerp
    bool startLerping;

    //lerping variables
    float elapsedTimeForThrowTorque;

    //these are the variables for flipping the card
    //we have a time limit, a duration, and a bool
    float elapsedTimeForCardFlip;
    public float flipDuration;
    bool flippingCard = false;
    Quaternion rotationAtFlipStart;

    //this reference is literally only used in order to instantiate a new deck
    //this is not really useful, and probably doesn't belong here
    //Hand playerHand;
    
    //same with this, it turns out I have a CreateNewDeck function on the cards
    //which I GUESS makes sense, because it's possible the deck could be destroyed
    //and the cards make up the deck, but then why wouldn't I just use the deckHand and throwingHand?
    GameObject newCardDeck;

    //this is the bool that's triggered if, on detach hand, the card is facing in an awkward way
    //right now we're using rotation to do that, but that's awkward, and I think we should use a raycast
    public bool cardThrownWrong;

    //basically checked if a card is flipped or not
    public bool cardIsFlipped;

    //these two variables allow us access to the card deck script
    //possibly could use some refactoring
    //maybe make it a singleton so we can just get access to the script
    GameObject cardDeck;
    CardDeckScript deckScript;

    //so this is one of those weird card position redundancies that I think can be solved with a raycast
    //because right now we have "cardIsFlipped" if we flipped the card
    //and we have "cardFacingUp" which checks the roation of the card
    //we have this so that if we pick up a card that is facing up, it stays facing up when we pick it up
    //it used to turn down because of the way pickups work in VR
    public bool cardFacingUp = false;

    //this is one of those super awkward things that I have that I got CLOSE to right, but never quite right
    //it's supposed to keep track of how many cards I'm currently holding in hand
    //you'll see what's super awkward about it when you see the code using it
    static float cardsInHand;

    //this is checking to see if the card is both being held, and also touching the table
    //if it is, then it's being laid down and we want to detach it
    //public bool layingCardsDown;
   

    // Use this for initialization
    void Start () {

        //lalalalalala setting shit in start lalalalalalalala
        cardDeck = GameObject.FindGameObjectWithTag("CardDeck"); //DEF will need to change this for recoupling purposes.
        deckScript = cardDeck.GetComponent<CardDeckScript>();  //gonna need to rework A LOT
        cardRay = GetComponent<CardRaycast>();
        rb = GetComponent<Rigidbody>();
        elapsedTimeForCardFlip = 0;
        //playerHand = GameObject.Find("Hand1").GetComponent<Hand>();
        cardsInHand = 0;

        if (Services.Dealer.OutsideVR)
        {
            throwingHand = GameObject.Find("TestHand1").GetComponent<Hand>();
            deckHand = GameObject.Find("TestHand2").GetComponent<Hand>();
        }

    }
	

    // Update is called once per frame
    void Update () {

        //yeah, so this was my awkward attempt at beginning a state machine?
        //I wanted cards to behave in different ways depending on whether we were in dealerMode or shuffleMode?
        //so I took everything in this update, and put it in a function
        //and then yeah. that's what I did.
        //it's weird and this definitely needs refactoring
        //in fact this whole game could probably benefit from using state machines considering how many states I have
        CardForDealingMode();

}
    //so this is literally the update function
    public void CardForDealingMode()
    {
        cardFacingUp = cardRay.CardIsFaceUp(90);
        //if (flyingAllowed) StartCoroutine(WaitToStopFlying(2));
        //this is the function that flips the card in your hand
        //basically while the bool is true, we're constantly setting a new Qauternion, but the x-axis is being lerped
        //once that's done, flippingCard is no longer true
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


        if (!is_flying)
        { 
            if (rb.isKinematic == false && cardThrownWrong == true && deckScript.deckWasThrown == false)
                ThrewSingleCardBadPhysics();


            if (rb.isKinematic == false && cardThrownWrong == true && deckScript.deckWasThrown == true)
            {
                ThrewWholeDeckPhysics();
            }

            //if we're not holding the card
            //and we threw the card hard enough
            //we start the fast torque by rotating the card
            //we also want to make sure that the card moves down a bit
            //so we add some downward force
            if (rb.isKinematic == false && startingFastTorque == true)
            {
                rb.drag = 0;
                rb.AddForce(Vector3.down * 5);
                transform.Rotate(Vector3.forward * (fastTorque * throwingVelocity));
            }
            //we do the same thing here, but if we threw the card more softly
            else if (rb.isKinematic == false && startingSlowTorque == true)
            {
                rb.drag = 0;
                rb.AddForce(Vector3.down * 5);
                transform.Rotate(Vector3.forward * (slowTorque * throwingVelocity));
            }

            //basically we call this when the card hits the table
            //this stops the card from rotating by lerping it's torque amount to zero
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
            //if we're holding a card, and the deck still has cards in it
            //we want to be constantly checking whether or not a player has swiped the track pad
            if (rb.isKinematic == true && deckIsEmpty == false)
            {
                CheckSwipeDirection(throwingHand);
                CheckTouchDown();
            }
        }
    }

    //this basically just makes a new deck by destroying all the cards on the table
    //and putting a new deck in your hand
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
        newCardDeck = Instantiate(Services.PrefabDB.CardDeck, throwingHand.transform.position, Quaternion.identity) as GameObject;
        deckIsEmpty = false;
        deckHand.AttachObject(newCardDeck);
        Table.dealerState = DealerState.DealingState;
    }

    //all the physics that act on the card when you throw it bad
    //it's kind of a lot
    //but it works?
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

    //this is the physics for each card when you throw the entire deck
    //it's kind of a lot
    //but it works?
    public void ThrewWholeDeckPhysics()
    {
        startingFastTorque = false;
        startingSlowTorque = false;
        rb.drag = 0.5f;
        rb.AddForce(Random.Range(0, 2), Random.Range(0, 2), Random.Range(0, 2));
        float badThrowVelocity = deckScript.badThrowVelocity;
        Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        transform.Rotate(randomRot * badThrowVelocity * Time.deltaTime);
    }

    IEnumerator WaitToStopFlying(float time)
    {
        yield return new WaitForSeconds(time);
        //flyingAllowed = false;
    }

    //so we want to check a few things when we collide with something
    //regardless of anything, if it collides with something and is no longer flying through the air
    //we want to stop its rotation
    //then we want to check whether we should stop the physics from a bad throw
    //the card is most likely going to collide with the deck, or other cards, in the air if it was thrown wrong
    //so we want to basically say that if the thing it's hitting is not the deck, or a card, then you can turn off the physics
    //if we're in shufflingMode, then we want to do this weird thing with card in hand. let me explain
    //so the idea was that while you're touching the card, and the card is touching the table, you can drag the cards
    //but that caused some major z-fighting
    //my solution was to keep track of how many cards I'm currently touching
    //don't turn off the collision for the first 5, so that they kind of stack on top of each other
    //but then turn off the collision for ALL the rest, so we can collect the cards and hide the z-fighting
    //but this doesn't always work. if I grab cards multiple times
    void OnCollisionEnter(Collision other)
    {

        startLerping = true;
        elapsedTimeForThrowTorque = 0;
        if (cardThrownWrong == true && other.gameObject.tag != "CardDeck" && other.gameObject.tag != "PlayingCard") 
        {
            gameObject.GetComponent<ConstantForce>().enabled = false;
            cardThrownWrong = false;
        }

        if(other.gameObject.tag == "Table")
        {
            Services.Dealer.cardsTouchingTable.Add(this);
        }
    }

    //so this is how we check if the card is on the table
    void OnCollisionStay(Collision other)
    {
        if(other.gameObject.tag == "Table")
        {
            cardOnTable = true;
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Table")
        {
            cardOnTable = false;
            Services.Dealer.cardsTouchingTable.Remove(this);
        }
    }

    //so this is where we can drag the cards
    //if we're in shuffleState and the card is on the table and the hand is touching the card
    //then the transform of the card is equal to the hand
    //since this is only true with both those conditions are true, the second you take your hand away, the cards stay where they are
    //I like this
    void OnTriggerStay(Collider other)
    {
        //if (Table.dealerState == DealerState.ShufflingState)
        //{
            if (((cardOnTable == true && CardIsDead(this)) || 
                  Table.gameState == GameState.CleanUp || Table.gameState == GameState.PostHand) && 
                  other.gameObject.tag == "Hand" && !Services.Dealer.handIsOccupied)
            {
                transform.position = new Vector3 (other.transform.position.x, transform.position.y, other.transform.position.z);
            }
        //}
    }

    public bool CardIsDead(Card cardToCheck)
    {
        return Services.Dealer.deadCardsList.Contains(cardToCheck);  
    }
    //so here's where I was trying to keep track of how many cards I'm touching
    //this is so fucking awkward it hurts
    //this is also where we are checking whether the card is being laid down
    public override void OnTriggerEnterX(Collider other)
    {
       
        //if (Table.dealerState == DealerState.ShufflingState)
        //{
            if (other.gameObject.tag == "Hand" && cardOnTable == true)
            {
                cardsInHand += 1;
                //Debug.Log("cardsInHand = " + cardsInHand);
            }
        //}
        //if (other.gameObject.tag == "Board") layingCardsDown = true;
        base.OnTriggerEnterX(other);
    }

    //the mirror of the last
    public override void OnTriggerExitX(Collider other)
    {
        //if (other.gameObject.tag == "Board") layingCardsDown = false;
        base.OnTriggerExitX(other);
    }

    //part of the interaction class this derives from.
    //if it's an interactable object it NEEDS these functions on it
    //that's why I had it derive from the InteractionSuperClass
    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
    }

    //part of the interaction class this derives from.
    //if it's an interactable object it NEEDS these functions on it
    //that's why I had it derive from the InteractionSuperClass
    //also here we've added some extra code so that the card faces the correct way when you pick it up
    //you'll notice the "GetAttachmentTransform"
    //that's basically an object on the Player object that the Valve.InteractionSystems can reference
    //also, when we attach a card to the hand, we reset the slow and fast torque so that if we throw it, it still spins
    public override void OnAttachedToHand(Hand attachedHand)
    {
        //if(Table.dealerState == DealerState.DealingState)
        //{
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
        //}

        base.OnAttachedToHand(attachedHand);
    }

    //part of the interaction class this derives from.
    //if it's an interactable object it NEEDS these functions on it
    //that's why I had it derive from the InteractionSuperClass
    //also added in that if layCardsDown is equal to true we can let go of the card
    public override void HandAttachedUpdate(Hand attachedHand)
    {
        cardPosHeld = transform.position;
        if(Table.gameState == GameState.NewRound)
        {
            if (cardFacingUp == true) Table.gameState = GameState.Misdeal;
        }
        base.HandAttachedUpdate(attachedHand);
    }

    //part of the interaction class this derives from.
    //if it's an interactable object it NEEDS these functions on it
    //that's why I had it derive from the InteractionSuperClass
    //notice the giant bug alert? this could probably be fixed with a RayCast
    public override void OnDetachedFromHand(Hand hand)
    {
            if (!Services.PokerRules.cardsPulled.Contains(cardType) && Table.gameState != GameState.Misdeal)
            {
                Services.PokerRules.cardsPulled.Add(cardType);
            }
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        if (cardRay.CardIsFaceUp(90) && cardIsFlipped == false)
        {
            //Debug.Log(this.gameObject.name + " card is facing the wrong way");
            cardThrownWrong = true;
        }
		Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cards[Random.Range(0,Services.SoundManager.cards.Length)], 0.25f, Random.Range(0.95f, 1.05f), transform.position);
            StartCoroutine(CheckVelocity(.025f));
        if (Table.gameState == GameState.NewRound)
        {
            StartCoroutine(CheckIfFaceUpPreFlop(2, this));
        }
        base.OnDetachedFromHand(hand);
    }

    IEnumerator CheckIfFaceUpPreFlop(float time, Card card)
    {
        yield return new WaitForSeconds(time);
        if (card.cardFacingUp) Table.gameState = GameState.Misdeal;
    }

    //basically we want to give some time for the card to actually LEAVE the hand before we check the velocity
    //it's this that lets us know whether the card was thrown hard or soft
    IEnumerator CheckVelocity(float time)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log("rb.velocity.magnitude = " + rb.velocity.magnitude);
        throwingVelocity = rb.velocity.magnitude;
        cardPosOnRelease = transform.position;
        float yDifference = cardPosHeld.y - cardPosOnRelease.y;
        if (yDifference < -0.1f)
        {
            Debug.Log("yDifference for up, down, and then out = " + yDifference);
            cardThrownWrong = true;
        }

        if (rb.velocity.magnitude > MAGNITUDE_THRESHOLD)
        {
            startingFastTorque = true;
        }
        else
        {
            startingSlowTorque = true;
        }
    }

    //part of the interaction class this derives from.
    public override void CheckSwipeDirection(Hand hand)
    {
        base.CheckSwipeDirection(throwingHand);
    }

    public override void TouchingTrackPad()
    {
        RotateCard();
        base.TouchingTrackPad();
    }

    //the actual function called in order to trigger the card flip
    public void RotateCard()
    {
        flippingCard = true;
        rotationAtFlipStart = transform.localRotation;
    }

}
