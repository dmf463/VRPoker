using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool readyToEvalute = false;

    // Use this for initialization
    void Start () {

        cardsDealt = 0;
		
	}
	
	// Update is called once per frame
	void Update () {

        if(boardCards.Count == 5 && readyToEvalute == false)
        {
            p1HoleCards.AddRange(boardCards);
            p2HoleCards.AddRange(boardCards);
            p3HoleCards.AddRange(boardCards);
            p4HoleCards.AddRange(boardCards);
            readyToEvalute = true;
        }
		
	}
}
