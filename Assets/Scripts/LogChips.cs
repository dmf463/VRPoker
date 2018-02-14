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

    //exactly the same as log cards
    //except this is divided into "putting chips in" and "taking chips out"
    //there might also be a bug here, because it doesn't seem like these are always logging
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Chip")
        {
            if(Services.Dealer.playersHaveBeenEvaluated)
            {
                for (int i = 0; i < playerNames.Count; i++)
                {
					if(gameObject.name == playerNames[i] && Services.Dealer.players[i].PlayerState != PlayerState.Winner)
					{
						if(!Services.Dealer.players[i].playerAudioSource.isPlaying && !Services.Dealer.players[i].playerIsInConversation && !Services.SoundManager.conversationIsPlaying){
							Services.SoundManager.GetSourceAndPlay(Services.Dealer.players[i].playerAudioSource, Services.Dealer.players[i].wrongChipsAudio);
						}
					}
                    if (gameObject.name == playerNames[i])
                    {
                        if (Services.Dealer.players[i].PlayerState == PlayerState.Winner)
                        {
                            if (Services.Dealer.players[i].chipCount != Services.Dealer.players[i].chipsWon + Services.Dealer.players[i].ChipCountToCheckWhenWinning)
                            {
                                //if (other.GetComponent<Chip>().chipStack == null)
                                //{
                                //    Debug.Log("adding a chip of " + other.GetComponent<Chip>().chipData.ChipValue);
                                //    Table.instance.AddChipTo(playerDestinations[i], other.GetComponent<Chip>().chipData.ChipValue);
                                //}
                                /*else */if (other.GetComponent<Chip>().chipStack != null)
                                {
                                    ChipStack chipStack;
                                    chipStack = other.GetComponent<Chip>().chipStack;
                                    Debug.Log("adding chipStack of " + chipStack.stackValue);
                                    Table.instance.AddChipTo(playerDestinations[i], chipStack.stackValue);
                                }
                            }
                            else Debug.Log("Player already has all their chips");
                        }
                    }
                }
            }
        }
        if(other.gameObject.tag == "Tip" && gameObject.name == "TipCatcher")
        {
            ChipStack chipStack;
            int chipValue;
            if (other.GetComponent<Chip>().chipStack != null)
            {
                chipStack = other.GetComponent<Chip>().chipStack;
                chipValue = chipStack.stackValue;
            }
            else chipValue = other.GetComponent<Chip>().chipData.ChipValue;
            Services.Dealer.tipCount += chipValue;
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
                    if (gameObject.name == playerNames[i] && 
                        Services.Dealer.players[i].PlayerState == PlayerState.Winner &&
                        Services.Dealer.players[i].chipCount != Services.Dealer.players[i].chipsWon + Services.Dealer.players[i].ChipCountToCheckWhenWinning)
                    {
                        //if (other.GetComponent<Chip>().chipStack == null)
                        //{
                        //    Table.instance.RemoveChipFrom(playerDestinations[i], other.GetComponent<Chip>().chipData.ChipValue);
                        //}
                        /*else */if (other.GetComponent<Chip>().chipStack != null)
                        {
                            ChipStack chipStack;
                            chipStack = other.GetComponent<Chip>().chipStack;
                            Table.instance.RemoveChipFrom(playerDestinations[i], chipStack.stackValue);
                        }
                    }
                }
            }
        }
    }
}
