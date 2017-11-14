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

    // Use this for initialization
    void Start () {
        boardPos.Add(GameObject.Find("Flop1"));
        boardPos.Add(GameObject.Find("Flop2"));
        boardPos.Add(GameObject.Find("Flop3"));
        boardPos.Add(GameObject.Find("Flop4"));
        boardPos.Add(GameObject.Find("Flop5"));
        playerDestinations = Table.instance.playerDestinations;
        chipPositionWhenPushing = CreateChipPositions(chipPositionWhenPushing[0], 0.05f, 0.075f, 5, 25);
	}

    void Update()
    {
        if(chipGroup.Count > 0)
        {
            PushGroupOfChips();
        }
        if(cardsPulled.Count <= Services.Dealer.PlayerAtTableCount() * 2)
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
            if (cardsPulled.Count - 1 == turnCard || Table.instance._board.Count == 4)
            {
                SetCardPlacement(Services.Dealer.PlayerAtTableCount());
                if (Table.instance._board.Count + playerCards + 1 == turnCard)
                {
                    if (Table.instance._burn.Count < 2)
                    {
                        CorrectMistakes();
                    }
                }
                else if (cardsPulled.Count - 1 == turnCard)
                {
                    CorrectMistakes();
                    Table.gameState = GameState.Turn;
                }
                else Table.gameState = GameState.Turn;
            }
        }
        else if (Table.gameState == GameState.Turn)
        {
            if (cardsPulled.Count - 1 == riverCard || Table.instance._board.Count == 5)
            {
                SetCardPlacement(Services.Dealer.PlayerAtTableCount());
                if (Table.instance._board.Count + playerCards + 1 == riverCard)
                {
                    if (Table.instance._burn.Count < 3)
                    {
                        CorrectMistakes();
                    }
                }
                else if (cardsPulled.Count - 1 == riverCard)
                {
                    CorrectMistakes();
                    Table.gameState = GameState.River;
                }
                else Table.gameState = GameState.River;
            }
        }
    }

    IEnumerator CheckFlopMistakes(float time)
    {
        checkedForCorrections = true;
        yield return new WaitForSeconds(time);
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        if (Table.instance._board.Count + playerCards + 1 == flopCards)
        {
            Debug.Log("boardCount = " + Table.instance._board.Count);
            Debug.Log("CardsPulled.count = " + cardsPulled.Count);
            Debug.Log("playerCards = " + playerCards);
            Debug.Log("flopCards = " + flopCards);
            Debug.Log("burnCards = " + Table.instance._burn.Count);
            if (Table.instance._burn.Count < 1)
            {
                Debug.Log("CorrectingMistakes cause no burn");
                CorrectMistakes();
            }
        }
        else if (cardsPulled.Count - 1 == flopCards)
        {
            Debug.Log("correcting mistakes because cardsPulled - 1 != flopCards");
            CorrectMistakes();
            Table.gameState = GameState.Flop;
        }
        else Table.gameState = GameState.Flop;
    }

    public void SetCardIndicator()
    {
        SetCardPlacement(Services.Dealer.PlayerAtTableCount());
        if (Table.gameState == GameState.PreFlop)
        {
            if (cardsPulled.Count == burnCard1)
            {
                Behaviour newHalo = (Behaviour)cardIndicators[5].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else if (cardsPulled.Count == flopCards - 2)
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[5].GetComponent("Halo");
                oldHalo.enabled = false;

                Behaviour newHalo = (Behaviour)cardIndicators[0].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else if(cardsPulled.Count == flopCards - 1)
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[0].GetComponent("Halo");
                oldHalo.enabled = false;

                Behaviour newHalo = (Behaviour)cardIndicators[1].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else if (cardsPulled.Count == flopCards)
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[1].GetComponent("Halo");
                oldHalo.enabled = false;

                Behaviour newHalo = (Behaviour)cardIndicators[2].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[2].GetComponent("Halo");
                oldHalo.enabled = false;
            }

        }
        else if(Table.gameState == GameState.Flop)
        {
            if(cardsPulled.Count == burnCard2)
            {
                Behaviour newHalo = (Behaviour)cardIndicators[5].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else if(cardsPulled.Count == turnCard)
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[5].GetComponent("Halo");
                oldHalo.enabled = false;

                Behaviour newHalo = (Behaviour)cardIndicators[3].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[3].GetComponent("Halo");
                oldHalo.enabled = false;
            }
        }
        else if(Table.gameState == GameState.Turn)
        {
            if (cardsPulled.Count == burnCard3)
            {
                Behaviour newHalo = (Behaviour)cardIndicators[5].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else if (cardsPulled.Count == riverCard)
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[5].GetComponent("Halo");
                oldHalo.enabled = false;

                Behaviour newHalo = (Behaviour)cardIndicators[4].GetComponent("Halo");
                newHalo.enabled = true;
            }
            else
            {
                Behaviour oldHalo = (Behaviour)cardIndicators[4].GetComponent("Halo");
                oldHalo.enabled = false;
            }
        }
        else if(Table.gameState == GameState.River)
        {
            TurnOffAllIndicators();
        }
    }

    public void TurnOffAllIndicators()
    {
        for (int i = 0; i < cardIndicators.Length; i++)
        {
            Behaviour halo = (Behaviour)cardIndicators[i].GetComponent("Halo");
            halo.enabled = false;
        }
    }

    public void IndicateCardPlacement(int cardPlace)
    {
        if(cardsPulled.Count < Services.Dealer.PlayerAtTableCount() * 2)
        {
            Behaviour oldHalo = (Behaviour)Services.Dealer.players[Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardPlace)].playerCardIndicator.GetComponent("Halo");
            oldHalo.enabled = false;

            Behaviour newHalo = (Behaviour)Services.Dealer.players[Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardPlace + 1)].playerCardIndicator.GetComponent("Halo");
            newHalo.enabled = true;
        }
        else
        {
            Behaviour oldHalo = (Behaviour)Services.Dealer.players[Services.Dealer.SeatsAwayFromDealerAmongstLivePlayers(cardPlace)].playerCardIndicator.GetComponent("Halo");
            oldHalo.enabled = false;

        }
    }

    public void SetCardPlacement(int playerCount)
    {
        //we take away 1 to account for the 0th position in the list
        playerCards = (playerCount * 2) - 1;
        burnCard1 = playerCards + 1;
        flopCards = playerCards + 4;
        burnCard2 = playerCards + 5;
        turnCard = playerCards + 6;
        burnCard3 = playerCards + 7;
        riverCard = playerCards + 8;
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
                    if(!player.playerIsAllIn)
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
            if(rb != null)
            {
                rb.MovePosition(new Vector3(dest.x, chipGroup[i].gameObject.transform.position.y, dest.z));
                RigidbodyConstraints constraints = rb.constraints;
                constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
    }
}
