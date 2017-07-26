using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Destination { player0, player1, player2, player3, player4, board, burn, pot}
public enum DealerState { DealingState, ShufflingState };
public enum GameState { PreFlop, Flop, Turn, River, ShowDown }

public class Table {
    
    private static Table _instance;
    public static Table instance
    {
        get
        {
            if (_instance == null)
                _instance = new Table();
            return _instance;
        }
    }

    public static GameState gameState;
    public static DealerState dealerState;

    //Where the cards could possibly go
    public List<Destination> playerDestinations = new List<Destination>
    {
        Destination.player0, Destination.player1, Destination.player2, Destination.player3, Destination.player4
    };
    public List<Card>[] playerCards = new List<Card>[5]
    {
        new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>()
    };
    public List<Card> _board = new List<Card>();
    public List<Card> _burn = new List<Card>();
    //
    //where the chips could possibly go
    public List<Chip>[] playerChipStacks = new List<Chip>[5]
    {
        new List<Chip>(), new List<Chip>(), new List<Chip>(), new List<Chip>(), new List<Chip>()
    };
    public List<Chip> _potChips = new List<Chip>();
    public int PotChips { get { return potChips; } set { potChips = value; } }
    private int potChips
    {
        get { return DeterminePotSize(); }
        set { }
    }



    public void NewHand()
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            playerCards[i].Clear();
        }
        _board.Clear();
        _burn.Clear();
        Services.GameManager.ResetPlayerStatus();
        gameState = GameState.PreFlop;
    }

    public int DeterminePotSize()
    {
        int potSize = 0;
        for (int i = 0; i < _potChips.Count; i++)
        {
            potSize += _potChips[i].chipValue;
        }
        //For testing purposes, I'm currently setting the pot to always be 10
        //since I don't actually have a pot.
        //return potSize;
        return 10;
    }

    public int GetChipStack(int seatPos)
    {
        int chipStack = 0;
        for (int i = 0; i < playerChipStacks.Length; i++)
        {
            if(seatPos == i)
            {
                for (int j = 0; j < playerChipStacks[seatPos].Count; j++)
                {
                    chipStack += playerChipStacks[seatPos][j].chipValue;
                }
            }
        }
        return chipStack;
    }

    public List<GameObject> GetCardObjects(int seatPos)
    {
        List<GameObject> cardsInHand = new List<GameObject>();
        for (int i = 0; i < playerCards[i].Count; i++)
        {
            cardsInHand.Add(playerCards[seatPos][i].gameObject);
        }
        return cardsInHand;
    }

    public void AddChipTo(Destination dest, Chip chip) 
    {
        for (int i = 0; i < playerDestinations.Count; i++)
        {
            if(dest == playerDestinations[i])
            {
                playerChipStacks[i].Add(chip);
            }
        }
    }

    public void AddCardTo (Destination dest, Card card)
    {
        for (int i = 0; i < playerDestinations.Count; i++)
        {
            if (dest == playerDestinations[i])
            {
                playerCards[i].Add(card);
            }
        }
        if(dest == Destination.board)
        {
            _board.Add(card);
        }
        else if (dest == Destination.burn)
        {
            _burn.Add(card);
        }
    }

    public List<CardType> EvaluatePlayerPreFlop(int seatPos)
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        EvaluatedHand = GetCardTypes(seatPos);
        EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        return EvaluatedHand;

    }

    public List<CardType> EvaluatePlayerAtFlop(int seatPos)
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        EvaluatedHand = GetCardTypes(seatPos);
        EvaluatedHand.Add(_board[0].cardType);
        EvaluatedHand.Add(_board[1].cardType);
        EvaluatedHand.Add(_board[2].cardType);
        EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));

        return EvaluatedHand;
    }

    public List<CardType> EvaluatePlayerAtTurn(int seatPos)
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        EvaluatedHand = GetCardTypes(seatPos);
        EvaluatedHand.Add(_board[0].cardType);
        EvaluatedHand.Add(_board[1].cardType);
        EvaluatedHand.Add(_board[2].cardType);
        EvaluatedHand.Add(_board[3].cardType);
        EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));

        return EvaluatedHand;
    }

    public List<CardType> EvaluatePlayerAtRiver(int seatPos)
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        EvaluatedHand = GetCardTypes(seatPos);
        EvaluatedHand.Add(_board[0].cardType);
        EvaluatedHand.Add(_board[1].cardType);
        EvaluatedHand.Add(_board[2].cardType);
        EvaluatedHand.Add(_board[3].cardType);
        EvaluatedHand.Add(_board[4].cardType);
        EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));

        return EvaluatedHand;
    }

    public List<CardType> GetCardTypes(int seatPos)
    {
        List<CardType> cardTypes = new List<CardType>();
        for (int i = 0; i < playerCards[seatPos].Count; i++)
        {
            cardTypes.Add(playerCards[seatPos][i].cardType);
        }
        return cardTypes;
    }

    public void DebugHands()
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            for (int j = 0; j < playerCards[i].Count; j++)
            {
                Debug.Log("Player" + i + " Card " + j + " is " + playerCards[i][j].cardType.rank + " of " + playerCards[i][j].cardType.suit);
            }
            
        }
        for (int i = 0; i < _board.Count; i++)
        {
            Debug.Log("Board Card " + i + " is " + _board[i].cardType.rank + " of " + _board[i].cardType.rank);
        }
    }
}
