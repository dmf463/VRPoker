using System.Collections;
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
            messageText.text = "Deal two cards to each player!";
            for (int playerCardIndex = 0; playerCardIndex < Table.instance.playerCards.Length; playerCardIndex++)
            {
                for (int cardTotal = 0; cardTotal < Table.instance.playerCards[playerCardIndex].Count; cardTotal++)
                {
                    cardCount++;
                }
            }
            //Debug.Log("newRound cardCount = " + cardCount);
            if (cardCount == players.Count * 2)
            {
                Table.gameState = GameState.PreFlop;
            }
        }
        else if (Table.gameState == GameState.CleanUp)
        {
            messageText.text = "I guess everyone folded, woohoo!";
        }
        else if (Table.gameState == GameState.PostHand)
        {
            messageText.text = "'Thanks dealer, here's a tip!' (you got a tip)";
        }
        else if (Table.gameState != GameState.ShowDown)
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
            Table.instance.DebugHandsAndChips();
        }

        if (playersReady && Table.gameState != lastGameState)
        {
            roundStarted = false;
            playersReady = false;
        }

        if (Table.gameState == GameState.PreFlop)
        {
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
            //messageText.text = "Click both trigger buttons to start the showdown";
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
                    if (players[i].Hand != null)
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
        yield return new WaitForSeconds(.5f);
        playerToAct.turnComplete = false;
        int currentPlayerSeatPos = playerToAct.SeatPos;
        bool roundFinished = true;
        PokerPlayer nextPlayer = null;
        for (int i = 1; i < players.Count; i++)
        {
            nextPlayer = players[(currentPlayerSeatPos + i) % players.Count];
            if ((!nextPlayer.actedThisRound || nextPlayer.currentBet < LastBet || nextPlayer.ChipCount == 0) && nextPlayer.PlayerState == PlayerState.Playing)
            {
                roundFinished = false;
                Debug.Log("nextPlayer to act is player " + nextPlayer.SeatPos);
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
        Debug.Log(players[SeatsAwayFromDealer(1)].ChipCount);
        players[SeatsAwayFromDealer(2)].Bet(BigBlind);
        Debug.Log(players[SeatsAwayFromDealer(2)].ChipCount);
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
            }
            else
            {
                foreach (PokerPlayer player in PlayerRank[i])
                {
                    player.PlayerState = PlayerState.Loser;
                }
            }
        }
        numberOfWinners = PlayerRank[0].Count;
    }

    public IEnumerator WaitForWinnersToGetPaid()
    {
        //need to figure out how much to give each player, and assign that value to them BEFORE I go into the while loop
        //need to be able to find whether or not I need to award the odd chip
        //it could be that the player next to the dealer gets an extra 5,
        //or that the TWO players next to the dealer get an extra 5, in a three way split.
        //so first I need to add the winning players to a list
        //once I have the list I need to take the pot and divide it by the number of winners
        //then for the first winner I find the remainder, subract that from the divided pot, and add the lowest chip denominator, that becomes the amount I give them
        //I then do this for each subsequent winner (if there are more than one). 
        //these values become how much they are supposed to be paid.
        Debug.Assert(numberOfWinners > 0);
        List<PokerPlayer> winningPlayers = new List<PokerPlayer>();
        int potAmount = Table.instance.PotChips;
        for (int i = 0; i < players.Count; i++)
        {
            PokerPlayer playerToCheck = players[SeatsAwayFromDealer(i + 1)];
            if(playerToCheck.PlayerState == PlayerState.Winner)
            {
                winningPlayers.Add(playerToCheck);
            }
        }
        int potRemaining = potAmount;
        for (int i = 0; i < winningPlayers.Count; i++)
        {
            potAmountToGiveWinner = potRemaining / (numberOfWinners - i);
            int remainder = potAmountToGiveWinner % ChipConfig.RED_CHIP_VALUE;
            if (remainder > 0)
            {
                winningPlayers[i].chipsWon = (potAmountToGiveWinner - remainder) + ChipConfig.RED_CHIP_VALUE;
            }
            else winningPlayers[i].chipsWon = potAmountToGiveWinner;
            potRemaining -= winningPlayers[i].chipsWon;
        }
        Debug.Log("number of Winners is " + numberOfWinners);
        while (!winnersHaveBeenPaid)
        {
            GivePlayersWinnings();
            yield return null;
        }
        Table.gameState = GameState.PostHand;
    }

    public void GivePlayersWinnings()
    {
        int winnerChipStack = 0;    
        foreach (PokerPlayer player in players)
        {
            if(player.PlayerState == PlayerState.Winner)
            {
                //Debug.Log("chipCountToCheckWhenWinning = " + player.ChipCountToCheckWhenWinning + " and potAmountToGiveWinner = " + potAmountToGiveWinner);
                winnerChipStack = player.ChipCountToCheckWhenWinning + player.chipsWon;
                Debug.Log("for player" + player.SeatPos + " the winnerChipStack = " + winnerChipStack + " and the Player has" + player.ChipCount);
                if (player.ChipCount == winnerChipStack && player.HasBeenPaid == false)
                {
                    winnersPaid++;
                    player.HasBeenPaid = true;
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
                    messageText.text = "Give the winner(s) their winnings. the pot size is " + Table.instance.PotChips + " and there is/are " + numberOfWinners + " winner(s)";
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
        Table.gameState = GameState.NewRound;
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
