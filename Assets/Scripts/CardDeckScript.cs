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
    public bool deckGotThrown = false;
    float explosionPower = 1;
    float explosionRadius = 30;
    public bool thrownDeck;
    public float badThrowVelocity;
    public bool testDeal;
    public List<GameObject> testSpaces = new List<GameObject>();
    int testCardPos;
    Vector3 testCardSecondPos;
    Vector3 testCardThirdPos;
    GameObject gameManager;
    GameManager gm;

    void Start()
    {
        newCardDeckScale = transform.localScale;
        decreaseCardDeckBy = new Vector3 (newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        //Debug.Log("decreaseCardDeckby = " + decreaseCardDeckBy);
        currentCardDeckScale = newCardDeckScale;
        testDeal = true;
        testCardPos = 0;
        testCardSecondPos = new Vector3(-0.1f, 0, 0);
        testCardThirdPos = new Vector3(-0.2f, 0, 0);
        gameManager = GameObject.Find("GameManager");
        gm = gameManager.GetComponent<GameManager>();
    }


    void Update()
    {
        //Debug.Log("deckGotThrown is " + deckGotThrown);
        //Debug.Log("cardDeck has this many children (SHOULD BE 52) : " + transform.childCount);
        //Debug.Log("first card in the deck is: " + transform.GetChild(0).name);
        cardDeck = this.gameObject;
        cardDeckPos = cardDeck.transform.rotation;
        if (playingCardList.Count == 0)
        {
            deckIsDestroyed = true;
            Destroy(cardDeck);
        }

        if(testDeal == true && Input.GetKeyDown(KeyCode.Space))
        {
            gm.cardsDealt += 1;
            Debug.Log(gm.cardsDealt);
            testingOutsideVR = true;
            if(testSpaces[testCardPos].GetComponent<PlayerPosition>().doneDealing == false)
            {
                int cardPos = Random.Range(0, playingCardList.Count);
                GameObject playingCard = Instantiate(playingCardList[cardPos], testSpaces[testCardPos].transform.position, Quaternion.identity);
                if (testSpaces[testCardPos].GetComponent<PlayerPosition>().cardCount == 1)
                {
                    playingCard.transform.position = testSpaces[testCardPos].transform.position + testCardSecondPos;
                }
                playingCard.transform.eulerAngles = new Vector3(90, 0, 0);
                playingCardList.Remove(playingCardList[cardPos]);
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.GetComponent<PlayingCardScript>().enabled = true;
                currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
                transform.localScale = currentCardDeckScale;
            }

            if(gm.burnACard == true)
            {
                int cardPos = Random.Range(0, playingCardList.Count);
                GameObject playingCard = Instantiate(playingCardList[cardPos], gm.burn.transform.position, Quaternion.identity);
                playingCard.transform.eulerAngles = new Vector3(270, 0, 0);
                gm.burnCards.Add(playingCard.name);
                playingCardList.Remove(playingCardList[cardPos]);
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.GetComponent<PlayingCardScript>().enabled = true;
                currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
                transform.localScale = currentCardDeckScale;
                gm.burnACard = false;
            }

            if (gm.cardsDealt == 8 && gm.readyForFlop == false)
            {
                gm.burnACard = true;
            }

            if(gm.cardsDealt == 10)
            {
                gm.readyForFlop = true;
            }

            if(gm.readyForFlop == true)
            {
                int cardPos = Random.Range(0, playingCardList.Count);
                GameObject playingCard = Instantiate(playingCardList[cardPos], gm.flop.transform.position, Quaternion.identity);
                if (gm.flopCards.Count == 1)
                {
                    playingCard.transform.position = gm.flop.transform.transform.position + new Vector3 (-0.1f, 0, 0);
                }
                if (gm.flopCards.Count == 2)
                {
                    playingCard.transform.position = gm.flop.transform.transform.position + new Vector3(-0.2f, 0, 0);
                }
                playingCard.transform.eulerAngles = new Vector3(90, 0, 0);
                gm.flopCards.Add(playingCard.name);
                playingCardList.Remove(playingCardList[cardPos]);
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.GetComponent<PlayingCardScript>().enabled = true;
                currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
                transform.localScale = currentCardDeckScale;
            }
            if(gm.flopCards.Count == 3)
            {
                gm.readyForFlop = false;
                gm.dealtFlop = true;
            }
            if(gm.dealtFlop == true)
            {
                gm.burnACard = true;
            }
            if(gm.cardsDealt == 13)
            {
                gm.dealtFlop = false;
                gm.burnACard = false;
            }
            if(gm.cardsDealt == 14)
            {
                int cardPos = Random.Range(0, playingCardList.Count);
                GameObject playingCard = Instantiate(playingCardList[cardPos], gm.turn.transform.position, Quaternion.identity);
                playingCard.transform.eulerAngles = new Vector3(90, 0, 0);
                gm.turnCards.Add(playingCard.name);
                playingCardList.Remove(playingCardList[cardPos]);
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.GetComponent<PlayingCardScript>().enabled = true;
                currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
                transform.localScale = currentCardDeckScale;
                gm.burnACard = true;
            }
            if(gm.cardsDealt == 15)
            {
                gm.burnACard = false;
            }
            if(gm.cardsDealt == 16)
            {
                int cardPos = Random.Range(0, playingCardList.Count);
                GameObject playingCard = Instantiate(playingCardList[cardPos], gm.river.transform.position, Quaternion.identity);
                playingCard.transform.eulerAngles = new Vector3(90, 0, 0);
                gm.riverCards.Add(playingCard.name);
                playingCardList.Remove(playingCardList[cardPos]);
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.GetComponent<PlayingCardScript>().enabled = true;
                currentCardDeckScale.y = currentCardDeckScale.y - decreaseCardDeckBy.y;
                transform.localScale = currentCardDeckScale;
                testDeal = false;
            }


            if (playingCardList.Count == 0)
            {
                Destroy(cardDeck);
            }
            testCardPos += 1;
            if (testCardPos > 3)
            {
                testCardPos = 0;
            }
        }
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
        badThrowVelocity = deckHand.GetTrackedObjectVelocity().magnitude;
        Debug.Log("there are " + playingCardList.Count + " cards in the deck");
        if (hand.GetTrackedObjectVelocity().magnitude > 1) deckGotThrown = true;
        if (deckGotThrown == true)
        {
            deckGotThrown = false;
            int playingCardListCount = playingCardList.Count;
            for (int i = playingCardListCount - 1; i >= 0; i--)
            {
                Debug.Log(i);
                GameObject playingCard = cardDeck.transform.GetChild(i).gameObject;
                playingCard.transform.parent = null;
                playingCard.GetComponent<BoxCollider>().enabled = true;
                playingCard.AddComponent<Rigidbody>();
                Debug.Log("rigidBody added at " + Time.time);
                playingCard.AddComponent<ConstantForce>();
                playingCard.GetComponent<PlayingCardScript>().enabled = true;
                playingCard.GetComponent<PlayingCardScript>().badThrow = true;
                playingCard.GetComponent<Rigidbody>().AddForce(hand.GetTrackedObjectVelocity(), ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddTorque(hand.GetTrackedObjectAngularVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
                playingCard.GetComponent<Rigidbody>().AddExplosionForce(explosionPower, playingCard.transform.position, explosionRadius, 0, ForceMode.Impulse);

                if (i == 0)
                {
                    deckIsDestroyed = true;
                    thrownDeck = true;
                }
            }
        }
        base.OnDetachedFromHand(hand);
    }

    //public int GetCardFromDeck()
    //{
    //    int cardPos = Random.Range(0, 52);
    //    return cardPos;
    //}

}
