using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class StartCardScript : InteractionSuperClass {

    public GameObject titleText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (!Services.Dealer.startingWithIntro)
        {
            titleText.SetActive(false);
            Destroy(gameObject);
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
        attachedHand.DetachObject(gameObject);
        titleText.SetActive(false);
        Destroy(gameObject);
        Services.Dealer.OpeningCutScene();
        base.OnAttachedToHand(attachedHand);
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        base.OnDetachedFromHand(hand);
    }

}
