﻿using System.Collections;
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
    public bool lerping = false;

    public bool is_flying = false;
    public float rotSpeed;
    public bool thrownWrong = false;
    int cardsCaught = 0;
    public int cardThrownNum;
    public bool cardChecked = false;
    float yPos = 0;
    public static float cardsDropped = 0;
    public float floatSpeed;
    public float floatDistance;

    public float checkTime;

    //this is the actual information for the card
    public CardType cardType;

    public bool stillTouchingCollider = false;

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

    //these two variables allow us access to the card deck script
    //possibly could use some refactoring
    //maybe make it a singleton so we can just get access to the script
    GameObject cardDeck;
    CardDeckScript deckScript;

    //basically checked if a card is flipped or not
    public bool cardFacingUp = false;
    public bool cardWasManuallyFlipped = false;


    //this is checking to see if the card is both being held, and also touching the table
    //if it is, then it's being laid down and we want to detach it
    //public bool layingCardsDown;
   

    // Use this for initialization
    void Start () {

        cardDeck = GameObject.FindGameObjectWithTag("CardDeck"); //DEF will need to change this for recoupling purposes.
        deckScript = cardDeck.GetComponent<CardDeckScript>();  //gonna need to rework A LOT
        rb = GetComponent<Rigidbody>();
        elapsedTimeForCardFlip = 0;

        if (Services.Dealer.OutsideVR)
        {
            throwingHand = GameObject.Find("TestHand1").GetComponent<Hand>();
            deckHand = GameObject.Find("TestHand2").GetComponent<Hand>();
        }

    }
    // Update is called once per frame
    void Update () {

        CardForDealingMode();
        BringCardBack();
        if(yPos != 0 && !is_flying)
        {
            transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
        }
        if(Vector3.Distance(transform.position, GameObject.Find("ShufflingArea").transform.position) > 10)
        {
            Vector3 pos = GameObject.Find("ShufflingArea").transform.position;
            transform.position = new Vector3(pos.x, pos.y, pos.z);
            rb.velocity = Vector3.zero;
        }

}
    //so this is literally the update function
    public void CardForDealingMode()
    {
        cardFacingUp = CardIsFaceUp();

        if (flippingCard == true)
        {
            elapsedTimeForCardFlip += Time.deltaTime;
            transform.localRotation = Quaternion.Euler(Mathf.Lerp(rotationAtFlipStart.eulerAngles.x, rotationAtFlipStart.eulerAngles.x + 180, elapsedTimeForCardFlip / flipDuration), rotationAtFlipStart.eulerAngles.y, rotationAtFlipStart.eulerAngles.z);
            if (elapsedTimeForCardFlip >= flipDuration)
            {
                elapsedTimeForCardFlip = 0;
                flippingCard = false;
                cardWasManuallyFlipped = true;
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
            yPos = transform.position.y;
        }
        if(other.gameObject.tag == "Floor")
        {
            if(Table.gameState != GameState.Misdeal)
            {
                Services.PokerRules.CorrectMistakes();
                cardsDropped++;
            }
            if (cardsDropped >= 3) Table.gameState = GameState.Misdeal;
        }

    }

    //so this is how we check if the card is on the table
    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Table")
        {
            cardOnTable = true;
            yPos = transform.position.y;
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Table")
        {
            cardOnTable = false;
            yPos = 0;
            Services.Dealer.cardsTouchingTable.Remove(this);
        }
    }

    public bool CardIsDead(Card cardToCheck)
    {
        return Services.Dealer.deadCardsList.Contains(cardToCheck);  
    }

    public override void OnTriggerEnterX(Collider other)
    {

        if(!Services.Dealer.killingCards && !Services.Dealer.cleaningCards && Table.gameState == GameState.NewRound)
        {
            Debug.Log("hitting " + other.gameObject.tag);
            if (other.gameObject.tag == "PlayerFace")
            {
                StartCoroutine(CheckIfCardIsAtDestination(0, cardThrownNum));
            }
        }
        base.OnTriggerEnterX(other);
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
        yPos = 0;
        if (!cardFacingUp)
        {
            transform.rotation = throwingHand.GetAttachmentTransform("CardFaceDown").transform.rotation;
        }
        else if (cardFacingUp)
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
            if (CardIsFaceUp()) Table.gameState = GameState.Misdeal;
        }
        base.HandAttachedUpdate(attachedHand);
    }

    //part of the interaction class this derives from.
    //if it's an interactable object it NEEDS these functions on it
    //that's why I had it derive from the InteractionSuperClass
    //notice the giant bug alert? this could probably be fixed with a RayCast
    public override void OnDetachedFromHand(Hand hand)
    {
        if (!Services.PokerRules.thrownCards.Contains(gameObject) && Table.gameState == GameState.NewRound)
        {
            Services.PokerRules.thrownCards.Add(gameObject);
        }
        if (!Services.PokerRules.cardsPulled.Contains(cardType) && Table.gameState != GameState.Misdeal)
        {
            Services.PokerRules.cardsPulled.Add(cardType);
            cardThrownNum = Services.PokerRules.cardsPulled.Count;
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (CardIsFaceUp() && !cardWasManuallyFlipped)
        {
            //Debug.Log(this.gameObject.name + " card is facing the wrong way");
            cardThrownWrong = true;
        }
        Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cards[Random.Range(0, Services.SoundManager.cards.Length)], 0.25f, Random.Range(0.95f, 1.05f), transform.position);
        StartCoroutine(CheckVelocity(.025f));
        //if (Table.gameState == GameState.NewRound)
        //{
        //    StartCoroutine(CheckIfFaceUpPreFlop(2));
        //}
        base.OnDetachedFromHand(hand);
    }

    //IEnumerator CheckIfFaceUpPreFlop(float time)
    //{
    //    yield return new WaitForSeconds(time);
    //    if (CardIsFaceUp(150, )) Table.gameState = GameState.Misdeal;
    //}

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
        if (!Services.Dealer.killingCards && !Services.Dealer.cleaningCards && Table.gameState == GameState.NewRound)
        {
            StartCoroutine(CheckIfCardIsAtDestination(checkTime, cardThrownNum));
        }
    }

    
    public bool CardsAreFlying(GameObject card)
    {
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        for (int i = 0; i < cardsOnTable.Length; i++)
        {
            if (cardsOnTable[i].GetComponent<Card>().is_flying)
            {
                if(card != cardsOnTable[i])
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void InitializeLerp(Vector3 dest)
    {
        flying_start_time = Time.time;
        flight_journey_distance = Vector3.Distance(transform.position, dest);
        flying_start_position = transform.position;
        lerping = true;
    }

    public IEnumerator LerpCardPos(Vector3 dest, float speed)
    {
        while(lerping)
        {
            float distCovered = (Time.time - flying_start_time) * speed;
            float fracJourney = distCovered / flight_journey_distance;
            transform.position = Vector3.Lerp(transform.position, dest, fracJourney);
            yield return null;
        }
    }

    public IEnumerator LerpCardRot(Quaternion dest, float speed)
    {
        while (lerping)
        {
            float distCovered = (Time.time - flying_start_time) * speed;
            float fracJourney = distCovered / flight_journey_distance;
            transform.rotation = Quaternion.Lerp(transform.rotation, dest, fracJourney);
            yield return null;
        }
    }

    public IEnumerator CheckIfCardIsAtDestination(float time, int cardsPulled)
    {
        yield return new WaitForSeconds(time);
        //if (!cardChecked)
        //{
        //    cardChecked = true;
        Debug.Log("thrownCardsList = " + Services.PokerRules.thrownCards.Count);
        float badCardsDebug = 0;
        for (int i = Services.PokerRules.thrownCards.Count - 1; i >= 0; i--)
        {
            if (Services.PokerRules.thrownCards[i].GetComponent<Card>().thrownWrong)
            {
                badCardsDebug++;
            }
        }
        Debug.Log("badcards count = " + badCardsDebug);
        if (Services.PokerRules.CardIsInCorrectLocation(this, cardsPulled) && !CardIsFaceUp())
        {
            float badCards = 0;
            //Services.Dealer.messageText.text = "Card is in correct location";
            Debug.Log("Card is in correct location");
            for (int i = Services.PokerRules.thrownCards.Count - 1; i >= 0; i--)
            {
                if (Services.PokerRules.thrownCards[i].GetComponent<Card>().thrownWrong ||
                    Services.PokerRules.thrownCards[i].GetComponent<Card>().is_flying ||
                    !Services.PokerRules.CardIsInCorrectLocation(Services.PokerRules.thrownCards[i].GetComponent<Card>(), Services.PokerRules.cardsPulled.Count))
                {
                    badCards++;
                }
            }
            if (badCards == 0 && !CardsAreFlying(gameObject))
            {
                Services.PokerRules.thrownCards.Clear();
            }
            else
            {
                thrownWrong = true;
                Table.instance.RemoveCardFrom(this);
                Services.PokerRules.cardsLogged.Remove(this);
                Services.PokerRules.cardsPulled.Remove(cardType);
                Debug.Log("Removing " + cardType.rank + " of " + cardType.suit);
                cardDeck.GetComponent<CardDeckScript>().cardsInDeck.Add(cardType);
                flying_start_time = Time.time;
                flight_journey_distance = Vector3.Distance(transform.position, cardDeck.transform.position);
                flying_start_position = transform.position;
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], .05f);
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], .05f);
            }
        }
        else
        {
            Debug.Log("card is in wrong location");
            thrownWrong = true;
            Table.instance.RemoveCardFrom(this);
            Services.PokerRules.cardsLogged.Remove(this);
            Services.PokerRules.cardsPulled.Remove(cardType);
            Debug.Log("Removing " + cardType.rank + " of " + cardType.suit);
            cardDeck.GetComponent<CardDeckScript>().cardsInDeck.Add(cardType);
            flying_start_time = Time.time;
            flight_journey_distance = Vector3.Distance(transform.position, cardDeck.transform.position);
            flying_start_position = transform.position;
            Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], .05f);
            Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], .05f);
        }
    }
    //}

    public void BringCardBack()
    {
        if (thrownWrong && !Services.Dealer.killingCards && !Services.Dealer.cleaningCards)
        {
            Debug.Log("cardsPulled = " + Services.PokerRules.cardsPulled.Count);
            if (Services.PokerRules.thrownCards.Contains(gameObject))
            {
                is_flying = true;
                float distCovered = (Time.time - flying_start_time) * (Services.Dealer.cardMoveSpeed * 5);
                float fracJourney = distCovered / flight_journey_distance;
                transform.position = Vector3.Lerp(GetComponent<Card>().flying_start_position, cardDeck.transform.position, fracJourney);
                if (transform.position == cardDeck.transform.position)
                {
                    is_flying = false;
                    thrownWrong = false;
                    Services.PokerRules.thrownCards.Remove(gameObject);
                    foreach(GameObject card in Services.PokerRules.thrownCards)
                    {
                        StartCoroutine(card.GetComponent<Card>().CheckIfCardIsAtDestination(0, Services.PokerRules.cardsPulled.Count));
                    }
                    cardDeck.GetComponent<CardDeckScript>().MakeDeckLarger();
                    Destroy(gameObject);
                }
            }
            if (Services.PokerRules.thrownCards.Count == 0)
            {
                Services.PokerRules.TurnOffAllIndicators();
                Services.PokerRules.IndicateCardPlacement(Services.PokerRules.cardsPulled.Count);
            }
        }
    }

    public Vector3 GetPlayerPos()
    {
        Vector3 pos;
        int playerIndex = Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(Services.PokerRules.cardsPulled.Count);
        PokerPlayerRedux player = Services.Dealer.players[playerIndex];
        GameObject playerCardSpace = player.gameObject.GetComponentInChildren<LogCards>().gameObject;
        Debug.Log("playerCardSpace name = " + playerCardSpace.name);
        pos = playerCardSpace.transform.position;
        return pos;
    }

    public void FloatInPlace(float speed, float distance)
    {
        Debug.Log("FLOATING");
        Vector3 playerPos = GetPlayerPos() +  new Vector3(0, .25f, 0);
        transform.position = new Vector3(playerPos.x, playerPos.y + Mathf.PingPong(Time.time / speed, distance), playerPos.z);
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
        if (!flippingCard)
        {
            flippingCard = true;
            rotationAtFlipStart = transform.localRotation;
        }
    }

    public bool CardIsFaceUp()
    {
        //float angle = GetCardAngle(comparisonPoint);
        //if (angle > angleThreshold || cardIsFlipped) return true;
        //else return false;
        //Debug.DrawRay(transform.position, transform.forward, Color.green);
        //Debug.DrawRay(transform.position, Vector3.down, Color.red);
        //Debug.Log(Vector3.Dot(transform.forward, Vector3.down));
        //Debug.Log(this.name + " is face up: " + (Vector3.Dot(transform.forward, Vector3.down) > 0));
        return (Vector3.Dot(transform.forward, Vector3.down) > 0);
    }

    public float GetCardAngle(string comparisonPoint)
    {
        Transform obj = GameObject.Find(comparisonPoint).transform;
        Vector3 targetDir = obj.position - transform.position;
        return Vector3.Angle(targetDir, -transform.forward);
    }

}
