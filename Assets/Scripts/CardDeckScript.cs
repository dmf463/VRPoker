using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class CardDeckScript : InteractionSuperClass {

    List<CardType> cardsInDeck;
    GameObject cardDeck;
    public float cardSpawnOffset;
    public float velocityThreshold;

    List<Mesh>[] cardMeshes;
    public List<Mesh> spadeMeshes;
    public List<Mesh> heartMeshes;
    public List<Mesh> diamondMeshes;
    public List<Mesh> clubMeshes;

    [HideInInspector]
    public Vector3 newCardDeckScale;
    [HideInInspector]
    public Vector3 currentCardDeckScale;
    [HideInInspector]
    public Vector3 oneCardScale;
    [HideInInspector]
    public bool deckIsBeingThrown = false;
    public bool deckWasThrown;
    public float badThrowVelocity;
    private bool readyForAnotherCard = false;
    private bool grabbingHighCard;
    private bool grabbingLowCard;

    void Start()
    {
        cardDeck = this.gameObject;
        cardMeshes = new List<Mesh>[4]
        {
            spadeMeshes,
            heartMeshes,
            diamondMeshes,
            clubMeshes
        };
        if (newCardDeckScale.y == 0)
        {
            newCardDeckScale = transform.localScale;
            currentCardDeckScale = newCardDeckScale;
        } 
        oneCardScale = new Vector3 (newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        PopulateCardDeck();
        deckIsEmpty = false;
    }


    void Update()
    {
        if(throwingHand != null)
        {
            CheckPressPosition(throwingHand);
            CheckSwipeDirection();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            int cardPos = Random.Range(0, cardsInDeck.Count);
            CardType cardType = cardsInDeck[cardPos];
            Card card = CreateCard(cardType, GameObject.Find("ShufflingArea").transform.position, Quaternion.identity);
            card.gameObject.name = (card.cardType.rank + " of " + card.cardType.suit);
        }

    }

    void OnCollisionEnter(Collision other)
    {
        if(Table.dealerState == DealerState.ShufflingState)
        {
            if(other.gameObject.tag == "PlayingCard")
            {
                Destroy(other.gameObject);
                MakeDeckLarger();
            }
            if (currentCardDeckScale.y >= newCardDeckScale.y)
            {
                GameObject[] deadCards = GameObject.FindGameObjectsWithTag("PlayingCard");
                foreach(GameObject card in deadCards)
                {
                    Destroy(card);
                }
                RefillCardDeck();
                Table.dealerState = DealerState.DealingState;
            }
        }
    }

    public override void OnTriggerEnterX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.gameObject.GetComponent<Hand>() == throwingHand)
            {
                handTouchingDeck = true;
                readyForAnotherCard = true;
            }
        }
    }

    public override void OnTriggerExitX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.GetComponent<Hand>() == throwingHand)
            {
                handTouchingDeck = false;
                readyForAnotherCard = false;
            }
        }
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
        if (hand.otherHand.GetStandardInteractionButtonDown() == true && 
            handIsHoldingCard == false && 
            handTouchingDeck == true && 
            cardsInDeck.Count != 0)
        {
            handIsHoldingCard = true;
            handTouchingDeck = false;
            Card card = CreateCard(GrabACard(), interactableObject.transform.position, Quaternion.identity);
            card.gameObject.name = (card.cardType.rank + " of " + card.cardType.suit);
            hand.otherHand.AttachObject(card.gameObject);
            MakeDeckSmaller();
            if (cardsInDeck.Count == 0)
            {
                hand.HoverUnlock(interactableObject);
                Destroy(cardDeck);
                Debug.Log("Destroyed Deck");
                Table.dealerState = DealerState.ShufflingState;
            }
        }
    }

    public CardType GrabACard()
    {
        CardType cardToGrab = null;
        if (grabbingLowCard == true)
        {
            Debug.Log("Grabbing lowest card");
            grabbingLowCard = false;
            cardToGrab = cardsInDeck[0];
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                CardType currentCard = cardsInDeck[i];
                if((int)currentCard.rank < (int)cardToGrab.rank)
                {
                    cardToGrab = currentCard;
                }
            }
        }
        else if (grabbingHighCard == true)
        {
            Debug.Log("Grabbing high card");
            grabbingHighCard = false;
            cardToGrab = cardsInDeck[0];
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                CardType currentCard = cardsInDeck[i];
                if ((int)currentCard.rank > (int)cardToGrab.rank)
                {
                    cardToGrab = currentCard;
                }
            }
        }
        else
        {
            Debug.Log("grabbing random card");
            int cardPos = Random.Range(0, cardsInDeck.Count);
            cardToGrab = cardsInDeck[cardPos];
        }
        return cardToGrab;
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        handTouchingDeck = false;
        if(attachedHand.currentAttachedObject.tag == "CardDeck")
        {
            deckHand = attachedHand;
            throwingHand = attachedHand.otherHand;
            //Debug.Log("deckHand = " + attachedHand.name + " and throwingHand = " + attachedHand.otherHand.name);
        }
        base.OnAttachedToHand(attachedHand);
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
        base.HandAttachedUpdate(attachedHand);
        if (attachedHand.otherHand.GetStandardInteractionButton() == false)
        {
            handIsHoldingCard = false;
        }
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        badThrowVelocity = deckHand.GetTrackedObjectVelocity().magnitude;
        //Debug.Log("badThrowVelocity from CardDeck = " + badThrowVelocity);

        //
        //should seriously rethink this because of the way it feels between dropping the deck and throwing the deck
        //
        if (hand.GetTrackedObjectVelocity().magnitude > velocityThreshold) deckIsBeingThrown = true;
        if (deckIsBeingThrown == true)
        {
            deckIsBeingThrown = false;
            FiftyTwoCardPickUp();
            deckIsEmpty = true;
            deckWasThrown = true;
        }
        base.OnDetachedFromHand(hand);
    }

    public void RefillCardDeck()
    {
        Debug.Log("Refilling CardDeck");
        cardsInDeck.Clear();
        Table.instance.NewHand();
        SuitType[] suits = new SuitType[4]
        {
            SuitType.Spades,
            SuitType.Hearts,
            SuitType.Diamonds,
            SuitType.Clubs
        };
        RankType[] ranks = new RankType[13]
        {
            RankType.Two,
            RankType.Three,
            RankType.Four,
            RankType.Five,
            RankType.Six,
            RankType.Seven,
            RankType.Eight,
            RankType.Nine,
            RankType.Ten,
            RankType.Jack,
            RankType.Queen,
            RankType.King,
            RankType.Ace
        };

        foreach (SuitType suit in suits)
        {
            foreach (RankType rank in ranks)
            {
                cardsInDeck.Add(new CardType(rank, suit));
            }
        }
    }

    public void PopulateCardDeck()
    {
        cardsInDeck = new List<CardType>();
        SuitType[] suits = new SuitType[4] 
        {
            SuitType.Spades,
            SuitType.Hearts,
            SuitType.Diamonds,
            SuitType.Clubs
        };
        RankType[] ranks = new RankType[13]
        {
            RankType.Two,
            RankType.Three,
            RankType.Four,
            RankType.Five,
            RankType.Six,
            RankType.Seven,
            RankType.Eight,
            RankType.Nine,
            RankType.Ten,
            RankType.Jack,
            RankType.Queen,
            RankType.King,
            RankType.Ace
        };

        foreach (SuitType suit in suits)
        {
            foreach(RankType rank in ranks)
            {
                cardsInDeck.Add(new CardType(rank, suit));
            }
        }
    }

    public Card CreateCard(CardType cardType, Vector3 position, Quaternion rotation)
    {
        cardsInDeck.Remove(cardType);
        GameObject playingCard = Instantiate(Services.PrefabDB.Card, position, rotation);
        playingCard.GetComponent<MeshFilter>().mesh = cardMeshes[(int)cardType.suit][(int)cardType.rank - 2];
        playingCard.GetComponent<Card>().cardType = cardType;
        return playingCard.GetComponent<Card>();
    }

    public void BuildDeckFromOneCard(GameObject newCardDeck)
    {
        newCardDeckScale = newCardDeck.transform.localScale;
        oneCardScale = new Vector3(newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        newCardDeck.transform.localScale = oneCardScale;
        currentCardDeckScale = newCardDeck.transform.localScale;
    }

    public void MakeDeckSmaller()
    {
        currentCardDeckScale.y = currentCardDeckScale.y - oneCardScale.y;
        transform.localScale = currentCardDeckScale;
    }

    public void MakeDeckLarger()
    {
        currentCardDeckScale.y = currentCardDeckScale.y + oneCardScale.y;
        transform.localScale = currentCardDeckScale;
    }

    public void FiftyTwoCardPickUp()
    {
        int cardsInDeckCount = cardsInDeck.Count;
        for (int i = cardsInDeckCount - 1; i >= 0; i--)
        {

            GameObject playingCard = CreateCard(cardsInDeck[i], transform.position + Random.insideUnitSphere * cardSpawnOffset , Quaternion.identity).gameObject;
            playingCard.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
            playingCard.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;


            playingCard.GetComponent<Card>().cardThrownWrong = true;
            ////Debug.Log("bad throw is = " + playingCard.GetComponent<Card>().badThrow);
            playingCard.GetComponent<Rigidbody>().AddForce(deckHand.GetTrackedObjectVelocity(), ForceMode.Impulse);
            playingCard.GetComponent<Rigidbody>().AddTorque(deckHand.GetTrackedObjectAngularVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
        }
        StartCoroutine(WaitToDestroyDeck(0.25f));
        
    }
    IEnumerator WaitToDestroyDeck(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(cardDeck);
        Table.dealerState = DealerState.ShufflingState;
    }

    public override void OnSwipeBottom()
    {
        if (handTouchingDeck) grabbingLowCard = true;
        base.OnSwipeBottom();
    }

    public override void OnSwipeTop()
    {
        if (handTouchingDeck) grabbingHighCard = true;
        base.OnSwipeTop();
    }

    public override void OnPressBottom()
    {
        if (readyForAnotherCard && handIsHoldingCard == true)
        {
            GameObject firstCard = throwingHand.currentAttachedObject.gameObject;
            Vector3 offset = new Vector3(-0.01f, -0.01f, 0) * throwingHand.AttachedObjects.Count;
            int cardPos = Random.Range(0, cardsInDeck.Count);
            CardType cardType = cardsInDeck[cardPos];
            Card card = CreateCard(cardType, throwingHand.transform.position, Quaternion.identity);
            card.gameObject.name = (card.cardType.rank + " of " + card.cardType.suit);
            if(throwingHand.AttachedObjects.Count < 3)
            {
                throwingHand.AttachObject(card.gameObject, Hand.AttachmentFlags.ParentToHand);
                card.gameObject.transform.localPosition = new Vector3(firstCard.transform.localPosition.x + offset.x, firstCard.transform.localPosition.y + offset.y, firstCard.transform.localPosition.z);
            }
            MakeDeckSmaller();
            if (cardsInDeck.Count == 0)
            {
                deckHand.HoverUnlock(interactableObject);
                Destroy(cardDeck);
                Debug.Log("Destroyed Deck");
                Table.dealerState = DealerState.ShufflingState;
            }
        }
    }
}
