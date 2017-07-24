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
    PokerPlayer player4;

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
        players.Add(player4);

        InitializePlayers();

        cardsDealt = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Table.instance.DebugHands();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            foreach(PokerPlayer player in players) 
            {
                player.EvaluateHandPreFlop();
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            foreach (PokerPlayer player in players) 
            {
                player.EvaluateHandOnFlop();
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (PokerPlayer player in players) 
            {
                player.EvaluateHandOnTurn();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (PokerPlayer player in players) 
            {
                player.EvaluateHandOnRiver();
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            EvaluatePlayersOnShowdown();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ResetPlayerStatus();
        }


    }

    public void InitializePlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {  
            players[i] = new PokerPlayer();
            players[i].SeatPos = i;
            players[i].PlayerState = PlayerState.Playing;
        }
    }

    public void EvaluatePlayersOnShowdown()
    {
        List<PokerPlayer> sortedPlayers = new List<PokerPlayer>(players.OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
        sortedPlayers[0].PlayerState = PlayerState.Winner;
        List<List<PokerPlayer>> PlayerRank = new List<List<PokerPlayer>>();
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (PlayerRank.Count == 0)
            {
                PlayerRank.Add(new List<PokerPlayer>() { sortedPlayers[i] });
            }
            else if (sortedPlayers[i].Hand.HandValues.PokerHand == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.PokerHand)
            {
                if(sortedPlayers[i].Hand.HandValues.Total == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.Total)
                {
                    if(sortedPlayers[i].Hand.HandValues.HighCard == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.HighCard)
                    {
                        PlayerRank[PlayerRank.Count - 1].Add(sortedPlayers[i]);
                    }
                    else 
                    {
                        PlayerRank.Add(new List<PokerPlayer>() { sortedPlayers[i] });
                    }
                }
                else 
                {
                    PlayerRank.Add(new List<PokerPlayer>() { sortedPlayers[i] });
                }
                
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

    public void ResetPlayerStatus()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].PlayerState = PlayerState.Playing;
        }
    }

}
