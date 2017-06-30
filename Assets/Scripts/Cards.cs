using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


    [Serializable]
    public class Cards
{
    public enum RankType
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace
    }

    public enum SuitType
    {
        Spades, Hearts, Diamonds, Clubs
    }

    public RankType rank;
    public SuitType suit;
}
