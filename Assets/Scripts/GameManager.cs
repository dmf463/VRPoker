using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour {

    public float cardsDealt;
    [Header("Player1")]
    public GameObject player1;
    public List<GameObject> p1HoleCards = new List<GameObject>();
    [Header("Player2")]
    public GameObject player2;
    public List<GameObject> p2HoleCards = new List<GameObject>();
    [Header("Player3")]
    public GameObject player3;
    public List<GameObject> p3HoleCards = new List<GameObject>();
    [Header("Player4")]
    public GameObject player4;
    public List<GameObject> p4HoleCards = new List<GameObject>();
    [Header("TheBoard")]
    public GameObject theBoard;
    public List<GameObject> boardCards = new List<GameObject>();
    [Header ("BurnCards")]
    public GameObject theBurn;
    public bool burnACard;
    public List<GameObject> burnCards = new List<GameObject>();

    private bool flopDealt = false;
    private bool turnDealt = false;
    private bool riverDealt = false;
    private bool readyToEvalute = false;
    private bool winnerDeclared = false;

    // Use this for initialization
    void Start () {

        cardsDealt = 0;
		
	}
	
	// Update is called once per frame
	void Update () {


        if (boardCards.Count == 3 && flopDealt == false)
        {
            OrderHands(p1HoleCards);
            OrderHands(p2HoleCards);
            OrderHands(p3HoleCards);
            OrderHands(p4HoleCards);
            flopDealt = true;
        }
        if (boardCards.Count == 4 && turnDealt == false)
        {
            OrderHands(p1HoleCards);
            OrderHands(p2HoleCards);
            OrderHands(p3HoleCards);
            OrderHands(p4HoleCards);
            turnDealt = true;
        }
        if(boardCards.Count == 5 && riverDealt == false)
        {
            OrderHands(p1HoleCards);
            OrderHands(p2HoleCards);
            OrderHands(p3HoleCards);
            OrderHands(p4HoleCards);
            riverDealt = true;
            readyToEvalute = true;
        }

        if(readyToEvalute == true && winnerDeclared == false)
        {
            PokerHands.EvaluateHand(p1HoleCards);
            //Debug.Log("high card is" + PokerHands.HandValues.HighCard);
            winnerDeclared = true;
        }

    }

    public void OrderHands(List<GameObject> playerCards)
    {
        playerCards.AddRange(boardCards);
        List<GameObject> newhand = playerCards.Distinct().ToList();
        playerCards.Clear();
        playerCards.AddRange(newhand);
        playerCards.Sort((cardLow, cardHigh) => cardLow.GetComponent<PlayingCardScript>().card.rank.CompareTo(cardHigh.GetComponent<PlayingCardScript>().card.rank));
    }


    public List<GameObject> GetHand(int playerNum)
    {
        switch(playerNum)
        {
            case 1:
                return p1HoleCards;
            case 2:
                return p2HoleCards;
            case 3:
                return p3HoleCards;
            case 4:
                return p4HoleCards;
            default:
                Debug.LogError("Player doesn't exist");
                return null;
        }
    }
}
