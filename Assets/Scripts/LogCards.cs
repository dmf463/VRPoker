using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCards : MonoBehaviour
{
    public float cardCount;
    string playerName;

    // Use this for initialization
    void Start()
    {
        playerName = this.gameObject.name;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //should change switch statement to an else if tree, so that I can do a better job debugging. and being more flexible.
    public void OnTriggerEnter(Collider other)
    {
        cardCount += 1;
        if (other.gameObject.tag == "PlayingCard")
        {
            Debug.Log(other.GetComponent<CardManager>().card);
            switch (playerName)
            {
                case "TestSpace1":
                    Debug.Log(other.GetComponent<CardManager>().card.rank + "" + other.GetComponent<CardManager>().card.suit);
                    TableCards.instance.AddCardTo(Destinations.player0, other.GetComponent<CardManager>().card);
                    break;
                case "TestSpace2":
                    TableCards.instance.AddCardTo(Destinations.player1, other.GetComponent<CardManager>().card);
                    break;
                case "TestSpace3":
                    TableCards.instance.AddCardTo(Destinations.player2, other.GetComponent<CardManager>().card);
                    break;
                case "TestSpace4":
                    TableCards.instance.AddCardTo(Destinations.player3, other.GetComponent<CardManager>().card);
                    break;
                case "TheBoard":
                    if (TableCards.instance._board.Contains(other.GetComponent<CardManager>().card))
                    {
                        break;
                    }
                    TableCards.instance.AddCardTo(Destinations.board, other.GetComponent<CardManager>().card);
                    break;
                case "BurnCards":
                    TableCards.instance.AddCardTo(Destinations.burn, other.GetComponent<CardManager>().card);
                    break;
                default:
                    break;
            }
        }
    }
}
