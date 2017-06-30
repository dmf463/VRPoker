using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum RankType
{
    Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace
}

public enum SuitType
{
    Spades, Hearts, Diamonds, Clubs
}
    [Serializable]
    public class Cards
{
    [SerializeField] public RankType rank { get; set; }
    [SerializeField] public SuitType suit { get; set; }

    public Cards(RankType _rank, SuitType _suit)
    {
        rank = _rank;
        suit = _suit;
    }
}
