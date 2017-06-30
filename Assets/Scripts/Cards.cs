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
    public RankType rank;
    public SuitType suit;
}
