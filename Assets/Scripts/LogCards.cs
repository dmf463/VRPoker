using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCards : MonoBehaviour
{
    public float cardCount;
    Card[] cardsOnTable = new Card[0];
    int cardsDestroyed = 0;

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
                TableCards.instance.AddCardTo(Destinations.player0, other.GetComponent<Card>().cardType);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TestSpace2")
            {
                TableCards.instance.AddCardTo(Destinations.player1, other.GetComponent<Card>().cardType);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TestSpace3")
            {
                TableCards.instance.AddCardTo(Destinations.player2, other.GetComponent<Card>().cardType);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TestSpace4")
            {
                TableCards.instance.AddCardTo(Destinations.player3, other.GetComponent<Card>().cardType);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TheBoard")
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
            else if (this.gameObject.name == "BurnCards")
            {
                TableCards.instance.AddCardTo(Destinations.burn, other.GetComponent<Card>().cardType);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if(this.gameObject.name == "ShufflingArea")
            {
                cardsOnTable = FindObjectsOfType<Card>();
                Debug.Log("cardsOnTable = " + cardsOnTable.Length + cardsOnTable[0].name);
                Destroy(other.gameObject);
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
