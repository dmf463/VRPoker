using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerState { Playing, NoHand, Winner, Loser}

public class PokerPlayer {

    public int SeatPos { get; set; }
    public int ChipCount { get { return chipCount; } set { value = chipCount; } }
    private int chipCount
    {
        get { return Table.instance.GetChipStackTotal(SeatPos); }
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
        List<CardType> sortedCards = Table.instance.SortPlayerCardsPreFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtPreFlop();
        Hand = playerHand;
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnFlop() 
    {
        List<CardType> sortedCards = Table.instance.SortPlayerCardsAtFlop(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtFlop();
        Hand = playerHand;
        if (!checkedHandStrength)
        {
            DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
            CreateAndOrganizeChipStacks();
            checkedHandStrength = true;
        }
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnTurn() 
    {
        List<CardType> sortedCards = Table.instance.SortPlayerCardsAtTurn(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtTurn();
        Hand = playerHand;
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void EvaluateHandOnRiver() 
    {
        List<CardType> sortedCards = Table.instance.SortPlayerCardsAtRiver(SeatPos);
        HandEvaluator playerHand = new HandEvaluator(sortedCards);
        playerHand.EvaluateHandAtRiver();
        Hand = playerHand;
        //Debug.Log("player" + SeatPos + " has " + Hand.HandValues.PokerHand + " with a highCard of " + Hand.HandValues.HighCard + " and a handTotal of " + Hand.HandValues.Total + " a chipCount of " + ChipCount);
    }

    public void FlipCards()
    {
        List<GameObject> cardsInHand = Table.instance.GetCardGameObjects(SeatPos);
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
        Services.GameManager.StartCoroutine(RunHandStrengthLoop(myCard1, myCard2));
    }

    IEnumerator RunHandStrengthLoop(CardType myCard1, CardType myCard2)
    {

        //set up all my empty lists to use 
        List<CardType> testDeck = new List<CardType>();
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
        List<CardType> referenceDeck = new List<CardType>();
        referenceDeck.AddRange(testDeck);

        List<CardType> testBoard = new List<CardType>();
        List<PokerPlayer> testPlayers = new List<PokerPlayer>()
        {
            new PokerPlayer(), new PokerPlayer(), new PokerPlayer(), new PokerPlayer(), new PokerPlayer(),
        };
        List<List<CardType>> playerCards = new List<List<CardType>>()
        {
            new List<CardType>(), new List<CardType>(), new List<CardType>(), new List<CardType>(), new List<CardType>()
        };
        List<HandEvaluator> testEvaluators = new List<HandEvaluator>()
        {
            new HandEvaluator(), new HandEvaluator(), new HandEvaluator(), new HandEvaluator(), new HandEvaluator()
        };
        float numberOfWins = 0;
        float handStrengthTestLoops = 0;
        while (handStrengthTestLoops < 100)
        {
            #region 10x For-Loop for Hand Strength
            for (int f = 0; f < 10; f++)
            {
                //clear everything
                //clear each players hands
                foreach (PokerPlayer player in testPlayers)
                {
                    player.Hand = null;
                }
                //clear each players handEvaluators
                foreach (HandEvaluator eval in testEvaluators)
                {
                    eval.ResetHandEvaluator();
                }
                //clear the deck
                testDeck.Clear();
                //add the deck
                testDeck.AddRange(referenceDeck);
                Debug.Assert(testDeck.Count == 52);
                //clear the board
                testBoard.Clear();
                Debug.Assert(testBoard.Count == 0);
                //clear each players cardList
                foreach (List<CardType> cardList in playerCards)
                {
                    cardList.Clear();
                    Debug.Assert(cardList.Count == 0);
                }
                //Start simulating the game
                //remove my cards from the deck
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if(testDeck[i].rank == myCard1.rank)
                    {
                        if(testDeck[i].suit == myCard1.suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == myCard2.rank)
                    {
                        if (testDeck[i].suit == myCard2.suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                Debug.Assert(testDeck.Count == 50);
                //remove the cards on the board from the deck and then add them to the fake board.
                foreach (Card boardCard in Table.instance._board)
                {
                    testDeck.Remove(boardCard.cardType);
                    testBoard.Add(boardCard.cardType);
                }
                Debug.Assert(testBoard.Count == 3);
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if(testDeck[i].rank == Table.instance._board[0].cardType.rank)
                    {
                        if (testDeck[i].suit == Table.instance._board[0].cardType.suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == Table.instance._board[1].cardType.rank)
                    {
                        if (testDeck[i].suit == Table.instance._board[1].cardType.suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                for (int i = 0; i < testDeck.Count; i++)
                {
                    if (testDeck[i].rank == Table.instance._board[2].cardType.rank)
                    {
                        if (testDeck[i].suit == Table.instance._board[2].cardType.suit)
                        {
                            testDeck.RemoveAt(i);
                        }
                    }
                }
                Debug.Assert(testDeck.Count == 47);
                //set THIS as test player0
                testPlayers[0].SeatPos = SeatPos;
                playerCards[0].Add(myCard1);
                playerCards[0].Add(myCard2);
                //give two cards two each other testPlayer, and then remove those cards from the deck
                //also give them a seat number
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
                //if we're on the flop, deal out two more card to the board
                //and take those from the deck
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
                //if we're on the turn, only take out one more card from the deck to the board
                else if (Table.instance._board.Count == 4)
                {
                    int cardPos = Random.Range(0, testDeck.Count);
                    CardType cardType = testDeck[cardPos];
                    testDeck.Remove(cardType);
                    testBoard.Add(cardType);
                }
                //for each player, add the board cards
                //sort the hands
                //assign them an evaluator
                //set the evaluator
                //evaluate the hand
                //set the hand = to the evaluator
                for (int i = 0; i < playerCards.Count; i++)
                {
                    playerCards[i].AddRange(testBoard);
                    playerCards[i].Sort((cardLow, cardHigh) => cardLow.rank.CompareTo(cardHigh.rank));
                    HandEvaluator testHand = testEvaluators[i];
                    testHand.SetHandEvalutor(playerCards[i]);
                    testHand.EvaluateHandAtRiver();
                    testPlayers[i].Hand = testHand;
                }
                //compare all test players and find the winner
                Services.GameManager.EvaluatePlayersOnShowdown(testPlayers);
                //if testPlayer[0] (this player) wins, we notch up the win score
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
                            //Debug.Log("losing player had a " + player.Hand.HandValues.PokerHand);
                        }
                    }
                    numberOfWins += (1 / numberOfTestWinners);
                }
            }
            #endregion
            handStrengthTestLoops++;
            yield return null;
        }
        HandStrength = numberOfWins / 1000;
        Debug.Log("Player" + SeatPos + " has a HandStrength of " + HandStrength);
        yield break;
    }

    public void CreateAndOrganizeChipStacks()
    {
        List<GameObject> chipsToOrganize = Table.instance.GetChipGameObjects(SeatPos);
        GameObject parentChip = null;
        float incrementStackBy = 0;
        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("Player0"), GameObject.Find("Player1"), GameObject.Find("Player2"), GameObject.Find("Player3"), GameObject.Find("Player4")
        };
        for(int i = 0; i < chipsToOrganize.Count; i++)
        {
            //organize the chip into stacks
            //these should be stacks in which the chips are on top of each other
            //parented to the first chip, so that I grab a stack
            //so first thing I need to do is assign a chip to make the parent. 
            if(i == 0)
            {
                parentChip = chipsToOrganize[0];
                incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
                parentChip.transform.position = playerPositions[SeatPos].transform.position;
            }
            else
            {
                chipsToOrganize[i].transform.parent = parentChip.transform;
                chipsToOrganize[i].transform.position = new Vector3(parentChip.transform.position.x, parentChip.transform.position.y + incrementStackBy, parentChip.transform.position.z);
                chipsToOrganize[i].transform.rotation = parentChip.transform.rotation;
            }
        }
    }


}

