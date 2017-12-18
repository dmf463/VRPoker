using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class Chip : InteractionSuperClass {

    //super important, this is how much the chip costs, we set this in chipConfig and assign it at start
    //public int chipValue;

    public ChipData chipData;

    //is the chip touching another chip?
    private bool isTouchingChip;

    //is the chip touching another stack?
    private bool isTouchingStack;

    //the chip that is going to be added to a stack
    [HideInInspector]
    public Chip incomingChip;

    //the stack that is going to be added to the stack
    [HideInInspector]
    public Chip incomingStack;

    [HideInInspector]
    public List<ChipData> stackToHold;

    //can this chip be grabbed? we want a little time between dropping a chip and picking it up
    //otherwise as soon as it's dropped it gets picked up by the controller
    public bool canBeGrabbed;

    //the coroutine that uses canBeGrabbed.
    private bool regrabCoroutineActive;

    //this is whether this is a chipStack or not
    //usually null, unless it's the first chip that makes the stack
    public ChipStack chipStack;

    //is the chip in a stack?

    public bool inAStack = false;

    //the velocity threshold by which chip stacks come apart
    const float MAGNITUDE_THRESHOLD = 2;

    //the max amount of chips that can go in a chipstack
    const float MAX_CHIPSTACK = 25;

    //the force modifier for when we throw chips
    const float CHIP_FORCE_MODIFIER = 1.5f;

    //is this chip going to be destroyed?
    //this was used before, but I don't think it's necessary now
    public bool markedForDestruction = false;

    //is this a chip for a bet?
    //I don't think this is actually relevant
    //it's set in multiple places, but not USED for anything
    public bool chipForBet = false;
    private bool pushingChip;
    private bool splittingChips;
    [HideInInspector]
    public int spotIndex;
    [HideInInspector]
    public Hand handPushingChip;

    public int stackValue;
    private int timesToSplit;

    Rigidbody rb;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        //set the proper bools
        //and assign the chip its value
        canBeGrabbed = true;
        pushingChip = false;
        regrabCoroutineActive = false;
        switch (GetComponent<MeshFilter>().mesh.name)
        {
            case "RedChip Instance":
                chipData = new ChipData(ChipConfig.RED_CHIP_VALUE);
                break;
            case "BlueChip Instance":
                chipData = new ChipData(ChipConfig.BLUE_CHIP_VALUE);
                break;
            case "WhiteChip Instance":
                chipData = new ChipData(ChipConfig.WHITE_CHIP_VALUE);
                break;
            case "BlackChip Instance":
                chipData = new ChipData(ChipConfig.BLACK_CHIP_VALUE);
                break;
            default:
                break;
        }

    }
	
	// Update is called once per frame
	void Update () {

        if (chipStack != null) stackValue = chipStack.stackValue;
    }

    void FixedUpdate()
    {
		if(!Services.Dealer.OutsideVR){
	        for (int i = 0; i < 2; i++)
	        {
	            Hand hand = i == 0 ? throwingHand : deckHand;
	            //if (chipStack != null && hand != null)
	            if (hand != null)
	            {
	                Vector2 handPos = new Vector2(hand.transform.position.x, hand.transform.position.z);
	                Vector2 chipPos = new Vector2(transform.position.x, transform.position.z);
	                Vector2 otherHandPos = new Vector2(hand.otherHand.transform.position.x, hand.otherHand.transform.position.z);
	                if (/*hand.currentAttachedObject.tag != "Chip" && hand.currentAttachedObject.tag != "PlayingCard" &&*/
	                    hand.controller.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
	                {
	                    if ((hand.transform.position - transform.position).magnitude < .2f && (handPos - chipPos).magnitude < .12f)
	                    {
	                        Vector3 vel = hand.GetTrackedObjectVelocity();
	                        Vector2 vel2D = new Vector2(vel.x, vel.z);
	                        Vector2 touchVect = (chipPos - (handPos));
	                        Vector2 chipDir = touchVect;
	                        float dot = Vector2.Dot(vel2D.normalized, touchVect.normalized);
	                        if (vel2D.magnitude > .2f && dot > .75f) //.6
	                        {
	                            chipDir = vel2D;
	                        }

	                        Vector3 dest = hand.transform.TransformPoint(Services.PokerRules.chipPositionWhenPushing[spotIndex]);
	                        if (!pushingChip && Services.PokerRules.chipGroup.Count <= 10)
	                        {
	                            Services.PokerRules.chipGroup.Add(this);
                                GameObject[] allChips = GameObject.FindGameObjectsWithTag("Chip");
                                foreach(GameObject chip in allChips)
                                {
                                    Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), chip.GetComponent<Collider>(), true);
                                }
	                            handPushingChip = hand;
	                            pushingChip = true;
                                Services.Dealer.handIsOccupied = true;
	                            spotIndex = Services.PokerRules.chipsBeingPushed;
	                            Services.PokerRules.chipsBeingPushed += 1;
                                Services.PokerRules.ConsolidateStack();
	                            //Debug.Log(Services.PokerRules.chipsBeingPushed);
	                        }
	                    }
	                }

	                else
	                {
	                    if (pushingChip && !handPushingChip.controller.GetTouch(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad))
	                    {
	                        Services.PokerRules.chipGroup.Clear();
                            GameObject[] allChips = GameObject.FindGameObjectsWithTag("Chip");
                            foreach (GameObject chip in allChips)
                            {
                                Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), chip.GetComponent<Collider>(), false);
                            }
                            timesToSplit = 0;
	                        handPushingChip = null;
	                        pushingChip = false;
                            Services.Dealer.handIsOccupied = false;
	                        spotIndex = 0;
	                        Services.PokerRules.chipsBeingPushed = 0;
	                    }
	                }
	            }
	        }
	    }
	}

    //on collision, we want to check:
    //a) is the object a chip?
    //b) is that chip in a stack?
    //if it's not in a stack, then we need to set up the chip to make a new chipstack
    //if it IS in a stack, then we want to set it up to be added to the stack it hit
    void OnCollisionEnter(Collision other)
    {
		//if (other.gameObject.tag == "PlayerFace"){
		//	AudioClip hitSound = other.gameObject.GetComponentInParent<PokerPlayerRedux>().cardHitAudio;
		//	AudioSource hitSource = other.gameObject.GetComponentInParent<AudioSource>();
		//	Services.SoundManager.GetSourceAndPlay(hitSource, hitSound);
		//}
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
                incomingStack = other.gameObject.GetComponent<Chip>();
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

        if (chipStack.chips.Count < MAX_CHIPSTACK)
        {
            if (isTouchingChip && incomingChip.canBeGrabbed)
            {
                //Debug.Log("adding " + incomingChip.gameObject.name);
                if(incomingChip != null && incomingChip.chipData.ChipValue == chipData.ChipValue)
                {
                    chipStack.AddToStackInHand(incomingChip.chipData);
                    Destroy(incomingChip.gameObject);
                    incomingChip = null;
                    isTouchingChip = false;
                }
                //we might not need this. what's it for?
                isTouchingChip = false;
            }
            if (isTouchingStack && incomingStack.chipData.ChipValue == chipData.ChipValue)
            {
                foreach (ChipData chip in incomingStack.chipStack.chips)
                {
                    chipStack.AddToStackInHand(chip);
                    isTouchingStack = false;
                }
                Destroy(incomingStack.gameObject);
                incomingStack = null;
            }
        }
        if (chipStack != null)
        {
            CheckPressPosition(attachedHand);
        }
        if (attachedHand.GetStandardInteractionButton() == false)
        {
            for (int i = 0; i < attachedHand.AttachedObjects.Count; i++)
            {
                if(attachedHand.AttachedObjects[i].attachedObject.GetComponentInChildren<Chip>().chipStack != null)
                {
                    attachedHand.DetachObject(attachedHand.AttachedObjects[i].attachedObject);
                }
            }
        }
    }

    //so this when a hand is hovering over a chip
    public override void HandHoverUpdate(Hand hand)
    {
        //if the chip HAS a rigidBody and the controller's trigger is pulled
        //then we want to attach the chip to the hand
        if (gameObject.GetComponent<Rigidbody>() != null)
        {
            if (hand.GetStandardInteractionButtonDown() == true && gameObject.GetComponent<Rigidbody>().isKinematic == false && !pushingChip) //on Vive controller, this is the trigger
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

    IEnumerator CheckVelocityForChipThrowing(float time, Hand hand)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log("rb.velocity.magnitude = " + rb.velocity.magnitude);
        if (GetComponent<Rigidbody>().velocity.magnitude > MAGNITUDE_THRESHOLD)
        {
            if (chipStack.chips.Count != 0)
            {
                float chipSpawnOffset = 0.07f;
                for (int i = 0; i < chipStack.chips.Count; i++)
                {
                    GameObject newChip = Instantiate(FindChipPrefab(chipStack.chips[i].ChipValue),
                                                     transform.position + Random.insideUnitSphere * chipSpawnOffset,
                                                     Quaternion.identity);
                    Rigidbody rb = newChip.gameObject.GetComponent<Rigidbody>();
                    rb.AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
                    rb.AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
                    rb.AddForce(Random.Range(0, 150), Random.Range(0, 150), Random.Range(0, 150));
                    Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                    transform.Rotate(randomRot * Time.deltaTime);
                }
                Destroy(gameObject);
            }
        }
    }
    

    //this is just me ease-of-life function for findining the correct prefab
    public GameObject FindChipPrefab(int chipValue)
    {
        GameObject chipPrefab = null;
        switch (chipValue)
        {
            case ChipConfig.RED_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.RedChip;
                break;
            case ChipConfig.BLUE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlueChip;
                break;
            case ChipConfig.WHITE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.WhiteChip;
                break;
            case ChipConfig.BLACK_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlackChip;
                break;
            default:
                break;
        }
        return chipPrefab;
    }

}
