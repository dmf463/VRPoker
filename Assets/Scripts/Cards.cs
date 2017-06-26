using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public enum RankType
    {
        Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace
    }

    public enum SuitType
    {
       Spades, Hearts, Diamonds, Clubs
    }



    public class Cards
{
    public RankType rank;
    public SuitType suit;

    public Cards(RankType _rank, SuitType _suit)
    {
        this.rank = _rank;
        this.suit = _suit;
    }
}
