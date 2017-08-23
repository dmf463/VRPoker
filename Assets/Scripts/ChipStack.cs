using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ChipStack {

    public List<Chip> chips = new List<Chip>();
    public int stackValue;
    private float incrementStackBy;

    public ChipStack(Chip chip)
    {
        chips.Add(chip);
        chip.inAStack = true;
        stackValue = chips[0].chipValue;
        incrementStackBy = ((chips[0].gameObject.GetComponent<Collider>().bounds.size.z / 88) * -1);
    }


    public void AddToStackInHand(Chip chip)
    {
        //Debug.Log("chipStack has " + chips.Count + "  and stackValue ==  " + stackValue + " and incrementStackBy =  " + incrementStackInHandBy);
        //Debug.Log("trying to destroy " + chip.gameObject.name);
        if (chip.gameObject != null)
        {
            GameObject incomingChip = chip.gameObject;
            GameObject.Destroy(incomingChip.GetComponent<Rigidbody>());
            incomingChip.transform.parent = chips[0].transform;
            incomingChip.transform.localPosition = new Vector3(chips[0].transform.localPosition.x, chips[0].transform.localPosition.y, (chips[chips.Count - 1].transform.localPosition.z + incrementStackBy));
            incomingChip.transform.rotation = chips[0].transform.rotation;
            incomingChip.GetComponent<Chip>().inAStack = true;
            stackValue += incomingChip.GetComponent<Chip>().chipValue;
            chips.Add(incomingChip.GetComponent<Chip>());
            //Debug.Log("chipStack is worth " + stackValue);   
        }

    }

    public void TakeFromStackInHand()
    {
        chips[chips.Count - 1].transform.parent = null;
        chips[chips.Count - 1].gameObject.AddComponent<Rigidbody>();
        stackValue -= chips[chips.Count - 1].chipValue;
        chips[chips.Count - 1].canBeGrabbed = false;
        chips[chips.Count - 1].inAStack = false;
        chips.Remove(chips[chips.Count - 1]);
    }


}
