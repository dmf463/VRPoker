using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /*
     * MINNIE PLAYER BEHAVIOUR
     */
    #region Minnie Player Behaviour Code

    public int MINNIE_DetermineRaiseAmount(PokerPlayerRedux player)
    {
        int raise = 0;
        int minimumRaise = Services.Dealer.LastBet;
        int modifier = 0;

        if (minimumRaise == 0) minimumRaise = Services.Dealer.BigBlind;

        if (Table.gameState == GameState.PreFlop)
        {
            if (((player.chipCount - Services.Dealer.LastBet) < (Services.Dealer.BigBlind * 4)) && player.HandStrength > 12f)
            {
                raise = player.chipCount;
            }
            else
            {
                if (Services.Dealer.LastBet == Services.Dealer.BigBlind)
                {
                    raise = Services.Dealer.BigBlind * (3 + Services.Dealer.GetActivePlayerCount());
                }
                else raise = Services.Dealer.LastBet * 2;
            }
            if (raise > player.chipCount) raise = player.chipCount;
        }
        else
        {
            modifier += Services.Dealer.GetActivePlayerCount(); //the more players there are the more unsafe your hand is, so bet bigger
            if (Services.Dealer.GetActivePlayerCount() > 2)
            {
                if (Table.instance.DealerPosition == player.SeatPos) modifier += 2; //if you're not heads up and you're the dealer, you're in good position
            }
            else
            {
                if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1) modifier = 0; //if you're first to act you're in a terrible position
                else if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 2) modifier = 1; //still pretty bad position
                else if (Services.Dealer.GetActivePlayerCount() - Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1 && Services.Dealer.GetActivePlayerCount() >= 3) modifier += 2; //good position
            }
            if ((player.chipCount - Services.Dealer.LastBet) > (Services.Dealer.BigBlind * 4)) modifier += 1; //if you can cover the next few hands thats good
            else modifier -= 1; //if you can't that's bad.

            if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand < PokerHand.OnePair) modifier += 3; //if you have a good HS but have less than OnePair, you're probably on a draw, so bet bigger
            else if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand > PokerHand.Flush) modifier = 1; //if you have a good HS and you have better than a flush, slow play it 

            if (player.HandStrength > .6) modifier += 1; //if you have a decent hand
            else modifier -= 1; //if you have a "meh" hand

            modifier -= Services.Dealer.raisesInRound; //cut down on how much you're betting based on betting history
            if (modifier <= 0) modifier = 1; //if it's less than zero, make it 1
            raise = minimumRaise * Mathf.Abs(modifier);
            Debug.Log(player.playerName + " is raising " + raise + " because of a modifier = " + modifier);
            if (raise > player.chipCount) raise = player.chipCount;
        }
        #region old Raise code
        //if (Table.gameState == GameState.PreFlop)
        //{
        //    #region pre-flop raises
        //    if (HandStrength > .8)
        //    {
        //        //95% chance to raise big, 5% to go all in
        //        float randomNum = Random.Range(0, 100);
        //        if (randomNum < 95)
        //        {
        //            if (Services.Dealer.LastBet == Services.Dealer.BigBlind)
        //            {
        //                raise = Services.Dealer.BigBlind * (3 + Services.Dealer.GetActivePlayerCount());
        //            }
        //            else raise = Services.Dealer.LastBet * 2;
        //        }
        //        else raise = chipCount;
        //    }
        //    else if (HandStrength > .5)
        //    {
        //        //98% chance to raise big, 2% to go all in 
        //        float randomNum = Random.Range(0, 100);
        //        if (randomNum < 98)
        //        {
        //            if (Services.Dealer.LastBet == Services.Dealer.BigBlind)
        //            {
        //                raise = Services.Dealer.BigBlind * (3 + Services.Dealer.GetActivePlayerCount());
        //            }
        //            else raise = Services.Dealer.LastBet * 2;
        //        }
        //        else raise = chipCount;
        //    }
        //    else if (HandStrength > .3)
        //    {
        //        //70% chance to raise big 30% chance to min raise 
        //        float randomNum = Random.Range(0, 100);
        //        if (randomNum < 70)
        //        {
        //            if (Services.Dealer.LastBet == Services.Dealer.BigBlind)
        //            {
        //                raise = Services.Dealer.BigBlind * (3 + Services.Dealer.GetActivePlayerCount());
        //            }
        //            else raise = Services.Dealer.LastBet;
        //        }
        //    }
        //    else raise = Services.Dealer.LastBet;
        //    #endregion
        //}
        //else
        //{
        //    #region post-flop raises
        //    if (HandStrength > .8)
        //    {
        //        //95% chance to raise pot, 5% to go all in
        //        float randomNum = Random.Range(0, 100);
        //        if (randomNum < 95)
        //        {
        //            raise = Table.instance.potChips;
        //        }
        //        else raise = chipCount;
        //    }
        //    else if (HandStrength > .5)
        //    {
        //        //50% to raise pot, 35% to half pot, 10% 2x lastBet (or 3x big blind), 5% all in
        //        float randomNum = Random.Range(0, 100);
        //        if (randomNum < 50)
        //        {
        //            raise = Table.instance.potChips;
        //        }
        //        else if (randomNum - 50 < 35)
        //        {
        //            int remainder = (Table.instance.potChips / 2) % ChipConfig.RED_CHIP_VALUE;
        //            if (remainder > 0) raise = ((Table.instance.potChips / 2) - remainder) + ChipConfig.RED_CHIP_VALUE;
        //            else raise = Table.instance.potChips / 2;
        //        }
        //        else if (randomNum - 50 < 10)
        //        {
        //            if (Services.Dealer.LastBet == 0)
        //            {
        //                raise = Services.Dealer.BigBlind * 3;
        //            }
        //            else raise = Services.Dealer.LastBet * 2;
        //        }
        //        else if (randomNum - 50 < 5)
        //        {
        //            raise = chipCount;
        //        }
        //    }
        //    else if (HandStrength > .3)
        //    {
        //        //70% raise half pot, 30% 2x lastbest (or 3x big blind)
        //        float randomNum = Random.Range(0, 100);
        //        if (randomNum < 70)
        //        {
        //            int remainder = (Table.instance.potChips / 2) % ChipConfig.RED_CHIP_VALUE;
        //            if (remainder > 0) raise = ((Table.instance.potChips / 2) - remainder) + ChipConfig.RED_CHIP_VALUE;
        //            else raise = Table.instance.potChips / 2;
        //        }
        //        else
        //        {
        //            if (Services.Dealer.LastBet == 0)
        //            {
        //                raise = Services.Dealer.BigBlind * 3;
        //            }
        //            else raise = Services.Dealer.LastBet * 2;
        //        }
        //    }
        //    else
        //    {
        //        if (Services.Dealer.LastBet == 0)
        //        {
        //            raise = Services.Dealer.BigBlind * 3;
        //        }
        //        else raise = Services.Dealer.LastBet;
        //    }
        //    #endregion
        //}
        //if (raise % ChipConfig.RED_CHIP_VALUE > 0)
        //{
        //    Debug.Log("Invalid Raise Amount");
        //    return 0;
        //}
        //if (raise == 0) raise = Services.Dealer.BigBlind * 3;
        #endregion
        return raise;
    }

    //this is the FCR decision and this is where we can adjust player types
    //we should go back to the generic one and make percentage variables that we can adjust in individual players
    public void MINNIE_FoldCallRaiseDecision(float returnRate, PokerPlayerRedux player)
    {
        //Debug.Log("Player " + SeatPos + " has a HS " + HandStrength);
        if (Table.gameState == GameState.PreFlop)
        {
            if (((player.chipCount - Services.Dealer.LastBet) < (Services.Dealer.BigBlind * 4)) && player.HandStrength > 12)
            {
                player.AllIn();
            }
            else if (player.HandStrength > 12 && player.timesRaisedThisRound == 0) player.Raise();
            else if (player.HandStrength < 5)
            {
                if ((Services.Dealer.LastBet - player.currentBet == 0) ||
                    ((Services.Dealer.LastBet - player.currentBet == Services.Dealer.SmallBlind) &&
                       Services.Dealer.GetActivePlayerCount() == 2) &&
                       ((player.chipCount - Services.Dealer.LastBet) > (Services.Dealer.BigBlind * 4))) player.Call();
                else player.Fold();
            }
            else player.Call();

            player.turnComplete = true;
            player.actedThisRound = true;
        }
        else
        {
            PokerPlayerRedux aggressor = null;
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if (Services.Dealer.players[i].isAggressor)
                {
                    aggressor = Services.Dealer.players[i];
                    break;
                }
            }
            if (Table.gameState == GameState.River && Services.Dealer.LastBet == 0) //if you're on the river and no one has bet, take a stab at it.
            {
                float randomNum = Random.Range(0, 100);
                if (randomNum > 50) player.Raise();
                else player.Call();
            }
            else if (aggressor != null)
            {
                if (aggressor.timesRaisedThisRound > 2 && player.timesRaisedThisRound > 1 && player.HandStrength > .85f) player.AllIn(); //if you're in a raise war, and you have a great hand just go all in
                else if (aggressor.PlayerState == PlayerState.Playing && aggressor.currentBet > Table.instance.potChips / Services.Dealer.raisesInRound) //if an aggressor has bet a huuge chunk of the pot
                {
                    if (player.Hand.HandValues.PokerHand < PokerHand.OnePair && player.HandStrength < .5f) player.Fold(); //if you have a bad hand, fold
                    else player.Call(); //else just call
                }
                else player.DetermineAction(returnRate, player);
            }
            else if ((player.chipCount - Services.Dealer.LastBet) < (Services.Dealer.BigBlind * 4) && player.HandStrength < 0.5) player.Fold(); //if betting would take the majority of your stack and you have a bad hand fold.
            else if (player.amountToRaise > player.chipCount / 4 && player.HandStrength < .75f || Services.Dealer.LastBet > player.chipCount / 4 && player.HandStrength < .75f) player.Fold(); //if you would bet 1/4 of your stack, and only a 75% chance of winning, fold
            else if (player.amountToRaise > player.chipCount / 4 && player.HandStrength < .85f || Services.Dealer.LastBet > player.chipCount / 4 && player.HandStrength < .85f) player.Call(); //75% - 85% chance of winning, call
            else if (player.amountToRaise > player.chipCount / 4 && player.HandStrength > .85f || Services.Dealer.LastBet > player.chipCount / 4 && player.HandStrength > .85f) player.AllIn(); //more than 85% go all in
            else if (Services.Dealer.raisesInRound >= 2 && player.Hand.HandValues.PokerHand >= PokerHand.OnePair && player.HandStrength < .6f) //if there are more than two raises and you have a decent hand, call or fold.
            {
                float randomNum = Random.Range(0, 100);
                if (randomNum > 70) player.Call();
                else player.Fold();
            }
            else player.DetermineAction(returnRate, player);
            player.turnComplete = true;
            player.actedThisRound = true;
        }
    }

    public void MINNIE_DetermineAction(float returnRate, PokerPlayerRedux player)
    {
        if (returnRate < player.lowReturnRate)
        {
            //Debug.Log("lowReturnRate");
            //95% chance fold, 5% bluff (raise)
            float randomNumber = Random.Range(0, 100);
            if (randomNumber < player.foldChanceLow)
            {
                //if there's not bet, don't fold, just call for free.
                if (Services.Dealer.LastBet > 0) player.Fold();
                else player.Call();
            }
            else if (randomNumber - player.foldChanceLow < player.callChanceLow) player.Call();
            else
            {
                if (Services.Dealer.previousPlayerToAct != null)
                {
                    if (Services.Dealer.previousPlayerToAct.playerIsAllIn) player.Call();
                    else player.Raise();
                }
                else player.Raise();
            }

        }
        else if (returnRate < player.decentReturnRate)
        {
            //Debug.Log("decentReturnRate");
            //80% chance fold, 5% call, 15% bluff(raise)
            float randomNumber = Random.Range(0, 100);
            if (randomNumber < player.foldChanceDecent)
            {
                if (Services.Dealer.LastBet > 0) player.Fold();
                else player.Call();
            }
            else if (randomNumber - player.foldChanceDecent < player.callChanceDecent) player.Call();
            else
            {
                if (Services.Dealer.previousPlayerToAct != null)
                {
                    if (Services.Dealer.previousPlayerToAct.playerIsAllIn) player.Call();
                    else player.Raise();
                }
                else player.Raise();
            }

        }
        else if (returnRate < player.highReturnRate)
        {
            //Debug.Log("highReturnRate");
            //60% chance call, 40% raise
            float randomNumber = Random.Range(0, 100);
            if (randomNumber < player.foldChanceHigh)
            {
                if (Services.Dealer.LastBet > 0) player.Fold();
                else player.Call();
            }
            else if (randomNumber - player.foldChanceHigh < player.callChanceHigh) player.Call();
            else
            {
                if (Services.Dealer.previousPlayerToAct != null)
                {
                    if (Services.Dealer.previousPlayerToAct.playerIsAllIn) player.Call();
                    else player.Raise();
                }
                else player.Raise();
            }

        }
        else if (returnRate >= player.highReturnRate)
        {
            //Debug.Log("superHighReturnRate");
            //70% chance raise, 30% call
            float randomNumber = Random.Range(0, 100);
            if (randomNumber < player.foldChanceVeryHigh)
            {
                if (Services.Dealer.LastBet > 0) player.Fold();
                else player.Call();
            }
            else if (randomNumber - player.foldChanceVeryHigh < player.callChanceVeryHigh) player.Call();
            else
            {
                if (Services.Dealer.previousPlayerToAct != null)
                {
                    if (Services.Dealer.previousPlayerToAct.playerIsAllIn) player.Call();
                    else player.Raise();
                }
                else player.Raise();
            }
        }
    }

    #endregion
}
