using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PokerHand { highCard, onePair, twoPair, threeOfKind, straight, flush, fullHouse, fourOfKind, straightFlush }

public struct HandValue
{
    public int Total { get;  set;}
    public int HighCard { get; set; }
}

public class HandEvaluator {

    private int spadeSum;
    private int heartSum;
    private int diamondSum;
    private int clubSum;
    private int straightCount;
    private List<Cards> incomingCards;
    private HandValue handValue;

    public HandEvaluator(List<Cards> sortedCards)
    {
        spadeSum = 0;
        heartSum = 0;
        diamondSum = 0;
        clubSum = 0;
        straightCount = 0;
        incomingCards = new List<Cards>(7);
        Cards = sortedCards;
    }

    public List<Cards> Cards
    {
        get { return incomingCards; }
        set
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

    public PokerHand EvaluateHand()
    {
        //get number of suit in each hand
        getNumberOfSuit();
        if (OnePair())
        {
            return PokerHand.onePair;
        }
        else if (TwoPair())
        {
            return PokerHand.twoPair;
        }
        else if (ThreeOfKind())
        {
            return PokerHand.threeOfKind;
        }
        else if (Straight())
        {
            return PokerHand.straight;
        }
        else if (Flush())
        {
            return PokerHand.flush;
        }
        else if (FullHouse())
        {
            return PokerHand.fullHouse;
        }
        else if (FourOfKind())
        {
            return PokerHand.fourOfKind;
        }
        else if (StraightFlush())
        {
            return PokerHand.straightFlush;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard = (int)incomingCards[6].rank;
        return PokerHand.highCard;

    }

    private void getNumberOfSuit()
    {
        foreach (Cards card in Cards)
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

    public bool OnePair()
    {
        return false;
    }

    public bool TwoPair()
    {
        return false;
    }

    public bool ThreeOfKind()
    {
        return false;
    }

    public bool Straight()
    {
        if (incomingCards[0].rank + 1 == incomingCards[1].rank)
        {
            straightCount++;
        }
        if (incomingCards[1].rank + 1 == incomingCards[2].rank)
        {
            straightCount++;
        }
        if (incomingCards[2].rank + 1 == incomingCards[3].rank)
        {
            straightCount++;
        }
        if (incomingCards[3].rank + 1 == incomingCards[4].rank)
        {
            straightCount++;
        }
        if (incomingCards[4].rank + 1 == incomingCards[5].rank)
        {
            straightCount++;
        }
        if (incomingCards[5].rank + 1 == incomingCards[6].rank)
        {
            straightCount++;
        }
        if(straightCount >= 4)
        {
            if(incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if(incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[5].rank;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[4].rank + 1 == incomingCards[6].rank)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank && incomingCards[4].rank + 1 != incomingCards[6].rank)
            {
                if(incomingCards[3].rank + 1 == incomingCards[4].rank)
                {
                    handValue.Total = (int)incomingCards[4].rank;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[5].rank)
                {
                    handValue.Total = (int)incomingCards[5].rank;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[6].rank)
                {
                    handValue.Total = (int)incomingCards[6].rank;
                    return true;
                }
            }
        }
        return false;
    }

    public bool Flush()
    {
        if(spadeSum == 5 || heartSum == 5 || diamondSum == 5 || clubSum == 5)
        {
            if (incomingCards[4].suit == incomingCards[5].suit && incomingCards[4].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if (incomingCards[4].suit == incomingCards[5].suit && incomingCards[4].suit != incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[5].rank;
                return true;
            }
            else if (incomingCards[4].suit != incomingCards[5].suit && incomingCards[5].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if (incomingCards[4].suit != incomingCards[5].suit && incomingCards[4].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if(incomingCards[4].suit != incomingCards[5].suit && incomingCards[4].suit != incomingCards[6].suit && incomingCards[5].suit != incomingCards[6].suit)
            {
                if(incomingCards[0].suit == incomingCards[4].suit)
                {
                    handValue.Total = (int)incomingCards[4].rank;
                    return true;
                }
                else if (incomingCards[0].suit == incomingCards[5].suit)
                {
                    handValue.Total = (int)incomingCards[5].rank;
                    return true;
                }
                else if (incomingCards[0].suit == incomingCards[6].suit)
                {
                    handValue.Total = (int)incomingCards[6].rank;
                    return true;
                }
            }

        }
        return false;
    }

    public bool FullHouse()
    {
        //if there is a pair and trips within the 7 cards, it's a full house

        //0 = 1, 2 = 3 = 4
        if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            return true;
        }
        //0 = 1, 3 = 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            return true;
        }
        //0 = 1, 4 = 5 = 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            return true;
        }
        //1 = 2, 3 = 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            return true;
        }
        //1 = 2, 4 = 5 = 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            return true;
        }
        //2 = 3, 4 =  5 = 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            return true;
        }
        //0 = 1 = 2, 3 = 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank);
            return true;
        }
        //0 = 1 = 2, 4 = 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            return true;
        }
        //0 = 1 = 2, 5 = 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[0].rank) + (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            return true;
        }
        //1 = 2 = 3, 4 = 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank);
            return true;
        }
        //1 = 2 = 3, 5 = 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[1].rank) + (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            return true;
        }
        //2 = 3 = 4, 5 = 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)(incomingCards[2].rank) + (int)(incomingCards[3].rank) + (int)(incomingCards[4].rank) + (int)(incomingCards[5].rank) + (int)(incomingCards[6].rank);
            return true;
        }
        return false;
    }

    public bool FourOfKind()
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
            return true;
        }
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[5].rank && incomingCards[2].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 4;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank && incomingCards[3].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[3].rank * 4;
            handValue.HighCard = (int)incomingCards[2].rank;
            return true;
        }
        return false;
    }

    public bool StraightFlush()
    {
        return false;
    }

}
