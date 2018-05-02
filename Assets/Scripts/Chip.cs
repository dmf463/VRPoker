using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class Chip : InteractionSuperClass {

    public Transform myTarget;  // drag the target here
    public float flyTime;  // elevation angle

    public float flying_start_time, flight_journey_distance;
    public Vector3 flying_start_position;
    public bool lerping;
    public PokerPlayerRedux owner;

    public bool is_flying = false;
    public float rotSpeed;
    public bool isTip = false;
    public bool isLerping = false;

    bool callingPulse = false;
    bool stopPulse = false;
    int pingPongCount = 0;
    float emission = 0;
    float glowSpeed = 1;
    float maxGlow = 2;
    float startTime;

    public Vector3 chipPushStartPos = Vector3.zero;

    public ChipData chipData;

    public bool isAtDestination = false;

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

    const float HEIGHT_THRESHOLD = .05f;

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
    GameObject stickHead;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        stickHead = GameObject.FindGameObjectWithTag("StickHead");
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
        PulseGlow();
        if(Table.gameState == GameState.ShowDown && chipForBet && !callingPulse && !stopPulse)
        {
            StartPulse();
        }
        if(Vector3.Distance(transform.position,GameObject.Find("TipZone").transform.position) > 20)
        {
            transform.position = GameObject.Find("TipZone").transform.position;
            rb.velocity = new Vector3(0, 0, 0);
        }
    }

    void FixedUpdate()
    {
        PushChips();
	}

    void PushChips()
    {
        Vector2 stickPos = new Vector2(stickHead.transform.position.x, stickHead.transform.position.z);
        Vector2 chipPos = new Vector2(transform.position.x, transform.position.z);
        float heightDifference = stickHead.transform.position.y - transform.position.y;
        if (Table.gameState == GameState.ShowDown && Services.Dealer.chipsInPot.Contains(this))
        {
            if ((stickHead.transform.position - transform.position).magnitude < .2f && (stickPos - chipPos).magnitude < .12f)
            {
                if (!pushingChip && Services.ChipManager.chipGroup.Count <= 10)
                {
                    if (chipPushStartPos == Vector3.zero) chipPushStartPos = transform.position;
                    maxGlow = 1;
                    Services.ChipManager.chipGroup.Add(this);
                    pushingChip = true;
                    Services.Dealer.handIsOccupied = true;
                    spotIndex = Services.ChipManager.chipsBeingPushed;
                    Services.ChipManager.chipsBeingPushed += 1;
                    Services.ChipManager.ConsolidateStack(Services.ChipManager.chipGroup);
                }
            }
            else
            {
                if (pushingChip && (stickHead.transform.position.y - transform.position.y) > HEIGHT_THRESHOLD)
                {
                    maxGlow = 2;
                    Services.ChipManager.chipGroup.Clear();
                    chipPushStartPos = Vector3.zero;
                    timesToSplit = 0;
                    pushingChip = false;
                    Services.Dealer.handIsOccupied = false;
                    spotIndex = 0;
                    Services.ChipManager.chipsBeingPushed = 0;
                }
            }
        }
        else if (Table.gameState == GameState.PostHand)
        {
            chipPushStartPos = Vector3.zero;
            Services.ChipManager.chipGroup.Clear();
            timesToSplit = 0;
            handPushingChip = null;
            pushingChip = false;
            Services.Dealer.handIsOccupied = false;
            spotIndex = 0;
            Services.ChipManager.chipsBeingPushed = 0;
            stopPulse = true;
        }
    }
   
    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("hitting " + other.gameObject.name);
        if ((other.gameObject.tag == "Chip" || other.gameObject.tag == "Tip") && other.gameObject.GetComponent<Chip>().chipStack == null)
        {
            isTouchingChip = true;
            incomingChip = other.gameObject.GetComponent<Chip>();
        }
        else if((other.gameObject.tag == "Chip" || other.gameObject.tag == "Tip") && other.gameObject.GetComponent<Chip>().chipStack != null)
        {
            if (other.gameObject.GetComponent<Chip>().chipStack.chips.Count < MAX_CHIPSTACK)
            {
                isTouchingStack = true;
                incomingStack = other.gameObject.GetComponent<Chip>();
            }
        }
        if(other.gameObject.tag == "Floor" && Table.gameState != GameState.Misdeal && chipForBet)
        {
            transform.position = GameObject.Find("TipZone").transform.position;
            rb.velocity = new Vector3(0, 0, 0);
        }
    }

    //when we are no longer colliding, we want to set everything we had previously set to false
    void OnCollisionExit(Collision other)
    {
        if ((other.gameObject.tag == "Chip" || other.gameObject.tag == "Tip") && other.gameObject.GetComponent<Chip>().chipStack == null)
        {
            isTouchingChip = false;
            incomingChip = null;
        }
        else if ((other.gameObject.tag == "Chip" || other.gameObject.tag == "Tip") && other.gameObject.GetComponent<Chip>().chipStack != null)
        {
            isTouchingStack = false;
            isTouchingChip = false;
            incomingChip = null;
        }
    }

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
                    //Services.ChipManager.chipsToDestroy.Add(incomingChip);
                    incomingChip = null;
                    isTouchingChip = false;
                }
                else if(incomingChip != null && incomingChip.gameObject.tag == "Tip")
                {
                    chipStack.AddToStackInHand(incomingChip.chipData);
                    Destroy(incomingChip.gameObject);
                    //Services.ChipManager.chipsToDestroy.Add(incomingChip);
                    incomingChip = null;
                    isTouchingChip = false;
                }
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
                //Services.ChipManager.chipsToDestroy.Add(incomingStack);
                incomingStack = null;
            }
            //NEW BUG
            else if(isTouchingStack && incomingStack.gameObject.tag == "Tip")
            {
                foreach (ChipData chip in incomingStack.chipStack.chips)
                {
                    chipStack.AddToStackInHand(chip);
                    isTouchingStack = false;
                }
                Destroy(incomingStack.gameObject);
                //Services.ChipManager.chipsToDestroy.Add(incomingStack);
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
            if (hand.GetStandardInteractionButtonDown() == true && 
                gameObject.GetComponent<Rigidbody>().isKinematic == false && 
                !pushingChip &&
                !Services.ChipManager.chipsToDestroy.Contains(gameObject)) //on Vive controller, this is the trigger
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
        if (!chipForBet)
        {
            //DAN PUT THE GRABBING CHIPS LINE HERE.
            //YOU CAN ACCESS THE OWNER, BY USING THE "OWNER" VARIABLE ON THIS SCRIPT
            Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(owner.playerName, LineCriteria.ChipsMoved));
            Debug.Log("misdeal here");
            Services.Dealer.TriggerMisdeal();
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
                float chipSpawnOffset = 0.05f;
                //DAN PUT THE THROWING CHIPS HERE!!!
                Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(owner.playerName, LineCriteria.ThrowsChips));
                for (int i = 0; i < chipStack.chips.Count; i++)
                {
                    GameObject newChip = Instantiate(FindChipPrefab(chipStack.chips[i].ChipValue),
                                                     transform.position + Random.insideUnitSphere * chipSpawnOffset,
                                                     Quaternion.identity);
                    if (gameObject.tag == "Tip")
                    {
                        newChip.GetComponent<MeshRenderer>().material = Services.PokerRules.tipMaterial;
                        newChip.gameObject.tag = "Tip";
                    }
                    if (chipForBet) newChip.GetComponent<Chip>().chipForBet = true;
                    Services.Dealer.thrownChips.Add(newChip);
                    Rigidbody rb = newChip.gameObject.GetComponent<Rigidbody>();
                    rb.AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
                    rb.AddTorque(hand.GetTrackedObjectAngularVelocity(), ForceMode.Impulse);
                    rb.AddForce(Random.Range(0, 150), Random.Range(0, 150), Random.Range(0, 150));
                    Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                    transform.Rotate(randomRot * Time.deltaTime);
                }
                Destroy(gameObject);
                //Services.ChipManager.chipsToDestroy.Add(this);
            }
        }
    }

public Vector3 BallisticVel(Transform target, float time)
    {
        //angle, target, time
        target = GameObject.Find("TipZone").transform;
        float startY = transform.position.y;
        float yAcceleration = Physics.gravity.y;
        float targetY = target.position.y;
        float yVel = ((targetY - startY) / time) - (yAcceleration * time / 2);

        float startX = transform.position.x;
        float targetX = target.position.x;
        float xVel = ((targetX - startX) / time);

        float startZ = transform.position.z;
        float targetZ = target.position.z;
        float zVel = ((targetZ - startZ) / time);

        return new Vector3(xVel, yVel, zVel);

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

    public void StartPulse()
    {
        callingPulse = true;
        startTime = Time.time;
        pingPongCount = 0;
        emission = 0;
    }

    public void PulseGlow()
    {
        if (callingPulse)
        {
            Color baseColor = new Vector4(0.8235294f, 0.1574394f, 0.5570934f, 0);
            emission = PingPong(glowSpeed * (startTime - Time.time), 0, maxGlow);
            Color finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
            GetComponent<Renderer>().material.SetColor("_EmissionColor", finalColor);
            if (stopPulse)
            {
                callingPulse = false;
                GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    float PingPong(float time, float minLength, float maxLength)
    {
        return Mathf.PingPong(time, maxLength - minLength) + minLength;
    }

    public void InitializeLerp(Vector3 dest)
    {
        flying_start_time = Time.time;
        flight_journey_distance = Vector3.Distance(transform.position, dest);
        flying_start_position = transform.position;
        lerping = true;
    }

    public IEnumerator LerpChipPos(Vector3 dest, float speed)
    {
        while (lerping)
        {
            float distCovered = (Time.time - flying_start_time) * speed;
            float fracJourney = distCovered / flight_journey_distance;
            //chip has been destroyed
            //FIX ASAP
            // DO IT NOW GOHAN
            transform.position = Vector3.Lerp(flying_start_position, dest, fracJourney);
            yield return null;
        }
    }

    public IEnumerator StopLerp(Vector3 pos)
    {
        while (lerping)
        {
            //chip has been destroyed
            if (transform.position == pos)
            {
                lerping = false;
            }
            else yield return null;
        }
        yield break;
    }

}
