using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Playing, NoHand, Winner, Loser}

/*
 * 
 * now that I have a way to log the chip count at the beginning of every round
 * I need to make it so that I can TAKE chips from there when the player bets
 * but that should be an instance of the player BETTING and then I would just need
 * A) take that value from thier chipCount 
 * AND ALSO
 * B) remove the chips equal to that value from the tableList
 * 
 * */

public class PokerPlayer {

    public int SeatPos { get; set; }
    public int ChipCount { get; set; }
    public HandEvaluator Hand { get; set; }
    public PlayerState PlayerState { get; set; }


    public void EvaluateHandPreFlop() 
    {
        Table.gameState = GameState.PreFlop;
        ChipCount = Table.instance.GetChipStack(SeatPos);
        List<CardType> sortedCards = Table.instance.EvaluatePlayerPreFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtPreFlop();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnFlop() 
    {
        Table.gameState = GameState.Flop;
        ChipCount = Table.instance.GetChipStack(SeatPos);
        List<CardType> sortedCards = Table.instance.EvaluatePlayerAtFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtFlop();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnTurn() 
    {
        Table.gameState = GameState.Turn;
        ChipCount = Table.instance.GetChipStack(SeatPos);
        List<CardType> sortedCards = Table.instance.EvaluatePlayerAtTurn(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtTurn();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnRiver() 
    {
        Table.gameState = GameState.River;
        ChipCount = Table.instance.GetChipStack(SeatPos);
        List<CardType> sortedCards = Table.instance.EvaluatePlayerAtRiver(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtRiver();
        Hand = playerHand;
        Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void FlipCards()
    {
        List<GameObject> cardsInHand = Table.instance.GetCardObjects(SeatPos);
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if(cardsInHand[i].GetComponent<Card>().cardIsFlipped == false)
            {
                Services.GameManager.StartCoroutine(FlipTime(.5f, cardsInHand[i], (GameObject.Find("TheBoard").transform.position + cardsInHand[i].transform.position) / 2));
            }
        }
    }

    IEnumerator FlipTime(float duration, GameObject card, Vector3 targetPos)
    {
        float timeElapsed = 0;
        Vector3 initialPos = card.transform.position;
        Quaternion initialRot = card.transform.rotation;
        float targetYRot = Mathf.Atan2(targetPos.x - initialPos.x, targetPos.z - initialPos.z) * Mathf.Rad2Deg;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            card.transform.rotation = Quaternion.Lerp(initialRot, Quaternion.Euler(90, targetYRot, initialRot.eulerAngles.z), timeElapsed / duration );
            card.transform.position = Vector3.Lerp(initialPos, targetPos, timeElapsed / duration);
            yield return null;
        }
    }

}

