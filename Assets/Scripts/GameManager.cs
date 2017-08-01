using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class GameManager : MonoBehaviour
{

    //holds all the cards where they need to be
    public List<PokerPlayer> players = new List<PokerPlayer>();

    PokerPlayer player0;
    PokerPlayer player1;
    PokerPlayer player2;
    PokerPlayer player3;
    PokerPlayer player4;
    private List<PokerPlayer> sortedPlayers = new List<PokerPlayer>();
    public GameObject MessageBoard;
    TextMesh messageText;
    public Hand hand1;
    public Hand hand2;


    private int numberOfWinners;
    private int potAmountToGiveWinner;
    private bool winnersHaveBeenPaid;
    private bool playersHaveBeenEvaluated;
    [HideInInspector]
    public int winnersPaid;
    void Awake()
    {
        messageText = MessageBoard.GetComponent<TextMesh>();

        Services.PrefabDB = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
        Services.GameManager = this;
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
        Table.gameState = GameState.PreFlop;
        Table.dealerState = DealerState.DealingState;
    }

    // Update is called once per frame
    void Update()
    {
        if(Table.gameState != GameState.ShowDown)
        {
            switch (Table.instance._board.Count)
            {
                case 0:
                    Table.gameState = GameState.PreFlop;
                    messageText.text = "Shuffle Up and Deal!";
                    break;
                case 3:
                    Table.gameState = GameState.Flop;
                    messageText.text = "player0 handStrength is " + players[0].HandStrength +
                                       "\n player1 handStrength is " + players[1].HandStrength +
                                       "\n player2 handStrength is " + players[2].HandStrength +
                                       "\n player3 handStrength is " + players[3].HandStrength +
                                       "\n player4 handStrength is " + players[4].HandStrength;
                    break;
                case 4:
                    Table.gameState = GameState.Turn;
                    messageText.text = "ready for the river";
                    break;
                case 5:
                    Table.gameState = GameState.River;
                    break;
                default:
                    break;
            }
        }
        #region Players evaluate their hands based on the gamestate
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Table.instance.DebugHands();
            Debug.Log(players[0].ChipCount);
        }
        
        if(Table.gameState == GameState.PreFlop)
        {
            foreach(PokerPlayer player in players)
            {
                if(player.Hand != null)
                {
                    if (player.Hand.Cards.Count == 2)
                    {
                        player.EvaluateHandPreFlop();
                    }
                }
            }
        }
        if(Table.gameState == GameState.Flop)
        {
            foreach (PokerPlayer player in players)
            {
                player.EvaluateHandOnFlop();
            }
        }
        if(Table.gameState == GameState.Turn)
        {
            foreach(PokerPlayer player in players)
            {
                player.EvaluateHandOnTurn();
            }
        }
        if(Table.gameState == GameState.River)
        {
            foreach(PokerPlayer player in players)
            {
                player.EvaluateHandOnRiver();
            }
            messageText.text = "Click both trigger buttons to start the showdown";
            if (hand1.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true && hand2.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true)
            {
                Table.gameState = GameState.ShowDown;
            }
        }
        if(Table.gameState == GameState.ShowDown)
        {
            if (!playersHaveBeenEvaluated)
            {
                messageText.text = "Give the winner(s) their winnings (pot size is 100, that's a black chip)";
                EvaluatePlayersOnShowdown(players);
            }
        }
        #endregion
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    players[0].DetermineHandStrength(Table.instance.playerCards[0][0].cardType, Table.instance.playerCards[0][1].cardType);
        //}


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

    public void EvaluatePlayersOnShowdown(List<PokerPlayer> playersToEvaluate)
    {
        List<PokerPlayer> sortedPlayers = new List<PokerPlayer>(playersToEvaluate.OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));

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
                if (sortedPlayers[i].Hand.HandValues.Total == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.Total)
                {
                    if (sortedPlayers[i].Hand.HandValues.HighCard == PlayerRank[PlayerRank.Count - 1][0].Hand.HandValues.HighCard)
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
                    player.ChipCountToCheckWhenWinning = player.ChipCount;
                    numberOfWinners++;
                }
                sortedPlayers[i].PlayerState = PlayerState.Winner;
                sortedPlayers[i].ChipCountToCheckWhenWinning = sortedPlayers[i].ChipCount;
                numberOfWinners++;
            }
            else
            {
                foreach (PokerPlayer player in PlayerRank[i])
                {
                    player.PlayerState = PlayerState.Loser;
                }
                sortedPlayers[i].PlayerState = PlayerState.Loser;
            }
        }


        //for (int i = 0; i < playersToEvaluate.Count; i++)
        //{
        //    playersToEvaluate[i].FlipCards();
        //    Debug.Log("player" + playersToEvaluate[i].SeatPos + "is the " + playersToEvaluate[i].PlayerState + " with (a) " + playersToEvaluate[i].Hand.HandValues.PokerHand + " with a highCard of " + playersToEvaluate[i].Hand.HandValues.HighCard + " and a handTotal of " + playersToEvaluate[i].Hand.HandValues.Total);
        //}

        //sortedPlayers.Clear();
        //playersHaveBeenEvaluated = true;
        //StartCoroutine(WaitForWinnersToGetPaid());
    }

    IEnumerator WaitForWinnersToGetPaid()
    {
        //Debug.Log("how do IEnumerators actually work lol");
        while (!winnersHaveBeenPaid)
        {
            GivePlayersWinnings();
            //Debug.Log("Winner is Waiting to be paid");
            yield return null;
        }
        if (winnersHaveBeenPaid)
        {
            messageText.text = "'Thanks dealer, here's a tip!' (you got a tip)";
            //Debug.Log("You gave the money to the right people! yay! let's play again!");
        }

    }

    public void GivePlayersWinnings()
    {
        potAmountToGiveWinner = Table.instance.PotChips / numberOfWinners;
        int winnerChipStack = 0;    
        foreach (PokerPlayer player in players)
        {
            if(player.PlayerState == PlayerState.Winner)
            {
                winnerChipStack = player.ChipCountToCheckWhenWinning + potAmountToGiveWinner;
                Debug.Log("for player" + player.SeatPos + " the winnerChipStack = " + winnerChipStack + " and the Player has" + player.ChipCount);
                if (player.ChipCount == winnerChipStack && player.HasBeenPaid == false)
                {
                    winnersPaid++;
                    player.HasBeenPaid = true;
                    //Debug.Log("winnersPaid = " + winnersPaid);
                   // Debug.Log("player.HasBeenPaid = " + player.HasBeenPaid);
                }
            }
            else if(player.PlayerState == PlayerState.Loser)
            {
                winnerChipStack = player.ChipCountToCheckWhenWinning + potAmountToGiveWinner;
                if(player.ChipCount == winnerChipStack)
                {
                    messageText.text = "I think you messed up, I didn't win this money.";
                }
                else
                {
                    messageText.text = "Give the winner(s) their winnings (pot size is 100, that's a black chip)";
                }
            }
        }
        if (winnersPaid == numberOfWinners)
        {
            //Debug.Log("winnersPaid == numberOfWinners");
            winnersHaveBeenPaid = true;
            winnersPaid = 0;
            numberOfWinners = 0;
        }
    }

    public void ResetPlayerStatus()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].PlayerState = PlayerState.Playing;
            players[i].HasBeenPaid = false;
        }
        playersHaveBeenEvaluated = false;
        winnersHaveBeenPaid = false;
    }

}
