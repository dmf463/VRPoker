﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PokerRules : MonoBehaviour {

    public Material tipMaterial;
    public List<Vector3> chipPositionWhenPushing;
    public int chipsBeingPushed;
    public List<Chip> chipGroup;
    public List<GameObject> thrownCards = new List<GameObject>();
    //this keeps track of ALL the cards that have been dealt in a given hand
    //this way we won't use the same card twice for multiple things
    public List<CardType> cardsPulled = new List<CardType>();
    public List<Card> cardsLogged = new List<Card>();
    public GameObject[] cardsToDestroy;
    private List<Destination> playerDestinations = new List<Destination>();
    List<GameObject> boardPos = new List<GameObject>();
    public GameObject[] cardIndicators;
    private int playerCards;
    private int burnCard1;
    private int burnCard2;
    private int burnCard3;
    private int flopCards;
    private int turnCard;
    private int riverCard;
    public bool checkedForCorrections;
    public int toneCount;
    public float cardToneVolume;

    public bool checkedPreFlop;
    public bool checkedFlop;
    public bool checkedTurn;
    public bool checkedRiver;
    private TaskManager tm;

    // Use this for initialization
    void Start() {
        tm = new TaskManager();
        boardPos.Add(GameObject.Find("Flop1"));
        boardPos.Add(GameObject.Find("Flop2"));
        boardPos.Add(GameObject.Find("Flop3"));
        boardPos.Add(GameObject.Find("Flop4"));
        boardPos.Add(GameObject.Find("Flop5"));
        playerDestinations = Table.instance.playerDestinations;
        chipPositionWhenPushing = CreateChipPositions(chipPositionWhenPushing[0], 0.06f, 0.075f, 5, 25);
    }

    void Update()
    {
        tm.Update();
        CheckCardCount();
        if (chipGroup.Count > 0)
        {
            PushGroupOfChips();
        }
        if (cardsPulled.Count <= Services.Dealer.PlayerAtTableCount() * 2 && thrownCards.Count == 0)
        {
            IndicateCardPlacement(cardsPulled.Count);
        }
        CheckCardPlacement();
    }

    public void CheckCardPlacement()
    {
        #region NormalChecking
        if (!Services.Dealer.OutsideVR)
        {
            if (Table.gameState == GameState.PreFlop)
            {
                if (Table.instance.board.Count == 3 && Table.instance.burn.Count == 1 && Services.Dealer.readyForCards && thrownCards.Count == 0)
                {
                    if (CardsAreFacingCorrectDirection() && !checkedForCorrections )
                    {
                        StartCoroutine(CheckFlopMistakes(1));
                    }
                }
            }
            else if (Table.gameState == GameState.Flop)
            {
                if (Table.instance.board.Count == 4 && Table.instance.burn.Count == 2 && Services.Dealer.readyForCards  && thrownCards.Count == 0)
                {
                    if (CardsAreFacingCorrectDirection() && !checkedForCorrections)
                    {
                        Debug.Log("checking turn");
                        StartCoroutine(CheckTurnMistakes(1));
                    }
                }
            }
            else if (Table.gameState == GameState.Turn)
            {
                if (Table.instance.board.Count == 5 && Table.instance.burn.Count == 3 && Services.Dealer.readyForCards && thrownCards.Count == 0)
                {
                    if (CardsAreFacingCorrectDirection() && !checkedForCorrections)
                    {
                        Debug.Log("checking river");
                        StartCoroutine(CheckRiverMistakes(1));
                    }
                }
            }
        }
        #endregion
        #region OutsideVR Checking
        else
        {
            if (Table.gameState == GameState.PreFlop)
            {
                if ((cardsPulled.Count - 1 == flopCards || Table.instance.board.Count == 3) && Services.Dealer.readyForCards && !checkedForCorrections && thrownCards.Count == 0)
                {
                    Debug.Log("checking flop");
                    StartCoroutine(CheckFlopMistakes(1));
                }
            }
            else if (Table.gameState == GameState.Flop)
            {
                if ((cardsPulled.Count - 1 == turnCard || Table.instance.board.Count == 4) && Services.Dealer.readyForCards && !checkedForCorrections && thrownCards.Count == 0)
                {
                    Debug.Log("checking turn");
                    StartCoroutine(CheckTurnMistakes(1));
                }
            }
            else if (Table.gameState == GameState.Turn)
            {
                if ((cardsPulled.Count - 1 == riverCard || Table.instance.board.Count == 5) && Services.Dealer.readyForCards && !checkedForCorrections && thrownCards.Count == 0)
                {
                    Debug.Log("checking river");
                    StartCoroutine(CheckRiverMistakes(1));
                }
            }
        }
        #endregion
    }

    public bool CardsAreFacingCorrectDirection()
    {
        return (BoardCardsAreFaceUp() && BurnCardsAreFaceDown());
    }
    public bool BoardCardsAreFaceUp()
    {
        for (int i = 0; i < Table.instance.board.Count; i++)
        {
            if (!Services.Dealer.OutsideVR)
            {
                if (!Table.instance.board[i].CardIsFaceUp())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool BurnCardsAreFaceDown()
    {
        for (int i = 0; i < Table.instance.burn.Count; i++)
        {
            if (!Services.Dealer.OutsideVR)
            {
                if (Table.instance.burn[i].CardIsFaceUp())
                {
                    return false;
                }
            }
        }
        return true;
    }
    public void CheckCardCount()
    {
        if (!Services.Dealer.OutsideVR)
        {
            if (Table.gameState > GameState.NewRound && Table.gameState < GameState.ShowDown)
            {
                SetCardPlacement(Services.Dealer.PlayerAtTableCount());
                if (Table.gameState == GameState.PreFlop)
                {
                    if (cardsPulled.Count - 1 > flopCards)
                    {
                        Debug.Log("misdeal here");
                        Table.gameState = GameState.Misdeal;
                    }
                }
                else if (Table.gameState == GameState.Flop)
                {
                    if (cardsPulled.Count - 1 > turnCard)
                    {
                        Debug.Log("misdeal here");
                        Table.gameState = GameState.Misdeal;
                    }
                }
                else if (Table.gameState == GameState.Turn || Table.gameState == GameState.River)
                {
                    if (cardsPulled.Count - 1 > riverCard)
                    {
                        Debug.Log("misdeal here");
                        Table.gameState = GameState.Misdeal;
                    }
                }
            }
        }
    }

    IEnumerator CheckFlopMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        //Debug.Log("playersAtTableCount = " + Services.Dealer.PlayerAtTableCount());
        //Debug.Log("in the preflop, checking the flop: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and flopCard = " + flopCards);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        if (cardsPulled.Count - 1 > flopCards)
        {
            Debug.Log("MISDEAL ON THE FLOP");
            Table.gameState = GameState.Misdeal;
        }
        else if (Table.instance.board.Count + playerCards + 1 == flopCards)
        {
            if (Table.instance.burn.Count < 1)
            {
                Debug.Log("CORRECTING MISTAKES BECAUSE OF BURN");
                CorrectMistakes();
                checkedForCorrections = false;
            }
            else
            {
                Table.gameState = GameState.Flop;
                checkedForCorrections = false;
                checkedFlop = true;
            }
        }
        else if (cardsPulled.Count - 1 == flopCards)
        {
            CorrectMistakes();
            Table.gameState = GameState.Flop;
            checkedForCorrections = false;
            checkedFlop = true;
        }
        else
        {
            Table.gameState = GameState.Flop;
            checkedForCorrections = false;
            checkedFlop = true;
        }
    }

    IEnumerator CheckTurnMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        //Debug.Log("in the flop, checking the turn: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and turnCard = " + turnCard);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        if (cardsPulled.Count - 1 > turnCard)
        {
            Debug.Log("MISDEAL ON THE TURN");
            Table.gameState = GameState.Misdeal;
        }
        else if (Table.instance.board.Count + playerCards + 2 == turnCard)
        {
            if (Table.instance.burn.Count < 2)
            {
                Debug.Log("CORRECTING MISTAKES BECAUSE OF BURN");
                CorrectMistakes();
                checkedForCorrections = false;
            }
            else
            {
                Table.gameState = GameState.Turn;
                checkedForCorrections = false;
                checkedTurn = true;
            }
        }
        else if (cardsPulled.Count - 1 == turnCard)
        {
            CorrectMistakes();
            Table.gameState = GameState.Turn;
            checkedForCorrections = false;
            checkedTurn = true;
        }
        else
        {
            Table.gameState = GameState.Turn;
            checkedForCorrections = false;
            checkedTurn = true;
        }
    }

    IEnumerator CheckRiverMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        //Debug.Log("in the turn, checking the river: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and riverCard = " + riverCard);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        if (cardsPulled.Count - 1 > riverCard)
        {
            Debug.Log("MISDEAL ON THE RIVER");
            Table.gameState = GameState.Misdeal;
        }
        else if (Table.instance.board.Count + playerCards + 3 == riverCard)
        {
            if (Table.instance.burn.Count < 3)
            {
                Debug.Log("CORRECTING MISTAKES BECAUSE OF BURN");
                CorrectMistakes();
                checkedForCorrections = false;
            }
            else
            {
                Table.gameState = GameState.River;
                checkedForCorrections = false;
                checkedRiver = true;
            }
        }
        else if (cardsPulled.Count - 1 == riverCard)
        {
            CorrectMistakes();
            Table.gameState = GameState.River;
            checkedForCorrections = false;
            checkedRiver = true;
        }
        else
        {
            Table.gameState = GameState.River;
            checkedForCorrections = false;
            checkedRiver = true;
        }
    }

    public void SetCardIndicator()
    {
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        if (Table.gameState == GameState.PreFlop)
        {
            //all of these used to be cardsPulled.count
            //I changed them to cardsLogged.Count in order to make sure the lights didn't progress unless you put a card there
            //changing it back to cardsPulled, but adding in another check for cardsLogged before we actually progress forward.

            if (cardsPulled.Count == burnCard1)
            {
                cardIndicators[5].SetActive(true);
            }
            else if (cardsLogged.Count == flopCards - 2)
            {
                cardIndicators[5].SetActive(false);
                cardIndicators[0].SetActive(true);
            }
            else if (cardsLogged.Count == flopCards - 1)
            {
                cardIndicators[0].SetActive(false);
                cardIndicators[1].SetActive(true);
            }
            else if (cardsLogged.Count == flopCards)
            {
                cardIndicators[1].SetActive(false);
                cardIndicators[2].SetActive(true);
            }
            else
            {
                cardIndicators[2].SetActive(false);
            }

        }
        else if (Table.gameState == GameState.Flop)
        {
            if (cardsPulled.Count == burnCard2)
            {
                cardIndicators[5].SetActive(true);
            }
            else if (cardsLogged.Count == turnCard)
            {
                cardIndicators[5].SetActive(false);
                cardIndicators[3].SetActive(true);
            }
            else cardIndicators[3].SetActive(false);
        }
        else if (Table.gameState == GameState.Turn)
        {
            if (cardsPulled.Count == burnCard3)
            {
                cardIndicators[5].SetActive(true);
            }
            else if (cardsLogged.Count == riverCard)
            {
                cardIndicators[5].SetActive(false);
                cardIndicators[4].SetActive(true);
            }
            else cardIndicators[4].SetActive(false);
        }
        else if (Table.gameState == GameState.River)
        {
            TurnOffAllIndicators();
        }
    }

    public void TurnOffAllIndicators()
    {
        for (int i = 0; i < cardIndicators.Length; i++)
        {
            cardIndicators[i].SetActive(false);
        }
        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            Services.Dealer.players[i].playerSpotlight.SetActive(false);
            Services.Dealer.players[i].playerCardIndicator.SetActive(false);
        }
    }

    public void IndicateCardPlacement(int cardPlace)
    {
        if (cardsPulled.Count < Services.Dealer.PlayerAtTableCount() * 2)
        {
            GameObject oldIndicator = Services.Dealer.players[Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardPlace)].playerCardIndicator;
            oldIndicator.SetActive(false);

            GameObject newIndicator = Services.Dealer.players[Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardPlace + 1)].playerCardIndicator;
            newIndicator.SetActive(true);
        }
        else
        {
            GameObject oldIndicator = Services.Dealer.players[Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardPlace)].playerCardIndicator;
            oldIndicator.SetActive(false);

        }
    }

    public void SetCardPlacement(int playerCount)
    {
        //we take away 1 to account for the 0th position in the list
        playerCards = (playerCount * 2) - 1; //this should be 9
        burnCard1 = playerCards + 1; //this should be 10
        flopCards = playerCards + 4; //this should be 13
        burnCard2 = playerCards + 5; //this should 14
        turnCard = playerCards + 6; //this should be 15
        burnCard3 = playerCards + 7; //this should be 16
        riverCard = playerCards + 8; //this should be 17
    }

    public void ClearAndDestroyAllLists()
    {
        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            if (!Services.Dealer.players[i].playerIsAllIn)
            {
                Table.instance.playerCards[i].Clear();
            }
            cardsToDestroy = GameObject.FindGameObjectsWithTag("PlayingCard");
            foreach (GameObject card in cardsToDestroy)
            {
                if (card.GetComponent<Card>().cardMarkedForDestruction)
                {
                    Destroy(card.gameObject);
                }
            }
        }
        foreach (Card card in Table.instance.board)
        {
            Destroy(card.gameObject);
        }
        Table.instance.board.Clear();
        foreach (Card card in Table.instance.burn)
        {
            Destroy(card.gameObject);
        }
        Table.instance.burn.Clear();
    }

    public void CorrectMistakesPreFlop(float speed)
    {
        Debug.Log("CorrectingMistakes");
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            if (!Services.Dealer.players[i].playerIsAllIn)
            {
                Table.instance.playerCards[i].Clear();
            }
        }
        int cardPos;

        for (int i = 0; i < cardsPulled.Count; i++)
        {
            if (i <= playerCards) //playerCards = 9
            {
                if (i >= (Services.Dealer.PlayerAtTableCount())) cardPos = 1;
                else cardPos = 0;
                int playerIndex = Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(i + 1);
                PokerPlayerRedux player = Services.Dealer.players[playerIndex];
                Debug.Log("player we're trying to check is + " + player);
                if (player.PlayerState == PlayerState.Playing)
                {
                    GameObject card = null;
                    Vector3 modPos = new Vector3(0, .025f, 0);
                    card = GetCardObject(i);
                    if (card.GetComponent<Card>().CardIsFaceUp()) card.GetComponent<Card>().RotateCard();
                    card.GetComponent<Card>().InitializeLerp(player.cardPos[cardPos].transform.position);
                    StartCoroutine(card.GetComponent<Card>().LerpCardPos(player.cardPos[cardPos].transform.position, speed));
                    StartCoroutine(card.GetComponent<Card>().LerpCardRot(card.GetComponent<Card>().GetCardRot(), speed * 2));
                    StartCoroutine(CorrectionsDone(player.cardPos[cardPos].transform.position, player.cardPos[cardPos].transform.rotation,
                                                   card, playerDestinations[playerIndex], card.GetComponent<Card>(), true));
                    //Debug.Log("player we're trying to check is + " + player);
                    //Debug.Log("firstPlayer = " + Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count]);
                    //Debug.Log("cardCount after replacing" + Table.instance.playerCards[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].Count);
                }
            }
        }
    }

    public void CorrectMistakes()
    {
        Debug.Log("Correcting mistakes");
        Table.instance.board.Clear();
        Table.instance.burn.Clear();
        for (int i = 0; i < cardsPulled.Count; i++)
        {
            PositionBoardAndBurnCards(i, 1, true);
        }
        SetCardIndicator();
    }

    public void PositionBoardAndBurnCards(int cardNum, float speed, bool correction)
    {
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        if(!Services.Dealer.OutsideVR) cardNum = cardNum - 1;
        if (cardNum == burnCard1)
        {
            FixCard(GameObject.Find("BurnCards").transform.position, 
                    GameObject.Find("BurnCards").transform.rotation, 
                    cardNum, speed, correction, Destination.burn);
        }
        else if (cardNum > burnCard1 && cardNum <= flopCards)
        {
            if (cardNum == burnCard1 + 1)
            {
                FixCard(boardPos[0].transform.position, 
                        boardPos[0].transform.rotation, 
                        cardNum, speed, correction, Destination.board);
            }
            else if (cardNum == burnCard1 + 2)
            {
                FixCard(boardPos[1].transform.position, 
                        boardPos[1].transform.rotation, 
                        cardNum, speed, correction, Destination.board);
            }
            else if (cardNum == burnCard1 + 3)
            {
                FixCard(boardPos[2].transform.position, 
                        boardPos[2].transform.rotation, 
                        cardNum, speed, correction, Destination.board);
            }
        }
        else if (cardNum == burnCard2)
        {
            FixCard(GameObject.Find("BurnCards").transform.position, 
                    GameObject.Find("BurnCards").transform.rotation, 
                    cardNum, speed, correction, Destination.burn);
        }
        else if (cardNum == turnCard)
        {
            FixCard(boardPos[3].transform.position, 
                    boardPos[3].transform.rotation, 
                    cardNum, speed, correction, Destination.board);
        }
        else if (cardNum == burnCard3)
        {;
            FixCard(GameObject.Find("BurnCards").transform.position, 
                    GameObject.Find("BurnCards").transform.rotation, 
                    cardNum, speed, correction, Destination.burn);
        }
        else if (cardNum == riverCard)
        {
            FixCard(boardPos[4].transform.position, 
                    boardPos[4].transform.rotation, 
                    cardNum, speed, correction, Destination.board);
        }
    }

    public void FixCard(Vector3 pos, Quaternion rot, int cardNum, float speed, bool correction, Destination dest)
    {
        Vector3 cardPos = pos;
        Quaternion cardRot = rot;
        GameObject cardObj = GetCardObject(cardNum);
        Card card = cardObj.GetComponent<Card>();
        card.InitializeLerp(cardPos);
        StartCoroutine(card.LerpCardPos(cardPos, speed));
        //StartCoroutine(card.LerpCardRot(cardRot, speed));
        LerpRotation lerpRot = new LerpRotation(cardObj, cardObj.transform.rotation, cardRot, .5f);
        tm.Do(lerpRot);
        StartCoroutine(CorrectionsDone(cardPos, cardRot, cardObj, dest, card, correction));
    }

    IEnumerator CorrectionsDone(Vector3 pos, Quaternion rot, GameObject cardObj, Destination dest, Card card, bool correction)
    {
        if (correction) Table.instance.AddCardTo(dest, card);
        while (card.GetComponent<Card>().lerping && Table.gameState != GameState.Misdeal)
        {
            if (card.transform.position == pos)
            {
                card.GetComponent<Card>().lerping = false;
                card.readyToFloat = true;
                Debug.Log("MADE IT");
            }
            else yield return null;
        }
        yield break;
    }

    public GameObject GetCardObject(int cardNum)
    {
        GameObject cardHolder = null;
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        foreach (GameObject obj in cardsOnTable)
        {
            if (obj.GetComponent<Card>().cardType.suit == cardsPulled[cardNum].suit && obj.GetComponent<Card>().cardType.rank == cardsPulled[cardNum].rank)
            {
                cardHolder = obj;
                Debug.Log("Grabbing the " + cardHolder.GetComponent<Card>().cardType.rank + " of " + cardHolder.GetComponent<Card>().cardType.suit);
            }
        }
        return cardHolder;
    }

    public bool CardIsInCorrectLocation(Card card, int cardCount)
    {
        card.cardChecked = true;
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        int playerIndex = Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardCount);
        //Debug.Log("correct location for card is " + Services.Dealer.players[playerIndex].playerName);
        return Table.instance.playerCards[playerIndex].Contains(card);
    }

    public PokerPlayerRedux CardOwner(int cardCount)
    {
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        int playerIndex = Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardCount);
        return Services.Dealer.players[playerIndex];
    }

    public Card CreateCard(CardType cardType, Vector3 position, Quaternion rotation)
    {
        GameObject playingCard = Instantiate(Services.PrefabDB.Card, position, rotation);
        playingCard.GetComponent<MeshFilter>().mesh =
            GameObject.FindGameObjectWithTag("CardDeck").GetComponent<CardDeckScript>().cardMeshes[(int)cardType.suit][(int)cardType.rank - 2];
        playingCard.GetComponent<Card>().cardType = cardType;
        playingCard.gameObject.name = (cardType.rank + " of " + cardType.suit);
        playingCard.GetComponent<Card>().cardMarkedForDestruction = true;
        cardsLogged.Add(playingCard.GetComponent<Card>());
        return playingCard.GetComponent<Card>();
    }

    private List<Vector3> CreateChipPositions(Vector3 startPosition, float xIncremenet, float zIncrement, int maxRowSize, int maxColumnSize)
    {
        List<Vector3> listOfPositions = new List<Vector3>();
        float xOffset;
        float zOffset;
        for (int i = 0; i < maxColumnSize; i++)
        {
            if (i % 2 == 0)
            {
                xOffset = (((i % maxRowSize) / 2) + 0.5f) * -xIncremenet;
            }
            else xOffset = (((i % maxRowSize) / 2) + 0.5f) * xIncremenet;

            zOffset = (i / maxRowSize) * zIncrement;

            listOfPositions.Add(new Vector3(startPosition.x + xOffset, 0, startPosition.z + zOffset));
        }

        return listOfPositions;
    }

    public void PushGroupOfChips()
    {
        Vector3 offset = new Vector3(0.1f, 0, 0);
        if (chipGroup.Count != 0)
            for (int i = 0; i < chipGroup.Count; i++)
            {
                Rigidbody rb = chipGroup[i].gameObject.GetComponent<Rigidbody>();
                int chipSpotIndex = chipGroup[i].spotIndex;
                float scalar = 1.1f;
                Vector3 startPos = chipGroup[i].chipPushStartPos + offset;
                Vector3 handPos = chipGroup[i].handPushingChip.transform.position;
                Vector3 linearDest = (handPos - startPos)/* * scalar*/;
                Vector3 dest = chipGroup[i].handPushingChip.GetAttachmentTransform("PushChip").transform.TransformPoint(chipPositionWhenPushing[chipSpotIndex]);
                if (rb != null)
                {
                    rb.MovePosition(new Vector3(dest.x + linearDest.x, chipGroup[i].gameObject.transform.position.y, dest.z + linearDest.z));
                }
            }
    }

    public void ConsolidateStack(List<Chip> chipsToConsolidate)
    {
        if (chipsToConsolidate.Count > 1)
        {
            for (int i = 0; i < chipsToConsolidate.Count; i++)
            {
                Chip chip = chipsToConsolidate[i];
                if (chip.chipStack != null && chip.chipStack.chips.Count < 10)
                {
                    for (int chipToCheck = 0; chipToCheck < chipsToConsolidate.Count; chipToCheck++)
                    {
                        if (chipToCheck != i)
                        {
                            if (chipsToConsolidate[chipToCheck].chipStack != null && chipsToConsolidate[chipToCheck].chipData.ChipValue == chip.chipData.ChipValue)
                            {
                                for (int chipsToAdd = 0; chipsToAdd < chip.chipStack.chips.Count; chipsToAdd++)
                                {
                                    chipsToConsolidate[chipToCheck].chipStack.AddToStackInHand(chip.chipStack.chips[chipsToAdd]);
                                }
                                chipsToConsolidate.Remove(chip);
                                Debug.Log("BUG HERE SOMETIMES");
                                //trying to destroy chips that already are destoryed, null reference
                                if(chip != null) Destroy(chip.gameObject);
                                break;
                            }
                        }
                    }
                }
                else if (chip.chipStack == null)
                {
                    for (int chipToCheck = 0; chipToCheck < chipsToConsolidate.Count; chipToCheck++)
                    {
                        if (chipToCheck != i)
                        {
                            if (chipsToConsolidate[chipToCheck].chipStack != null)
                            {
                                if (chipsToConsolidate[chipToCheck].chipData.ChipValue == chip.chipData.ChipValue && chipsToConsolidate[chipToCheck].chipStack.chips.Count < 10)
                                {
                                    chipsToConsolidate[chipToCheck].chipStack.AddToStackInHand(chip.chipData);
                                    chipsToConsolidate.Remove(chip);
                                    Destroy(chip.gameObject);
                                    break;
                                }
                            }
                            else
                            {
                                chipsToConsolidate[chipToCheck].chipStack = new ChipStack(chipsToConsolidate[chipToCheck]);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < chipsToConsolidate.Count; i++)
            {
                chipsToConsolidate[i].spotIndex = i;
            }
        }
    }

    public void PlayTone()
    {
        if (Table.gameState == GameState.NewRound)
        {
            SetCardPlacement(Services.Dealer.PlayerAtTableCount());
            int cardPos;

            if ((cardsPulled.Count - 1) <= playerCards) //playerCards = 9
            {
                if ((cardsPulled.Count - 1) >= (Services.Dealer.PlayerAtTableCount())) cardPos = 1;
                else cardPos = 0;
                int playerIndex = Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardsPulled.Count);
                PokerPlayerRedux player = Services.Dealer.players[playerIndex];
                //Debug.Log("player we're trying to check is + " + player);
                if (player.PlayerState == PlayerState.Playing)
                {
                    if (Table.instance.playerCards[player.SeatPos].Count == (cardPos + 1))
                    {
                        int tonesToSkip = (Services.Dealer.players.Count - Services.Dealer.PlayerAtTableCount()) * 2;
                        if (toneCount < 0 || toneCount > Services.SoundManager.cardTones.Length - 1) toneCount = 0;
                        Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[(toneCount + tonesToSkip)], cardToneVolume);
                        toneCount++;
                    }
                    else
                    {
                        toneCount = 0;
                    }
                }
            }
        }
        else if (Table.gameState == GameState.PreFlop)
        {
            if (Table.instance.burn.Count == 1 && Table.instance.board.Count == 0) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[6], cardToneVolume);
            else if (Table.instance.board.Count == 1 && Table.instance.burn.Count == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], cardToneVolume);
            else if (Table.instance.board.Count == 2 && Table.instance.burn.Count == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], cardToneVolume);
            else if (Table.instance.board.Count == 3 && Table.instance.burn.Count == 1) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[9], cardToneVolume);
            else
            {
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], cardToneVolume);
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], cardToneVolume);
            }
        }
        else if (Table.gameState == GameState.Flop)
        {
            if (Table.instance.burn.Count == 2 && Table.instance.board.Count == 3) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], cardToneVolume);
            else if (Table.instance.burn.Count == 2 && Table.instance.board.Count == 4) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[9], cardToneVolume);
            else
            {
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], cardToneVolume);
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], cardToneVolume);
            }
        }
        else if (Table.gameState == GameState.Turn)
        {
            if (Table.instance.burn.Count == 3 && Table.instance.board.Count == 4) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], cardToneVolume);
            else if (Table.instance.burn.Count == 3 && Table.instance.board.Count == 5) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[9], cardToneVolume);
            else
            {
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], cardToneVolume);
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], cardToneVolume);
            }
        }
        else if (Table.gameState == GameState.River)
        {
            toneCount = 0;
        }
    }

    public void ChangeHeight()
    {
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        for (int i = 0; i < cardsOnTable.Length; i++)
        {
            Card card1 = cardsOnTable[i].GetComponent<Card>();
            Card card2 = cardsOnTable[(i + 1) % cardsOnTable.Length].GetComponent<Card>();
            if (card1.foldedCards && card2.foldedCards)
            {
                if (Mathf.Approximately(card2.transform.position.y, card1.transform.position.y))
                {
                    Debug.Log("changing height");
                    card1.height += 0.01f;
                    card2.height -= 0.01f;
                }
            }
        }
    }
}

