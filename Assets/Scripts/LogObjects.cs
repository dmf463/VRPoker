using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogObjects : MonoBehaviour
{
    #region MonoBehaviour Stuff
    public float cardCount;
    private GameObject newCardDeck;
    private bool madeNewDeck;
    private List<string> playerNames = new List<string>
    {
        "Player0", "Player1", "Player2", "Player3", "Player4"
    };
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
        cardCount += 1;
        #region Logging the PlayingCard for each space
        if (other.gameObject.tag == "PlayingCard")
        {
            for (int i = 0; i < playerNames.Count; i++)
            {
                if (gameObject.name == playerNames[i])
                {
                    if (Table.dealerState == DealerState.DealingState)
                    {
                        if (Table.instance.playerCards[i].Contains(other.GetComponent<Card>()))
                        {
                            Debug.Log(other.gameObject.name + " is already in play.");
                        }
                        else if (Table.instance.playerCards[i].Count == 2)
                        {
                            Debug.Log(other.gameObject.name + " cannot be added to " + playerNames[i]);
                        }
                        else
                        {
                            Table.instance.AddCardTo(playerDestinations[i], other.GetComponent<Card>());
                            Debug.Log("Card went into " + playerNames[i]);
                        }
                    }
                }

            }
            if (this.gameObject.name == "TheBoard")
            {
                if(Table.dealerState == DealerState.DealingState)
                {
                    if (Table.instance._board.Contains(other.GetComponent<Card>()))
                    {
                        Debug.Log(other.gameObject.name + " is already in play");
                    }
                    else if(Table.instance._board.Count == 5)
                    {
                        Debug.Log(other.gameObject.name + "cannot be added to the board");
                    }
                    else
                    {
                        Table.instance.AddCardTo(Destination.board, other.GetComponent<Card>());
                        Debug.Log("Card went into " + this.gameObject.name);
                    }
                }

            }
            else if (this.gameObject.name == "BurnCards")
            {
                Table.instance.AddCardTo(Destination.burn, other.GetComponent<Card>());
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
                if (Table.dealerState == DealerState.ShufflingState && madeNewDeck == true)
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
                        Table.dealerState = DealerState.DealingState;
                    }
                }
            }

        }
        #endregion
        #region Logging the Chip for each Space
        if (other.gameObject.tag == "Chip")
        {
            
            for (int i = 0; i < playerNames.Count; i++)
            {
                if(gameObject.name == playerNames[i])
                {
                    if (other.GetComponent<Chip>().inAStack == false)
                    {
                        if (Table.instance.playerChipStacks[i].Contains(other.GetComponent<Chip>()))
                        {
                            Debug.Log("this chip is already in the stack");
                        }
                        else Table.instance.AddChipTo(playerDestinations[i], other.GetComponent<Chip>());
                    }
                    else if (other.GetComponent<Chip>().inAStack == true)
                    {
                        ChipStack chipStack;
                        if (other.GetComponent<Chip>().chipStack != null)
                        {
                            chipStack = other.GetComponent<Chip>().chipStack;
                        }
                        else
                        {
                            chipStack = other.transform.parent.gameObject.GetComponent<Chip>().chipStack;
                        }
                        foreach (Chip chip in chipStack.chips)
                        {
                            if (Table.instance.playerChipStacks[i].Contains(chip))
                            {
                                Debug.Log("this chip is already in the stack");
                            }
                            else Table.instance.AddChipTo(playerDestinations[i], chip);
                        }
                    }
                }
            }
        }
        #endregion
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Chip")
        {

            for (int i = 0; i < playerNames.Count; i++)
            {
                if (gameObject.name == playerNames[i])
                {
                    if (other.GetComponent<Chip>().inAStack == false)
                    {
                        if (Table.instance.playerChipStacks[i].Contains(other.GetComponent<Chip>()))
                        {
                            Debug.Log("removed this chip already");
                        }
                        else Table.instance.RemoveChipFrom(playerDestinations[i], other.GetComponent<Chip>());
                    }
                    else if (other.GetComponent<Chip>().inAStack == true)
                    {
                        ChipStack chipStack;
                        if (other.GetComponent<Chip>().chipStack != null)
                        {
                            chipStack = other.GetComponent<Chip>().chipStack;
                        }
                        else
                        {
                            chipStack = other.transform.parent.gameObject.GetComponent<Chip>().chipStack;
                        }
                        foreach (Chip chip in chipStack.chips)
                        {
                            if (Table.instance.playerChipStacks[i].Contains(chip))
                            {
                                Debug.Log("this ship already left");
                            }
                            else Table.instance.RemoveChipFrom(playerDestinations[i], chip);
                        }
                    }
                }
            }
        }
    }
}
