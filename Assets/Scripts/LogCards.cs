using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCards : MonoBehaviour
{
    #region MonoBehaviour Stuff
    public float cardCount;
    private GameObject newCardDeck;
    private bool madeNewDeck;
    private int cardCountForTones;
    private List<string> playerNames = new List<string>
    {
        "P0Cards", "P1Cards", "P2Cards", "P3Cards", "P4Cards"
    };
    [HideInInspector]
    public List<Destination> playerDestinations = new List<Destination>
    {
        Destination.player0, Destination.player1, Destination.player2, Destination.player3, Destination.player4
    };

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion
    public void OnTriggerEnter(Collider other)
    {
        //increase the cardCount to see how many cards have hit the table
        cardCount += 1;
        #region Logging the PlayingCard for each space
        //so if it's a card
        if (other.gameObject.tag == "PlayingCard")
        {
            //we go through all the player names
            for (int i = 0; i < playerNames.Count; i++)
            {
                //when we get to the match, we know which place to put this into
                if (gameObject.name == playerNames[i] && Table.gameState == GameState.NewRound)
                {
                    if (Table.instance.playerCards[i].Contains(other.GetComponent<Card>()))
                    {
                        //Debug.Log(other.gameObject.name + " is already in play.");
                    }
                    //and the player does not have 2 cards already
                    else if (Table.instance.playerCards[i].Count == 2)
                    {
                        //Debug.Log(other.gameObject.name + " cannot be added to " + playerNames[i]);
                    }
                    //and the card has not already been dealt to somewhere else
                    else if (Services.PokerRules.cardsLogged.Contains(other.GetComponent<Card>()))
                    {
                        Debug.Log(other.gameObject.name + " has already been logged by something else");
                    }
                    else
                    {
                        Table.instance.AddCardTo(playerDestinations[i], other.GetComponent<Card>());
                        Services.PokerRules.cardsLogged.Add(other.GetComponent<Card>());
                        Services.PokerRules.PlayTone();
                        //Debug.Log(other.gameObject.name + " went into " + playerNames[i]);
                    }
                }

            }
            //if the card is going into TheBoard
            if (this.gameObject.name == "TheBoard" && Services.Dealer.playerToAct == null && 
                (Table.gameState != GameState.CleanUp ||
                 Table.gameState != GameState.PostHand ||
                 Table.gameState != GameState.NewRound))
            {
                //same thing as above
                //if (Table.dealerState == DealerState.DealingState)
                //{
                if (Table.instance._board.Contains(other.GetComponent<Card>()))
                {
                    //Debug.Log(other.gameObject.name + " is already in the board");
                }
                else if (Table.instance._board.Count == 5)
                {
                    //Debug.Log(other.gameObject.name + "cannot be added to the board");
                }
                else if (Services.PokerRules.cardsLogged.Contains(other.GetComponent<Card>()))
                {
                    //Debug.Log(other.gameObject.name + " is already logged");
                }
                else if (other.gameObject.GetComponent<Rigidbody>().isKinematic)
                {
                    //Debug.Log("is Kinematic");
                }
                else if (Services.Dealer.deadCardsList.Contains(other.GetComponent<Card>()))
                {
                    //Debug.Log(other.gameObject.name + "card is dead");
                }
                else
                {
                    Table.instance.AddCardTo(Destination.board, other.GetComponent<Card>());
                    Services.PokerRules.cardsLogged.Add(other.GetComponent<Card>());
                    //Debug.Log(other.gameObject.name + " went into " + this.gameObject.name);
                }
                //}

            }
            else if (this.gameObject.name == "BurnCards" && Services.Dealer.playerToAct == null &&
                (Table.gameState != GameState.CleanUp ||
                 Table.gameState != GameState.PostHand ||
                 Table.gameState != GameState.NewRound))
            {
                //if (Table.dealerState == DealerState.DealingState)
                //{
                if (Table.instance._burn.Contains(other.GetComponent<Card>()))
                {
                    //Debug.Log(other.gameObject.name + " is already in the burn");
                }
                else if (Services.PokerRules.cardsLogged.Contains(other.GetComponent<Card>()))
                {
                    //Debug.Log(other.gameObject.name + "card is already logged");
                }
                else if (Services.Dealer.deadCardsList.Contains(other.GetComponent<Card>()))
                {
                   // Debug.Log(other.gameObject.name + "card is dead");
                }
                else
                {
                    Table.instance.AddCardTo(Destination.burn, other.GetComponent<Card>());
                    Services.PokerRules.cardsLogged.Add(other.GetComponent<Card>());
                    //Debug.Log(other.gameObject.name + "Card went into " + this.gameObject.name);
                }
                //}
            }
            //else if (this.gameObject.name == "ShufflingArea")
            //{
            //    if (GameObject.FindGameObjectWithTag("CardDeck") == null)
            //    {
            //        //Debug.Log("Could not find CardDeck, instantiating new one");
            //        newCardDeck = Instantiate(Services.PrefabDB.CardDeck, transform.position, Quaternion.identity) as GameObject;
            //        newCardDeck.GetComponent<CardDeckScript>().BuildDeckFromOneCard(newCardDeck);
            //        madeNewDeck = true;
            //    }
            //    if (/*Table.dealerState == DealerState.ShufflingState &&*/ madeNewDeck == true)
            //    {
            //        Destroy(other.gameObject);
            //        //Debug.Log("destroying cards");
            //        newCardDeck.GetComponent<CardDeckScript>().MakeDeckLarger();
            //        if (newCardDeck.GetComponent<CardDeckScript>().currentCardDeckScale.y > newCardDeck.GetComponent<CardDeckScript>().newCardDeckScale.y)
            //        {
            //            madeNewDeck = false;
            //            GameObject[] deadCards = GameObject.FindGameObjectsWithTag("PlayingCard");
            //            foreach (GameObject card in deadCards)
            //            {
            //                Destroy(card);
            //            }
            //            Table.dealerState = DealerState.DealingState;
            //        }
            //    }
            //}

        }
        #endregion
    }
}