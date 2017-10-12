using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem; //we need this to interact with objects

public class InteractionSuperClass : MonoBehaviour {

    //insert cardDeck, or other interactable that you need hoverlocked in inspector
    //the interactable class allows the object to have certain functions called on it
    //we need the cardDeck as an interactable in order to make sure that we can't grab it more than once
    public Interactable interactableObject;
    
    //this is true if we're holding a card 
    protected bool handIsHoldingCard;

    //this is true if we're touching the deck, WHILE holding the deck
    protected bool handTouchingDeck = false;
    
    //this is the multiplier we use for when we're throwing things
    protected const float FORCE_MULTIPLIER = 1.80f;

    //these are variables we set so we know which hand is holding the card and which is holding the deck
    protected static Hand deckHand;
    protected static Hand throwingHand;

    //this is only true if the deck has been destroyed or if there are no cards in the deck
    protected static bool deckIsEmpty = false;

    //VARIABLE FOR CHECKING SWIPE
    protected bool trackingSwipe;
    protected bool checkSwipe;
    protected Vector2 startPosition;
    protected Vector2 endPosition;
    protected float swipeStartTime;
    // To recognize as swipe user should at lease swipe for this many pixels
    protected const float MIN_SWIPE_DIST = .01f;
    // To recognize as a swipe the velocity of the swipe
    // should be at least mMinVelocity
    // Reduce or increase to control the swipe speed
    protected const float MIN_VELOCITY = 0.25f;
    protected readonly Vector2 xAxis = new Vector2(1, 0);
    protected readonly Vector2 yAxis = new Vector2(0, 1);
    // The angle range for detecting swipe
    protected const float angleRange = 30;


    //see? these are stupid
    //a lot of these are dumb and useless imo
    //but I AM using them
    //basically the only way to make collision somethat that can be extended is to have the normal version
    //and then have the X version, which is what is actually extended, and then sent to the real OnTrigger or OnCollision
    public void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterX(other);
    }

    public virtual void OnTriggerEnterX(Collider other)
    {

    }

    public void OnTriggerExit(Collider other)
    {
        OnTriggerExitX(other);
    }

    public virtual void OnTriggerExitX(Collider other)
    {

    }

    //this happens whenever a hand is near this object
    //this is literally an update function
    //it runs EVERY frame that the hand is hovering over something
    public virtual void HandHoverUpdate(Hand hand) //this applies to either controller
    {
        //Debug.Log("Hand is holding " + hand.AttachedObjects.Count + " objects.");
        if(gameObject.GetComponent<Rigidbody>() != null)
        {
            //on Vive controller, "StandardInteractionButton" is the trigger
            //so when we're hovering and we click the trigger, we attach the gameObject
            //and we hoverlock the interactable object
            //which is only ever the cardDeck
            if (hand.GetStandardInteractionButtonDown() == true && gameObject.GetComponent<Rigidbody>().isKinematic == false) 
            {
                hand.AttachObject(gameObject);
                hand.HoverLock(interactableObject);
            }
        }

    }

    //this happens whenever an object is attached to this hand, for whatever reason
    //at the moment of attachment, this gets called
    //so basically, when we attach something to our hand we want to make it kinematic so that it moves as if it's in our hand
    public virtual void OnAttachedToHand(Hand attachedHand)
    {
        if(GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true; //turn off the physics, we we can hold it
        }
    }

    //this is like update, as long as we're holding something
    //so if we are holding something and we release the trigger
    //it detaches from our hand
    public virtual void HandAttachedUpdate(Hand attachedHand)
    {
        if (attachedHand.GetStandardInteractionButton() == false)
        {
            attachedHand.DetachObject(gameObject);
        }
    }

    //this happens whent he object is detached from the hand for whatever reason
    //and when we detatch the object, we aren't touching the deck
    //the object is kinematic again
    //and we add whatever force to it as we had when we released it
    public virtual void OnDetachedFromHand(Hand hand)
    {
        handTouchingDeck = false;
        GetComponent<Rigidbody>().isKinematic = false; //turns on physics
        hand.HoverUnlock(interactableObject);

        //apply forces to it, as if we're throwing it
        GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
        GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
    }

    //this checks the position of the finger when pressed, allowing us to know whether it was pressed left, right, up, or down
    public virtual void CheckPressPosition(Hand hand)
    {
        var device = hand.GetComponent<Hand>().controller;
        if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y < 0)
        {
            OnPressBottom();
        }
    }

    //same as above but with checking swiping
    //I got this off the internet
    //so I don't understand all the trigonometry in it
    public virtual void CheckSwipeDirection()
    {
        Vector2 touch = throwingHand.controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
        var device = throwingHand.GetComponent<Hand>().controller;
        if (device.GetTouchDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            //Debug.Log("touching Trackpad");
            trackingSwipe = true;
            startPosition = new Vector2(touch.x, touch.y);
            swipeStartTime = Time.time;
        }
        else if (device.GetTouchUp(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            //Debug.Log("not touching trackpad");
            trackingSwipe = false;
            trackingSwipe = true;
            checkSwipe = true;
            //Debug.Log("Tracking Finish");
        }
        else if (trackingSwipe)
        {
            endPosition = new Vector2(touch.x, touch.y);
        }
        if (checkSwipe)
        {
            checkSwipe = false;
            float deltaTime = Time.time - swipeStartTime;
            Vector2 swipeVector = endPosition - startPosition;
            float velocity = swipeVector.magnitude / deltaTime;
            //Debug.Log("velocity is " + velocity);
            if (velocity > MIN_VELOCITY && swipeVector.magnitude > MIN_SWIPE_DIST)
            {
                // if the swipe has enough velocity and enough distance
                swipeVector.Normalize();
                float angleOfSwipe = Vector2.Dot(swipeVector, xAxis);
                angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                // Detect left and right swipe
                if (angleOfSwipe < angleRange)
                {
                    OnSwipeRight();
                }
                else if ((180f - angleOfSwipe) < angleRange)
                {
                    OnSwipeLeft();
                }
                else
                {
                    // Detect top and bottom swipe
                    angleOfSwipe = Vector2.Dot(swipeVector, yAxis);
                    angleOfSwipe = Mathf.Acos(angleOfSwipe) * Mathf.Rad2Deg;
                    if (angleOfSwipe < angleRange)
                    {
                        OnSwipeTop();
                    }
                    else if ((180f - angleOfSwipe) < angleRange)
                    {
                        OnSwipeBottom();
                    }
                    else
                    {
                        //messageIndex = 0;
                    }
                }
            }
        }
    }


    //the functions below are basically what we can call when we swipe or press in a certain direction
    public virtual void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
    }

    public virtual void OnSwipeRight()
    {
        Debug.Log("Swipe right");
    }

    public virtual void OnSwipeTop()
    {
        Debug.Log("Swipe Top");
    }

    public virtual void OnSwipeBottom()
    {
        Debug.Log("Swipe Bottom");
    }

    public virtual void OnPressBottom()
    {
        Debug.Log("Press Bottom");
    }

}
