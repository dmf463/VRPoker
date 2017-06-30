using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerPlayer {

    public int seatPos;
    Cards holeCard1;
    Cards holeCard2;
    Cards boardCards;
    Cards[] cards; 

    public PokerPlayer(int _seatPos, Cards card1, Cards card2, Cards theBoard)
    {
        _seatPos = seatPos;
        card1 = holeCard1;
        card2 = holeCard2;
        theBoard = boardCards;
    }
}
