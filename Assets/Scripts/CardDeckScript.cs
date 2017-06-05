using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

public class CardDeckScript : InteractionSuperClass {

    GameObject cardDeck;
    Quaternion cardDeckPos;
    public List<GameObject> playingCardList = new List<GameObject>();
    Vector3 newCardDeckScale;
    Vector3 currentCardDeckScale;
    Vector3 decreaseCardDeckBy;

    void Start()
    {
        newCardDeckScale = transform.localScale;
        decreaseCardDeckBy = new Vector3 (newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        currentCardDeckScale = newCardDeckScale;
    }


    void Update()
    {
        //Debug.Log("cardDeck has this many children (SHOULD BE 52) : " + transform.childCount);
        //Debug.Log("first card in the deck is: " + transform.GetChild(0).name);
        cardDeck = this.gameObject;
        cardDeckPos = cardDeck.transform.rotation;
    }

    public override void OnTriggerEnterX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.gameObject.GetComponent<Hand>() == throwingHand)
            {
                isTouchingDeck = true;
                //Debug.Log("isTouchingDeck = " + isTouchingDeck);
            }
        }
    }

    public override void OnTriggerExitX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.GetComponent<Hand>() == throwingHand)
            {
                isTouchingDeck = false;
                //Debug.Log("isTouchingDeck = " + isTouchingDeck);
            }
        }
    }

    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
        if(hand.otherHand.GetStandardInteractionButtonDown() == true && isHoldingCard == false && isTouchingDeck == true && playingCardList.Count != 0)
        {
            isHoldingCard = true;
            isTouchingDeck = false;
            int cardPos = Random.Range(0, playingCardList.Count);
            //Debug.Log("Pulling a Card");
            //GameObject playingCard = Instantiate(transform.GetChild(GetCardFromDeck()).gameObject, interactableObject.transform.position, Quaternion.identity);
            //GameObject playingCard = Instantiate(cardPrefab, interactableObject.transform.position, Quaternion.identity);
            GameObject playingCard = Instantiate(playingCardList[cardPos], interactableObject.transform.position, Quaternion.identity);
            playingCardList.Remove(playingCardList[cardPos]);
            playingCard.GetComponent<BoxCollider>().enabled = true;
            playingCard.GetComponent<PlayingCardScript>().enabled = true;
            hand.otherHand.AttachObject(playingCard);

            currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
            transform.localScale = currentCardDeckScale;
            if(playingCardList.Count == 0)
            {
                deckIsDestroyed = true;
                hand.HoverUnlock(interactableObject);
                Destroy(cardDeck);
            }
        }
    }

    public override void OnAttachedToHand(Hand attachedHand)
    {
        isTouchingDeck = false;
        //cardDeck.transform.rotation = Quaternion.Euler(0, 0, 0);
        if(attachedHand.currentAttachedObject.tag == "CardDeck")
        {
            //Debug.Log("attachedHand = " + attachedHand.name + " and attached to it is " + attachedHand.currentAttachedObject.name);
            deckHand = attachedHand;
            throwingHand = attachedHand.otherHand;
            Debug.Log("deckHand = " + attachedHand.name + " and throwingHand = " + attachedHand.otherHand.name);
        }
        base.OnAttachedToHand(attachedHand);
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
        base.HandAttachedUpdate(attachedHand);
        if (attachedHand.otherHand.GetStandardInteractionButton() == false)
        {
            isHoldingCard = false;
        }
    }

    public override void OnDetachedFromHand(Hand hand)
    {
        base.OnDetachedFromHand(hand);
    }

    //public int GetCardFromDeck()
    //{
    //    int cardPos = Random.Range(0, 52);
    //    return cardPos;
    //}

}
