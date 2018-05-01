using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//this is the individual state of each player and controls how the game can flow
//I've had to add some states here and there as they pop up
//the most important one is "Playing" because if they aren't marked as playing, then they'll be skipped
public enum PlayerState {Playing, NotPlaying, Winner, Loser, Eliminated}
public enum PlayerName {None, Casey, Zombie, Minnie, Nathaniel, Floyd}
public enum PlayerAction { Fold, Call, Raise, None}
public enum LineCriteria {None, AllIn, Bet, BuyIn, BuyInAsk, Call, CardHit, Check, ChipsMoved, FiftyTwo, Fold, IdleTime, Lose, Misdeal, Raise, ThrowsChips, Tip, TouchedByHand, Win, WrongChips}


public class PokerPlayerRedux : MonoBehaviour{

    public GameObject turnIndicator;
    public PlayerName playerName;
    public PlayerAction lastAction;
    public float percievedHandStrength = 0;
    public GameObject playerSpotlight;
    public GameObject playerCardIndicator;
    public List<GameObject> parentChips;
    public GameObject[] cardPos;
    public int cardsReplaced = 0;
    public bool isAggressor = false;
    public int timesRaisedThisRound = 0;
    public bool gaveTip = false;
    [HideInInspector]
    public List<int> playersLostAgainst;
    [HideInInspector]
    public int lossCount;
    public int maxWinnings;
    public int moneyCommitted;
    //what position they are at the table, this is set in Dealer and is massively important
    //this is the current means by which we differentiate between which instance of PokerPlayerRedux we're currently working with
    public int SeatPos { get; set; }

    //this lets me keep track of the players chip count, but only when I call ChipCount, so it may be less reliable than it can be
    public int chipCount { get { return ChipCount; } set { chipCount = value; } }
    private int ChipCount
    {
        get { return Table.instance.playerChipStacks[SeatPos]; }
        set { }
    }

    //this is the Hand of the player and holds ALL the information about thier hand
    //it has the hand Total, the HighCard, the PokerHand enum, and all relevant hand info
    public HandEvaluator Hand { get; set; }

    //this is the float that determines their handStrength, it's a number from 0-1.
    [HideInInspector]
    public float HandStrength;

    //the player's current state
    [HideInInspector]
    public PlayerState PlayerState { get; set; }

    //the following variables are kind of hopshod and are used in a variety of functions to keep the flow

    //when players are being paid, this keeps track of whether a player has been paid or not
    [HideInInspector]
    public bool HasBeenPaid;

    //when awarding players, we have to know what their chipstack is BEFORE they have the money they won
    //this is used to log that.
    [HideInInspector]
    public int ChipCountToCheckWhenWinning;

    [HideInInspector]
    public bool playerIsAllIn = false;

    //we want to know what the chipCount before going all in is
    //so that we can check if we need to make change for that player
    [HideInInspector]
    public int chipCountBeforeAllIn;

    //this is the amount the player has won in a given pot.
    //typically it's equal to the pot, but sometimes it's divided by 2 or 3 or maybe even more
    [HideInInspector]
    public int chipsWon;

    [HideInInspector]
    public bool playerLookedAt = false;

    //this is the the last amount of money that the player has bet in a round.
    //not to be confused with "lastBet" on Dealer, this keeps track of ONLY what the player's last best was
    //as opposed to LastBest, in dealer, which keeps track of the last bet any player has made. 
    [HideInInspector]
    public int currentBet;

    [HideInInspector]
    public int continuationBet = 0;

    //this is actually super important and is the crux of figuring out whether a player will make the FCR (Fold/Call/Raise) decision. 
    [HideInInspector]
    public float rateOfReturn;

    //this let's me know if a player's turn has been completed
    //if they have met the last, or folded, they are marked as completing their turn
    //if someone acts after in a way that would require the player to act again, turnComplete is set to false
    [HideInInspector]
    public bool turnComplete;

    //this is similar to turnComplete, but is only set once, once the player has acted at all. 
    //this is a means by which we can check if all players have acted before we can move onto the next round
    [HideInInspector]
    public bool actedThisRound;

    [HideInInspector]
    public bool flippedCards;

    //this is the int we use in order to determine how much a player will raise in a given situation
    //the function that uses this needs to be reexamined I think
    [HideInInspector]
    public int amountToRaise;

    public bool waitingToGetPaid = false;


	[Header("Voice Lines")]
	public AudioSource playerAudioSource;
	public bool playerIsInConversation = false;
	public AudioClip fiftyTwoAudio;
	public AudioClip allInAudio;
	public AudioClip betAudio;
	public AudioClip callAudio;
	public AudioClip cardHitAudio;
	public AudioClip checkAudio;
	public AudioClip foldAudio;
	public AudioClip misdealAudio;
	public AudioClip raiseAudio;
	public AudioClip tipAudio;
	public AudioClip tip2Audio;
	public AudioClip winAudio;
	public AudioClip wrongCardAudio;
	public AudioClip wrongChipsAudio;
    //public AudioClip loseAudio;

    private TaskManager tm;
    private bool MakeThemAllIn = false;
    private bool playedSound;


    //this is here so that I can run for-loops and access the functions from Table that use the playerDest enum
    [HideInInspector]
    public List<Destination> playerDestinations;

	//this is a public PokerPlayerRedux used in initialization, but also to create fake players for determining handstrength
	public PokerPlayerRedux(int seatPos)
	{
		SeatPos = seatPos;
		PlayerState = PlayerState.Playing;
	}
    //
    //This causes the player to Fold
    //first the player says fold
    //then it grabs each card in the players hand and places it where the player would normally bet
    //this visually indicates that the player folded
    //we set the players playerstate to "Not Playing"
    //set their hands to null, since they no longer have a hand.
    //after that, we check whether that was the LAST player to fold
    //if the player folded, and there is only one player left, that player becomes the winner
    //so we set the game to CleanUp and run the function used to award players their winnings
    void Start()
    {
        tm = new TaskManager();
        playerDestinations = new List<Destination>
        {
            Destination.player0, Destination.player1, Destination.player2, Destination.player3, Destination.player4
        };

        switch (gameObject.name)
        {
            case "Casey":
                playerName = PlayerName.Casey;
                break;
            case "Zombie":
                playerName = PlayerName.Zombie;
                break;
            case "Minnie":
                playerName = PlayerName.Minnie;
                break;
            case "Nathaniel":
                playerName = PlayerName.Nathaniel;
                break;
            case "Floyd":
                playerName = PlayerName.Floyd;
                break;
            default:
                break;
        }

        parentChips = new List<GameObject>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) MakeThemAllIn = true;
        tm.Update();
        if (playerAudioSource.isPlaying) playerSpotlight.SetActive(true);
        else playerSpotlight.SetActive(false);

        if (ChipCount <= 0)
        {
            Table.instance.playerChipStacks[SeatPos] = 0;
        }

        if (Services.Dealer.playerToAct == this && Table.gameState != GameState.Intro)
        {
            if (!playedSound)
            {
                playedSound = true;
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.clockTick, .4f);
            }
            if(turnIndicator != null) turnIndicator.SetActive(true);
        }
        else
        {
            playedSound = false;
            if(turnIndicator != null) turnIndicator.SetActive(false);
        }
    }

    //so this is the function that calls all the organization functions, evaluation functions, and handStrength
    //basically this is what happens on each players turn no matter what if they are playing
    //first we check if they are playing
    //then we check what gamestate we're in, so we can call the right functions (this might be a good place for delegates)
    //we get the sorted cards from the Table
    //we make a new handEvaluator with those sorted cards
    //then we evaluate that hand
    //and set the evaluater as the player's hand.
    //then we Determine the hand strength of the player, which decides whether they Fold Call or Raise
    public void EvaluateHand()
    {
        if (PlayerState == PlayerState.Playing)
        {
            if (Table.gameState == GameState.PreFlop)
            {
                turnComplete = false;
                List<CardType> sortedCards = Table.instance.SortPlayerCardsPreFlop(SeatPos);
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtPreFlop();
                Hand = playerHand;
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                StartCoroutine(SetNextPlayer());
            }
            else if (Table.gameState == GameState.Flop)
            {
                turnComplete = false;
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtFlop(SeatPos);
                if (sortedCards == null)
                {
                    Debug.Log("misdeal here");
                    Table.gameState = GameState.Misdeal;
                }
                else
                {
                    HandEvaluator playerHand = new HandEvaluator(sortedCards);
                    playerHand.EvaluateHandAtFlop();
                    Hand = playerHand;
                    DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                    StartCoroutine(SetNextPlayer());
                }
            }
            else if (Table.gameState == GameState.Turn)
            {
                turnComplete = false;
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtTurn(SeatPos);
                if (sortedCards == null)
                {
                    Debug.Log("misdeal here");
                    Table.gameState = GameState.Misdeal;
                }
                else
                {
                    HandEvaluator playerHand = new HandEvaluator(sortedCards);
                    playerHand.EvaluateHandAtTurn();
                    Hand = playerHand;
                    DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                    StartCoroutine(SetNextPlayer());
                }
            }
            else if (Table.gameState == GameState.River)
            {
                turnComplete = false;
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtRiver(SeatPos);
                if (sortedCards == null)
                {
                    Debug.Log("misdeal here");
                    Table.gameState = GameState.Misdeal;
                }
                else
                {
                    HandEvaluator playerHand = new HandEvaluator(sortedCards);
                    playerHand.EvaluateHandAtRiver();
                    Hand = playerHand;
                    DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                    StartCoroutine(SetNextPlayer());
                }
            }
        }
    }

    //this finds the rate of return
    //first we find the amount to raise by calling the DetermineRaiseAmount function
    //then we find each necessary variable, and do the equations
    public float FindRateOfReturn()
	{
		//in this case, since we're going to do limit, the bet will always be the bigBlind;
		//else, we need to write another function that determines what the possible bet will be
		amountToRaise = DetermineRaiseAmount(this);
		float potSize = Table.instance.potChips;
        int lastBet = Services.Dealer.LastBet;
        if (lastBet == 0) lastBet = Services.Dealer.BigBlind; 
		float potOdds = amountToRaise / (amountToRaise + potSize);
        //Debug.Log(gameObject.name + " has an HS of " + HandStrength + " and pot odds of " + potOdds + " in round " + Table.gameState);
        if (Table.gameState > GameState.PreFlop && Table.gameState != GameState.Misdeal) Debug.Assert(HandStrength <= 1);
        float returnRate = HandStrength / potOdds;
        //Debug.Log(gameObject.name + " has a return rate of " + returnRate);
		return returnRate;
	}

    public float PossibleOpponentHS()
    {
        float opponentHS = 0;

        if(Services.Dealer.LastBet != Services.Dealer.BigBlind || Services.Dealer.LastBet != 0)
        {
            float potSize = Table.instance.potChips;
            int lastBet = Services.Dealer.LastBet;
            if (lastBet == 0) lastBet = Services.Dealer.BigBlind;
            float potOdds = lastBet / (lastBet + potSize);
            Debug.Log("lastBet = " + Services.Dealer.LastBet + " and pot = " + Table.instance.potChips + " and raisesInRound = " + Services.Dealer.raisesInRound);
            opponentHS = (lastBet / (potSize - lastBet)) / (potOdds);
            Debug.Log("possibleOpponentHS = " + opponentHS);
        }
        return opponentHS;
    }

    //basically this is a KEY function. right now it's using primarily handStrength to determine how much to raise
    //but as we continue I'll need to actually add in a TON of factors when considering how much to raise
    //MUST REWRITE THIS TO ACCOUNT FOR A WHOLE LOT
    public int DetermineRaiseAmount(PokerPlayerRedux player)
    {
        int raise = 0;
        switch (playerName)
        {
            case PlayerName.Casey:
                raise = Services.PlayerBehaviour.CASEY_DetermineRaiseAmount(player);
                break;
            case PlayerName.Zombie:
                raise = Services.PlayerBehaviour.ZOMBIE_DetermineRaiseAmount(player);
                break;
            case PlayerName.Minnie:
                raise = Services.PlayerBehaviour.MINNIE_DetermineRaiseAmount(player);
                break;
            case PlayerName.Nathaniel:
                raise = Services.PlayerBehaviour.NATHANIEL_DetermineRaiseAmount(player);
                break;
            case PlayerName.Floyd:
                raise = Services.PlayerBehaviour.FLOYD_DetermineRaiseAmount(player);
                break;
            default:
                break;
        }
        //raise = Services.PlayerBehaviour.DetermineInitialBetSize(player);
        return raise;
    }

    //this is the FCR decision and this is where we can adjust player types
    //we should go back to the generic one and make percentage variables that we can adjust in individual players
    public void FoldCallRaiseDecision(float returnRate, PokerPlayerRedux player)
    {
        if (MakeThemAllIn == true)
        {
            amountToRaise = chipCount;
            Raise();
            turnComplete = true;
            actedThisRound = true;
        }
        else if (Table.gameState == GameState.PreFlop)
        {
            switch (playerName)
            {
                case PlayerName.Casey:
                    Services.PlayerBehaviour.Casey_Preflop_FCR(player);
                    break;
                case PlayerName.Zombie:
                    Services.PlayerBehaviour.Zombie_Preflop_FCR(player);
                    break;
                case PlayerName.Minnie:
                    Services.PlayerBehaviour.Minnie_Preflop_FCR(player);
                    break;
                case PlayerName.Nathaniel:
                    Services.PlayerBehaviour.Nathaniel_Preflop_FCR(player);
                    break;
                case PlayerName.Floyd:
                    Services.PlayerBehaviour.Floyd_Preflop_FCR(player);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (playerName)
            {
                case PlayerName.Casey:
                    Services.PlayerBehaviour.Casey_FCR(player);
                    break;
                case PlayerName.Zombie:
                    Services.PlayerBehaviour.Zombie_FCR(player);
                    break;
                case PlayerName.Minnie:
                    Services.PlayerBehaviour.Minnie_FCR(player);
                    break;
                case PlayerName.Nathaniel:
                    Services.PlayerBehaviour.Nathaniel_FCR(player);
                    break;
                case PlayerName.Floyd:
                    Services.PlayerBehaviour.Floyd_FCR(player);
                    break;
                default:
                    break;
            }
        }
    }

    public void Fold()
    {
        Services.AnimationScript.ActionAnimation(playerName, "Fold");
        Debug.Log("ENTERED FOLD FUNCTION");
        lastAction = PlayerAction.Fold;
        SayFold();
        foreach (Card card in Table.instance.playerCards[SeatPos])
        {
            card.GetComponent<Renderer>().material.shader = card.dissolve;
            card.transform.GetChild(0).gameObject.SetActive(true);
            LerpBurnProgress burnCard = new LerpBurnProgress(card.gameObject.GetComponent<Renderer>(), 1f, 0f, Easing.FunctionType.QuadEaseOut, 2f);
            tm.Do(burnCard);
            card.cardMarkedForDestruction = false;
            Services.Dealer.deadCardsList.Add(card);
        }
        PlayerState = PlayerState.NotPlaying;
        Hand = null;

        //Debug.Log("Player " + SeatPos + " folded!");
        if (Services.Dealer.GetActivePlayerCount() == 1)
        {
            Table.gameState = GameState.ShowDown;
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if (Services.Dealer.players[i].PlayerState == PlayerState.Playing)
                {
                    Services.Dealer.everyoneFolded = true;
                    Services.Dealer.players[i].PlayerState = PlayerState.Winner;
                    Services.Dealer.numberOfWinners = 1;
                    //Debug.Log("current chip stack is at " + chipCount);
                    Services.Dealer.players[i].ChipCountToCheckWhenWinning = Services.Dealer.players[i].chipCount;
                    //Debug.Log("We are getting into the fold and the chipCountToCheckWhenWinning = " + ChipCountToCheckWhenWinning);
                    Services.Dealer.playersReady = true;
                    Services.Dealer.playersHaveBeenEvaluated = true;
                    //Services.Dealer.WaitForWinnersToGetPaid();
                    Services.Dealer.StartCoroutine(Services.Dealer.WaitForWinnersToGetPaid());
                }
            }
        }
    }
    public void Call()
    {
        Debug.Log("ENTERED CALL FUNCTION");
        lastAction = PlayerAction.Call;
        if (chipCount > 0)
        {
            //Debug.Log("currentBet = " + currentBet);
            int betToCall = Services.Dealer.LastBet - currentBet;
            if (chipCount - betToCall <= 0)
            {
                AllIn();
                currentBet = chipCount;
                Services.Dealer.LastBet = currentBet;
                //Debug.Log("Player " + SeatPos + " didn't have enough chips and went all in for " + chipCount);
            }
            else
            {
                //Debug.Log("betToCall = " + betToCall);
                if (betToCall == 0)
                {
                    Services.AnimationScript.ActionAnimation(playerName, "Check");
                    SayCheck();
                }
                else
                {
                    Services.AnimationScript.ActionAnimation(playerName, "Call");
                    SayCall();
                }
                Bet(betToCall, false);
                currentBet = betToCall + currentBet;
                moneyCommitted += betToCall;
                Services.Dealer.LastBet = currentBet;
                //Debug.Log("Player " + SeatPos + " called " + betToCall);
                //Debug.Log("and the pot is now at " + Table.instance.potChips);
            }
        }
    }

    public void Raise()
    {
        Services.AnimationScript.ActionAnimation(playerName, "Raise");
        lastAction = PlayerAction.Raise;
        Services.Dealer.raisesInRound++;
        int aggressors = 0;
        timesRaisedThisRound++;
        if (!isAggressor)
        {
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if (Services.Dealer.players[i].isAggressor)
                {
                    aggressors++;
                }
                if (aggressors == 0) isAggressor = true;
            }
        }

        if (chipCount > 0)
        {
            int raiseAmount = amountToRaise;
            int betToRaise = 0;
            int lastBet = Services.Dealer.LastBet;
            betToRaise = lastBet + (raiseAmount - currentBet);
            int remainder = betToRaise % ChipConfig.RED_CHIP_VALUE;
            if (remainder > 0) betToRaise = (betToRaise - remainder) + ChipConfig.RED_CHIP_VALUE;
            if (chipCount - betToRaise <= 0)
            {
                AllIn();
                //Debug.Log("Player " + SeatPos + " didn't have enough chips and went all in for " + chipCount);
                currentBet = betToRaise + currentBet;
                Services.Dealer.LastBet = currentBet;
                Debug.Log("player " + SeatPos + " raises " + betToRaise);
                //Debug.Log("Player " + SeatPos + " raised!");
                //Debug.Log("and the pot is now at " + Table.instance.potChips);
                //Debug.Log("and player " + SeatPos + " is now at " + chipCount);
            }
            else
            {
                if (Services.Dealer.LastBet == 0)
                {
                    Debug.Log("Saying Bet");
                    SayBet();
                }
                else
                {
                    Debug.Log("Saying Raise");
                    SayRaise();
                }
                Bet(betToRaise, false);
                continuationBet = betToRaise;
                currentBet = betToRaise + currentBet;
                moneyCommitted += betToRaise;
                Services.Dealer.LastBet = currentBet;
                Debug.Log("player " + SeatPos + " raises " + betToRaise);
                Debug.Log("Player " + SeatPos + " raised!");
                Debug.Log("and the pot is now at " + Table.instance.potChips);
                Debug.Log("and player " + SeatPos + " is now at " + chipCount);
            }
        }
    }

    public void AllIn()
    {
        Debug.Log("ENTERED ALL IN FUNCTION");
        SayAllIn();
        foreach (Card card in Table.instance.playerCards[SeatPos])
        {
            card.cardMarkedForDestruction = false;
        }
        chipCountBeforeAllIn = chipCount;
        maxWinnings += chipCount;
        playerIsAllIn = true;
        //Debug.Log("getting ready to go all in");
        Bet(chipCount, false);
        //Debug.Log("Player " + SeatPos + " folded!");
        if (Services.Dealer.GetActivePlayerCount() == 2)
        {
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if (Services.Dealer.players[i].PlayerState == PlayerState.Playing && Services.Dealer.players[i] != this)
                {
                    if(Services.Dealer.players[i].currentBet > chipCountBeforeAllIn)
                    {
                        Table.instance.AddChipTo(playerDestinations[i], (Services.Dealer.players[i].currentBet - chipCountBeforeAllIn));
                        Table.instance.potChips -= ((Services.Dealer.players[i].currentBet - chipCountBeforeAllIn));
                    }
                }
            }
        }
        Services.Dealer.CheckAllInPlayers();
    }

    IEnumerator SetNextPlayer()
    {
        while (!turnComplete)
        {
            yield return null;
        }
        //Debug.Log("calling set next player from player");
        Services.Dealer.SetNextPlayer();
        playerLookedAt = false;
        yield break;
    }

	#region These are all the functions that deal with just flipping the cards

	//this is the initial call for flipping the cards
	//it gets a list of the cards as gameobjects, so that we can manipulate them
	//then we go through the cards, and if the card is not flipped (which is a bool on each card that indicates it's direction), we can flip it
	//we ignore the collision between the cards so they don't flip out
	//and then we call the coroutine that actually lerps the flip
	//once the cards are flipped, we call another coroutine that repositions them for readability
	//the flipping works fine, but we need to fine tune the WaitForReposition coroutine
	public void PushInCards()
	{
        flippedCards = true;
		List<GameObject> cardsInHand = Table.instance.GetCardGameObjects(SeatPos);
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            Physics.IgnoreCollision(cardsInHand[0].gameObject.GetComponent<Collider>(), cardsInHand[1].gameObject.GetComponent<Collider>());
            Services.Dealer.StartCoroutine
                (FlipCardsAndMoveTowardsBoard(.5f, cardsInHand[i], 
                (GameObject.Find("TheBoard").GetComponent<Collider>().ClosestPointOnBounds(cardsInHand[i].transform.position) + cardsInHand[i].transform.position) / 2, 
                 SeatPos));
            Services.Dealer.StartCoroutine(WaitForReposition(.5f, .5f, cardsInHand[0], cardsInHand[1], SeatPos));
        }
	}

    //does what the name says. we pass the parameters which are unique to each players
    //we want to make sure the cards all flip TOWARDS the board, so we have to math
    //we get the initial position of the cards, and the initial rotation
    //then we get the target rotation of the cards using trig.
    //I kinda don't know exactly how that works, and it doesn't even work super right. 
    //but it's necessary because we want the cards to flip properly for each player
    //then we just lerp that shit
    IEnumerator FlipCardsAndMoveTowardsBoard(float duration, GameObject card, Vector3 targetPos, int seatPos)
    {
        float timeElapsed = 0;
        Vector3 initialPos = card.transform.position;
        Quaternion initialRot = card.transform.rotation;
        float targetYRot = Mathf.Atan2(targetPos.x - initialPos.x, targetPos.z - initialPos.z) * Mathf.Rad2Deg;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            card.transform.rotation = Quaternion.Lerp(initialRot, Quaternion.Euler(90, targetYRot, initialRot.eulerAngles.z), timeElapsed / duration);
            card.transform.position = Vector3.Lerp(initialPos, targetPos, timeElapsed / duration);
            yield return null;
        }
        Card cs = card.GetComponent<Card>(); //cs = cardScript
    }

	//this calls the proper coroutine, which allows the cards to be spread apart so that they aren't overlapping
	//we want to have a WaitForSeconds because we need the cards to be flipped before we can reposition them
	IEnumerator WaitForReposition(float time, float duration, GameObject card1, GameObject card2, int seatPos)
	{
		yield return new WaitForSeconds(time);
		Services.Dealer.StartCoroutine(RepositionCardsForReadability(duration, card1, card2, seatPos));
	}

	//okay, so this is a little clunky
	//basically we want to move the cards the proper distance away from each other.
	//but because of the way circles work, that means that sometimes that requires us to change the x position, and sometimes the z position
	//so we have a unitsToMove float which determines exactly how much the card should move.
	//we get the cardPos for each card and then calculate the distance between the two
	//then we get lots of if statements that are hard coded AF.
	//seat pos 2 is special in that it requires us to use the z axis.
	//so if it's not seat 2, then we know we're working on the x, then basically we check the distance and move one card the correct units
	//if it is seat 2, we do the same thing, but along the z axis
	//it's stupid and we should definitely rethink how we're flipping cards and repositioning them

	IEnumerator RepositionCardsForReadability(float duration, GameObject card1, GameObject card2, int seatPos)
	{
        if (card1 != null && card2 != null)
        {
            float timeElapsed = 0;
            float unitsToMove = 0.1f;
            Vector3 card1Pos = card1.transform.position;
            Vector3 card2Pos = card2.transform.position;
            float distanceBetweenCards = Vector3.Distance(card1Pos, card2Pos);
            if (distanceBetweenCards < 1)
            {
                if (seatPos != 2)
                {
                    if (card1Pos.x - card2Pos.x > 0)
                    {
                        while (timeElapsed < duration)
                        {
                            timeElapsed += Time.deltaTime;
                            card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x + unitsToMove, card2Pos.y, card2Pos.z), timeElapsed / duration);
                            yield return null;
                        }
                    }
                    else if (card1Pos.x - card2Pos.x < 0)
                    {
                        while (timeElapsed < duration)
                        {
                            timeElapsed += Time.deltaTime;
                            card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x - unitsToMove, card2Pos.y, card2Pos.z), timeElapsed / duration);
                            yield return null;
                        }
                    }                       
                }
                else
                {
                    if (card1Pos.z - card2Pos.z > 0)
                    {
                        while (timeElapsed < duration)
                        {
                            timeElapsed += Time.deltaTime;
                            card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x, card2Pos.y, card2Pos.z + unitsToMove), timeElapsed / duration);
                            yield return null;
                        }
                    }
                    else if (card1Pos.z - card2Pos.z < 0)
                    {
                        while (timeElapsed < duration)
                        {
                            timeElapsed += Time.deltaTime;
                            card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x, card2Pos.y, card2Pos.z - unitsToMove), timeElapsed / duration);
                            yield return null;
                        }
                    }
                }
            }
        }
	}

	#endregion

	//this is where we call the handstrength coroutine. first we check the gamestate
	//we then use the appropriate handstrenth function for the gamestate
	public void DetermineHandStrength(CardType myCard1, CardType myCard2)
	{
		if(Table.gameState == GameState.PreFlop)
		{
			Services.Dealer.StartCoroutine(DeterminePreFlopHandStrength(0.5f, myCard1, myCard2));
		}
		else
		{
			Services.Dealer.StartCoroutine(RunHandStrengthLoopAfterFlop(myCard1, myCard2, Services.Dealer.GetActivePlayerCount()));
		}
	}

	//this is actually not far from the truth of how to determine preflop hands
	//but I don't have the FCR decision set up to accomodate these handStrengths
	//shouldn't be too hard
	//this is essentially why they're always calling preflop
	IEnumerator DeterminePreFlopHandStrength(float time, CardType myCard1, CardType myCard2)
	{
        yield return new WaitForSeconds(time);
        HandStrength = Hand.HandValues.Total;
        rateOfReturn = FindRateOfReturn();
		FoldCallRaiseDecision(rateOfReturn, this);
	}

	//we determine the handStrength
	//this is pretty well documented in the function
	//basically it runs a fake game 1000x with the cards available and sees how many times you win
	IEnumerator RunHandStrengthLoopAfterFlop(CardType myCard1, CardType myCard2, int activePlayers)
	{

		//set up all my empty lists to use 
		List<CardType> testDeck = new List<CardType>();
		#region populatingTheDeck
		SuitType[] suits = new SuitType[4]
		{
			SuitType.Spades,
			SuitType.Hearts,
			SuitType.Diamonds,
			SuitType.Clubs
		};
		RankType[] ranks = new RankType[13]
		{
			RankType.Two,
			RankType.Three,
			RankType.Four,
			RankType.Five,
			RankType.Six,
			RankType.Seven,
			RankType.Eight,
			RankType.Nine,
			RankType.Ten,
			RankType.Jack,
			RankType.Queen,
			RankType.King,
			RankType.Ace
		};

		foreach (SuitType suit in suits)
		{
			foreach (RankType rank in ranks)
			{
				testDeck.Add(new CardType(rank, suit));
			}
		}
		#endregion
		List<CardType> referenceDeck = new List<CardType>();
		referenceDeck.AddRange(testDeck);
		List<CardType> testBoard = new List<CardType>();
		List<PokerPlayerRedux> testPlayers = new List<PokerPlayerRedux>();
		List<List<CardType>> playerCards = new List<List<CardType>>();
		List<HandEvaluator> testEvaluators = new List<HandEvaluator>();
		for (int i = 0; i < activePlayers; i++)
		{
			testPlayers.Add(new PokerPlayerRedux(i));
			playerCards.Add(new List<CardType>());
			testEvaluators.Add(new HandEvaluator());
		}
        Debug.Assert(testPlayers.Count == Services.Dealer.GetActivePlayerCount());
		float numberOfWins = 0;
		float handStrengthTestLoops = 0;
		while (handStrengthTestLoops < 100)
		{
			#region 10x For-Loop for Hand Strength
			for (int f = 0; f < 10; f++)
			{
				//clear everything
				//clear each players hands
				foreach (PokerPlayerRedux player in testPlayers)
				{
					player.Hand = null;
				}
				//clear each players handEvaluators
				foreach (HandEvaluator eval in testEvaluators)
				{
					eval.ResetHandEvaluator();
				}
				//clear the deck
				testDeck.Clear();
				//add the deck
				testDeck.AddRange(referenceDeck);
				Debug.Assert(testDeck.Count == 52);
				//clear the board
				testBoard.Clear();
				Debug.Assert(testBoard.Count == 0);
				//clear each players cardList
				foreach (List<CardType> cardList in playerCards)
				{
					cardList.Clear();
					Debug.Assert(cardList.Count == 0);
				}
				//Start simulating the game
				//remove my cards from the deck
				for (int i = 0; i < testDeck.Count; i++)
				{
					if(testDeck[i].rank == myCard1.rank)
					{
						if(testDeck[i].suit == myCard1.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				for (int i = 0; i < testDeck.Count; i++)
				{
					if (testDeck[i].rank == myCard2.rank)
					{
						if (testDeck[i].suit == myCard2.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				Debug.Assert(testDeck.Count == 50);
				//remove the cards on the board from the deck and then add them to the fake board.
				foreach (Card boardCard in Table.instance.board)
				{
					testDeck.Remove(boardCard.cardType);
					testBoard.Add(boardCard.cardType);
				}
				for (int i = 0; i < testDeck.Count; i++)
				{
					if(testDeck[i].rank == Table.instance.board[0].cardType.rank)
					{
						if (testDeck[i].suit == Table.instance.board[0].cardType.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				for (int i = 0; i < testDeck.Count; i++)
				{
					if (testDeck[i].rank == Table.instance.board[1].cardType.rank)
					{
						if (testDeck[i].suit == Table.instance.board[1].cardType.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				for (int i = 0; i < testDeck.Count; i++)
				{
					if (testDeck[i].rank == Table.instance.board[2].cardType.rank)
					{
						if (testDeck[i].suit == Table.instance.board[2].cardType.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				//set THIS as test player0
				playerCards[0].Add(myCard1);
				playerCards[0].Add(myCard2);
				//give two cards two each other testPlayer, and then remove those cards from the deck
				for (int i = 1; i < testPlayers.Count; i++)
				{
                    //for (int j = 0; j < 2; j++)
                    //{
                    //	int cardPos = Random.Range(0, testDeck.Count);
                    //	CardType cardType = testDeck[cardPos];
                    //	playerCards[i].Add(cardType);
                    //	testDeck.Remove(cardType);
                    //}
                    List<CardType> holeCards = GetPreFlopHand(testDeck);
                    foreach(CardType card in holeCards)
                    {
                        playerCards[i].Add(card);
                        testDeck.Remove(card);
                    }
				}
				//if we're on the flop, deal out two more card to the board
				//and take those from the deck
				if (Table.instance.board.Count == 3)
				{
					for (int i = 0; i < 2; i++)
					{
						int cardPos = Random.Range(0, testDeck.Count);
						CardType cardType = testDeck[cardPos];
						testDeck.Remove(cardType);
						testBoard.Add(cardType);
					}
				}
				//if we're on the turn, only take out one more card from the deck to the board
				else if (Table.instance.board.Count == 4)
				{
					int cardPos = Random.Range(0, testDeck.Count);
					CardType cardType = testDeck[cardPos];
					testDeck.Remove(cardType);
					testBoard.Add(cardType);
				}
				//for each player, add the board cards
				//sort the hands
				//assign them an evaluator
				//set the evaluator
				//evaluate the hand
				//set the hand = to the evaluator
				for (int i = 0; i < playerCards.Count; i++)
				{
					playerCards[i].AddRange(testBoard);
					playerCards[i].Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
					HandEvaluator testHand = testEvaluators[i];
					testHand.SetHandEvalutor(playerCards[i]);
					testHand.EvaluateHandAtRiver();
					testPlayers[i].Hand = testHand;
				}
				//compare all test players and find the winner
				Services.Dealer.EvaluatePlayersOnShowdown(testPlayers);
				//if testPlayer[0] (this player) wins, we notch up the win score
				if (testPlayers[0].PlayerState == PlayerState.Winner)
				{
					float numberOfTestWinners = 0;
					foreach (PokerPlayerRedux player in testPlayers)
					{
						if (player.PlayerState == PlayerState.Winner)
						{
							numberOfTestWinners++;
						}
						else
						{
							//Debug.Log("losing player had a " + player.Hand.HandValues.PokerHand);
						}
					}
					numberOfWins += (1 / numberOfTestWinners);
				}
			}
			#endregion
			handStrengthTestLoops++;
			yield return null;
		}
        float tempHandStrength = numberOfWins / 1000f;
        Debug.Log(playerName + " has a raw HandStrength of " + tempHandStrength + " and a numberOfWins of " + numberOfWins);
        //HandStrength = Mathf.Pow(tempHandStrength, (float)Services.Dealer.GetActivePlayerCount());
        HandStrength = tempHandStrength;
        Debug.Log(playerName + " has an adjusted HS of " + HandStrength);
		rateOfReturn = FindRateOfReturn();
		FoldCallRaiseDecision(rateOfReturn, this);
		yield break;
	}

    public List<CardType> GetPreFlopHand(List<CardType> testDeck)
    {
        List<CardType> deck = testDeck;
        List<CardType> holeCards = new List<CardType>();
        /*
         * so we take two cards at random from the testDeck
         * we evaluate that hand
         * if it's good return those two cards
         * if it's not, then return those two cards into the deck and do it again until you do
         */
        for (int i = 0; i < 2; i++)
        {
            int cardPos = Random.Range(0, testDeck.Count);
            CardType cardType = deck[cardPos];
            holeCards.Add(cardType);
        }
        holeCards.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        HandEvaluator eval = new HandEvaluator();
        eval.isTesting = true;
        eval.SetHandEvalutor(holeCards);
        eval.EvaluateHandAtPreFlop();

        if (eval.HandValues.Total < 5)
        {
           return GetPreFlopHand(testDeck);
        }
        else
        {
            eval.isTesting = false;
            return holeCards;
        }
    }

    public void Tip()
    {
        if (!gaveTip)
        {
            gaveTip = true;
            int tipAmount = (int)DetermineTipAmount();
            int remainder = tipAmount % ChipConfig.RED_CHIP_VALUE;
            if (remainder > 0)
            {
                tipAmount = (tipAmount - remainder) + ChipConfig.RED_CHIP_VALUE;
            }
            Bet(tipAmount, true);
            Debug.Log(playerName + " tipped $" + tipAmount);
        }
    }

    public float DetermineTipAmount()
    {
        float tip = ChipConfig.RED_CHIP_VALUE * Services.Dealer.tipMultiplier;
        return tip;
    }

    public void Bet(int betAmount, bool isTipping)
    {
        Services.ChipManager.Bet(betAmount, isTipping, SeatPos, chipCount, parentChips);
    }

    //audio cue functuions for each decision
    //def gonna need to refactor
    public void SayCheck()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Debug.Log(playerName + " is saying checkl");
            //Services.SoundManager.GetSourceAndPlay(playerAudioSource, checkAudio);
            Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(playerName, LineCriteria.Check));
        }
    }

    public void SayFold()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation)
        {
            if (!Services.SoundManager.conversationIsPlaying)
            {
            Debug.Log(playerName + " folded and is starting a conversation!");

                for (int i = 0; i < 50; i++)
                {
                    Services.SoundManager.PlayConversation();
                }
                
            }
           
            else
            {
                if (Services.SoundManager.conversationIsPlaying)
                {
                    playerAudioSource.volume = 0.25f;
                }
                else
                {
                    playerAudioSource.volume = 1f;
                }
                Debug.Log(playerName + " is saying fold");
                Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(playerName, LineCriteria.Fold));
            }

        }

    }

    public void SayRaise()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Debug.Log(playerName + " is saying raise");
            //Services.SoundManager.GetSourceAndPlay(playerAudioSource, raiseAudio);
            Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(playerName, LineCriteria.Raise));
        }
    }

    public void SayBet()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Debug.Log(playerName + " is saying bet");
            Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(playerName, LineCriteria.Bet));
            //Services.SoundManager.GetSourceAndPlay(playerAudioSource, betAudio);
        }
    }

    public void SayCall()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Debug.Log(playerName + " is saying call");
            Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(playerName, LineCriteria.Call));
            //Services.SoundManager.GetSourceAndPlay(playerAudioSource, callAudio);
        }
    }

    public void SayAllIn()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            //Services.SoundManager.GetSourceAndPlay(playerAudioSource, allInAudio);
            Debug.Log(playerName + " is saying all in");
            Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(playerName, LineCriteria.AllIn));
        }
    }

    //determines which reaction to have
    public void WinnerReactions()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.SoundManager.conversationIsPlaying)
        {
            //Services.SoundManager.GetSourceAndPlay(playerAudioSource, winAudio);
            //Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(playerName, LineCriteria.Win));
        }
    }
}

