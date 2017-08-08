using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

/*
 * okay, so in terms of physics I want to be able to
 * a) stack chips (in acceptable heights, maybe like stacks of 20 max, or else they'll fall when I move them)
 *    - be able to move those stacks around (unless they're over 20) as a single unit
 *    - use a fixed joint maybe?
 * b) shovel chips around
 *    -so if they're NOT in a stack, I can push them towards the player
 *    -basically give the player the option, if they want to stack them, fine. if not, also fine.
 *    -could probably use the same basic logic for shoveling cards in shuffle mode
 * c) from a stack, be able to grab a single chip, so that I can make change, or take a tip.
 */

public class Chip : InteractionSuperClass {

    [HideInInspector]
    public ChipColor ChipColor;
    public int chipValue;
    private bool isTouchingChip;
    private bool isTouchingStack;
    [HideInInspector]
    public Chip incomingChip;
    [HideInInspector]
    public List<Chip> incomingStack;
    public bool canBeGrabbed;
    private bool regrabCoroutineActive;
    public ChipStack chipStack;
    [HideInInspector]
    public bool inAStack;
    const float MAGNITUDE_THRESHOLD = 1;
    const float MAX_CHIPSTACK = 20;

    // Use this for initialization
    void Start () {

        canBeGrabbed = true;
        regrabCoroutineActive = false;
        switch (GetComponent<MeshFilter>().mesh.name)
        {
            case "RedChip Instance":
                ChipColor = ChipColor.Red;
                chipValue = 5;
                break;
            case "BlueChip Instance":
                ChipColor = ChipColor.Blue;
                chipValue = 25;
                break;
            case "WhiteChip Instance":
                ChipColor = ChipColor.White;
                chipValue = 50;
                break;
            case "BlackChip Instance":
                ChipColor = ChipColor.Black;
                chipValue = 100;
                break;
            default:
                break;
        }

    }
	
	// Update is called once per frame
	void Update () {

        if (!canBeGrabbed)
        {
            if (!regrabCoroutineActive)
            {
                regrabCoroutineActive = true;
                StartCoroutine(ReadyToBeGrabbed(1.5f));
            }
        }

    }

    IEnumerator ReadyToBeGrabbed(float time)
    {
        yield return new WaitForSeconds(time);
        canBeGrabbed = true;
        regrabCoroutineActive = false;
        //Debug.Log("coroutine finished at time " + Time.time);
    }


    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("hitting " + other.gameObject.name);
        if (other.gameObject.tag == "Chip" && other.gameObject.GetComponent<Chip>().chipStack == null)
        {
            isTouchingChip = true;
            incomingChip = other.gameObject.GetComponent<Chip>();
        }
        else if(other.gameObject.tag == "Chip" && other.gameObject.GetComponent<Chip>().chipStack != null)
        {
            if (other.gameObject.GetComponent<Chip>().chipStack.chips.Count < MAX_CHIPSTACK)
            {
                isTouchingStack = true;
                incomingStack = other.gameObject.GetComponent<Chip>().chipStack.chips;
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Chip" && other.gameObject.GetComponent<Chip>().chipStack == null)
        {
            isTouchingChip = false;
        }
        else if (other.gameObject.tag == "Chip" && other.gameObject.GetComponent<Chip>().chipStack != null)
        {
            isTouchingStack = false;
            isTouchingChip = false;
        }
    }


    public override void HandAttachedUpdate(Hand attachedHand)
    {
        if(chipStack.chips.Count < MAX_CHIPSTACK)
        {
            if (isTouchingChip && incomingChip.canBeGrabbed)
            {
                //Debug.Log("adding " + incomingChip.gameObject.name);
                chipStack.AddToStackInHand(incomingChip);
                isTouchingChip = false;
            }
            if (isTouchingStack)
            {
                foreach (Chip chip in incomingStack)
                {
                    chipStack.AddToStackInHand(chip);
                    isTouchingStack = false;
                }
            }
        }
        if (chipStack != null)
        {
            CheckPressPosition(attachedHand);
        }
        base.HandAttachedUpdate(attachedHand);
    }

    public override void HandHoverUpdate(Hand hand)
    {
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            if (hand.GetStandardInteractionButtonDown() == true && gameObject.GetComponent<Rigidbody>().isKinematic == false) //on Vive controller, this is the trigger
            {
                hand.AttachObject(gameObject);
                hand.HoverLock(interactableObject);
            }
        }
        else
        {
            if (inAStack == true)
            {
                GameObject chipToGrab = gameObject.transform.parent.gameObject;
                if (hand.GetStandardInteractionButtonDown() == true) //on Vive controller, this is the trigger
                {
                    hand.AttachObject(chipToGrab);
                    hand.HoverLock(interactableObject);
                }

            }
        }
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        if(chipStack != null && chipStack.chips.Count == 1)
        {
            chipStack = null;
            inAStack = false;
        }
        if(chipStack != null)
        {
            StartCoroutine(CheckVelocityForChipThrowing(.025f, hand));
        }
        GetComponent<Rigidbody>().isKinematic = false; //turns on physics
        hand.HoverUnlock(interactableObject);

        //apply forces to it, as if we're throwing it
        GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
        GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        transform.rotation = attachedHand.GetAttachmentTransform("CardFaceDown").transform.rotation;
        if(chipStack == null)
        {
            chipStack = new ChipStack(this);
            inAStack = true;
        }
        base.OnAttachedToHand(attachedHand);
    }

    public override void OnPressBottom()
    {
        if(chipStack != null)
        {
            if (chipStack.chips.Count > 1) chipStack.TakeFromStackInHand();
        }
        base.OnPressBottom();
    }

    IEnumerator CheckVelocityForChipThrowing(float time, Hand hand)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log("rb.velocity.magnitude = " + rb.velocity.magnitude);
        if (GetComponent<Rigidbody>().velocity.magnitude > MAGNITUDE_THRESHOLD)
        {
            foreach(Chip chip in chipStack.chips)
            {
                chip.gameObject.AddComponent<Rigidbody>();
                chip.gameObject.transform.parent = null;
                chip.inAStack = false;
                chip.gameObject.GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
                chip.gameObject.GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
                if (chip.chipStack != null)
                {
                    chip.chipStack = null;
                }
            }
        }
    }

}
