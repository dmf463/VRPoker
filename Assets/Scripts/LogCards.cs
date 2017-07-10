using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCards : MonoBehaviour
{
    public float cardCount;
    private GameObject newCardDeck;
    private Vector3 newCardDeckScale;
    private Vector3 currentCardDeckScale;
    private Vector3 decreaseCardDeckBy;

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
                    TableCards.instance.AddCardTo(Destinations.player0, other.GetComponent<Card>().cardType);
                    Debug.Log("Card went into " + this.gameObject.name);
                }
            }
            else if (this.gameObject.name == "TestSpace2")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    TableCards.instance.AddCardTo(Destinations.player1, other.GetComponent<Card>().cardType);
                    Debug.Log("Card went into " + this.gameObject.name);
                }
            }
            else if (this.gameObject.name == "TestSpace3")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    TableCards.instance.AddCardTo(Destinations.player2, other.GetComponent<Card>().cardType);
                    Debug.Log("Card went into " + this.gameObject.name);
                }
            }
            else if (this.gameObject.name == "TestSpace4")
            {
                if(TableCards.dealerState == DealerState.DealingState)
                {
                    TableCards.instance.AddCardTo(Destinations.player3, other.GetComponent<Card>().cardType);
                    Debug.Log("Card went into " + this.gameObject.name);
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
                if(TableCards.dealerState == DealerState.ShufflingState)
                {
                    if(GameObject.FindGameObjectWithTag("CardDeck") == null)
                    {
                        Debug.Log("Could not find CardDeck, instantiating new one");
                        newCardDeck = Instantiate(Resources.Load("Prefabs/PlayingCardDeck"), transform.position, Quaternion.identity) as GameObject;
                        newCardDeckScale = newCardDeck.transform.localScale;
                        newCardDeck.GetComponent<CardDeckScript>().newCardDeckScale = newCardDeckScale;
                        decreaseCardDeckBy = new Vector3(newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
                        newCardDeck.transform.localScale = decreaseCardDeckBy;
                        currentCardDeckScale = newCardDeck.transform.localScale;
                    }
                    if(GameObject.FindGameObjectWithTag("CardDeck") != null && 
                        GameObject.FindGameObjectWithTag("CardDeck").GetComponent<Transform>().childCount == 0 &&
                        GameObject.FindGameObjectsWithTag("CardDeck").Length == 1)
                    {
                        Destroy(GameObject.FindGameObjectWithTag("CardDeck"));
                        newCardDeck = Instantiate(Resources.Load("Prefabs/PlayingCardDeck"), transform.position, Quaternion.identity) as GameObject;
                        newCardDeckScale = newCardDeck.transform.localScale;
                        newCardDeck.GetComponent<CardDeckScript>().newCardDeckScale = newCardDeckScale;
                        decreaseCardDeckBy = new Vector3(newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
                        newCardDeck.transform.localScale = decreaseCardDeckBy;
                        currentCardDeckScale = newCardDeck.transform.localScale;
                    }
   
                    Destroy(other.gameObject);
                    Debug.Log("destroying cards");
                    currentCardDeckScale.y = currentCardDeckScale.y + decreaseCardDeckBy.y;
                    newCardDeck.transform.localScale = currentCardDeckScale;   
                    if(currentCardDeckScale.y > newCardDeckScale.y)
                    {
                        Debug.Log("currentCardsDeck.y is " + currentCardDeckScale.y + " and newCardDeckScale.y - decreaseCardDeckBy.y is " + (newCardDeckScale.y - decreaseCardDeckBy.y));
                        TableCards.dealerState = DealerState.DealingState;
                    }
                }

                //if (cardsOnTable.Length == 1)
                //{
                //    TableCards.instance.NewGame();
                //    Destroy(GameObject.Find("PlayingCardDeck").gameObject);
                //    GameObject newCardDeck = Instantiate(Resources.Load("Prefabs/PlayingCardDeck"), transform.position, Quaternion.identity) as GameObject;
                //}
            }

        }
    }
}
