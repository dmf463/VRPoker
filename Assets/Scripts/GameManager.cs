using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour {

    //holds all the cards where they need to be
    public float cardsDealt;
    //[Header("Player1")]
    //public GameObject player1;
    //public List<GameObject> p1HoleCards = new List<GameObject>();
    //[Header("Player2")]
    //public GameObject player2;
    //public List<GameObject> p2HoleCards = new List<GameObject>();
    //[Header("Player3")]
    //public GameObject player3;
    //public List<GameObject> p3HoleCards = new List<GameObject>();
    //[Header("Player4")]
    //public GameObject player4;
    //public List<GameObject> p4HoleCards = new List<GameObject>();
    //[Header("TheBoard")]
    //public GameObject theBoard;
    //public List<GameObject> boardCards = new List<GameObject>();
    //[Header ("BurnCards")]
    //public GameObject theBurn;
    //public bool burnACard;
    //public List<GameObject> burnCards = new List<GameObject>();

    //keep track of where we are in the game
    private bool flopDealt = false;
    private bool turnDealt = false;
    private bool riverDealt = false;
    private bool readyToEvalute = false;
    private bool winnerDeclared = false;

    //evaluate hands
    enum HandType
    {
        HighCard,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush
    }
    public int spadeSum;
    public int heartSum;
    public int diamondSum;
    public int clubSum;
    PlayingCardPhysics cardScript;
    int highCard;

    // Use this for initialization
    void Start () {

        cardsDealt = 0;
    }

	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TableCards.instance.DebugHands();
        }

        //if (boardCards.Count == 3 && flopDealt == false)
        //{
        //    OrderHands(p1HoleCards);
        //    OrderHands(p2HoleCards);
        //    OrderHands(p3HoleCards);
        //    OrderHands(p4HoleCards);
        //    flopDealt = true;
        //}
        //if (boardCards.Count == 4 && turnDealt == false)
        //{
        //    OrderHands(p1HoleCards);
        //    OrderHands(p2HoleCards);
        //    OrderHands(p3HoleCards);
        //    OrderHands(p4HoleCards);
        //    turnDealt = true;
        //}
        //if(boardCards.Count == 5 && riverDealt == false)
        //{
        //    OrderHands(p1HoleCards);
        //    OrderHands(p2HoleCards);
        //    OrderHands(p3HoleCards);
        //    OrderHands(p4HoleCards);
        //    riverDealt = true;
        //    readyToEvalute = true;
        //}

        if(readyToEvalute == true && winnerDeclared == false)
        {
            //EvaluateHand(p1HoleCards, p1HoleCards[6].GetComponent<PlayingCardScript>().handValue.HighCard);
            //EvaluateHand(p2HoleCards, p2HoleCards[6].GetComponent<PlayingCardScript>().handValue.HighCard);
            //EvaluateHand(p3HoleCards, p3HoleCards[6].GetComponent<PlayingCardScript>().handValue.HighCard);
            //EvaluateHand(p4HoleCards, p4HoleCards[6].GetComponent<PlayingCardScript>().handValue.HighCard);
            winnerDeclared = true;
        }

    }

    public void OrderHands(List<GameObject> playerCards)
    {
        //playerCards.AddRange(boardCards);
        List<GameObject> newhand = playerCards.Distinct().ToList();
        playerCards.Clear();
        playerCards.AddRange(newhand);
        //playerCards.Sort((cardLow, cardHigh) => cardLow.GetComponent<PlayingCardScript>().card.rank.CompareTo(cardHigh.GetComponent<PlayingCardScript>().card.rank));
    }

    //public List<GameObject> GetHand(int playerNum)
    //{
    //    switch(playerNum)
    //    {
    //        case 1:
    //            return p1HoleCards;
    //        case 2:
    //            return p2HoleCards;
    //        case 3:
    //            return p3HoleCards;
    //        case 4:
    //            return p4HoleCards;
    //        default:
    //            Debug.LogError("Player doesn't exist");
    //            return null;
    //    }
    //}

    HandType EvaluateHand(List<GameObject> hand, int _highCard)
    {
        getNumberOfSuit(hand);
        if (OnePair(hand))
            return HandType.Pair;
        else if (TwoPair(hand))
            return HandType.TwoPair;
        else if (ThreeOfKind(hand))
            return HandType.ThreeOfAKind;
        else if (Straight(hand))
            return HandType.Straight;
        else if (Flush(hand))
            return HandType.Flush;
        else if (FourOfKind(hand))
            return HandType.FourOfAKind;
        else if (FullHouse(hand))
            return HandType.FullHouse;
        else if (StraightFlush(hand))
            return HandType.StraightFlush;
        //hand[6].GetComponent<PlayingCardScript>().handValue.HighCard = _highCard;
        Debug.Log(hand[0].name + "has a highCard of " + _highCard);
        return HandType.HighCard;
    }

    public void getNumberOfSuit(List<GameObject> hand)
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        foreach (GameObject card in hand)
        {
            //if(card.GetComponent<PlayingCardScript>().card.suit == Cards.SuitType.Spades)
            //{
            //    spadeSum++;
            //}
            //else if(card.GetComponent<PlayingCardScript>().card.suit == Cards.SuitType.Hearts)
            //{
            //    heartSum++;
            //}
            //else if (card.GetComponent<PlayingCardScript>().card.suit == Cards.SuitType.Diamonds)
            //{
            //    diamondSum++;
            //}
            //else if (card.GetComponent<PlayingCardScript>().card.suit == Cards.SuitType.Clubs)
            //{
            //    clubSum++;
            //}
        }
    }

    public bool HighCard(List<GameObject> hand)
    {
        return false;
    }

    public bool OnePair(List<GameObject> hand)
    {
        //if(hand[0].GetComponent<PlayingCardScript>().card.rank == hand[1].GetComponent<PlayingCardScript>().card.rank)
        //{
        //    hand[0].GetComponent<PlayingCardScript>().handValue.Total = (int)hand[0].GetComponent<PlayingCardScript>().card.rank * 2;
        //    Debug.Log(hand.ToString());
        //    return true;
        //}
        //else if (hand[1].GetComponent<PlayingCardScript>().card.rank == hand[2].GetComponent<PlayingCardScript>().card.rank)
        //{
        //    hand[1].GetComponent<PlayingCardScript>().handValue.Total = (int)hand[1].GetComponent<PlayingCardScript>().card.rank * 2;
        //    return true;
        //}
        //else if (hand[2].GetComponent<PlayingCardScript>().card.rank == hand[3].GetComponent<PlayingCardScript>().card.rank)
        //{
        //    hand[2].GetComponent<PlayingCardScript>().handValue.Total = (int)hand[2].GetComponent<PlayingCardScript>().card.rank * 2;
        //    return true;
        //}
        return false;
    }

    public bool TwoPair(List<GameObject> hand)
    {
        return false;
    }

    public bool ThreeOfKind(List<GameObject> hand)
    {
        return false;
    }

    public bool Straight(List<GameObject> hand)
    {
        return false;
    }

    public bool Flush(List<GameObject> hand)
    {
        return false;
    }

    public bool FourOfKind(List<GameObject> hand)
    {
        return false;
    }

    public bool FullHouse(List<GameObject> hand)
    {
        return false;
    }

    public bool StraightFlush(List<GameObject> hand)
    {
        return false;
    }
}
