using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem; //we need this to interact with objects

public class InteractionSuperClass : MonoBehaviour {

    public Interactable interactableObject; //insert cardDeck, or other interactable that you need hoverlocked in inspector
    protected bool handIsHoldingCard;
    protected bool handTouchingDeck = false;
    protected const float FORCE_MULTIPLIER = 1.80f;
    protected static Hand deckHand;
    protected static Hand throwingHand;
    protected static bool deckIsEmpty = false;

    //VARIABLE FOR CHECKING SWIPE
    protected bool trackingSwipe;
    protected bool checkSwipe;
    protected Vector2 startPosition;
    protected Vector2 endPosition;
    protected float swipeStartTime;
    // To recognize as swipe user should at lease swipe for this many pixels
    protected const float MIN_SWIPE_DIST = .02f;
    // To recognize as a swipe the velocity of the swipe
    // should be at least mMinVelocity
    // Reduce or increase to control the swipe speed
    protected const float MIN_VELOCITY = 3f;
    protected readonly Vector2 xAxis = new Vector2(1, 0);
    protected readonly Vector2 yAxis = new Vector2(0, 1);
    // The angle range for detecting swipe
    protected const float angleRange = 30;

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
    public virtual void HandHoverUpdate(Hand hand) //this applies to either controller
    {
        //Debug.Log("Hand is holding " + hand.AttachedObjects.Count + " objects.");
        if(gameObject.GetComponent<Rigidbody>() != null)
        {
            if (hand.GetStandardInteractionButtonDown() == true && gameObject.GetComponent<Rigidbody>().isKinematic == false) //on Vive controller, this is the trigger
            {
                hand.AttachObject(gameObject);
                hand.HoverLock(interactableObject);
            }
        }

    }

    //this happens whenever an object is attached to this hand, for whatever reason
    public virtual void OnAttachedToHand(Hand attachedHand)
    {
        if(GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().isKinematic = true; //turn off the physics, we we can hold it
        }
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

    public virtual void CheckPressPosition(Hand hand)
    {
        var device = hand.GetComponent<Hand>().controller;
        if (device.GetPressDown(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad) && device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad).y < 0)
        {
            OnPressBottom();
        }
    }

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
