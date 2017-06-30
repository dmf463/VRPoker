using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Destinations { player0, player1, player2, player3, board, burn, table}

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

    public List<Cards> _player0 = new List<Cards>();
    public List<Cards> _player1 = new List<Cards>();
    public List<Cards> _player2 = new List<Cards>();
    public List<Cards> _player3 = new List<Cards>();
    public List<Cards> _board = new List<Cards>();
    public List<Cards> _burn = new List<Cards>();
    public List<Cards> _table = new List<Cards>();

    public void AddCardTo (Destinations dest, Cards card)
    {
        if (dest == Destinations.player0)
        {
            _player0.Add(card);
            Debug.Log(_player0[0].rank);
        }
        else if (dest == Destinations.player0)
        {
            _player0.Add(card);
            Debug.Log(_player1[0].rank);
        }
        else if (dest == Destinations.player1)
        {
            _player0.Add(card);
            Debug.Log(_player2[0].rank);
        }
        else if (dest == Destinations.player2)
        {
            _player0.Add(card);
            Debug.Log(_player3[0].rank);
        }
        else if(dest == Destinations.board)
        {
            _board.Add(card);
            Debug.Log(_board[0].rank);
        }
        else if (dest == Destinations.burn)
        {
            _burn.Add(card);
            Debug.Log(_burn[0].rank);
        }
        else if (dest == Destinations.table)
        {
            _player0.Add(card);
            Debug.Log(_table[0].rank);
        }
    }

    public void NewGame()
    {
        _player0.Clear();
        _player1.Clear();
        _player2.Clear();
        _player3.Clear();
        _board.Clear();
        _burn.Clear();
        _table.Clear();
    }

    public void EvaluatePlayer(Destinations dest)
    {
        List<Cards> EvaluatedHand = new List<Cards>();
        if (dest == Destinations.player0)
        {
            EvaluatedHand = _player0;
            EvaluatedHand.AddRange(_board);
        }
        if (dest == Destinations.player1)
        {
            EvaluatedHand = _player1;
            EvaluatedHand.AddRange(_board);
        }
        if (dest == Destinations.player2)
        {
            EvaluatedHand = _player2;
            EvaluatedHand.AddRange(_board);
        }
        if (dest == Destinations.player3)
        {
            EvaluatedHand = _player3;
            EvaluatedHand.AddRange(_board);
        }

        //Pass To Whatever
    }

    public void DebugHands()
    {
        Debug.Log("player0 has " + _player0.Count + "card(s)");
        for (int i = 0; i <= _player0.Count; i++)
        {
            Debug.Log("Card " + i + " is " + _player0[i].rank + " " + _player0[i].suit);
        }
        for (int i = 0; i <= _player1.Count; i++)
        {
            Debug.Log("Card " + i + " is " + _player1[i].rank + " " + _player1[i].suit);
        }
        for (int i = 0; i <= _player2.Count; i++)
        {
            Debug.Log("Card " + i + " is " + _player2[i].rank + " " + _player2[i].suit);
        }
        for (int i = 0; i <= _player3.Count; i++)
        {
            Debug.Log("Card " + i + " is " + _player3[i].rank + " " + _player3[i].suit);
        }
        for (int i = 0; i <= _table.Count; i++)
        {
            Debug.Log("Card " + i + " is " + _table[i].rank + " " + _table[i].suit);
        }
    }
}
