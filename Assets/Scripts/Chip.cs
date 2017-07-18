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
 */

public class Chip : InteractionSuperClass {

    public int chipValue;
    public ChipStack chipStack;

	// Use this for initialization
	void Start () {

        chipStack = null;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Chip")
        {
            if (other.gameObject.transform.parent)
            {
                Hand hand = other.gameObject.transform.parent.gameObject.GetComponent<Hand>();
                Chip otherChip = other.gameObject.GetComponent<Chip>();
                if (chipStack == null && otherChip.chipStack == null)
                {
                    ChipStack newChipStack = Instantiate(Services.PrefabDB.ChipStack, transform.position, Quaternion.identity).GetComponent<ChipStack>();
                    newChipStack.AddToStack(this);
                    newChipStack.AddToStack(otherChip);
                    hand.AttachObject(newChipStack.gameObject);
                }
            }
        }
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
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
