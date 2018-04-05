using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BehaviorTree;
using System;

public class PlayerBehaviour {

    private Tree<PokerPlayerRedux> FCR_Tree;
    private Tree<PokerPlayerRedux> preflop_FCR_Tree;

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
        if (Table.gameState >= GameState.Turn) minimumRaise = Table.instance.potChips / 4;
        int remainder = minimumRaise % ChipConfig.RED_CHIP_VALUE;
        if (remainder > 0)
        {
            minimumRaise = (minimumRaise - remainder) + ChipConfig.RED_CHIP_VALUE;
        }

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
                else
                {
                    if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1) modifier = 0; //if you're first to act you're in a terrible position
                    else if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 2) modifier = 1; //still pretty bad position
                    else if (Services.Dealer.GetActivePlayerCount() - Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1 && Services.Dealer.GetActivePlayerCount() >= 3) modifier += 2; //good position
                }
            }
            if ((player.chipCount - Services.Dealer.LastBet) > (Services.Dealer.BigBlind * 4)) modifier += 1; //if you can cover the next few hands thats good
            else modifier -= 1; //if you can't that's bad.

            if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand < PokerHand.OnePair) modifier += 3; //if you have a good HS but have less than OnePair, you're probably on a draw, so bet bigger
            else if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand > PokerHand.Flush && Table.gameState < GameState.River) modifier = 1; //if you have a good HS and you have better than a flush, slow play it 

            if (player.HandStrength > .6) modifier += 1; //if you have a decent hand
            else modifier -= 1; //if you have a "meh" hand

            modifier -= Services.Dealer.raisesInRound; //cut down on how much you're betting based on betting history
            if (modifier <= 0) modifier = 1; //if it's less than zero, make it 1
            raise = minimumRaise * Mathf.Abs(modifier);
            Debug.Log(player.playerName + " is raising " + raise + " because of a modifier = " + modifier);
            if (raise > player.chipCount) raise = player.chipCount;
        }
        return raise;
    }
    #endregion

    /*
   * FLOYD PLAYER BEHAVIOUR
   */
    #region FLOYD Player Behaviour Code

    public int FLOYD_DetermineRaiseAmount(PokerPlayerRedux player)
    {
        int raise = 0;
        int minimumRaise = Services.Dealer.LastBet;
        int modifier = 0;

        if (minimumRaise == 0) minimumRaise = Services.Dealer.BigBlind;
        if (Table.gameState >= GameState.Turn) minimumRaise = Table.instance.potChips / 4;
        int remainder = minimumRaise % ChipConfig.RED_CHIP_VALUE;
        if (remainder > 0)
        {
            minimumRaise = (minimumRaise - remainder) + ChipConfig.RED_CHIP_VALUE;
        }

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
            if (Services.Dealer.GetActivePlayerCount() > 2) //as long as you're not heads up
            {

                if (Table.instance.DealerPosition == player.SeatPos)
                {
                    if (Services.Dealer.raisesInRound == 0) modifier += 4;
                    else modifier += 2; //if you're not heads up and you're the dealer, you're in good position
                }
                else
                {
                    if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1) modifier = 0; //if you're first to act you're in a terrible position
                    else if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 2) modifier = 1; //still pretty bad position
                    else if (Services.Dealer.GetActivePlayerCount() - Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1 && Services.Dealer.GetActivePlayerCount() >= 3) modifier += 3; //good position
                }
            }
            if ((player.chipCount - Services.Dealer.LastBet) > (Services.Dealer.BigBlind * 4)) modifier += 1; //if you can cover the next few hands thats good
            else modifier = 0; //if you can't that's bad.

            if (player.HandStrength > .75 && player.Hand.HandValues.PokerHand < PokerHand.OnePair) modifier += 3; //if you have a good HS but have less than OnePair, you're probably on a draw, so bet bigger
            else if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand > PokerHand.Flush) modifier = 0; //if you have a good HS and you have better than a flush, slow play it 

            if (player.HandStrength > .5) modifier += 1; //if you have a decent hand
            else modifier -= 1; //if you have a "meh" hand

            if (player.chipCount >= Services.Dealer.startingChipCount) modifier += UnityEngine.Random.Range(2, 5);
            else if (player.chipCount < Services.Dealer.startingChipCount) modifier -= UnityEngine.Random.Range(2, 5);

            modifier -= Services.Dealer.raisesInRound; //cut down on how much you're betting based on betting history
            if (modifier <= 0) modifier = 1; //if it's less than zero, make it 1
            raise = minimumRaise * Mathf.Abs(modifier);
            Debug.Log(player.playerName + " is raising " + raise + " because of a modifier = " + modifier);
            if (raise > player.chipCount) raise = player.chipCount;
        }
        return raise;
    }
    #endregion

    /*
    * CASEY PLAYER BEHAVIOUR
    */
    #region CASEY Player Behaviour Code

    public int CASEY_DetermineRaiseAmount(PokerPlayerRedux player)
    {
        int raise = 0;
        int minimumRaise = Services.Dealer.LastBet;
        int modifier = 0;

        if (minimumRaise == 0) minimumRaise = Services.Dealer.BigBlind;
        if (Table.gameState >= GameState.Turn) minimumRaise = Table.instance.potChips / 4;
        int remainder = minimumRaise % ChipConfig.RED_CHIP_VALUE;
        if (remainder > 0)
        {
            minimumRaise = (minimumRaise - remainder) + ChipConfig.RED_CHIP_VALUE;
        }

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
            //modifier += Services.Dealer.GetActivePlayerCount(); //the more players there are the more unsafe your hand is, so bet bigger
            if (Services.Dealer.GetActivePlayerCount() > 2)
            {
                //if (Table.instance.DealerPosition == player.SeatPos) modifier += 2; //if you're not heads up and you're the dealer, you're in good position
                //else
                //{
                //    if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1) modifier = 0; //if you're first to act you're in a terrible position
                //    else if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 2) modifier = 1; //still pretty bad position
                //    else if (Services.Dealer.GetActivePlayerCount() - Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1 && Services.Dealer.GetActivePlayerCount() >= 3) modifier += 2; //good position
                //}
            }
            if ((player.chipCount - Services.Dealer.LastBet) > (Services.Dealer.BigBlind * 4)) modifier += 1; //if you can cover the next few hands thats good
            else modifier -= 1; //if you can't that's bad.

            if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand < PokerHand.OnePair) modifier += 3; //if you have a good HS but have less than OnePair, you're probably on a draw, so bet bigger
            else if (player.HandStrength > .7 && player.Hand.HandValues.PokerHand > PokerHand.Flush) modifier = 3; //if you have a good HS and you have better than a flush, slow play it 

            if (player.HandStrength > .8) modifier += 5; //if you have a decent hand
            else modifier -= 1; //if you have a "meh" hand

            modifier -= Services.Dealer.raisesInRound; //cut down on how much you're betting based on betting history
            if (modifier <= 0) modifier = 1; //if it's less than zero, make it 1
            raise = minimumRaise * Mathf.Abs(modifier);
            Debug.Log(player.playerName + " is raising " + raise + " because of a modifier = " + modifier);
            if (raise > player.chipCount) raise = player.chipCount;
        }
        return raise;
    }
    #endregion

    /*
    * ZOMBIE PLAYER BEHAVIOUR
    */
    #region ZOMBIE Player Behaviour Code

    public int ZOMBIE_DetermineRaiseAmount(PokerPlayerRedux player)
    {
        int raise = 0;
        int minimumRaise = Services.Dealer.LastBet;
        int modifier = 0;

        if (minimumRaise == 0) minimumRaise = Services.Dealer.BigBlind;
        int remainder = minimumRaise % ChipConfig.RED_CHIP_VALUE;
        if (remainder > 0)
        {
            minimumRaise = (minimumRaise - remainder) + ChipConfig.RED_CHIP_VALUE;
        }
        if (Table.gameState >= GameState.Turn) minimumRaise = Table.instance.potChips / 4;

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
                else
                {
                    if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1) modifier = 0; //if you're first to act you're in a terrible position
                    else if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 2) modifier = 1; //still pretty bad position
                    else if (Services.Dealer.GetActivePlayerCount() - Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1 && Services.Dealer.GetActivePlayerCount() >= 3) modifier += 2; //good position
                }
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
        return raise;
    }
    #endregion

    /*
    * NATHANIEL PLAYER BEHAVIOUR
    */
    #region NATHANIEL Player Behaviour Code

    public int NATHANIEL_DetermineRaiseAmount(PokerPlayerRedux player)
    {
        int raise = 0;
        int minimumRaise = Services.Dealer.LastBet;
        int modifier = 0;

        if (minimumRaise == 0) minimumRaise = Services.Dealer.BigBlind;
        if (Table.gameState >= GameState.Turn) minimumRaise = Table.instance.potChips / 4;
        int remainder = minimumRaise % ChipConfig.RED_CHIP_VALUE;
        if (remainder > 0)
        {
            minimumRaise = (minimumRaise - remainder) + ChipConfig.RED_CHIP_VALUE;
        }

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
                else
                {
                    if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1) modifier = 0; //if you're first to act you're in a terrible position
                    else if (Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 2) modifier = 1; //still pretty bad position
                    else if (Services.Dealer.GetActivePlayerCount() - Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(player.SeatPos) == 1 && Services.Dealer.GetActivePlayerCount() >= 3) modifier += 2; //good position
                }
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
        return raise;
    }
    #endregion

    public List<PokerPlayerRedux> RankedPlayerHands(PokerPlayerRedux me)
    {
        List<PokerPlayerRedux> playersToEvaluate = new List<PokerPlayerRedux>();
        playersToEvaluate.Add(me);
        for (int i = 0; i < Services.Dealer.PlayerAtTableCount(); i++)
        {
            if (Services.Dealer.players[i].lastAction != PlayerAction.None && Services.Dealer.players[i] != me)
            {
                Services.Dealer.players[i].percievedHandStrength = Services.Dealer.players[i].HandStrength + (AdjustHandStrength(Services.Dealer.players[i]));
                playersToEvaluate.Add(Services.Dealer.players[i]);
            }
        }
        me.percievedHandStrength = me.HandStrength;
        List<PokerPlayerRedux> sortedPlayers = new List<PokerPlayerRedux>(playersToEvaluate.OrderByDescending(bestHand => bestHand.percievedHandStrength));
        return sortedPlayers;
    }

    public float AdjustHandStrength(PokerPlayerRedux opponent)
    {
        float betMod = 0.25f;
        float otherBetMod = 0;
        float exponent = .25f;
        float mod = UnityEngine.Random.Range(-0.2f, 0.2f);
        mod += 0.1f * (Services.Dealer.GetActivePlayerCount() - Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(opponent.SeatPos));
        mod += betMod * Mathf.Pow(opponent.currentBet, exponent) + otherBetMod;
        return mod;
    }

    public void PreFlopFoldCallRaise(PokerPlayerRedux player)
    {
        if (((player.chipCount - Services.Dealer.LastBet) < (Services.Dealer.BigBlind * 4)) && player.HandStrength > 12)
        {
            player.AllIn();
        }
        else if (player.HandStrength > 12 && player.timesRaisedThisRound == 0) player.Raise();
        else if (player.HandStrength < 4)
        {
            if ((Services.Dealer.LastBet - player.currentBet == 0) ||
                ((Services.Dealer.LastBet - player.currentBet == Services.Dealer.SmallBlind) &&
                   Services.Dealer.GetActivePlayerCount() == 2) &&
                   ((player.chipCount - Services.Dealer.LastBet) > (Services.Dealer.BigBlind * 4))) player.Call();
            else player.Fold();
        }
        else
        {
            if (Services.Dealer.raisesInRound > 1 && player.HandStrength < 8) player.Fold();
            else player.Call();
        }
        player.turnComplete = true;
        player.actedThisRound = true;
    }

    public void Floyd_Preflop_FCR(PokerPlayerRedux player)
    {
        preflop_FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
        //BIG STACK BET
        new Sequence<PokerPlayerRedux>(
            new IsChipLeader(),
            new Not<PokerPlayerRedux>(new SomeoneHasRaised()),
            new Raise()
            ),
        //BULLY
        new Sequence<PokerPlayerRedux>(
            new HasMoreMoneyThanOpponent(),
            new Not<PokerPlayerRedux>(new SomeoneHasRaised()),
            new Raise()
            ),
        //ASSHOLE BET
        new Sequence<PokerPlayerRedux>(
            new Condition<PokerPlayerRedux>(context => UnityEngine.Random.Range(0, 100) < 40),
            new HasEnoughMoney(),
            new Not<PokerPlayerRedux>(new SomeoneHasRaised()),
            new Raise()
            ),
        //PROTECT YOUR STACK
        new Sequence<PokerPlayerRedux>(
            new Not<PokerPlayerRedux>(new HasEnoughMoney()),
            new HasAGreatHand_PreFlop(),
            new AllIn()
            ),
        //RAISE ON A GOOD HAND
        new Sequence<PokerPlayerRedux>(
            new HasEnoughMoney(),
            new HasAGreatHand_PreFlop(),
            new Not<PokerPlayerRedux>(new RaisedAlready()),
            new Raise()
            ),
        //CALL ON DECENT HAND OR IF SMALL BLIND OR BIG BLIND
        new Sequence<PokerPlayerRedux>(
            new HasEnoughMoney(),
            new Selector<PokerPlayerRedux>(
                new Sequence<PokerPlayerRedux>(
                    new IsSmallBlind(),
                    new Call()
                    ),
                new Sequence<PokerPlayerRedux>(
                    new IsBigBlind(),
                    new Call()
                    ),
                new Sequence<PokerPlayerRedux>(
                    new Not<PokerPlayerRedux>(new HasAGreatHand_PreFlop()),
                    new Not<PokerPlayerRedux>(new HasABadHand_PreFlop()),
                    new Call()
                    )
                )
            ),
        //SOMEONE RAISED AND YOU DONT HAVE A GREAT HAND
        new Sequence<PokerPlayerRedux>(
            new SomeoneHasRaised(),
            new Condition<PokerPlayerRedux>(p => player.HandStrength > 8),
            new Call()
            ),
        //CALL ON BIG BLIND EVEN IF LOW STACK IF NO ONE RAISED
        new Sequence<PokerPlayerRedux>(
            new IsBigBlind(),
            new Call()
            ),
        new Fold()
        ));

        preflop_FCR_Tree.Update(player);
    }
    public void Floyd_FCR(PokerPlayerRedux player)
    {
        FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
                //BLUFF
                new Sequence<PokerPlayerRedux>(
                    new HasABadHand(),
                    new HasEnoughMoney(),
                    new BetIsZero(),
                    new IsInPosition(),
                    new Raise()
                    ),
                //ON TILT
                new Sequence<PokerPlayerRedux>(
                    new IsOnALoseStreak(),
                    new HasEnoughMoney(),
                    new BetIsZero(),
                    new Raise()
                    ),
                //HATES A PLAYER
                new Sequence<PokerPlayerRedux>(
                    new LostManyHandsToOpponent(),
                    new HasEnoughMoney(),
                    new BetIsZero(),
                    new Raise()
                    ),
                //CONTINUATION
                new Sequence<PokerPlayerRedux>(
                    new BetPreFlop(),
                    new Condition<PokerPlayerRedux>(context => player.Hand.HandValues.PokerHand <= PokerHand.OnePair),
                    new BetIsZero(),
                    new HasEnoughMoney(),
                    new Raise()
                    ),
                //SLOW PLAY
                new Sequence<PokerPlayerRedux>(
                    new HasAGreathand(),
                    new Condition<PokerPlayerRedux>(context => Services.Dealer.GetActivePlayerCount() <= 3),
                    new BeforeRiver(),
                    new Selector<PokerPlayerRedux>(
                        new Sequence<PokerPlayerRedux>(
                            new BetIsZero(),
                            new Raise()
                            ),
                        new Sequence<PokerPlayerRedux>(
                            new Not<PokerPlayerRedux>(new BetIsZero()),
                            new Call()
                            )
                        )
                    ),
                //BULLY IN GENERAL
                new Sequence<PokerPlayerRedux>(
                    new IsChipLeader(),
                    new BetIsZero(),
                    new Raise()
                    ),
                //BULLY ONE ON ON
                new Sequence<PokerPlayerRedux>(
                    new HasMoreMoneyThanOpponent(),
                    new BetIsZero(),
                    new Raise()
                    ),
                //POSITION PLAY
                new Sequence<PokerPlayerRedux>(
                    new IsInPosition(),
                    new Selector<PokerPlayerRedux>(
                        new Sequence<PokerPlayerRedux>(
                            new BetIsZero(),
                            new Raise()
                        ),
                        new Sequence<PokerPlayerRedux>(
                            new Not<PokerPlayerRedux>(new BetIsZero()),
                            new HasAGoodHand(),
                            new Call()
                        )
                    )
                ),
                //CALL
                new Sequence<PokerPlayerRedux>(
                    new HasEnoughMoney(),
                    new HasAGoodHand(),
                    new Not<PokerPlayerRedux>(new BetIsZero()),
                    new Call()
                ),
                //RAISE
                new Sequence<PokerPlayerRedux>(
                    new HasEnoughMoney(),
                    new HasAGreathand(),
                    new Raise()
                ),
                //FOLD
                new Selector<PokerPlayerRedux>(
                    new Sequence<PokerPlayerRedux>(
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<PokerPlayerRedux>(
                        new Not<PokerPlayerRedux>(new BetIsZero()),
                        new Fold()
                    )
                ),
                new Fold()
                ));
        FCR_Tree.Update(player);
    }


    public void Casey_Preflop_FCR(PokerPlayerRedux player)
    {
        preflop_FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
           //SCARED OF OPPONENT
           new Sequence<PokerPlayerRedux>(
               new LostManyHandsToOpponent(),
               new Not<PokerPlayerRedux>(new HasAGreatHand_PreFlop()),
               new Not<PokerPlayerRedux>(new IsSmallBlind()),
               new Not<PokerPlayerRedux>(new IsBigBlind()),
               new Fold()
               ),
           //PROTECT YOUR STACK
           new Sequence<PokerPlayerRedux>(
               new Not<PokerPlayerRedux>(new HasEnoughMoney()),
               new HasAGreatHand_PreFlop(),
               new AllIn()
               ),
           //RAISE ON A GOOD HAND
           new Sequence<PokerPlayerRedux>(
               new HasEnoughMoney(),
               new HasAGreatHand_PreFlop(),
               new Not<PokerPlayerRedux>(new SomeoneHasRaised()),
               new Not<PokerPlayerRedux>(new RaisedAlready()),
               new Raise()
               ),
           //CALL ON DECENT HAND OR IF SMALL BLIND OR BIG BLIND
           new Sequence<PokerPlayerRedux>(
               new HasEnoughMoney(),
               new Selector<PokerPlayerRedux>(
                   new Sequence<PokerPlayerRedux>(
                       new IsSmallBlind(),
                       new Call()
                       ),
                   new Sequence<PokerPlayerRedux>(
                       new IsBigBlind(),
                       new Call()
                       ),
                   new Sequence<PokerPlayerRedux>(
                       new Not<PokerPlayerRedux>(new HasAGreatHand_PreFlop()),
                       new Not<PokerPlayerRedux>(new HasABadHand_PreFlop()),
                       new Call()
                       ),
                   new Sequence<PokerPlayerRedux>(
                       new HasAGreatHand_PreFlop(),
                       new Call()
                       )
                   )
               ),
           //SOMEONE RAISED AND YOU DONT HAVE A GREAT HAND
           new Sequence<PokerPlayerRedux>(
               new SomeoneHasRaised(),
               new Condition<PokerPlayerRedux>(p => player.HandStrength > 8),
               new Call()
               ),
          //CALL ON BIG BLIND EVEN IF LOW STACK IF NO ONE RAISED
          new Sequence<PokerPlayerRedux>(
              new IsBigBlind(),
              new Call()
              ),
           new Fold()
           ));
        preflop_FCR_Tree.Update(player);
    }
    public void Casey_FCR(PokerPlayerRedux player)
    {
        FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
                //SCARED OF OPPONENT
                new Sequence<PokerPlayerRedux>(
                    new LostManyHandsToOpponent(),
                    new Not<PokerPlayerRedux>(new HasAGreathand()),
                    new Not<PokerPlayerRedux>(new BetIsZero()),
                    new Fold()
                    ),
                //CALL
                new Sequence<PokerPlayerRedux>(
                    new HasEnoughMoney(),
                    new HasAGoodHand(),
                    new Not<PokerPlayerRedux>(new BetIsZero()),
                    new Call()
                ),
                //RAISE
                new Sequence<PokerPlayerRedux>(
                    new HasEnoughMoney(),
                    new HasAGreathand(),
                    new Raise()
                ),
                //FOLD
                new Selector<PokerPlayerRedux>(
                    new Sequence<PokerPlayerRedux>(
                        new BetIsZero(),
                        new Call()
                        ),
                    new Sequence<PokerPlayerRedux>(
                        new Not<PokerPlayerRedux>(new BetIsZero()),
                        new Fold()
                    )
                ),
                new Fold()
                ));
        FCR_Tree.Update(player);
    }


    public void Minnie_Preflop_FCR(PokerPlayerRedux player)
    {
        preflop_FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(

          //PROTECT YOUR STACK
          new Sequence<PokerPlayerRedux>(
              new Not<PokerPlayerRedux>(new HasEnoughMoney()),
              new HasAGreatHand_PreFlop(),
              new AllIn()
              ),
          //RAISE ON A GOOD HAND
          new Sequence<PokerPlayerRedux>(
              new HasEnoughMoney(),
              new HasAGreatHand_PreFlop(),
              new Not<PokerPlayerRedux>(new RaisedAlready()),
              new Raise()
              ),
          //CALL ON DECENT HAND OR IF SMALL BLIND OR BIG BLIND
          new Sequence<PokerPlayerRedux>(
              new HasEnoughMoney(),
              new Selector<PokerPlayerRedux>(
                  new Sequence<PokerPlayerRedux>(
                      new IsSmallBlind(),
                      new Call()
                      ),
                  new Sequence<PokerPlayerRedux>(
                      new IsBigBlind(),
                      new Call()
                      ),
                  new Sequence<PokerPlayerRedux>(
                      new Not<PokerPlayerRedux>(new HasAGreatHand_PreFlop()),
                      new Not<PokerPlayerRedux>(new HasABadHand_PreFlop()),
                      new Call()
                      )
                  )
              ),
          //SOMEONE RAISED AND YOU DONT HAVE A GREAT HAND
          new Sequence<PokerPlayerRedux>(
              new SomeoneHasRaised(),
              new Condition<PokerPlayerRedux>(p => player.HandStrength > 8),
              new Call()
              ),
          //CALL ON BIG BLIND EVEN IF LOW STACK IF NO ONE RAISED
          new Sequence<PokerPlayerRedux>(
              new IsBigBlind(),
              new Call()
              ),
          new Fold()
          ));
        preflop_FCR_Tree.Update(player);
    }
    public void Minnie_FCR (PokerPlayerRedux player)
    {
        FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
               //BLUFF
               new Sequence<PokerPlayerRedux>(
                   new HasABadHand(),
                   new HasEnoughMoney(),
                   new BetIsZero(),
                   new IsInPosition(),
                   new Raise()
                   ),
               //CONTINUATION
               new Sequence<PokerPlayerRedux>(
                   new BetPreFlop(),
                   new Condition<PokerPlayerRedux>(context => player.Hand.HandValues.PokerHand <= PokerHand.OnePair),
                   new BetIsZero(),
                   new HasEnoughMoney(),
                   new Raise()
                   ),
               //SLOW PLAY
               new Sequence<PokerPlayerRedux>(
                   new HasAGreathand(),
                   new Condition<PokerPlayerRedux>(context => Services.Dealer.GetActivePlayerCount() <= 3),
                   new BeforeRiver(),
                   new Selector<PokerPlayerRedux>(
                       new Sequence<PokerPlayerRedux>(
                           new BetIsZero(),
                           new Raise()
                           ),
                       new Sequence<PokerPlayerRedux>(
                           new Not<PokerPlayerRedux>(new BetIsZero()),
                           new Call()
                           )
                       )
                   ),
               //POSITION PLAY
               new Sequence<PokerPlayerRedux>(
                   new IsInPosition(),
                   new Selector<PokerPlayerRedux>(
                       new Sequence<PokerPlayerRedux>(
                           new BetIsZero(),
                           new Raise()
                       ),
                       new Sequence<PokerPlayerRedux>(
                           new Not<PokerPlayerRedux>(new BetIsZero()),
                           new HasAGoodHand(),
                           new Call()
                       )
                   )
               ),
               //CALL
               new Sequence<PokerPlayerRedux>(
                   new HasEnoughMoney(),
                   new HasAGoodHand(),
                   new Not<PokerPlayerRedux>(new BetIsZero()),
                   new Call()
               ),
               //RAISE
               new Sequence<PokerPlayerRedux>(
                   new HasEnoughMoney(),
                   new HasAGreathand(),
                   new Raise()
               ),
               //FOLD
               new Selector<PokerPlayerRedux>(
                   new Sequence<PokerPlayerRedux>(
                       new BetIsZero(),
                       new Call()
                       ),
                   new Sequence<PokerPlayerRedux>(
                       new Not<PokerPlayerRedux>(new BetIsZero()),
                       new Fold()
                   )
               ),
               new Fold()
               ));
        FCR_Tree.Update(player);
    }

    public void Nathaniel_Preflop_FCR(PokerPlayerRedux player)
    {
        preflop_FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(

          //PROTECT YOUR STACK
          new Sequence<PokerPlayerRedux>(
              new Not<PokerPlayerRedux>(new HasEnoughMoney()),
              new HasAGreatHand_PreFlop(),
              new AllIn()
              ),
          //CALL MOST ANYTHING WHEN NO ONE RAISED
          new Sequence<PokerPlayerRedux>(
              new HasEnoughMoney(),
              new Not<PokerPlayerRedux>(new HasABadHand_PreFlop()),
              new Not<PokerPlayerRedux>(new SomeoneHasRaised()),
              new Call()
              ),
          //RAISE ON A GOOD HAND
          new Sequence<PokerPlayerRedux>(
              new HasEnoughMoney(),
              new HasAGreatHand_PreFlop(),
              new Not<PokerPlayerRedux>(new RaisedAlready()),
              new Raise()
              ),
          //CALL ON DECENT HAND OR IF SMALL BLIND OR BIG BLIND
          new Sequence<PokerPlayerRedux>(
              new HasEnoughMoney(),
              new Selector<PokerPlayerRedux>(
                  new Sequence<PokerPlayerRedux>(
                      new IsSmallBlind(),
                      new Call()
                      ),
                  new Sequence<PokerPlayerRedux>(
                      new IsBigBlind(),
                      new Call()
                      ),
                  new Sequence<PokerPlayerRedux>(
                      new Not<PokerPlayerRedux>(new HasAGreatHand_PreFlop()),
                      new Not<PokerPlayerRedux>(new HasABadHand_PreFlop()),
                      new Call()
                      )
                  )
              ),
          //SOMEONE RAISED AND YOU DONT HAVE A GREAT HAND
          new Sequence<PokerPlayerRedux>(
              new SomeoneHasRaised(),
              new Condition<PokerPlayerRedux>(p => player.HandStrength > 8),
              new Call()
              ),
          //CALL ON BIG BLIND EVEN IF LOW STACK IF NO ONE RAISED
          new Sequence<PokerPlayerRedux>(
              new IsBigBlind(),
              new Call()
              ),
          new Fold()
          ));
        preflop_FCR_Tree.Update(player);
    }
    public void Nathaniel_FCR(PokerPlayerRedux player)
    {
        FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
               //HAS A PAIR? CALL.
               new Sequence<PokerPlayerRedux>(
                   new Not<PokerPlayerRedux>(new HasABadHand()),
                   new Condition<PokerPlayerRedux>(context => player.Hand.HandValues.PokerHand >= PokerHand.OnePair),
                   new HasEnoughMoney(),
                   new Call()
                   ),
               //NO PAIR BUT GOOD HAND? CALL
               new Sequence<PokerPlayerRedux>(
                   new HasAGoodHand(),
                   new HasEnoughMoney(),
                   new Call()
                   ),
               //BLUFF
               new Sequence<PokerPlayerRedux>(
                   new HasABadHand(),
                   new HasEnoughMoney(),
                   new BetIsZero(),
                   new IsInPosition(),
                   new Raise()
                   ),
               //CONTINUATION
               new Sequence<PokerPlayerRedux>(
                   new BetPreFlop(),
                   new Condition<PokerPlayerRedux>(context => player.Hand.HandValues.PokerHand <= PokerHand.OnePair),
                   new BetIsZero(),
                   new HasEnoughMoney(),
                   new Raise()
                   ),
               //SLOW PLAY
               new Sequence<PokerPlayerRedux>(
                   new HasAGreathand(),
                   new Condition<PokerPlayerRedux>(context => Services.Dealer.GetActivePlayerCount() <= 3),
                   new BeforeRiver(),
                   new Selector<PokerPlayerRedux>(
                       new Sequence<PokerPlayerRedux>(
                           new BetIsZero(),
                           new Raise()
                           ),
                       new Sequence<PokerPlayerRedux>(
                           new Not<PokerPlayerRedux>(new BetIsZero()),
                           new Call()
                           )
                       )
                   ),
               //POSITION PLAY
               new Sequence<PokerPlayerRedux>(
                   new IsInPosition(),
                   new Selector<PokerPlayerRedux>(
                       new Sequence<PokerPlayerRedux>(
                           new BetIsZero(),
                           new Raise()
                       ),
                       new Sequence<PokerPlayerRedux>(
                           new Not<PokerPlayerRedux>(new BetIsZero()),
                           new HasAGoodHand(),
                           new Call()
                       )
                   )
               ),
               //CALL
               new Sequence<PokerPlayerRedux>(
                   new HasEnoughMoney(),
                   new HasAGoodHand(),
                   new Not<PokerPlayerRedux>(new BetIsZero()),
                   new Call()
               ),
               //RAISE
               new Sequence<PokerPlayerRedux>(
                   new HasEnoughMoney(),
                   new HasAGreathand(),
                   new Raise()
               ),
               //FOLD
               new Selector<PokerPlayerRedux>(
                   new Sequence<PokerPlayerRedux>(
                       new BetIsZero(),
                       new Call()
                       ),
                   new Sequence<PokerPlayerRedux>(
                       new Not<PokerPlayerRedux>(new BetIsZero()),
                       new Fold()
                   )
               ),
               new Fold()
               ));
        FCR_Tree.Update(player);
    }

    public void Zombie_Preflop_FCR(PokerPlayerRedux player)
    {
        preflop_FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
        //WILDCARD BITCHES
        //WILDCARD RAISE
        new Sequence<PokerPlayerRedux>(
             new Condition<PokerPlayerRedux>(context => UnityEngine.Random.Range(0, 100) < 15),
             new Raise()
             ),
        //WILDCARD CALL
        new Sequence<PokerPlayerRedux>(
             new Condition<PokerPlayerRedux>(context => UnityEngine.Random.Range(0, 100) < 15),
             new Call()
             ),
        //WILDCARD FOLD
        new Sequence<PokerPlayerRedux>(
             new Condition<PokerPlayerRedux>(context => UnityEngine.Random.Range(0, 100) < 15),
             new Fold()
             ),
          //PROTECT YOUR STACK
          new Sequence<PokerPlayerRedux>(
              new Not<PokerPlayerRedux>(new HasEnoughMoney()),
              new HasAGreatHand_PreFlop(),
              new AllIn()
              ),
          //RAISE ON A GOOD HAND
          new Sequence<PokerPlayerRedux>(
              new HasEnoughMoney(),
              new HasAGreatHand_PreFlop(),
              new Not<PokerPlayerRedux>(new RaisedAlready()),
              new Raise()
              ),
          //CALL ON DECENT HAND OR IF SMALL BLIND OR BIG BLIND
          new Sequence<PokerPlayerRedux>(
              new HasEnoughMoney(),
              new Selector<PokerPlayerRedux>(
                  new Sequence<PokerPlayerRedux>(
                      new IsSmallBlind(),
                      new Call()
                      ),
                  new Sequence<PokerPlayerRedux>(
                      new IsBigBlind(),
                      new Call()
                      ),
                  new Sequence<PokerPlayerRedux>(
                      new Not<PokerPlayerRedux>(new HasAGreatHand_PreFlop()),
                      new Not<PokerPlayerRedux>(new HasABadHand_PreFlop()),
                      new Call()
                      )
                  )
              ),
          //SOMEONE RAISED AND YOU DONT HAVE A GREAT HAND
          new Sequence<PokerPlayerRedux>(
              new SomeoneHasRaised(),
              new Condition<PokerPlayerRedux>(p => player.HandStrength > 8),
              new Call()
              ),
          //CALL ON BIG BLIND EVEN IF LOW STACK IF NO ONE RAISED
          new Sequence<PokerPlayerRedux>(
              new IsBigBlind(),
              new Call()
              ),
          new Fold()
          ));
        preflop_FCR_Tree.Update(player);
    }
    public void Zombie_FCR(PokerPlayerRedux player)
    {
        FCR_Tree = new Tree<PokerPlayerRedux>(new Selector<PokerPlayerRedux>(
               //WILDCARD BITCHES//
               //WILDCARD RAISE
               new Sequence<PokerPlayerRedux>(
                   new Condition<PokerPlayerRedux>(context => UnityEngine.Random.Range(0, 100) < 25),
                   new Raise()
                   ),
               //WILDCARD CALL
               new Sequence<PokerPlayerRedux>(
                   new Condition<PokerPlayerRedux>(context => UnityEngine.Random.Range(0, 100) < 25),
                   new Call()
                   ),
               //WILDCARD FOLD
               new Sequence<PokerPlayerRedux>(
                   new Condition<PokerPlayerRedux>(context => UnityEngine.Random.Range(0, 100) < 25),
                   new Fold()
                   ),
               //BLUFF
               new Sequence<PokerPlayerRedux>(
                   new HasABadHand(),
                   new HasEnoughMoney(),
                   new BetIsZero(),
                   new IsInPosition(),
                   new Raise()
                   ),
               //CONTINUATION
               new Sequence<PokerPlayerRedux>(
                   new BetPreFlop(),
                   new Condition<PokerPlayerRedux>(context => player.Hand.HandValues.PokerHand <= PokerHand.OnePair),
                   new BetIsZero(),
                   new HasEnoughMoney(),
                   new Raise()
                   ),
               //SLOW PLAY
               new Sequence<PokerPlayerRedux>(
                   new HasAGreathand(),
                   new Condition<PokerPlayerRedux>(context => Services.Dealer.GetActivePlayerCount() <= 3),
                   new BeforeRiver(),
                   new Selector<PokerPlayerRedux>(
                       new Sequence<PokerPlayerRedux>(
                           new BetIsZero(),
                           new Raise()
                           ),
                       new Sequence<PokerPlayerRedux>(
                           new Not<PokerPlayerRedux>(new BetIsZero()),
                           new Call()
                           )
                       )
                   ),
               //POSITION PLAY
               new Sequence<PokerPlayerRedux>(
                   new IsInPosition(),
                   new Selector<PokerPlayerRedux>(
                       new Sequence<PokerPlayerRedux>(
                           new BetIsZero(),
                           new Raise()
                       ),
                       new Sequence<PokerPlayerRedux>(
                           new Not<PokerPlayerRedux>(new BetIsZero()),
                           new HasAGoodHand(),
                           new Call()
                       )
                   )
               ),
               //CALL
               new Sequence<PokerPlayerRedux>(
                   new HasEnoughMoney(),
                   new HasAGoodHand(),
                   new Not<PokerPlayerRedux>(new BetIsZero()),
                   new Call()
               ),
               //RAISE
               new Sequence<PokerPlayerRedux>(
                   new HasEnoughMoney(),
                   new HasAGreathand(),
                   new Raise()
               ),
               //FOLD
               new Selector<PokerPlayerRedux>(
                   new Sequence<PokerPlayerRedux>(
                       new BetIsZero(),
                       new Call()
                       ),
                   new Sequence<PokerPlayerRedux>(
                       new Not<PokerPlayerRedux>(new BetIsZero()),
                       new Fold()
                   )
               ),
               new Fold()
               ));
        FCR_Tree.Update(player);
    }

    ///////////////////////////////
    ///////////NODES///////////////
    ///////////////////////////////

    /////////CONDITIONS///////////
    private class IsOnALoseStreak : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return player.lossCount > 5;
        }
    }

    private class LostManyHandsToOpponent : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            if(Services.Dealer.GetActivePlayerCount() == 2)
            {
                for (int i = 0; i < Services.Dealer.players.Count; i++)
                {
                    if (Services.Dealer.players[i] != player && Services.Dealer.players[i].PlayerState == PlayerState.Playing)
                    {
                        if (player.playersLostAgainst[Services.Dealer.players[i].SeatPos] > 10) return true;
                    }
                }
            }
            return false;
        }
    }

    private class HasMoreMoneyThanOpponent : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            if(Services.Dealer.GetActivePlayerCount() == 2)
            {
                for (int i = 0; i < Services.Dealer.players.Count; i++)
                {
                    if(Services.Dealer.players[i] != player && Services.Dealer.players[i].PlayerState == PlayerState.Playing)
                    {
                        if (player.chipCount > Services.Dealer.players[i].chipCount) return true;
                    }
                }
            }
            return false;
        }
    }

    private class IsChipLeader : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if (player != Services.Dealer.players[i] && player.chipCount < Services.Dealer.players[i].chipCount) return false;
            }
            return true;
        }
    }

    private class IsBigBlind : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return Services.Dealer.LastBet - player.currentBet == 0;
        }
    }

    private class IsSmallBlind : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return Services.Dealer.LastBet - player.currentBet == Services.Dealer.SmallBlind;
        }
    }

    private class HasEnoughMoney : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return (player.chipCount - Services.Dealer.LastBet) > Services.Dealer.BigBlind * 4;
        }
    }

    private class HasABadHand_PreFlop : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return player.HandStrength <= 4;
        }
    }

    private class HasAGreatHand_PreFlop : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return player.HandStrength >= 12;
        }
    }

    private class HasADecentHand_Preflop : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return player.HandStrength > 4 && player.HandStrength < 12;
        }
    }

    private class HasABadHand : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            List<PokerPlayerRedux> rankedPlayers = Services.PlayerBehaviour.RankedPlayerHands(player);
            if(rankedPlayers.Count >= Services.Dealer.GetActivePlayerCount() / 2)
            {
                return player != rankedPlayers[0];
            }
            Debug.Log(player.playerName + " has a HS of " + player.HandStrength);
            return player.HandStrength < .2f;
        }
    }

    private class HasAGoodHand : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            List<PokerPlayerRedux> rankedPlayers = Services.PlayerBehaviour.RankedPlayerHands(player);
            if (rankedPlayers.Count >= Services.Dealer.GetActivePlayerCount() / 2)
            {
                return player != rankedPlayers[0] && player.HandStrength > .4f;
            }
            Debug.Log(player.playerName + " has a HS of " + player.HandStrength);
            return player.HandStrength > .5f;
        }
    }

    private class HasAGreathand : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            List<PokerPlayerRedux> rankedPlayers = Services.PlayerBehaviour.RankedPlayerHands(player);
            if (rankedPlayers.Count >= Services.Dealer.GetActivePlayerCount() / 2)
            {
                return player == rankedPlayers[0];
            }
            Debug.Log(player.playerName + " has a HS of " + player.HandStrength);
            return player.HandStrength > .75f;
        }
    }

    private class IsInPosition : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            bool inPosition;
            if (Services.Dealer.PlayerSeatsAwayFromDealerAmongstLivePlayers(Services.Dealer.GetActivePlayerCount() - 1) == player || 
                Services.Dealer.players[Table.instance.DealerPosition] == player)
            {
                Debug.Log(player.playerName + " is in position");
                inPosition = true;
            }
            else inPosition = false;
            return inPosition;
        }
    }

    private class BeforeRiver : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return Table.gameState < GameState.River;
        }
    }

    private class BetIsZero : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return Services.Dealer.LastBet == 0;
        }
    }

    private class BetPreFlop : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return Table.gameState == GameState.Flop && player.lastAction == PlayerAction.Raise;
        }
    }

    private class RaisedAlready : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            return player.lastAction == PlayerAction.Raise;
        }
    }

    private class SomeoneHasRaised : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            foreach (PokerPlayerRedux p in Services.Dealer.players)
            {
                if (p != player && p.lastAction == PlayerAction.Raise) return true;
            }
            return false;
        }
    }

    //////////ACTIONS//////////
    private class Fold : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            player.Fold();
            player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }

    private class Call : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            player.Call();
            player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }

    private class Raise : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            player.Raise();
            player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }

    private class AllIn : Node<PokerPlayerRedux>
    {
        public override bool Update(PokerPlayerRedux player)
        {
            player.AllIn();
            player.turnComplete = true;
            player.actedThisRound = true;
            return true;
        }
    }
}
