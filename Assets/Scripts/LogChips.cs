using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class LogChips : MonoBehaviour
{

    #region MonoBehaviour Stuff
    private List<string> playerNames = new List<string>
    {
        "P0Cards", "P1Cards", "P2Cards", "P3Cards", "P4Cards"
    };
    [HideInInspector]
    public List<Destination> playerDestinations = new List<Destination>
    {
        Destination.player0, Destination.player1, Destination.player2, Destination.player3, Destination.player4
    };
    float startUpTime;
    bool startTimer = false;
    float timeLimit = 2;
    PokerPlayerRedux player;
    bool playerMadeMistake = false;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (startTimer)
        {
            Debug.Log("STARTING TIMER");
            float timeSinceEnter = Time.time - startUpTime;
            if (timeSinceEnter >= timeLimit)
            {
                Debug.Log("SHOULD SAY LINE");
                if (!player.playerAudioSource.isPlaying && !player.playerIsInConversation && !Services.SoundManager.conversationIsPlaying)
                {
                    Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(player.playerName, LineCriteria.WrongChips));
                    //Services.SoundManager.GetSourceAndPlay(player.playerAudioSource, player.wrongChipsAudio);
                }
                startTimer = false;
            }
        }
    }
    #endregion

    //exactly the same as log cards
    //except this is divided into "putting chips in" and "taking chips out"
    //there might also be a bug here, because it doesn't seem like these are always logging
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Chip")
        {
            if (Services.Dealer.playersHaveBeenEvaluated)
            {
                for (int i = 0; i < playerNames.Count; i++)
                {
                    if (gameObject.name == playerNames[i] && Services.Dealer.players[i].PlayerState != PlayerState.Winner)
                    {
                        Debug.Log("NOT MY CHIP");
                        player = Services.Dealer.players[i];
                        startUpTime = Time.time;
                        startTimer = true;
                    }
                    if (gameObject.name == playerNames[i])
                    {
                        if (Services.Dealer.players[i].PlayerState == PlayerState.Winner)
                        {
                            Chip chip = other.GetComponent<Chip>();
                            if (Services.Dealer.players[i].chipCount + chip.stackValue <= (Services.Dealer.players[i].chipsWon + Services.Dealer.players[i].ChipCountToCheckWhenWinning) &&
                               !Services.Dealer.players[i].gaveTip)
                            {
                                other.GetComponent<Chip>().isAtDestination = true;
                                other.GetComponent<Chip>().owner = Services.Dealer.players[i];
                                Debug.Log("adding chipStack of " + chip.stackValue);
                                Table.instance.AddChipTo(playerDestinations[i], chip.stackValue);
                            }
                            else Debug.Log("Player already has all their chips");
                        }
                    }
                }
            }
        }
        if (other.gameObject.tag == "Tip" && gameObject.name == "TipCatcher")
        {
            if (other.gameObject.GetComponentInParent<Hand>() != null)
            {
                other.gameObject.GetComponentInParent<Hand>().DetachObject(other.gameObject);
            }
            //if (other.GetComponent<Chip>().chipStack != null)
            //{
            Chip chip = other.GetComponent<Chip>();
            //Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.fallingTip, .7f);
            Services.SoundManager.GetNonPlayerSourceAndPlay(GetComponent<AudioSource>(), Services.SoundManager.fallingTip);
            Services.Dealer.tipCount += chip.stackValue;
            Destroy(other.gameObject);
        }
    }


    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Chip")
        {
            if (Services.Dealer.playersHaveBeenEvaluated)
            {
                //Debug.Log("chip is leaving");
                for (int i = 0; i < playerNames.Count; i++)
                {
                    if (player != null)
                    {
                        if (Services.Dealer.players[i].SeatPos == player.SeatPos)
                        {
                            startTimer = false;
                            Debug.Log("TURNING TIMER OFF");
                        }
                    }
                    if (gameObject.name == playerNames[i] && Services.Dealer.players[i].PlayerState == PlayerState.Winner && !other.GetComponent<Chip>().isAtDestination)
                    {
                        //if (other.GetComponent<Chip>().chipStack != null)
                        //{
                        Chip chip = other.GetComponent<Chip>();
                        Debug.Log("removed chip of value " + chip.stackValue);
                        Table.instance.RemoveChipFrom(playerDestinations[i], chip.stackValue);
                        //}
                    }
                }
            }
        }
    }
}
