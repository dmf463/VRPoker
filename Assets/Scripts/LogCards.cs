using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCards : MonoBehaviour
{
    public float cardCount;
    private GameObject newCardDeck;
    private bool madeNewDeck;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider other)
    {
        cardCount += 1;
        if (other.gameObject.tag == "PlayingCard")
        {
            if (this.gameObject.name == "TestSpace1")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    if (TableCards.instance._player0.Contains(other.GetComponent<Card>().cardType))
                    {
                        Debug.Log(other.gameObject.name + " is already in play");
                    }
                    else if (TableCards.instance._player0.Count == 2)
                    {
                        Debug.Log(other.gameObject.name + "cannot be added to the board");
                    }
                    else
                    {
                        TableCards.instance.AddCardTo(Destinations.player0, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }
            }
            else if (this.gameObject.name == "TestSpace2")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    if (TableCards.instance._player1.Contains(other.GetComponent<Card>().cardType))
                    {
                        Debug.Log(other.gameObject.name + " is already in play");
                    }
                    else if (TableCards.instance._player1.Count == 2)
                    {
                        Debug.Log(other.gameObject.name + "cannot be added to the board");
                    }
                    else
                    {
                        TableCards.instance.AddCardTo(Destinations.player1, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }
            }
            else if (this.gameObject.name == "TestSpace3")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    if (TableCards.instance._player2.Contains(other.GetComponent<Card>().cardType))
                    {
                        Debug.Log(other.gameObject.name + " is already in play");
                    }
                    else if (TableCards.instance._player2.Count == 2)
                    {
                        Debug.Log(other.gameObject.name + "cannot be added to the board");
                    }
                    else
                    {
                        TableCards.instance.AddCardTo(Destinations.player2, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }
            }
            else if (this.gameObject.name == "TestSpace4")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    if (TableCards.instance._player3.Contains(other.GetComponent<Card>().cardType))
                    {
                        Debug.Log(other.gameObject.name + " is already in play");
                    }
                    else if (TableCards.instance._player3.Count == 2)
                    {
                        Debug.Log(other.gameObject.name + "cannot be added to the board");
                    }
                    else
                    {
                        TableCards.instance.AddCardTo(Destinations.player3, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }

            }
            else if (this.gameObject.name == "TheBoard")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    if (TableCards.instance._board.Contains(other.GetComponent<Card>().cardType))
                    {
                        Debug.Log(other.gameObject.name + " is already in play");
                    }
                    else if(TableCards.instance._board.Count == 5)
                    {
                        Debug.Log(other.gameObject.name + "cannot be added to the board");
                    }
                    else
                    {
                        TableCards.instance.AddCardTo(Destinations.board, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }

            }
            else if (this.gameObject.name == "BurnCards")
            {
                TableCards.instance.AddCardTo(Destinations.burn, other.GetComponent<Card>().cardType);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if(this.gameObject.name == "ShufflingArea")
            {
                if (GameObject.FindGameObjectWithTag("CardDeck") == null)
                {
                    Debug.Log("Could not find CardDeck, instantiating new one");
                    newCardDeck = Instantiate(Services.PrefabDB.CardDeck, transform.position, Quaternion.identity) as GameObject;
                    newCardDeck.GetComponent<CardDeckScript>().BuildDeckFromOneCard(newCardDeck);
                    madeNewDeck = true;
                }
                if (TableCards.dealerState == DealerState.ShufflingState && madeNewDeck == true)
                {
                    Destroy(other.gameObject);
                    Debug.Log("destroying cards");
                    newCardDeck.GetComponent<CardDeckScript>().MakeDeckLarger();   
                    if(newCardDeck.GetComponent<CardDeckScript>().currentCardDeckScale.y > newCardDeck.GetComponent<CardDeckScript>().newCardDeckScale.y)
                    {
                        madeNewDeck = false;
                        TableCards.dealerState = DealerState.DealingState;
                    }
                }
            }

        }
    }
}
