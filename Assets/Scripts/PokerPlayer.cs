using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PlayerState { Playing, NotPlaying, Winner, Loser}

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
    //public bool checkedHandStrength;
    public bool organizingChips = false;
    public int currentBet;
    public float rateOfReturn;
    public bool turnComplete;
    public bool actedThisRound;

    public PokerPlayer(int seatPos)
    {
        SeatPos = seatPos;
        PlayerState = PlayerState.Playing;
    }

    public void Fold()
    {
        foreach(Card card in Table.instance.playerCards[SeatPos])
        {
            card.transform.position = Table.instance.playerBetZones[SeatPos].transform.position;
        }
        PlayerState = PlayerState.NotPlaying;
        Hand = null;
        int activePlayerNum = 0;
        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            if(Services.Dealer.players[i].PlayerState == PlayerState.Playing)
            {
                activePlayerNum++;
            }
        }
        if(activePlayerNum == 1)
        {
            for (int i = 0; i < Services.Dealer.players.Count; i++)
            {
                if(Services.Dealer.players[i].PlayerState == PlayerState.Playing)
                {
                    Services.Dealer.players[i].PlayerState = PlayerState.Winner;
                    Services.Dealer.numberOfWinners = 1;
                    Services.Dealer.StartCoroutine(Services.Dealer.WaitForWinnersToGetPaid());
                }
            }
        }
    }

    public void Call()
    {
        Debug.Log("currentBet = " + currentBet);
        int betToCall = Services.Dealer.LastBet - currentBet;
        Debug.Log("betToCall = " + betToCall);
        Bet(betToCall);
        currentBet = betToCall + currentBet;
        Services.Dealer.LastBet = currentBet;
    }

    public void Raise()
    {
        //for purposes of testing, we're gonna make this a limit game.
        int raiseAmount = Services.Dealer.BigBlind;
        int betToRaise = Services.Dealer.LastBet + (raiseAmount - currentBet);
        Bet(betToRaise);
        currentBet = betToRaise + currentBet;
        Services.Dealer.LastBet = currentBet;
    }

    public float FindRateOfReturn()
    {
        //in this case, since we're going to do limit, the bet will always be the bigBlind;
        //else, we need to write another function that determines what the possible bet will be
        int betAmount = Services.Dealer.BigBlind;
        int potSize = Table.instance.PotChips;
        float potOdds = betAmount / (betAmount + potSize);
        float returnRate = HandStrength / potOdds;
        return returnRate;
    }

    public void FoldCallRaiseDecision(float returnRate)
    {
        if(returnRate < 1)
        {
            Call();
        }
        else if(returnRate > 1 && returnRate < 1.3)
        {
            Call();
        }
        else if(returnRate > 1.3)
        {
            //Raise();
            Call();
        }
        turnComplete = true;
        actedThisRound = true;
    }

    public void EvaluateHand() 
    {
        if(PlayerState == PlayerState.Playing)
        {
            if (Table.gameState == GameState.PreFlop)
            {
                List<CardType> sortedCards = Table.instance.SortPlayerCardsPreFlop(SeatPos);
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtPreFlop();
                Hand = playerHand;
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                //CreateAndOrganizeChipStacks(Table.instance.GetChipGameObjects(SeatPos));
                //checkedHandStrength = true;
            }
            else if (Table.gameState == GameState.Flop)
            {
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtFlop(SeatPos);
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtFlop();
                Hand = playerHand;
                //if (!checkedHandStrength)
                //{
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                //CreateAndOrganizeChipStacks(Table.instance.GetChipGameObjects(SeatPos));
                //checkedHandStrength = true;
                //}
            }
            else if (Table.gameState == GameState.Turn)
            {
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtTurn(SeatPos);
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtTurn();
                Hand = playerHand;
                //if (!checkedHandStrength)
                //{
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
               // CreateAndOrganizeChipStacks(Table.instance.GetChipGameObjects(SeatPos));
                //checkedHandStrength = true;
                //}
            }
            else if (Table.gameState == GameState.River)
            {
                List<CardType> sortedCards = Table.instance.SortPlayerCardsAtRiver(SeatPos);
                HandEvaluator playerHand = new HandEvaluator(sortedCards);
                playerHand.EvaluateHandAtRiver();
                Hand = playerHand;
                //if (!checkedHandStrength)
                //{
                DetermineHandStrength(Table.instance.playerCards[SeatPos][0].cardType, Table.instance.playerCards[SeatPos][1].cardType);
                //CreateAndOrganizeChipStacks(Table.instance.GetChipGameObjects(SeatPos));
                //    checkedHandStrength = true;
                //}
            }
        }
    }

    public void FlipCards()
    {
        if(PlayerState == PlayerState.Playing)
        {
            List<GameObject> cardsInHand = Table.instance.GetCardGameObjects(SeatPos);
            for (int i = 0; i < cardsInHand.Count; i++)
            {
                if (cardsInHand[i].GetComponent<Card>().cardIsFlipped == false)
                {
                    Physics.IgnoreCollision(cardsInHand[0].gameObject.GetComponent<Collider>(), cardsInHand[1].gameObject.GetComponent<Collider>());
                    Services.Dealer.StartCoroutine(FlipCardsAndMoveTowardsBoard(.5f, cardsInHand[i], (GameObject.Find("TheBoard").GetComponent<Collider>().ClosestPointOnBounds(cardsInHand[i].transform.position) + cardsInHand[i].transform.position) / 2, SeatPos));
                }
                Services.Dealer.StartCoroutine(WaitForReposition(.5f, .5f, cardsInHand[0], cardsInHand[1], SeatPos));
            }
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
        Services.Dealer.StartCoroutine(RepositionCardsForReadability(duration, card1, card2, seatPos));
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
        if(Table.gameState == GameState.PreFlop)
        {
            DeterminePreFlopHandStrength(myCard1, myCard2);
        }
        else
        {
            Services.Dealer.StartCoroutine(RunHandStrengthLoopAfterFlop(myCard1, myCard2));
        }
    }

    public void DeterminePreFlopHandStrength(CardType myCard1, CardType myCard2)
    {
        //public enum PokerHand { Connectors, SuitedConnectors, HighCard, OnePair, TwoPair, ThreeOfKind, Straight, Flush, FullHouse, FourOfKind, StraightFlush }
        if (Hand.HandValues.PokerHand == PokerHand.HighCard)
        {
            HandStrength = Hand.HandValues.Total;
        }
        if(Hand.HandValues.PokerHand == PokerHand.Connectors)
        {
            HandStrength = Hand.HandValues.Total + 10;
        }
        if(Hand.HandValues.PokerHand == PokerHand.SuitedConnectors)
        {
            HandStrength = Hand.HandValues.Total + 15;
        }
        if(Hand.HandValues.PokerHand == PokerHand.OnePair)
        {
            HandStrength = Hand.HandValues.Total + 15;
        }
        rateOfReturn = FindRateOfReturn();
        FoldCallRaiseDecision(rateOfReturn);
    }

    IEnumerator RunHandStrengthLoopAfterFlop(CardType myCard1, CardType myCard2)
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
            new PokerPlayer(0), new PokerPlayer(1), new PokerPlayer(2), new PokerPlayer(3), new PokerPlayer(4),
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
                Services.Dealer.EvaluatePlayersOnShowdown(testPlayers);
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
        rateOfReturn = FindRateOfReturn();
        FoldCallRaiseDecision(rateOfReturn);
        yield break;
    }

    public List<List<GameObject>> OrganizeChipsIntoColorStacks(List<GameObject> chipsToOrganize)
    {
        List<List<GameObject>> colorChips = new List<List<GameObject>>();
        List<GameObject> redChips = new List<GameObject>();
        List<GameObject> blueChips = new List<GameObject>();
        List<GameObject> whiteChips = new List<GameObject>();
        List<GameObject> blackChips = new List<GameObject>();

        for (int chipIndex = 0; chipIndex < chipsToOrganize.Count; chipIndex++)
        {
            if(chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.RED_CHIP_VALUE)
            {
                redChips.Add(chipsToOrganize[chipIndex]);
            }
            else if(chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.BLUE_CHIP_VALUE)
            {
                blueChips.Add(chipsToOrganize[chipIndex]);
            }
            else if (chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.WHITE_CHIP_VALUE)
            {
                whiteChips.Add(chipsToOrganize[chipIndex]);
            }
            else if (chipsToOrganize[chipIndex].GetComponent<Chip>().chipValue == ChipConfig.BLACK_CHIP_VALUE)
            {
                blackChips.Add(chipsToOrganize[chipIndex]);
            }
        }
        colorChips.Add(redChips);
        colorChips.Add(blueChips);
        colorChips.Add(whiteChips);
        colorChips.Add(blackChips);
        return colorChips;
    }

    public void CreateAndOrganizeChipStacks(List<GameObject> chipsToOrganize )
    {
        organizingChips = true;
        List<List<GameObject>> organizedChips = OrganizeChipsIntoColorStacks(chipsToOrganize);
        GameObject parentChip = null;
        float incrementStackBy = 0;
        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("Player0"), GameObject.Find("Player1"), GameObject.Find("Player2"), GameObject.Find("Player3"), GameObject.Find("Player4")
        };
        Vector3 offSet = Vector3.zero;
        Vector3 containerOffset = Vector3.up * .05f;
        GameObject chipContainer = GameObject.Instantiate(new GameObject(), playerPositions[SeatPos].transform.position + containerOffset, playerPositions[SeatPos].transform.rotation);
        chipContainer.transform.rotation = Quaternion.Euler(0, chipContainer.transform.rotation.eulerAngles.y + 90, 0);
        Vector3 lastStackPos = Vector3.zero;
        Vector3 firstStackPos = Vector3.zero;
        for (int chipStacks = 0; chipStacks < organizedChips.Count; chipStacks++)
        {
            #region commments
            //organize the chip into stacks
            //these should be stacks in which the chips are on top of each other
            //parented to the first chip, so that I grab a stack
            //so first thing I need to do is assign a chip to make the parent.
            //
            //
            //now that they chips stack...how about stacking them into colors...
            /*
             * first off, let's divide the chips into lists of their values
             * then run the for loop for each list of chips.
             * so, the final version should look like
             * if it's the first chip in the stack
             *      make that chip the parent chip
             *      set the amount to increment the stack by
             *      set the position of the object to the center of the playerspace
             * else, add each chip on top of the parent chip
             * if a parents child count > 20
             * set the new parent to the next chip (unless it's null)
             */
            #endregion
            if (organizedChips[chipStacks].Count != 0)
            {
                for (int chipIndex = 0; chipIndex < organizedChips[chipStacks].Count; chipIndex++)
                {
                    if (chipIndex == 0)
                    {
                        parentChip = organizedChips[chipStacks][0];
                        parentChip.transform.parent = chipContainer.transform;
                        parentChip.transform.rotation = Quaternion.Euler(-90, 0, 0);
                        parentChip.GetComponent<Chip>().chipStack = new ChipStack(parentChip.GetComponent<Chip>());
                        if(parentChip.GetComponent<Rigidbody>() == null)
                        {
                            parentChip.AddComponent<Rigidbody>();
                        }
                        incrementStackBy = parentChip.gameObject.GetComponent<Collider>().bounds.size.y;
                        parentChip.transform.localPosition = offSet;
                        offSet += new Vector3(parentChip.GetComponent<Collider>().bounds.size.x + .01f, 0, 0);
                        if(firstStackPos == Vector3.zero)
                        {
                            firstStackPos = parentChip.transform.position;
                        }
                        lastStackPos = parentChip.transform.position;
                    }
                    else
                    {
                        if(organizedChips[chipStacks][chipIndex].GetComponent<Rigidbody>() != null)
                        {
                            GameObject.Destroy(organizedChips[chipStacks][chipIndex].GetComponent<Rigidbody>());
                        }
                        organizedChips[chipStacks][chipIndex].transform.parent = parentChip.transform;
                        organizedChips[chipStacks][chipIndex].transform.position = new Vector3(parentChip.transform.position.x, parentChip.transform.position.y - (incrementStackBy * chipIndex), parentChip.transform.position.z);
                        organizedChips[chipStacks][chipIndex].transform.rotation = parentChip.transform.rotation;
                        parentChip.GetComponent<Chip>().chipStack.chips.Add(organizedChips[chipStacks][chipIndex].GetComponent<Chip>());
                    }
                }
            }
        }
        Vector3 trueOffset = firstStackPos - lastStackPos;
        chipContainer.transform.position += trueOffset / 2;
        if(parentChip != null)
        Services.Dealer.StartCoroutine(ResetOrganizingChipsBool(2));
    }

    IEnumerator ResetOrganizingChipsBool(float time)
    {
        yield return new WaitForSeconds(time);
        organizingChips = false;
    }

    public List<GameObject> SetChipStacks(int chipAmount)
    {

        List<GameObject> startingStack = new List<GameObject>();

        List<GameObject> playerPositions = new List<GameObject>
        {
            GameObject.Find("Player0"), GameObject.Find("Player1"), GameObject.Find("Player2"), GameObject.Find("Player3"), GameObject.Find("Player4")
        };

        int valueRemaining = chipAmount;
        int blackChipCount = 0;
        int whiteChipCount = 0;
        int blueChipCount = 0;
        int redChipCount = 0;

        int blackChipCountMAX = 15;
        int whiteChipCountMAX = 25;
        int blueChipCountMAX = 25;

        blackChipCount = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
        valueRemaining -= blackChipCount * ChipConfig.BLACK_CHIP_VALUE;

        whiteChipCount = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
        valueRemaining -= whiteChipCount * ChipConfig.WHITE_CHIP_VALUE;

        blueChipCount = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
        valueRemaining -= blueChipCount * ChipConfig.BLUE_CHIP_VALUE;

        redChipCount = valueRemaining / ChipConfig.RED_CHIP_VALUE;

        for (int i = 0; i < blackChipCount; i++)
        {
            GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.BLACK_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
            startingStack.Add(newChip);
        }
        for (int i = 0; i < whiteChipCount; i++)
        {
            GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.WHITE_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
            startingStack.Add(newChip);
        }
        for (int i = 0; i < blueChipCount; i++)
        {
            GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.BLUE_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
            startingStack.Add(newChip);
        }
        for (int i = 0; i < redChipCount; i++)
        {
            GameObject newChip = GameObject.Instantiate(FindChipPrefab(ChipConfig.RED_CHIP_VALUE), playerPositions[SeatPos].transform.position, Quaternion.Euler(-90, 0, 0));
            startingStack.Add(newChip);
        }

        return startingStack;
    }

    public void Bet(int betAmount)
    {
        int oldChipStackValue = ChipCount;
        List<GameObject> playerBetZones = new List<GameObject>
        {
            GameObject.Find("P0BetZone"), GameObject.Find("P1BetZone"), GameObject.Find("P2BetZone"), GameObject.Find("P3BetZone"), GameObject.Find("P4BetZone")
        };

        List<int> colorChipCount = new List<int>()
        {
            //blackChipCount, whiteChipCount, blueChipCount, redChipCount
            0, 0, 0, 0
        };

        List<int> chipPrefab = new List<int>
        {
            ChipConfig.BLACK_CHIP_VALUE, ChipConfig.WHITE_CHIP_VALUE, ChipConfig.BLUE_CHIP_VALUE, ChipConfig.RED_CHIP_VALUE
        };
        int valueRemaining = betAmount;

        int blackChipCountMAX = FindChipMax(ChipConfig.BLACK_CHIP_VALUE);
        int whiteChipCountMAX = FindChipMax(ChipConfig.WHITE_CHIP_VALUE);
        int blueChipCountMAX = FindChipMax(ChipConfig.BLUE_CHIP_VALUE);
        int redChipCountMAX = FindChipMax(ChipConfig.RED_CHIP_VALUE);

        colorChipCount[0] = Mathf.Min(blackChipCountMAX, valueRemaining / ChipConfig.BLACK_CHIP_VALUE);
        valueRemaining -= colorChipCount[0] * ChipConfig.BLACK_CHIP_VALUE;

        colorChipCount[1] = Mathf.Min(whiteChipCountMAX, valueRemaining / ChipConfig.WHITE_CHIP_VALUE);
        valueRemaining -= colorChipCount[1] * ChipConfig.WHITE_CHIP_VALUE;

        colorChipCount[2] = Mathf.Min(blueChipCountMAX, valueRemaining / ChipConfig.BLUE_CHIP_VALUE);
        valueRemaining -= colorChipCount[2] * ChipConfig.BLUE_CHIP_VALUE;

        colorChipCount[3] = Mathf.Min(redChipCountMAX, valueRemaining / ChipConfig.RED_CHIP_VALUE);
        valueRemaining -= colorChipCount[3] * ChipConfig.RED_CHIP_VALUE;

        if(valueRemaining > 0)
        {
            List<GameObject> chipChange = SetChipStacks(valueRemaining);
            for (int i = 0; i < chipChange.Count; i++)
            {
                if(chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.BLACK_CHIP_VALUE)
                {
                    colorChipCount[0]++;
                }
                else if(chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.WHITE_CHIP_VALUE)
                {
                    colorChipCount[1]++;
                }
                else if (chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.BLUE_CHIP_VALUE)
                {
                    colorChipCount[2]++;
                }
                else if (chipChange[i].GetComponent<Chip>().chipValue == ChipConfig.RED_CHIP_VALUE)
                {
                    colorChipCount[3]++;
                }
            }
            foreach(GameObject chip in chipChange)
            {
                GameObject.Destroy(chip);
            }

        }

        for (int colorListIndex = 0; colorListIndex < colorChipCount.Count; colorListIndex++)
        {
            for (int colorCount = 0; colorCount < colorChipCount[colorListIndex]; colorCount++)
            {
                Vector3 offSet = new Vector3(Random.Range(0, .03f), .1f, Random.Range(0, .03f));
                GameObject newChip = GameObject.Instantiate(FindChipPrefab(chipPrefab[colorListIndex]), playerBetZones[SeatPos].transform.position + offSet, Quaternion.Euler(-90, 0, 0));
                for (int tableChipIndex = 0; tableChipIndex < Table.instance.playerChipStacks[SeatPos].Count; tableChipIndex++)
                {
                    if (newChip.GetComponent<Chip>().chipValue == Table.instance.playerChipStacks[SeatPos][tableChipIndex].chipValue && valueRemaining == 0)
                    {
                        GameObject chipToRemove = Table.instance.playerChipStacks[SeatPos][tableChipIndex].gameObject;
                        Debug.Log("ChipRemoved was a " + chipToRemove.GetComponent<Chip>().chipValue + " chip");
                        Table.instance.playerChipStacks[SeatPos].Remove(chipToRemove.GetComponent<Chip>());
                        Table.instance._potChips.Add(chipToRemove.GetComponent<Chip>());
                        GameObject.Destroy(chipToRemove);
                        break;
                    }
                }
            }    
        }
        if (valueRemaining > 0)
        {
            int newChipStackValue = oldChipStackValue - betAmount;
            for (int i = Table.instance.playerChipStacks[SeatPos].Count - 1; i >= 0; i--)
            {
                GameObject chip = Table.instance.playerChipStacks[SeatPos][i].gameObject;
                Table.instance.playerChipStacks[SeatPos].Remove(chip.GetComponent<Chip>());
                GameObject.Destroy(chip);
            }
            List<GameObject> newChipStack = SetChipStacks(newChipStackValue);
            foreach(GameObject chip in newChipStack)
            {
                Table.instance.playerChipStacks[SeatPos].Add(chip.GetComponent<Chip>());
            }
            CreateAndOrganizeChipStacks(newChipStack);
        }
        else
        {
            CreateAndOrganizeChipStacks(Table.instance.GetChipGameObjects(SeatPos));
        }
    }

    public int FindChipMax(int chipValue)
    {
        int chipMax = 0;

        for (int i = 0; i < Table.instance.playerChipStacks[SeatPos].Count; i++)
        {
            if(chipValue == Table.instance.playerChipStacks[SeatPos][i].chipValue)
            {
                chipMax++;
            }
        }

        return chipMax;
    }

    public GameObject FindChipPrefab(int chipValue)
    {
        GameObject chipPrefab = null;
        switch (chipValue)
        {
            case ChipConfig.RED_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.RedChip;
                break;
            case ChipConfig.BLUE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlueChip;
                break;
            case ChipConfig.WHITE_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.WhiteChip;
                break;
            case ChipConfig.BLACK_CHIP_VALUE:
                chipPrefab = Services.PrefabDB.BlackChip;
                break;
            default:
                break;
        }
        return chipPrefab;
    }

}

