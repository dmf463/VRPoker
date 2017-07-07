using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCards : MonoBehaviour
{
    public float cardCount;
    Vector3 newCardDeckScale;
    Vector3 currentCardDeckScale;
    Vector3 decreaseCardDeckBy;

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
                        GameObject newCardDeck = Instantiate(Resources.Load("Prefabs/PlayingCardDeck"), transform.position, Quaternion.identity) as GameObject;
                        newCardDeckScale = newCardDeck.transform.localScale;
                        decreaseCardDeckBy = new Vector3(newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
                        newCardDeck.transform.localScale = new Vector3(newCardDeckScale.x, newCardDeckScale.y / 52, newCardDeckScale.z);
                        //have it increase every time it destroys an object until the deck is full and then TableCards.instance.PopulateDeck();
                    }
                    Destroy(other.gameObject);
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
