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
 * 
 * so what I want to do:
 * The Chip at start is a lifeless husk that only contains a reference to its color, if anything?
 * The chipStack is what holds the information. if it's one chip, then it's a chipStack of one.
 * but ONLY once I pick it up. until then, it's nothing but a physics object.
 * So, when I pick up a chip, I add a chipStack component
 * the chipStack says, okay, this chip is this color, so therefore it's this value
 * every time I pick up a subsequent chip, it adds that chip to the stack, destroying the chip object, and instantiating a new one, placing it below the bottom most chip
 * every time I pick up a chip, I also add that value to the chipStack value
 * the topmost chip retains its rigid body and collider info, because that's what I'm picking up and controlling the stack with.
 * each subsequent chip has it's box collider turned off, and it's rigidbody made kinematic, and also made a child of the first chip
 * if I reach a certain velocity threshold, I can destory the chips in the stack, and instantiate them with an offset, creating the effect like a threw the stack
 * I would also remove the chipStack component. 
 *
 * 
 * So Frank says I should just nix chips and abstract them...but will that work with VR? I guess the easiest route would be to abstract them, and then if people want it, add it
 * 
 */

public class Chip : InteractionSuperClass {

    [HideInInspector]
    public ChipColor ChipColor;
    public int chipValue;
    private bool isTouchingChip;
    private Chip incomingChip;

	// Use this for initialization
	void Start () {

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
		
	}

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Chip")
        {
            isTouchingChip = true;
            incomingChip = other.gameObject.GetComponent<Chip>();
        }
    }


    public override void HandAttachedUpdate(Hand attachedHand)
    {
        if(isTouchingChip == true)
        {
            GetComponent<ChipStack>().AddToStack(incomingChip);
            isTouchingChip = false;
        }
        base.HandAttachedUpdate(attachedHand);
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        base.OnDetachedFromHand(hand);
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        transform.rotation = attachedHand.GetAttachmentTransform("CardFaceDown").transform.rotation;
        gameObject.AddComponent<ChipStack>();
        base.OnAttachedToHand(attachedHand);
    }

    public override void OnTriggerEnterX(Collider other)
    {
        base.OnTriggerEnterX(other);
    }

    public override void OnTriggerExitX(Collider other)
    {
        base.OnTriggerExitX(other);
    }

}
