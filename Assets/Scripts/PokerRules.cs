using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PokerRules : MonoBehaviour {

    public List<Vector3> chipPositionWhenPushing;
    public int chipsBeingPushed;
    public List<Chip> chipGroup;
    //this keeps track of ALL the cards that have been dealt in a given hand
    //this way we won't use the same card twice for multiple things
    public List<CardType> cardsPulled = new List<CardType>();
    public List<Card> cardsLogged = new List<Card>();
    public List<CardType> misdealtCards = new List<CardType>();
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
    bool checkedForCorrections;
    private int toneCount;

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
        if (cardsPulled.Count <= Services.Dealer.PlayerAtTableCount() * 2)
        {
            IndicateCardPlacement(cardsPulled.Count);
        }

        if (Table.gameState == GameState.PreFlop)
        {
            if ((cardsPulled.Count - 1 == flopCards || Table.instance._board.Count == 3) && !checkedForCorrections)
            {
                StartCoroutine(CheckFlopMistakes(1));
            }
        }
        else if (Table.gameState == GameState.Flop)
        {
            if ((cardsPulled.Count - 1 == turnCard || Table.instance._board.Count == 4) && !checkedForCorrections)
            {
                StartCoroutine(CheckTurnMistakes(1));
            }
        }
        else if (Table.gameState == GameState.Turn)
        {
            if ((cardsPulled.Count - 1 == riverCard || Table.instance._board.Count == 5) && !checkedForCorrections)
            {
                StartCoroutine(CheckRiverMistakes(1));
            }
        }

        if (Table.gameState != GameState.NewRound) toneCount = 0;
    }

    IEnumerator CheckFlopMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        bool misdeal = false;
        //Debug.Log("playersAtTableCount = " + Services.Dealer.PlayerAtTableCount());
        //Debug.Log("in the preflop, checking the flop: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and flopCard = " + flopCards);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        for (int i = 0; i < Table.instance._board.Count; i++)
        {
            if (!Services.Dealer.OutsideVR)
            {
                if (Table.instance._board[i].cardFacingUp)
                {
                    misdeal = false;
                }
                else misdeal = true;
            }
        }
        if (cardsPulled.Count - 1 > flopCards && !Services.Dealer.OutsideVR)
        {
            Debug.Log("MISDEAL ON THE FLOP");
            Table.gameState = GameState.Misdeal;
        }
        else if (misdeal)
        {
            Debug.Log("A card is facedown");
            Table.gameState = GameState.Misdeal;
        }
        else if (Table.instance._board.Count + playerCards + 1 == flopCards)
        {
            if (Table.instance._burn.Count < 1)
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
        bool misdeal = false;
        //Debug.Log("in the flop, checking the turn: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and turnCard = " + turnCard);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        for (int i = 0; i < Table.instance._board.Count; i++)
        {
            if (!Services.Dealer.OutsideVR)
            {
                if (Table.instance._board[i].cardFacingUp)
                {
                    misdeal = false;
                }
                else misdeal = true;
            }
        }
        if (cardsPulled.Count - 1 > turnCard && !Services.Dealer.OutsideVR)
        {
            Debug.Log("MISDEAL ON THE TURN");
            Table.gameState = GameState.Misdeal;
        }
        else if (misdeal)
        {
            Debug.Log("A card is facedown");

            Table.gameState = GameState.Misdeal;
        }
        else if (Table.instance._board.Count + playerCards + 2 == turnCard)
        {
            if (Table.instance._burn.Count < 2)
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
        bool misdeal = false;
        //Debug.Log("in the turn, checking the river: boardCound = " + Table.instance._board.Count + " and playerCards + 1 =  " + (playerCards + 1) + " and riverCard = " + riverCard);
        //Debug.Log("also, cardsPulled.count = " + cardsPulled.Count);
        for (int i = 0; i < Table.instance._board.Count; i++)
        {
            if (!Services.Dealer.OutsideVR)
            {
                if (!Services.Dealer.OutsideVR)
                {
                    if (Table.instance._board[i].cardFacingUp)
                    {
                        misdeal = false;
                    }
                    else misdeal = true;
                }
            }
        }
        if (cardsPulled.Count - 1 > riverCard && !Services.Dealer.OutsideVR)
        {
            Debug.Log("MISDEAL ON THE RIVER");
            Table.gameState = GameState.Misdeal;
        }
        else if (misdeal)
        {
            Debug.Log("A card is facedown");
            Table.gameState = GameState.Misdeal;
        }
        else if (Table.instance._board.Count + playerCards + 3 == riverCard)
        {
            if (Table.instance._burn.Count < 3)
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
            if (cardsPulled.Count == burnCard1)
            {
                cardIndicators[5].SetActive(true);
            }
            else if (cardsPulled.Count == flopCards - 2)
            {
                cardIndicators[5].SetActive(false);
                cardIndicators[0].SetActive(true);
            }
            else if (cardsPulled.Count == flopCards - 1)
            {

                cardIndicators[0].SetActive(false);
                cardIndicators[1].SetActive(true);
            }
            else if (cardsPulled.Count == flopCards)
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
            else if (cardsPulled.Count == turnCard)
            {
                cardIndicators[5].SetActive(false);
                cardIndicators[3].SetActive(true);
            }
            else
            {
                cardIndicators[3].SetActive(false);
            }
        }
        else if (Table.gameState == GameState.Turn)
        {
            if (cardsPulled.Count == burnCard3)
            {
                cardIndicators[5].SetActive(true);
            }
            else if (cardsPulled.Count == riverCard)
            {
                cardIndicators[5].SetActive(false);
                cardIndicators[4].SetActive(true);
            }
            else
            {
                cardIndicators[4].SetActive(false);
            }
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
        foreach (Card card in Table.instance._board)
        {
            Destroy(card.gameObject);
        }
        Table.instance._board.Clear();
        foreach (Card card in Table.instance._burn)
        {
            Destroy(card.gameObject);
        }
        Table.instance._burn.Clear();
    }

    public void CorrectMistakesPreFlop()
    {
        // Debug.Log("CorrectingMistakes");
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        cardsLogged.Clear();
        ClearAndDestroyAllLists();
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
                    if (!player.playerIsAllIn)
                    {
                        Card newCard = CreateCard(cardsPulled[i], player.cardPos[cardPos].transform.position, player.cardPos[cardPos].transform.rotation);
                        Table.instance.playerCards[player.SeatPos].Add(newCard);
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
        cardsLogged.Clear();
        ClearAndDestroyAllLists();
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
                    if (!player.playerIsAllIn)
                    {
                        Card newCard = CreateCard(cardsPulled[i], player.cardPos[cardPos].transform.position, player.cardPos[cardPos].transform.rotation);
                        Table.instance.playerCards[player.SeatPos].Add(newCard);
                    }
                    //Debug.Log("player we're trying to check is + " + player);
                    //Debug.Log("firstPlayer = " + Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count]);
                    //Debug.Log("cardCount after replacing" + Table.instance.playerCards[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].Count);
                }
            }
            else if (i == burnCard1)
            {
                GameObject burnPos = GameObject.Find("BurnCards");
                Card newCard = CreateCard(cardsPulled[i], burnPos.transform.position, burnPos.transform.rotation);
                Table.instance.AddCardTo(Destination.burn, newCard);
            }
            else if (i > burnCard1 && i <= flopCards)
            {
                if (i == burnCard1 + 1)
                {
                    Card newCard = CreateCard(cardsPulled[i], boardPos[0].transform.position, boardPos[0].transform.rotation);
                    Table.instance.AddCardTo(Destination.board, newCard);
                }
                else if (i == burnCard1 + 2)
                {
                    Card newCard = CreateCard(cardsPulled[i], boardPos[1].transform.position, boardPos[1].transform.rotation);
                    Table.instance.AddCardTo(Destination.board, newCard);
                }
                else if (i == burnCard1 + 3)
                {
                    Card newCard = CreateCard(cardsPulled[i], boardPos[2].transform.position, boardPos[2].transform.rotation);
                    Table.instance.AddCardTo(Destination.board, newCard);
                }
            }
            else if (i == burnCard2)
            {
                GameObject burnPos = GameObject.Find("BurnCards");
                Card newCard = CreateCard(cardsPulled[i], burnPos.transform.position, burnPos.transform.rotation);
                Table.instance.AddCardTo(Destination.burn, newCard);
            }
            else if (i == turnCard)
            {
                Card newCard = CreateCard(cardsPulled[i], boardPos[3].transform.position, boardPos[3].transform.rotation);
                Table.instance.AddCardTo(Destination.board, newCard);
            }
            else if (i == burnCard3)
            {
                GameObject burnPos = GameObject.Find("BurnCards");
                Card newCard = CreateCard(cardsPulled[i], burnPos.transform.position, burnPos.transform.rotation);
                Table.instance.AddCardTo(Destination.burn, newCard);
            }
            else if (i == riverCard)
            {
                Card newCard = CreateCard(cardsPulled[i], boardPos[4].transform.position, boardPos[4].transform.rotation);
                Table.instance.AddCardTo(Destination.board, newCard);
            }
        }
        SetCardIndicator();
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
                        Debug.Log("player " + player.SeatPos + " has this many cards: " + Table.instance.playerCards[player.SeatPos].Count);
                        Debug.Log("cardPos = " + cardPos);
                        Debug.Log("cardPos + 1  = " + (cardPos + 1));
                        Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[toneCount]);
                        toneCount++;
                        Debug.Log("toneCount = " + toneCount);
                    }
                    else
                    {
                        Debug.Log("resetting tone count");
                        toneCount = 0;
                    }
                }
            }
        }
    }
}

