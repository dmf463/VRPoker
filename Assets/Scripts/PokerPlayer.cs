using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PokerPlayer
{
    public Destinations SeatPos { get; set; }
    public int ChipCount { get; set; }
    public List<Card> Cards { get; set;}
    public HandEvaluator Hand { get; set; }
}

public class PlayerEvaluator {

    private PokerPlayer pokerPlayer;

    public PokerPlayer PokerPlayer
    {
        get { return pokerPlayer; }
        set { pokerPlayer = value; }
    }

    public void GetPlayerInfo(Destinations dest, HandEvaluator hand)
    {
        if(dest == Destinations.player0)
        {
            pokerPlayer.SeatPos = Destinations.player0;
            pokerPlayer.Hand = hand;
        }
        else if (dest == Destinations.player1)
        {
            pokerPlayer.SeatPos = Destinations.player1;
            pokerPlayer.Hand = hand;
        }
        else if (dest == Destinations.player2)
        {
            pokerPlayer.SeatPos = Destinations.player2;
            pokerPlayer.Hand = hand;
        }
        else if (dest == Destinations.player3)
        {
            pokerPlayer.SeatPos = Destinations.player3;
            pokerPlayer.Hand = hand;
        }
    }

}
