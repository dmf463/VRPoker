using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class LogCards : MonoBehaviour
{
    #region MonoBehaviour Stuff
    private GameObject newCardDeck;
    private bool madeNewDeck;
    private int cardCountForTones;
    private float cardPositionSpeed = 0.3f;
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
        #region Logging the PlayingCard for each space
        //so if it's a card
        if (other.gameObject.tag == "PlayingCard")
        {
            //we go through all the player names
            for (int i = 0; i < playerNames.Count; i++)
            {
                //when we get to the match, we know which place to put this into
                if (gameObject.name == playerNames[i] &&
                    Table.gameState == GameState.NewRound &&
                    Services.Dealer.players[i].PlayerState != PlayerState.Eliminated &&
                    !other.GetComponent<Card>().thrownWrong)
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
                    else if (CardIsOccupied(other.GetComponent<Card>()))
                    {
                        Debug.Log("card belongs to someone else");
                    }
                    else if (other.gameObject.GetComponent<Rigidbody>().isKinematic == true)
                    {
                        Debug.Log(other.gameObject.name + " hasn't been let go of yet");
                    }
                    else if (other.GetComponent<Card>().CardIsFaceUp())
                    {
                        Debug.Log("misdeal here");
                        Table.gameState = GameState.Misdeal;
                    }
                    else
                    {
                        if (!Services.Dealer.killingCards && !Services.Dealer.cleaningCards && Table.gameState == GameState.NewRound)
                        {

                            Table.instance.AddCardTo(playerDestinations[i], other.GetComponent<Card>());
                            //other.GetComponent<Card>().is_flying = false;
                            Services.PokerRules.cardsLogged.Add(other.GetComponent<Card>());
                            Services.PokerRules.PlayTone();
                            Debug.Log(other.gameObject.name + " went into " + playerNames[i]);
                            if (Services.Dealer.isCheating)
                            {
                                Services.PokerRules.cardsPulled.Add(other.GetComponent<Card>().cardType);
                                other.GetComponent<Card>().cardThrownNum = Services.PokerRules.cardsPulled.Count;
                                StartCoroutine(other.GetComponent<Card>().CheckIfCardIsAtDestination(.25f, other.GetComponent<Card>().cardThrownNum));
                            }
                            other.GetComponent<Card>().StopCheating();
                        }
                    }
                }
            }
            //if the card is going into TheBoard
            if (this.gameObject.name == "TheBoard" && Services.Dealer.playerToAct == null &&
                 Table.gameState != GameState.PostHand &&
                 Table.gameState != GameState.NewRound &&
                 Table.gameState != GameState.Misdeal &&
                 !Services.Dealer.OutsideVR)
            {
                if (Table.instance.board.Contains(other.GetComponent<Card>()))
                {
                    Debug.Log(other.gameObject.name + " is already in the board");
                }
                else if (Table.instance.board.Count == 5)
                {
                    Debug.Log(other.gameObject.name + "cannot be added to the board");
                }
                else if (Services.PokerRules.cardsLogged.Contains(other.GetComponent<Card>()))
                {
                    Debug.Log(other.gameObject.name + " is already logged");
                }
                else if (Services.Dealer.deadCardsList.Contains(other.GetComponent<Card>()))
                {
                    Debug.Log(other.gameObject.name + "card is dead");
                }
                else
                {
                    if (other.gameObject.GetComponent<Rigidbody>().isKinematic)
                    {
                        other.GetComponentInParent<Hand>().DetachObject(other.gameObject);
                    }
                    Table.instance.AddCardTo(Destination.board, other.GetComponent<Card>());
                    Services.PokerRules.cardsLogged.Add(other.GetComponent<Card>());
                    //Debug.Log(other.gameObject.name + " went into " + this.gameObject.name);
                    other.GetComponent<Card>().fastTorque = 0;
                    other.GetComponent<Card>().slowTorque = 0;
                    Services.PokerRules.PlayTone();
                    if (Services.Dealer.isCheating)
                    {
                        Services.PokerRules.cardsPulled.Add(other.GetComponent<Card>().cardType);
                        other.GetComponent<Card>().cardThrownNum = Services.PokerRules.cardsPulled.Count;
                        Services.PokerRules.PositionBoardAndBurnCards(other.GetComponent<Card>().cardThrownNum, cardPositionSpeed, false);
                    }
                    else Services.PokerRules.PositionBoardAndBurnCards(other.GetComponent<Card>().cardThrownNum, cardPositionSpeed, false);
                    other.GetComponent<Card>().StopCheating();
                }
            }
            else if (this.gameObject.name == "BurnCards" && Services.Dealer.playerToAct == null &&
                 Table.gameState != GameState.PostHand &&
                 Table.gameState != GameState.NewRound &&
                 Table.gameState != GameState.Misdeal)
            {
                if (Table.instance.burn.Contains(other.GetComponent<Card>()))
                {
                    //Debug.Log(other.gameObject.name + " is already in the burn");
                }
                else if (Services.PokerRules.cardsLogged.Contains(other.GetComponent<Card>()))
                {
                    //Debug.Log(other.gameObject.name + "card is already logged");
                }
                else if(Table.instance.burn.Count == 3)
                {
                    //Debug.Log("too many cards in the burn");
                }
                else if (Services.Dealer.deadCardsList.Contains(other.GetComponent<Card>()))
                {
                    // Debug.Log(other.gameObject.name + "card is dead");
                }
                else
                {
                    if (other.gameObject.GetComponent<Rigidbody>().isKinematic)
                    {
                        other.GetComponentInParent<Hand>().DetachObject(other.gameObject);
                    }
                    Table.instance.AddCardTo(Destination.burn, other.GetComponent<Card>());
                    Services.PokerRules.cardsLogged.Add(other.GetComponent<Card>());
                    //Debug.Log(other.gameObject.name + "Card went into " + this.gameObject.name);
                    Services.PokerRules.PlayTone();
                    other.GetComponent<Card>().fastTorque = 0;
                    other.GetComponent<Card>().slowTorque = 0;
                    if (Services.Dealer.isCheating)
                    {
                        Services.PokerRules.cardsPulled.Add(other.GetComponent<Card>().cardType);
                        other.GetComponent<Card>().cardThrownNum = Services.PokerRules.cardsPulled.Count;
                        Services.PokerRules.PositionBoardAndBurnCards(other.GetComponent<Card>().cardThrownNum, cardPositionSpeed, false);
                    }
                    else Services.PokerRules.PositionBoardAndBurnCards(other.GetComponent<Card>().cardThrownNum, cardPositionSpeed, false);
                    other.GetComponent<Card>().StopCheating();
                }
            }
        }
        #endregion
    }


    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "PlayingCard" && this.gameObject.name != "BurnCards" && this.gameObject.name != "TheBoard") 
        {
            //Debug.Log("Distance between objects is " + Vector3.Distance(other.gameObject.transform.position, transform.position));
            if (GetComponent<Collider>().bounds.Contains(other.ClosestPointOnBounds(transform.position)))
            {
                other.GetComponent<Card>().stillTouchingCollider = true;
            }
            else other.GetComponent<Card>().stillTouchingCollider = false;

            if (!other.GetComponent<Card>().stillTouchingCollider)
            {
                //we go through all the player names
                for (int i = 0; i < playerNames.Count; i++)
                {
                    //when we get to the match, we know which place to remove this from
                    if (gameObject.name == playerNames[i] && Table.gameState == GameState.NewRound && Services.Dealer.players[i].PlayerState != PlayerState.Eliminated)
                    {

                        Table.instance.RemoveCard(playerDestinations[i], other.GetComponent<Card>());
                        Services.PokerRules.cardsLogged.Remove(other.GetComponent<Card>());
                        Services.PokerRules.PlayTone();
                        Debug.Log(other.gameObject.name + " is gone from " + playerNames[i]);
                    }
                }
            }
        }
    }


    public bool CardIsOccupied(Card card)
    {
        for (int i = 0; i < Table.instance.playerCards.Length; i++)
        {
            for (int j = 0; j < Table.instance.playerCards[i].Count; j++)
            {
                if (card.cardType.rank == Table.instance.playerCards[i][j].cardType.rank &&
                   card.cardType.suit == Table.instance.playerCards[i][j].cardType.suit)
                {
                    if (GetComponentInParent<PokerPlayerRedux>().SeatPos != (int)playerDestinations[i])
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool CardsAreFlying(GameObject card)
    {
        GameObject[] cardsOnTable = GameObject.FindGameObjectsWithTag("PlayingCard");
        for (int i = 0; i < cardsOnTable.Length; i++)
        {
            if (cardsOnTable[i].GetComponent<Card>().is_flying)
            {
                if(card != cardsOnTable[i])
                {
                    return true;
                }
            }
        }
        return false;
    }
}