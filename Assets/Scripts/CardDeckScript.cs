﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR; //we need this for SteamVR
using Valve.VR.InteractionSystem;

//this is the script that holds all the cards in the deck
//as well it controls all the physics for the card deck
//the biggest problem are here is how we use the scale to decrement and increment the deck
//basically because the size of the deck is a float, keeping it a constant size has been an in issue
//mostly cause we're messing with the size
public class CardDeckScript : InteractionSuperClass {

    //this is the actual list of cards in the deck still
    [HideInInspector]
    public List<CardType> cardsInDeck;
    
    //this is the deck...idk why I have a reference to the deck on the deck..
    //oh...apparently I have it for readability
    GameObject cardDeck;

    //this is the offset used when playing 52 Card Pickup, so that all the cards don't spawn in the same place
    //this probably doesn't need to be global
    //note: we should pretty much always reduce the amount of global variables
    public float cardSpawnOffset;

    //this is the threshold to check whether the deck was thrown or not. 
    //this is awkward because it doesn't hold up to realism
    //cause if you drop the deck, it just stays as a solid object
    //this has been something that's bothered me since the beginning
    public float velocityThreshold;

    //this is the list of meshes that hold the actual faces of the cards
    public List<Mesh>[] cardMeshes;

    //we added these in manually in the inspector
    //it's just so we can have a single card prefab and then change the mesh
    //though we should probably mess around with the card faces to make them more readable
    //idk, cause then we lose some realism 
    public List<Mesh> spadeMeshes;
    public List<Mesh> heartMeshes;
    public List<Mesh> diamondMeshes;
    public List<Mesh> clubMeshes;

    //these are the the scales of the deck. I don't like this
    //it leads to random instances where the card deck thinks it's full before it actually is
    //we should avoid messing with the scale TOO much, though we pretty much need to in order to make the illusion of pulling cards
    [HideInInspector]
    public Vector3 newCardDeckScale;
    [HideInInspector]
    public Vector3 currentCardDeckScale;
    [HideInInspector]
    public Vector3 oneCardScale;

    //these are the bools for throwing the deck
    //which I don't like the way we're currently throwing the deck
    //so like. yeah.
    [HideInInspector]
    public bool deckIsBeingThrown = false;
    public bool deckWasThrown;
    public float badThrowVelocity;

    //so this is the bool that prevents us from instantiating more than one card at a time 
    private bool readyForAnotherCard = false;

    //these are the bools that get set to allow us to cheat >=)
    private bool grabbingHighCard;
    private bool grabbingLowCard;

    //lalalalalala, setting stuff, lalalalalalalala
    void Start()
    {
        cardDeck = this.gameObject;
        cardMeshes = new List<Mesh>[4]
        {
            spadeMeshes,
            heartMeshes,
            diamondMeshes,
            clubMeshes
        };
        if (newCardDeckScale.y == 0)
        {
            newCardDeckScale = transform.localScale;
            currentCardDeckScale = newCardDeckScale;
        } 
        oneCardScale = new Vector3 (newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        PopulateCardDeck();
        deckIsEmpty = false;
    }


    void Update()
    {

        //so if we have a throwing hand, want to make sure we're always checking 
        //whether it was swiped, or pressed
        if (!Services.Dealer.OutsideVR)
        {
            if (throwingHand != null)
            {
                CheckPressPosition(throwingHand);
                CheckSwipeDirection();
            }
        }

        //this is so if we're doing something outside of VR we can pull a card
        if (Input.GetKeyDown(KeyCode.Z))
        {
            int cardPos = Random.Range(0, cardsInDeck.Count);
            CardType cardType = cardsInDeck[cardPos];
            Card card = CreateCard(cardType, GameObject.Find("ShufflingArea").transform.position, Quaternion.identity);
            card.gameObject.name = (card.cardType.rank + " of " + card.cardType.suit);
            if(!Services.Dealer.inRound) Services.PokerRules.cardsPulled.Add(card.cardType);
        }

    }

    //so if we're in shuffling state
    //and a card hits the existing deck
    //we destroy the deck and make the deck larger
    //this is where we have issues, because TECHNICALLY, the card deck is full
    //we've refilled it already
    //but we're just messing with scale in order to achieve the illusion that we're pulling cards and stuff
    void OnCollisionEnter(Collision other)
    {
        if (Table.dealerState == DealerState.ShufflingState)
        {
            if (Table.gameState == GameState.CleanUp || Table.gameState == GameState.PostHand)
            {
                if (other.gameObject.tag == "PlayingCard")
                {
                    Destroy(other.gameObject);
                    MakeDeckLarger();
                }
                //this is the problem, because like, whenever we decrement and increment the card deck
                //we permanently change the scale
                //which makes this trigger early sometimes
                //I should do
                //have a variable that knows how many cards are on the table
                //once I've destroyed that many cards
                //refill the deck, and set the decks scale to the original deck scale
                if (currentCardDeckScale.y >= newCardDeckScale.y)
                {
                    GameObject[] deadCards = GameObject.FindGameObjectsWithTag("PlayingCard");
                    foreach (GameObject card in deadCards)
                    {
                        Destroy(card);
                    }
                    RefillCardDeck();
                    Table.dealerState = DealerState.DealingState;
                }
            }
        }
    }

    //so this is how we know whether the hand is touching the deck or not
    public override void OnTriggerEnterX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.gameObject.GetComponent<Hand>() == throwingHand)
            {
                handTouchingDeck = true;
                readyForAnotherCard = true;
            }
        }
    }

    //this is how we know that the hand is no longer touching the deck
    public override void OnTriggerExitX(Collider other)
    {
        if(other.gameObject.tag == "Hand")
        {
            if(other.GetComponent<Hand>() == throwingHand)
            {
                handTouchingDeck = false;
                readyForAnotherCard = false;
            }
        }
    }

    //part of the interaction class this derives from.
    //this is alos where we grab a card and put it in the hand
    //when we're hovering over the deck, this function is being called
    //so if we're not holding a card, the deck is full, and we're touching the deck
    //then we can create a card, attach it to the hand, and make the deck smaller
    //if it's the last card, then we destroy the deck
    public override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);
        if (hand.otherHand.GetStandardInteractionButtonDown() == true && 
            handIsHoldingCard == false && 
            handTouchingDeck == true && 
            cardsInDeck.Count != 0)
        {
            handIsHoldingCard = true;
            handTouchingDeck = false;
            Card card = CreateCard(GrabACard(), interactableObject.transform.position, Quaternion.identity);
            card.gameObject.name = (card.cardType.rank + " of " + card.cardType.suit);
            //Services.PokerRules.cardsPulled.Add(card.cardType);
            //Services.PokerRules.cardsToDestroy.Add(card);
            hand.otherHand.AttachObject(card.gameObject);
            MakeDeckSmaller();
            if (cardsInDeck.Count == 0)
            {
                hand.HoverUnlock(interactableObject);
                Destroy(cardDeck);
                Debug.Log("Destroyed Deck");
                Table.dealerState = DealerState.ShufflingState;
            }
        }
    }

    //so this is the function we have that figures out which card to grab
    //if it was picked up without any special swipes, then it grabs a random card from the deck
    //if it was picked up when grabbingHighCard is true, then it finds the highest value card in the deck
    //the reverse is true if grabbingLowCard is true
    public CardType GrabACard()
    {
        CardType cardToGrab = null;
        if (grabbingLowCard == true)
        {
            //Debug.Log("Grabbing lowest card");
            grabbingLowCard = false;
            cardToGrab = cardsInDeck[0];
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                CardType currentCard = cardsInDeck[i];
                if((int)currentCard.rank < (int)cardToGrab.rank)
                {
                    cardToGrab = currentCard;
                }
            }
        }
        else if (grabbingHighCard == true)
        {
            //Debug.Log("Grabbing high card");
            grabbingHighCard = false;
            cardToGrab = cardsInDeck[0];
            for (int i = 0; i < cardsInDeck.Count; i++)
            {
                CardType currentCard = cardsInDeck[i];
                if ((int)currentCard.rank > (int)cardToGrab.rank)
                {
                    cardToGrab = currentCard;
                }
            }
        }
        else
        {
            //Debug.Log("grabbing random card");
            int cardPos = Random.Range(0, cardsInDeck.Count);
            cardToGrab = cardsInDeck[cardPos];
        }
        return cardToGrab;
    }

    //part of the interaction class this derives from.
    //so when we something is attached to the hand, we set handTouchingDeck to false, because it's not touching the deck
    //it's touching the thing it's holding
    //here we also set which hand is the "deckHand" and which hand is the "throwingHand"
    //these are variables from the superClass and make it super convenient to know which hand is which when writing functions
    public override void OnAttachedToHand(Hand attachedHand)
    {
        handTouchingDeck = false;
        if(attachedHand.currentAttachedObject.tag == "CardDeck")
        {
            deckHand = attachedHand;
            throwingHand = attachedHand.otherHand;
            //Debug.Log("deckHand = " + attachedHand.name + " and throwingHand = " + attachedHand.otherHand.name);
        }
        base.OnAttachedToHand(attachedHand);
    }

    //part of the interaction class this derives from.
    //so we have all these references to the other hand one the deckScript 
    //this is necessary because if the otherHand is holding stuff, or already grabbed a card
    //then we need to not be able to pull other objects or pick up other things
    public override void HandAttachedUpdate(Hand attachedHand)
    {
        base.HandAttachedUpdate(attachedHand);
        if (attachedHand.otherHand.GetStandardInteractionButton() == false)
        {
            handIsHoldingCard = false;
        }
    }

    //part of the interaction class this derives from.
    //this gets called when something is detached
    //we're using this here in order to check the throw velocity of the deck
    //if it WAS thrown then it needs to burst into cards
    //the problem is that it should really be relative, and right now it's not
    //we'll learn more when we go to FiftyTwoCardPickUp();
    public override void OnDetachedFromHand(Hand hand)
    {
        badThrowVelocity = deckHand.GetTrackedObjectVelocity().magnitude;

        //
        //should seriously rethink this because of the way it feels between dropping the deck and throwing the deck
        //
        if (hand.GetTrackedObjectVelocity().magnitude > velocityThreshold) deckIsBeingThrown = true;
        if (deckIsBeingThrown == true)
        {
            deckIsBeingThrown = false;
            FiftyTwoCardPickUp();
            deckIsEmpty = true;
            deckWasThrown = true;
        }
        base.OnDetachedFromHand(hand);
    }

    //this refills the card deck with cards
    //we clear the current deck
    //setup  newHand
    //and then refill the deck with 52 new cardTypes
    public void RefillCardDeck()
    {
        Debug.Log("Refilling CardDeck");
        cardsInDeck.Clear();
        Table.instance.NewHand();
        SuitType[] suits = new SuitType[4]
        {
            SuitType.Spades,
            SuitType.Hearts,
            SuitType.Diamonds,
            SuitType.Clubs
        };
        RankType[] ranks = new RankType[13]
        {
            RankType.Two,
            RankType.Three,
            RankType.Four,
            RankType.Five,
            RankType.Six,
            RankType.Seven,
            RankType.Eight,
            RankType.Nine,
            RankType.Ten,
            RankType.Jack,
            RankType.Queen,
            RankType.King,
            RankType.Ace
        };

        foreach (SuitType suit in suits)
        {
            foreach (RankType rank in ranks)
            {
                cardsInDeck.Add(new CardType(rank, suit));
            }
        }
    }

    //this is the inititial creation of the deck
    //same as above, except it creates the list
    public void PopulateCardDeck()
    {
        cardsInDeck = new List<CardType>();
        SuitType[] suits = new SuitType[4] 
        {
            SuitType.Spades,
            SuitType.Hearts,
            SuitType.Diamonds,
            SuitType.Clubs
        };
        RankType[] ranks = new RankType[13]
        {
            RankType.Two,
            RankType.Three,
            RankType.Four,
            RankType.Five,
            RankType.Six,
            RankType.Seven,
            RankType.Eight,
            RankType.Nine,
            RankType.Ten,
            RankType.Jack,
            RankType.Queen,
            RankType.King,
            RankType.Ace
        };

        foreach (SuitType suit in suits)
        {
            foreach(RankType rank in ranks)
            {
                cardsInDeck.Add(new CardType(rank, suit));
            }
        }
    }

    //this is just the function that we use in order to instantiate a new card
    //we also set it's mesh here so it has the proper face
    public Card CreateCard(CardType cardType, Vector3 position, Quaternion rotation)
    {
        cardsInDeck.Remove(cardType);
        GameObject playingCard = Instantiate(Services.PrefabDB.Card, position, rotation);
        playingCard.GetComponent<MeshFilter>().mesh = cardMeshes[(int)cardType.suit][(int)cardType.rank - 2];
        playingCard.GetComponent<Card>().cardType = cardType;
        playingCard.GetComponent<Card>().cardMarkedForDestruction = true;
        return playingCard.GetComponent<Card>();
    }

    //this is what we call when the deck is completely empty, and we want to start with "one" card
    //this is another instance where we're messing with the transform and I'm not sure it's a good idea
    public void BuildDeckFromOneCard(GameObject newCardDeck)
    {
        newCardDeckScale = newCardDeck.transform.localScale;
        oneCardScale = new Vector3(newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
        newCardDeck.transform.localScale = oneCardScale;
        currentCardDeckScale = newCardDeck.transform.localScale;
    }

    //same as above
    //like, it works right now
    //but it can be buggy because we're messing with floats
    public void MakeDeckSmaller()
    {
        currentCardDeckScale.y = currentCardDeckScale.y - oneCardScale.y;
        transform.localScale = currentCardDeckScale;
    }

    //it's just kind of unreliable
    public void MakeDeckLarger()
    {
        currentCardDeckScale.y = currentCardDeckScale.y + oneCardScale.y;
        transform.localScale = currentCardDeckScale;
    }

    //if we've thrown the deck, we want to have all the cards go flying
    //so we basically instantiate every card left in the deck within a sphere and at a random offset
    //then we set the proper bools on each card to activate the proper physics
    //we also destroy the deck at the end because it interacts with the cards in weird fucking ways
    //I'm not a fan of the way we're doing this currently because even if you throw the deck softly, it just kind of explodes. 
    //I really liked the way it worked originally, but it also worked stupidly and caused bugs
    //even though THIS causes bugs cause sometimes the deck will just fucking dissapear without instantiating anything
    public void FiftyTwoCardPickUp()
    {
        int cardsInDeckCount = cardsInDeck.Count;
        for (int i = cardsInDeckCount - 1; i >= 0; i--)
        {

            GameObject playingCard = CreateCard(cardsInDeck[i], transform.position + Random.insideUnitSphere * cardSpawnOffset , Quaternion.identity).gameObject;
            playingCard.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
            playingCard.GetComponent<Rigidbody>().angularVelocity = GetComponent<Rigidbody>().angularVelocity;


            playingCard.GetComponent<Card>().cardThrownWrong = true;
            ////Debug.Log("bad throw is = " + playingCard.GetComponent<Card>().badThrow);
            playingCard.GetComponent<Rigidbody>().AddForce(deckHand.GetTrackedObjectVelocity(), ForceMode.Impulse);
            playingCard.GetComponent<Rigidbody>().AddTorque(deckHand.GetTrackedObjectAngularVelocity() * FORCE_MULTIPLIER, ForceMode.Impulse);
        }
        StartCoroutine(WaitToDestroyDeck(0.25f));
        
		PokerPlayerRedux randomPlayer = Services.Dealer.players[Random.Range(0,5)];
		Services.SoundManager.GenerateSourceAndPlay(randomPlayer.fiftyTwoAudio);
    }

    //destroys the deck and sets us to shuffling mode
    IEnumerator WaitToDestroyDeck(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(cardDeck);
        Table.dealerState = DealerState.ShufflingState;
    }

    //when we swipe down, we set grabbingLowCard to true
    public override void OnSwipeBottom()
    {
        if (handTouchingDeck) grabbingLowCard = true;
        base.OnSwipeBottom();
    }

    //when we swipe up we set grabbingHighCard to true
    public override void OnSwipeTop()
    {
        if (handTouchingDeck) grabbingHighCard = true;
        base.OnSwipeTop();
    }

    //when we CLICK down
    //if we can hold another card, and we're already holding a card
    //we instantiate a new card into your hand with the proper offset
    //we also make the deck smaller and destroy the deck if it was the last card
    //public override void OnPressBottom()
    //{
    //    if (readyForAnotherCard && handIsHoldingCard == true)
    //    {
    //        GameObject firstCard = throwingHand.currentAttachedObject.gameObject;
    //        Vector3 offset = new Vector3(-0.01f, -0.01f, 0) * throwingHand.AttachedObjects.Count;
    //        int cardPos = Random.Range(0, cardsInDeck.Count);
    //        CardType cardType = cardsInDeck[cardPos];
    //        Card card = CreateCard(cardType, throwingHand.transform.position, Quaternion.identity);
    //        card.gameObject.name = (card.cardType.rank + " of " + card.cardType.suit);
    //        if(throwingHand.AttachedObjects.Count < 3)
    //        {
    //            throwingHand.AttachObject(card.gameObject, Hand.AttachmentFlags.ParentToHand);
    //            card.gameObject.transform.localPosition = new Vector3(firstCard.transform.localPosition.x + offset.x, firstCard.transform.localPosition.y + offset.y, firstCard.transform.localPosition.z);
    //        }
    //        MakeDeckSmaller();
    //        if (cardsInDeck.Count == 0)
    //        {
    //            deckHand.HoverUnlock(interactableObject);
    //            Destroy(cardDeck);
    //            Debug.Log("Destroyed Deck");
    //            Table.dealerState = DealerState.ShufflingState;
    //        }
    //    }
    //}
}
