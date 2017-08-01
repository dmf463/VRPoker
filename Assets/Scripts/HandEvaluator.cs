using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PokerHand { Connectors, SuitedConnectors, HighCard, OnePair, TwoPair, ThreeOfKind, Straight, Flush, FullHouse, FourOfKind, StraightFlush }


public struct HandValue
{
    public int Total { get;  set;}
    public int HighCard { get; set; }
    public PokerHand PokerHand { get; set; }
    public static bool operator == (HandValue hv1, HandValue hv2) { return hv1.Equals(hv2); }
    public static bool operator != (HandValue hv1, HandValue hv2) { return !hv1.Equals(hv2); }
}

public class HandEvaluator {

    private int spadeSum;
    private int heartSum;
    private int diamondSum;
    private int clubSum;
    private int straightCount;
    private int straightFlushCount;
    private List<CardType> incomingCards;
    private HandValue handValue;

    public HandEvaluator(List<CardType> sortedCards)
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        straightCount = 0;
        straightFlushCount = 0;
        incomingCards = sortedCards;
        Cards = sortedCards;
    }

    //TEST SHIT
    public HandEvaluator()
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        straightCount = 0;
        straightFlushCount = 0;
    }

    public void SetHandEvalutor(List<CardType> sortedCards)
    {
        incomingCards = sortedCards;
        Cards = sortedCards;
    }
    public void ResetHandEvaluator()
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        straightCount = 0;
        straightFlushCount = 0;
    }
    //END TEST SHIT

    public HandValue HandValues
    {
        get { return handValue; }
        set { handValue = value; }
    }

    public List<CardType> Cards
    {
        get { return incomingCards; }
        set
        {
            GameState _gameState = Table.gameState;
            if(_gameState == GameState.PreFlop)
            {
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
            }

            else if(_gameState == GameState.Flop)
            {
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
                incomingCards[2] = value[2];
                incomingCards[3] = value[3];
                incomingCards[4] = value[4];
            }

            else if (_gameState == GameState.Turn)
            {
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
                incomingCards[2] = value[2];
                incomingCards[3] = value[3];
                incomingCards[4] = value[4];
                incomingCards[5] = value[5];
            }

            else if(_gameState == GameState.River || _gameState == GameState.ShowDown)
            {
                incomingCards[0] = value[0];
                incomingCards[1] = value[1];
                incomingCards[2] = value[2];
                incomingCards[3] = value[3];
                incomingCards[4] = value[4];
                incomingCards[5] = value[5];
                incomingCards[6] = value[6];
            }
        }
    }

    public PokerHand EvaluateHandAtPreFlop()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (PocketPair())
        {
            return PokerHand.OnePair;
        }
        else if (SuitedConnectors())
        {
            return PokerHand.SuitedConnectors;
        }
        else if (Connectors())
        {
            return PokerHand.Connectors;
        }
        handValue.HighCard = (int)incomingCards[1].rank;
        handValue.Total = (int)incomingCards[0].rank + (int)incomingCards[1].rank;
        handValue.PokerHand = PokerHand.HighCard;
        return PokerHand.HighCard;
    }

    public PokerHand EvaluateHandAtFlop()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (StraightFlushAtFlop())
        {
            return PokerHand.StraightFlush;
        }
        else if (FourOfKindAtFlop())
        {
            return PokerHand.FourOfKind;
        }
        else if (FullHouseAtFlop())
        {
            return PokerHand.FullHouse;
        }
        else if (FlushAtFlop())
        {
            return PokerHand.Flush;
        }
        else if (StraightAtFlop())
        {
            return PokerHand.Straight;
        }
        else if (ThreeOfKindAtFlop())
        {
            return PokerHand.ThreeOfKind;
        }
        else if (TwoPairAtFlop())
        {
            return PokerHand.TwoPair;
        }
        else if (OnePairAtFlop())
        {
            return PokerHand.OnePair;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard = (int)incomingCards[4].rank;
        handValue.PokerHand = PokerHand.HighCard;
        return PokerHand.HighCard;
    }

    public PokerHand EvaluateHandAtTurn()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (StraightFlushAtTurn())
        {
            return PokerHand.StraightFlush;
        }
        else if (FourOfKindAtTurn())
        {
            return PokerHand.FourOfKind;
        }
        else if (FullHouseAtTurn())
        {
            return PokerHand.FullHouse;
        }
        else if (FlushAtTurn())
        {
            return PokerHand.Flush;
        }
        else if (StraightAtTurn())
        {
            return PokerHand.Straight;
        }
        else if (ThreeOfKindAtTurn())
        {
            return PokerHand.ThreeOfKind;
        }
        else if (TwoPairAtTurn())
        {
            return PokerHand.TwoPair;
        }
        else if (OnePairAtTurn())
        {
            return PokerHand.OnePair;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard = (int)incomingCards[5].rank;
        handValue.PokerHand = PokerHand.HighCard;
        return PokerHand.HighCard;
    }

    public PokerHand EvaluateHandAtRiver()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (StraightFlushAtRiver())
        {
            return PokerHand.StraightFlush;
        }
        else if (FourOfKindAtRiver())
        {
            return PokerHand.FourOfKind;
        }
        else if (FullHouseRiver())
        {
            return PokerHand.FullHouse;
        }
        else if (FlushAtRiver())
        {
            return PokerHand.Flush;
        }
        else if (StraightAtRiver())
        {
            return PokerHand.Straight;
        }
        else if (ThreeOfKindAtRiver())
        {
            return PokerHand.ThreeOfKind;
        }
        else if (TwoPairRiver())
        {
            return PokerHand.TwoPair;
        }
        else if (OnePairAtRiver())
        {
            return PokerHand.OnePair;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard = (int)incomingCards[6].rank;
        handValue.PokerHand = PokerHand.HighCard;
        return PokerHand.HighCard;

    }

    private void getNumberOfSuit()
    {
        foreach (CardType card in Cards)
        {
            if(card.suit == SuitType.Spades)
            {
                spadeSum++;
            }
            else if(card.suit == SuitType.Hearts)
            {
                heartSum++;
            }
            else if(card.suit == SuitType.Diamonds)
            {
                diamondSum++;
            }
            else if(card.suit == SuitType.Clubs)
            {
                clubSum++;
            }
        }
    }

 /*
 * 
 * Everything below here is the logic for PRE-FLOP hand evaluation 
 * 
 */

    public bool PocketPair()
    {
        //if your preflop cards are the same, you have a pocket pair
        if(incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 2;
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        return false;
    }

    public bool SuitedConnectors()
    {
        //if you're cards fall into a sequence and have the same suit, then you have suitedConnectors
        if(incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit)
        {
            handValue.Total = (int)incomingCards[0].rank + (int)incomingCards[1].rank;
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.SuitedConnectors;
            return true;
        }
        return false;
    }

    public bool Connectors()
    {
        //if you're cards fall into a sequence, then you have Connectors
        if (incomingCards[0].rank + 1 == incomingCards[1].rank)
        {
            handValue.Total = (int)incomingCards[0].rank + (int)incomingCards[1].rank;
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.Connectors;
            return true;
        }
        return false;
    }


/*
* 
* Everything below here is the logic for FLOP hand evaluation 
* 
*/


    public bool OnePairAtFlop()
    {
        //if there are two cards that are the same, it's a pair
        //0, 1 with 4 high card
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + (int)incomingCards[2].rank + (int)incomingCards[3].rank + (int)incomingCards[4].rank;
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //1, 2 with 4 high card
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + (int)incomingCards[0].rank + (int)incomingCards[3].rank + (int)incomingCards[4].rank;
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //2, 3 with 4 high card
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + (int)incomingCards[0].rank + (int)incomingCards[1].rank + (int)incomingCards[4].rank;
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //3, 4 with 2 high card
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[4].rank * 2) + (int)incomingCards[0].rank + (int)incomingCards[1].rank + (int)incomingCards[2].rank;
            handValue.HighCard = (int)incomingCards[2].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        return false;
    }

    public bool TwoPairAtFlop()
    {
        //if there are two pairs of cards that are the same then it's two pair

        //1, 2 && 3, 4
        if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[0].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 3, 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[2].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 2, 3
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[2].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        return false;
    }

    public bool ThreeOfKindAtFlop()
    {
        //if 3 cards are the same then it's a three of a kind
        //0, 1, 2
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 3;
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //1, 2, 3
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 3;
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //2, 3, 4
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 3;
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }

        return false;
    }

    public bool StraightAtFlop()
    {
        if( incomingCards[0].rank + 1 == incomingCards[1].rank &&
            incomingCards[1].rank + 1 == incomingCards[2].rank &&
            incomingCards[2].rank + 1 == incomingCards[3].rank &&
            incomingCards[3].rank + 1 == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.Straight;
            return true;
        }
        if (incomingCards[0].rank == RankType.Two &&
            incomingCards[1].rank == RankType.Three &&
            incomingCards[2].rank == RankType.Four &&
            incomingCards[3].rank == RankType.Five &&
            incomingCards[4].rank == RankType.Ace)
        {
            handValue.Total = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.Straight;
            return true;
        }
        return false;
    }

    public bool FlushAtFlop()
    {
        if (spadeSum == 5 || heartSum == 5 || diamondSum == 5 || clubSum == 5)
        {
            handValue.Total = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.Flush;
            return true;
        }
        return false;
    }

    public bool FullHouseAtFlop()
    {
        //if there is a pair and trips , it's a full house

        //0 = 1, 2 = 3 = 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        return false;
    }

    public bool FourOfKindAtFlop()
    {
        //if any 4 ordered cards are the same, it's a 4 of a kind
        //0, 1, 2, 3
        //1, 2, 3, 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[0].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        return false;

    }

    public bool StraightFlushAtFlop()
    {
        if (incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit &&
            incomingCards[1].rank + 1 == incomingCards[2].rank && incomingCards[1].suit == incomingCards[2].suit &&
            incomingCards[2].rank + 1 == incomingCards[3].rank && incomingCards[2].suit == incomingCards[3].suit &&
            incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
        {
            handValue.Total = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.StraightFlush;
            return true;
        }
        if (incomingCards[0].rank == RankType.Two && incomingCards[0].suit == incomingCards[1].suit &&
            incomingCards[1].rank == RankType.Three && incomingCards[1].suit == incomingCards[2].suit &&
            incomingCards[2].rank == RankType.Four && incomingCards[2].suit == incomingCards[3].suit &&
            incomingCards[3].rank == RankType.Five && incomingCards[3].suit == incomingCards[4].suit &&
            incomingCards[4].rank == RankType.Ace)
        {
            handValue.Total = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.StraightFlush;
            return true;
        }
        return false;
    }


/*
* 
* Everything below here is the logic for TURN hand evaluation 
* 
*/

    public bool OnePairAtTurn()
    {
        //if there are two cards that are the same, it's a pair
        //0, 1 with 5 high card
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + (int)incomingCards[3].rank + (int)incomingCards[4].rank + (int)incomingCards[5].rank;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //1, 2 with 5 high card
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + (int)incomingCards[3].rank + (int)incomingCards[4].rank + (int)incomingCards[5].rank;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //2, 3 with 5 high card
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + (int)incomingCards[4].rank + (int)incomingCards[5].rank + (int)incomingCards[1].rank;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //3, 4 with 5 high card
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[3].rank * 2) + (int)incomingCards[4].rank + (int)incomingCards[5].rank + (int)incomingCards[2].rank;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //4, 5 with 3 high card
        else if (incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[4].rank * 2) + (int)incomingCards[3].rank + (int)incomingCards[2].rank + (int)incomingCards[1].rank;
            handValue.HighCard = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        return false;

    }

    public bool TwoPairAtTurn()
    {
        //if there are two pairs of cards that are the same then it's two pair

        //2, 3 && 4, 5
        if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 4, 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 4, 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 3, 4
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 3, 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 2, 3
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[2].rank * 2);
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        return false;
    }

    public bool ThreeOfKindAtTurn()
    {
        //if 3 cards are the same then it's a three of a kind
        //0, 1, 2
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 3;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //1, 2, 3
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 3;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //2, 3, 4
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 3;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //3, 4, 5
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[5].rank * 3;
            handValue.HighCard = (int)incomingCards[2].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        return false;
    }

    public bool StraightAtTurn()
    {
        //need to check the straight, but because of the possibility of pairs in the middle of the straight I need to check each sequence
        //if the cards rank is equal to the following card + 1, then they are connectors and the "straight count" goes up
        //if the straight count reachs 4, that means there were at least 4 instances of connectors and there MUST be a straight
        //to check the high card, I do the same thing as the flush
        //if 4, 5, and 6 are consecutive, 6 is the high card
        //if 4 and 6 are consecutive, then 6 is the high card
        // if 5 and 6 are consecutive, then 6 is the high card
        //if 4 and 5 are consectuive, then 5 is the high card,
        //if neither 4, 5, or 6 are consecutive then ONE of them MUST be consecutive to card 3, so I check that
        //EDGE CASE, THE WHEEL: A, 2, 3, 4, 5. A = 1 and RANK5 is the high card, because we don't know where the 5 is, but we know it's the highcard


        if (incomingCards[0].rank + 1 == incomingCards[1].rank)
        {
            straightCount++;
        }
        else if (incomingCards[0].rank == incomingCards[1].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[1].rank + 1 == incomingCards[2].rank)
        {
            straightCount++;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[2].rank + 1 == incomingCards[3].rank)
        {
            straightCount++;
        }
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[3].rank + 1 == incomingCards[4].rank)
        {
            straightCount++;
        }
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[4].rank + 1 == incomingCards[5].rank)
        {
            straightCount++;
        }
        else if (incomingCards[4].rank == incomingCards[5].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        //NEED TO MAKE A FRINGE CASE FOR CHECK FOR THE WHEEL (A, 2, 3, 4, 5)
        //will be ordered with as (2..3..4..5..A) with two cards inserted at any other point, but the order would make the straigh count = 0
        //so need to run a check AFTER the straight count has been recalculated to see if it's THE WHEEL

        if(straightCount == 0)
        {
            int countForTheWheel = 0;
            foreach(CardType card in incomingCards)
            {
                switch (card.rank)
                {
                    case RankType.Two:
                        countForTheWheel++;
                        break;
                    case RankType.Three:
                        countForTheWheel++;
                        break;
                    case RankType.Four:
                        countForTheWheel++;
                        break;
                    case RankType.Five:
                        countForTheWheel++;
                        break;
                    case RankType.Ace:
                        countForTheWheel++;
                        break;
                    default:
                        break;
                }
            }
            if(countForTheWheel == 5)
            {
                handValue.Total = 5;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else return false;
        }

        if (straightCount >= 4)
        {
           if(incomingCards[4].rank + 1 == incomingCards[5].rank)
            {
                handValue.Total = (int)incomingCards[5].rank;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else
            {
                handValue.Total = (int)incomingCards[4].rank;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
        }
        return false;
    }

    public bool FlushAtTurn()
    {
        if (spadeSum >= 5 || heartSum >= 5 || diamondSum >= 5 || clubSum >= 5)
        {
            if (incomingCards[4].suit == incomingCards[5].suit)
            {
                handValue.Total = (int)incomingCards[5].rank;
                handValue.PokerHand = PokerHand.Flush;
                return true;
            }
            else
            {
                handValue.Total = (int)incomingCards[4].rank;
                handValue.PokerHand = PokerHand.Flush;
                return true;
            }
        }
        return false;
    }

    public bool FullHouseAtTurn()
    {
        //if there is a pair and trips within the 7 cards, it's a full house

        //0 = 1, 2 = 3 = 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1, 3 = 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2, 3 = 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2 = 3, 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        return false;
    }

    public bool FourOfKindAtTurn()
    {
        //if any 4 ordered cards are the same, it's a 4 of a kind
        //0, 1, 2, 3
        //1, 2, 3, 4
        //2, 3, 4, 5
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[0].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 4;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 4;
            handValue.HighCard = (int)incomingCards[5].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank && incomingCards[2].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 4;
            handValue.HighCard = (int)incomingCards[1].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        return false;
    }

    public bool StraightFlushAtTurn()
    {
        //need to check the straight AND the flush, but I can't just check for a straight and a flush.
        //instead, I'm taking the Straight() and just adding to check if the connecting cards are also suited.
        //if the count reaches 4, then hey, you've got yourself a straight flush!

        //need to check the straight, but because of the possibility of pairs in the middle of the straight I need to check each sequence
        //if the cards rank is equal to the following card + 1, then they are connectors and the "straight count" goes up
        //if the straight count reachs 4, that means there were at least 4 instances of connectors and there MUST be a straight
        //to check the high card, I do the same thing as the flush
        //if 4, 5, and 6 are consecutive, 6 is the high card
        //if 4 and 6 are consecutive, then 6 is the high card
        // if 5 and 6 are consecutive, then 6 is the high card
        //if 4 and 5 are consectuive, then 5 is the high card,
        //if neither 4, 5, or 6 are consecutive then ONE of them MUST be consecutive to card 3, so I check that
        if (incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[1].rank + 1 == incomingCards[2].rank && incomingCards[1].suit == incomingCards[2].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[2].rank + 1 == incomingCards[3].rank && incomingCards[2].suit == incomingCards[3].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[4].suit == incomingCards[5].suit)
        {
            straightFlushCount++;
        }

        //FRINGE CASE FOR THE WHEEL
        if (straightFlushCount == 0)
        {
            if (incomingCards[0].rank == RankType.Two && incomingCards[5].rank == RankType.Ace && incomingCards[0].suit == incomingCards[5].suit)
            {
                if (incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[1].rank + 1 == incomingCards[2].rank && incomingCards[1].suit == incomingCards[2].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[2].rank + 1 == incomingCards[3].rank && incomingCards[2].suit == incomingCards[3].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[4].suit == incomingCards[5].suit)
                {
                    straightFlushCount++;
                }
            }
        }

        if (incomingCards[0].rank == RankType.Two && incomingCards[5].rank == RankType.Ace && incomingCards[0].suit == incomingCards[5].suit)
        {
            if (straightFlushCount == 3)
            {
                handValue.Total = 5;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
        }

        if (straightFlushCount >= 4)
        {
            if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[4].suit == incomingCards[5].suit)
            {
                handValue.Total = (int)incomingCards[5].rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
            else
            {
                handValue.Total = (int)incomingCards[4].rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
        }
        return false;
    }


/*
* 
* Everything below here is the logic for RIVER and ShowDown hand evaluation 
* 
*/

    public bool OnePairAtRiver()
    {
        //if there are two cards that are the same, it's a pair
        //0, 1 with 6 high card
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + (int)incomingCards[6].rank + (int)incomingCards[5].rank + (int)incomingCards[4].rank;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //1, 2 with 6 high card
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + (int)incomingCards[6].rank + (int)incomingCards[5].rank + (int)incomingCards[4].rank;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //2, 3 with 6 high card
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + (int)incomingCards[6].rank + (int)incomingCards[5].rank + (int)incomingCards[4].rank;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //3, 4 with 6 high card
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[3].rank * 2) + (int)incomingCards[6].rank + (int)incomingCards[5].rank + (int)incomingCards[2].rank;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //4, 5 with 6 high card
        else if (incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[4].rank * 2) + (int)incomingCards[6].rank + (int)incomingCards[3].rank + (int)incomingCards[2].rank;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        //5, 6 with 4 high card
        else if (incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[5].rank * 2) + (int)incomingCards[4].rank + (int)incomingCards[3].rank + (int)incomingCards[2].rank;
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.OnePair;
            return true;
        }
        return false;
    }

    public bool TwoPairRiver()
    {
        //if there are two pairs of cards that are the same then it's two pair
        //currently have an issue where the ordered hands can actually have THREE pairs
        //for example, 4, 4, 9, 9, 10, Q, Q
        //system evaluates it as the hand being 4's and 9's when it SHOULD be 9's and Q's
        //solution: put the if else tree in the opposite order, so it finds the highest hand last

        //3, 4 && 5, 6
        if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[3].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[2].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //2, 3 && 5, 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 5, 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 5, 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //2, 3 && 4, 5
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 4, 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 4, 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //1, 2 && 3, 4
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 3, 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        //0, 1 && 2, 3
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[2].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.TwoPair;
            return true;
        }
        return false;
    }

    public bool ThreeOfKindAtRiver()
    {
        //if 3 cards are the same then it's a three of a kind
        //0, 1, 2
        if(incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //1, 2, 3
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //2, 3, 4
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //3, 4, 5
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[5].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        //4, 5, 6
        else if (incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[4].rank * 3;
            handValue.HighCard = (int)incomingCards[3].rank;
            handValue.PokerHand = PokerHand.ThreeOfKind;
            return true;
        }
        return false;
    }

    public bool StraightAtRiver()
    {
        //need to check the straight, but because of the possibility of pairs in the middle of the straight I need to check each sequence
        //if the cards rank is equal to the following card + 1, then they are connectors and the "straight count" goes up
        //if the straight count reachs 4, that means there were at least 4 instances of connectors and there MUST be a straight
        //to check the high card, I do the same thing as the flush
        //if 4, 5, and 6 are consecutive, 6 is the high card
        //if 4 and 6 are consecutive, then 6 is the high card
        // if 5 and 6 are consecutive, then 6 is the high card
        //if 4 and 5 are consectuive, then 5 is the high card,
        //if neither 4, 5, or 6 are consecutive then ONE of them MUST be consecutive to card 3, so I check that
        //EDGE CASE, THE WHEEL: A, 2, 3, 4, 5. A = 1 and RANK5 is the high card, because we don't know where the 5 is, but we know it's the highcard


        if (incomingCards[0].rank + 1 == incomingCards[1].rank)
        {
            straightCount++;
        }
        else if (incomingCards[0].rank == incomingCards[1].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;
        
        if (incomingCards[1].rank + 1 == incomingCards[2].rank)
        {
            straightCount++;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[2].rank + 1 == incomingCards[3].rank)
        {
            straightCount++;
        }
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[3].rank + 1 == incomingCards[4].rank)
        {
            straightCount++;
        }
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[4].rank + 1 == incomingCards[5].rank)
        {
            straightCount++;
        }
        else if (incomingCards[4].rank == incomingCards[5].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        if (incomingCards[5].rank + 1 == incomingCards[6].rank)
        {
            straightCount++;
        }
        else if (incomingCards[5].rank == incomingCards[6].rank)
        {
            straightCount += 0;
        }
        else straightCount = 0;

        //NEED TO MAKE A FRINGE CASE FOR CHECK FOR THE WHEEL (A, 2, 3, 4, 5)
        //will be ordered with as (2..3..4..5..A) with two cards inserted at any other point, but the order would make the straigh count = 0
        //so need to run a check AFTER the straight count has been recalculated to see if it's THE WHEEL


        if (straightCount == 0)
        {
            int countForTheWheel = 0;
            foreach (CardType card in incomingCards)
            {
                switch (card.rank)
                {
                    case RankType.Two:
                        countForTheWheel++;
                        break;
                    case RankType.Three:
                        countForTheWheel++;
                        break;
                    case RankType.Four:
                        countForTheWheel++;
                        break;
                    case RankType.Five:
                        countForTheWheel++;
                        break;
                    case RankType.Ace:
                        countForTheWheel++;
                        break;
                    default:
                        break;
                }
            }
            if (countForTheWheel == 5)
            {
                handValue.Total = 5;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else return false;
        }

        if (straightCount >= 4)
        {
            if(incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else if(incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[5].rank;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[4].rank + 1 == incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.Straight;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank && incomingCards[4].rank + 1 != incomingCards[6].rank)
            {
                if(incomingCards[3].rank + 1 == incomingCards[4].rank)
                {
                    handValue.Total = (int)incomingCards[4].rank;
                    handValue.PokerHand = PokerHand.Straight;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[5].rank)
                {
                    handValue.Total = (int)incomingCards[5].rank;
                    handValue.PokerHand = PokerHand.Straight;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[6].rank)
                {
                    handValue.Total = (int)incomingCards[6].rank;
                    handValue.PokerHand = PokerHand.Straight;
                    return true;
                }
            }
        }
        return false;
    }

    public bool FlushAtRiver()
    {
        //if there are at least 5 of the same suited cards, then the player has a flush
        //since they are ordered by rank, cards 4, 5, or 6 MUST be in the hand, but whether those cards are PART of the flush I need to check
        //so if 4, 5, and 6 are all the same suit, then 6 is the high card
        //if 4 and 5 are the same suit and 6 is not, then 5 is the high card
        //if 5 and 6 are the same suit and 4 is not, then 6 is the high card
        //if 4 and 6 are the same suit and 5 is not, then 6 is the high card
        //if neither 4, 5, or 6 are the same suit, then ONE of them must be the high card
        //therefore, if one of those three cards has a suit equal to card THREE, then that card is the high card.
        if(spadeSum >= 5 || heartSum >= 5 || diamondSum >= 5 || clubSum >= 5)
        {
            if (incomingCards[4].suit == incomingCards[5].suit && incomingCards[4].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.Flush;
                return true;
            }
            else if (incomingCards[4].suit == incomingCards[5].suit && incomingCards[4].suit != incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[5].rank;
                handValue.PokerHand = PokerHand.Flush;
                return true;
            }
            else if (incomingCards[4].suit != incomingCards[5].suit && incomingCards[5].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.Flush;
                return true;
            }
            else if (incomingCards[4].suit != incomingCards[5].suit && incomingCards[4].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.Flush;
                return true;
            }
            else if(incomingCards[4].suit != incomingCards[5].suit && incomingCards[4].suit != incomingCards[6].suit && incomingCards[5].suit != incomingCards[6].suit)
            {
                if(incomingCards[0].suit == incomingCards[4].suit)
                {
                    handValue.Total = (int)incomingCards[4].rank;
                    handValue.PokerHand = PokerHand.Flush;
                    return true;
                }
                else if (incomingCards[0].suit == incomingCards[5].suit)
                {
                    handValue.Total = (int)incomingCards[5].rank;
                    handValue.PokerHand = PokerHand.Flush;
                    return true;
                }
                else if (incomingCards[0].suit == incomingCards[6].suit)
                {
                    handValue.Total = (int)incomingCards[6].rank;
                    handValue.PokerHand = PokerHand.Flush;
                    return true;
                }
            }

        }
        return false;
    }

    public bool FullHouseRiver()
    {
        //if there is a pair and trips within the 7 cards, it's a full house

        //0 = 1, 2 = 3 = 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1, 3 = 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1, 4 = 5 = 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2, 3 = 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2, 4 = 5 = 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //2 = 3, 4 =  5 = 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //0 = 1 = 2, 5 = 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2 = 3, 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //1 = 2 = 3, 5 = 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        //2 = 3 = 4, 5 = 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            handValue.PokerHand = PokerHand.FullHouse;
            return true;
        }
        return false;
    }

    public bool FourOfKindAtRiver()
    {
        //if any 4 ordered cards are the same, it's a 4 of a kind
        //0, 1, 2, 3
        //1, 2, 3, 4
        //2, 3, 4, 5
        //3, 4, 5, 6
        if(incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[0].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank && incomingCards[2].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank && incomingCards[3].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[3].rank * 4;
            handValue.HighCard = (int)incomingCards[2].rank;
            handValue.PokerHand = PokerHand.FourOfKind;
            return true;
        }
        return false;
    }

    public bool StraightFlushAtRiver()
    {
        //need to check the straight AND the flush, but I can't just check for a straight and a flush.
        //instead, I'm taking the Straight() and just adding to check if the connecting cards are also suited.
        //if the count reaches 4, then hey, you've got yourself a straight flush!

        //need to check the straight, but because of the possibility of pairs in the middle of the straight I need to check each sequence
        //if the cards rank is equal to the following card + 1, then they are connectors and the "straight count" goes up
        //if the straight count reachs 4, that means there were at least 4 instances of connectors and there MUST be a straight
        //to check the high card, I do the same thing as the flush
        //if 4, 5, and 6 are consecutive, 6 is the high card
        //if 4 and 6 are consecutive, then 6 is the high card
        // if 5 and 6 are consecutive, then 6 is the high card
        //if 4 and 5 are consectuive, then 5 is the high card,
        //if neither 4, 5, or 6 are consecutive then ONE of them MUST be consecutive to card 3, so I check that
        if (incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[1].rank + 1 == incomingCards[2].rank && incomingCards[1].suit == incomingCards[2].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[2].rank + 1 == incomingCards[3].rank && incomingCards[2].suit == incomingCards[3].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[4].suit == incomingCards[5].suit)
        {
            straightFlushCount++;
        }
        if (incomingCards[5].rank + 1 == incomingCards[6].rank && incomingCards[5].suit == incomingCards[6].suit)
        {
            straightFlushCount++;
        }

        //FRINGE CASE FOR THE WHEEL
        if (straightFlushCount == 0)
        {
            if (incomingCards[0].rank == RankType.Two && incomingCards[6].rank == RankType.Ace && incomingCards[0].suit == incomingCards[6].suit)
            {
                if (incomingCards[0].rank + 1 == incomingCards[1].rank && incomingCards[0].suit == incomingCards[1].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[1].rank + 1 == incomingCards[2].rank && incomingCards[1].suit == incomingCards[2].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[2].rank + 1 == incomingCards[3].rank && incomingCards[2].suit == incomingCards[3].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
                {
                    straightFlushCount++;
                }
                if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[4].suit == incomingCards[5].suit)
                {
                    straightFlushCount++;
                }
            }
        }

        if (incomingCards[0].rank == RankType.Two && incomingCards[6].rank == RankType.Ace && incomingCards[0].suit == incomingCards[6].suit)
        {
            if (straightFlushCount == 3)
            {
                handValue.Total = 5;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
        }

        if (straightFlushCount >= 4)
        {
            if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank 
                && incomingCards[4].suit == incomingCards[5].suit && incomingCards[5].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
            else if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank
                && incomingCards[4].suit == incomingCards[5].suit)
            {
                handValue.Total = (int)incomingCards[5].rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank
                && incomingCards[5].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[4].rank + 1 == incomingCards[6].rank
                && incomingCards[4].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                handValue.PokerHand = PokerHand.StraightFlush;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank && incomingCards[4].rank + 1 != incomingCards[6].rank)
            {
                if (incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
                {
                    handValue.Total = (int)incomingCards[4].rank;
                    handValue.PokerHand = PokerHand.StraightFlush;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[5].rank && incomingCards[3].suit == incomingCards[5].suit)
                {
                    handValue.Total = (int)incomingCards[5].rank;
                    handValue.PokerHand = PokerHand.StraightFlush;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[6].rank && incomingCards[3].suit == incomingCards[6].suit)
                {
                    handValue.Total = (int)incomingCards[6].rank;
                    handValue.PokerHand = PokerHand.StraightFlush;
                    return true;
                }
            }
        }
        return false;
    }

}
