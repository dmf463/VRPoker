﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//these are the important enums for the game.
//Destination controls where obejcts go for the Table functions, but tbh it probably needs to be reconsidered
//Dealerstate switches the dealer between shuffling and dealing which have MINOR differences between them.
//Gamestate is super important and controls the flow of the game. 
public enum Destination { player0, player1, player2, player3, player4, board, burn, pot}
public enum DealerState { DealingState, ShufflingState };
public enum GameState {NewRound, PreFlop, Flop, Turn, River, ShowDown, CleanUp, PostHand}

public class Table {
    
    //not really sure why I have this as a singleton
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



    //the pubic functions for controlling the enums
    public static GameState gameState;
    public static DealerState dealerState;

    //Where the cards could possibly go
    public List<Destination> playerDestinations = new List<Destination>
    {
        Destination.player0, Destination.player1, Destination.player2, Destination.player3, Destination.player4
    };

    //where the players chip spawn when they bet
    public List<GameObject> playerBetZones = new List<GameObject>
    {
       GameObject.Find("P0BetZone"), GameObject.Find("P1BetZone"), GameObject.Find("P2BetZone"), GameObject.Find("P3BetZone"), GameObject.Find("P4BetZone")
    };
    //this is the list that holds each players "hole cards"
    public List<Card>[] playerCards = new List<Card>[5]
    {
        new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>(), new List<Card>()
    };

    //this is the list that holds the community cards
    public List<Card> _board = new List<Card>();

    //currently don't actually require the player to burn, but that will change.
    public List<Card> _burn = new List<Card>();

    //this is the list that holds all the players chips. this is where we get the actual chips to count and instantiate
    public List<Chip>[] playerChipStacks = new List<Chip>[5]
    {
        new List<Chip>(), new List<Chip>(), new List<Chip>(), new List<Chip>(), new List<Chip>()
    };

    //we transfer chips that have been bet to here
    public List<Chip> _potChips = new List<Chip>();
    
    //the int value of the chips in the pot
    public int PotChips { get { return potChips; } set { potChips = value; } }
    private int potChips
    {
        get { return DeterminePotSize(); }
        set { }
    }
    public int DealerPosition;

    //this function instantiates a new dealer button if there is none
    //if there is, then we put it at the correct dealer pos for a given round
    public void SetDealerButtonPos(int dealerPos)
    {
        if(GameObject.FindGameObjectWithTag("DealerButton") == null)
        {
            GameObject dealerButton = GameObject.Instantiate(Services.PrefabDB.DealerButton, playerBetZones[dealerPos].transform.position, Quaternion.identity);
        }
        else
        {
            GameObject dealerButton = GameObject.FindGameObjectWithTag("DealerButton");
            dealerButton.transform.position = playerBetZones[dealerPos].transform.position;
        }
    }

    public void ClearAllLists()
    {
        foreach(List<Card> playerCards in playerCards)
        {
            playerCards.Clear();
        }
        _board.Clear();
        _burn.Clear();
    }

    //this function goes through everything that needs to be reset and resets it
    //it also sets the dealer button to its new position and has players post thier blinds
    //we need to make the blind posting more malleable
    //because right now it doesn't really figure in for eliminated players in any meaningful way
    public void NewHand()
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            playerCards[i].Clear();
        }
        _board.Clear();
        _burn.Clear();
        _potChips.Clear();
        Services.Dealer.ResetPlayerStatus();
        gameState = GameState.NewRound;
        //DealerPosition = (DealerPosition + 1) % Services.Dealer.players.Count;
        DealerPosition = (Services.Dealer.FindFirstPlayerToAct(1).SeatPos); //this does not account for a dead dealer
        SetDealerButtonPos(DealerPosition);

        Services.Dealer.PlayerSeatsAwayFromDealerAmongstLivePlayers(1).Bet(Services.Dealer.SmallBlind);
        Services.Dealer.PlayerSeatsAwayFromDealerAmongstLivePlayers(1).currentBet = Services.Dealer.SmallBlind;

        Services.Dealer.PlayerSeatsAwayFromDealerAmongstLivePlayers(2).Bet(Services.Dealer.BigBlind);
        Services.Dealer.PlayerSeatsAwayFromDealerAmongstLivePlayers(2).currentBet = Services.Dealer.BigBlind;

        Services.Dealer.LastBet = Services.Dealer.BigBlind;

        //if(Services.Dealer.players[(DealerPosition + 1) % Services.Dealer.players.Count].PlayerState == PlayerState.Eliminated)
        //{
        //    Services.Dealer.players[(DealerPosition + 2) % Services.Dealer.players.Count].Bet(Services.Dealer.SmallBlind);
        //    Services.Dealer.players[(DealerPosition + 2) % Services.Dealer.players.Count].currentBet = Services.Dealer.SmallBlind;
        //    Services.Dealer.players[(DealerPosition + 3) % Services.Dealer.players.Count].Bet(Services.Dealer.BigBlind);
        //    Services.Dealer.players[(DealerPosition + 3) % Services.Dealer.players.Count].currentBet = Services.Dealer.BigBlind;
        //}
        //else
        //{
        //    Services.Dealer.players[(DealerPosition + 1) % Services.Dealer.players.Count].Bet(Services.Dealer.SmallBlind);
        //    Services.Dealer.players[(DealerPosition + 1) % Services.Dealer.players.Count].currentBet = Services.Dealer.SmallBlind;
        //    Services.Dealer.players[(DealerPosition + 2) % Services.Dealer.players.Count].Bet(Services.Dealer.BigBlind);
        //    Services.Dealer.players[(DealerPosition + 2) % Services.Dealer.players.Count].currentBet = Services.Dealer.BigBlind;
        //    Services.Dealer.LastBet = Services.Dealer.BigBlind;
        //}
    }

    //this is the function that converts the chips in the pot into an int value
    public int DeterminePotSize()
    {
        int potSize = 0;
        for (int i = 0; i < _potChips.Count; i++)
        {
            potSize += _potChips[i].chipValue;
        }
        return potSize;
    }

    //literally same as above but for chipStacks
    //since both of these hold chips, we need to take those chip values and add them up
    public int GetChipStackTotal(int seatPos)
    {
        int chipStack = 0;
        for (int i = 0; i < playerChipStacks[seatPos].Count; i++)
        {
            chipStack += playerChipStacks[seatPos][i].chipValue;
        }
        return chipStack;
    }

    //we use this function in order to get access to the actual card gameObjects
    //this is really only used when we want to flip and rearrange the cards (which should probably be made better)
    public List<GameObject> GetCardGameObjects(int seatPos)
    {
        List<GameObject> cardsInHand = new List<GameObject>();
        for (int i = 0; i < playerCards[seatPos].Count; i++)
        {
            cardsInHand.Add(playerCards[seatPos][i].gameObject);
        }
        return cardsInHand;
    }

    //same as above. sometimes we need access to the actual chip gameObjects, and this is where we do that
    //this was the source of some major problems for me for a while, because it interacted with log chips in a weird way
    //LogChips would sometimes trigger and remove gameobjects, but the Chip itself was still there, so I would get a missing reference
    public List<GameObject> GetChipGameObjects(int seatPos)
    {
        List<GameObject> chipsInStack = new List<GameObject>();
        for (int i = 0; i < playerChipStacks[seatPos].Count; i++)
        {
            if (playerChipStacks[seatPos][i] == null)
            {
                Debug.Log("ChipCount from GetChipGameObjects() for player " + seatPos + " is equal to " + GetChipStackTotal(seatPos));
                Debug.Log("ChipCount from ChipCount for player " + seatPos + " is equal to " + Services.Dealer.players[seatPos].ChipCount);
                Debug.Log("in the for loop, i = " + i + " and the playerChipStack[" + seatPos + "].count = " + playerChipStacks[seatPos].Count + " and time is " + Time.time);
            }
            chipsInStack.Add(playerChipStacks[seatPos][i].gameObject);
        }
        return chipsInStack;
    }

    //two functions below are mirror images of each other, and are used in LogChips to add and remove chips to and from thier proper lists
    //this was part of the above issue
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

    //see above
    public void RemoveChipFrom(Destination dest, Chip chip)
    {
        for (int i = 0; i < playerDestinations.Count; i++)
        {
            if (dest == playerDestinations[i])
            {
                playerChipStacks[i].Remove(chip);
            }
        }
    }

    //this is used in LogCards in order send cards to thier proper list
    //currently this depends on an actual card object
    //which can be annoying sometimes, because if a player overshoots or misses thier target by like a millimeter, then the card doesn't get logged
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

    //this series of functions takes the cards from the player's list and sorts them from high to low
    //this is necessary for evaluating the cards in the evaluator
    //it's possible that these could all be a single function, but as it currently stands, we need different ones
    //because each round of evaluation leads to different combinations of cards, and if we try to look at cards 
    //that don't exist, then we obviously get problems
    public List<CardType> SortPlayerCardsPreFlop(int seatPos)
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        EvaluatedHand = GetCardTypes(seatPos);
        EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
        return EvaluatedHand;
    }

    //see above
    public List<CardType> SortPlayerCardsAtFlop(int seatPos)
    {
        List<CardType> EvaluatedHand = new List<CardType>();
        EvaluatedHand = GetCardTypes(seatPos);
        EvaluatedHand.Add(_board[0].cardType);
        EvaluatedHand.Add(_board[1].cardType);
        EvaluatedHand.Add(_board[2].cardType);
        EvaluatedHand.Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));

        return EvaluatedHand;
    }

    //see above
    public List<CardType> SortPlayerCardsAtTurn(int seatPos)
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

    //see above
    public List<CardType> SortPlayerCardsAtRiver(int seatPos)
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

    //this returns the cards from the proper list and takes their CardType
    public List<CardType> GetCardTypes(int seatPos)
    {
        List<CardType> cardTypes = new List<CardType>();
        for (int i = 0; i < playerCards[seatPos].Count; i++)
        {
            cardTypes.Add(playerCards[seatPos][i].cardType);
        }
        return cardTypes;
    }

    //speaks for itself
    public void DebugHandsAndChips()
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

        for (int i = 0; i < playerChipStacks.Length; i++)
        {
            Debug.Log("Player " + i + " has a chipStack of " + GetChipStackTotal(i));
        }
        Debug.Log("the pot is at" + PotChips);
    }
}
