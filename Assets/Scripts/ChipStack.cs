using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ChipStack {

    public List<ChipData> chips = new List<ChipData>();

    public int stackValue;

    private float incrementStackBy;

    public GameObject parentChip;

    public ChipStack(Chip chip)
    {
        //Debug.Log("chip.ChipData.chipValue = " + chip.chipData.ChipValue);
        chips.Add(chip.chipData);
        parentChip = chip.gameObject;
        chip.inAStack = true;
        stackValue = chip.chipData.ChipValue;
        incrementStackBy = parentChip.gameObject.transform.localScale.z;
    }

    public void AddToStackInHand(ChipData chip)
    {
            stackValue += chip.ChipValue;
            chips.Add(chip);
        //trying to change the chip?????
            parentChip.transform.localScale = new Vector3(parentChip.transform.localScale.x,
                                                        parentChip.transform.localScale.y,
                                                        parentChip.transform.localScale.z + incrementStackBy);
        }

    }

