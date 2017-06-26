using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    public int seatPos;
    Cards holeCard1;
    Cards holeCard2;
    Cards boardCards;

    public Player(int _seatPos, Cards card1, Cards card2, Cards theBoard)
    {
        _seatPos = seatPos;
        card1 = holeCard1;
        card2 = holeCard2;
        theBoard = boardCards;
    }
}
