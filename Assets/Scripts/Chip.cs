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
    private Chip incomingChip;
    private List<Chip> incomingStack;

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
        if (other.gameObject.tag == "Chip" && other.gameObject.GetComponent<ChipStack>() == null)
        {
            isTouchingChip = true;
            incomingChip = other.gameObject.GetComponent<Chip>();
        }
        else if(other.gameObject.tag == "Chip" && other.gameObject.GetComponent<ChipStack>() != null)
        {
            isTouchingStack = true;
            incomingStack = other.gameObject.GetComponent<ChipStack>().chips;
        }
    }


    public override void HandAttachedUpdate(Hand attachedHand)
    {
        if(isTouchingChip == true)
        {
            GetComponent<ChipStack>().AddToStack(incomingChip);
            isTouchingChip = false;
        }
        if(isTouchingStack == true)
        {
            foreach(Chip chip in incomingStack)
            {
                GetComponent<ChipStack>().AddToStack(chip);
                isTouchingStack = false;
            }
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
