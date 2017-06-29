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
    private bool readyToEvalute = false;

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
        if(boardCards.Count == 5 && readyToEvalute == false)
        {
            OrderHands(p1HoleCards);
            OrderHands(p2HoleCards);
            OrderHands(p3HoleCards);
            OrderHands(p4HoleCards);
            readyToEvalute = true;
        }

    }

    public void OrderHands(List<GameObject> playerCards)
    {
        playerCards.AddRange(boardCards);
        List<GameObject> newhand = playerCards.Distinct().ToList();
        playerCards.Clear();
        playerCards.AddRange(newhand);
        playerCards.Sort((cardLow, cardHigh) => cardLow.GetComponent<PlayingCardScript>().rank.CompareTo(cardHigh.GetComponent<PlayingCardScript>().rank));
        //for (int i = 0; i < playerCards.Count; i++)
        //{
        //    if (playerCards[i] == playerCards[i + 1])
        //    {
        //        playerCards.Remove(playerCards[i]);
        //    }
        //}
    }
}
