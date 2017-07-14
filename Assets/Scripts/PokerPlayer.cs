using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Playing, NoHand, Winner, Loser}


public class PokerPlayer {

    public int SeatPos { get; set; }
    public int ChipCount { get; set; }
    public HandEvaluator Hand { get; set; }
    public PlayerState PlayerState { get; set; }

}

