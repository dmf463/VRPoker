using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

//put this script on cube
public class InteractionPickUp : MonoBehaviour {


    public Interactable cardDeck; //insert cardDeck in inspector
    public GameObject cardPrefab; //insert playingCard prefab in inspector
    public bool isHoldingCard;
    public bool isTouchingDeck = false;


    //adding this in to make it so that I can only instantiate the card if it's colliding with the card deck
    public void OnCollisionEnter(Collider other)
    {
        if (other.gameObject.tag == "CardDeck")
        {
            isTouchingDeck = true;
        }
    }


    //this happens whenever a hand is near this object
    void HandHoverUpdate(Hand hand) //this applies to either controller
    {
        Debug.Log("Hand is holding " + hand.AttachedObjects.Count + " objects.");
        if (hand.GetStandardInteractionButtonDown() == true) //on Vive controller, this is the trigger
        {
            hand.AttachObject(gameObject);
            hand.HoverLock(cardDeck);
        }

        if (hand.otherHand.GetStandardInteractionButtonDown() == true && isHoldingCard == false && isTouchingDeck == true)
        {
            isHoldingCard = true;
            Debug.Log("Pulling a Card");
            GameObject playingCard = Instantiate(cardPrefab, cardDeck.transform.position, Quaternion.identity);
            hand.otherHand.AttachObject(playingCard);

        }

    }

    //this happens whenever an object is attached to this hand, for whatever reason
    void OnAttachedToHand(Hand hand)
    {
        GetComponent<Rigidbody>().isKinematic = true; //turn off the physics, we we can hold it
    }

    //this is like update, as long as we're holding something
    void HandAttachedUpdate(Hand hand)
    {
        if (hand.GetStandardInteractionButton() == false)
        {
            hand.DetachObject(gameObject);
            hand.HoverUnlock(cardDeck);
        }
        if (hand.otherHand.GetStandardInteractionButton() == false)
        {
            isHoldingCard = false;
            isTouchingDeck = false;
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
