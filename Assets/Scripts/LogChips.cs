using System.Collections;
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

    public void OnTriggerEnter(Collider other)
    {
        #region logging chips for each space
        if (other.gameObject.tag == "Chip")
        {

            for (int i = 0; i < playerNames.Count; i++)
            {
                if (gameObject.name == playerNames[i])
                {
                    if (Services.GameManager.players[i].organizingChips == false)
                    {
                        #region Logging the Chip for each Space and stacking at the same time currently disabled because I think I need to make those separate
                        //create a list of chips that are about to be added
                        //  if its the first chip the player gets
                        //      add that chip to the list
                        //      destroy the chip
                        //      instantiate it in the player space.
                        //      add the list to the players chipstack
                        //      IF there IS a chip in the space
                        //          add the new chip to the list
                        //          destroy the chip
                        //          IF the chip is NOT a stack
                        //              make the first chip in there a chipstack
                        //          add the chip in the list to the chipstack
                        //          add the list to the players chipstack list

                        //      IF the chip IS in a stack
                        //          add those chips to the chip list
                        //          destroy those chips
                        //          add the chips in the list to the chipstack
                        //          add the chips in the list to the chiplist
                        //  if it's the first chipStack in the player space
                        //     destroy the chip stack
                        //     instantiate the parent object as the first chip
                        //     add each child object to the first chip
                        //     add the chips in the space to the chiplist
                        //List<Chip> tempChipList = new List<Chip>();
                        //if (other.GetComponent<Chip>().inAStack == false)
                        //{
                        //    if (Table.instance.playerChipStacks[i].Contains(other.GetComponent<Chip>()))
                        //    {
                        //        Debug.Log("this chip is already in the stack");
                        //    }
                        //    else
                        //    {
                        //        Debug.Log("got into the new part");
                        //        if(Table.instance.playerChipStacks[i].Count == 0)
                        //        {
                        //            Table.instance.AddChipTo(playerDestinations[i], other.GetComponent<Chip>());
                        //            Table.instance.playerChipStacks[i][0].chipStack = new ChipStack(Table.instance.playerChipStacks[i][0]);
                        //        }
                        //        else
                        //        {
                        //            tempChipList.Add(other.GetComponent<Chip>());
                        //            Chip referenceChip = Table.instance.playerChipStacks[i][0];
                        //            referenceChip.chipStack.AddToStackOnTable(tempChipList[0]);
                        //            Table.instance.AddChipTo(playerDestinations[i], tempChipList[0]);
                        //        }
                        //    }
                        //}
                        //else if (other.GetComponent<Chip>().inAStack == true)
                        //{
                        //    ChipStack chipStack;
                        //    if (other.GetComponent<Chip>().chipStack != null)
                        //    {
                        //        chipStack = other.GetComponent<Chip>().chipStack;
                        //    }
                        //    else
                        //    {
                        //        chipStack = other.transform.parent.gameObject.GetComponent<Chip>().chipStack;
                        //    }
                        //    foreach (Chip chip in chipStack.chips)
                        //    {
                        //        if (Table.instance.playerChipStacks[i].Contains(chip))
                        //        {
                        //            Debug.Log("this chip is already in the stack");
                        //        }
                        //        else
                        //        {
                        //            if(Table.instance.playerChipStacks[i].Count == 0)
                        //            {
                        //                Table.instance.AddChipTo(playerDestinations[i], chip);
                        //            }
                        //            else
                        //            {
                        //                Chip referenceChip = Table.instance.playerChipStacks[i][0];
                        //                referenceChip.chipStack.AddToStackOnTable(chip);
                        //                Table.instance.AddChipTo(playerDestinations[i], chip);
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion
                        #region the original log chips code
                        if (other.GetComponent<Chip>().inAStack == false)
                        {
                            if (Table.instance.playerChipStacks[i].Contains(other.GetComponent<Chip>()))
                            {
                                Debug.Log("this chip is already in the stack");
                            }
                            else
                            {
                                Table.instance.AddChipTo(playerDestinations[i], other.GetComponent<Chip>());
                            }
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
                                else
                                {
                                    Table.instance.AddChipTo(playerDestinations[i], chip);
                                }
                            }
                        }
                        #endregion
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
                    if (Services.GameManager.players[i].organizingChips == false)
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
