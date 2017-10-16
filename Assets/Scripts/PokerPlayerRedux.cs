using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//this is the individual state of each player and controls how the game can flow
//I've had to add some states here and there as they pop up
//the most important one is "Playing" because if they aren't marked as playing, then they'll be skipped
public enum PlayerState { Playing, NotPlaying, Winner, Loser, Eliminated}


public class PokerPlayerRedux : MonoBehaviour{

    public GameObject playerSpotlight;
    public GameObject[] cardPos;
    public int cardsReplaced = 0;

	//what position they are at the table, this is set in Dealer and is massively important
	//this is the current means by which we differentiate between which instance of PokerPlayerRedux we're currently working with
	public int SeatPos { get; set; }

	//this lets me keep track of the players chip count, but only when I call ChipCount, so it may be less reliable than it can be
	public int ChipCount { get { return chipCount; } set { value = chipCount; } }
	private int chipCount
	{
		get { return Table.instance.GetChipStackTotal(SeatPos); }
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

	//this is the int we use in order to determine how much a player will raise in a given situation
	//the function that uses this needs to be reexamined I think
	private int amountToRaise;

	//the individual player variables for the Fold, Call, Raise decision based on Return Rate
	private float lowReturnRate;
	private float decentReturnRate;
	private float highReturnRate;

	[Header("Player Behavior")]
	[Header("Low RR (<)")]
	public float foldChanceLow = 95f;
	public float callChanceLow = 5f;
	public float raiseChanceLow = 0f;
	[Header("Decent RR (<)")]
	public float foldChanceDecent = 80f;
	public float callChanceDecent = 5f;
	public float raiseChanceDecent = 0f;
	[Header("High RR (<)")]
	public float foldChanceHigh = 0f;
	public float callChanceHigh = 60f;
	public float raiseChanceHigh = 40f;
	[Header("Very High RR (>=)")]
	public float foldChanceVeryHigh = 0f;
	public float callChanceVeryHigh = 30f;
	public float raiseChanceVeryHigh = 70f;

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

        lowReturnRate = 0.8f;
        decentReturnRate = 1f;
        highReturnRate = 1.3f;
        Debug.Log("lowReturn Rate: " + lowReturnRate + ", decentReturnRate: " + decentReturnRate + ", highReturnRate: " + highReturnRate);
        //Debug.Log(playerDestinations.Count); 
        //playerDestinations = //Table.instance.playerDestinations;
        //for (int i = 0; i < playerDestinations.Count; i++) {
        //    Debug.Log("Slot " + i + " in list contains: " + playerDestinations[i]);
        //}
    }

	public void Fold()
	{
		SayFold();
		foreach(Card card in Table.instance.playerCards[SeatPos])
		{
			card.transform.position = Table.instance.playerBetZones[SeatPos].transform.position;
            card.cardMarkedForDestruction = false;
		}
		PlayerState = PlayerState.NotPlaying;
		Hand = null;
		Debug.Log("Player " + SeatPos + " folded!");
		if(Services.Dealer.GetActivePlayerCount() == 1)
		{
			Table.gameState = GameState.CleanUp;
			for (int i = 0; i < Services.Dealer.players.Count; i++)
			{
				if(Services.Dealer.players[i].PlayerState == PlayerState.Playing)
				{
					Services.Dealer.players[i].PlayerState = PlayerState.Winner;
					Services.Dealer.numberOfWinners = 1;
					Debug.Log("current chip stack is at " + ChipCount);
					Services.Dealer.players[i].ChipCountToCheckWhenWinning = Services.Dealer.players[i].ChipCount;
					Debug.Log("We are getting into the fold and the chipCountToCheckWhenWinning = " + ChipCountToCheckWhenWinning);
					Services.Dealer.playersReady = true;
                    Services.Dealer.playersHaveBeenEvaluated = true;
					Services.Dealer.StartCoroutine(Services.Dealer.WaitForWinnersToGetPaid());
				}
			}
		}
	}

	//the player Calls
	//a player can only call if they have chips to bet with
	//the "betToCall" is the int we use to determine how much we'll pass to the Bet function
	//we take the last bet, minus the current bet, and that's it.
	//if the bet would make the player go All In, then we call AllIn();
	//if not we check if the bet is 0, and we say Check
	//else we say Call
	//then we call Bet with betToCall. If it's a check, the player is betting 0.
	//since there is money being added to the table, we make the current bet = lastBet
	public void Call()
	{
		if(chipCount > 0)
		{
			Debug.Log("currentBet = " + currentBet);
			int betToCall = Services.Dealer.LastBet - currentBet;
			if(ChipCount - betToCall <= 0)
			{
				AllIn();
				Debug.Log("Player " + SeatPos + " didn't have enough chips and went all in for " + chipCount);
			}
			else
			{
				Debug.Log("betToCall = " + betToCall);
				if(betToCall == 0) SayCheck();
				else SayCall();
				Bet(betToCall);
				currentBet = betToCall + currentBet;
				Services.Dealer.LastBet = currentBet;
				Debug.Log("Player " + SeatPos + " called " + betToCall);
				Debug.Log("and the pot is now at " + Table.instance.PotChips);
			}
		}
	}


	//the player Raises
	//this works essentially the same as Call() except the betToRaise is equal the last best + the raise amount minus the current bet
	//amountToRaise is determined before Raise is ever called because we need the raise amount to determined the Rate of Return
	//again, the amount to raise probably needs more refiguring and balancing
	public void Raise()
	{
		if (ChipCount > 0)
		{
			int raiseAmount = amountToRaise; 
			int betToRaise = Services.Dealer.LastBet + (raiseAmount - currentBet);
			if (ChipCount - betToRaise <= 0)
			{
				AllIn();
				Debug.Log("Player " + SeatPos + " didn't have enough chips and went all in for " + chipCount);
				currentBet = betToRaise + currentBet;
				Services.Dealer.LastBet = currentBet;
				Debug.Log("player " + SeatPos + " raises " + betToRaise);
				Debug.Log("Player " + SeatPos + " raised!");
				Debug.Log("and the pot is now at " + Table.instance.PotChips);
				Debug.Log("and player " + SeatPos + " is now at " + ChipCount);
			}
			else
			{
				SayRaise();
				Bet(betToRaise);
				currentBet = betToRaise + currentBet;
				Services.Dealer.LastBet = currentBet;
				Debug.Log("player " + SeatPos + " raises " + betToRaise);
				Debug.Log("Player " + SeatPos + " raised!");
				Debug.Log("and the pot is now at " + Table.instance.PotChips);
				Debug.Log("and player " + SeatPos + " is now at " + ChipCount);
			}
		}
	}

	//the player goes All In
	//the player says "All in"
	//then we grab all the chip gameObjects and we run through them until we find the ChipContainer
	//then we remove all the chips from the proper list, and add those to the pot
	//then we call the coroutine that pushes the container, which holds all the chips, to their proper location
	//this visually signals the all in 
	public void AllIn()
	{
		SayAllIn();
        chipCountBeforeAllIn = ChipCount;
        playerIsAllIn = true;
		Debug.Log("getting ready to go all in");
        Bet(ChipCount);
		//List<GameObject> allInChips = Table.instance.GetChipGameObjects(SeatPos);
		//GameObject chipStackContainer = null;
		//for (int i = 0; i < allInChips.Count; i++)
		//{
		//	if (allInChips[i].GetComponent<Chip>().chipStack != null)
		//	{
		//		chipStackContainer = allInChips[i].transform.parent.gameObject;
		//	}
		//	Table.instance._potChips.Add(allInChips[i].GetComponent<Chip>());
		//	Table.instance.RemoveChipFrom(playerDestinations[SeatPos], allInChips[i].GetComponent<Chip>());
		//}
		//Services.Dealer.StartCoroutine(PushChipsIn(1, chipStackContainer, Table.instance.playerBetZones[SeatPos].transform.position));
        //similar to fold, when we go all in, we want to see if we're the last person to go all in
        //if so, then we want to flip the cards
        int allInPlayerCount = 0;
        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            if (Services.Dealer.players[i].playerIsAllIn == true) allInPlayerCount++;
        }
        if (Services.Dealer.GetActivePlayerCount() == allInPlayerCount)
        {
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if (Services.Dealer.players[i].playerIsAllIn == true)
                {
                    Services.Dealer.players[i].FlipCards();
                }
            }
        }
    }

	//this is the coroutine for pushing in chips
	//we take a duration, the chipStack (container), and the target pos
	//then we lerp it
	IEnumerator PushChipsIn(float duration, GameObject chipStack, Vector3 targetPos)
	{
		Debug.Log("extending my hand to push all in");
		float timeElapsed = 0;
		Vector3 initialPos = chipStack.gameObject.transform.position;
		Vector3 finalPos = new Vector3(targetPos.x, initialPos.y, targetPos.z);
		while(timeElapsed < duration)
		{
			Debug.Log("pushing all in");
			timeElapsed += Time.deltaTime;
			chipStack.transform.position = Vector3.Lerp(initialPos, finalPos, timeElapsed / duration);
			yield return null;
		}
	}

	//audio cue functuions for each decision
	//def gonna need to refactor
	public void SayCheck()
	{
		if (SeatPos == 0) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.checkP1);
		else if (SeatPos == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.checkP2);
	}

	public void SayFold()
	{
		if (SeatPos == 0) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.foldP1);
		else if (SeatPos == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.foldP2);
	}

	public void SayRaise()
	{
		if (SeatPos == 0) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.raiseP1);
		else if (SeatPos == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.raiseP2);
	}

	public void SayCall()
	{
		if(SeatPos == 0 ) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.callP1);
		else if(SeatPos == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.callP2);
	}

	public void SayAllIn()
	{
		if(SeatPos == 0) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.allInP1);
		else if(SeatPos == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.allInP2);
	}

	//determines which reaction to have
	public void WinnerReactions()
	{
		for (int i = 0; i < Services.Dealer.players.Count; i++)
		{
			if(Services.Dealer.players[i].PlayerState == PlayerState.Loser)
			{
				PokerPlayerRedux losingPlayer = Services.Dealer.players[i];
				if (losingPlayer.HandStrength < 0.25) IncredulousReaction();
				else if (losingPlayer.HandStrength < 0.5) RespectfulReaction();
				else GoodReaction();
			}
		}
	}

	//determines which loser reaction there is
	public void LoserReactions()
	{
		if (HandStrength < 0.75) BadBeat();
		else GenericLoss();
	}

	//these are all the different reactions
	public void BadBeat()
	{
		if(SeatPos == 0)
		{
			if(Services.SoundManager.badBeatP1.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if(randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.badBeatP1[0]);
					Services.SoundManager.badBeatP1.Remove(Services.SoundManager.badBeatP1[0]);
				}
			}
		}
		else if(SeatPos == 1)
		{
			if (Services.SoundManager.badBeatP2.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.badBeatP2[0]);
					Services.SoundManager.badBeatP2.Remove(Services.SoundManager.badBeatP2[0]);
				}
			}
		}
	}

	public void GenericLoss()
	{
		if (SeatPos == 0)
		{
			if (Services.SoundManager.genericBadP1.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.genericBadP1[0]);
					Services.SoundManager.genericBadP1.Remove(Services.SoundManager.badBeatP1[0]);
				}
			}
		}
		else if (SeatPos == 1)
		{
			if (Services.SoundManager.genericBadP2.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.genericBadP2[0]);
					Services.SoundManager.genericBadP2.Remove(Services.SoundManager.genericBadP2[0]);
				}
			}
		}
	}

	public void IncredulousReaction()
	{
		if (SeatPos == 0)
		{
			if (Services.SoundManager.incredulousP1.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.incredulousP1[0]);
					Services.SoundManager.incredulousP1.Remove(Services.SoundManager.incredulousP1[0]);
				}
			}
		}
		else if (SeatPos == 1)
		{
			if (Services.SoundManager.incredulousP2.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.incredulousP2[0]);
					Services.SoundManager.incredulousP2.Remove(Services.SoundManager.incredulousP2[0]);
				}
			}
		}
	}

	public void RespectfulReaction()
	{
		if (SeatPos == 0)
		{
			if (Services.SoundManager.respectP1.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.respectP1[0]);
					Services.SoundManager.respectP1.Remove(Services.SoundManager.respectP1[0]);
				}
			}
		}
		else if (SeatPos == 1)
		{
			if (Services.SoundManager.respectP2.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.respectP2[0]);
					Services.SoundManager.respectP2.Remove(Services.SoundManager.respectP2[0]);
				}
			}
		}
	}

	public void GoodReaction()
	{
		if (SeatPos == 0)
		{
			if (Services.SoundManager.goodResponseP1.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.goodResponseP1[0]);
					Services.SoundManager.goodResponseP1.Remove(Services.SoundManager.goodResponseP1[0]);
				}
			}
		}
		else if (SeatPos == 1)
		{
			if (Services.SoundManager.goodResponseP2.Count != 0)
			{
				float randomNumber = Random.Range(0, 100);
				if (randomNumber < 100)
				{
					Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.goodResponseP2[0]);
					Services.SoundManager.goodResponseP2.Remove(Services.SoundManager.goodResponseP2[0]);
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
		amountToRaise = DetermineRaiseAmount();
		float potSize = Table.instance.PotChips;
		float potOdds = amountToRaise / (amountToRaise + potSize);
		float returnRate = HandStrength / potOdds;
		return returnRate;
	}

	//basically this is a KEY function. right now it's using primarily handStrength to determine how much to raise
	//but as we continue I'll need to actually add in a TON of factors when considering how much to raise
	public int DetermineRaiseAmount()
	{
		int raise = 0;

		if (Table.gameState == GameState.PreFlop)
		{
			#region pre-flop raises
			if (HandStrength > .8)
			{
				//95% chance to raise big, 5% to go all in
				float randomNum = Random.Range(0, 100);
				if (randomNum < 95)
				{
					if (Services.Dealer.LastBet == Services.Dealer.BigBlind)
					{
						raise = Services.Dealer.BigBlind * 3;
					}
					else raise = Services.Dealer.LastBet * 2;
				}
				else raise = ChipCount;
			}
			else if (HandStrength > .5)
			{
				//98% chance to raise big, 2% to go all in 
				float randomNum = Random.Range(0, 100);
				if (randomNum < 98)
				{
					if (Services.Dealer.LastBet == Services.Dealer.BigBlind)
					{
						raise = Services.Dealer.BigBlind * 3;
					}
					else raise = Services.Dealer.LastBet * 2;
				}
				else raise = ChipCount;
			}
			else if (HandStrength > .3)
			{
				//70% chance to raise big 30% chance to min raise 
				float randomNum = Random.Range(0, 100);
				if (randomNum < 70)
				{
					if (Services.Dealer.LastBet == Services.Dealer.BigBlind)
					{
						raise = Services.Dealer.BigBlind * 3;
					}
					else raise = Services.Dealer.LastBet;
				}
			}
			else raise = Services.Dealer.LastBet;
			#endregion
		}
		else
		{
			#region post-flop raises
			if (HandStrength > .8)
			{
				//95% chance to raise pot, 5% to go all in
				float randomNum = Random.Range(0, 100);
				if (randomNum < 95)
				{
					raise = Table.instance.PotChips;
				}
				else raise = ChipCount;
			}
			else if (HandStrength > .5)
			{
				//50% to raise pot, 35% to half pot, 10% 2x lastBet (or 3x big blind), 5% all in
				float randomNum = Random.Range(0, 100);
				if (randomNum < 50)
				{
					raise = Table.instance.PotChips;
				}
				else if(randomNum - 50 < 35)
				{
					int remainder = (Table.instance.PotChips / 2) % ChipConfig.RED_CHIP_VALUE;
					if (remainder > 0) raise = ((Table.instance.PotChips / 2) - remainder) + ChipConfig.RED_CHIP_VALUE;
					else raise = Table.instance.PotChips / 2;
				}
				else if(randomNum - 50 < 10)
				{
					if (Services.Dealer.LastBet == 0)
					{
						raise = Services.Dealer.BigBlind * 3;
					}
					else raise = Services.Dealer.LastBet * 2;
				}
				else if(randomNum - 50 < 5)
				{
					raise = ChipCount;
				}
			}
			else if (HandStrength > .3)
			{
				//70% raise half pot, 30% 2x lastbest (or 3x big blind)
				float randomNum = Random.Range(0, 100);
				if (randomNum < 70)
				{
					int remainder = (Table.instance.PotChips / 2) % ChipConfig.RED_CHIP_VALUE;
					if (remainder > 0) raise = ((Table.instance.PotChips / 2) - remainder) + ChipConfig.RED_CHIP_VALUE;
					else raise = Table.instance.PotChips / 2;
				}
				else
				{
					if (Services.Dealer.LastBet == 0)
					{
						raise = Services.Dealer.BigBlind * 3;
					}
					else raise = Services.Dealer.LastBet * 2;
				}
			}
			else
			{
				if (Services.Dealer.LastBet == 0)
				{
					raise = Services.Dealer.BigBlind * 3;
				}
				else raise = Services.Dealer.LastBet;
			}
			#endregion
		}
		if(raise % ChipConfig.RED_CHIP_VALUE > 0)
		{
			Debug.Log("Invalid Raise Amount");
			return 0;
		}
		if(raise == 0) raise = Services.Dealer.BigBlind * 3;
		return raise;
	}

    //this is the FCR decision and this is where we can adjust player types
    //we should go back to the generic one and make percentage variables that we can adjust in individual players
    public void FoldCallRaiseDecision(float returnRate)
    {
        if (Table.gameState == GameState.PreFlop)
        {
            Call();
            turnComplete = true;
            actedThisRound = true;
        }
        else
        {
            if ((ChipCount - Services.Dealer.LastBet) < (Services.Dealer.BigBlind * 4) && HandStrength < 0.5) Fold();
            else if (returnRate < lowReturnRate)
            {
                //95% chance fold, 5% bluff (raise)

                float randomNumber = Random.Range(0, 100);
                if (randomNumber < foldChanceLow)
                {
                    //if there's not bet, don't fold, just call for free.
                    if (Services.Dealer.LastBet > 0) Fold();
                    else Call();
                }
                else if (randomNumber - foldChanceLow < callChanceLow) Call();
                else Raise();

            }
            else if (returnRate < decentReturnRate)
            {
                //80% chance fold, 5% call, 15% bluff(raise)
                float randomNumber = Random.Range(0, 100);
                if (randomNumber < foldChanceDecent)
                {
                    if (Services.Dealer.LastBet > 0) Fold();
                    else Call();
                }
                else if (randomNumber - foldChanceDecent < callChanceDecent) Call();
                else Raise();

            }
            else if (returnRate < highReturnRate)
            {
                //60% chance call, 40% raise
                float randomNumber = Random.Range(0, 100);
                if (randomNumber < foldChanceHigh)
                {
                    if (Services.Dealer.LastBet > 0) Fold();
                    else Call();
                }
                else if (randomNumber - foldChanceHigh < callChanceHigh) Call();
                else Raise();

            }
            else if (returnRate >= highReturnRate)
            {
                //70% chance raise, 30% call
                float randomNumber = Random.Range(0, 100);
                if (randomNumber < foldChanceVeryHigh)
                {
                    if (Services.Dealer.LastBet > 0) Fold();
                    else Call();
                }
                else if (randomNumber - foldChanceVeryHigh < callChanceVeryHigh) Call();
                else Raise();
            }
            turnComplete = true;
            actedThisRound = true;
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

    IEnumerator SetNextPlayer()
    {
        while (!turnComplete)
        {
            yield return null;
        }
        Debug.Log("calling set next player from player");
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
		List<GameObject> cardsInHand = Table.instance.GetCardGameObjects(SeatPos);
		for (int i = 0; i < cardsInHand.Count; i++)
		{
			if (cardsInHand[i].GetComponent<Card>().cardIsFlipped == false)
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
		float timeElapsed = 0;
		float unitsToMove = 0.1f;
		Vector3 card1Pos = card1.transform.position;
		Vector3 card2Pos = card2.transform.position;
		float distanceBetweenCards = Vector3.Distance(card1Pos, card2Pos);
		if(distanceBetweenCards < 1)
		{
			if(seatPos != 2)
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

	#endregion

	//this is where we call the handstrength coroutine. first we check the gamestate
	//we then use the appropriate handstrenth function for the gamestate
	public void DetermineHandStrength(CardType myCard1, CardType myCard2)
	{
		if(Table.gameState == GameState.PreFlop)
		{
			DeterminePreFlopHandStrength(myCard1, myCard2);
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
	public void DeterminePreFlopHandStrength(CardType myCard1, CardType myCard2)
	{
		//public enum PokerHand { Connectors, SuitedConnectors, HighCard, OnePair, TwoPair, ThreeOfKind, Straight, Flush, FullHouse, FourOfKind, StraightFlush }
		if (Hand.HandValues.PokerHand == PokerHand.HighCard)
		{
			HandStrength = (Hand.HandValues.Total);
		}
		if(Hand.HandValues.PokerHand == PokerHand.Connectors)
		{
			HandStrength = (Hand.HandValues.Total + 10);
		}
		if(Hand.HandValues.PokerHand == PokerHand.SuitedConnectors)
		{
			HandStrength = (Hand.HandValues.Total + 15);
		}
		if(Hand.HandValues.PokerHand == PokerHand.OnePair)
		{
			HandStrength = (Hand.HandValues.Total + 15);
		}
		rateOfReturn = FindRateOfReturn();
		FoldCallRaiseDecision(rateOfReturn);
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
				foreach (Card boardCard in Table.instance._board)
				{
					testDeck.Remove(boardCard.cardType);
					testBoard.Add(boardCard.cardType);
				}
				for (int i = 0; i < testDeck.Count; i++)
				{
					if(testDeck[i].rank == Table.instance._board[0].cardType.rank)
					{
						if (testDeck[i].suit == Table.instance._board[0].cardType.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				for (int i = 0; i < testDeck.Count; i++)
				{
					if (testDeck[i].rank == Table.instance._board[1].cardType.rank)
					{
						if (testDeck[i].suit == Table.instance._board[1].cardType.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				for (int i = 0; i < testDeck.Count; i++)
				{
					if (testDeck[i].rank == Table.instance._board[2].cardType.rank)
					{
						if (testDeck[i].suit == Table.instance._board[2].cardType.suit)
						{
							testDeck.RemoveAt(i);
						}
					}
				}
				//set THIS as test player0
				playerCards[0].Add(myCard1);
				playerCards[0].Add(myCard2);
				//give two cards two each other testPlayer, and then remove those cards from the deck
				//also give them a seat number
				for (int i = 1; i < testPlayers.Count; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						int cardPos = Random.Range(0, testDeck.Count);
						CardType cardType = testDeck[cardPos];
						playerCards[i].Add(cardType);
						testDeck.Remove(cardType);
					}
				}
				//if we're on the flop, deal out two more card to the board
				//and take those from the deck
				if (Table.instance._board.Count == 3)
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
				else if (Table.instance._board.Count == 4)
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
		HandStrength = numberOfWins / 1000;
		Debug.Log("Player" + SeatPos + " has a HandStrength of " + HandStrength + " and a numberOfWins of " + numberOfWins);
		rateOfReturn = FindRateOfReturn();
		FoldCallRaiseDecision(rateOfReturn);
		yield break;
	}

	//okay so we essentially use this function in order to organize whatever list of chips we're passing it into lists by color
	//we can use this function then to create chip stacks and make bets
	//basically we go through the chips that were passed, look at the value, and add them to the proper list

	public List<List<GameObject>> OrganizeChipsIntoColorStacks(List<GameObject> chipsToOrganize)
	{
		List<List<GameObject>> colorChips = new List<List<GameObject>>();
		List<GameObject> redChips = new List<GameObject>();
		List<GameObject> blueChips = new List<GameObject>();
		List<GameObject> whiteChips = new List<GameObject>();
		List<GameObject> blackChips = new List<GameObject>();

		for (int chipIndex = 0; chipIndex < chipsToOrganize.Count; chipIndex++)
		{
			if(chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.RED_CHIP_VALUE)
			{
				redChips.Add(chipsToOrganize[chipIndex]);
			}
			else if(chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.BLUE_CHIP_VALUE)
			{
				blueChips.Add(chipsToOrganize[chipIndex]);
			}
			else if (chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.WHITE_CHIP_VALUE)
			{
				whiteChips.Add(chipsToOrganize[chipIndex]);
			}
			else if (chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.BLACK_CHIP_VALUE)
			{
				blackChips.Add(chipsToOrganize[chipIndex]);
			}
		}
		colorChips.Add(redChips);
		colorChips.Add(blueChips);
		colorChips.Add(whiteChips);
		colorChips.Add(blackChips);
		return colorChips;
	}

	//so here we're actually instantiating the chips that we passed in the last function. 
	//this function makes it so that we can have nicely organized chipstacks at any given moment
	//so first we pass the organized chipstacks
	//we set a parentChip to null in order to use it late
	//we have an amount we're going to increment the stack by, because when we organize a chip, we want to put it in the right place
	//then we have a list of the playerPositions, so we know where to put the chips, and an offset that we set to zero.
	//as well, for ease of use, we are going to create an empty container class so that all the chips have the same parent
	//so we make an offset for that in order to have them instantiate above the table and in not IN it.
	//then we make the chipContainer, which will hold all subsequent chips
	//we set the lastStackPos and firstStackPos to zero, so that we can revalue them later. 
	//we'll use these two values to make sure each chipStack is centered in their proper area
	//then we run the for-loop that makes the chips
	//for each list of coloredStacks we grab the first chip and make that the parent
	//we make that chip a chipStack (which is a class on Chips) and call all the appropriate functions and set the appropriate bools
	//then we set the increment size by the bounds of that chip and set the offset by those bounds as well, so that each chips instantiates
	//on top of the other
	//for every subsequent chip, we destory the rigidBody so that there are no collisions
	//then set all the necessary parent information and bools
	//we also add it to the list that the parent chip is holding
	//we do this for EACH stack of colored chips, which are then instantiated next to each other according to the offset
	//then in order to move them ALL to the center, we take the firstStackPos and the lastStackPos and find the average
	//and move the container there
	public void CreateAndOrganizeChipStacks(List<GameObject> chipsToOrganize )
	{
		List<List<GameObject>> organizedChips = OrganizeChipsIntoColorStacks(chipsToOrganize);
		GameObject parentChip = null;
		float incrementStackBy = 0;
		List<GameObject> playerPositions = new List<GameObject>
		{
			GameObject.Find("P0Cards"), GameObject.Find("P1Cards"), GameObject.Find("P2Cards"), GameObject.Find("P3Cards"), GameObject.Find("P4Cards")
		};
		Vector3 offSet = Vector3.zero;
		Vector3 containerOffset = Vector3.up * .08f;
		GameObject chipContainer = GameObject.Instantiate(new GameObject(), playerPositions[SeatPos].transform.position + containerOffset, playerPositions[SeatPos].transform.rotation);
        chipContainer.tag = "Container";
        chipContainer.name = "Container";
		chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
		Vector3 lastStackPos = Vector3.zero;
		Vector3 firstStackPos = Vector3.zero;
		for (int chipStacks = 0; chipStacks < organizedChips.Count; chipStacks++)
		{
			if (organizedChips[chipStacks].Count != 0)
			{
				for (int chipIndex = 0; chipIndex < organizedChips[chipStacks].Count; chipIndex++)
				{
					if (chipIndex == 0)
					{
						parentChip = organizedChips[chipStacks][0];
						parentChip.transform.parent = chipContainer.transform;
						parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
						parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
						if(parentChip.GetComponent<Rigidbody>() == null)
						{
							parentChip.AddComponent<Rigidbody>();
						}
						incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
						parentChip.transform.localPosition = offSet;
						offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
						if(firstStackPos == Vector3.zero)
						{
							firstStackPos = parentChip.transform.position;
						}
						lastStackPos = parentChip.transform.position;
					}
					else
					{
						if(organizedChips[chipStacks][chipIndex].GetComponent<Rigidbody>() != null)
						{
							GameObject.Destroy(organizedChips[chipStacks][chipIndex].GetComponent<Rigidbody>());
						}
						organizedChips[chipStacks][chipIndex].transform.parent = parentChip.transform;
						organizedChips[chipStacks][chipIndex].transform.position = new Vector3(parentChip.transform.position.x, parentChip.transform.position.y - (incrementStackBy * chipIndex), parentChip.transform.position.z);
						organizedChips[chipStacks][chipIndex].transform.rotation = parentChip.transform.rotation;
						organizedChips[chipStacks][chipIndex].GetComponent<Chip>().inAStack = true;
						organizedChips[chipStacks][chipIndex].GetComponent<Chip>().chipForBet = false;
						parentChip.GetComponent<Chip>().chipStack.chips.Add(organizedChips[chipStacks][chipIndex].GetComponent<Chip>());
						parentChip.GetComponent<Chip>().chipStack.stackValue += organizedChips[chipStacks][chipIndex].GetComponent<Chip>().chipValue;
					}
				}
			}
		}
		Vector3 trueOffset = firstStackPos - lastStackPos;
		chipContainer.transform.position += trueOffset / 2;
        GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
        foreach(GameObject container in emptyContainers)
        {
            if(container.transform.childCount == 0)
            {
                Destroy(container);
            }
        }
	}

	//this is LIKE create and organize chipStacks, except is used only during intialization
	public List<GameObject> SetChipStacks(int chipAmount)
	{

		List<GameObject> startingStack = new List<GameObject>();

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
		int blackChipCountMAX = 15;
		int whiteChipCountMAX = 25;
		int blueChipCountMAX = 25;

		blackChipCount = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
		valueRemaining -= blackChipCount * ChipConfig.BLACK_CHIP_VALUE;

		whiteChipCount = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
		valueRemaining -= whiteChipCount * ChipConfig.WHITE_CHIP_VALUE;

		blueChipCount = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
		valueRemaining -= blueChipCount * ChipConfig.BLUE_CHIP_VALUE;

		redChipCount = valueRemaining / ChipConfig.RED_CHIP_VALUE;

		for (int i = 0; i < blackChipCount; i++)
		{
			GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.BLACK_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
			startingStack.Add(newChip);
		}
		for (int i = 0; i < whiteChipCount; i++)
		{
			GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.WHITE_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
			startingStack.Add(newChip);
		}
		for (int i = 0; i < blueChipCount; i++)
		{
			GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.BLUE_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
			startingStack.Add(newChip);
		}
		for (int i = 0; i < redChipCount; i++)
		{
			GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.RED_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
			startingStack.Add(newChip);
		}

		return startingStack;
	}

	//so this controls all betting
	//but it also controls refiguring the player's chipstack AFTER a bet
	//it also controls making change in the event that a player doesn't have the proper chips to make a bet
	//that MIGHT end up being the dealer's job? but that can be later. for now the player makes their own change
	//so we create a count of all the chips by color
	//the problem with this code is that if any of the code falls outside the order of the list, EVERYTHING is wrong
	//so first, we find the max amount of each color chips the player has
	//then we set the valueRemaining to the betAmount
	//this lets us check if we have to make change
	//if by the end of the check, valueRemaining = 0, then we can go straight to instantating, if it isn't then we need to make change
	//if we make change, we add that amount to the colorChipCounts
	//all instantiating is based on the the int values in colorChipCount
	//then we go through each int and instantiate the proper chip in the bet zone
	//at the end of all of this, we recreate the chipstack to represent the new value
	//it's not super sleek, but it works
	public void Bet(int betAmount)
	{
		Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.chips);
		int oldChipStackValue = ChipCount;
		List<GameObject> playerBetZones = new List<GameObject>
		{
			GameObject.Find("P0BetZone"), GameObject.Find("P1BetZone"), GameObject.Find("P2BetZone"), GameObject.Find("P3BetZone"), GameObject.Find("P4BetZone")
		};

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

		if(valueRemaining > 0)
		{
			List<GameObject> chipChange = SetChipStacks(valueRemaining);
			for (int i = 0; i < chipChange.Count; i++)
			{
				if(chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.BLACK_CHIP_VALUE)
				{
					colorChipCount[0]++;
				}
				else if(chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.WHITE_CHIP_VALUE)
				{
					colorChipCount[1]++;
				}
				else if (chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.BLUE_CHIP_VALUE)
				{
					colorChipCount[2]++;
				}
				else if (chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.RED_CHIP_VALUE)
				{
					colorChipCount[3]++;
				}
			}
			foreach(GameObject chip in chipChange)
			{
				Table.instance.RemoveChipFrom(playerDestinations[SeatPos], chip.GetComponent<Chip>());
				chip.GetComponent<Chip>().DestroyChip();       
			}

		}

        //if there are less than 5 chips, don't even bother putting them in a stack. because why even?
        if((colorChipCount[0] + colorChipCount[1] + colorChipCount[2] + colorChipCount[3]) < 2)
        {
            for (int colorListIndex = 0; colorListIndex < colorChipCount.Count; colorListIndex++)
            {
                if (colorChipCount[colorListIndex] > 0)
                {
                    for (int colorCount = 0; colorCount < colorChipCount[colorListIndex]; colorCount++)
                    {
                        Vector3 offSet = new Vector3(Random.Range(0, .03f), .1f, Random.Range(0, .03f));
                        GameObject newChip = GameObject.Instantiate(FindChipPrefab(chipPrefab[colorListIndex]), playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                        newChip.GetComponent<Chip>().chipForBet = true;
                        Table.instance._potChips.Add(newChip.GetComponent<Chip>());
                        for (int tableChipIndex = 0; tableChipIndex < Table.instance.playerChipStacks[SeatPos].Count; tableChipIndex++)
                        {
                            if (newChip.GetComponent<Chip>().chipValue == Table.instance.playerChipStacks[SeatPos][tableChipIndex].chipValue && valueRemaining == 0)
                            {
                                Chip chipToRemove = Table.instance.playerChipStacks[SeatPos][tableChipIndex];
                                //Debug.Log("ChipRemoved was a " + chipToRemove.GetComponent<Chip>().chipValue + " chip");
                                //Debug.Log("Removing chip from seat" + SeatPos);
                                //Debug.Log(playerDestinations.Count);
                                //Debug.Log("Removing chip from seat" + playerDestinations[SeatPos]);
                                Table.instance.RemoveChipFrom(playerDestinations[SeatPos], chipToRemove);
                                chipToRemove.DestroyChip();
                                break;
                            }
                        }
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
            GameObject chipContainer = GameObject.Instantiate(new GameObject(), playerBetZones[SeatPos].transform.position + containerOffset, playerBetZones[SeatPos].transform.rotation);
            chipContainer.tag = "Container";
            chipContainer.name = "Container";
            chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
            Vector3 lastStackPos = Vector3.zero;
            Vector3 firstStackPos = Vector3.zero;
            for (int colorListIndex = 0; colorListIndex < colorChipCount.Count; colorListIndex++) //this runs 4 times, one for each color
            {
                if (colorChipCount.Count != 0) //if there is a number
                {
                    for (int chipIndex = 0; chipIndex < colorChipCount[colorListIndex]; chipIndex++)
                    {
                        if (chipIndex == 0)
                        {
                            GameObject newChip = GameObject.Instantiate(FindChipPrefab(chipPrefab[colorListIndex]), playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                            Table.instance.RemoveChipFrom(playerDestinations[SeatPos], newChip.GetComponent<Chip>());
                            Table.instance._potChips.Add(newChip.GetComponent<Chip>());
                            for (int tableChipIndex = 0; tableChipIndex < Table.instance.playerChipStacks[SeatPos].Count; tableChipIndex++)
                            {
                                if (newChip.GetComponent<Chip>().chipValue == Table.instance.playerChipStacks[SeatPos][tableChipIndex].chipValue && valueRemaining == 0)
                                {
                                    Chip chipToRemove = Table.instance.playerChipStacks[SeatPos][tableChipIndex];
                                    //Debug.Log("ChipRemoved was a " + chipToRemove.GetComponent<Chip>().chipValue + " chip");
                                    //Debug.Log("Removing chip from seat" + SeatPos);
                                    //Debug.Log(playerDestinations.Count);
                                    //Debug.Log("Removing chip from seat" + playerDestinations[SeatPos]);
                                    Table.instance.RemoveChipFrom(playerDestinations[SeatPos], chipToRemove);
                                    chipToRemove.DestroyChip();
                                    break;
                                }
                            }
                            parentChip = newChip;
                            parentChip.transform.parent = chipContainer.transform;
                            parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                            parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                            if (parentChip.GetComponent<Rigidbody>() == null)
                            {
                                parentChip.AddComponent<Rigidbody>();
                            }
                            incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
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
							GameObject newChip = GameObject.Instantiate(FindChipPrefab(chipPrefab[colorListIndex]), playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
							Destroy(newChip.GetComponent<Rigidbody>());
                            Table.instance.RemoveChipFrom(playerDestinations[SeatPos], newChip.GetComponent<Chip>());
                            Table.instance._potChips.Add(newChip.GetComponent<Chip>());
                            for (int tableChipIndex = 0; tableChipIndex < Table.instance.playerChipStacks[SeatPos].Count; tableChipIndex++)
                            {
                                if (newChip.GetComponent<Chip>().chipValue == Table.instance.playerChipStacks[SeatPos][tableChipIndex].chipValue && valueRemaining == 0)
                                {
                                    Chip chipToRemove = Table.instance.playerChipStacks[SeatPos][tableChipIndex];
                                    //Debug.Log("ChipRemoved was a " + chipToRemove.GetComponent<Chip>().chipValue + " chip");
                                    //Debug.Log("Removing chip from seat" + SeatPos);
                                    //Debug.Log(playerDestinations.Count);
                                    //Debug.Log("Removing chip from seat" + playerDestinations[SeatPos]);
                                    Table.instance.RemoveChipFrom(playerDestinations[SeatPos], chipToRemove);
                                    chipToRemove.DestroyChip();
                                    break;
                                }
                            }
                            newChip.transform.parent = parentChip.transform;
							newChip.transform.position = new Vector3(parentChip.transform.position.x, parentChip.transform.position.y - (incrementStackBy * chipIndex), parentChip.transform.position.z);
							newChip.transform.rotation = parentChip.transform.rotation;
							newChip.GetComponent<Chip>().inAStack = true;
							newChip.GetComponent<Chip>().chipForBet = false;
							parentChip.GetComponent<Chip>().chipStack.chips.Add(newChip.GetComponent<Chip>());
							parentChip.GetComponent<Chip>().chipStack.stackValue += newChip.GetComponent<Chip>().chipValue;
                        }
                    }
                }
            }
            Vector3 trueOffset = firstStackPos - lastStackPos;
            chipContainer.transform.position += trueOffset / 2;
            GameObject[] emptyContainers = GameObject.FindGameObjectsWithTag("Container");
            foreach (GameObject container in emptyContainers)
            {
                if (container.transform.childCount == 0)
                {
                    Destroy(container);
                }
            }
        }

		if (valueRemaining > 0)
		{
			int newChipStackValue = oldChipStackValue - betAmount;
			for (int i = Table.instance.playerChipStacks[SeatPos].Count - 1; i >= 0; i--)
			{
				Chip chip = Table.instance.playerChipStacks[SeatPos][i];
				Table.instance.RemoveChipFrom(playerDestinations[SeatPos], chip);
				Debug.Log("Removing a " + chip.chipValue + " chip");
				chip.DestroyChip();
			}
			Debug.Assert(Table.instance.playerChipStacks[SeatPos].Count == 0);
			List<GameObject> newChipStack = SetChipStacks(newChipStackValue);
			foreach(GameObject chip in newChipStack)
			{
				Table.instance.AddChipTo(playerDestinations[SeatPos], chip.GetComponent<Chip>());
				Debug.Log("adding a " + chip.GetComponent<Chip>().chipValue + " chip");
			}
            //if the chips that are physically in the players stack, are not equal to what they actually have, assert
			Debug.Assert(Table.instance.playerChipStacks[SeatPos].Count == newChipStackValue);
			CreateAndOrganizeChipStacks(newChipStack);
		}
		else
		{
			CreateAndOrganizeChipStacks(Table.instance.GetChipGameObjects(SeatPos));
		}
	}

	//this basically goes through a given chipValue and finds each instance of that chipValue in the playerChipStack
	public int FindChipMax(int chipValue)
	{
		int chipMax = 0;
		for (int i = 0; i < Table.instance.playerChipStacks[SeatPos].Count; i++)
		{
			if(chipValue == Table.instance.playerChipStacks[SeatPos][i].chipValue)
			{
				chipMax++;
			}
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

}

