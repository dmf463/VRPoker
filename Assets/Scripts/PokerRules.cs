using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerRules : MonoBehaviour {

    //this keeps track of ALL the cards that have been dealt in a given hand
    //this way we won't use the same card twice for multiple things
    public List<CardType> cardsPulled = new List<CardType>();
    public List<Card> cardsLogged = new List<Card>();
    public GameObject[] cardsToDestroy;
    private List<Destination> playerDestinations = new List<Destination>();
    List<GameObject> boardPos = new List<GameObject>();
    int playerCards;
    int burnCard1;
    int burnCard2;
    int burnCard3;
    int flopCards;
    int turnCard;
    int riverCard;

    // Use this for initialization
    void Start () {
        boardPos.Add(GameObject.Find("Flop1"));
        boardPos.Add(GameObject.Find("Flop2"));
        boardPos.Add(GameObject.Find("Flop3"));
        boardPos.Add(GameObject.Find("Flop4"));
        boardPos.Add(GameObject.Find("Flop5"));
        for (int i = 0; i < boardPos.Count; i++)
        {
            Debug.Log("board pos " + i + " is " + boardPos[i].transform.position);
        }
        playerDestinations = Table.instance.playerDestinations;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //the issue we're currently having is that when it instantiates the new cards to replace the old ones
    //they instantiate somehow BEFORE the old cards are destroyed, so they aren't added to the player cards? I think? idfk
    //also, as of right now, it ONLY destroys the cards that were LOGGED.
    //but if the player supremely fucked up and didn't even hit their mark, those cards aren't destroyed...I guess I COULD add them to a different list...
    //adding them to a different list didn't solve the problem. time to take a breather
    public void CorrectMistakes()
    {
        Debug.Log("CorrectingMistakes");
        //we take away 1 to account for the 0th position in the list
        playerCards = (Services.Dealer.GetActivePlayerCount() * 2) - 1;
        burnCard1 = playerCards + 1;
        flopCards = playerCards + 4;
        burnCard2 = playerCards + 5;
        turnCard = playerCards + 6;
        burnCard3 = playerCards + 7;
        riverCard = playerCards + 8;

        cardsLogged.Clear();

        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            //List<Card> cardsToDestroy = new List<Card>();
            //for (int cardIndex = 0; cardIndex < Table.instance.playerCards[i].Count; cardIndex++)
            //{
            //    cardsToDestroy.Add(Table.instance.playerCards[i][cardIndex]);
            //}
            Table.instance.playerCards[i].Clear();
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

        for (int i = 0; i < cardsPulled.Count; i++)
        {
            if(i <= playerCards)
            {
                if (Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].cardsReplaced == 0)
                {
                    Debug.Log("i = " + i);
                    Debug.Log("firstPlayer = " + Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count]);
                    Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].cardsReplaced++;
                    Card newCard = CreateCard(cardsPulled[i], 
                        Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].cardPos[0].transform.position,
                        Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].cardPos[0].transform.rotation);
                    Table.instance.AddCardTo(playerDestinations[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count], newCard);
                    Debug.Log("cardCount after replacing" + Table.instance.playerCards[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].Count);
                }
                else
                {
                    Card newCard = CreateCard(cardsPulled[i], 
                        Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].cardPos[1].transform.position,
                        Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].cardPos[1].transform.rotation);
                    Table.instance.AddCardTo(playerDestinations[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count], newCard);
                    Services.Dealer.players[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].cardsReplaced = 0;
                    Debug.Log("cardCount after replacing" + Table.instance.playerCards[Services.Dealer.SeatsAwayFromDealer(i + 1) % playerDestinations.Count].Count);
                }   
            }
            else if(i == burnCard1)
            {
                GameObject burnPos = GameObject.Find("BurnCards");
                Card newCard = CreateCard(cardsPulled[i], burnPos.transform.position, burnPos.transform.rotation);
                Table.instance.AddCardTo(Destination.burn, newCard);
            }
            else if(i > burnCard1 && i <= flopCards)
            {
                if(i == burnCard1 + 1)
                {
                    Card newCard = CreateCard(cardsPulled[i], boardPos[0].transform.position, boardPos[0].transform.rotation);
                    Table.instance.AddCardTo(Destination.board, newCard);
                }
                else if(i == burnCard1 + 2)
                {
                    Card newCard = CreateCard(cardsPulled[i], boardPos[1].transform.position, boardPos[1].transform.rotation);
                    Table.instance.AddCardTo(Destination.board, newCard);
                }
                else if(i == burnCard1 + 3)
                {
                    Card newCard = CreateCard(cardsPulled[i], boardPos[2].transform.position, boardPos[2].transform.rotation);
                    Table.instance.AddCardTo(Destination.board, newCard);
                }
            }
            else if(i == burnCard2)
            {
                GameObject burnPos = GameObject.Find("BurnCards");
                Card newCard = CreateCard(cardsPulled[i], burnPos.transform.position, burnPos.transform.rotation);
                Table.instance.AddCardTo(Destination.board, newCard);
            }
            else if(i == turnCard)
            {
                Card newCard = CreateCard(cardsPulled[i], boardPos[3].transform.position, boardPos[3].transform.rotation);
                Table.instance.AddCardTo(Destination.board, newCard);
            }
            else if(i == burnCard3)
            {
                GameObject burnPos = GameObject.Find("BurnCards");
                Card newCard = CreateCard(cardsPulled[i], burnPos.transform.position, burnPos.transform.rotation);
                Table.instance.AddCardTo(Destination.board, newCard);
            }
            else if(i == riverCard)
            {
                Card newCard = CreateCard(cardsPulled[i], boardPos[4].transform.position, boardPos[4].transform.rotation);
                Table.instance.AddCardTo(Destination.board, newCard);
            }
        }
    }

    public Card CreateCard(CardType cardType, Vector3 position, Quaternion rotation)
    {
        GameObject playingCard = Instantiate(Services.PrefabDB.Card, position, rotation);
        playingCard.GetComponent<MeshFilter>().mesh = 
            GameObject.FindGameObjectWithTag("CardDeck").GetComponent<CardDeckScript>().cardMeshes[(int)cardType.suit][(int)cardType.rank - 2];
        playingCard.GetComponent<Card>().cardType = cardType;
        playingCard.gameObject.name = (cardType.rank + " of " + cardType.suit);
        playingCard.GetComponent<Card>().cardMarkedForDestruction = true;
        return playingCard.GetComponent<Card>();
    }
}
