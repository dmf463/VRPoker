﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//this is the individual state of each player and controls how the game can flow
//I've had to add some states here and there as they pop up
//the most important one is "Playing" because if they aren't marked as playing, then they'll be skipped
public enum PlayerState { Playing, NotPlaying, Winner, Loser, Eliminated}
public enum PlayerName { None, Casey, Zombie, Minnie, Nathaniel, Floyd}


public class PokerPlayerRedux : MonoBehaviour{

    public PlayerName playerName;

    public GameObject playerSpotlight;
    public GameObject playerCardIndicator;
    private List<GameObject> parentChips;
    public GameObject[] cardPos;
    public int cardsReplaced = 0;
    public bool isAggressor = false;
    public int timesRaisedThisRound = 0;
    public bool gaveTip = false;
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

    //this is the the last amount of money that the player has bet.
    //not to be confused with "lastBet" on Dealer, this keeps track of ONLY what the player's last best was
    //as opposed to LastBest, in dealer, which keeps track of the last bet any player has made. 
    [HideInInspector]
    public int currentBet;

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

	//the individual player variables for the Fold, Call, Raise decision based on Return Rate
	public float lowReturnRate;
	public float decentReturnRate;
	public float highReturnRate;

    [Header("Player Behavior")]
    [Header("PlayerInsightPercent")]
    public float playerInsightPercent;
    [Header("Low RR (<)")]
    public float foldChanceLow;// = 95f;
    public float callChanceLow;// = 5f;
    public float raiseChanceLow;// = 0f;
    [Header("Decent RR (<)")]
    public float foldChanceDecent;// = 80f;
    public float callChanceDecent;// = 5f;
    public float raiseChanceDecent;// = 0f;
    [Header("High RR (<)")]
    public float foldChanceHigh;// = 0f;
    public float callChanceHigh;// = 60f;
    public float raiseChanceHigh;// = 40f;
    [Header("Very High RR (>=)")]
    public float foldChanceVeryHigh;// = 0f;
    public float callChanceVeryHigh;// = 30f;
    public float raiseChanceVeryHigh;// = 70f;


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
        lowReturnRate = 0.8f;
        decentReturnRate = 1f;
        highReturnRate = 1.3f;
    }

    void Update()
    {
        if (playerAudioSource.isPlaying) playerSpotlight.SetActive(true);
        else playerSpotlight.SetActive(false);
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
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtFlop();
                Hand = playerHand;
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                StartCoroutine(SetNextPlayer());
            }
            else if (Table.gameState == GameState.Turn)
            {
                turnComplete = false;
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtTurn(SeatPos);
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtTurn();
                Hand = playerHand;
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                StartCoroutine(SetNextPlayer());
            }
            else if (Table.gameState == GameState.River)
            {
                turnComplete = false;
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtRiver(SeatPos);
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtRiver();
                Hand = playerHand;
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                StartCoroutine(SetNextPlayer());
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
        return raise;
    }

    //this is the FCR decision and this is where we can adjust player types
    //we should go back to the generic one and make percentage variables that we can adjust in individual players
    public void FoldCallRaiseDecision(float returnRate, PokerPlayerRedux player)
    {
        switch (playerName)
        {
            case PlayerName.Casey:
                Services.PlayerBehaviour.CASEY_FoldCallRaiseDecision(returnRate, player);
                break;
            case PlayerName.Zombie:
                Services.PlayerBehaviour.ZOMBIE_FoldCallRaiseDecision(returnRate, player);
                break;
            case PlayerName.Minnie:
                Services.PlayerBehaviour.MINNIE_FoldCallRaiseDecision(returnRate, player);
                break;
            case PlayerName.Nathaniel:
                Services.PlayerBehaviour.NATHANIEL_FoldCallRaiseDecision(returnRate, player);
                break;
            case PlayerName.Floyd:
                Services.PlayerBehaviour.FLOYD_FoldCallRaiseDecision(returnRate, player);
                break;
            default:
                break;
        }
    }

    public void DetermineAction(float returnRate, PokerPlayerRedux player)
    {
        if (Services.Dealer.inTutorial) Call();
        else
        {
            switch (playerName)
            {
                case PlayerName.Casey:
                    Services.PlayerBehaviour.CASEY_DetermineAction(returnRate, player);
                    break;
                case PlayerName.Zombie:
                    Services.PlayerBehaviour.ZOMBIE_DetermineAction(returnRate, player);
                    break;
                case PlayerName.Minnie:
                    Services.PlayerBehaviour.MINNIE_DetermineAction(returnRate, player);
                    break;
                case PlayerName.Nathaniel:
                    Services.PlayerBehaviour.NATHANIEL_DetermineAction(returnRate, player);
                    break;
                case PlayerName.Floyd:
                    Services.PlayerBehaviour.FLOYD_DetermineAction(returnRate, player);
                    break;
                default:
                    break;
            }
        }
    }
    public void Fold()
    {
        SayFold();
        foreach (Card card in Table.instance.playerCards[SeatPos])
        {
            card.transform.position = Table.instance.playerFoldZones[SeatPos].transform.position;
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
                    Services.Dealer.players[i].FlipCards();
                    Services.Dealer.StartCoroutine(Services.Dealer.WaitForWinnersToGetPaid());

                }
            }
        }
    }
    public void Call()
    {
        if (chipCount > 0)
        {
            //Debug.Log("currentBet = " + currentBet);
            int betToCall = Services.Dealer.LastBet - currentBet;
            if (chipCount - betToCall <= 0)
            {
                AllIn();
                //Debug.Log("Player " + SeatPos + " didn't have enough chips and went all in for " + chipCount);
            }
            else
            {
                //Debug.Log("betToCall = " + betToCall);
                if (betToCall == 0) SayCheck();
                else SayCall();
                Bet(betToCall, false);
                currentBet = betToCall + currentBet;
                Services.Dealer.LastBet = currentBet;
                Debug.Log("Player " + SeatPos + " called " + betToCall);
                //Debug.Log("and the pot is now at " + Table.instance.potChips);
            }
        }
    }

    public void Raise()
    {
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
            int betToRaise = Services.Dealer.LastBet + (raiseAmount - currentBet);
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
                    SayBet();
                }
                else
                {
                    SayRaise();
                }
                Bet(betToRaise, false);
                currentBet = betToRaise + currentBet;
                Services.Dealer.LastBet = currentBet;
                //Debug.Log("player " + SeatPos + " raises " + betToRaise);
                //Debug.Log("Player " + SeatPos + " raised!");
                //Debug.Log("and the pot is now at " + Table.instance.potChips);
                //Debug.Log("and player " + SeatPos + " is now at " + chipCount);
            }
        }
    }

    public void AllIn()
    {
        SayAllIn();
        foreach (Card card in Table.instance.playerCards[SeatPos])
        {
            card.cardMarkedForDestruction = false;
        }
        chipCountBeforeAllIn = chipCount;
        playerIsAllIn = true;
        //Debug.Log("getting ready to go all in");
        Bet(chipCount, false);
        //similar to fold, when we go all in, we want to see if we're the last person to go all in
        //if so, then we want to flip the cards
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
	public void FlipCards()
	{
        flippedCards = true;
		List<GameObject> cardsInHand = Table.instance.GetCardGameObjects(SeatPos);
		for (int i = 0; i < cardsInHand.Count; i++)
		{
			if (!cardsInHand[i].GetComponent<Card>().CardIsFaceUp())
			{
				Physics.IgnoreCollision(cardsInHand[0].gameObject.GetComponent<Collider>(), cardsInHand[1].gameObject.GetComponent<Collider>());
				Services.Dealer.StartCoroutine(FlipCardsAndMoveTowardsBoard(.5f, cardsInHand[i], (GameObject.Find("TheBoard").GetComponent<Collider>().ClosestPointOnBounds(cardsInHand[i].transform.position) + cardsInHand[i].transform.position) / 2, SeatPos));
			}
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
        HandStrength = Mathf.Pow(tempHandStrength, (float)Services.Dealer.GetActivePlayerCount());
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

    IEnumerator WaitToReceiveWinnings(float time)
    {
        yield return new WaitForSeconds(time);
        ReceiveWinnings();
    }

    public void Tip()
    {
        if (!gaveTip)
        {
            gaveTip = true;
            int tipAmount = (int)(chipsWon * .10f);
            int remainder = tipAmount % ChipConfig.RED_CHIP_VALUE;
            if (remainder > 0)
            {
                tipAmount = (tipAmount - remainder) + ChipConfig.RED_CHIP_VALUE;
            }
            Bet(tipAmount, true);
            Debug.Log(playerName + " tipped $" + tipAmount);
        }
    }

    public void ReceiveWinnings()
    {
        float winnerCount = 0;
        float winnersPaid = 0;
        List<PokerPlayerRedux> winningPlayers = new List<PokerPlayerRedux>();
        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            if (Services.Dealer.players[i].PlayerState == PlayerState.Winner)
            {
                winningPlayers.Add(Services.Dealer.players[i]);
                winnerCount++;
                if (Services.Dealer.players[i].HasBeenPaid) winnersPaid++;
            }
        }

        if (PlayerState == PlayerState.Winner)
        {
            int tipAmount = (int)(chipsWon * .10f);
            int remainder = tipAmount % ChipConfig.RED_CHIP_VALUE;
            if (remainder > 0)
            {
                tipAmount = (tipAmount - remainder) + ChipConfig.RED_CHIP_VALUE;
            }
            Debug.Log(playerName + " tipped $" + tipAmount);
            Table.instance.playerChipStacks[SeatPos] = chipsWon + ChipCountToCheckWhenWinning;
            winnersPaid++;
            HasBeenPaid = true;
            Bet(tipAmount, true);
            if (!playerAudioSource.isPlaying &&
                !playerIsInConversation &&
                !Services.SoundManager.conversationIsPlaying &&
                !Services.Dealer.inTutorial)
            {
                Services.SoundManager.GetSourceAndPlay(playerAudioSource, tipAudio);
            }
            if(winnersPaid == winnerCount)
            {
                Services.Dealer.cleaningCards = true;
            }
        }
        else if (PlayerState == PlayerState.Loser)
        {
            if (!playerAudioSource.isPlaying &&
                !playerIsInConversation &&
                !Services.SoundManager.conversationIsPlaying &&
                !Services.Dealer.inTutorial)
            {
                Services.SoundManager.GetSourceAndPlay(playerAudioSource, wrongChipsAudio);
            }
        }
    }

    //this is LIKE create and organize chipStacks, except is used only during intialization
    public List<int> SetChipStacks(int chipAmount)
    {

        List<int> startingStack = new List<int>();

        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("P0Chips"), GameObject.Find("P1Chips"), GameObject.Find("P2Chips"), GameObject.Find("P3Chips"), GameObject.Find("P4Chips")
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


    public void CreateAndOrganizeChipStacks(List<int> chipsToOrganize )
	{
        GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
        foreach (GameObject container in emptyContainers)
        {
            if (container.transform.childCount == 0)
            {
                Destroy(container);
            }
        }
        if(parentChips.Count != 0)
        {
            foreach (GameObject chip in parentChips)
            {
                Destroy(chip);
            }
            parentChips.Clear();
        }

        List<int>organizedChips = chipsToOrganize;
		GameObject parentChip = null;
		float incrementStackBy = 0;
		List<GameObject> playerPositions = new List<GameObject>
		{
			GameObject.Find("P0Chips"), GameObject.Find("P1Chips"), GameObject.Find("P2Chips"), GameObject.Find("P3Chips"), GameObject.Find("P4Chips")
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
                        parentChips.Add(parentChip);
						parentChip.transform.parent = chipContainer.transform;
						parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
						parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
						if(parentChip.GetComponent<Rigidbody>() == null)
						{
							parentChip.AddComponent<Rigidbody>();
						}
                        //incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
                        incrementStackBy = parentChip.transform.localScale.z;
						parentChip.transform.localPosition = offSet;
						offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
						if(firstStackPos == Vector3.zero)
						{
							firstStackPos = parentChip.transform.position;
						}
						lastStackPos = parentChip.transform.position;
					}
                    else if(chipStackSize >= stackCountMax)
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
                        parentChips.Add(parentChip);
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


    public void Bet(int betAmount, bool isTipping)
    {
        if (isTipping) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.tipSFX, 1f);
		else Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.chips[Random.Range(0,Services.SoundManager.chips.Length)], 0.25f, Random.Range(0.95f,1.05f), transform.position);
        int oldChipStackValue = chipCount;
        List<GameObject> playerBetZones = new List<GameObject>
        {
            GameObject.Find("P0BetZone"), GameObject.Find("P1BetZone"), GameObject.Find("P2BetZone"), GameObject.Find("P3BetZone"), GameObject.Find("P4BetZone")
        };
        Vector3 tipPos = GameObject.Find("TipZone").transform.position;
        List<GameObject> betChips = new List<GameObject>();
        List<int> colorChipCount = new List<int>()
        {
			//blackChipCount, whiteChipCount, blueChipCount, redChipCount
			0, 0, 0, 0
        };

        List<int> chipPrefab = new List<int>
        {
            ChipConfig.BLACK_CHIP_VALUE, ChipConfig.WHITE_CHIP_VALUE, ChipConfig.BLUE_CHIP_VALUE, ChipConfig.RED_CHIP_VALUE
        };

        int valueRemaining = betAmount;
        int remainder = valueRemaining % ChipConfig.RED_CHIP_VALUE;
        if (remainder > 0)
        {
            valueRemaining = (valueRemaining - remainder) + ChipConfig.RED_CHIP_VALUE;
        }

        int blackChipCountMAX = FindChipMax(ChipConfig.BLACK_CHIP_VALUE);
        int whiteChipCountMAX = FindChipMax(ChipConfig.WHITE_CHIP_VALUE);
        int blueChipCountMAX = FindChipMax(ChipConfig.BLUE_CHIP_VALUE);
        int redChipCountMAX = FindChipMax(ChipConfig.RED_CHIP_VALUE);

        colorChipCount[0] = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
        valueRemaining -= colorChipCount[0] * ChipConfig.BLACK_CHIP_VALUE;

        colorChipCount[1] = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
        valueRemaining -= colorChipCount[1] * ChipConfig.WHITE_CHIP_VALUE;

        colorChipCount[2] = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
        valueRemaining -= colorChipCount[2] * ChipConfig.BLUE_CHIP_VALUE;

        colorChipCount[3] = Mathf.Min(redChipCountMAX, valueRemaining / ChipConfig.RED_CHIP_VALUE);
        valueRemaining -= colorChipCount[3] * ChipConfig.RED_CHIP_VALUE;

        if (valueRemaining > 0)
        {
            List<int> chipChange = SetChipStacks(valueRemaining);
            for (int i = 0; i < chipChange.Count; i++)
            {
                for (int chipChangeIndex = 0; chipChangeIndex < chipChange[chipChangeIndex]; chipChangeIndex++)
                {
                    if(chipChange[chipChangeIndex] != 0) colorChipCount[i]++;
                }
            }
            Table.instance.RemoveChipFrom(playerDestinations[SeatPos], valueRemaining);
        }

        //if there are less than 2 chips, don't even bother putting them in a stack. because why even?
        if ((colorChipCount[0] + colorChipCount[1] + colorChipCount[2] + colorChipCount[3]) < 0)
        {
            for (int colorListIndex = 0; colorListIndex < colorChipCount.Count; colorListIndex++)
            {
                GameObject chipToMake = null;
                switch (colorListIndex)
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
                if (colorChipCount[colorListIndex] > 0)
                {

                    for (int colorCount = 0; colorCount < colorChipCount[colorListIndex]; colorCount++)
                    {
                        Vector3 offSet = new Vector3(Random.Range(0, .5f), .5f, Random.Range(0, .5f));
                        GameObject newChip;
                        if (!isTipping)
                        {
                            newChip = GameObject.Instantiate(chipToMake, playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                        }
                        else
                        {
                            newChip = GameObject.Instantiate(chipToMake, tipPos + offSet, Quaternion.Euler(-90, 0, 0));
                            newChip.GetComponent<MeshRenderer>().material = Services.PokerRules.tipMaterial;
                        }
                        newChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                        if(!isTipping) Services.Dealer.chipsInPot.Add(newChip.GetComponent<Chip>());
                        if(!isTipping) Table.instance.potChips += newChip.GetComponent<Chip>().chipData.ChipValue;
                        Table.instance.RemoveChipFrom(playerDestinations[SeatPos], newChip.GetComponent<Chip>().chipData.ChipValue);
                    }
                }
            }
        }

        else //if there are more than 5 chips to bet, STACK THOSE MOTHERFUCKERS.
        {
            GameObject parentChip = null;
            float incrementStackBy = 0;
            Vector3 offSet = Vector3.zero;
            Vector3 containerOffset = Vector3.up * .08f;
            GameObject chipContainer;
            if (!isTipping)
            {
                chipContainer = GameObject.Instantiate(new GameObject(), playerBetZones[SeatPos].transform.position + containerOffset, playerBetZones[SeatPos].transform.rotation);
            }
            else
            {
                chipContainer = GameObject.Instantiate(new GameObject(), tipPos + containerOffset, playerBetZones[SeatPos].transform.rotation);
            }
            if(!isTipping) chipContainer.tag = "TipContainer";
            else chipContainer.tag = "Container";
            chipContainer.name = "Container";
            chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
            Vector3 lastStackPos = Vector3.zero;
            Vector3 firstStackPos = Vector3.zero;
            int chipCountMax = 30;
            for (int colorListIndex = 0; colorListIndex < colorChipCount.Count; colorListIndex++) //this runs 4 times, one for each color
            {
                GameObject chipToMake = null;
                switch (colorListIndex)
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
                if (colorChipCount.Count != 0) //if there is a number
                {
                    int chipStackCount = 0;
                    for (int chipIndex = 0; chipIndex < colorChipCount[colorListIndex]; chipIndex++)
                    {
                        if (chipIndex == 0)
                        {
                            chipStackCount++;
                            GameObject newChip;
                            if (!isTipping)
                            {
                                newChip = GameObject.Instantiate(chipToMake, playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                            }
                            else
                            {
                                newChip = GameObject.Instantiate(chipToMake, tipPos + offSet, Quaternion.Euler(-90, 0, 0));
                                newChip.GetComponent<MeshRenderer>().material = Services.PokerRules.tipMaterial;
                                newChip.gameObject.tag = "Tip";
                            }
                            newChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                            betChips.Add(newChip);
                            Table.instance.RemoveChipFrom(playerDestinations[SeatPos], newChip.GetComponent<Chip>().chipData.ChipValue);
                            if (!isTipping) Table.instance.potChips += newChip.GetComponent<Chip>().chipData.ChipValue;
                            parentChip = newChip;
                            if (!isTipping) Services.Dealer.chipsInPot.Add(newChip.GetComponent<Chip>());
                            parentChip.transform.parent = chipContainer.transform;
                            parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                            parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                            if (parentChip.GetComponent<Rigidbody>() == null)
                            {
                                parentChip.AddComponent<Rigidbody>();
                            }
                            incrementStackBy = parentChip.transform.localScale.z;
                            parentChip.transform.localPosition = offSet;
                            offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                            if (firstStackPos == Vector3.zero)
                            {
                                firstStackPos = parentChip.transform.position;
                            }
                            lastStackPos = parentChip.transform.position;
                        }
                        else if (chipStackCount >= chipCountMax)
                        {
                            chipStackCount = 1;
                            GameObject newChip;
                            if (!isTipping)
                            {
                                newChip = GameObject.Instantiate(chipToMake, playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                            }
                            else
                            {
                                newChip = GameObject.Instantiate(chipToMake, tipPos + offSet, Quaternion.Euler(-90, 0, 0));
                                newChip.GetComponent<MeshRenderer>().material = Services.PokerRules.tipMaterial;
                                newChip.gameObject.tag = "Tip";
                            }
                            newChip.GetComponent<Chip>().chipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                            betChips.Add(newChip);
                            Table.instance.RemoveChipFrom(playerDestinations[SeatPos], newChip.GetComponent<Chip>().chipData.ChipValue);
                            if(!isTipping) Table.instance.potChips += newChip.GetComponent<Chip>().chipData.ChipValue;
                            parentChip = newChip;
                            if(!isTipping) Services.Dealer.chipsInPot.Add(newChip.GetComponent<Chip>());
                            parentChip.transform.parent = chipContainer.transform;
                            parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                            parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                            if (parentChip.GetComponent<Rigidbody>() == null)
                            {
                                parentChip.AddComponent<Rigidbody>();
                            }
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
                            chipStackCount++;
                            ChipData newChipData = new ChipData(chipToMake.GetComponent<Chip>().chipData.ChipValue);
                            Table.instance.RemoveChipFrom(playerDestinations[SeatPos], newChipData.ChipValue);
                            if(!isTipping) Table.instance.potChips += newChipData.ChipValue;
                            parentChip.GetComponent<Chip>().chipStack.chips.Add(newChipData);
                            parentChip.GetComponent<Chip>().chipStack.stackValue += newChipData.ChipValue;
                            parentChip.transform.localScale = new Vector3(parentChip.transform.localScale.x,
                                                                          parentChip.transform.localScale.y,
                                                                          parentChip.transform.localScale.z + incrementStackBy);
                        }
                    }
                }
            }
            Vector3 trueOffset = firstStackPos - lastStackPos;
            chipContainer.transform.position += trueOffset / 2;
        }

        List<int> newChipStack = SetChipStacks(ChipCount);
        CreateAndOrganizeChipStacks(newChipStack);
        GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
        foreach (GameObject container in emptyContainers)
        {
            if (container.transform.childCount == 0)
            {
                Destroy(container);
            }
            else Debug.Log("container has " + container.transform.childCount + " children");
        }
        foreach(GameObject chip in betChips)
        {
            chip.GetComponent<Chip>().chipForBet = true;
        }
    }

    //this basically goes through a given chipValue and finds each instance of that chipValue in the playerChipStack
    public int FindChipMax(int chipValue)
    {
        int chipMax = 0;
        //Debug.Log("chipCount = " + chipCount);
        switch (chipValue)
        {
            case ChipConfig.BLACK_CHIP_VALUE:
                chipMax = (int)((float)(chipCount * 0.45f) / (float)ChipConfig.BLACK_CHIP_VALUE);
                break;
            case ChipConfig.WHITE_CHIP_VALUE:
                chipMax = (int)((float)(chipCount * 0.35f) / (float)ChipConfig.WHITE_CHIP_VALUE);
                break;
            case ChipConfig.BLUE_CHIP_VALUE:
                chipMax = (int)((float)(chipCount) * 0.15f / (float)ChipConfig.BLUE_CHIP_VALUE);
                break;
            case ChipConfig.RED_CHIP_VALUE:
                chipMax = (int)((float)(chipCount) * 0.05f / (float)ChipConfig.RED_CHIP_VALUE);
                break;
            default:
                break;
        }
        return chipMax;
    }

    //this is just me ease-of-life function for findining the correct prefab
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

    //audio cue functuions for each decision
    //def gonna need to refactor
    public void SayCheck()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.Dealer.inTutorial)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Services.SoundManager.GetSourceAndPlay(playerAudioSource, checkAudio);
        }
    }

    public void SayFold()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.Dealer.inTutorial)
        {
            //Services.SoundManager.GetSourceAndPlay(playerAudioSource, foldAudio);
            float chanceOfConvo = Random.Range(0, 100);
            if (chanceOfConvo < 25f)
            {
                if (!Services.SoundManager.conversationIsPlaying)
                {
                    //Services.SoundManager.PlayAsideConversation(UnityEngine.Random.Range(0, 5));
                    Services.SoundManager.PlayAsideConversation(this);
                    Debug.Log(gameObject + " started a conversation.");
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
                Services.SoundManager.GetSourceAndPlay(playerAudioSource, foldAudio);
                Debug.Log(gameObject + " said fold.");
            }

        }

    }

    public void SayRaise()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.Dealer.inTutorial)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Services.SoundManager.GetSourceAndPlay(playerAudioSource, raiseAudio);
        }
    }

    public void SayBet()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.Dealer.inTutorial)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Services.SoundManager.GetSourceAndPlay(playerAudioSource, betAudio);
        }
    }

    public void SayCall()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.Dealer.inTutorial)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Services.SoundManager.GetSourceAndPlay(playerAudioSource, callAudio);
        }
    }

    public void SayAllIn()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.Dealer.inTutorial)
        {
            if (Services.SoundManager.conversationIsPlaying)
            {
                playerAudioSource.volume = 0.25f;
            }
            else
            {
                playerAudioSource.volume = 1f;
            }
            Services.SoundManager.GetSourceAndPlay(playerAudioSource, allInAudio);
        }
    }

    //determines which reaction to have
    public void WinnerReactions()
    {
        if (!playerAudioSource.isPlaying && !playerIsInConversation && !Services.SoundManager.conversationIsPlaying && !Services.Dealer.inTutorial)
        {
            Services.SoundManager.GetSourceAndPlay(playerAudioSource, winAudio);
        }
    }

}

