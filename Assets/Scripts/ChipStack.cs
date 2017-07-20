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
        //Vector3 bottomPos = chips[chips.Count - 1].transform.position;
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
        //Chip oldParent = chips[chips.Count - 1];
        //Hand hand = oldParent.GetComponentInParent<Hand>();
        //hand.DetachObject(oldParent.gameObject);
        //hand.AttachObject(newChip);
        //for (int i = 0; i < chips.Count; i++)
        //{
        //    Chip chipInStack = chips[i];
        //    chipInStack.transform.parent = newChip.transform;
        //    chipInStack.transform.localPosition = new Vector3(0, 0, (chips.Count - i) * incrementStackBy);
        //}
        GameObject.Destroy(newChip.GetComponent<Rigidbody>());
        //newChip.GetComponent<Rigidbody>().isKinematic = true;
        newChip.transform.parent = chips[0].transform;
        newChip.transform.localPosition = new Vector3(chips[0].transform.localPosition.x, chips[0].transform.localPosition.y, (chips[chips.Count - 1].transform.localPosition.z + incrementStackBy));
        newChip.transform.rotation = chips[0].transform.rotation;
        stackValue += newChip.GetComponent<Chip>().chipValue;


        //oldParent.GetComponent<BoxCollider>().enabled = false;
        //oldParent.GetComponent<Rigidbody>().isKinematic = true;
        //oldParent.chipStack = null;
        //newChip.GetComponent<Chip>().chipStack = this;
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

        ////Debug.Log("chips.count = " + chips.Count);

        //Chip oldParent = chips[chips.Count - 1];
        //Hand hand = oldParent.GetComponentInParent<Hand>();
        //Chip newParent = chips[chips.Count - 2];
        //hand.DetachObject(oldParent.gameObject);
        //newParent.GetComponent<BoxCollider>().enabled = true;
        //newParent.GetComponent<Rigidbody>().isKinematic = true;
        //hand.AttachObject(newParent.gameObject);
        //chips.Remove(oldParent);
        //chips.TrimExcess();
        //for (int i = 0; i < chips.Count - 1; i++)
        //{
        //    Chip chipInStack = chips[i];
        //    chipInStack.transform.parent = newParent.transform;
        //    chipInStack.transform.localPosition = new Vector3(0, 0, (chips.Count - i) * incrementStackBy);
        //}
        //stackValue -= oldParent.chipValue;

        //oldParent.GetComponent<BoxCollider>().enabled = true;
        //oldParent.GetComponent<Rigidbody>().isKinematic = false;
        //oldParent.chipStack = null;
        //newParent.chipStack = this;
        //oldParent.transform.parent = null;
        //oldParent.canBeGrabbed = false;
    }

    public void ClearStack()
    {

    }



}
