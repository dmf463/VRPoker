﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Valve.VR;
using Valve.VR.InteractionSystem;


public class Dealer : MonoBehaviour
{

    //holds all the cards where they need to be
    public List<PokerPlayer> players = new List<PokerPlayer>();
    public List<Destination> playerDestinations = new List<Destination>
    {
        Destination.player0, Destination.player1, Destination.player2, Destination.player3, Destination.player4
    };

    private List<PokerPlayer> sortedPlayers = new List<PokerPlayer>();
    public GameObject MessageBoard;
    public TextMesh messageText;
    public Hand hand1;
    public Hand hand2;

    const int PLAYER_COUNT = 5;


    public int numberOfWinners;
    private int potAmountToGiveWinner;
    private bool winnersHaveBeenPaid;
    private bool playersHaveBeenEvaluated;
    private bool readyToAwardPlayers;
    [HideInInspector]
    public int winnersPaid;

    public int SmallBlind = 5;
    public int BigBlind = 10;
    public int LastBet;
    public bool roundStarted = false;
    public bool playersReady = true;
    public GameState lastGameState;
    public List<Card> cardsDealt = new List<Card>();

    void Awake()
    {
        messageText = MessageBoard.GetComponent<TextMesh>();

        Services.PrefabDB = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
        Services.Dealer = this;
    }

    // Use this for initialization
    void Start()
    {
        InitializePlayers(3500);
        Table.gameState = GameState.NewRound;
        Table.dealerState = DealerState.DealingState;
        lastGameState = GameState.NewRound;
    }

    // Update is called once per frame
    void Update()
    {

        if (Table.gameState == GameState.NewRound)
        {
            int cardCount = 0;
            for (int playerCardIndex = 0; playerCardIndex < Table.instance.playerCards.Length; playerCardIndex++)
            {
                for (int cardTotal = 0; cardTotal < Table.instance.playerCards[playerCardIndex].Count; cardTotal++)
                {
                    cardCount++;
                }
            }
            Debug.Log("newRound cardCount = " + cardCount);
            if(cardCount == players.Count * 2)
            {
                Table.gameState = GameState.PreFlop;
            }
        }
        if(Table.gameState != GameState.ShowDown)
        {
            switch (Table.instance._board.Count)
            {
                case 0:
                    messageText.text = "player0 chipCount is " + players[0].ChipCount +
                                       "\nplayer1 chipCount is " + players[1].ChipCount +
                                       "\nplayer2 chipCount is " + players[2].ChipCount +
                                       "\nplayer3 chipCount is " + players[3].ChipCount +
                                       "\nplayer4 chipCount is " + players[4].ChipCount +
                                       "\npotSize is at " + Table.instance.DeterminePotSize();
                    break;
                case 3:
                    Table.gameState = GameState.Flop;
                    messageText.text = "player0 handStrength is " + players[0].HandStrength +
                                       "\nplayer1 handStrength is " + players[1].HandStrength +
                                       "\nplayer2 handStrength is " + players[2].HandStrength +
                                       "\nplayer3 handStrength is " + players[3].HandStrength +
                                       "\nplayer4 handStrength is " + players[4].HandStrength;
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
            //Table.instance.DebugHands();
            ////Debug.Log(players[0].ChipCount);
            //int betSizeTest = 5;
            //if (betSizeTest % 5 == 0)
            //{
            //    foreach (PokerPlayer player in players)
            //    {
            //        player.Bet(betSizeTest);
            //    }
            //}
            //else
            //{
            //    Debug.Log("Warning: Invalid Bet Size");
            //}
            //StartRound();
        }

        if (playersReady && Table.gameState != lastGameState)
        {
            roundStarted = false;
            playersReady = false;
        }

        if (Table.gameState == GameState.PreFlop)
        {
            /*
             * at the start of the round, find the first position player and start the round with that person
             * that person acts until they finish, and then they end their turn
             */
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
            }
        }
        if (Table.gameState == GameState.Flop)
        {
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
            }
        }
        if (Table.gameState == GameState.Turn)
        {
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
            }
        }
        if (Table.gameState == GameState.River)
        {
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
            }
            messageText.text = "Click both trigger buttons to start the showdown";
            if (hand1.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true && hand2.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true)
            {
                Table.gameState = GameState.ShowDown;
            }
        }
        if (Table.gameState == GameState.ShowDown)
        {
            if (!playersHaveBeenEvaluated)
            {
                messageText.text = "Give the winner(s) their winnings (pot size is " + Table.instance.PotChips + " , that's a black chip)";
                List<PokerPlayer> playersInHand = new List<PokerPlayer>();
                for (int i = 0; i < players.Count; i++)
                {
                    if(players[i].PlayerState == PlayerState.Playing)
                    {
                        playersInHand.Add(players[i]);
                    }
                }
                EvaluatePlayersOnShowdown(playersInHand);
            }
            if (!readyToAwardPlayers)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if(players[i].PlayerState == PlayerState.Playing)
                    {
                        players[i].FlipCards();
                        Debug.Log("player" + players[i].SeatPos + "is the " + players[i].PlayerState + " with (a) " + players[i].Hand.HandValues.PokerHand + " with a highCard of " + players[i].Hand.HandValues.HighCard + " and a handTotal of " + players[i].Hand.HandValues.Total);
                    }
                }
                sortedPlayers.Clear();
                playersHaveBeenEvaluated = true;
                StartCoroutine(WaitForWinnersToGetPaid());
                readyToAwardPlayers = true;
            }
        }
        #endregion
        lastGameState = Table.gameState;
    }

    public void StartRound()
    {
        /*
         * at the start of the round, find the first position player and start the round with that person
         * that person acts until they finish, and then they end their turn
         * so the first person acts. 
         * when they finish, it proceeds to the next player.
         */
        SetCurrentAndLastBet();
        PokerPlayer firstPlayerToAct;
        if (Table.gameState == GameState.PreFlop)
        {
            firstPlayerToAct = players[SeatsAwayFromDealer(3)];
        }
        else
        {
            firstPlayerToAct = FindFirstPlayerToAct();
        }
        StartCoroutine(playerAction(firstPlayerToAct));
    }

    public PokerPlayer FindFirstPlayerToAct()
    {
        PokerPlayer player;
        player = players[SeatsAwayFromDealer(1)];
        if(player.PlayerState == PlayerState.NotPlaying)
        {
            for (int i = 0; i < players.Count; i++)
            {
                PokerPlayer nextPlayer = players[(player.SeatPos + i) % players.Count];
                if(nextPlayer.PlayerState == PlayerState.Playing)
                {
                    player = nextPlayer;
                    break;
                }
            }
        }
        return player;
    }

    IEnumerator playerAction(PokerPlayer playerToAct)
    {
        playerToAct.EvaluateHand();
        while (!playerToAct.turnComplete)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1);
        playerToAct.turnComplete = false;
        int currentPlayerSeatPos = playerToAct.SeatPos;
        bool roundFinished = true;
        PokerPlayer nextPlayer = null;
        for (int i = 1; i < players.Count; i++)
        {
            nextPlayer = players[(currentPlayerSeatPos + i) % players.Count];
            if ((!nextPlayer.actedThisRound || nextPlayer.currentBet < LastBet) && nextPlayer.PlayerState == PlayerState.Playing)
            {
                roundFinished = false;
                break;
            }
        }
        if (!roundFinished) StartCoroutine(playerAction(nextPlayer));
        else playersReady = true;
        yield break;
        //check if all players have acted && all players have met last bet
        //      if yes, move to next round
        //      else, move to next player
    }

    public void InitializePlayers(int chipCount)
    {
        for (int i = 0; i < PLAYER_COUNT; i++)
        {
            players.Add(new PokerPlayer(i));
            List<GameObject> startingStack  = players[i].SetChipStacks(chipCount);
            foreach(GameObject chip in startingStack)
            {
                Table.instance.playerChipStacks[i].Add(chip.GetComponent<Chip>());
            }
            players[i].CreateAndOrganizeChipStacks(startingStack);
        }
        Table.instance.DealerPosition = 0;
        Table.instance.SetDealerButtonPos(Table.instance.DealerPosition);
        players[SeatsAwayFromDealer(1)].Bet(SmallBlind);
        players[SeatsAwayFromDealer(2)].Bet(BigBlind);
    }

    public void SetCurrentAndLastBet()
    {
        if(Table.gameState == GameState.PreFlop)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].currentBet = 0;
                players[i].actedThisRound = false;
            }
            players[SeatsAwayFromDealer(1)].currentBet = SmallBlind;
            players[SeatsAwayFromDealer(2)].currentBet = BigBlind;
            LastBet = BigBlind;
        }
        else
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].currentBet = 0;
                players[i].actedThisRound = false;
                LastBet = 0;
            }
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
                }
                //sortedPlayers[i].PlayerState = PlayerState.Winner;
                //sortedPlayers[i].ChipCountToCheckWhenWinning = sortedPlayers[i].ChipCount;
                //numberOfWinners++;
            }
            else
            {
                foreach (PokerPlayer player in PlayerRank[i])
                {
                    player.PlayerState = PlayerState.Loser;
                }
                //sortedPlayers[i].PlayerState = PlayerState.Loser;
            }
        }
        numberOfWinners = PlayerRank[0].Count;
    }

    public IEnumerator WaitForWinnersToGetPaid()
    {
        int potAmount = Table.instance.PotChips;
        Debug.Log("number of Winners is " + numberOfWinners);
        while (!winnersHaveBeenPaid)
        {
            GivePlayersWinnings(potAmount);
            yield return null;
        }
        messageText.text = "'Thanks dealer, here's a tip!' (you got a tip)";

    }

    public void GivePlayersWinnings(int winnings)
    {
        Debug.Assert(numberOfWinners > 0);
        potAmountToGiveWinner = winnings / numberOfWinners;
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
                    messageText.text = "Give the winner(s) their winnings (pot size is " + winnings + ", that's a black chip)";
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
            //players[i].checkedHandStrength = false;
        }
        playersHaveBeenEvaluated = false;
        winnersHaveBeenPaid = false;
        readyToAwardPlayers = false;
    }

    public int SeatsAwayFromDealer(int distance)
    {
        int seatPos;
        seatPos = (Table.instance.DealerPosition + distance) % players.Count;
        return seatPos;
    }

}
