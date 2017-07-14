using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{

    //holds all the cards where they need to be
    public float cardsDealt;
    public List<PokerPlayer> players = new List<PokerPlayer>();

    //keep track of where we are in the game
    //private bool flopDealt = false;
    //private bool turnDealt = false;
    //private bool riverDealt = false;
    //private bool readyToEvalute = false;
    //private bool winnerDeclared = false;
    PokerPlayer player0;
    PokerPlayer player1;
    PokerPlayer player2;
    PokerPlayer player3;

    void Awake()
    {
        Services.PrefabDB = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
    }

    // Use this for initialization
    void Start()
    {
        player0 = new PokerPlayer();
        player1 = new PokerPlayer();
        player2 = new PokerPlayer();
        player3 = new PokerPlayer();

        player0.SeatPos = Destinations.player0;
        player1.SeatPos = Destinations.player1;
        player2.SeatPos = Destinations.player2;
        player3.SeatPos = Destinations.player3;

        player0.PlayerState = PlayerState.Playing;
        player1.PlayerState = PlayerState.Playing;
        player2.PlayerState = PlayerState.Playing;
        player3.PlayerState = PlayerState.Playing;

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
            HandEvaluator player1Hand = new HandEvaluator(TableCards.instance._player1);
            HandEvaluator player2Hand = new HandEvaluator(TableCards.instance._player2);
            HandEvaluator player3Hand = new HandEvaluator(TableCards.instance._player3);

            player0Hand.EvaluateHand();
            player1Hand.EvaluateHand();
            player2Hand.EvaluateHand();
            player3Hand.EvaluateHand();

            player0.Hand = player0Hand;
            player1.Hand = player1Hand;
            player2.Hand = player2Hand;
            player3.Hand = player3Hand;

            Debug.Log(player0.SeatPos + " has " + player0.Hand.HandValues.PokerHand + " with a total HandValue of " + player0.Hand.HandValues.Total + " with a highCard of " + player0.Hand.HandValues.HighCard);
            Debug.Log(player1.SeatPos + " has " + player1.Hand.HandValues.PokerHand + " with a total HandValue of " + player1.Hand.HandValues.Total + " with a highCard of " + player1.Hand.HandValues.HighCard);
            Debug.Log(player2.SeatPos + " has " + player2.Hand.HandValues.PokerHand + " with a total HandValue of " + player2.Hand.HandValues.Total + " with a highCard of " + player2.Hand.HandValues.HighCard);
            Debug.Log(player3.SeatPos + " has " + player3.Hand.HandValues.PokerHand + " with a total HandValue of " + player3.Hand.HandValues.Total + " with a highCard of " + player3.Hand.HandValues.HighCard);

            players.Add(player0);
            players.Add(player1);
            players.Add(player2);
            players.Add(player3);

            List<PokerPlayer> sortedPlayers = new List<PokerPlayer>(players.OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
            sortedPlayers[0].PlayerState = PlayerState.Winner;
            List<List<PokerPlayer>> PlayerRank = new List<List<PokerPlayer>>();
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                if(PlayerRank.Count == 0)
                {
                    PlayerRank.Add(new List<PokerPlayer>() { sortedPlayers[i] });
                }
                else if(sortedPlayers[i].Hand.HandValues == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues)
                {
                    PlayerRank[PlayerRank.Count - 1].Add(sortedPlayers[i]);
                }
                else
                {
                    PlayerRank.Add(new List<PokerPlayer>() { sortedPlayers[i]});
                }
            }

            for (int i = 0; i < PlayerRank.Count; i++)
            {
                if(i == 0)
                {
                    foreach (PokerPlayer player in PlayerRank[0])
                    {
                        player.PlayerState = PlayerState.Winner;
                    }
                }
                else
                {
                    foreach (PokerPlayer player in PlayerRank[i])
                    {
                        player.PlayerState = PlayerState.Loser;
                    }
                }
            }

            //Debug.Log(sortedPlayers[0].SeatPos + " has the best hand with a " + sortedPlayers[0].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[0].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[0].Hand.HandValues.Total);
            //Debug.Log(sortedPlayers[1].SeatPos + " has the second best hand with a " + sortedPlayers[1].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[1].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[1].Hand.HandValues.Total);
            //Debug.Log(sortedPlayers[2].SeatPos + " has the third best hand with a " + sortedPlayers[2].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[2].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[2].Hand.HandValues.Total);
            //Debug.Log(sortedPlayers[3].SeatPos + " has the worst hand with a " + sortedPlayers[3].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[3].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[3].Hand.HandValues.Total);

            for (int i = 0; i < players.Count; i++)
            {
                Debug.Log(players[i].SeatPos + "is the " + players[i].PlayerState + " with (a) " + players[i].Hand.HandValues.PokerHand + " with a highCard of " + players[i].Hand.HandValues.HighCard + " and a handTotal of " + players[i].Hand.HandValues.Total);
            }

            sortedPlayers.Clear();
            players.Clear();
            Debug.Log("sortedPlayers has " + sortedPlayers.Count + "players");

        }

    }
}
