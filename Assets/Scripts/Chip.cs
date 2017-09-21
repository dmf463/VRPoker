using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class Chip : InteractionSuperClass {

    //this is the chip color, and I don't think we ever actually use it?
    //should probably be deleted
    [HideInInspector]
    public ChipColor ChipColor;

    //super important, this is how much the chip costs, we set this in chipConfig and assign it at start
    public int chipValue;

    //is the chip touching another chip?
    private bool isTouchingChip;

    //is the chip touching another stack?
    private bool isTouchingStack;

    //the chip that is going to be added to a stack
    [HideInInspector]
    public Chip incomingChip;

    //the stack that is going to be added to the stack
    [HideInInspector]
    public List<Chip> incomingStack;

    //can this chip be grabbed? we want a little time between dropping a chip and picking it up
    //otherwise as soon as it's dropped it gets picked up by the controller
    public bool canBeGrabbed;

    //the coroutine that uses canBeGrabbed.
    private bool regrabCoroutineActive;

    //this is whether this is a chipStack or not
    //usually null, unless it's the first chip that makes the stack
    public ChipStack chipStack;

    //is the chip in a stack?
    [HideInInspector]
    public bool inAStack = false;

    //the velocity threshold by which chip stacks come apart
    const float MAGNITUDE_THRESHOLD = 1;

    //the max amount of chips that can go in a chipstack
    const float MAX_CHIPSTACK = 20;

    //the force modifier for when we throw chips
    const float CHIP_FORCE_MODIFIER = 1.5f;

    //is this chip going to be destroyed?
    //this was used before, but I don't think it's necessary now
    public bool markedForDestruction = false;

    //is this a chip for a bet?
    //I don't think this is actually relevant
    //it's set in multiple places, but not USED for anything
    public bool chipForBet;

    // Use this for initialization
    void Start () {

        //set the proper bools
        //and assign the chip its value
        canBeGrabbed = true;
        regrabCoroutineActive = false;
        switch (GetComponent<MeshFilter>().mesh.name)
        {
            case "RedChip Instance":
                ChipColor = ChipColor.Red;
                chipValue = ChipConfig.RED_CHIP_VALUE;
                break;
            case "BlueChip Instance":
                ChipColor = ChipColor.Blue;
                chipValue = ChipConfig.BLUE_CHIP_VALUE;
                break;
            case "WhiteChip Instance":
                ChipColor = ChipColor.White;
                chipValue = ChipConfig.WHITE_CHIP_VALUE;
                break;
            case "BlackChip Instance":
                ChipColor = ChipColor.Black;
                chipValue = ChipConfig.BLACK_CHIP_VALUE;
                break;
            default:
                break;
        }

    }
	
	// Update is called once per frame
	void Update () {

        //if the chip can't be grabbed, start the coroutine that sets it back to being grabbable
        if (!canBeGrabbed)
        {
            if (!regrabCoroutineActive)
            {
                regrabCoroutineActive = true;
                StartCoroutine(ReadyToBeGrabbed(1.5f));
            }
        }
    }

    //we're making a function for Destroy Chip, rather than calling Destroy on the gameObject at a given moment
    //this is happening because there are certain situations that apply to certain chips in certain times when destroyed
    //by having a function to destroy the chip, we can make sure we're never accidentally forgetting to write some line of code
    public void DestroyChip()
    {
        markedForDestruction = true;
        if (inAStack)
        {
            Chip[] groupedChips = GetComponentsInParent<Chip>();
            for (int i = 0; i < groupedChips.Length; i++)
            {
                if(groupedChips[i] != this)
                {
                    groupedChips[i].chipStack.chips.Remove(this);
                }
            }
        }
        Destroy(gameObject);
    }

    //the chip can be grabbed again soon
    IEnumerator ReadyToBeGrabbed(float time)
    {
        yield return new WaitForSeconds(time);
        canBeGrabbed = true;
        regrabCoroutineActive = false;
        //Debug.Log("coroutine finished at time " + Time.time);
    }

    //on collision, we want to check:
    //a) is the object a chip?
    //b) is that chip in a stack?
    //if it's not in a stack, then we need to set up the chip to make a new chipstack
    //if it IS in a stack, then we want to set it up to be added to the stack it hit
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

    //when we are no longer colliding, we want to set everything we had previously set to false
    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag == "Chip" && other.gameObject.GetComponent<Chip>().chipStack == null)
        {
            isTouchingChip = false;
            incomingChip = null;
        }
        else if (other.gameObject.tag == "Chip" && other.gameObject.GetComponent<Chip>().chipStack != null)
        {
            isTouchingStack = false;
            isTouchingChip = false;
            incomingChip = null;
        }
    }

    //so when the chip is in your hand, literally as soon as you pickup a chip, it becomes a chipstack of 1
    //first, we don't want to do ANYTHING to the chip in your hand if you're holding the max chipstack size
    //if we're not then we're basically saying:
        //if we're touching a chip, and the chip can be grabbed
        //and that chip has been set as an incoming chip
        //then we add that chip to the chipstack in your hand
        //if we're touching a stack with the stack in hand, we add those chips to your stack
    //finally, whenever we're holding a chip, we want to be checking the "press position" of the controller holding the chip
    //this way we know whether we're calling the dropChip function or not
    public override void HandAttachedUpdate(Hand attachedHand)
    {
        if(chipStack.chips.Count < MAX_CHIPSTACK)
        {
            if (isTouchingChip && incomingChip.canBeGrabbed)
            {
                //Debug.Log("adding " + incomingChip.gameObject.name);
                if(incomingChip != null)
                {
                    chipStack.AddToStackInHand(incomingChip);
                    incomingChip = null;
                    isTouchingChip = false;
                }
                //we might not need this. what's it for?
                isTouchingChip = false;
            }
            if (isTouchingStack)
            {
                foreach (Chip chip in incomingStack)
                {
                    chipStack.AddToStackInHand(chip);
                    isTouchingStack = false;
                }
                incomingStack = null;
            }
        }
        if (chipStack != null)
        {
            CheckPressPosition(attachedHand);
        }
        base.HandAttachedUpdate(attachedHand);
    }

    //so this when a hand is hovering over a chip
    public override void HandHoverUpdate(Hand hand)
    {
        //if the chip HAS a rigidBody and the controller's trigger is pulled
        //then we want to attach the chip to the hand
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            if (hand.GetStandardInteractionButtonDown() == true && gameObject.GetComponent<Rigidbody>().isKinematic == false) //on Vive controller, this is the trigger
            {
                hand.AttachObject(gameObject);
                hand.HoverLock(interactableObject);
            }
        }
        //if we're in a stack, then we want to grab the parent object, because that controls the chipstack
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

    //when we detach from hand, if there is no rigidBody, we want to give it one
    //if it's the only chip left in the chipStack at the time of detachment
    //then it isn't in a stack anymore
    //if the chipstack still has chips in it when we detach the stack
    //then we want to check the velocity to see if we should make the chips go flying
    public override void OnDetachedFromHand(Hand hand)
    {
        if(GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }
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

    //when we attach a chip to the hand, we want to set the rotation so it looks good
    //but we also want to give it a chipStack
    //essentially ANYTIME you're holding a chip, you're holding a chipStack of 1.
    //you can then ADD to that chipStack
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

    //if we're holding a chip and we press on the bottom
    //then it drops a chip
    public override void OnPressBottom()
    {
        if(chipStack != null)
        {
            if (chipStack.chips.Count > 1) chipStack.TakeFromStackInHand();
        }
        base.OnPressBottom();
    }

    //if the velocity of the chipStack is greater than the threshold
    //then set all the chips back to their original state and destroy the chipStack
    //this allows them to go flying, it's nice, I like
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
                chip.gameObject.GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity() * CHIP_FORCE_MODIFIER, ForceMode.Impulse);
                chip.gameObject.GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
                if (chip.chipStack != null)
                {
                    chip.chipStack = null;
                }
            }
        }
    }

}
