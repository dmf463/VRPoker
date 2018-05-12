using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Valve.VR;
using Valve.VR.InteractionSystem;
using TMPro;

//this entire script is essentially what the dealer does
//the three most important scripts are Dealer, Table, and PokerPlayerRedux
//Dealer handles anything a dealer would do at a table
//Table pretty much holds all the cards and any object. if it's at the table, it's in Table
//PokerPlayerRedux handles all the functions and info that a poker player would need to play
public class Dealer : MonoBehaviour
{
    List<List<PokerPlayerRedux>> PlayerRank = new List<List<PokerPlayerRedux>>();

    private DateTime oldTime;
    private DateTime newTime;
    private int minutes;
    public int handsCompleted;

    public Light lighting;

    public int chipsMoved;
    public bool readyForCards = false;

    public int tipCount;
    public GameObject tipIndicator;
    public float tipMultiplier;
    public GameObject multiplerObj;

    public int startingChipCount;
    public float cardMoveSpeed = 1;
    public bool killingCards = false;
    float startUpTime;
    bool flyingClicked = false;
    public float flyTime;
    public bool madeNewDeck;
    GameObject newCardDeck;
    public bool deckIsDead;
    public bool cleaningCards = false;
    public bool holdRotate = false;
    private bool doneLerping = false;
    GameObject cardToCheck;
    public bool isCheating;

    private float radius;
    private float theta;
    [HideInInspector]
    public float height;
    Vector3 centerPoint;
    public float noiseMagnitude;
    public float noiseSpeed;
    public float rotationSpeed;

    public List<GameObject> thrownChips = new List<GameObject>();

    [HideInInspector]
    public List<Card> deadCardsList = new List<Card>();
    //we set this to true if we're outside VR so we can text
    public bool OutsideVR = false;
 
    bool first_time = true;

    [HideInInspector]
    public List<Card> cardsTouchingTable = new List<Card>();

	public List<PokerPlayerRedux> players = new List<PokerPlayerRedux>();
	public List<PokerPlayerRedux> activePlayers = new List<PokerPlayerRedux>();

    public PokerPlayerRedux playerToAct;
    public PokerPlayerRedux previousPlayerToAct;

    private List<Destination> playerDestinations = new List<Destination>();

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
    public bool playerHasBeenEliminated;

    [HideInInspector]
    public bool readyToAwardPlayers = false;

    [HideInInspector]
    public bool everyoneFolded = false;

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

    public PokerGameData startingGameState;
    public bool consolidatingChips = false;

    [HideInInspector]
	public bool misdealAudioPlayed =false;

    private TaskManager tm;
    public float timeBetweenIdle;

    public GameObject[] objectsToHide;
    public GameObject[] chipsToBring;
    public bool startingWithIntro;
    [HideInInspector]
    public List<AudioSource> audioSources = new List<AudioSource>();
    

    void Awake()
    {
        //this is where we intialize all our services stuff
        tm = new TaskManager();
        Services.ChipManager = new ChipManager();
        Services.ChipManager.ChipInit();
        Services.PrefabDB = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
        Services.SoundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        Services.DialogueDataManager = new DialogueDataManager();
        Services.Dealer = this;
        Services.PokerRules = GameObject.Find("PokerRules").GetComponent<PokerRules>();
		Services.PlayerBehaviour = new PlayerBehaviour();
		Services.DialogueDataManager.ParseConvoDialogueFile((Services.SoundManager.convoDialogueFile));
        Services.DialogueDataManager.ParseOneLinerDialogueFile((Services.SoundManager.oneLinerDialogueFiler));
        Services.TextManager = GameObject.Find("TableGraphics").GetComponent<TextManager>();
        Services.AnimationScript = GameObject.Find("AnimationController").GetComponent<AnimationScript>();
    }

    // Use this for initialization
    void Start()
    {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        oldTime = System.DateTime.Now;
        //oldTimeForIdle = System.DateTime.Now;
        timeBetweenIdle = 0;
        minutes = 0;
        tipMultiplier = 1;
        tipCount = 0;
        playerDestinations = Table.instance.playerDestinations;
        InitializePlayers(startingChipCount);
        if (startingWithIntro)
        {
            Table.gameState = GameState.Intro;
            lastGameState = GameState.Intro;
        }
        else
        {
            Table.gameState = GameState.NewRound;
            lastGameState = GameState.NewRound;
        }
        startingGameState = new PokerGameData(0, players);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            IntroduceCharacters();
        }
        Services.ChipManager.ChipUpdate();
        newTime = System.DateTime.Now;
        minutes = newTime.Minute - oldTime.Minute;
        if(Table.gameState != GameState.Intro)
        {
            timeBetweenIdle += Time.deltaTime;
        }

        if(timeBetweenIdle >= 15 && !OutsideVR && Table.gameState != GameState.Intro)
        {
            PokerPlayerRedux randomPlayer = Services.Dealer.players[UnityEngine.Random.Range(0, Services.Dealer.players.Count)];
            if (!randomPlayer.playerAudioSource.isPlaying)
            {
                ResetIdleTime();
                Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(randomPlayer.playerName, LineCriteria.IdleTime));
            }
        }
        tm.Update();
        if (deckIsDead)
        {
            Services.Dealer.TriggerMisdeal();
        }
        tipIndicator.GetComponent<TextMeshPro>().text = tipCount.ToString();
        multiplerObj.GetComponent<TextMeshPro>().text = tipMultiplier.ToString();
        WaitingToGrabCardsOn_ThrownDeck();
        WaitingToGrabCardsOn_MisDeal();
        //RunTutorial();
        IncreaseBlinds();
        CheckGameState();

        buttonATimer--;
        buttonBTimer--;
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

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (!Services.SoundManager.conversationIsPlaying)
            {
                for (int i = 0; i < 100; i++)
                {
                    Services.SoundManager.PlayConversation();
                    Debug.Log(gameObject + " started a conversation.");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            PokerPlayerRedux randomPlayer = Services.Dealer.players[UnityEngine.Random.Range(0, Services.Dealer.players.Count)];
            Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(randomPlayer.playerName, LineCriteria.Call));
        }
       
        if (Input.GetKeyDown(KeyCode.D))
        {
            AudioClip audioFile = Resources.Load("Audio/Voice/TutorialVO/tutorial0.wav") as AudioClip; //gets the audiofile from resources using the string name 
                Debug.Log(audioFile);  
        }

        if (playersReady)
        {
            Services.PokerRules.SetCardIndicator();
        }
        //this resets bools necessary to start new rounds
        //once both of these are true, then the next round will start
        if (playersReady && Table.gameState != lastGameState)
        {
            Debug.Log("starting round");
            readyForCards = false;
            roundStarted = false;
            playersReady = false;
            Services.PokerRules.TurnOffAllIndicators();
        }
        //starts the round for pre-flop
        if (Table.gameState == GameState.PreFlop)
        {
            if (!roundStarted)
            {
                roundStarted = true;
                if (!OnlyAllInPlayersLeft()) StartRound();
                else
                {
                    playersReady = true;
                    readyForCards = true;
                }
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
                else
                {
                    playersReady = true;
                    readyForCards = true;
                }
            }
        }

        //starts next round
        if (Table.gameState == GameState.Turn)
        {
            if (!roundStarted)
            {
                roundStarted = true;
                if (!OnlyAllInPlayersLeft()) StartRound();
                else
                {
                    playersReady = true;
                    readyForCards = true;
                }
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
                            player.Hand = SetHand(player);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if(players[i].playerIsAllIn || players[i].chipCount == 0)
                        {
                            if (players[i].PlayerState == PlayerState.Playing)
                            {
                                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtRiver(players[i].SeatPos);
                                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                                playerHand.EvaluateHandAtRiver();
                                players[i].Hand = playerHand;
                            }
                        }
                    }
                }
                //Debug.Log("River Debug should only go once");
                roundStarted = true;
                if (!OnlyAllInPlayersLeft()) StartRound();
                else playersReady = true;
                readyForShowdown = false;
                StartCoroutine(WaitForShowDown());
            }
            int allInPlayerCount = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].playerIsAllIn == true) allInPlayerCount++;
            }
            if (GetActivePlayerCount() == allInPlayerCount)
            {
                Debug.Log("Only ALL IN left");
                Table.gameState = GameState.ShowDown;
            }
            else if (GetActivePlayerCount() - allInPlayerCount == 1 && playersReady)
            {
                Debug.Log("all in and one player left");
                Table.gameState = GameState.ShowDown;
            }
            if (readyForShowdown == true)
            {
                StartCoroutine(ReadyForShowDown(1));
            }
        }

        if (Table.gameState == GameState.ShowDown)
        {
            if (!playersHaveBeenEvaluated)
            {
                //messageText.text = "Give the winner(s) their winnings (pot size is " + Table.instance.potChips + " , that's a black chip)";
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
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.clockDing, .05f);
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].Hand != null)
                    {
                        if(!players[i].flippedCards && !everyoneFolded) players[i].PushInCards();
                        Debug.Log("player" + players[i].SeatPos + 
                                  "is the " + players[i].PlayerState + 
                                  " with (a) " + players[i].Hand.HandValues.PokerHand + 
                                  " with a highCard of " + players[i].Hand.HandValues.HighCard + 
                                  " and a handTotal of " + players[i].Hand.HandValues.Total);
                    }
                }
                StartCoroutine(WaitForWinnersToGetPaid());
                readyToAwardPlayers = true;
            }
        }
        #endregion
        lastGameState = Table.gameState;
        Services.ChipManager.DestroyChips();
    }

    public void ResetIdleTime()
    {
        timeBetweenIdle = 0;
    }

    public void IntroduceCharacters()
    {
        foreach (GameObject o in objectsToHide)
        {
            o.SetActive(true);
        }
        foreach (GameObject o in chipsToBring)
        {
            o.SetActive(true);
        }
        Table.gameState = GameState.NewRound;
        StartCoroutine(WaitToPostBlinds(.25f));
    }

    public void TriggerMisdeal()
    {
        if (Table.gameState != GameState.Intro)
        {
            Table.gameState = GameState.Misdeal;
        }
    }

    IEnumerator WaitToResetBool(float time, AudioData clip)
    {
        yield return new WaitForSeconds(time);
        clip.finishedPlaying = true;
    }

    IEnumerator ReadyForShowDown(float time)
    {
        yield return new WaitForSeconds(time);
        Table.gameState = GameState.ShowDown;
    }

    public void CheckGameState()
    {
        if(hand1.GetStandardInteractionButton() || hand2.GetStandardInteractionButton())
        {
            ResetIdleTime();
        }

        if (Table.gameState == GameState.NewRound)
        {
            if (Services.PokerRules.cardsPulled.Count == PlayerAtTableCount() * 2 && !checkedPreFlopCardCount && Services.PokerRules.thrownCards.Count == 0)
            {
                checkedPreFlopCardCount = true;
                StartCoroutine(CheckForMistakesPreFlop(1.5f));
            }
        }
        else if(Table.gameState == GameState.PostHand)
        {
            GameObject[] tips = GameObject.FindGameObjectsWithTag("Tip");
            if (tips.Length == 0) cleaningCards = true;
        }
        else if (Table.gameState == GameState.Misdeal)
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
                    Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(players[i].playerName, LineCriteria.Misdeal));
                    //Services.SoundManager.GetSourceAndPlay(players[i].playerAudioSource, players[i].misdealAudio);
                }
            }
            //messageText.text = "You misdealt the hand, click both triggers to restart the round.";

            if (hand1.GetStandardInteractionButtonDown()) buttonATimer = bufferPeriod;
            if (hand2.GetStandardInteractionButtonDown()) buttonBTimer = bufferPeriod;
            if (hand1.GetStandardInteractionButtonDown() && buttonBTimer > 0 ||
                hand2.GetStandardInteractionButtonDown() && buttonATimer > 0)
            {
                if (deckIsDead) killingCards = true;
                else cleaningCards = true;
                tipMultiplier = 0;
            }
        }
    }

    void IncreaseBlinds()
    {
		if (minutes >= 60) //60 minutes
		{
			SmallBlind = 1200;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 55) //55 minutes
		{
			SmallBlind = 1000;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 50) //50 minutes
		{
			SmallBlind = 900;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 45) //45 minutes
		{
			SmallBlind = 800;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 40) //40 minutes
		{
			SmallBlind = 700;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 35) //35 minutes
		{
			SmallBlind = 600;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 30) //30 minutes
		{
			SmallBlind = 500;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 25) //25 minutes
		{
			SmallBlind = 400;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 20) //20 minutes
		{
			SmallBlind = 300;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 15) //15 minutes
		{
			SmallBlind = 200;
			BigBlind = SmallBlind * 2;
		}
		else if (minutes >= 10) //10 minutes
		{
			SmallBlind = 100;
			BigBlind = SmallBlind * 2;
		}
        else if(minutes >= 5) //5 minutes
        {
            SmallBlind = 50;
            BigBlind = SmallBlind * 2;
        }
    }

    IEnumerator CheckForMistakesPreFlop(float time)
    {
        if (Table.gameState != GameState.Misdeal)
        {
            //Debug.Log("CheckingPreFlopMistakes");
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
            if (misdeal)
            {
                Debug.Log("misdeal");
                Services.Dealer.TriggerMisdeal();
            }
            else if (cardCountForPreFlop == Services.PokerRules.cardsPulled.Count)
            {
                Table.gameState = GameState.PreFlop;
                Services.PokerRules.checkedFlop = true;
            }
            else if (Services.PokerRules.cardsPulled.Count > players.Count * 2/* && !Services.Dealer.OutsideVR*/)
            {
                Debug.Log("misdeal here");
                Services.Dealer.TriggerMisdeal();
            }
            else
            {
                Services.PokerRules.CorrectMistakesPreFlop(1f);
                Table.gameState = GameState.PreFlop;
                Services.PokerRules.checkedPreFlop = true;
            }
            checkedPreFlopCardCount = false;
        }
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
		
        //Debug.Log("Starting round " + Table.gameState);
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
            //Debug.Log("player to act = " + playerToAct);
        }
        else
        {
            playerToAct = FindFirstPlayerToAct(1);
            //Debug.Log("player to act = " + playerToAct);
        }
    }

    public PokerPlayerRedux FindFirstPlayerToAct(int distance)
    {
        //Debug.Log("FindingFirstPlayerToAct");
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
    IEnumerator WaitForShowDown()
    {
        while(playerToAct != null)
        {
            yield return null;
        }
        readyForShowdown = true;
        yield break;
    }

    public void WaitingToGrabCardsOn_MisDeal()
    {
        if (cleaningCards)
        {
            //Debug.Log("entering CleaningCards");
            if (!flyingClicked)
            {
                flyingClicked = true;
                startUpTime = Time.time;
                GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
                if (cardsOnTable.Length != 0)
                {
                    foreach (GameObject card in cardsOnTable)
                    {
                        Destroy(card.GetComponent<ConstantForce>());
                        Destroy(card.GetComponent<BoxCollider>());
                        Destroy(card.GetComponent<Rigidbody>());
                        card.GetComponent<Card>().readyToFloat = false;
                        card.GetComponent<Card>().is_flying = true;
                        card.GetComponent<Card>().StartPulse();
                    }
                }
            }
        }
        CleanUpCards();
    }

    public void CleanUpCards()
    {
        if (cleaningCards)
        {
            if (flyingClicked)
            {
                Vector3 modPos = new Vector3(0, 0.03f, 0);
                GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
                GameObject cardDeck = GameObject.FindGameObjectWithTag("CardDeck");
                if (first_time)
                {

                    //Debug.Log("first time values set");
                    foreach (GameObject card in cardsOnTable)
                    {
                        card.GetComponent<Card>().InitializeLerp(cardDeck.transform.position + modPos);
                    }
                    first_time = false;
                }
                foreach (GameObject card in cardsOnTable)
                {   //BUG HERE
                    StartCoroutine(card.GetComponent<Card>().LerpCardPos(cardDeck.transform.position + modPos, cardMoveSpeed));
                    if (card.transform.position == cardDeck.transform.position + modPos)
                    {
                        card.GetComponent<Card>().lerping = false;
                        card.GetComponent<Card>().is_flying = false;
                        Destroy(card);
                        cardDeck.GetComponent<CardDeckScript>().MakeDeckLarger();
                    }
                }
                if (cardsOnTable.Length == 0)
                {
                    flyingClicked = false;
                    first_time = true;
                    Table.instance.RestartRound();
                    cleaningCards = false;
                }
            }
        }
    }

    public void WaitingToGrabCardsOn_ThrownDeck()
    {
        if (killingCards)
        {
            //Debug.Log("entering KillingCards");
            if (!flyingClicked)
            {
                flyingClicked = true;
                startUpTime = Time.time;
                GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
                if (cardsOnTable.Length != 0)
                {
                    foreach (GameObject card in cardsOnTable)
                    {
                        card.GetComponent<Card>().rotSpeed = UnityEngine.Random.Range(500, 1000);
                        card.GetComponent<Card>().cardMoveSpeed = UnityEngine.Random.Range(0.25f, 1.25f);
                        Destroy(card.GetComponent<ConstantForce>());
                        card.GetComponent<Rigidbody>().useGravity = false;
                        card.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        Destroy(card.GetComponent<BoxCollider>());
                        card.GetComponent<Card>().readyToFloat = false;
                        card.GetComponent<Card>().is_flying = true;
                        card.GetComponent<Card>().StartPulse();

                    }
                }
            }
        }
        GrabAndKillCards();
    }

    public void GrabAndKillCards()
    {
        if (killingCards)
        {
            if (flyingClicked)
            {
                float timeSinceClick = Time.time - startUpTime;
                GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
                GameObject cardDeck = GameObject.Find("ShufflingArea");
                float handDistance = Vector3.Distance(hand1.transform.position, hand2.transform.position);
                Vector3 newRotPos = Vector3.zero;

                if (timeSinceClick <= flyTime)
                {
                    Vector3 rotPos = GameObject.Find("Table").transform.position;
                    foreach (GameObject card in cardsOnTable)
                    {
                        card.transform.RotateAround(rotPos, Vector3.up, cardMoveSpeed * card.GetComponent<Card>().rotSpeed * Time.deltaTime);
                        Vector3 randomRot = new Vector3(UnityEngine.Random.Range(100, 360), UnityEngine.Random.Range(100, 360), UnityEngine.Random.Range(100, 360));
                        card.transform.Rotate(randomRot * cardMoveSpeed * 2 * Time.deltaTime);
                        float step = card.GetComponent<Card>().cardMoveSpeed * 2 * Time.deltaTime;
                        if (card.transform.position.y < Camera.main.transform.position.y + 0.25f)
                        {
                            card.transform.position = Vector3.MoveTowards(card.transform.position, rotPos + new Vector3(0 + rotPos.x, 1 + rotPos.y, 0 + rotPos.z), step);
                        }
                        else
                        {
                            newRotPos = new Vector3(rotPos.x, transform.position.y, rotPos.z);
                            card.GetComponent<Card>().InitializeRotation(newRotPos, true);
                        }
                    }
                    holdRotate = true;
                }
                else if (timeSinceClick <= flyTime + 0.5f || holdRotate)
                {
                    
                    if (holdRotate)
                    {
                        handDistance = Mathf.Pow(handDistance, 1.2f);
                    }
                    else handDistance = 1;
                    if (handDistance >= 1.5f) handDistance = 1.5f;
                    else if (handDistance <= .5f) handDistance = .5f;
                    Vector3 rotPos = GameObject.Find("Table").transform.position;
                    foreach (GameObject card in cardsOnTable)
                    {
                        card.GetComponent<Card>().radius = card.GetComponent<Card>().constRadius + handDistance;
                        card.transform.position = card.GetComponent<Card>().ManualRotation(card.GetComponent<Card>().rotSpeed / 400 * handDistance, rotPos);
                        //card.transform.RotateAround(rotPos, Vector3.up, cardMoveSpeed * 40 * Time.deltaTime * handDistance);
                        Vector3 randomRot = new Vector3(UnityEngine.Random.Range(100, 360), UnityEngine.Random.Range(100, 360), UnityEngine.Random.Range(100, 360));
                        card.transform.Rotate(randomRot * handDistance * Time.deltaTime);
                    }
                    StartCoroutine(HoldForSpin());
                }
                else if (!holdRotate)
                {
                    if (first_time)
                    {
                        //Debug.Log("handdistance = " + handDistance);
                        foreach (GameObject card in cardsOnTable)
                        {
                            card.GetComponent<Card>().flying_start_time = Time.time;
                            card.GetComponent<Card>().flight_journey_distance = Vector3.Distance(card.transform.position, cardDeck.transform.position);
                            card.GetComponent<Card>().flying_start_position = card.transform.position;
                            card.GetComponent<Card>().lerping = false;
                        }
                        first_time = false;
                    }
                    foreach (GameObject card in cardsOnTable)
                    {
                        //float step = cardMoveSpeed * Time.deltaTime;
                        card.GetComponent<Card>().lerping = false;
                        float timeElapsed = (Time.time - card.GetComponent<Card>().flying_start_time) * (cardMoveSpeed * 5);
                        float fracJourney = timeElapsed / card.GetComponent<Card>().flight_journey_distance;
                        card.transform.position = Vector3.Lerp(card.GetComponent<Card>().flying_start_position, cardDeck.transform.position, fracJourney);
                        if (card.transform.position == cardDeck.transform.position)
                        {
                            if (!madeNewDeck)
                            {
                                Destroy(card);
                                card.GetComponent<Card>().is_flying = false;
                                newCardDeck = Instantiate(Services.PrefabDB.CardDeck, cardDeck.transform.position, cardDeck.transform.rotation) as GameObject;
                                newCardDeck.GetComponent<CardDeckScript>().BuildDeckFromOneCard(newCardDeck);
                                madeNewDeck = true;
                            }
                            if (madeNewDeck == true)
                            {
                                Destroy(card);
                                card.GetComponent<Card>().is_flying = false;
                                newCardDeck.GetComponent<CardDeckScript>().MakeDeckLarger();
                            }
                        }
                    }

                    if (cardsOnTable.Length == 0)
                    {
                        flyingClicked = false;
                        first_time = true;
                        Table.instance.RestartRound();
                        newCardDeck.GetComponent<CardDeckScript>().ResetDeckScale();
                        Services.Dealer.killingCards = false;
                        madeNewDeck = false;
                        thrownChips.Clear();
                    }
                }
            }
        }
    }
    IEnumerator HoldForSpin()
    {
        while(hand1.GetStandardInteractionButton() && hand2.GetStandardInteractionButton())
        {
            holdRotate = true;
            yield return null;
        }
        holdRotate = false;
        yield break;
    }

    public void ChangeMusicSpeed(bool cheating)
    {
        if (audioSources.Count == 0)
        {
            AudioSource[] a = FindObjectsOfType<AudioSource>();
            foreach (AudioSource _a in a)
            {
                audioSources.Add(_a);
            }
        }
        foreach (AudioSource a in audioSources)
        {
            if (cheating) a.pitch = 0.5f;
            else a.pitch = 1;
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
                if (players[i].playerIsAllIn == true && !everyoneFolded)
                {
                    players[i].PushInCards();
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
                if ((!nextPlayer.actedThisRound || nextPlayer.currentBet < LastBet) && nextPlayer.PlayerState == PlayerState.Playing && nextPlayer.chipCount != 0 && !OnlyAllInPlayersLeft())
                {
                    roundFinished = false;
                    previousPlayerToAct = playerToAct;
                    playerToAct = nextPlayer;
                    Debug.Log("nextPlayer to act is player " + playerToAct);
                    //if(OutsideVR) playerToAct.EvaluateHand();
                    break;
                }
            }
            if (roundFinished)
            {
                if (OnlyAllInPlayersLeft())
                {
                    for (int i = 0; i < players.Count; i++)
                    {
                        if(players[i].chipCount == 0 && players[i].PlayerState == PlayerState.Playing)
                        {
                            if(players[i].Hand == null)
                            {
                                players[i].Hand = SetHand(players[i]);
                            }
                        }
                        if (players[i].Hand != null && !everyoneFolded)
                        {
                            players[i].PushInCards();
                        }
                    }
                }
                Services.SoundManager.roundsFinished++; //increment int for tutorial vo based on when players are done betting
                if (LastBet != 0)
                {
                    List<Vector3> positions = new List<Vector3>();
                    List<Vector3> endPosition = new List<Vector3>();
                    for (int i = chipsInPot.Count - 1; i >= 0; i--)
                    {
                        //BUG HERE
                        positions.Add(chipsInPot[i].transform.position);
                        endPosition.Add(Services.ChipManager.chipPositionInPot[i]);
                    }
                    LerpBetChips lerpChips = new LerpBetChips(chipsInPot, positions, endPosition, .5f);
                    if (!everyoneFolded)
                    {
                        ConsolidateChips consolidate = new ConsolidateChips(chipsInPot);
                        lerpChips.Then(consolidate);
                    }
                    tm.Do(lerpChips);
                }
                playerToAct.playerSpotlight.SetActive(false);
                playerToAct = null;
                playersReady = true;
                readyForCards = true;
            }
        }
    }

    public HandEvaluator SetHand(PokerPlayerRedux player)
    {
        if (Table.gameState == GameState.NewRound || Table.gameState == GameState.PreFlop)
        {
            List<CardType> sortedCards = Table.instance.SortPlayerCardsPreFlop(player.SeatPos);
            HandEvaluator playerHand = new HandEvaluator(sortedCards);
            playerHand.EvaluateHandAtPreFlop();
            return playerHand;
        }
        else if (Table.gameState == GameState.Flop)
        {
            List<CardType> sortedCards = Table.instance.SortPlayerCardsAtFlop(player.SeatPos);
            HandEvaluator playerHand = new HandEvaluator(sortedCards);
            playerHand.EvaluateHandAtFlop();
            return playerHand;
        }
        else if (Table.gameState == GameState.Turn)
        {
            List<CardType> sortedCards = Table.instance.SortPlayerCardsAtTurn(player.SeatPos);
            HandEvaluator playerHand = new HandEvaluator(sortedCards);
            playerHand.EvaluateHandAtTurn();
            return playerHand;
        }
        else if (Table.gameState == GameState.River)
        {
            List<CardType> sortedCards = Table.instance.SortPlayerCardsAtRiver(player.SeatPos);
            HandEvaluator playerHand = new HandEvaluator(sortedCards);
            playerHand.EvaluateHandAtRiver();
            return playerHand;
        }
        return null;
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
            players[i].lastAction = PlayerAction.None;
            activePlayers.Add(players[i]);
            //Debug.Log ("Adding " + players[i] + " to active players!");
            List<int> startingStack = Services.ChipManager.SetChipStacks(chipCount);
            Table.instance.AddChipTo(playerDestinations[i], chipCount);
            Services.ChipManager.CreateAndOrganizeChipStacks(startingStack, players[i].parentChips, i);
            if (players.Count == 5)
            {
                players[i].playersLostAgainst = new List<int>(5)
                {
                     0, 0, 0, 0, 0
                };
            }
        }
        Table.instance.DealerPosition = 0;
        Table.instance.SetDealerButtonPos(Table.instance.DealerPosition);
        Table.instance.gameData = new PokerGameData(Table.instance.DealerPosition, players);
        if (startingWithIntro)
        {
            foreach (GameObject o in objectsToHide)
            {
                o.SetActive(false);
            }
            chipsToBring = GameObject.FindGameObjectsWithTag("Chip");
            foreach (GameObject o in chipsToBring)
            {
                o.SetActive(false);
            }
        }
        else
        {
            StartCoroutine(WaitToPostBlinds(.25f));
        }
    }

    public void OpeningCutScene()
    {
        Wait startTimePoof = new Wait(2);
        Wait waitForPoof = new Wait(2);
        SetGameState setGameState = new SetGameState(GameState.NewRound);
        SetObjectActive poofCaseySmoke = new SetObjectActive(objectsToHide[0], true);
        SetObjectActive poofCasey = new SetObjectActive(objectsToHide[1], false);
        SetObjectActive poofZombieSmoke = new SetObjectActive(objectsToHide[2], true);
        SetObjectActive poofZombie = new SetObjectActive(objectsToHide[3], false);
        SetObjectActive poofMinnieSmoke = new SetObjectActive(objectsToHide[4], true);
        SetObjectActive poofMinnie = new SetObjectActive(objectsToHide[5], false);
        SetObjectActive poofNathanielSmoke = new SetObjectActive(objectsToHide[6], true);
        SetObjectActive poofNathaniel = new SetObjectActive(objectsToHide[7], false);
        SetObjectActive poofFloydSmoke = new SetObjectActive(objectsToHide[8], true);
        SetObjectActive poofFloyd = new SetObjectActive(objectsToHide[9], false);
        SetObjectActive poofCards = new SetObjectActive(objectsToHide[10], false);

        PlayPlayerLine nathanielSpeaks = new PlayPlayerLine(players[3], Services.SoundManager.Nathaniel_Intro1);
        PlayPlayerLine floydSpeaks = new PlayPlayerLine(players[4], Services.SoundManager.Floyd_Intro);
        PlayPlayerLine zombieSpeaks = new PlayPlayerLine(players[1], Services.SoundManager.Zombie_Intro);
        PlayPlayerLine minnieSpeaks = new PlayPlayerLine(players[2], Services.SoundManager.Minnie_Intro);
        PlayPlayerLine caseySpeaks = new PlayPlayerLine(players[0], Services.SoundManager.Casey_Intro);
        PlayPlayerLine nathanielSpeaksAgain = new PlayPlayerLine(players[3], Services.SoundManager.Nathaniel_Intro2);
        PlayPlayerLine minnieSpeaksAgain = new PlayPlayerLine(players[2], Services.SoundManager.Minnie_Intro2);

        PostBlinds player1Bets = new PostBlinds(players[SeatsAwayFromDealerAmongstLivePlayers(1)], SmallBlind, false);
        PostBlinds player2Bets = new PostBlinds(players[SeatsAwayFromDealerAmongstLivePlayers(2)], BigBlind, false);
        float randomTime = UnityEngine.Random.Range(.05f, .5f);
        Wait waitForNextPlayer = new Wait(randomTime);

        startTimePoof.
            Then(poofCaseySmoke).
                     Then(new Wait(0.5f)).
            Then(poofCasey).
                     Then(new Wait(1)).
            Then(poofZombieSmoke).
                     Then(new Wait(0.5f)).
            Then(poofZombie).
                     Then(new Wait(1)).
            Then(poofMinnieSmoke).
                     Then(new Wait(0.5f)).
            Then(poofMinnie).
                     Then(new Wait(1)).
            Then(poofNathanielSmoke).
                     Then(new Wait(0.5f)).
            Then(poofNathaniel).
                     Then(new Wait(1)).
            Then(poofFloydSmoke).
                     Then(new Wait(0.5f)).
            Then(poofFloyd).
                     Then(new Wait(0.5f)).
            Then(nathanielSpeaks).
                     Then(new Wait(Services.SoundManager.Nathaniel_Intro1.length)).
            Then(floydSpeaks).
                     Then(new Wait(Services.SoundManager.Floyd_Intro.length)).
            Then(zombieSpeaks).
                     Then(new Wait(Services.SoundManager.Zombie_Intro.length)).
            Then(minnieSpeaks).
                     Then(new Wait(Services.SoundManager.Minnie_Intro.length)).
            Then(caseySpeaks).
                     Then(new Wait(Services.SoundManager.Casey_Intro.length)).
            Then(nathanielSpeaksAgain).
                     Then(new Wait(Services.SoundManager.Nathaniel_Intro2.length)).
            Then(minnieSpeaksAgain).
                     Then(new Wait(Services.SoundManager.Minnie_Intro2.length)).
            Then(setGameState).
            Then(poofCards).
            Then(new TurnOnChipsFromTutorial()).
            Then(new TurnOnTutorial()).
            Then(player1Bets).
            Then(waitForNextPlayer).
            Then(player2Bets);

        tm.Do(startTimePoof);
        LastBet = BigBlind; 
    }

    public IEnumerator WaitToPostBlinds(float time)
    {
        yield return new WaitForSeconds(time);
        SetBlinds();
    }
    public void SetBlinds()
    {
        //Debug.Log("posting blinds");
        PostBlinds player1Bets = new PostBlinds(players[SeatsAwayFromDealerAmongstLivePlayers(1)], SmallBlind, false);
        PostBlinds player2Bets = new PostBlinds(players[SeatsAwayFromDealerAmongstLivePlayers(2)], BigBlind, false);
        float randomTime = UnityEngine.Random.Range(.05f, .5f);
        Wait waitForNextPlayer = new Wait(randomTime);

        player1Bets.
            Then(waitForNextPlayer).
            Then(player2Bets);
        tm.Do(player1Bets);

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

        PlayerRank = new List<List<PokerPlayerRedux>>();

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

    public IEnumerator WaitForWinnersToGetPaid()
    {
        handsCompleted += 1;
        Debug.Assert(numberOfWinners > 0);
        List<PokerPlayerRedux> winningPlayers = new List<PokerPlayerRedux>();
        int potAmount = Table.instance.potChips;
        for (int i = 0; i < players.Count; i++)
        {
            PokerPlayerRedux playerToCheck = players[SeatsAwayFromDealer(i + 1)];
            if (playerToCheck.PlayerState == PlayerState.Winner)
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
                if (winningPlayers[i].chipsWon > (winningPlayers[i].maxWinnings * PlayersInPot()) && winningPlayers[i].maxWinnings != 0 && winningPlayers[i].playerIsAllIn)
                {
                    winningPlayers[i].chipsWon = winningPlayers[i].maxWinnings * PlayersInPot();
                }
            }
            else
            {
                winningPlayers[i].chipsWon = potAmountToGiveWinner;
                if (winningPlayers[i].chipsWon > (winningPlayers[i].maxWinnings * PlayersInPot()) && winningPlayers[i].maxWinnings != 0 && winningPlayers[i].playerIsAllIn)
                {
                    winningPlayers[i].chipsWon = winningPlayers[i].maxWinnings * PlayersInPot();
                }
            }
            potRemaining -= winningPlayers[i].chipsWon;
        }
        if (winningPlayers.Count >= 2)
        {
            foreach (Chip chip in chipsInPot)
            {
                if (chip != null)
                {
                    //Destroy(chip.gameObject);
                    Services.ChipManager.chipsToDestroy.Add(chip.gameObject);
                }
                else Debug.Log("You are trying to Destroy a chip that is alread DEAD");
            }
            for (int i = 0; i < winningPlayers.Count; i++)
            {
                List<int> splitPot = Services.ChipManager.PrepChipsForSplit(winningPlayers[i].chipsWon);
                Services.ChipManager.SplitPot(splitPot, winningPlayers[i].SeatPos, chipsInPot);
            }
        }
        Debug.Log("number of Winners is " + numberOfWinners);
        //added this in because of voiceActing, and not wanting two clips playing at the same time
        if (winningPlayers.Count == 2)
        {
            Debug.Log("DONT SAY A FUCKING WORD");
        }
        else
        {
            float randomNum = UnityEngine.Random.Range(0, 100);
            if (randomNum < 50)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].PlayerState == PlayerState.Winner)
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
        tipMultiplier += 1f;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].PlayerState == PlayerState.Winner)
            {
                players[i].lossCount = 0;
                players[i].Tip();
            }
            else if (players[i].PlayerState == PlayerState.Loser)
            {
                players[i].lossCount++;
                for (int winners = 0; winners < winningPlayers.Count; winners++)
                {
                    players[i].playersLostAgainst[winningPlayers[winners].SeatPos]++;
                }
            }
        }
        PokerPlayerRedux losingPlayer = null;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].chipCount <= 0 && players[i].PlayerState == PlayerState.Loser)
            {
                //Services.SoundManager.InterruptChaos();
                //DAN PUT THE "BUY ME BACK IN LINE HERE"
                Debug.Log("LOSER LINES FOR LOSERS");
                //Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(players[i].playerName, LineCriteria.BuyInAsk));
                playerHasBeenEliminated = true;
                losingPlayer = players[i];
                //losingPlayer.gameObject.SetActive(false);
                DragMeToHell(losingPlayer, losingPlayer.gameObject);
                //Services.AnimationScript.ConvoAnimation(losingPlayer.playerName, "Idle", false);
                //Services.AnimationScript.ConvoAnimation(losingPlayer.playerName, "BuyBackIn", true);
                //StartCoroutine(WaitToSave(10f));
            }
        }
        //while (playerHasBeenEliminated)
        //{
        //    yield return null;
        //}
        //if (losingPlayer != null)
        //{
        //    Services.AnimationScript.ConvoAnimation(losingPlayer.playerName, "Idle", true);
        //    Services.AnimationScript.ConvoAnimation(losingPlayer.playerName, "BuyBackIn", false);
        //}
        Table.gameState = GameState.PostHand;
        yield break;
    }

    public void DragMeToHell(PokerPlayerRedux player, GameObject playerObj)
    {
        player.scaryTentacles.SetActive(true);
        Vector3 finalDestination = new Vector3(playerObj.transform.position.x, playerObj.transform.position.y - 10, playerObj.transform.position.z);
        Wait waitToDrag = new Wait(2f);
        LerpPos dragPlayer = new LerpPos(playerObj, playerObj.transform.position, finalDestination, 1f);

        waitToDrag.Then(dragPlayer);
        tm.Do(waitToDrag);
    }

    public int PlayersInPot()
    {
        int playersInPot = 0;
        foreach (PokerPlayerRedux player in players)
        {
            if (player.PlayerState == PlayerState.Winner || player.PlayerState == PlayerState.Loser)
            {
                playersInPot++;
            }
        }
        return playersInPot;
    }
    IEnumerator WaitToSave(float time)
    {
        yield return new WaitForSeconds(time);
        playerHasBeenEliminated = false;
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
            if (player.PlayerState == PlayerState.Winner)
            {
                winnerChipStack = player.ChipCountToCheckWhenWinning + player.chipsWon;
                ////
                //
                //
                //IMPORTANT DEBUG
                Debug.Log("for player" + player.SeatPos + " the winnerChipStack = " + winnerChipStack + " and the Player has" + player.chipCount);
                //
                ///
                ///
                //
                if (OutsideVR)
                {
                    Table.instance.AddChipTo(playerDestinations[player.SeatPos], winnerChipStack);
                }
                if (player.chipCount >= winnerChipStack && player.HasBeenPaid == false)
                {
                    winnersPaid++;
                    player.HasBeenPaid = true;
                }
            }
            else if (player.PlayerState == PlayerState.Loser)
            {
                winnerChipStack = player.ChipCountToCheckWhenWinning + potAmountToGiveWinner;
                if (player.chipCount == winnerChipStack)
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
                if (players[i].PlayerState == PlayerState.Winner)
                {
                    for (int playersChecked = 0; playersChecked < players.Count; playersChecked++)
                    {
                        if (players[playersChecked].PlayerState == PlayerState.Winner)
                        {
                            winnerCount++;
                        }
                    }
                    if (!players[i].playerAudioSource.isPlaying &&
                       !players[i].playerIsInConversation &&
                       !Services.SoundManager.conversationIsPlaying &&
                       winnerCount < 2)
                    {
                        Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(players[i].playerName, LineCriteria.Tip));
                        //Services.SoundManager.GetSourceAndPlay(players[i].playerAudioSource, players[i].tipAudio);
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
            players[i].continuationBet = 0;
            players[i].percievedHandStrength = 0;
            players[i].moneyCommitted = 0;
            players[i].gaveTip = false;
            players[i].lastAction = PlayerAction.None;
            //players[i].checkedHandStrength = false;
        }
        Services.PokerRules.cardsPulled.Clear();
        Services.PokerRules.cardsLogged.Clear();
        Services.PokerRules.thrownCards.Clear();
        Services.Dealer.PlayerRank.Clear();
        cardsTouchingTable.Clear();
        Table.gameState = GameState.NewRound;
        playersHaveBeenEvaluated = false;
        winnersHaveBeenPaid = false;
        readyToAwardPlayers = false;
        finalHandEvaluation = false;
		misdealAudioPlayed = false;
        everyoneFolded = false;
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
                //Services.ChipManager.chipsToDestroy.Add(chip.GetComponent<Chip>());
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].PlayerState == PlayerState.Playing)
            {
                List<int> newStacks = Services.ChipManager.SetChipStacks(players[i].chipCount);
                GameObject[] chipsToDestroy = GameObject.FindGameObjectsWithTag("Chip");
                Services.ChipManager.CreateAndOrganizeChipStacks(newStacks, players[i].parentChips, i);
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
            players[i].continuationBet = 0;
            players[i].percievedHandStrength = 0;
            players[i].moneyCommitted = 0;
            players[i].gaveTip = false;
            players[i].lastAction = PlayerAction.None;
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
        everyoneFolded = false;
        roundStarted = false;
        raisesInRound = 0;
        Services.PokerRules.TurnOffAllIndicators();
        GameObject[] chipsOnTable = GameObject.FindGameObjectsWithTag("Chip");
        if (chipsOnTable.Length > 0)
        {
            foreach (GameObject chip in chipsOnTable)
            {
                Destroy(chip);
                //Services.ChipManager.chipsToDestroy.Add(chip.GetComponent<Chip>());
            }
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].PlayerState == PlayerState.Playing)
            {
                List<int> newStacks = Services.ChipManager.SetChipStacks(players[i].chipCount);
                //GameObject[] chipsToDestroy = GameObject.FindGameObjectsWithTag("Chip");
                //foreach (GameObject chip in chipsToDestroy)
                //{
                //    Services.ChipManager.chipsToDestroy.Add(chip);
                //}
                Services.ChipManager.CreateAndOrganizeChipStacks(newStacks, players[i].parentChips, i);
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
}

       