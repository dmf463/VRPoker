using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public float cardsDealt;
    [Header("Player1")]
    public GameObject player1;
    public List<string> p1HoleCards = new List<string>();
    [Header("Player2")]
    public GameObject player2;
    public List<string> p2HoleCards = new List<string>();
    [Header("Player3")]
    public GameObject player3;
    public List<string> p3HoleCards = new List<string>();
    [Header("Player4")]
    public GameObject player4;
    public List<string> p4HoleCards = new List<string>();
    [Header("TheBoard")]
    //public bool readyForFlop;
    //public GameObject flop;
    //public List<string> flopCards = new List<string>();
    //public bool dealtFlop;
    //public GameObject turn;
    //public List<string> turnCards = new List<string>();
    //public bool dealtTurn;
    //public GameObject river;
    //public List<string> riverCards = new List<string>();
    //public bool dealtRiver;
    //public GameObject burn;
    public GameObject theBoard;
    public List<string> boardCards = new List<string>();
    [Header ("BurnCards")]
    public GameObject theBurn;
    public bool burnACard;
    public List<string> burnCards = new List<string>();

    // Use this for initialization
    void Start () {

        cardsDealt = 0;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
