using System.Collections;
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

    // Use this for initialization
    void Start() {
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
                if (Table.instance.board.Count == 3 && Table.instance.burn.Count == 1 && !checkedForCorrections && thrownCards.Count == 0)
                {
                    if (CardsAreFacingCorrectDirection())
                    {
                        StartCoroutine(CheckFlopMistakes(1));
                    }
                }
            }
            else if (Table.gameState == GameState.Flop)
            {
                if (Table.instance.board.Count == 4 && Table.instance.burn.Count == 2 && !checkedForCorrections && thrownCards.Count == 0)
                {
                    if (CardsAreFacingCorrectDirection())
                    {
                        Debug.Log("checking turn");
                        StartCoroutine(CheckTurnMistakes(1));
                    }
                }
            }
            else if (Table.gameState == GameState.Turn)
            {
                if (Table.instance.board.Count == 5 && Table.instance.burn.Count == 3 && !checkedForCorrections && thrownCards.Count == 0)
                {
                    if (CardsAreFacingCorrectDirection())
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
                if ((cardsPulled.Count - 1 == flopCards || Table.instance.board.Count == 3) && !checkedForCorrections && thrownCards.Count == 0)
                {
                    StartCoroutine(CheckFlopMistakes(1));
                }
            }
            else if (Table.gameState == GameState.Flop)
            {
                if ((cardsPulled.Count - 1 == turnCard || Table.instance.board.Count == 4) && !checkedForCorrections && thrownCards.Count == 0)
                {
                    Debug.Log("checking turn");
                    StartCoroutine(CheckTurnMistakes(1));
                }
            }
            else if (Table.gameState == GameState.Turn)
            {
                if ((cardsPulled.Count - 1 == riverCard || Table.instance.board.Count == 5) &&  !checkedForCorrections && thrownCards.Count == 0)
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

    IEnumerator CheckFlopMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        //Debug.Log("playersAtTableCount = " + Services.Dealer.PlayerAtTableCount());
        //Debug.Log("in the preflop, checking the flop: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and flopCard = " + flopCards);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        if (cardsPulled.Count - 1 > flopCards && !Services.Dealer.OutsideVR)
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
            }
        }
        else if (cardsPulled.Count - 1 == flopCards)
        {
            CorrectMistakes();
            Table.gameState = GameState.Flop;
            checkedForCorrections = false;
        }
        else
        {
            Table.gameState = GameState.Flop;
            checkedForCorrections = false;
        }
    }

    IEnumerator CheckTurnMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        //Debug.Log("in the flop, checking the turn: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and turnCard = " + turnCard);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        if (cardsPulled.Count - 1 > turnCard && !Services.Dealer.OutsideVR)
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
            }
        }
        else if (cardsPulled.Count - 1 == turnCard)
        {
            CorrectMistakes();
            Table.gameState = GameState.Turn;
            checkedForCorrections = false;
        }
        else
        {
            Table.gameState = GameState.Turn;
            checkedForCorrections = false;
        }
    }

    IEnumerator CheckRiverMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        //Debug.Log("in the turn, checking the river: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and riverCard = " + riverCard);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        if (cardsPulled.Count - 1 > riverCard && !Services.Dealer.OutsideVR)
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
            }
        }
        else if (cardsPulled.Count - 1 == riverCard)
        {
            CorrectMistakes();
            Table.gameState = GameState.River;
            checkedForCorrections = false;
        }
        else
        {
            Table.gameState = GameState.River;
            checkedForCorrections = false;
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
        // Debug.Log("CorrectingMistakes");
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
                //Debug.Log("player we're trying to check is + " + player);
                if (player.PlayerState == PlayerState.Playing)
                {
                    GameObject card = null;
                    Vector3 modPos = new Vector3(0, .025f, 0);
                    if (!player.playerIsAllIn)
                    {
                        card = GetCardObject(i);
                        if (card.GetComponent<Card>().CardIsFaceUp()) card.GetComponent<Card>().RotateCard();
                        card.GetComponent<Card>().InitializeLerp(player.cardPos[cardPos].transform.position);
                        StartCoroutine(card.GetComponent<Card>().LerpCardPos(player.cardPos[cardPos].transform.position, speed));
                        StartCoroutine(card.GetComponent<Card>().LerpCardRot(card.GetComponent<Card>().GetCardRot(player), speed * 2));
                        StartCoroutine(CorrectionsDone(player.cardPos[cardPos].transform.position, card, playerDestinations[playerIndex], card.GetComponent<Card>()));
                    }
                    //Debug.Log("player we're trying to check is + " + player);
                    //Debug.Log("firstPlayer = " + Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count]);
                    //Debug.Log("cardCount after replacing" + Table.instance.playerCards[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].Count);
                }
            }
        }
    }

    public void CorrectMistakes()
    {
        // Debug.Log("CorrectingMistakes");
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        Table.instance.board.Clear();
        Table.instance.burn.Clear();

        for (int i = 0; i < cardsPulled.Count; i++)
        {
            if (i == burnCard1)
            {
                Vector3 burnPos = GameObject.Find("BurnCards").transform.position;
                Quaternion burnRot = GameObject.Find("BurnCards").transform.rotation;
                GameObject cardObj = GetCardObject(i);
                Card card = cardObj.GetComponent<Card>();
                if (card.CardIsFaceUp()) card.RotateCard();
                card.InitializeLerp(burnPos);
                StartCoroutine(card.LerpCardPos(burnPos, 1));
                StartCoroutine(card.LerpCardRot(burnRot, 1));
                StartCoroutine(CorrectionsDone(burnPos, cardObj, Destination.burn, card));
            }
            else if (i > burnCard1 && i <= flopCards)
            {
                if (i == burnCard1 + 1)
                {
                    Vector3 flopPos = boardPos[0].transform.position;
                    Quaternion flopRot = boardPos[0].transform.rotation;
                    GameObject cardObj = GetCardObject(i);
                    Card card = cardObj.GetComponent<Card>();
                    if (!card.CardIsFaceUp()) card.RotateCard();
                    card.InitializeLerp(flopPos);
                    StartCoroutine(card.LerpCardPos(flopPos, 1));
                    StartCoroutine(card.LerpCardRot(flopRot, 1));
                    StartCoroutine(CorrectionsDone(flopPos, cardObj, Destination.board, card));
                }
                else if (i == burnCard1 + 2)
                {
                    Vector3 flopPos = boardPos[1].transform.position;
                    Quaternion flopRot = boardPos[1].transform.rotation;
                    GameObject cardObj = GetCardObject(i);
                    Card card = cardObj.GetComponent<Card>();
                    if (!card.CardIsFaceUp()) card.RotateCard();
                    card.InitializeLerp(flopPos);
                    StartCoroutine(card.LerpCardPos(flopPos, 1));
                    StartCoroutine(card.LerpCardRot(flopRot, 1));
                    StartCoroutine(CorrectionsDone(flopPos, cardObj, Destination.board, card));
                }
                else if (i == burnCard1 + 3)
                {
                    Vector3 flopPos = boardPos[2].transform.position;
                    Quaternion flopRot = boardPos[2].transform.rotation;
                    GameObject cardObj = GetCardObject(i);
                    Card card = cardObj.GetComponent<Card>();
                    if (!card.CardIsFaceUp()) card.RotateCard();
                    card.InitializeLerp(flopPos);
                    StartCoroutine(card.LerpCardPos(flopPos, 1));
                    StartCoroutine(card.LerpCardRot(flopRot, 1));
                    StartCoroutine(CorrectionsDone(flopPos, cardObj, Destination.board, card));
                }
            }
            else if (i == burnCard2)
            {
                Vector3 burnPos = GameObject.Find("BurnCards").transform.position;
                Quaternion burnRot = GameObject.Find("BurnCards").transform.rotation;
                GameObject cardObj = GetCardObject(i);
                Card card = cardObj.GetComponent<Card>();
                if (card.CardIsFaceUp()) card.RotateCard();
                card.InitializeLerp(burnPos);
                StartCoroutine(card.LerpCardPos(burnPos, 1));
                StartCoroutine(card.LerpCardRot(burnRot, 1));
                StartCoroutine(CorrectionsDone(burnPos, cardObj, Destination.burn, card));
            }
            else if (i == turnCard)
            {
                Vector3 turnPos = boardPos[3].transform.position;
                Quaternion turnRot = boardPos[3].transform.rotation;
                GameObject cardObj = GetCardObject(i);
                Card card = cardObj.GetComponent<Card>();
                if (!card.CardIsFaceUp()) card.RotateCard();
                card.InitializeLerp(turnPos);
                StartCoroutine(card.LerpCardPos(turnPos, 1));
                StartCoroutine(card.LerpCardRot(turnRot, 1));
                StartCoroutine(CorrectionsDone(turnPos, cardObj, Destination.board, card));
            }
            else if (i == burnCard3)
            {
                Vector3 burnPos = GameObject.Find("BurnCards").transform.position;
                Quaternion burnRot = GameObject.Find("BurnCards").transform.rotation;
                GameObject cardObj = GetCardObject(i);
                Card card = cardObj.GetComponent<Card>();
                if (card.CardIsFaceUp()) card.RotateCard();
                card.InitializeLerp(burnPos);
                StartCoroutine(card.LerpCardPos(burnPos, 1));
                StartCoroutine(card.LerpCardRot(burnRot, 1));
                StartCoroutine(CorrectionsDone(burnPos, cardObj, Destination.burn, card));
            }
            else if (i == riverCard)
            {
                Vector3 riverPos = boardPos[4].transform.position;
                Quaternion riverRot = boardPos[4].transform.rotation;
                GameObject cardObj = GetCardObject(i);
                Card card = cardObj.GetComponent<Card>();
                if (!card.CardIsFaceUp()) card.RotateCard();
                card.InitializeLerp(riverPos);
                StartCoroutine(card.LerpCardPos(riverPos, 1));
                StartCoroutine(card.LerpCardRot(riverRot, 1));
                StartCoroutine(CorrectionsDone(riverPos, cardObj, Destination.board, card));
            }
        }
        SetCardIndicator();
    }

    IEnumerator CorrectionsDone(Vector3 pos, GameObject cardObj, Destination dest, Card card)
    {
        while (card.GetComponent<Card>().lerping)
        {
            if (card.transform.position == pos)
            {
                card.GetComponent<Card>().lerping = false;
                Table.instance.AddCardTo(dest, card);
                card.readyToFloat = true;
                Debug.Log("MADE IT");
            }
            else yield return null;
        }
        yield break;
    }

    public GameObject GetCardObject(int i)
    {
        GameObject cardHolder = null;
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        foreach (GameObject obj in cardsOnTable)
        {
            if (obj.GetComponent<Card>().cardType.suit == cardsPulled[i].suit && obj.GetComponent<Card>().cardType.rank == cardsPulled[i].rank)
            {
                cardHolder = obj;
            }
        }
        return cardHolder;
    }

    public bool CardIsInCorrectLocation(Card card, int cardCount)
    {
        card.cardChecked = true;
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        int playerIndex = Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardCount);
        Debug.Log("correct location for card is " + Services.Dealer.players[playerIndex].playerName);
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
        if (chipGroup.Count != 0) 
        for (int i = 0; i < chipGroup.Count; i++)
        {
            Rigidbody rb = chipGroup[i].gameObject.GetComponent<Rigidbody>();
            int chipSpotIndex = chipGroup[i].spotIndex;
            Vector3 dest = chipGroup[i].handPushingChip.GetAttachmentTransform("PushChip").transform.TransformPoint(chipPositionWhenPushing[chipSpotIndex]);
            if (rb != null)
            {
                rb.MovePosition(new Vector3(dest.x, chipGroup[i].gameObject.transform.position.y, dest.z));
            }
        }
    }

    public void ConsolidateStack()
    {
        if (chipGroup.Count > 1)
        {
            for (int i = 0; i < chipGroup.Count; i++)
            {
                Chip chip = chipGroup[i];
                if (chip.chipStack != null)
                {
                    for (int chipToCheck = 0; chipToCheck < chipGroup.Count; chipToCheck++)
                    {
                        if (chipToCheck != i)
                        {
                            if (chipGroup[chipToCheck].chipStack != null && chipGroup[chipToCheck].chipData.ChipValue == chip.chipData.ChipValue)
                            {
                                for (int chipsToAdd = 0; chipsToAdd < chip.chipStack.chips.Count; chipsToAdd++)
                                {
                                    chipGroup[chipToCheck].chipStack.AddToStackInHand(chip.chipStack.chips[chipsToAdd]);
                                }
                                chipGroup.Remove(chip);
                                Destroy(chip.gameObject);
                                break;
                            }
                        }
                    }
                }
                else if (chip.chipStack == null)
                {
                    for (int chipToCheck = 0; chipToCheck < chipGroup.Count; chipToCheck++)
                    {
                        if (chipToCheck != i)
                        {
                            if (chipGroup[chipToCheck].chipStack != null)
                            {
                                if (chipGroup[chipToCheck].chipData.ChipValue == chip.chipData.ChipValue)
                                {
                                    chipGroup[chipToCheck].chipStack.AddToStackInHand(chip.chipData);
                                    chipGroup.Remove(chip);
                                    Destroy(chip.gameObject);
                                    break;
                                }
                            }
                            else
                            {
                                chipGroup[chipToCheck].chipStack = new ChipStack(chipGroup[chipToCheck]);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < chipGroup.Count; i++)
            {
                chipGroup[i].spotIndex = i;
            }
        }
    }

    public void PlayTone()
    {
        if (toneCount < 0 || toneCount > Services.SoundManager.cardTones.Length - 1) toneCount = 0;
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
                        Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[toneCount + tonesToSkip], cardToneVolume);
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
}

