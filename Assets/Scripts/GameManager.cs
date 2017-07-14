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
        players.Add(player0);
        players.Add(player1);
        players.Add(player2);
        players.Add(player3);

        InitializePlayers();

        cardsDealt = 0;

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("player0 in GM has " + player0Cards.Count + " cards");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EvaluateHandOnRiver();
        }

    }

    public void InitializePlayers()
    {
        int[] enumValues = new int[] { 0, 1, 2, 3 };
        for (int i = 0; i < players.Count; i++)
        {  
            players[i] = new PokerPlayer();
            players[i].SeatPos = i;
            players[i].PlayerState = PlayerState.Playing;
        }
    }

    public void EvaluateHandOnRiver()
    {
        TableCards.instance.DebugHands();
        for (int i = 0; i < players.Count; i++)
        {
            List<CardType> sortedCards = TableCards.instance.EvaluatePlayer(players[i].SeatPos);
            HandEvaluator playerHand = new HandEvaluator(sortedCards);
            playerHand.EvaluateHand();
            players[i].Hand = playerHand;
        }

        List<PokerPlayer> sortedPlayers = new List<PokerPlayer>(players.OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
        sortedPlayers[0].PlayerState = PlayerState.Winner;
        List<List<PokerPlayer>> PlayerRank = new List<List<PokerPlayer>>();
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (PlayerRank.Count == 0)
            {
                PlayerRank.Add(new List<PokerPlayer>() { sortedPlayers[i] });
            }
            else if (sortedPlayers[i].Hand.HandValues == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues)
            {
                PlayerRank[PlayerRank.Count - 1].Add(sortedPlayers[i]);
            }
            else
            {
                PlayerRank.Add(new List<PokerPlayer>() { sortedPlayers[i] });
            }
        }

        for (int i = 0; i < PlayerRank.Count; i++)
        {
            if (i == 0)
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

        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log("player" + players[i].SeatPos + "is the " + players[i].PlayerState + " with (a) " + players[i].Hand.HandValues.PokerHand + " with a highCard of " + players[i].Hand.HandValues.HighCard + " and a handTotal of " + players[i].Hand.HandValues.Total);
        }

        sortedPlayers.Clear();

    }
}
