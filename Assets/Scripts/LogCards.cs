using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCards : MonoBehaviour
{
    public float cardCount;

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
                TableCards.instance.AddCardTo(Destinations.player0, other.GetComponent<CardManager>().card);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TestSpace2")
            {
                TableCards.instance.AddCardTo(Destinations.player1, other.GetComponent<CardManager>().card);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TestSpace3")
            {
                TableCards.instance.AddCardTo(Destinations.player2, other.GetComponent<CardManager>().card);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TestSpace4")
            {
                TableCards.instance.AddCardTo(Destinations.player3, other.GetComponent<CardManager>().card);
                Debug.Log("Card went into " + this.gameObject.name);
            }
            else if (this.gameObject.name == "TheBoard")
            {
                if (TableCards.instance._board.Contains(other.GetComponent<CardManager>().card))
                {
                    Debug.Log(other.gameObject.name + " is already in play");
                }
                else
                {
                    TableCards.instance.AddCardTo(Destinations.board, other.GetComponent<CardManager>().card);
                    Debug.Log("Card went into " + this.gameObject.name);
                }

            }
            else if (this.gameObject.name == "BurnCards")
            {
                TableCards.instance.AddCardTo(Destinations.burn, other.GetComponent<CardManager>().card);
                Debug.Log("Card went into " + this.gameObject.name);
            }

        }
    }
}
