using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class CardDeckScript : InteractionSuperClass {

    GameObject cardDeck;
    Quaternion cardDeckPos;

    void Update()
    {
        cardDeck = this.gameObject;
        cardDeckPos = cardDeck.transform.rotation;
    }

    public override void OnTriggerEnterX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.gameObject.GetComponent<Hand>() == throwingHand)
            {
                isTouchingDeck = true;
                Debug.Log("isTouchingDeck = " + isTouchingDeck);
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
                Debug.Log("isTouchingDeck = " + isTouchingDeck);
            }
        }
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
        if(hand.otherHand.GetStandardInteractionButtonDown() == true && isHoldingCard == false && isTouchingDeck == true)
        {
            isHoldingCard = true;
            Debug.Log("Pulling a Card");
            GameObject playingCard = Instantiate(cardPrefab, interactableObject.transform.position, Quaternion.identity);
            hand.otherHand.AttachObject(playingCard);
        }
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        cardDeck.transform.rotation = Quaternion.Euler(0, 90, 0);
        if(attachedHand.currentAttachedObject.tag == "CardDeck")
        {
            Debug.Log("attachedHand = " + attachedHand.name + " and attached to it is " + attachedHand.currentAttachedObject.name);
            deckHand = attachedHand;
            throwingHand = attachedHand.otherHand;
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
        base.OnDetachedFromHand(hand);
    }

}
