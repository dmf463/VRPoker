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
        stackValue = chips[0].chipValue;
        incrementStackBy = ((chips[0].gameObject.GetComponent<Collider>().bounds.size.z / 88) * -1);
    }


    public void AddToStack(Chip chip)
    {
        Debug.Log("trying to destroy " + chip.gameObject.name);
        int chipValue = chip.chipValue;
        GameObject.Destroy(chip.gameObject);
        GameObject newChip = null;
        if (chipValue == 5)
        {
            newChip = GameObject.Instantiate(Services.PrefabDB.RedChip5, Vector3.zero, Quaternion.identity);
        }
        else if (chipValue == 25)
        {
            newChip = GameObject.Instantiate(Services.PrefabDB.BlueChip25, Vector3.zero, Quaternion.identity);
        }
        else if (chipValue == 50)
        {
            newChip = GameObject.Instantiate(Services.PrefabDB.WhiteChip50, Vector3.zero, Quaternion.identity);
        }
        else if (chipValue == 100)
        {
            newChip = GameObject.Instantiate(Services.PrefabDB.BlackChip100, Vector3.zero, Quaternion.identity);
        }
        GameObject.Destroy(newChip.GetComponent<Rigidbody>());
        newChip.transform.parent = chips[0].transform;
        newChip.transform.localPosition = new Vector3(chips[0].transform.localPosition.x, chips[0].transform.localPosition.y, (chips[chips.Count - 1].transform.localPosition.z + incrementStackBy));
        newChip.transform.rotation = chips[0].transform.rotation;
        stackValue += newChip.GetComponent<Chip>().chipValue;
        chips.Add(newChip.GetComponent<Chip>());
        //Debug.Log("chipStack is worth " + stackValue);

    }

    public void TakeFromStack()
    {
        chips[chips.Count - 1].transform.parent = null;
        chips[chips.Count - 1].gameObject.AddComponent<Rigidbody>();
        stackValue -= chips[chips.Count - 1].chipValue;
        chips[chips.Count - 1].canBeGrabbed = false;
        chips.Remove(chips[chips.Count - 1]);
        chips.TrimExcess();

    }

    public void ClearStack()
    {

    }



}
