using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour {

    GameObject gameManager;
    GameManager gm;
    string playerName;
    public Cards card;
    public HandValue handValue;

    public struct HandValue
    {
        public int Total { get; set; }
        public int HighCard { get; set; }
    }


    // Use this for initialization
    void Start () {
        RankType myRank;
        SuitType mySuit;
        if(this.gameObject.name.Substring(0, 1) == "C")
        {
            mySuit = SuitType.Clubs;
        }
        else if (this.gameObject.name.Substring(0, 1) == "D")
        {
            mySuit = SuitType.Diamonds;
        }
        else if(this.gameObject.name.Substring(0, 1) == "H")
        {
            mySuit = SuitType.Hearts;
        }
        else
        {
            mySuit = SuitType.Spades;
        }

        if (this.gameObject.name.Substring(1,1) == "1")
        {
            myRank = RankType.Ten;
        }
        else if (this.gameObject.name.Substring(1, 1) == "2")
        {
            myRank = RankType.Two;
        }
        else if (this.gameObject.name.Substring(1, 1) == "3")
        {
            myRank = RankType.Three;
        }
        else if (this.gameObject.name.Substring(1, 1) == "4")
        {
            myRank = RankType.Four;
        }
        else if (this.gameObject.name.Substring(1, 1) == "5")
        {
            myRank = RankType.Five;
        }
        else if (this.gameObject.name.Substring(1, 1) == "6")
        {
            myRank = RankType.Six;
        }
        else if (this.gameObject.name.Substring(1, 1) == "7")
        {
            myRank = RankType.Seven;
        }
        else if (this.gameObject.name.Substring(1, 1) == "8")
        {
            myRank = RankType.Eight;
        }
        else if (this.gameObject.name.Substring(1, 1) == "9")
        {
            myRank = RankType.Nine;
        }
        else if (this.gameObject.name.Substring(1, 1) == "j")
        {
            myRank = RankType.Jack;
        }
        else if (this.gameObject.name.Substring(1, 1) == "q")
        {
            myRank = RankType.Queen;
        }
        else if (this.gameObject.name.Substring(1, 1) == "k")
        {
            myRank = RankType.King;
        }
        else
        {
            myRank = RankType.Ace;
        }
        card = new Cards(myRank, mySuit);
        Debug.Log(card.rank + " " + card.suit);
	}

	// Update is called once per frame
	void Update () {

    }
}