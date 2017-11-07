using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

//so this is the chipStack script
//pretty much anytime a chip is in a chipstack, the parent chip has a chipStack assigned to it
//this way, we can keep track of every chip in a chipstack and the value of the chips
//this also allows us to handle chips AS stacks

public class ChipStack {

    //the list of chips in the stack
    public List<Chip> chips = new List<Chip>();

    //the value of the stack
    public int stackValue;

    //the float by which we're incrementing the stack and putting chips on top of other chip
    private float incrementStackBy;

    //a public construct for when we create a new chipStack
    //we add the initial chip
    //we say that chip is in a stack
    //we make the chipValue equal to that stack
    //and we set the increment stack float
    public ChipStack(Chip chip)
    {
        chips.Add(chip);
        chip.inAStack = true;
        stackValue = chips[0].chipValue;
        //incrementStackBy = ((chips[0].gameObject.GetComponent<Collider>().bounds.size.z / 88) * -1);
        incrementStackBy = chips[0].gameObject.transform.localScale.z;
    }

    //so when you're holding a chip
    //and you hit another chip, this gets called
    //if the chip is not null
    //then we set the chip as the incomingChip
    //destroy the rigid body (so that they don't collide)
    //set the parent on the chip that holds the chipstack
    //and then set its transform so that it's all in the right place
    //then we set the boos we need to say the incoming chip is in a stack and NOT a chip to bet with
    //we increase the stackValue
    //and then add the chip to the list
    public void AddToStackInHand(Chip chip)
    {
        //Debug.Log("chipStack has " + chips.Count + "  and stackValue ==  " + stackValue + " and incrementStackBy =  " + incrementStackInHandBy);
        //Debug.Log("trying to destroy " + chip.gameObject.name);
        Debug.Log("adding a " + chip.chipValue + " chip back to the stack");
        if (chip.gameObject != null)
        {
            if(chip.chipValue == chips[0].chipValue)
            {
                GameObject incomingChip = chip.FindChipPrefab(chip.chipValue);
                Chip newChip = incomingChip.GetComponent<Chip>();
                GameObject.Destroy(incomingChip.gameObject);
                newChip.GetComponent<Chip>().inAStack = true;
                newChip.GetComponent<Chip>().chipForBet = true;
                stackValue += newChip.GetComponent<Chip>().chipValue;
                chips.Add(newChip.GetComponent<Chip>());
                Debug.Log("incomingChip is a " + newChip.chipValue + " dollar chip");
                GameObject mainChip = chips[0].gameObject;
                mainChip.transform.localScale = new Vector3(mainChip.transform.localScale.x,
                                                            mainChip.transform.localScale.y,
                                                            mainChip.transform.localScale.z + incrementStackBy);
                Debug.Log("chipStack has " + chips.Count + "chips");
            }
            //else
            //{
            //    GameObject incomingChip = chip.gameObject;
            //    float addChipSize = ((chips[0].gameObject.GetComponent<Collider>().bounds.size.z / (88 * chips.Count)) * -1);
            //    //pretty sure these two lines are the source of some issues FOR SOME REASON
            //    GameObject.Destroy(incomingChip.GetComponent<Rigidbody>());
            //    incomingChip.transform.parent = chips[0].transform;
            //    incomingChip.transform.localPosition = new Vector3(chips[0].transform.localPosition.x, chips[0].transform.localPosition.y, (chips[chips.Count - 1].transform.localPosition.z + addChipSize));
            //    incomingChip.transform.rotation = chips[0].transform.rotation;
            //    incomingChip.GetComponent<Chip>().inAStack = true;
            //    incomingChip.GetComponent<Chip>().chipForBet = true;
            //    stackValue += incomingChip.GetComponent<Chip>().chipValue;
            //    chips.Add(incomingChip.GetComponent<Chip>());
            //    Debug.Log("chipStack is worth " + stackValue);
            //}
        }

    }

    //so this is when we want to drop a chip
    //we basically reset the bools, remove it from the stack
    //and by setting its parent to null, it drops from the stack
    //public void TakeFromStackInHand()
    //{
    //    chips[chips.Count - 1].transform.parent = null;
    //    chips[chips.Count - 1].gameObject.AddComponent<Rigidbody>();
    //    stackValue -= chips[chips.Count - 1].chipValue;
    //    chips[chips.Count - 1].canBeGrabbed = false;
    //    chips[chips.Count - 1].inAStack = false;
    //    chips.Remove(chips[chips.Count - 1]);
    //}


}
