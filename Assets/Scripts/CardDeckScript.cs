using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class CardDeckScript : InteractionSuperClass {

    GameObject cardDeck;
    Quaternion cardDeckPos;
    public List<GameObject> playingCardList = new List<GameObject>();
    Vector3 newCardDeckScale;
    Vector3 currentCardDeckScale;
    Vector3 decreaseCardDeckBy;
    public bool deckGotThrown = false;
    float explosionPower = 1;
    float explosionRadius = 30;
    public bool thrownDeck;

    void Start()
    {
        newCardDeckScale = transform.localScale;
        decreaseCardDeckBy = new Vector3 (newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        //Debug.Log("decreaseCardDeckby = " + decreaseCardDeckBy);
        currentCardDeckScale = newCardDeckScale;
    }


    void Update()
    {
        //Debug.Log("deckGotThrown is " + deckGotThrown);
        //Debug.Log("cardDeck has this many children (SHOULD BE 52) : " + transform.childCount);
        //Debug.Log("first card in the deck is: " + transform.GetChild(0).name);
        cardDeck = this.gameObject;
        cardDeckPos = cardDeck.transform.rotation;
        if (playingCardList.Count == 0)
        {
            deckIsDestroyed = true;
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
                //Debug.Log("isTouchingDeck = " + isTouchingDeck);
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
                //Debug.Log("isTouchingDeck = " + isTouchingDeck);
            }
        }
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
        if(hand.otherHand.GetStandardInteractionButtonDown() == true && isHoldingCard == false && isTouchingDeck == true && playingCardList.Count != 0)
        {
            isHoldingCard = true;
            isTouchingDeck = false;
            int cardPos = Random.Range(0, playingCardList.Count);
            //Debug.Log("Pulling a Card");
            //GameObject playingCard = Instantiate(transform.GetChild(GetCardFromDeck()).gameObject, interactableObject.transform.position, Quaternion.identity);
            //GameObject playingCard = Instantiate(cardPrefab, interactableObject.transform.position, Quaternion.identity);
            GameObject playingCard = Instantiate(playingCardList[cardPos], interactableObject.transform.position, Quaternion.identity);
            playingCardList.Remove(playingCardList[cardPos]);
            playingCard.GetComponent<BoxCollider>().enabled = true;
            playingCard.GetComponent<PlayingCardScript>().enabled = true;
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
        //cardDeck.transform.rotation = Quaternion.Euler(0, 0, 0);
        if(attachedHand.currentAttachedObject.tag == "CardDeck")
        {
            //Debug.Log("attachedHand = " + attachedHand.name + " and attached to it is " + attachedHand.currentAttachedObject.name);
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
        Debug.Log("there are " + playingCardList.Count + " cards in the deck");
        if (hand.GetTrackedObjectVelocity().magnitude > 1) deckGotThrown = true;
        if (deckGotThrown == true)
        {
            deckGotThrown = false;
            int playingCardListCount = cardDeck.transform.childCount;
            for (int i = playingCardListCount - 1; i >= 0; i--)
            {
                Debug.Log(i);
                GameObject playingCard = cardDeck.transform.GetChild(i).gameObject;
                playingCard.transform.parent = null;
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.AddComponent<Rigidbody>();
                Debug.Log("rigidBody added at " + Time.time);
                playingCard.AddComponent<ConstantForce>();
                playingCard.GetComponent<PlayingCardScript>().enabled = true;
                playingCard.GetComponent<PlayingCardScript>().badThrow = true;
                playingCard.GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddExplosionForce(explosionPower, playingCard.transform.position, explosionRadius, 0, ForceMode.Impulse);

                if (i == 0)
                {
                    deckIsDestroyed = true;
                    thrownDeck = true;
                    //Destroy(cardDeck);
                }
            }
        }
        base.OnDetachedFromHand(hand);
    }

    //public int GetCardFromDeck()
    //{
    //    int cardPos = Random.Range(0, 52);
    //    return cardPos;
    //}

}
