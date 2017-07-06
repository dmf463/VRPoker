using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class CardDeckScript : InteractionSuperClass {

    //everything in the script that controls the physics needs to be in a script, or a section of script attached to an enum we'll call PitchMode that way, depending on what controller mode I'm in, I can either switch on the elements I need in this script, or switch on the RakeMode and do the raking script stuff.

    public List<GameObject> playingCardList = new List<GameObject>();
    GameObject cardDeck;

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
        newCardDeckScale = transform.localScale;
        decreaseCardDeckBy = new Vector3 (newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        currentCardDeckScale = newCardDeckScale;
    }


    void Update()
    {

        cardDeck = this.gameObject;
        if (playingCardList.Count == 0)
        {
            deckIsDestroyed = true;
            Destroy(cardDeck);
        }


            if (playingCardList.Count == 0)
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
            playingCardList.Count != 0)
        {
            isHoldingCard = true;
            isTouchingDeck = false;
            int cardPos = Random.Range(0, playingCardList.Count);
            GameObject playingCard = Instantiate(playingCardList[cardPos], interactableObject.transform.position, Quaternion.identity);
            playingCard.name = playingCardList[cardPos].name;
            playingCardList.Remove(playingCardList[cardPos]);
            playingCard.GetComponent<BoxCollider>().enabled = true;
            playingCard.GetComponent<PlayingCardPhysics>().enabled = true;
            hand.otherHand.AttachObject(playingCard);

            currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
            transform.localScale = currentCardDeckScale;
            if(playingCardList.Count == 0)
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
            int playingCardListCount = playingCardList.Count;
            for (int i = playingCardListCount - 1; i >= 0; i--)
            {
                Debug.Log(i);
                GameObject playingCard = cardDeck.transform.GetChild(i).gameObject;
                playingCard.transform.parent = null;
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.AddComponent<Rigidbody>();
                Debug.Log("rigidBody added at " + Time.time);
                playingCard.AddComponent<ConstantForce>();
                playingCard.GetComponent<PlayingCardPhysics>().enabled = true;
                playingCard.GetComponent<PlayingCardPhysics>().badThrow = true;
                playingCard.GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddExplosionForce(explosionPower, playingCard.transform.position, explosionRadius, 0, ForceMode.Impulse);

                if (i == 0)
                {
                    deckIsDestroyed = true;
                    thrownDeck = true;
                }
            }
        }
        base.OnDetachedFromHand(hand);
    }

}
