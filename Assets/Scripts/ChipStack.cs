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
    public List<ChipData> chips = new List<ChipData>();

    //the value of the stack
    public int stackValue;

    //the float by which we're incrementing the stack and putting chips on top of other chip
    private float incrementStackBy;

    public GameObject parentChip;

    public ChipStack(Chip chip)
    {
        //Debug.Log("chip.ChipData.chipValue = " + chip.chipData.ChipValue);
        chips.Add(chip.chipData);
        parentChip = chip.gameObject;
        chip.inAStack = true;
        stackValue = chip.chipData.ChipValue;
        //incrementStackBy = ((chips[0].gameObject.GetComponent<Collider>().bounds.size.z / 88) * -1);
        incrementStackBy = parentChip.gameObject.transform.localScale.z;
    }

    public void AddToStackInHand(ChipData chip)
    {
            stackValue += chip.ChipValue;
            chips.Add(chip);
            parentChip.transform.localScale = new Vector3(parentChip.transform.localScale.x,
                                                        parentChip.transform.localScale.y,
                                                        parentChip.transform.localScale.z + incrementStackBy);
        }

    }

