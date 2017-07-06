using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class CardDeckScript : InteractionSuperClass {

    //everything in the script that controls the physics needs to be in a script, or a section of script attached to an enum we'll call PitchMode that way, depending on what controller mode I'm in, I can either switch on the elements I need in this script, or switch on the RakeMode and do the raking script stuff.

    List<CardType> cardsInDeck;
    GameObject cardDeck;
    public GameObject cardPrefab;

    List<Mesh>[] cardMeshes;
    public List<Mesh> spadeMeshes;
    public List<Mesh> heartMeshes;
    public List<Mesh> diamondMeshes;
    public List<Mesh> clubMeshes;


    Vector3 newCardDeckScale;
    Vector3 currentCardDeckScale;
    Vector3 decreaseCardDeckBy;
    public bool deckGotThrown = false;
    float explosionPower = 1;
    float explosionRadius = 30;
    public bool thrownDeck;
    public float badThrowVelocity;

    void Start()
    {
        cardMeshes = new List<Mesh>[4]
        {
            spadeMeshes,
            heartMeshes,
            diamondMeshes,
            clubMeshes
        };
        newCardDeckScale = transform.localScale;
        decreaseCardDeckBy = new Vector3 (newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        currentCardDeckScale = newCardDeckScale;
        PopulateCardDeck();
    }


    void Update()
    {

        cardDeck = this.gameObject;
        if (cardsInDeck.Count == 0)
        {
            deckIsDestroyed = true;
            Destroy(cardDeck);
        }


            if (cardsInDeck.Count == 0)
            {
                Destroy(cardDeck);
            }
        }

    public override void OnTriggerEnterX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.gameObject.GetComponent<Hand>() == throwingHand)
            {
                isTouchingDeck = true;
            }
        }
    }

    public override void OnTriggerExitX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.GetComponent<Hand>() == throwingHand)
            {
                isTouchingDeck = false;
            }
        }
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
        if(hand.otherHand.GetStandardInteractionButtonDown() == true && 
            isHoldingCard == false && 
            isTouchingDeck == true && 
            cardsInDeck.Count != 0)
        {
            isHoldingCard = true;
            isTouchingDeck = false;
            int cardPos = Random.Range(0, cardsInDeck.Count);
            CardType cardType = cardsInDeck[cardPos];
            Card card = CreateCard(cardType, interactableObject.transform.position, Quaternion.identity);
            hand.otherHand.AttachObject(card.gameObject);
            currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
            transform.localScale = currentCardDeckScale;
            if(cardsInDeck.Count == 0)
            {
                hand.HoverUnlock(interactableObject);
                Destroy(cardDeck);
            }
        }
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        isTouchingDeck = false;
        if(attachedHand.currentAttachedObject.tag == "CardDeck")
        {
            deckHand = attachedHand;
            throwingHand = attachedHand.otherHand;
            Debug.Log("deckHand = " + attachedHand.name + " and throwingHand = " + attachedHand.otherHand.name);
        }
        base.OnAttachedToHand(attachedHand);
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
        base.HandAttachedUpdate(attachedHand);
        if (attachedHand.otherHand.GetStandardInteractionButton() == false)
        {
            isHoldingCard = false;
        }
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        badThrowVelocity = deckHand.GetTrackedObjectVelocity().magnitude;

        //
        //should seriously rethink this because of the way it feels between dropping the deck and throwing the deck
        //
        if (hand.GetTrackedObjectVelocity().magnitude > 1) deckGotThrown = true;
        if (deckGotThrown == true)
        {
            deckGotThrown = false;
            int cardsInDeckCount = cardsInDeck.Count;
            for (int i = cardsInDeckCount - 1; i >= 0; i--)
            {
                //Debug.Log(i);
                //GameObject referenceCard = cardDeck.transform.GetChild(i).gameObject;
                //Vector3 pos = referenceCard.transform.position;
                //Quaternion rot = referenceCard.transform.rotation;

                //GameObject playingCard = CreateCard(cardsInDeck[i], pos, rot).gameObject;
                //playingCard.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
                //playingCard.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;

                GameObject playingCard = cardDeck.transform.GetChild(i).gameObject;

                playingCard.transform.parent = null;

                playingCard.GetComponent<BoxCollider>().enabled = true;

                playingCard.AddComponent<Rigidbody>();

                Debug.Log("rigidBody added at " + Time.time);

                playingCard.AddComponent<ConstantForce>();

                playingCard.GetComponent<Card>().enabled = true;


                playingCard.GetComponent<Card>().badThrow = true;
                playingCard.GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddExplosionForce(explosionPower, playingCard.transform.position, explosionRadius, 0, ForceMode.Impulse);
            }
            Debug.Log("deck thrown at time " + Time.time);
            deckIsDestroyed = true;
            thrownDeck = true;
        }
        base.OnDetachedFromHand(hand);
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
        GameObject playingCard = Instantiate(cardPrefab, position, rotation);
        playingCard.GetComponent<MeshFilter>().mesh = cardMeshes[(int)cardType.suit][(int)cardType.rank - 2];
        playingCard.GetComponent<Card>().cardType = cardType;
        return playingCard.GetComponent<Card>();
    }

}
