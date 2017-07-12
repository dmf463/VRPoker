using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class CardDeckScript : InteractionSuperClass {

    List<CardType> cardsInDeck;
    GameObject cardDeck;

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

    }

    void OnCollisionEnter(Collision other)
    {
        if(TableCards.dealerState == DealerState.ShufflingState)
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
                TableCards.dealerState = DealerState.DealingState;
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
            }
        }
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
        if(hand.otherHand.GetStandardInteractionButtonDown() == true && 
            handIsHoldingCard == false && 
            handTouchingDeck == true && 
            cardsInDeck.Count != 0)
        {
            handIsHoldingCard = true;
            handTouchingDeck = false;
            int cardPos = Random.Range(0, cardsInDeck.Count);
            CardType cardType = cardsInDeck[cardPos];
            Card card = CreateCard(cardType, interactableObject.transform.position, Quaternion.identity);
            card.gameObject.name = (card.cardType.rank + " of " + card.cardType.suit);
            hand.otherHand.AttachObject(card.gameObject);
            MakeDeckSmaller();
            if (cardsInDeck.Count == 0)
            {
                hand.HoverUnlock(interactableObject);
                Destroy(cardDeck);
                Debug.Log("Destroyed Deck");
                TableCards.dealerState = DealerState.ShufflingState;
            }
        }
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
        Debug.Log("badThrowVelocity from CardDeck = " + badThrowVelocity);

        //
        //should seriously rethink this because of the way it feels between dropping the deck and throwing the deck
        //
        if (hand.GetTrackedObjectVelocity().magnitude > 1) deckIsBeingThrown = true;
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
        cardsInDeck.Clear();
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
        Debug.Log("deck has" + cardsInDeck.Count + "cards");
        foreach(CardType card in cardsInDeck)
        {
            Debug.Log(card.rank + " of " + card.suit);
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
            //Debug.Log(i);
            //GameObject referenceCard = cardDeck.transform.GetChild(i).gameObject;
            //Vector3 pos = referenceCard.transform.position;
            //Quaternion rot = referenceCard.transform.rotation;

            GameObject playingCard = CreateCard(cardsInDeck[i], transform.position, Quaternion.identity).gameObject;
            playingCard.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
            playingCard.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;

            //GameObject playingCard = cardDeck.transform.GetChild(i).gameObject;
            ////Debug.Log("playingCard is " + playingCard.name);

            //playingCard.transform.parent = null;
            ////Debug.Log("playingCard parent = " + playingCard.transform.parent);

            //playingCard.GetComponent<BoxCollider>().enabled = true;
            ////Debug.Log("playing card boxCollider enabled = " + playingCard.GetComponent<BoxCollider>().enabled);

            //playingCard.AddComponent<Rigidbody>();
            ////Debug.Log("playing Card has rb and rb is " + playingCard.GetComponent<Rigidbody>());

            ////Debug.Log("rigidBody added at " + Time.time);

            //playingCard.AddComponent<ConstantForce>();
            ////Debug.Log("playing card has constantForce and constantForce is " + playingCard.GetComponent<ConstantForce>());

            //playingCard.GetComponent<Card>().enabled = true;
            ////Debug.Log("playingCard script is enabled = " + playingCard.GetComponent<Card>().enabled);


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
        TableCards.dealerState = DealerState.ShufflingState;
    }

}
