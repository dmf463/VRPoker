using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
 */

public class PokerPlayer {

    public int SeatPos { get; set; }
    public int ChipCount { get { return chipCount; } set { value = chipCount; } }
    private int chipCount
    {
        get { return Table.instance.GetChipStack(SeatPos); }
        set { }
    }
    public HandEvaluator Hand { get; set; }
    public float HandStrength;
    public PlayerState PlayerState { get; set; }
    public bool HasBeenPaid;
    public int ChipCountToCheckWhenWinning;
    public bool checkedHandStrength;

    public void EvaluateHandPreFlop() 
    {
        //Table.gameState = GameState.PreFlop;
        List<CardType> sortedCards = Table.instance.EvaluatePlayerPreFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtPreFlop();
        Hand = playerHand;
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnFlop() 
    {
        //Table.gameState = GameState.Flop;
        List<CardType> sortedCards = Table.instance.EvaluatePlayerAtFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtFlop();
        Hand = playerHand;
        if (!checkedHandStrength)
        {
            DetermineHandStrength(Hand.Cards[0], Hand.Cards[1]);
            Debug.Log("Player" + SeatPos + " has a HandStrength of " + HandStrength);
            checkedHandStrength = true;
        }
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnTurn() 
    {
        //Table.gameState = GameState.Turn;
        List<CardType> sortedCards = Table.instance.EvaluatePlayerAtTurn(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtTurn();
        Hand = playerHand;
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnRiver() 
    {
        //Table.gameState = GameState.River;
        List<CardType> sortedCards = Table.instance.EvaluatePlayerAtRiver(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtRiver();
        Hand = playerHand;
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void FlipCards()
    {
        List<GameObject> cardsInHand = Table.instance.GetCardObjects(SeatPos);
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].GetComponent<Card>().cardIsFlipped == false)
            {
                Physics.IgnoreCollision(cardsInHand[0].gameObject.GetComponent<Collider>(), cardsInHand[1].gameObject.GetComponent<Collider>());
                Services.GameManager.StartCoroutine(FlipCardsAndMoveTowardsBoard(.5f, cardsInHand[i], (GameObject.Find("TheBoard").GetComponent<Collider>().ClosestPointOnBounds(cardsInHand[i].transform.position) + cardsInHand[i].transform.position) / 2, SeatPos));
            }
            Services.GameManager.StartCoroutine(WaitForReposition(.5f, .5f, cardsInHand[0], cardsInHand[1], SeatPos));
        }
    }

    IEnumerator FlipCardsAndMoveTowardsBoard(float duration, GameObject card, Vector3 targetPos, int seatPos)
    {
        float timeElapsed = 0;
        Vector3 initialPos = card.transform.position;
        Quaternion initialRot = card.transform.rotation;
        float targetYRot = Mathf.Atan2(targetPos.x - initialPos.x, targetPos.z - initialPos.z) * Mathf.Rad2Deg;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            card.transform.rotation = Quaternion.Lerp(initialRot, Quaternion.Euler(90, targetYRot, initialRot.eulerAngles.z), timeElapsed / duration);
            card.transform.position = Vector3.Lerp(initialPos, targetPos, timeElapsed / duration);
            yield return null;
        }
    }

    IEnumerator WaitForReposition(float time, float duration, GameObject card1, GameObject card2, int seatPos)
    {
        yield return new WaitForSeconds(time);
        Services.GameManager.StartCoroutine(RepositionCardsForReadability(duration, card1, card2, seatPos));
    }

    IEnumerator RepositionCardsForReadability(float duration, GameObject card1, GameObject card2, int seatPos)
    {
        float timeElapsed = 0;
        float unitsToMove = 0.1f;
        Vector3 card1Pos = card1.transform.position;
        Vector3 card2Pos = card2.transform.position;
        float distanceBetweenCards = Vector3.Distance(card1Pos, card2Pos);
        if(distanceBetweenCards < 1)
        {
            if(seatPos != 2)
            {
                if (card1Pos.x - card2Pos.x > 0)
                {
                    while (timeElapsed < duration)
                    {
                        timeElapsed += Time.deltaTime;
                        card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x + unitsToMove, card2Pos.y, card2Pos.z), timeElapsed / duration);
                        yield return null;
                    }
                }
                else if (card1Pos.x - card2Pos.x < 0)
                {
                    while (timeElapsed < duration)
                    {
                        timeElapsed += Time.deltaTime;
                        card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x - unitsToMove, card2Pos.y, card2Pos.z), timeElapsed / duration);
                        yield return null;
                    }
                }
            }
            else
            {
                if (card1Pos.z - card2Pos.z > 0)
                {
                    while (timeElapsed < duration)
                    {
                        timeElapsed += Time.deltaTime;
                        card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x, card2Pos.y, card2Pos.z + unitsToMove), timeElapsed / duration);
                        yield return null;
                    }
                }
                else if (card1Pos.z - card2Pos.z < 0)
                {
                    while (timeElapsed < duration)
                    {
                        timeElapsed += Time.deltaTime;
                        card1.transform.position = Vector3.Lerp(card1Pos, new Vector3(card2Pos.x, card2Pos.y, card2Pos.z - unitsToMove), timeElapsed / duration);
                        yield return null;
                    }
                }
            }
        }

    }

    public void DetermineHandStrength(CardType myCard1, CardType myCard2)
    {
        float numberOfWins = 0;
        List<CardType> testDeck = new List<CardType>();
        List<PokerPlayer> testPlayers = new List<PokerPlayer>();
        List<CardType> testBoard = new List<CardType>();
        //PokerPlayer fakeSelf = new PokerPlayer();
        List<List<CardType>> playerCards = new List<List<CardType>>()
            {
                new List<CardType>(), new List<CardType>(), new List<CardType>(), new List<CardType>(), new List<CardType>()
            };
        for (int f = 0; f < 1000; f++)
        {
            testDeck.Clear();
            testPlayers.Clear();
            testBoard.Clear();
            foreach(List<CardType> cardList in playerCards)
            {
                cardList.Clear();
            } 
            //fakeSelf.SeatPos = SeatPos;
            #region populatingTheDeck
            SuitType[] suits = new SuitType[4]
            {
            SuitType.Spades,
            SuitType.Hearts,
            SuitType.Diamonds,
            SuitType.Clubs
            };
            RankType[] ranks = new RankType[13]
            {
            RankType.Two,
            RankType.Three,
            RankType.Four,
            RankType.Five,
            RankType.Six,
            RankType.Seven,
            RankType.Eight,
            RankType.Nine,
            RankType.Ten,
            RankType.Jack,
            RankType.Queen,
            RankType.King,
            RankType.Ace
            };

            foreach (SuitType suit in suits)
            {
                foreach (RankType rank in ranks)
                {
                    testDeck.Add(new CardType(rank, suit));
                }
            }
            #endregion
            testDeck.Remove(myCard1);
            testDeck.Remove(myCard2);
            //testPlayers.Add(fakeSelf);
            foreach (Card boardCard in Table.instance._board)
            {
                testDeck.Remove(boardCard.cardType);
                testBoard.Add(boardCard.cardType);
            }
            for (int i = 0; i < Services.GameManager.players.Count; i++)
            {
                testPlayers.Add(new PokerPlayer());
            }
            testPlayers[0].SeatPos = SeatPos;
            playerCards[0].Add(myCard1);
            playerCards[0].Add(myCard2);
            for (int i = 1; i < testPlayers.Count; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int cardPos = Random.Range(0, testDeck.Count);
                    CardType cardType = testDeck[cardPos];
                    playerCards[i].Add(cardType);
                    testDeck.Remove(cardType);
                }
                testPlayers[i].SeatPos = i;
            }
            if (Table.instance._board.Count == 3)
            {
                for (int i = 0; i < 2; i++)
                {
                    int cardPos = Random.Range(0, testDeck.Count);
                    CardType cardType = testDeck[cardPos];
                    testDeck.Remove(cardType);
                    testBoard.Add(cardType);
                }
            }
            else if (Table.instance._board.Count == 4)
            {
                int cardPos = Random.Range(0, testDeck.Count);
                CardType cardType = testDeck[cardPos];
                testDeck.Remove(cardType);
                testBoard.Add(cardType);
            }

            for (int i = 0; i < playerCards.Count; i++)
            {
                playerCards[i].AddRange(testBoard);
                playerCards[i].Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
                HandEvaluator testHand = new HandEvaluator(playerCards[i]);
                testHand.EvaluateHandAtRiver();
                testPlayers[i].Hand = testHand;
            }
            Services.GameManager.EvaluatePlayersOnShowdown(testPlayers);
            if (testPlayers[0].PlayerState == PlayerState.Winner)
            {
                float numberOfTestWinners = 0;
                foreach (PokerPlayer player in testPlayers)
                {
                    if (player.PlayerState == PlayerState.Winner)
                    {
                        numberOfTestWinners++;
                    }
                    else
                    {
                        Debug.Log("losing player had a " + player.Hand.HandValues.PokerHand);
                    }
                }
                numberOfWins += (1 / numberOfTestWinners);
            }
        }
        Debug.Log("number of wins = " + numberOfWins);
        HandStrength = numberOfWins / 1000;
        Debug.Log("The real Player " + SeatPos + " is holding " + Hand.Cards.Count + "cards");
    }
    
}

