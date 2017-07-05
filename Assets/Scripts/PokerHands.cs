//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//    public enum HandType
//    {
//        HighCard,
//        Pair,
//        TwoPair,
//        ThreeOfAKind,
//        Straight,
//        Flush,
//        FullHouse,
//        FourOfAKind,
//        StraightFlush
//    }

//public struct HandValue
//{
//    public int Total { get; set; }
//    public int HighCard { get; set; }
//}

//public static class PokerHands
//{
//    private int spadeSum;
//    private int heartSum;
//    private int diamondSum;
//    private int clubSum;
//    private static HandValue handValue;

//    public PokerHand(Cards[] sortedHand)
//    {
//        spadeSum = 0;
//        heartSum = 0;
//        diamondSum = 0;
//        clubSum = 0;
//        handValue = new HandValue();
//    }

//    public static HandValue HandValues
//    {
//        get { return handValue; }
//        set { handValue = value; }
//    }

//    public static Cards[] Card
//    {
//        get { return card; }
//        set
//        {
//            card[0] = value[0];
//            card[1] = value[1];
//            card[2] = value[2];
//            card[3] = value[3];
//            card[4] = value[4];
//            card[5] = value[5];
//            card[6] = value[6];
//        }
//    }

//    public HandType EvaluateHand(List<GameObject> hand)
//    {
//        getNumberOfSuit();
//        if (OnePair())
//            return HandType.Pair;
//        else if (TwoPair())
//            return HandType.TwoPair;
//        else if (ThreeOfKind())
//            return HandType.ThreeOfAKind;
//        else if (Straight())
//            return HandType.Straight;
//        else if (Flush())
//            return HandType.Flush;
//        else if (FourOfKind())
//            return HandType.FourOfAKind;
//        else if (FullHouse())
//            return HandType.FullHouse;
//        else if (StraightFlush())
//            return HandType.StraightFlush;
//        handValue.HighCard = (int)hand[6].GetComponent<PlayingCardScript>().card.rank;
//        return HandType.HighCard;
//    }

//    public static int getNumberOfSuit()
//    {
//        return 0;
//    }

    //public static bool OnePair()
    //{
    //    return false;
    //}

    //public static bool TwoPair()
    //{
    //    return false;
    //}

    //public static bool ThreeOfKind()
    //{
    //    return false;
    //}

    //public static bool Straight()
    //{
    //    return false;
    //}

    //public static bool Flush()
    //{
    //    return false;
    //}

    //public static bool FourOfKind()
    //{
    //    return false;
    //}

    //public static bool FullHouse()
    //{
    //    return false;
    //}

    //public static bool StraightFlush()
    //{
    //    return false;
    //}

//}



