using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Playing, NoHand, Winner, Loser}


public class PokerPlayer {

    public int SeatPos { get; set; }
    public int ChipCount { get; set; }
    public HandEvaluator Hand { get; set; }
    public PlayerState PlayerState { get; set; }


    public void EvaluateHandPreFlop() 
    {
        TableCards.gameState = GameState.PreFlop;
        ChipCount = TableCards.instance.GetChipStack(SeatPos);
        List<CardType> sortedCards = TableCards.instance.EvaluatePlayerPreFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtPreFlop();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnFlop() 
    {
        ChipCount = TableCards.instance.GetChipStack(SeatPos);
        TableCards.gameState = GameState.Flop;
        List<CardType> sortedCards = TableCards.instance.EvaluatePlayerAtFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtFlop();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnTurn() 
    {
        ChipCount = TableCards.instance.GetChipStack(SeatPos);
        TableCards.gameState = GameState.Turn;
        List<CardType> sortedCards = TableCards.instance.EvaluatePlayerAtTurn(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtTurn();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnRiver() 
    {
        ChipCount = TableCards.instance.GetChipStack(SeatPos);
        TableCards.gameState = GameState.River;
        List<CardType> sortedCards = TableCards.instance.EvaluatePlayerAtRiver(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtRiver();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

}

