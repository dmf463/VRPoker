using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{

    //holds all the cards where they need to be
    public float cardsDealt;

    //keep track of where we are in the game
    private bool flopDealt = false;
    private bool turnDealt = false;
    private bool riverDealt = false;
    private bool readyToEvalute = false;
    private bool winnerDeclared = false;

    // Use this for initialization
    void Start()
    {

        cardsDealt = 0;

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("player0 in GM has " + player0Cards.Count + " cards");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //this is purelyfor testing purposes
            //I need to write a function to do this, but also need to write comparisons
            //I need to log who had the highest hand, the second highest, and so forth, so that eventually I can add in side pots and shit
            //I COULD do that in another script? some kind of Comparer Script maybe? and make instances of itself to compare? but either way it's gonna be a pain in the ass
            TableCards.instance.DebugHands();
            TableCards.instance.EvaluatePlayer(Destinations.player0);
            TableCards.instance.EvaluatePlayer(Destinations.player1);
            TableCards.instance.EvaluatePlayer(Destinations.player2);
            TableCards.instance.EvaluatePlayer(Destinations.player3);

            HandEvaluator player0Hand = new HandEvaluator(TableCards.instance._player0);
            Debug.Log("player0 has " + player0Hand.Cards.Count + "cards according to the GM");
            HandEvaluator player1Hand = new HandEvaluator(TableCards.instance._player1);
            HandEvaluator player2Hand = new HandEvaluator(TableCards.instance._player2);
            HandEvaluator player3Hand = new HandEvaluator(TableCards.instance._player3);

            PokerHand finalP0Hand = player0Hand.EvaluateHand();
            PokerHand finalP1Hand = player1Hand.EvaluateHand();
            PokerHand finalP2Hand = player2Hand.EvaluateHand();
            PokerHand finalP3Hand = player3Hand.EvaluateHand();

            Debug.Log("player0 has " + finalP0Hand + " with a highCard of " + player0Hand.HandValues.HighCard);
            Debug.Log("player1 has " + finalP1Hand + " with a highCard of " + player1Hand.HandValues.HighCard);
            Debug.Log("player2 has " + finalP2Hand + " with a highCard of " + player2Hand.HandValues.HighCard);
            Debug.Log("player3 has " + finalP3Hand + " with a highCard of " + player3Hand.HandValues.HighCard);
        }

        if (readyToEvalute == true && winnerDeclared == false)
        {
            winnerDeclared = true;
        }

    }
}
