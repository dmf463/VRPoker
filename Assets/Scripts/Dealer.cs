using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Valve.VR;
using Valve.VR.InteractionSystem;

//this entire script is essentially what the dealer does
//the three most important scripts are Dealer, Table, and PokerPlayerRedux
//Dealer handles anything a dealer would do at a table
//Table pretty much holds all the cards and any object. if it's at the table, it's in Table
//PokerPlayerRedux handles all the functions and info that a poker player would need to play
public class Dealer : MonoBehaviour
{

    //this is the players List, this is essentially the current list that handles ALL the players at the poker table
    //you'll notice that EVERY function dealing with the poker players is using the players list. 
    //while this is useful for this version of the prototype, it will need to be changed in order to accomodate for 
    //specific character-players that derive from the PokerPlayerRedux class
    //we'll need to discuss how that might look
	public List<PokerPlayerRedux> players = new List<PokerPlayerRedux>();

    public PokerPlayerRedux playerToAct;

    //the list of destinations, so that I could make for-loops where the seatNum corresponds to the destination num in the list
    private List<Destination> playerDestinations = new List<Destination>();

    //this is the list used during evaluation to sort players from best hand to worst
    //private List<PokerPlayerRedux> sortedPlayers = new List<PokerPlayerRedux>();

    //the board and the text are pretty much placeholders to give messages to the player via text
    public GameObject MessageBoard;
    public TextMesh messageText;

    //these are references to the two players hands
    //we need these in order to know which hand is which
    //but that should also probably change, because right now, it's a little awkward the way we're referencing hands
    public Hand hand1;
    public Hand hand2;

    //this is the number of players in the game currently
    //we can change this to add more or less players, max 5
    //this won't be necessary when we make PokerPlayerRedux abstract
    //public int playerCount = 5;

    //this int tells us how many winners there are in a given hand
    //more often than not it's 1, but it can technically be as many people that are in the hand
    public int numberOfWinners;

    //this is the amount each player is supposed to recieve
    //basically it's the pot amount divides by the number of winners
    private int potAmountToGiveWinner;

    //this is our bool to check whether every person who was supposed to be paid DID get paid
    //this is in place to end the while loop in the coroutine
    private bool winnersHaveBeenPaid;

    //this is true when player's have been evaluated and is in place in order to make sure the function is called only once
    public bool playersHaveBeenEvaluated;

    //this is another way to ensure that we are ready to award the players and make sure everything is in flow
    //I feel like I have so many bools
    [HideInInspector]
    public bool readyToAwardPlayers = false;

    //this is how many people have been paid
    //if winners paid is equal to the number of winners, then you've paid everyone
    [HideInInspector]
    public int winnersPaid;

    //this is where we control our small and big blind
    //right now the blinds don't raise, but we'll have to make a function that does that
    public int SmallBlind = 5;
    public int BigBlind = 10;

    //this is the value of the last bet that was put on the table
    public int LastBet;

    //this lets us know when a round has started, so that each function for the round is only called once
    public bool roundStarted = false;

    //this is basically our insurance that no round can start unless all players have acted and are ready
    public bool playersReady = true;

    //this keeps track of what the lastGameState
    //this is what resets the previous bools, it indicates that a new round is in session
    //basically, if the lastGameState is not equal to the current game state, then we know we're in a new round
	[HideInInspector]
    public GameState lastGameState;

    //this is the bool that ensures all the functions involved with the showdown only happen once
    private bool readyForShowdown = false;

    void Awake()
    {
        //just the message board stuff
        messageText = MessageBoard.GetComponent<TextMesh>();

        //this is where we intialize all our services stuff
        Services.PrefabDB = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
        Services.SoundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        Services.Dealer = this;
        Services.PokerRules = GameObject.Find("PokerRules").GetComponent<PokerRules>();
    }

    // Use this for initialization
    void Start()
    {
        //in start we're just setting up the game. 
        //this will technically be different for any "scene" we're in 
        //so we may have to make this more modular and allowed to be accessed through the inspector
        //for easy scene creation
        playerDestinations = Table.instance.playerDestinations;
        InitializePlayers(3500);
        Table.gameState = GameState.NewRound;
		Debug.Log("Gamestate = " + Table.gameState);
        Table.dealerState = DealerState.DealingState;
        lastGameState = GameState.NewRound;
    }

    // Update is called once per frame
    void Update()
    {
        //in update we're essentially controlling the gamestates
        //the game states are determined by how many cards are on "the board"
        //right now this is a little weird, because if a card accidentally hits the board before it goes into a space it counts towards
        //the board and the gameState
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
            if (cardCount == GetActivePlayerCount() * 2)
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
                    messageText.text = "player0 chipCount is " + players[0].ChipCount +
                                       "\nplayer1 chipCount is " + players[1].ChipCount +
                                       "\nplayer2 chipCount is " + players[2].ChipCount +
                                       "\nplayer3 chipCount is " + players[3].ChipCount +
                                       "\nplayer4 chipCount is " + players[4].ChipCount +
                                       "\npotSize is at " + Table.instance.DeterminePotSize();
                    break;
                case 4:
                    Table.gameState = GameState.Turn;
                    messageText.text = "player0 chipCount is " + players[0].ChipCount +
                   "\nplayer1 chipCount is " + players[1].ChipCount +
                   "\nplayer2 chipCount is " + players[2].ChipCount +
                   "\nplayer3 chipCount is " + players[3].ChipCount +
                   "\nplayer4 chipCount is " + players[4].ChipCount +
                   "\npotSize is at " + Table.instance.DeterminePotSize();
                    break;
                case 5:
                    Table.gameState = GameState.River;
                    messageText.text = "player0 chipCount is " + players[0].ChipCount +
                   "\nplayer1 chipCount is " + players[1].ChipCount +
                   "\nplayer2 chipCount is " + players[2].ChipCount +
                   "\nplayer3 chipCount is " + players[3].ChipCount +
                   "\nplayer4 chipCount is " + players[4].ChipCount +
                   "\npotSize is at " + Table.instance.DeterminePotSize();
                    break;
                default:
                    break;
            }
        }
        #region Players evaluate their hands based on the gamestate
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.callP1);
            //Table.instance.DebugHandsAndChips();
            Debug.Log("CardsPulled = " + Services.PokerRules.cardsPulled.Count);
            Services.PokerRules.CorrectMistakes();
        }

        //this resets bools necessary to start new rounds
        //once both of these are true, then the next round will start
        if (playersReady && Table.gameState != lastGameState)
        {
            roundStarted = false;
            playersReady = false;
        }

        //starts the round for pre-flop
        if (Table.gameState == GameState.PreFlop)
        {
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
            }
        }
        
        //starts next round
        if (Table.gameState == GameState.Flop)
        {
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
            }
        }

        //starts next round
        if (Table.gameState == GameState.Turn)
        {
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
            }
        }
        
        //starts the river
        //we put a coroutine here to make it so that you couldn't trigger the showdown accidentally
        //when the three seconds end, readyForShowdown sets to true and the player can trigger it
        if (Table.gameState == GameState.River)
        {
            if (!roundStarted)
            {
                StartRound();
                roundStarted = true;
                readyForShowdown = false;
                StartCoroutine(WaitForShowDown(2));
            }
            //messageText.text = "Click both trigger buttons to start the showdown";
            int allInPlayerCount = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerIsAllIn == true) allInPlayerCount++;
            }
            if (GetActivePlayerCount() == allInPlayerCount) readyForShowdown = true;
            if(readyForShowdown == true)
            {
                if (hand1.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true && hand2.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true)
                {
                    Table.gameState = GameState.ShowDown;
                }
            }
        }

        //so here, if's the showdown, and the players have not already been evaluated
        //we add all the players in the hand to a new list and then pass that list to the proper evaluate function
        //once that's done, we get readyToAwardPlayers and each player flips their cards
        //we then clear the sortedPlayer list, because reasons?
        //and we start the coroutine that puts the players in idle while they await their winnings
        //finally, once that's done, we set the lastGameState to the current table state
        if (Table.gameState == GameState.ShowDown)
        {
            if (!playersHaveBeenEvaluated)
            {
                messageText.text = "Give the winner(s) their winnings (pot size is " + Table.instance.PotChips + " , that's a black chip)";
                List<PokerPlayerRedux> playersInHand = new List<PokerPlayerRedux>();
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
                //sortedPlayers.Clear();
                playersHaveBeenEvaluated = true;
                StartCoroutine(WaitForWinnersToGetPaid());
                readyToAwardPlayers = true;
            }
        }
        #endregion
        lastGameState = Table.gameState;
    }



    //this is an ease of life function for finding the amount of active players in a round
    public int GetActivePlayerCount()
    {
        int activePlayers = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].PlayerState == PlayerState.Playing)
            {
                activePlayers++;
            }
        }
        return activePlayers;
    }

    //we call this to start a new round
    //this sets which player should be the first to act
    //if it's preflop, it's always the player after the big blind (3 seats away from the dealer button)
    //if not, we call the function to find the first player
    //then we start the coroutine to make player's actually act on that first player
    public void StartRound()
    {
        SetCurrentAndLastBet();
        if (Table.gameState == GameState.PreFlop)
        {
            playerToAct = FindFirstPlayerToAct(3);
        }
        else
        {
            playerToAct = FindFirstPlayerToAct(1);
            Debug.Log("player to act = " + playerToAct);
        }
        playerToAct.playerSpotlight.SetActive(true);
        //StartCoroutine(playerAction(playerToAct));
    }


    //this finds the player who is supposed to act first
    //we find the person after the dealer button
    //if that player is NotPlaying, then we find the next possible person to act
    //in either case, we return the PokerPlayerRedux
    public PokerPlayerRedux FindFirstPlayerToAct(int distance)
    {
        PokerPlayerRedux player;
        player = players[SeatsAwayFromDealer(distance)];
        if(player.PlayerState == PlayerState.NotPlaying)
        {
            for (int i = 0; i < players.Count; i++)
            {
                PokerPlayerRedux nextPlayer = players[(player.SeatPos + i) % players.Count];
                if(nextPlayer.PlayerState == PlayerState.Playing)
                {
                    player = nextPlayer;
                    break;
                }
            }
        }
        return player;
    }


    //this is just a way to give the player a little buffer time so they don't accidentally trigger the showdown
    IEnumerator WaitForShowDown(float time)
    {
        yield return new WaitForSeconds(time);
        readyForShowdown = true;
    }

    //this is actually a pretty simple function
    //so we pass it either the firstPlayerToAct or nextPlayer
    //we have that player evaluate their hand, and until their turn is complete, the coroutine pretty much pauses
    //we wait for .5 seconds for shits and giggles
    //we then set that players turnComplete to false, in case they need to act again
    //we keep track of who the current player is and set round finished to true, in case the round is ACTUALLY finished
    //then we figure out the next player
    //the if statement is essentially a way to check, whether the round ends or not
    //if everyone has acted, all the people that are playing are still playing
    //and all the bets have been met, then we end the round
    //if not, then someone else needs to act, to we call this function and pass the nextPlayer
    IEnumerator playerAction(PokerPlayerRedux playerToAct)
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
        PokerPlayerRedux nextPlayer = null;
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
    }


    //this gets invoked whenever we gaze at a player
    public void SetNextPlayer()
    {
        Debug.Log("current playerToAct = " + playerToAct);
        int currentPlayerSeatPos = playerToAct.SeatPos;
        playerToAct.playerSpotlight.SetActive(false);
        bool roundFinished = true;
        PokerPlayerRedux nextPlayer = null;
        for (int i = 1; i < players.Count; i++)
        {
            nextPlayer = players[(currentPlayerSeatPos + i) % players.Count];
            if(nextPlayer.PlayerState == PlayerState.Playing)
            {
                Debug.Log("nextPlayer = " + nextPlayer.name);
                Debug.Log("nextPlayer.actedThisRound = " + nextPlayer.actedThisRound);
                Debug.Log("nextPlayer.currentBet = " + nextPlayer.currentBet + " and lastBet = " + LastBet);
                Debug.Log("nextPlayer.chipCount = " + nextPlayer.ChipCount);
                Debug.Log("nextPlayer.PlayerState = " + nextPlayer.PlayerState);
            }
            if ((!nextPlayer.actedThisRound || nextPlayer.currentBet < LastBet || nextPlayer.ChipCount == 0) && nextPlayer.PlayerState == PlayerState.Playing)
            {
                roundFinished = false;
                playerToAct = nextPlayer;
                playerToAct.playerSpotlight.SetActive(true);
                Debug.Log("nextPlayer to act is player " + playerToAct);
                break;
            }
        }
        if (roundFinished)
        {
            Debug.Log(Table.gameState + " Finished");
            playerToAct.playerSpotlight.SetActive(false);
            playerToAct = null;
            playersReady = true;
        }
    }


    //this sets all the players by adding them to the player list
    //setting their starting stack
    //adding their chips to the proper list
    //setting the dealer position
        //WHICH NOTE
            //this will have to change. 
            //when we choose who has the dealer button, we do it by flipping cards, and the player with the highest card is the dealer
            //this should be mirrored in the game
            //so we'll probably have to run a coroutine here in order to determine who the dealer is, accounting for player action
    //and then have the big and small blinds bet their chips
    public void InitializePlayers(int chipCount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            //players.Add(new PokerPlayerRedux(i));
			players[i].SeatPos = i;
			players[i].PlayerState = PlayerState.Playing;
            List<GameObject> startingStack  = players[i].SetChipStacks(chipCount);
            foreach(GameObject chip in startingStack)
            {
                Table.instance.AddChipTo(playerDestinations[i], chip.GetComponent<Chip>());
            }
            players[i].CreateAndOrganizeChipStacks(startingStack);
        }
        Table.instance.DealerPosition = 0;
        Table.instance.SetDealerButtonPos(Table.instance.DealerPosition);
        for (int i = 0; i < 5; i++) {
            Debug.Log(players[SeatsAwayFromDealer(i)].playerDestinations.Count);
        }

        players[SeatsAwayFromDealer(1)].Bet(SmallBlind);
        players[SeatsAwayFromDealer(2)].Bet(BigBlind);
    }

    //we call this at the beginning of each round in order to set the blinds as the current and last bet
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

    //okay, so here we evaluate the players on the showdown
    //we organize all the players into a sorted list, ranked by hand rank
    //but then we need to see if anyone has the same hand, cause this is important for split pots and side pots
    //so we check the person ranked highest, who is always going to be first
    //and we compare the second.
    //if they're the same cards, they get added to the same list in a list of player ranks
    //therefore the first list in the playerRank list is ALWAYS the list of winners
    //then we just move down and add players to subsequent lists as they have worse and worse hands
    public void EvaluatePlayersOnShowdown(List<PokerPlayerRedux> playersToEvaluate)
    {
        List<PokerPlayerRedux> sortedPlayers = new List<PokerPlayerRedux>(playersToEvaluate.
                                                                          OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).
                                                                          ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).
                                                                          ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));

        sortedPlayers[0].PlayerState = PlayerState.Winner;

        List<List<PokerPlayerRedux>> PlayerRank = new List<List<PokerPlayerRedux>>();

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            if (PlayerRank.Count == 0)
            {
                PlayerRank.Add(new List<PokerPlayerRedux>() { sortedPlayers[i] });
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
                        PlayerRank.Add(new List<PokerPlayerRedux>() { sortedPlayers[i] });
                    }
                }
                else
                {
                    PlayerRank.Add(new List<PokerPlayerRedux>() { sortedPlayers[i] });
                }
            }
            else
            {
                PlayerRank.Add(new List<PokerPlayerRedux>() { sortedPlayers[i] });
            }
        }
        for (int i = 0; i < PlayerRank.Count; i++)
        {
            if (i == 0)
            {
                foreach (PokerPlayerRedux player in PlayerRank[0])
                {
                    player.PlayerState = PlayerState.Winner;
                    player.ChipCountToCheckWhenWinning = player.ChipCount;
                }
            }
            else
            {
                foreach (PokerPlayerRedux player in PlayerRank[i])
                {
                    player.PlayerState = PlayerState.Loser;
                }
            }
        }
        numberOfWinners = PlayerRank[0].Count;
    }

    //this is the coroutine where we check how much each player is supposed to get paid
    //whether they get paid
    //and waits for us to pay them
    //basically I need to know how much the winner has BEFORE they get paid, and the coroutine is constantly checking
    //whether THAT number, plus whatever winnings they were supposed to receive is equal to their current pot value
    //when it IS then we know that player has been paid
    public IEnumerator WaitForWinnersToGetPaid()
    {
        Debug.Assert(numberOfWinners > 0);
        List<PokerPlayerRedux> winningPlayers = new List<PokerPlayerRedux>();
        int potAmount = Table.instance.PotChips;
        for (int i = 0; i < players.Count; i++)
        {
            PokerPlayerRedux playerToCheck = players[SeatsAwayFromDealer(i + 1)];
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
        //added this in because of voiceActing, and not wanting two clips playing at the same time
        if(winningPlayers.Count == 2)
        {
            Debug.Log("DONT SAY A FUCKING WORD");
        }
        else
        {
            float randomNum = UnityEngine.Random.Range(0, 100);
            if(randomNum < 50)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if(players[i].PlayerState == PlayerState.Winner)
                    {
                        StartCoroutine(WaitForWinner(2, players[i]));
                    }
                }
            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].PlayerState == PlayerState.Loser)
                    {
                        StartCoroutine(WaitForLoser(2, players[i]));
                    }
                }
            }
        }

        while (!winnersHaveBeenPaid)
        {
            GivePlayersWinnings();
            yield return null;
        }
        Table.gameState = GameState.PostHand;
    }

    //this is out current method for calling the reaction audio for winners
    IEnumerator WaitForWinner(float time, PokerPlayerRedux player)
    {
        yield return new WaitForSeconds(time);
        player.WinnerReactions();
    }

    //this is out current method for calling the reaction audio for losers
    IEnumerator WaitForLoser(float time, PokerPlayerRedux player)
    {
        yield return new WaitForSeconds(time);
        player.LoserReactions();
    }

    //this is the function that actually runs to check whether the winners have gotten paid
    //this gets called like a million times
    public void GivePlayersWinnings()
    {
        int winnerChipStack = 0;    
        foreach (PokerPlayerRedux player in players)
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

    //this resets all the player states and gamestates and resets bools
    //so that the player is ready for each round
    public void ResetPlayerStatus()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ChipCount == 0) players[i].PlayerState = PlayerState.Eliminated;
            if (players[i].PlayerState != PlayerState.Eliminated)
            {
                players[i].PlayerState = PlayerState.Playing;
            }
            players[i].HasBeenPaid = false;
            players[i].playerIsAllIn = false;
            //players[i].checkedHandStrength = false;
        }
        Services.PokerRules.cardsPulled.Clear();
        Table.gameState = GameState.NewRound;
        playersHaveBeenEvaluated = false;
        winnersHaveBeenPaid = false;
        readyToAwardPlayers = false;
    }

    //this is an ease of life function to find how far away from the dealer button a given player is
    public int SeatsAwayFromDealer(int distance)
    {
        int seatPos;
        seatPos = (Table.instance.DealerPosition + distance) % players.Count;
        return seatPos;
    }

}
