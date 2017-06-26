using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum HandType
    {
        HighCard,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush
    }

public class PokerHands
{
    public HandType hand;

    public PokerHands(HandType _hand)
    {
        this.hand = _hand;
    }
}

