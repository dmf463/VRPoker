using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ChipStack : InteractionSuperClass {

    public List<GameObject> chips = new List<GameObject>();

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //void OnCollisionEnter(Collision other)
    //{
    //    if(other.gameObject.tag == "Chip")
    //    {
    //        AddToStack(other.gameObject.GetComponent<Chip>());
    //    }
    //}

    public void AddToStack(Chip chip)
    {

        //need to make it so that when it adds it to the stack, it adds it in the right position
        //I think I need to rethink this, Chris said I shouldn't be using gameObjects, yet here I am.
        //these things in this stack need to like, NOT be gameObjects.
        //that's what I was trying to avoid in the first place...I think?
        int chipValue = chip.chipValue;
        Destroy(chip.gameObject);
        GameObject newChip = null;

        if(chipValue == 5)
        {
            newChip = Instantiate(Services.PrefabDB.RedChip5, transform.position, Quaternion.identity);
        }
        else if(chipValue == 25)
        {
            newChip = Instantiate(Services.PrefabDB.BlueChip25, transform.position, Quaternion.identity);
        }
        else if (chipValue == 50)
        {
            newChip = Instantiate(Services.PrefabDB.WhiteChip50, transform.position, Quaternion.identity);
        }
        else if (chipValue == 100)
        {
            newChip = Instantiate(Services.PrefabDB.BlackChip100, transform.position, Quaternion.identity);
        }
        newChip.GetComponent<BoxCollider>().enabled = false;
        newChip.GetComponent<Rigidbody>().isKinematic = true;
        newChip.transform.parent = gameObject.transform;
        newChip.GetComponent<Chip>().chipStack = this;
        chips.Add(newChip);


    }

    public void TakeFromStack()
    {

    }

    public void ClearStack()
    {

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
