using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

enum DealerState { DealingState, ShufflingState };

public class InteractionSuperClass : MonoBehaviour {

    public Interactable interactableObject; //insert cardDeck, or other interactable that you need hoverlocked in inspector
    protected bool handIsHoldingCard;
    protected bool handTouchingDeck = false;
    protected bool handIsTouchingCard;
    protected const float FORCE_MULTIPLIER = 1.80f;
    protected static Hand deckHand;
    protected static Hand throwingHand;
    protected static bool deckIsEmpty = false;
    protected static bool instantiatingDeck = false;
    protected static bool growingDeck = false;
    protected static bool testingOutsideVR = false;

    //in the trigger enters and exits, I want to make sure that I'm colliding with the right thing, and also making it so that I can only instantiate a cards if I'm touching the deck of cards.
    public void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterX(other);
    }

    public virtual void OnTriggerEnterX(Collider other)
    {
        //Debug.Log(this.name + " is touching " + other.name);
    }

    public void OnTriggerExit(Collider other)
    {
        OnTriggerExitX(other);
    }

    public virtual void OnTriggerExitX(Collider other)
    {
        //Debug.Log(this.name + " is touching " + other.name);
    }

    //this happens whenever a hand is near this object
    public virtual void HandHoverUpdate(Hand hand) //this applies to either controller
    {
        //Debug.Log("Hand is holding " + hand.AttachedObjects.Count + " objects.");
        if (hand.GetStandardInteractionButtonDown() == true) //on Vive controller, this is the trigger
        {
            hand.AttachObject(gameObject);
            hand.HoverLock(interactableObject);
        }
    }

    //this happens whenever an object is attached to this hand, for whatever reason
    public virtual void OnAttachedToHand(Hand attachedHand)
    {
        GetComponent<Rigidbody>().isKinematic = true; //turn off the physics, we we can hold it
        //attachedHand.HoverLock(interactableObject);
    }

    //this is like update, as long as we're holding something
    public virtual void HandAttachedUpdate(Hand attachedHand)
    {
        if (attachedHand.GetStandardInteractionButton() == false)
        {
            attachedHand.DetachObject(gameObject);
        }
    }

    //this happens whent he object is detached from the hand for whatever reason
    public virtual void OnDetachedFromHand(Hand hand)
    {
        handTouchingDeck = false;
        GetComponent<Rigidbody>().isKinematic = false; //turns on physics
        hand.HoverUnlock(interactableObject);

        //apply forces to it, as if we're throwing it
        GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
    }

}
