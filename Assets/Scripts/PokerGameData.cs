using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerGameData {

    //things I need to know to reset a round
    //How many players were in the hand
    //how many chips did they each have
    //where was the dealer button and the blinds.
    public int DealerPosition;
    public List<int> PlayerChipStacks = new List<int>();

    public PokerGameData(int DealerPos, List<PokerPlayerRedux> players)
    {
        DealerPosition = DealerPos;
        for (int i = 0; i < players.Count; i++)
        {
            PlayerChipStacks.Add(players[i].chipCount);
        }
    }

}
