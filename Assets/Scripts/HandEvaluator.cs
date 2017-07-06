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
        if (StraightFlush())
        {
            return PokerHand.straightFlush;
        }
        else if (FourOfKind())
        {
            return PokerHand.fourOfKind;
        }
        else if (FullHouse())
        {
            return PokerHand.fullHouse;
        }
        else if (Flush())
        {
            return PokerHand.flush;
        }
        else if (Straight())
        {
            return PokerHand.straight;
        }
        else if (ThreeOfKind())
        {
            return PokerHand.threeOfKind;
        }
        else if (TwoPair())
        {
            return PokerHand.twoPair;
        }
        else if (OnePair())
        {
            return PokerHand.onePair;
        }
        //if the hand is nothing, than the player with highest card wins
        handValue.HighCard = (int)incomingCards[6].rank;
        return PokerHand.highCard;

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

    public bool OnePair()
    {
        //if there are two cards that are the same, it's a pair
        //0, 1 with 6 high card
        if (incomingCards[0].rank == incomingCards[1].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 2;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //1, 2 with 6 high card
        else if (incomingCards[1].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 2;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //2, 3 with 6 high card
        else if (incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 2;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //3, 4 with 6 high card
        else if (incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[3].rank * 2;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //4, 5 with 6 high card
        else if (incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[4].rank * 2;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //5, 6 with 4 high card
        else if (incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[5].rank * 2;
            handValue.HighCard = (int)incomingCards[4].rank;
            return true;
        }
        return false;
    }

    public bool TwoPair()
    {
        //if there are two pairs of cards that are the same then it's two pair
        //0, 1 && 2, 3
        if(incomingCards[0].rank == incomingCards[1].rank && incomingCards[2].rank == incomingCards[3].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[2].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //0, 1 && 3, 4
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //0, 1 && 4, 5
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //0, 1 && 5, 6
        else if (incomingCards[0].rank == incomingCards[1].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[0].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            return true;
        }
        //1, 2 && 3, 4
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[3].rank == incomingCards[4].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[3].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //1, 2 && 4, 5
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //1, 2 && 5, 6
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[1].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            return true;
        }
        //2, 3 && 4, 5
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[4].rank == incomingCards[5].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + ((int)incomingCards[4].rank * 2);
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //2, 3 && 5, 6
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[2].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[4].rank;
            return true;
        }
        //3, 4 && 5, 6
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[5].rank == incomingCards[6].rank)
        {
            handValue.Total = ((int)incomingCards[3].rank * 2) + ((int)incomingCards[5].rank * 2);
            handValue.HighCard = (int)incomingCards[2].rank;
            return true;
        }
        return false;
    }

    public bool ThreeOfKind()
    {
        //if 3 cards are the same then it's a three of a kind
        //0, 1, 2
        if(incomingCards[0].rank == incomingCards[1].rank && incomingCards[0].rank == incomingCards[2].rank)
        {
            handValue.Total = (int)incomingCards[0].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //1, 2, 3
        else if (incomingCards[1].rank == incomingCards[2].rank && incomingCards[1].rank == incomingCards[3].rank)
        {
            handValue.Total = (int)incomingCards[1].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //2, 3, 4
        else if (incomingCards[2].rank == incomingCards[3].rank && incomingCards[2].rank == incomingCards[4].rank)
        {
            handValue.Total = (int)incomingCards[2].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //3, 4, 5
        else if (incomingCards[3].rank == incomingCards[4].rank && incomingCards[3].rank == incomingCards[5].rank)
        {
            handValue.Total = (int)incomingCards[5].rank * 3;
            handValue.HighCard = (int)incomingCards[6].rank;
            return true;
        }
        //4, 5, 6
        else if (incomingCards[4].rank == incomingCards[5].rank && incomingCards[4].rank == incomingCards[6].rank)
        {
            handValue.Total = (int)incomingCards[4].rank * 3;
            handValue.HighCard = (int)incomingCards[3].rank;
            return true;
        }
        return false;
    }

    public bool Straight()
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

        if (straightCount >= 4)
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
        if (straightFlushCount >= 4)
        {
            if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank 
                && incomingCards[4].suit == incomingCards[5].suit && incomingCards[5].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if (incomingCards[4].rank + 1 == incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank
                && incomingCards[4].suit == incomingCards[5].suit)
            {
                handValue.Total = (int)incomingCards[5].rank;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 == incomingCards[6].rank
                && incomingCards[5].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[4].rank + 1 == incomingCards[6].rank
                && incomingCards[4].suit == incomingCards[6].suit)
            {
                handValue.Total = (int)incomingCards[6].rank;
                return true;
            }
            else if (incomingCards[4].rank + 1 != incomingCards[5].rank && incomingCards[5].rank + 1 != incomingCards[6].rank && incomingCards[4].rank + 1 != incomingCards[6].rank)
            {
                if (incomingCards[3].rank + 1 == incomingCards[4].rank && incomingCards[3].suit == incomingCards[4].suit)
                {
                    handValue.Total = (int)incomingCards[4].rank;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[5].rank && incomingCards[3].suit == incomingCards[5].suit)
                {
                    handValue.Total = (int)incomingCards[5].rank;
                    return true;
                }
                else if (incomingCards[3].rank + 1 == incomingCards[6].rank && incomingCards[3].suit == incomingCards[6].suit)
                {
                    handValue.Total = (int)incomingCards[6].rank;
                    return true;
                }
            }
        }
        return false;
    }

}
