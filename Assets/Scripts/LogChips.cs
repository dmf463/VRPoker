﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogChips : MonoBehaviour
{

    #region MonoBehaviour Stuff
    private List<string> playerNames = new List<string>
    {
        "P0Chips", "P1Chips", "P2Chips", "P3Chips", "P4Chips"
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

    //exactly the same as log cards
    //except this is divided into "putting chips in" and "taking chips out"
    //there might also be a bug here, because it doesn't seem like these are always logging
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Chip")
        {
            //if (!other.GetComponent<Chip>().markedForDestruction || other.GetComponent<Chip>().chipForBet == true)
            if(Services.Dealer.playersHaveBeenEvaluated)
            {
                for (int i = 0; i < playerNames.Count; i++)
                {
                    if (gameObject.name == playerNames[i])
                    {
                        if (other.GetComponent<Chip>().inAStack == false)
                        {
                            if (!Table.instance.playerChipStacks[i].Contains(other.GetComponent<Chip>()))
                            {
                                Table.instance.AddChipTo(playerDestinations[i], other.GetComponent<Chip>());
                                //Debug.Log("something is being added at " + Time.time);
                            }
                            //else Debug.Log("This chip has already been logged");
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
                                if (!Table.instance.playerChipStacks[i].Contains(chip))
                                {
                                    Table.instance.AddChipTo(playerDestinations[i], chip);
                                    //Debug.Log("something is being added at " + Time.time);
                                }
                                //else Debug.Log("this chip, as part of a stack, is already logged");
                            }
                        }
                    }
                }
            }
        }
    }


    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Chip")
        {
            if (Services.Dealer.playersHaveBeenEvaluated)
            {
                for (int i = 0; i < playerNames.Count; i++)
                {
                    if (gameObject.name == playerNames[i])
                    {
                        if (other.GetComponent<Chip>().inAStack == false)
                        {
                            Table.instance.RemoveChipFrom(playerDestinations[i], other.GetComponent<Chip>());
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
                                Table.instance.RemoveChipFrom(playerDestinations[i], chip);
                            }
                        }
                    }
                }
            }
        }
    }
}
