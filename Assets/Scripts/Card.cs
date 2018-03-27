using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;
using SpriteGlow;

//this script pretty much holds two things
//1) the actual card value of the card
//2) all the physics of the card
public class Card : InteractionSuperClass {

    public Shader dissolve;
    public Shader normal;

    private float testingDot;
    [HideInInspector]
    public float flying_start_time, flight_journey_distance, flight_journey_angle;
    [HideInInspector]
    public Vector3 flying_start_position;
    [HideInInspector]
    public Quaternion flying_start_rotation;
    [HideInInspector]
    public bool lerping = false;
    [HideInInspector]
    public bool readyToFloat = false;
    [HideInInspector]
    public bool rotateOnAdd = false;
    [HideInInspector]
    public bool firstTime = false;

    public bool burningCard = false;

    public int randomTexture;
    private Texture2D myTexture;
   

    private Renderer myRenderer;

    private TaskManager tm;

    public float glowSpeed;
    public float maxGlow;
    [HideInInspector]
    public bool callingPulse;
    float pingPongCount = 0;
    float startTime;
    float emission;
    [HideInInspector]
    public bool foldedCards = false;
    [HideInInspector]
    public bool changedHeight = false;

    [HideInInspector]
    public float radius;
    [HideInInspector]
    public float constRadius;
    private float theta;
    [HideInInspector]
    public float height;
    Vector3 centerPoint;
    public float noiseMagnitude;
    public float noiseSpeed;
    public float rotationSpeed;
    public float cardMoveSpeed;
    public bool lerpedRadius;

    [HideInInspector]
    public bool is_flying = false;
    [HideInInspector]
    public float rotSpeed;
    [HideInInspector]
    public bool thrownWrong = false;
    int cardsCaught = 0;
    [HideInInspector]
    public int cardThrownNum;
    [HideInInspector]
    public bool cardChecked = false;
    float yPos = 0;
    public static float cardsDropped = 0;
    public float floatSpeed;
    public float floatDistance;

    public float checkTime;

    //this is the actual information for the card
    [HideInInspector]
    public CardType cardType;

    [HideInInspector]
    public bool stillTouchingCollider = false;

    private Vector3 cardPosHeld;
    private Vector3 cardPosOnRelease;
    bool cardHitPlayer;
    bool cardOnTable;
    [HideInInspector]
    public bool cardMarkedForDestruction;

    const float MAGNITUDE_THRESHOLD = 2.5f;

    float throwingVelocity;
    Rigidbody rb;

    //the bools used so that they only start the torque once
    bool startingFastTorque;
    bool startingSlowTorque;
    public float fastTorque;
    public float slowTorque;
    public float torqueDuration;

    //lerping variables    
    bool startLerping;
    float elapsedTimeForThrowTorque;
    float elapsedTimeForCardFlip;
    public float flipDuration;
    bool flippingCard = false;
    Quaternion rotationAtFlipStart;

    GameObject newCardDeck;
    [HideInInspector]
    public bool cardThrownWrong;
    GameObject cardDeck;
    CardDeckScript deckScript;

    //basically checked if a card is flipped or not
    [HideInInspector]
    public bool cardFacingUp = false;
    [HideInInspector]
    public bool cardWasManuallyFlipped = false;

    [HideInInspector]
    public bool isFloating = false;
    bool straighteningCards = false;
    [HideInInspector]
    bool checkingIfCheatedCard = false;
    public bool cheatCard = false;


    // Use this for initialization
    void Start() {

        tm = new TaskManager();
        noiseMagnitude += Random.Range(-0.075f, 0.02f);
        rotationSpeed += Random.Range(0, .2f);
        centerPoint = GameObject.Find("BurnCards").transform.position;
        cardDeck = GameObject.FindGameObjectWithTag("CardDeck"); //DEF will need to change this for recoupling purposes.
        deckScript = cardDeck.GetComponent<CardDeckScript>();  //gonna need to rework A LOT
        rb = GetComponent<Rigidbody>();
        elapsedTimeForCardFlip = 0;
        floatDistance += Random.Range(0.001f, 0.005f);
        floatSpeed += Random.Range(1, 50);


        myRenderer = GetComponent<Renderer>();
        randomTexture = Random.Range(1, 12);
        myTexture = (Texture2D)Resources.Load("Textures/noise" + randomTexture);
        myRenderer.material.SetTexture("_DissolveTex", myTexture);

        if (Services.Dealer.OutsideVR)
        {
            throwingHand = GameObject.Find("TestHand1").GetComponent<Hand>();
            deckHand = GameObject.Find("TestHand2").GetComponent<Hand>();
        }

    }
    // Update is called once per frame
    void Update() {

        tm.Update();
        myRenderer.material.SetColor("_EmissionColor", Color.black);
        if (Table.gameState == GameState.Misdeal) StopCheating();
        if (foldedCards)
        {
            transform.position = RotateWithPerlinNoise(rotationSpeed);
            if(!callingPulse && Table.gameState < GameState.ShowDown) StartPulse();
            maxGlow = 1;
            glowSpeed = .5f;
        }
        PulseGlow();
        CardForDealingMode();
        BringCardBack();
        FloatInPlace(floatSpeed, floatDistance);
        if (Vector3.Distance(transform.position, GameObject.Find("ShufflingArea").transform.position) > 20)
        {
            Vector3 pos = GameObject.Find("ShufflingArea").transform.position;
            transform.position = new Vector3(pos.x, pos.y, pos.z);
            rb.velocity = Vector3.zero;
        }

    }

    public Vector3 RotateWithPerlinNoise(float speed)
    {
        theta += speed * Time.deltaTime;
        Vector3 center = new Vector3(centerPoint.x, 0, centerPoint.z);
        Services.PokerRules.ChangeHeight();
        Vector3 pos = new Vector3(radius * Mathf.Sin(theta), height, radius * Mathf.Cos(theta)) + center;
        Vector3 perlinOffset = noiseMagnitude * new Vector3(
            Mathf.PerlinNoise(0, noiseSpeed * Time.time), 
            Mathf.PerlinNoise(10000,noiseSpeed * Time.time), 
            Mathf.PerlinNoise(20000, noiseSpeed*Time.time));
        pos += perlinOffset;
        return pos;
    }

    public Vector3 ManualRotation(float speed, Vector3 pos)
    {
        theta += speed * Time.deltaTime;
        Vector3 center = new Vector3(pos.x, 0, pos.z);
        return new Vector3(radius * Mathf.Sin(theta), transform.position.y, radius * Mathf.Cos(theta)) + center;
    }
    //so this is literally the update function
    public void CardForDealingMode()
    {
        if (!Services.Dealer.isCheating)
        {
            cardFacingUp = CardIsFaceUp();
        }

        if (flippingCard == true)
        {
            elapsedTimeForCardFlip += Time.deltaTime;
            transform.localRotation = Quaternion.Euler(Mathf.Lerp(rotationAtFlipStart.eulerAngles.x, rotationAtFlipStart.eulerAngles.x + 180, elapsedTimeForCardFlip / flipDuration), rotationAtFlipStart.eulerAngles.y, rotationAtFlipStart.eulerAngles.z);
            if (elapsedTimeForCardFlip >= flipDuration)
            {
                elapsedTimeForCardFlip = 0;
                flippingCard = false;
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
                if (elapsedTimeForThrowTorque >= torqueDuration)
                {
                    startLerping = false;
                    if (Table.gameState == GameState.NewRound && !firstTime)
                    {
                        firstTime = true;
                        InitializeLerpForTorqueFlair(GetCardRot());
                        StartCoroutine(LerpCardRotOnAdd(GetCardRot(), 1));
                        StartCoroutine(StopRotating(GetCardRot()));
                    }
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

    public void InstantiateNewDeck()
    {
        GameObject[] oldCards = GameObject.FindGameObjectsWithTag("PlayingCard");
        foreach (GameObject deadCard in oldCards)
        {
            Destroy(deadCard);
        }
        deckScript.deckWasThrown = false;
        if (GameObject.FindGameObjectWithTag("CardDeck") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("CardDeck")); //maybe could pool the card decks
        }
        deckScript.deckWasThrown = false;
        newCardDeck = Instantiate(Services.PrefabDB.CardDeck, throwingHand.transform.position, Quaternion.identity) as GameObject;
        deckIsEmpty = false;
        deckHand.AttachObject(newCardDeck);
        Table.dealerState = DealerState.DealingState;
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
        if (!Services.Dealer.OutsideVR && !startLerping)
        {
            //startLerping = true;
            //elapsedTimeForThrowTorque = 0;
        }
        if (cardThrownWrong == true && other.gameObject.tag != "CardDeck" && other.gameObject.tag != "PlayingCard")
        {
            gameObject.GetComponent<ConstantForce>().enabled = false;
            cardThrownWrong = false;
        }

        if (other.gameObject.tag == "Table")
        {
            Services.Dealer.cardsTouchingTable.Add(this);
        }
        if (other.gameObject.tag == "Floor" && !Services.Dealer.isCheating)
        {
            if (Table.gameState != GameState.Misdeal)
            {
                Debug.Log("misdeal here");
                Services.PokerRules.CorrectMistakes();
                cardsDropped++;
            }
            if (cardsDropped >= 5)
            {
                Debug.Log("misdeal here");
                Table.gameState = GameState.Misdeal;
            }
        }

    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Table")
        {
            cardOnTable = true;
            if (!checkingIfCheatedCard)
            {
                checkingIfCheatedCard = true;
                if (cheatCard)
                {
                    StartCoroutine(WaitToCheckIfCardInList(1f));
                }
            }
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

    public bool CardIsDead(Card cardToCheck)
    {
        return Services.Dealer.deadCardsList.Contains(cardToCheck);
    }

    public void StopCheating()
    {
        Services.Dealer.lighting.gameObject.SetActive(true);
        Services.Dealer.isCheating = false;
    }

    public override void OnTriggerEnterX(Collider other)
    {

        if (!Services.Dealer.killingCards && !Services.Dealer.cleaningCards && Table.gameState == GameState.NewRound)
        {
            //Debug.Log("hitting " + other.gameObject.tag);
            if (other.gameObject.tag == "PlayerFace")
            {
                StartCoroutine(CheckIfCardIsAtDestination(0, cardThrownNum));
            }
        }
        base.OnTriggerEnterX(other);
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        //if(Table.dealerState == DealerState.DealingState)
        //{
        if (!cardFacingUp)
        {
            transform.rotation = throwingHand.GetAttachmentTransform("CardFaceDown").transform.rotation;
        }
        else if (cardFacingUp)
        {
            transform.rotation = throwingHand.GetAttachmentTransform("CardFaceUp").transform.rotation;
        }
        fastTorque = 4;
        slowTorque = .5f;

        base.OnAttachedToHand(attachedHand);
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
        if (LookingAtCard() && !cardWasManuallyFlipped)
        {
            Services.Dealer.lighting.gameObject.SetActive(false);
            Services.Dealer.isCheating = true;
            cheatCard = true;
        }
        cardPosHeld = transform.position;
        if (Table.gameState == GameState.NewRound)
        {
            if (CardIsFaceUp() && !Services.Dealer.isCheating)
            {
                Debug.Log("misdeal here");
                Table.gameState = GameState.Misdeal;
            }
        }
        base.HandAttachedUpdate(attachedHand);
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        //CardDeckScript deck;
        //if (!Services.Dealer.killingCards || Services.Dealer.cleaningCards)
        //{
        //    deck = GameObject.FindGameObjectWithTag("CardDeck").GetComponent<CardDeckScript>();
        //}
        //else deck = null;
        //if (deck != null && deck.safeToPutCardBack)
        //{
        //    deck.cardsInDeck.Add(cardType);
        //    deck.MakeDeckLarger();
        //    deck.safeToPutCardBack = false;
        //    Destroy(gameObject);
        //}
        //else
        //{
        StartPulse();
        if (!Services.PokerRules.thrownCards.Contains(gameObject) && Table.gameState == GameState.NewRound && !Services.Dealer.isCheating)
        {
            Services.PokerRules.thrownCards.Add(gameObject);
        }
        if (!Services.PokerRules.cardsPulled.Contains(cardType) && Table.gameState != GameState.Misdeal && !Services.Dealer.isCheating)
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
        base.OnDetachedFromHand(hand);
        //}
    }

    //basically we want to give some time for the card to actually LEAVE the hand before we check the velocity
    //it's this that lets us know whether the card was thrown hard or soft
    IEnumerator CheckVelocity(float time)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log("rb.velocity.magnitude = " + rb.velocity.magnitude);
        if (rb != null)
        {
            throwingVelocity = rb.velocity.magnitude;
            cardPosOnRelease = transform.position;
            float yDifference = cardPosHeld.y - cardPosOnRelease.y;
            if (yDifference < -0.1f)
            {
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
            if (!Services.Dealer.killingCards && !Services.Dealer.cleaningCards && !Services.Dealer.OutsideVR && !Services.Dealer.isCheating)
            {
                StartCoroutine(CheckIfCardThrownAtWrongTime(checkTime, cardThrownNum));
            }
            if (!Services.Dealer.killingCards && !Services.Dealer.cleaningCards && Table.gameState == GameState.NewRound && !Services.Dealer.OutsideVR && !Services.Dealer.isCheating)
            {
                StartCoroutine(CheckIfCardIsAtDestination(checkTime, cardThrownNum));
            }
        }
    }

    public bool CardIsInList(Card card)
    {
        if (Services.PokerRules.cardsLogged.Contains(card)) return true;
        else if (Services.PokerRules.cardsPulled.Contains(card.cardType)) return true;
        else if (Services.Dealer.deadCardsList.Contains(card)) return true;
        else return false;
    }

    IEnumerator WaitToCheckIfCardInList(float time)
    {
        yield return new WaitForSeconds(time);
        if (!CardIsInList(this) && cardOnTable)
        {
            Debug.Log("misdeal here");
            Table.gameState = GameState.Misdeal;
        }
    }

    public bool CardsAreFlying(GameObject card)
    {
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        for (int i = 0; i < cardsOnTable.Length; i++)
        {
            if (cardsOnTable[i].GetComponent<Card>().is_flying)
            {
                if (card != cardsOnTable[i])
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
        flying_start_rotation = transform.rotation;
        lerping = true;
        burningCard = true;
    }

    public void InitializeLerpForTorqueFlair(Quaternion dest)
    {
        flying_start_time = Time.time;
        flight_journey_angle = Quaternion.Angle(transform.rotation, dest);
        flying_start_position = transform.position;
        flying_start_rotation = transform.rotation;
        rotateOnAdd = true;
    }



    public IEnumerator LerpCardPos(Vector3 dest, float speed)
    {
        if (this != null)
        {
            while (lerping)
            {
                float distCovered = (Time.time - flying_start_time) * speed;
                float fracJourney = distCovered / flight_journey_distance;
                transform.position = Vector3.Lerp(flying_start_position, dest, fracJourney);
                yield return null;
            }
        }
    }

    public IEnumerator LerpCardRot(Quaternion dest, float speed)
    {
        if (this != null)
        {
            while (lerping)
            {
                float distCovered = (Time.time - flying_start_time) * speed;
                float fracJourney = distCovered / flight_journey_distance;
                transform.rotation = Quaternion.Lerp(flying_start_rotation, dest, fracJourney);
                yield return null;
            }
        }
    }

    public IEnumerator LerpCardRotOnAdd(Quaternion dest, float speed)
    {
        if (this != null)
        {
            while (rotateOnAdd)
            {
                float distCovered = (Time.time - flying_start_time) * speed;
                float fracJourney = distCovered / flight_journey_angle;
                fastTorque = Mathf.Lerp(fastTorque, 0, fracJourney);
                slowTorque = Mathf.Lerp(slowTorque, 0, fracJourney);
                transform.rotation = Quaternion.Lerp(transform.rotation, dest, fracJourney);
                yield return null;
            }
        }
    }


    IEnumerator StopRotating(Quaternion rot)
    {
        if (this != null)
        {
            while (rotateOnAdd)
            {
                float angle = Quaternion.Angle(transform.rotation, rot);
                if (Mathf.Approximately(angle, 0))
                {
                    //Debug.Log("done straightening");
                    rotateOnAdd = false;
                    firstTime = false;
                    fastTorque = 0;
                    slowTorque = 0;
                }
                else yield return null;
            }
            yield break;
        }
    }

    public void StraightenOutCards()
    {
        Vector3 endPos = GetCardPos();
        Quaternion endRot = GetCardRot();
        InitializeLerp(endPos);
        StartCoroutine(LerpCardPos(endPos, 0.25f));
        StartCoroutine(StopLerp(endPos));
    }

    public IEnumerator StopLerp(Vector3 pos)
    {
        while (lerping)
        {
            if (transform.position == pos)
            {
                lerping = false;
            }
            else yield return null;
        }
        yield break;
    }

    public IEnumerator StopFoldLerp(Vector3 pos)
    {
        while (lerping)
        {
            if (Vector3.Distance(transform.position, pos) < Random.Range(0, .09f))
            {
                lerping = false;
                foldedCards = true;
                InitializeRotation(pos, false);
            }
            else yield return null;
        }
        yield break;
    }

    public void InitializeRotation(Vector3 pos, bool forFlight)
    {
        height = transform.position.y;
        if (forFlight) radius = ChangeRadiusForHeight(pos);
        else radius = Vector3.Distance(transform.position, pos);
        constRadius = radius;
        //theta = Mathf.Atan2(transform.position.z - pos.z, transform.position.x - pos.x);
        theta = Mathf.Atan2(transform.position.x - pos.x, transform.position.z - pos.z);
    }

    public float ChangeRadiusForHeight(Vector3 pos)
    {
        Vector3 newPos = new Vector3(pos.x, height, pos.z);
        return Vector3.Distance(transform.position, newPos);
    }

    public IEnumerator CheckIfCardThrownAtWrongTime(float time, int cardspulled)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log("PLAYER TO ACT = " + Services.Dealer.playerToAct);
        if (Services.Dealer.playerToAct != null)
        {
            //Debug.Log("PLAYER TO ACT = " + Services.Dealer.playerToAct);
            StartPulse();
            Services.PokerRules.thrownCards.Add(gameObject);
            CleanUpCardInfo(this);
        }
    }

    public IEnumerator CheckIfCardIsAtDestination(float time, int cardsPulled)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log("thrownCardsList = " + Services.PokerRules.thrownCards.Count);
        float badCardsDebug = 0;
        for (int i = Services.PokerRules.thrownCards.Count - 1; i >= 0; i--)
        {
            //area for bug
            //had deck in hand and continuously pulled trigger
            if (Services.PokerRules.thrownCards[i].GetComponent<Card>() != null)
            {
                if (Services.PokerRules.thrownCards[i].GetComponent<Card>().thrownWrong || Services.PokerRules.thrownCards[i].GetComponent<Card>().CardIsFaceUp())
                {
                    badCardsDebug++;
                }
            }
        }
        //Debug.Log("badcards count = " + badCardsDebug);
        if (Services.PokerRules.CardIsInCorrectLocation(this, cardsPulled) && !CardIsFaceUp())
        {
            float badCards = 0;
            //Services.Dealer.messageText.text = "Card is in correct location";
            //Debug.Log("Card is in correct location");
            for (int i = Services.PokerRules.thrownCards.Count - 1; i >= 0; i--)
            {
                if (Services.PokerRules.thrownCards[i].GetComponent<Card>().thrownWrong ||
                    Services.PokerRules.thrownCards[i].GetComponent<Card>().is_flying ||
                    !Services.PokerRules.CardIsInCorrectLocation(Services.PokerRules.thrownCards[i].GetComponent<Card>(), Services.PokerRules.cardsPulled.Count) ||
                    Services.PokerRules.thrownCards[i].GetComponent<Card>().CardIsFaceUp())
                {
                    badCards++;
                }
            }
            if (badCards == 0 && !CardsAreFlying(gameObject))
            {
                Services.PokerRules.thrownCards.Clear();
                if (yPos == 0)
                {
                    StraightenOutCards();
                    readyToFloat = true;
                    if (!startLerping)
                    {
                        startLerping = true;
                        elapsedTimeForThrowTorque = 0;
                    }
                    yPos = GetCardPos().y;
                    GetComponent<BoxCollider>().enabled = false;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }
            }
            else
            {
                StartPulse();
                CleanUpCardInfo(this);
            }
        }
        else
        {
            StartPulse();
            CleanUpCardInfo(this);
        }
    }

    public void CleanUpCardInfo(Card card)
    {
        //Debug.Log("card is in wrong location");
        thrownWrong = true;
        Table.instance.RemoveCardFrom(card);
        Services.PokerRules.cardsLogged.Remove(card);
        Services.PokerRules.cardsPulled.Remove(cardType);
        //Debug.Log("Removing " + cardType.rank + " of " + cardType.suit);
        cardDeck.GetComponent<CardDeckScript>().cardsInDeck.Add(cardType);
        flying_start_time = Time.time;
        flight_journey_distance = Vector3.Distance(transform.position, cardDeck.transform.position);
        flying_start_position = transform.position;
        Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], .05f);
        Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], .05f);
    }

    public void BringCardBack()
    {
        if (thrownWrong && !Services.Dealer.killingCards && !Services.Dealer.cleaningCards)
        {
            //Debug.Log("cardsPulled = " + Services.PokerRules.cardsPulled.Count);
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
                    foreach (GameObject card in Services.PokerRules.thrownCards)
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

    public PokerPlayerRedux GetCardOwner()
    {
        int playerIndex = Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardThrownNum);
        return Services.Dealer.players[playerIndex];
    }

    public void FloatInPlace(float speed, float distance)
    {
        if (!foldedCards && !Services.Dealer.killingCards && !Services.Dealer.cleaningCards)
        {
            if (readyToFloat)
            {
                if (yPos == 0) yPos = GetCardPos().y;
                if (rb != null) rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                GetComponent<BoxCollider>().enabled = false;
                transform.position = new Vector3(transform.position.x, yPos + Mathf.PingPong(Time.time / speed, distance), transform.position.z);
            }
            else
            {
                if (rb != null) rb.useGravity = true;
            }
        }
    }

    public Vector3 GetCardPos()
    {
        PokerPlayerRedux player = GetCardOwner();
        Vector3 endPos;
        if (Table.instance.playerCards[player.SeatPos].Count == 1)
        {
            endPos = player.cardPos[0].transform.position;
        }
        else
        {
            endPos = player.cardPos[1].transform.position;
        }
        return endPos;
    }

    public Quaternion GetCardRot()
    {
        PokerPlayerRedux player = GetCardOwner();
        //Debug.Log(cardType.rank + " of " + cardType.suit + " belongs to " + player.playerName);
        Quaternion endRot;
        if (Table.instance.playerCards[player.SeatPos].Count == 1)
        {
            endRot = player.cardPos[0].transform.rotation;
        }
        else
        {
            endRot = player.cardPos[1].transform.rotation;
        }
        return endRot;
    }

    public Quaternion GetCardRot(PokerPlayerRedux player)
    {
        Quaternion endRot;
        if (Table.instance.playerCards[player.SeatPos].Count == 1)
        {
            endRot = player.cardPos[0].transform.rotation;
        }
        else
        {
            endRot = player.cardPos[1].transform.rotation;
        }
        return endRot;
    }

    //part of the interaction class this derives from.
    public override void CheckSwipeDirection(Hand hand)
    {
        base.CheckSwipeDirection(throwingHand);
    }

    public override void TouchingTrackPad()
    {
        //RotateCard();
        base.TouchingTrackPad();
    }

    //the actual function called in order to trigger the card flip
    public void RotateCard()
    {
        if (!flippingCard)
        {
            flippingCard = true;
            rotationAtFlipStart = transform.localRotation;
            cardWasManuallyFlipped = true;
        }
    }

    public bool CardIsFaceUp()
    {
        return (Vector3.Dot(transform.forward, Vector3.down) > 0);
    }

    public bool LookingAtCard()
    {
        //if(testingDot == 0)
        //{
        //    testingDot = Vector3.Dot(transform.forward, Camera.main.transform.forward);
        //}
        //else
        //{
        //    if(testingDot < Vector3.Dot(transform.forward, Camera.main.transform.forward))
        //    {
        //        testingDot = Vector3.Dot(transform.forward, Camera.main.transform.forward);
        //    }
        //}
        //Debug.Log("highest testingDot = " + testingDot);
        return Vector3.Dot(transform.forward, Camera.main.transform.forward) > 0.5f;
    }

    public float GetCardAngle(string comparisonPoint)
    {
        Transform obj = GameObject.Find(comparisonPoint).transform;
        Vector3 targetDir = obj.position - transform.position;
        return Vector3.Angle(targetDir, -transform.forward);
    }

    public void StartPulse()
    {
        callingPulse = true;
        startTime = Time.time;
        pingPongCount = 0;
        emission = 0;
    }

    public void PulseGlow()
    {
        if (callingPulse)
        {
            Color baseColor = new Vector4(0.8235294f, 0.1574394f, 0.5570934f, 0);
            float previousEmission = emission;
            emission = PingPong(glowSpeed * (startTime - Time.time), 0, maxGlow);
            float currentEmission = emission;
            Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
            GetComponent<Renderer>().material.SetColor("_EmissionColor", finalColor);
            if(currentEmission < previousEmission)
            {
                pingPongCount++;
            }
            else if(currentEmission > previousEmission && pingPongCount >= 1)
            {
                callingPulse = false;
                GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    float PingPong(float time, float minLength, float maxLength)
    {
        return Mathf.PingPong(time, maxLength - minLength) + minLength;
    }


}
