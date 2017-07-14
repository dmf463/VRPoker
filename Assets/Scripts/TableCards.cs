﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Destination { player0, player1, player2, player3, board, burn, table}
public enum DealerState { DealingState, ShufflingState };

public class TableCards {
    
    private static TableCards _instance;
    public static TableCards instance
    {
        get
        {
            if (_instance == null)
                _instance = new TableCards();
            return _instance;
        }
    }

    public static DealerState dealerState;

    public List<CardType> _player0 = new List<CardType>();
    public List<CardType> _player1 = new List<CardType>();
    public List<CardType> _player2 = new List<CardType>();
    public List<CardType> _player3 = new List<CardType>();
    public List<CardType> _board = new List<CardType>();
    public List<CardType> _burn = new List<CardType>();
    public List<CardType> _table = new List<CardType>();

    public void AddCardTo (Destination dest, CardType card)
    {
        if (dest == Destination.player0)
        {
            _player0.Add(card);
            //Debug.Log(_player0[0].rank);
        }
        else if (dest == Destination.player1)
        {
            _player1.Add(card);
            //Debug.Log(_player1[0].rank);
        }
        else if (dest == Destination.player2)
        {
            _player2.Add(card);
            //Debug.Log(_player2[0].rank);
        }
        else if (dest == Destination.player3)
        {
            _player3.Add(card);
            //Debug.Log(_player3[0].rank);
        }
        else if(dest == Destination.board)
        {
            _board.Add(card);
           // Debug.Log(_board[0].rank);
        }
        else if (dest == Destination.burn)
        {
            _burn.Add(card);
            //Debug.Log(_burn[0].rank);
        }
    }

    public void NewHand()
    {
        _player0.Clear();
        _player1.Clear();
        _player2.Clear();
        _player3.Clear();
        _board.Clear();
        _burn.Clear();
        _table.Clear();
    }

    public List<CardType> EvaluatePlayer(int seatPos)
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        if (seatPos == 0)
        {
            EvaluatedHand = _player0;
            EvaluatedHand.AddRange(_board);
            EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        }
        if (seatPos == 1)
        {
            EvaluatedHand = _player1;
            EvaluatedHand.AddRange(_board);
            EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        }
        if (seatPos == 2)
        {
            EvaluatedHand = _player2;
            EvaluatedHand.AddRange(_board);
            EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        }
        if (seatPos == 3)
        {
            EvaluatedHand = _player3;
            EvaluatedHand.AddRange(_board);
            EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        }
        return EvaluatedHand;
    }

    public void DebugHands()
    {
        //Debug.Log("player0 has " + _player0.Count + "card(s)");
        for (int i = 0; i < _player0.Count; i++)
        {
            Debug.Log("Player 0 Card " + i + " is " + _player0[i].rank + " of " + _player0[i].suit);
        }
        for (int i = 0; i < _player1.Count; i++)
        {
            Debug.Log("Player 1 Card " + i + " is " + _player1[i].rank + " of " + _player1[i].suit);
        }
        for (int i = 0; i < _player2.Count; i++)
        {
            Debug.Log("Player 2 Card " + i + " is " + _player2[i].rank + " of " + _player2[i].suit);
        }
        for (int i = 0; i < _player3.Count; i++)
        {
            Debug.Log("Player 3 Card " + i + " is " + _player3[i].rank + " of " + _player3[i].suit);
        }
        for (int i = 0; i < _board.Count; i++)
        {
            Debug.Log("Board Card " + i + " is " + _board[i].rank + " of " + _board[i].suit);
        }
    }
}
