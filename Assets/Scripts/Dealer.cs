﻿using System.Collections;
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
    public int startingChipCount;
    public float cardMoveSpeed;
    public bool killingCards = false;
    public bool cleaningCards = false;
    public bool madeNewDeck;
    GameObject newCardDeck;

    [HideInInspector]
    public List<Card> deadCardsList = new List<Card>();
    //we set this to true if we're outside VR so we can text
    public bool OutsideVR = false;

    //TUTORIAL STUFF 
    public bool inTutorial = true;
    public int roundCounter = 1; //which hand are we on? First, second, third.... 
    public bool havePickedUpDeckOnce = false;
    public bool haveShuffledOnce = false;

    [HideInInspector]
    public List<Card> cardsTouchingTable = new List<Card>();

	public List<PokerPlayerRedux> players = new List<PokerPlayerRedux>();
	public List<PokerPlayerRedux> activePlayers = new List<PokerPlayerRedux>();

    public PokerPlayerRedux playerToAct;
    public PokerPlayerRedux previousPlayerToAct;

    private List<Destination> playerDestinations = new List<Destination>();

    //the board and the text are pretty much placeholders to give messages to the player via text
    public GameObject MessageBoard;
    public TextMesh messageText;

    [HideInInspector]
    public bool handIsOccupied;

    //these are references to the two players hands
    //we need these in order to know which hand is which
    public Hand hand1;
    public Hand hand2;
    public int numberOfWinners;
    private int potAmountToGiveWinner;
    private bool winnersHaveBeenPaid;
    public bool playersHaveBeenEvaluated;

    [HideInInspector]
    public bool readyToAwardPlayers = false;

    [HideInInspector]
    public int winnersPaid;

    public int SmallBlind;
    public int BigBlind;

    //this is the value of the last bet that was put on the table
    public int LastBet;
    public bool roundStarted = false;
    public bool playersReady = true;

	[HideInInspector]
    public GameState lastGameState;
    private bool readyForShowdown = false;

    private bool checkedPreFlopCardCount;
    private bool finalHandEvaluation;
    [HideInInspector]
    public List<Chip> chipsInPot = new List<Chip>();

    //ints and bools for a test
    private float buttonATimer = 0;
    private float buttonBTimer = 0;
    private int bufferPeriod = 10;

    public int raisesInRound;

    private float gameLength;

    [HideInInspector]
	public bool misdealAudioPlayed =false;

    void Awake()
    {
        //just the message board stuff
        messageText = MessageBoard.GetComponent<TextMesh>();

        //this is where we intialize all our services stuff
        Services.PrefabDB = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
        Services.SoundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		Services.DialogueDataManager = new DialogueDataManager();
        Services.Dealer = this;
        Services.PokerRules = GameObject.Find("PokerRules").GetComponent<PokerRules>();
		Services.PlayerBehaviour = new PlayerBehaviour();
		Services.DialogueDataManager.ParseDialogueFile((Services.SoundManager.dialogueFile));
    }

    // Use this for initialization
    void Start()
    {
        playerDestinations = Table.instance.playerDestinations;
        InitializePlayers(startingChipCount);
        Table.gameState = GameState.NewRound;
        Debug.Log("Gamestate = " + Table.gameState);
        Table.dealerState = DealerState.DealingState;
        lastGameState = GameState.NewRound;

        if (inTutorial)
        {
            if (roundCounter == 1 && Services.SoundManager.tutorialAudioFiles[0].hasBeenPlayed == false)
            {
                Services.SoundManager.PlayTutorialAudio(0);
            }
            //OutsideVR = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //    if(Input.GetKeyDown(KeyCode.K)) 
        //    { 
        //      //Debug.Log(Services.SoundManager.tutorialAudioFiles.Count); 
        //      if(!Services.SoundManager.tutorialAudioFiles[0].hasBeenPlayed) 
        //      { 
        //        //Debug.Log(Services.SoundManager.tutorialAudioFiles[0].audio); 
        //        Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.tutorialAudioFiles[0].audio, 1f, 1f); 
        //        Services.SoundManager.tutorialAudioFiles[0].hasBeenPlayed = true; 
        //      } 
        //    } 
        if (killingCards)
        {
            GrabAndKillCards();
        }
        if (cleaningCards) CleanUpTable();
        if (inTutorial)
        {
            Services.SoundManager.CheckForTutorialAudioToBePlayed();
        //    if (roundCounter == 1)
        //    {
        //        //player picks up deck for first time 
        //        if (Services.SoundManager.tutorialAudioFiles[0].finishedPlaying &&
        //          havePickedUpDeckOnce &&
        //          Services.SoundManager.tutorialAudioFiles[1].hasBeenPlayed == false)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(1);

        //        }
        //        //player deals face up card to each player 
        //        else if (Services.SoundManager.tutorialAudioFiles[1].finishedPlaying &&
        //          cardsTouchingTable.Count >= 5 &&
        //          !Services.SoundManager.tutorialAudioFiles[2].hasBeenPlayed)
        //        {
        //            int cardsFaceUp = 0;
        //            for (int i = 0; i < cardsTouchingTable.Count; i++)
        //            {
        //                if (cardsTouchingTable[i].cardIsFlipped) cardsFaceUp++;
        //            }
        //            if (cardsFaceUp >= 5)
        //            {
        //                Services.SoundManager.PlayTutorialAudio(2);
        //            }
        //        }
        //        //player placed dealer button in correct place 
        //        else if (Services.SoundManager.tutorialAudioFiles[2].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[3].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(3);
        //        }
        //        //player collects cards into deck 
        //        else if (Services.SoundManager.tutorialAudioFiles[3].finishedPlaying &&
        //          haveShuffledOnce &&
        //          !Services.SoundManager.tutorialAudioFiles[5].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(5);
        //        }
        //        //player deals 2 cards to each character 
        //        else if (Services.SoundManager.tutorialAudioFiles[5].finishedPlaying &&
        //          Table.gameState == GameState.PreFlop &&
        //          !Services.SoundManager.tutorialAudioFiles[6].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(6);
        //        }
        //        //looks at first player 
        //        else if (Services.SoundManager.tutorialAudioFiles[6].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[7].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(7);
        //        }
        //        //round over 
        //        else if (Services.SoundManager.tutorialAudioFiles[7].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[8].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(8);
        //        }
        //        //player puts cards in burn pile 
        //        else if (Services.SoundManager.tutorialAudioFiles[8].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[9].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(9);
        //        }
        //        //puts 3 cards in center 
        //        else if (Services.SoundManager.tutorialAudioFiles[9].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[10].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(10);
        //        }
        //        //round over 
        //        else if (Services.SoundManager.tutorialAudioFiles[10].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[11].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(11);
        //        }
        //        //puts card in center 
        //        else if (Services.SoundManager.tutorialAudioFiles[11].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[12].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(12);
        //        }
        //        //round over 
        //        else if (Services.SoundManager.tutorialAudioFiles[12].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[13].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(13);
        //        }
        //        //end of round 
        //        else if (Services.SoundManager.tutorialAudioFiles[13].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[14].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(14);
        //        }
        //        //player pushes chips to the winner 
        //        else if (Services.SoundManager.tutorialAudioFiles[14].finishedPlaying &&
        //          !Services.SoundManager.tutorialAudioFiles[15].hasBeenPlayed)
        //        {
        //            Services.SoundManager.PlayTutorialAudio(15);
        //        }

        //    }
        }
        IncreaseBlinds();

        buttonATimer--;
        buttonBTimer--;

        if (Table.gameState == GameState.NewRound)
        {
            //Debug.Log("newRound");
            messageText.text = "";
            if (Services.PokerRules.cardsPulled.Count == PlayerAtTableCount() * 2 && !checkedPreFlopCardCount)
            {
                checkedPreFlopCardCount = true;
                StartCoroutine(CheckForMistakesPreFlop(1.5f));
            }
        }
        else if (Table.gameState == GameState.CleanUp)
        {
            //messageText.text = "I guess everyone folded, woohoo!";
        }
        else if(Table.gameState == GameState.Misdeal)
        {
            //Debug.Log("misdeal");
            if (!misdealAudioPlayed)
            {
                misdealAudioPlayed = true;
                int i = UnityEngine.Random.Range(0, players.Count);
                if (!players[i].playerAudioSource.isPlaying &&
                    !players[i].playerIsInConversation &&
                    !Services.SoundManager.conversationIsPlaying)
                {
                    if (!inTutorial)
                    {
                        Services.SoundManager.GetSourceAndPlay(players[i].playerAudioSource, players[i].misdealAudio);
                    }
                }
            }
            //messageText.text = "You misdealt the hand, click both triggers to restart the round.";

            if(hand1.GetStandardInteractionButtonDown()) buttonATimer = bufferPeriod;
            if(hand2.GetStandardInteractionButtonDown()) buttonBTimer = bufferPeriod;
            if(hand1.GetStandardInteractionButtonDown() && buttonBTimer > 0 ||
                hand2.GetStandardInteractionButtonDown() && buttonATimer > 0)
            {
                //Debug.Log("Beginning to restart round");
                //Table.instance.RestartRound();
                killingCards = true;
                GameObject[] cards = GameObject.FindGameObjectsWithTag("PlayingCard");
                foreach(GameObject card in cards)
                {
                    card.GetComponent<Card>().flyingAllowed = true;
                }
            }
        }
        else if (Table.gameState == GameState.PostHand)
        {
            //messageText.text = "'Thanks dealer, here's a tip!' (you got a tip)";
        }
        else if (Table.gameState != GameState.ShowDown)
        {
            switch (Table.gameState)
            {
                case GameState.PreFlop:
                    //Debug.Log("PREFLOP!");
                    if (OutsideVR)
                    {
                        messageText.text = "player0 chipCount is " + players[0].chipCount +
                                           "\nplayer1 chipCount is " + players[1].chipCount +
                                           "\nplayer2 chipCount is " + players[2].chipCount +
                                           "\nplayer3 chipCount is " + players[3].chipCount +
                                           "\nplayer4 chipCount is " + players[4].chipCount +
                                           "\npotSize is at " + Table.instance.potChips;
                    }
                    break;
                case GameState.Flop:
                    //Debug.Log("FLOP!");
                    //messageText.text = "burnCards.count = " + Table.instance._burn.Count;

                    break;
                case GameState.Turn:
                    //Debug.Log("TURN!");
                    //messageText.text = "burnCards.count = " + Table.instance._burn.Count;

                    break;
                case GameState.River:
                    //Debug.Log("RIVER!");
                    //messageText.text = "burnCards.count = " + Table.instance._burn.Count;

                    break;
                default:
                    break;
            }
        }
        #region Players evaluate their hands based on the gamestate
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Table.instance.DebugHandsAndChips();
            cleaningCards = true;
            
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (playerToAct != null)
            {
                playerToAct.EvaluateHand();
            }
        }

        //this resets bools necessary to start new rounds
        //once both of these are true, then the next round will start
        if (playersReady && Table.gameState != lastGameState)
        {
            Debug.Log("starting round");
            roundStarted = false;
            playersReady = false;
        }
        if (playersReady)
        {
            Services.PokerRules.SetCardIndicator();
        }
        //starts the round for pre-flop
        if (Table.gameState == GameState.PreFlop)
        {
            if (!roundStarted)
            {
                roundStarted = true;
                if (!OnlyAllInPlayersLeft()) StartRound();
                else playersReady = true;
            }
            //else Debug.Log("Round is already registered as having started");
        }
        
        //starts next round
        if (Table.gameState == GameState.Flop)
        {
            if (!roundStarted)
            {
                roundStarted = true;
                if (!OnlyAllInPlayersLeft()) StartRound();
                else playersReady = true;
            }
        }

        //starts next round
        if (Table.gameState == GameState.Turn)
        {
            if (!roundStarted)
            {
                Debug.Log("turn Debug should only go once");
                roundStarted = true;
                if (!OnlyAllInPlayersLeft()) StartRound();
                else playersReady = true;
            }
        }
        
        //starts the river
        //we put a coroutine here to make it so that you couldn't trigger the showdown accidentally
        //when the three seconds end, readyForShowdown sets to true and the player can trigger it
        if (Table.gameState == GameState.River)
        {
            if (!roundStarted)
            {
                if (OnlyAllInPlayersLeft() && !finalHandEvaluation)
                {
                    finalHandEvaluation = true;
                    foreach (PokerPlayerRedux player in players)
                    {
                        if (player.PlayerState == PlayerState.Playing)
                        {
                            //player.EvaluateHand();
                            List<CardType> sortedCards = Table.instance.SortPlayerCardsAtRiver(player.SeatPos);
                            HandEvaluator playerHand = new HandEvaluator(sortedCards);
                            playerHand.EvaluateHandAtRiver();
                            player.Hand = playerHand;
                        }
                    }
                }
                //Debug.Log("River Debug should only go once");
                roundStarted = true;
                if (!OnlyAllInPlayersLeft()) StartRound();
                else playersReady = true;
                readyForShowdown = false;
                StartCoroutine(WaitForShowDown(2));
            }
            //messageText.text = "Click both trigger buttons to start the showdown";
            int allInPlayerCount = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerIsAllIn == true) allInPlayerCount++;
            }
            if (GetActivePlayerCount() == allInPlayerCount) Table.gameState = GameState.ShowDown;
            else if (GetActivePlayerCount() - allInPlayerCount == 1 && playersReady) Table.gameState = GameState.ShowDown;
            if(readyForShowdown == true)
            {
                if (!OutsideVR)
                {
                    if (hand1.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true && hand2.controller.GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger) == true)
                    {
                        Table.gameState = GameState.ShowDown;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow)) Table.gameState = GameState.ShowDown;
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
                messageText.text = "Give the winner(s) their winnings (pot size is " + Table.instance.potChips + " , that's a black chip)";
                List<PokerPlayerRedux> playersInHand = new List<PokerPlayerRedux>();
                for (int i = 0; i < players.Count; i++)
                {
                    if(players[i].PlayerState == PlayerState.Playing)
                    {
                        playersInHand.Add(players[i]);
                        players[i].playerSpotlight.SetActive(false);
                    }
                }
                EvaluatePlayersOnShowdown(playersInHand);
                playersHaveBeenEvaluated = true;
            }
            if (!readyToAwardPlayers)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].Hand != null)
                    {
                        if(!players[i].flippedCards) players[i].FlipCards();
                        Debug.Log("player" + players[i].SeatPos + 
                                  "is the " + players[i].PlayerState + 
                                  " with (a) " + players[i].Hand.HandValues.PokerHand + 
                                  " with a highCard of " + players[i].Hand.HandValues.HighCard + 
                                  " and a handTotal of " + players[i].Hand.HandValues.Total);
                    }
                }
                WaitForWinnersToGetPaid();
                readyToAwardPlayers = true;
            }
        }
        #endregion
        lastGameState = Table.gameState;
    }

    IEnumerator WaitToResetBool(float time, AudioData clip)
    {
        yield return new WaitForSeconds(time);
        clip.finishedPlaying = true;
    }

    void IncreaseBlinds()
    {
        gameLength += Time.deltaTime;
        if(gameLength >= 300) //5 minutes
        {
            SmallBlind = 50;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 600) //10 minutes
        {
            SmallBlind = 75;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 900) //15 minutes
        {
            SmallBlind = 100;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 1200) //20 minutes
        {
            SmallBlind = 125;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 1500) //25 minutes
        {
            SmallBlind = 150;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 1800) //30 minutes
        {
            SmallBlind = 175;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 2100) //40 minutes
        {
            SmallBlind = 200;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 2400) //45 minutes
        {
            SmallBlind = 300;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 2700) //50 minutes
        {
            SmallBlind = 400;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 3000) //50 minutes
        {
            SmallBlind = 400;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 3300) //55 minutes
        {
            SmallBlind = 500;
            BigBlind = SmallBlind * 2;
        }
        else if (gameLength >= 3600) //60 minutes
        {
            SmallBlind = 600;
            BigBlind = SmallBlind * 2;
        }
    }

    IEnumerator CheckForMistakesPreFlop(float time)
    {
        Debug.Log("CheckingPreFlopMistakes");
        yield return new WaitForSeconds(time);
        int cardCountForPreFlop = 0;
        bool misdeal = false;
        for (int playerCardIndex = 0; playerCardIndex < Table.instance.playerCards.Length; playerCardIndex++)
        {
            for (int cardTotal = 0; cardTotal < Table.instance.playerCards[playerCardIndex].Count; cardTotal++)
            {
                cardCountForPreFlop++;
            }
        }
        for (int i = 0; i < cardsTouchingTable.Count; i++)
        {
            if (!OutsideVR)
            {
                if (cardsTouchingTable[i].cardFacingUp) misdeal = true;
            }
        }
        if(Services.Dealer.cardsTouchingTable.Count <= (PlayerAtTableCount()/2))
        {
            Debug.Log("Misdeal for there being less than 5 cards on the table");
            Table.gameState = GameState.Misdeal;
        }
        else if(misdeal)
        {
            Debug.Log("misdeal");
            Table.gameState = GameState.Misdeal;
        }
        else if (cardCountForPreFlop == Services.PokerRules.cardsPulled.Count)
        {
            Debug.Log("got all the right cards");
            Table.gameState = GameState.PreFlop;
        }
        else if(Services.PokerRules.cardsPulled.Count > players.Count * 2 && !Services.Dealer.OutsideVR)
        {
            Debug.Log("Dealt too many cards");
            Table.gameState = GameState.Misdeal;
        }
        else
        {
            Debug.Log("correctingMistakes because cardCountForPreflop != cardsPulled");
            Services.PokerRules.CorrectMistakesPreFlop();
            Table.gameState = GameState.PreFlop;
        }
        Debug.Log("ending the check");
        checkedPreFlopCardCount = false;
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

    public int PlayerAtTableCount()
    {
        int activePlayers = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].PlayerState != PlayerState.Eliminated)
            {
                activePlayers++;
            }
        }
        return activePlayers;
    }

    public void StartRound()
    {
		
        Debug.Log("Starting round " + Table.gameState);
        SetCurrentAndLastBet();
        foreach (PokerPlayerRedux player in players) player.timesRaisedThisRound = 0;
        raisesInRound = 0;
        if (Table.gameState == GameState.PreFlop)
        {
            if(GetActivePlayerCount() == 2)
            {
                playerToAct = FindFirstPlayerToAct(0);
            }
            else playerToAct = FindFirstPlayerToAct(3);
            Debug.Log("player to act = " + playerToAct);
        }
        else
        {
            playerToAct = FindFirstPlayerToAct(1);
            Debug.Log("player to act = " + playerToAct);
        }
        //(playerToAct != null) playerToAct.
        //StartCoroutine(playerAction(playerToAct));
//        if (!Services.SoundManager.conversationIsPlaying)
//        {
//            Services.SoundManager.PlayAsideConversation(UnityEngine.Random.Range(0, 5));
//        }
    }

    public PokerPlayerRedux FindFirstPlayerToAct(int distance)
    {
        Debug.Log("FindingFirstPlayerToAct");
        PokerPlayerRedux player;
        player = PlayerSeatsAwayFromDealerAmongstLivePlayers(distance);
        if(player.PlayerState == PlayerState.NotPlaying || player.playerIsAllIn || player.PlayerState == PlayerState.Eliminated || player.currentBet > 0)
        {
            for (int i = 0; i < players.Count; i++)
            {
                PokerPlayerRedux nextPlayer = players[(player.SeatPos + i) % players.Count];
                if (nextPlayer.PlayerState != PlayerState.NotPlaying && !nextPlayer.playerIsAllIn && nextPlayer.PlayerState != PlayerState.Eliminated)
                {
                    player = nextPlayer;
                    break;

                }
            }
        }
        return player;
    }

    public bool OnlyAllInPlayersLeft()
    {
        bool onlyAllinPlayersLeft;
        float allInPlayers = 0;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerIsAllIn) allInPlayers++;
        }
        if (allInPlayers == GetActivePlayerCount())
        {
            onlyAllinPlayersLeft = true;
        }
        else if(GetActivePlayerCount() - allInPlayers == 1 && playerToAct == null)
        {
            onlyAllinPlayersLeft = true;
        }
        else onlyAllinPlayersLeft = false;

        return onlyAllinPlayersLeft;
    }

    //this is just a way to give the player a little buffer time so they don't accidentally trigger the showdown
    IEnumerator WaitForShowDown(float time)
    {
        yield return new WaitForSeconds(time);
        readyForShowdown = true;
    }

    public void GrabAndKillCards()
    {
        Debug.Log("grabbingCards");
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        GameObject cardDeck = GameObject.Find("ShufflingArea"); ;
        bool flyUp = false;
        foreach (GameObject card in cardsOnTable)
        {
            card.GetComponent<Rigidbody>().useGravity = false;
            card.GetComponent<BoxCollider>().enabled = false;
            Vector3 randomRot = new Vector3(UnityEngine.Random.Range(100, 360), UnityEngine.Random.Range(100, 360), UnityEngine.Random.Range(100, 360));
            card.transform.Rotate(randomRot * cardMoveSpeed * 2 * Time.deltaTime);
            if (card.transform.position.y <= cardDeck.transform.position.y + UnityEngine.Random.Range(0.5f, 6f) && card.GetComponent<Card>().flyingAllowed) flyUp = true;
            else flyUp = false;
            if (flyUp)
            {
                float step = cardMoveSpeed * 2 * Time.deltaTime;
                card.transform.position = Vector3.MoveTowards(card.transform.position, card.transform.position + new Vector3 (0, 0.05f, 0), step);
            }
            else
            {
                card.GetComponent<Card>().flyingAllowed = false;
                float step = cardMoveSpeed * Time.deltaTime;
                card.transform.position = Vector3.MoveTowards(card.transform.position, cardDeck.transform.position, step);
                if (card.transform.position == cardDeck.transform.position)
                {
                    if (!madeNewDeck)
                    {
                        newCardDeck = Instantiate(Services.PrefabDB.CardDeck, cardDeck.transform.position, Quaternion.identity) as GameObject;
                        newCardDeck.GetComponent<CardDeckScript>().BuildDeckFromOneCard(newCardDeck);
                        madeNewDeck = true;
                    }
                    if (madeNewDeck == true)
                    {
                        Destroy(card);
                        //Debug.Log("destroying cards");
                        newCardDeck.GetComponent<CardDeckScript>().MakeDeckLarger();
                        if (cardsOnTable.Length < 5)
                        {
                            Table.instance.RestartRound();
                            Services.Dealer.killingCards = false;
                            madeNewDeck = false;
                            GameObject[] deadCards = GameObject.FindGameObjectsWithTag("PlayingCard");
                            foreach (GameObject dc in deadCards)
                            {
                                Destroy(dc);
                            }
                        }
                    }
                }
            }
        }
    }

    public void CleanUpTable()
    {
        Debug.Log("grabbingCards");
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        GameObject cardDeck = GameObject.FindGameObjectWithTag("CardDeck");
        foreach (GameObject card in cardsOnTable)
        {
            float step = cardMoveSpeed * Time.deltaTime;
            card.transform.position = Vector3.MoveTowards(card.transform.position, cardDeck.transform.position, step);
            if (card.transform.position == cardDeck.transform.position)
            {
                Destroy(card);
                //Debug.Log("destroying cards");
                Debug.Log(cardsOnTable.Length);
                cardDeck.GetComponent<CardDeckScript>().MakeDeckLarger();
                if (cardsOnTable.Length < 5)
                {
                    Table.instance.RestartRound();
                    cleaningCards = false;
                    GameObject[] deadCards = GameObject.FindGameObjectsWithTag("PlayingCard");
                    foreach (GameObject dc in deadCards)
                    {
                        Destroy(dc);
                    }
                }
            }
        }
    }
    
    public void CheckAllInPlayers()
    {
        int allInPlayerCount = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerIsAllIn == true) allInPlayerCount++;
        }
        if (GetActivePlayerCount() == allInPlayerCount)
        {
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if (players[i].playerIsAllIn == true)
                {
                    players[i].FlipCards();
                }
            }
        }
        else if((GetActivePlayerCount() - allInPlayerCount) == 1)
        {
            if(playerToAct == null) playersReady = true;
        }
    }

    //this gets invoked whenever we gaze at a player
    public void SetNextPlayer()
    {
        if (playerToAct != null)
        {
            //Debug.Log("current playerToAct = " + playerToAct);
            int currentPlayerSeatPos = playerToAct.SeatPos;

            playerToAct.playerSpotlight.SetActive(false);
            bool roundFinished = true;
            PokerPlayerRedux nextPlayer = null;
            for (int i = 1; i < players.Count; i++)
            {
                nextPlayer = players[(currentPlayerSeatPos + i) % players.Count];
                if (nextPlayer.PlayerState == PlayerState.Playing)
                {
                    //Debug.Log("nextPlayer = " + nextPlayer.name);
                    //Debug.Log("nextPlayer.actedThisRound = " + nextPlayer.actedThisRound);
                    //Debug.Log("nextPlayer.currentBet = " + nextPlayer.currentBet + " and lastBet = " + LastBet);
                    //Debug.Log("nextPlayer.chipCount = " + nextPlayer.chipCount);
                    //Debug.Log("nextPlayer.PlayerState = " + nextPlayer.PlayerState);
                }
                if ((!nextPlayer.actedThisRound || nextPlayer.currentBet < LastBet) && nextPlayer.PlayerState == PlayerState.Playing && nextPlayer.chipCount != 0)
                {
                    roundFinished = false;
                    previousPlayerToAct = playerToAct;
                    playerToAct = nextPlayer;
                    Debug.Log("nextPlayer to act is player " + playerToAct);
                    break;
                }
            }
            if (roundFinished)
            {
                if (OnlyAllInPlayersLeft())
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (players[i].Hand != null)
                        {
                            players[i].FlipCards();
                        }
                    }
                }
                Debug.Log(Table.gameState + " Finished");
                playerToAct.playerSpotlight.SetActive(false);
                playerToAct = null;
                playersReady = true;
            }
        }
    }

    public bool CheckIfOnlyPlayerNotAllIn()
    {
        bool onlyOnePlayerNotAllIn = false;
        int playersAllIn = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerIsAllIn)
            {
                playersAllIn++;
            }
        }
        if(GetActivePlayerCount() - playersAllIn == 1)
        { 
            onlyOnePlayerNotAllIn = true;
        }

        return onlyOnePlayerNotAllIn;
    }

    public void InitializePlayers(int chipCount)
    {
        for (int i = 0; i < players.Count; i++)
        {
			players[i].SeatPos = i;
			players[i].PlayerState = PlayerState.Playing;
			activePlayers.Add(players[i]);
			Debug.Log ("Adding " + players[i] + " to active players!");
            List<int> startingStack  = players[i].SetChipStacks(chipCount);
            Table.instance.AddChipTo(playerDestinations[i], chipCount);
            players[i].CreateAndOrganizeChipStacks(startingStack);
        }

        Table.instance.DealerPosition = 0;
        Table.instance.SetDealerButtonPos(Table.instance.DealerPosition);
        Table.instance.gameData = new PokerGameData(Table.instance.DealerPosition, players);
        StartCoroutine(WaitToPostBlinds(.25f));
    }

    public IEnumerator WaitToPostBlinds(float time)
    {
        yield return new WaitForSeconds(time);
        PostBlinds();
    }
    public void PostBlinds()
    {
        //Debug.Log("posting blinds");
        players[SeatsAwayFromDealerAmongstLivePlayers(1)].Bet(SmallBlind);
        players[SeatsAwayFromDealerAmongstLivePlayers(1)].currentBet = SmallBlind;
        players[SeatsAwayFromDealerAmongstLivePlayers(2)].Bet(BigBlind);
        players[SeatsAwayFromDealerAmongstLivePlayers(2)].currentBet = BigBlind;
        LastBet = BigBlind;
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
            players[SeatsAwayFromDealerAmongstLivePlayers(1)].currentBet = SmallBlind;
            players[SeatsAwayFromDealerAmongstLivePlayers(2)].currentBet = BigBlind;
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
                    player.ChipCountToCheckWhenWinning = player.chipCount;
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

    public void WaitForWinnersToGetPaid()
    {
        Debug.Assert(numberOfWinners > 0);
        List<PokerPlayerRedux> winningPlayers = new List<PokerPlayerRedux>();
        int potAmount = Table.instance.potChips;
        for (int i = 0; i < players.Count; i++)
        {
            PokerPlayerRedux playerToCheck = players[SeatsAwayFromDealer(i + 1)];
            if(playerToCheck.PlayerState == PlayerState.Winner)
            {
                winningPlayers.Add(playerToCheck);
                playerToCheck.playerLookedAt = false;
            }
        }
        int potRemaining = potAmount;
        for (int i = 0; i < winningPlayers.Count; i++)
        {
            if (numberOfWinners - i == 0) Debug.Log("numberOfWinners = " + numberOfWinners + " and i = " + i);
            potAmountToGiveWinner = potRemaining / (numberOfWinners - i);
            int remainder = potAmountToGiveWinner % ChipConfig.RED_CHIP_VALUE;
            if (remainder > 0)
            {
                winningPlayers[i].chipsWon = (potAmountToGiveWinner - remainder) + ChipConfig.RED_CHIP_VALUE;
            }
            else winningPlayers[i].chipsWon = potAmountToGiveWinner;
            potRemaining -= winningPlayers[i].chipsWon;
        }
        //PROBLEM AREA FOR GAZE SPLIT
        //if(winningPlayers.Count >= 2)
        //{
        //    foreach (Chip chip in chipsInPot)
        //    {
        //        if (chip != null) Destroy(chip.gameObject);
        //        else Debug.Log("You are trying to Destroy a chip that is alread DEAD");
        //    }
        //    for (int i = 0; i < winningPlayers.Count; i++)
        //    {
        //        List<int> splitPot = PrepChipsForSplit(winningPlayers[i].chipsWon);
        //        SplitPot(splitPot, winningPlayers[i].SeatPos);
        //    }
        //}
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

        //if (!winnersHaveBeenPaid)
        //{
        //    for (int i = 0; i < players.Count; i++)
        //    {
        //        if(players[i].PlayerState == PlayerState.Winner || players[i].PlayerState == PlayerState.Loser)
        //        {
        //            players[i].GetComponentInChildren<PlayerGazeTrigger>().questionMark.fillAmount = 1;
        //        }
        //    }
        //}
        //Table.gameState = GameState.PostHand;
        //GameObject[] deadCards = GameObject.FindGameObjectsWithTag("PlayingCard");
        //foreach(GameObject card in deadCards)
        //{
        //    deadCardsList.Add(card.GetComponent<Card>());
        //}
        
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
     //   player.LoserReactions();
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
                ////
                //
                //
                //IMPORTANT DEBUG
                //Debug.Log("for player" + player.SeatPos + " the winnerChipStack = " + winnerChipStack + " and the Player has" + player.chipCount);
                //
                ///
                ///
                //
                if (player.chipCount == winnerChipStack && player.HasBeenPaid == false)
                {
                    winnersPaid++;
                    player.HasBeenPaid = true;
                }
            }
            else if(player.PlayerState == PlayerState.Loser)
            {
                winnerChipStack = player.ChipCountToCheckWhenWinning + potAmountToGiveWinner;
                if(player.chipCount == winnerChipStack)
                {
                    //messageText.text = "I think you messed up, I didn't win this money.";
                }
                else
                {
                    //messageText.text = "Give the winner(s) their winnings. the pot size is " + Table.instance.potChips + " and there is/are " + numberOfWinners + " winner(s)";
                }
            }
        }
        if (winnersPaid == numberOfWinners)
        {
            //Debug.Log("winnersPaid == numberOfWinners");
            winnersHaveBeenPaid = true;
            winnersPaid = 0;
            numberOfWinners = 0;

			for (int i = 0; i < players.Count; i++) 
			{
                int winnerCount = 0;
				if(players[i].PlayerState == PlayerState.Winner)
				{
                    for (int playersChecked = 0; playersChecked < players.Count; playersChecked++)
                    {
                        if(players[playersChecked].PlayerState == PlayerState.Winner)
                        {
                            winnerCount++;
                        }
                    }
					if(!players[i].playerAudioSource.isPlaying && 
                       !players[i].playerIsInConversation && 
                       !Services.SoundManager.conversationIsPlaying &&
                       winnerCount < 2)
					{
                        if (!inTutorial)
                        {
                            Services.SoundManager.GetSourceAndPlay(players[i].playerAudioSource, players[i].tipAudio);
                        }
                    }
				}
			}
        }
    }

    //this resets all the player states and gamestates and resets bools
    //so that the player is ready for each round
    public void ResetPlayerStatus()
    {
        for (int i = 0; i < players.Count; i++)
        {
			if (players[i].chipCount == 0)
			{
				players[i].PlayerState = PlayerState.Eliminated;
				if (activePlayers.Contains(players[i]))
				{
					activePlayers.Remove(players[i]);
					Debug.Log ("Removing " + players[i] + " from active players!");
				}
			}

            if (players[i].PlayerState != PlayerState.Eliminated)
            {
                players[i].PlayerState = PlayerState.Playing;
            }

            players[i].HasBeenPaid = false;
            players[i].playerIsAllIn = false;
            players[i].flippedCards = false;
            players[i].isAggressor = false;
            players[i].waitingToGetPaid = false;
            players[i].playerLookedAt = false;
            players[i].timesRaisedThisRound = 0;
            //players[i].checkedHandStrength = false;
        }
        Services.PokerRules.cardsPulled.Clear();
        Services.PokerRules.cardsLogged.Clear();
        cardsTouchingTable.Clear();
        Table.gameState = GameState.NewRound;
        playersHaveBeenEvaluated = false;
        winnersHaveBeenPaid = false;
        readyToAwardPlayers = false;
        finalHandEvaluation = false;
		misdealAudioPlayed = false;
        raisesInRound = 0;
        Services.PokerRules.checkedForCorrections = false;
        chipsInPot.Clear();
        deadCardsList.Clear();
        GameObject[] chipsOnTable = GameObject.FindGameObjectsWithTag("Chip");
        if (chipsOnTable.Length > 0)
        {
            foreach (GameObject chip in chipsOnTable)
            {
                Destroy(chip);
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].PlayerState == PlayerState.Playing)
            {
                List<int> newStacks = players[i].SetChipStacks(players[i].chipCount);
                GameObject[] chipsToDestroy = GameObject.FindGameObjectsWithTag("Chip");
                players[i].CreateAndOrganizeChipStacks(newStacks);
            }
        }
    }

    public void ResetGameState()
    {
        //Debug.Log("ResettingGameStatus");
        for (int i = 0; i < players.Count; i++)
        {
            Table.instance.playerChipStacks[i] = Table.instance.gameData.PlayerChipStacks[i];
			if (players[i].chipCount == 0)
			{
				players[i].PlayerState = PlayerState.Eliminated;
				if (activePlayers.Contains(players[i]))
				{
					Debug.Log ("Removing " + players[i] + " from active players!");
					activePlayers.Remove(players[i]);

				}
			}

            if (players[i].PlayerState != PlayerState.Eliminated)
            {
                players[i].PlayerState = PlayerState.Playing;
                players[i].actedThisRound = false;
                players[i].turnComplete = false;
            }
            players[i].HasBeenPaid = false;
            players[i].playerIsAllIn = false;
            players[i].flippedCards = false;
            players[i].isAggressor = false;
            players[i].waitingToGetPaid = false;
            players[i].playerLookedAt = false;
            players[i].timesRaisedThisRound = 0;
            //players[i].checkedHandStrength = false;
        }
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        foreach (GameObject card in cardsOnTable) Destroy(card);
        Table.gameState = GameState.NewRound;
        Services.Dealer.playerToAct = null;
        Services.Dealer.previousPlayerToAct = null;
        Services.PokerRules.checkedForCorrections = false;
        playersHaveBeenEvaluated = false;
        winnersHaveBeenPaid = false;
        readyToAwardPlayers = false;
		misdealAudioPlayed = false;
        finalHandEvaluation = false;
        roundStarted = false;
        raisesInRound = 0;
        Services.PokerRules.TurnOffAllIndicators();
        GameObject[] chipsOnTable = GameObject.FindGameObjectsWithTag("Chip");
        if (chipsOnTable.Length > 0)
        {
            foreach (GameObject chip in chipsOnTable)
            {
                Destroy(chip);
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].PlayerState == PlayerState.Playing)
            {
                List<int> newStacks = players[i].SetChipStacks(players[i].chipCount);
                GameObject[] chipsToDestroy = GameObject.FindGameObjectsWithTag("Chip");
                players[i].CreateAndOrganizeChipStacks(newStacks);
            }
        }
    }

    //this is an ease of life function to find how far away from the dealer button a given player is
    public int SeatsAwayFromDealer(int distance)
    {
        return (Table.instance.gameData.DealerPosition + distance) % players.Count();
    }

    public int SeatsAwayFromDealerAmongstLivePlayers(int distance)
    {
        int playersInLine = 0;
        int index = 0;
        distance = distance % PlayerAtTableCount(); //distance = 3, and P@T = 3, so distance = 0
        Debug.Assert(PlayerAtTableCount() > 0);
        while(playersInLine <= distance) //while 0 <= 0
        {
            PokerPlayerRedux player = players[SeatsAwayFromDealer(index)]; //player = players[
            if (player.PlayerState != PlayerState.Eliminated)
            {
                if (playersInLine == distance) return player.SeatPos;
                playersInLine += 1;
            }
            index += 1;
        }
        //should never get here
        Debug.Assert(false);
        return 0;
    }

    public PokerPlayerRedux PlayerSeatsAwayFromDealerAmongstLivePlayers(int distance)
    {
        return players[SeatsAwayFromDealerAmongstLivePlayers(distance)];
    }

    public List<int> PrepChipsForSplit(int chipAmount)
    {

        List<int> startingStack = new List<int>();

        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("P0Cards"), GameObject.Find("P1Cards"), GameObject.Find("P2Cards"), GameObject.Find("P3Cards"), GameObject.Find("P4Cards")
        };

        int valueRemaining = chipAmount;
        int blackChipCount = 0;
        int whiteChipCount = 0;
        int blueChipCount = 0;
        int redChipCount = 0;

        //change these hard coded variables to a function that finds the proper amount of chips based on a percent of the chipAmount
        int blackChipCountMAX = (int)((chipAmount * 0.45f) / ChipConfig.BLACK_CHIP_VALUE);
        int whiteChipCountMAX = (int)((chipAmount * 0.35f) / ChipConfig.WHITE_CHIP_VALUE);
        int blueChipCountMAX = (int)((chipAmount) * 0.15f / ChipConfig.BLUE_CHIP_VALUE);

        blackChipCount = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
        valueRemaining -= (blackChipCount * ChipConfig.BLACK_CHIP_VALUE);
        startingStack.Add(blackChipCount);

        whiteChipCount = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
        valueRemaining -= (whiteChipCount * ChipConfig.WHITE_CHIP_VALUE);
        startingStack.Add(whiteChipCount);

        blueChipCount = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
        valueRemaining -= (blueChipCount * ChipConfig.BLUE_CHIP_VALUE);
        startingStack.Add(blueChipCount);

        redChipCount = valueRemaining / ChipConfig.RED_CHIP_VALUE;
        startingStack.Add(redChipCount);

        return startingStack;
    }

    public void SplitPot(List<int> chipsToOrganize, int SeatPos)
    {
        GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
        foreach (GameObject container in emptyContainers)
        {
            if (container.transform.childCount == 0)
            {
                Destroy(container);
            }
        }

        List<int> organizedChips = chipsToOrganize;
        GameObject parentChip = null;
        float incrementStackBy = 0;
        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("P0BetZone"), GameObject.Find("P1BetZone"), GameObject.Find("P2BetZone"), GameObject.Find("P3BetZone"), GameObject.Find("P4BetZone")
        };
        Vector3 offSet = Vector3.zero;
        Vector3 containerOffset = Vector3.up * .08f;
        GameObject chipContainer = GameObject.Instantiate(new GameObject(), playerPositions[SeatPos].transform.position + containerOffset, playerPositions[SeatPos].transform.rotation);
        chipContainer.tag = "Container";
        chipContainer.name = "Container";
        chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
        Vector3 lastStackPos = Vector3.zero;
        Vector3 firstStackPos = Vector3.zero;

        int stackCountMax = 30;
        int stacksCreated = 0;
        //int stackRowMax = 5;

        for (int chipStacks = 0; chipStacks < organizedChips.Count; chipStacks++)
        {
            GameObject chipToMake = null;
            if (organizedChips[chipStacks] != 0)
            {
                switch (chipStacks)
                {
                    case 0:
                        chipToMake = FindChipPrefab(ChipConfig.BLACK_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLACK_CHIP_VALUE);
                        break;
                    case 1:
                        chipToMake = FindChipPrefab(ChipConfig.WHITE_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.WHITE_CHIP_VALUE);
                        break;
                    case 2:
                        chipToMake = FindChipPrefab(ChipConfig.BLUE_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.BLUE_CHIP_VALUE);
                        break;
                    case 3:
                        chipToMake = FindChipPrefab(ChipConfig.RED_CHIP_VALUE);
                        chipToMake.GetComponent<Chip>().chipData = new ChipData(ChipConfig.RED_CHIP_VALUE);
                        break;
                    default:
                        break;
                }
                int chipStackSize = 0;
                for (int chipIndex = 0; chipIndex < organizedChips[chipStacks]; chipIndex++)
                {
                    if (chipIndex == 0)
                    {
                        chipStackSize++;
                        stacksCreated++;
                        //if(stacksCreated >= stackRowMax)
                        //{
                        //    stacksCreated = 0;
                        //    offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.z + .01f, 0, 0);
                        //}
                        //Debug.Log("ChipToMake = " + chipToMake);
                        parentChip = Instantiate(chipToMake, chipContainer.transform.position, Quaternion.identity) as GameObject;
                        parentChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        //incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else if (chipStackSize >= stackCountMax)
                    {
                        //Debug.Log("creating new stack cause max stack count reached");
                        chipStackSize = 0;
                        stacksCreated++;
                        //if (stacksCreated >= stackRowMax)
                        //{
                        //    Debug.Log("moving row forward");
                        //    stacksCreated = 0;
                        //    offSet += new Vector3(0, 0, (parentChip.GetComponent<Collider>().bounds.size.x + .01f) * 1.5f);
                        //}
                        //parentChip = organizedChips[chipStacks][chipIndex];
                        parentChip = Instantiate(chipToMake, chipContainer.transform.position, Quaternion.identity) as GameObject;
                        parentChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if (parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        //incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
                        incrementStackBy = parentChip.transform.localScale.z;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if (firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else
                    {
                        chipStackSize++;
                        parentChip.transform.localScale = new Vector3(parentChip.transform.localScale.x,
                                                                      parentChip.transform.localScale.y,
                                                                      parentChip.transform.localScale.z + incrementStackBy);
                        ChipData newChipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        parentChip.GetComponent<Chip>().chipStack.chips.Add(newChipData);
                        parentChip.GetComponent<Chip>().chipStack.stackValue += newChipData.ChipValue;
                    }
                }
            }
        }
        Vector3 trueOffset = firstStackPos - lastStackPos;
        chipContainer.transform.position += trueOffset / 2;
    }

    public GameObject FindChipPrefab(int chipValue)
    {
        GameObject chipPrefab = null;
        switch (chipValue)
        {
            case ChipConfig.RED_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.RedChip;
                break;
            case ChipConfig.BLUE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlueChip;
                break;
            case ChipConfig.WHITE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.WhiteChip;
                break;
            case ChipConfig.BLACK_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlackChip;
                break;
            default:
                break;
        }
        return chipPrefab;
    }
}

       