using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ChipStick : InteractionSuperClass
{
    Vector3 startPos;
    Quaternion startRot;

    // Use this for initialization
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Floor")
        {
            transform.position = new Vector3(startPos.x, startPos.y, startPos.z);
            transform.rotation = new Quaternion(startRot.x, startRot.y, startRot.z, startRot.w);

            GetComponent<Rigidbody>().velocity = Vector3.zero;  

            GetComponent<Rigidbody>().velocity = Vector3.zero; 

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

    public override void OnAttachedToHand(Hand attachedHand)
    {
        transform.rotation = attachedHand.GetAttachmentTransform("Stick").transform.rotation;
        base.OnAttachedToHand(attachedHand);
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        Services.ChipManager.chipGroup.Clear();
        Services.Dealer.handIsOccupied = false;
        Services.ChipManager.chipsBeingPushed = 0;
        base.OnDetachedFromHand(hand);
    }
}
