﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogObjects : MonoBehaviour
{
    #region MonoBehaviour Stuff
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
    #endregion
    public void OnTriggerEnter(Collider other)
    {
        cardCount += 1;
        #region Logging the PlayingCard for each space
        if (other.gameObject.tag == "PlayingCard")
        {
            if (this.gameObject.name == "Player0")
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
                        TableCards.instance.AddCardTo(Destination.player0, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }
            }
            else if (this.gameObject.name == "Player1")
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
                        TableCards.instance.AddCardTo(Destination.player1, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }
            }
            else if (this.gameObject.name == "Player2")
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
                        TableCards.instance.AddCardTo(Destination.player2, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }
            }
            else if (this.gameObject.name == "Player3")
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
                        TableCards.instance.AddCardTo(Destination.player3, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }

            }
            else if (this.gameObject.name == "Player4") 
                {
                if (TableCards.dealerState == DealerState.DealingState) 
                {
                    if (TableCards.instance._player4.Contains(other.GetComponent<Card>().cardType)) 
                    {
                        Debug.Log(other.gameObject.name + " is already in play");
                    }
                    else if (TableCards.instance._player4.Count == 2) 
                    {
                        Debug.Log(other.gameObject.name + "cannot be added to the board");
                    }
                    else 
                    {
                        TableCards.instance.AddCardTo(Destination.player4, other.GetComponent<Card>().cardType);
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
                        TableCards.instance.AddCardTo(Destination.board, other.GetComponent<Card>().cardType);
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }

            }
            else if (this.gameObject.name == "BurnCards")
            {
                TableCards.instance.AddCardTo(Destination.burn, other.GetComponent<Card>().cardType);
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
                        GameObject[] deadCards = GameObject.FindGameObjectsWithTag("PlayingCard");
                        foreach (GameObject card in deadCards)
                        {
                            Destroy(card);
                        }
                        TableCards.dealerState = DealerState.DealingState;
                    }
                }
            }

        }
        #endregion
        #region Logging the Chip for each Space
        if (other.gameObject.tag == "Chip")
        {
            if(gameObject.name == "Player0") 
            {
                if(other.GetComponent<Chip>().chipStack == null)
                {
                    TableCards.instance.AddChipTo(Destination.player0, other.GetComponent<Chip>());
                }
                else if(other.GetComponent<Chip>().chipStack != null)
                {
                    foreach(Chip chip in other.GetComponent<Chip>().chipStack.chips)
                    {
                        TableCards.instance.AddChipTo(Destination.player0, chip);
                    }
                }

            }
            else if (gameObject.name == "Player1")
            {
                if (other.GetComponent<Chip>().chipStack == null)
                {
                    TableCards.instance.AddChipTo(Destination.player1, other.GetComponent<Chip>());
                }
                else if (other.GetComponent<Chip>().chipStack != null)
                {
                    foreach (Chip chip in other.GetComponent<Chip>().chipStack.chips)
                    {
                        TableCards.instance.AddChipTo(Destination.player1, chip);
                    }
                }

            }
            else if (gameObject.name == "Player2")
            {
                if (other.GetComponent<Chip>().chipStack == null)
                {
                    TableCards.instance.AddChipTo(Destination.player2, other.GetComponent<Chip>());
                }
                else if (other.GetComponent<Chip>().chipStack != null)
                {
                    foreach (Chip chip in other.GetComponent<Chip>().chipStack.chips)
                    {
                        TableCards.instance.AddChipTo(Destination.player2, chip);
                    }
                }

            }
            else if (gameObject.name == "Player3")
            {
                if (other.GetComponent<Chip>().chipStack == null)
                {
                    TableCards.instance.AddChipTo(Destination.player3, other.GetComponent<Chip>());
                }
                else if (other.GetComponent<Chip>().chipStack != null)
                {
                    foreach (Chip chip in other.GetComponent<Chip>().chipStack.chips)
                    {
                        TableCards.instance.AddChipTo(Destination.player3, chip);
                    }
                }

            }
            else if (gameObject.name == "Player4")
            {
                if (other.GetComponent<Chip>().chipStack == null)
                {
                    TableCards.instance.AddChipTo(Destination.player4, other.GetComponent<Chip>());
                }
                else if (other.GetComponent<Chip>().chipStack != null)
                {
                    foreach (Chip chip in other.GetComponent<Chip>().chipStack.chips)
                    {
                        TableCards.instance.AddChipTo(Destination.player4, chip);
                    }
                }

            }
        }
        #endregion
    }
}
