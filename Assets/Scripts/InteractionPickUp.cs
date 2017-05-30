﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

//put this script on cube
public class InteractionPickUp : MonoBehaviour {


    public Interactable interactableObject; //insert cardDeck, or other interactable that you need hoverlocked in inspector
    public GameObject cardPrefab; //insert playingCard prefab in inspector
    protected bool isHoldingCard;
    protected bool isTouchingDeck = false;

    protected Hand deckHand;
    protected Hand throwingHand;


    //adding this in to make it so that I can only instantiate the card if it's colliding with the card deck
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name + " is touching the cardDeck");
        if(this.gameObject.name == "CardDeck")
        {
            if(other.gameObject.tag == "Hand")
            {
                if(other.gameObject.GetComponent<Hand>() == throwingHand)
                {
                    Debug.Log("isTouchingDeck = " + isTouchingDeck);
                    isTouchingDeck = true;
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log(other.gameObject.name + " is touching the cardDeck");
        if (this.gameObject.name == "CardDeck")
        {
            if (other.gameObject.tag == "Hand")
            {
                if (other.gameObject.GetComponent<Hand>() == throwingHand)
                {
                    Debug.Log("isTouchingDeck = " + isTouchingDeck);
                    isTouchingDeck = false;
                }
            }
        }
    }


    //this happens whenever a hand is near this object
    void HandHoverUpdate(Hand hand) //this applies to either controller
    {
        Debug.Log("Hand is holding " + hand.AttachedObjects.Count + " objects.");
        if (hand.GetStandardInteractionButtonDown() == true) //on Vive controller, this is the trigger
        {
            hand.AttachObject(gameObject);
            hand.HoverLock(interactableObject);
        }

        if (hand.otherHand.GetStandardInteractionButtonDown() == true && isHoldingCard == false && isTouchingDeck == true)
        {
            isHoldingCard = true;
            Debug.Log("Pulling a Card");
            GameObject playingCard = Instantiate(cardPrefab, interactableObject.transform.position, Quaternion.identity);
            hand.otherHand.AttachObject(playingCard);
        }

    }

    //this happens whenever an object is attached to this hand, for whatever reason
    void OnAttachedToHand(Hand attachedHand)
    {
        if(attachedHand.currentAttachedObject.tag == "CardDeck")
        {
            Debug.Log("attachedHand = " + attachedHand.name + " and attached to it is " + attachedHand.currentAttachedObject.name);
            deckHand = attachedHand;
            throwingHand = attachedHand.otherHand;
        }
        GetComponent<Rigidbody>().isKinematic = true; //turn off the physics, we we can hold it
    }

    //this is like update, as long as we're holding something
    void HandAttachedUpdate(Hand attachedHand)
    {
        if (attachedHand.GetStandardInteractionButton() == false)
        {
            attachedHand.DetachObject(gameObject);
            attachedHand.HoverUnlock(interactableObject);
        }
        if (attachedHand.otherHand.GetStandardInteractionButton() == false)
        {
            isHoldingCard = false;
        }
    }

    //this happens whent he object is detached from the hand for whatever reason
    void OnDetachedFromHand(Hand hand)
    {
        GetComponent<Rigidbody>().isKinematic = false; //turns on physics

        //apply forces to it, as if we're throwing it
        GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
        GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
    }

}
