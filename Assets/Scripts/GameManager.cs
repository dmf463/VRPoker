using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{

    //holds all the cards where they need to be
    public float cardsDealt;
    public List<PokerPlayer> players = new List<PokerPlayer>();

    //keep track of where we are in the game
    //private bool flopDealt = false;
    //private bool turnDealt = false;
    //private bool riverDealt = false;
    //private bool readyToEvalute = false;
    //private bool winnerDeclared = false;
    PokerPlayer player0;
    PokerPlayer player1;
    PokerPlayer player2;
    PokerPlayer player3;

    void Awake()
    {
        Services.PrefabDB = Resources.Load<PrefabDB>("Prefabs/PrefabDB");
    }

    // Use this for initialization
    void Start()
    {
        player0 = new PokerPlayer();
        player1 = new PokerPlayer();
        player2 = new PokerPlayer();
        player3 = new PokerPlayer();

        player0.SeatPos = Destinations.player0;
        player1.SeatPos = Destinations.player1;
        player2.SeatPos = Destinations.player2;
        player3.SeatPos = Destinations.player3;

        player0.PlayerState = PlayerState.Playing;
        player1.PlayerState = PlayerState.Playing;
        player2.PlayerState = PlayerState.Playing;
        player3.PlayerState = PlayerState.Playing;

        cardsDealt = 0;

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("player0 in GM has " + player0Cards.Count + " cards");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //this is purelyfor testing purposes
            //I need to write a function to do this, but also need to write comparisons
            //I need to log who had the highest hand, the second highest, and so forth, so that eventually I can add in side pots and shit
            //I COULD do that in another script? some kind of Comparer Script maybe? and make instances of itself to compare? but either way it's gonna be a pain in the ass
            TableCards.instance.DebugHands();
            TableCards.instance.EvaluatePlayer(Destinations.player0);
            TableCards.instance.EvaluatePlayer(Destinations.player1);
            TableCards.instance.EvaluatePlayer(Destinations.player2);
            TableCards.instance.EvaluatePlayer(Destinations.player3);

            HandEvaluator player0Hand = new HandEvaluator(TableCards.instance._player0);
            HandEvaluator player1Hand = new HandEvaluator(TableCards.instance._player1);
            HandEvaluator player2Hand = new HandEvaluator(TableCards.instance._player2);
            HandEvaluator player3Hand = new HandEvaluator(TableCards.instance._player3);

            player0Hand.EvaluateHand();
            player1Hand.EvaluateHand();
            player2Hand.EvaluateHand();
            player3Hand.EvaluateHand();

            player0.Hand = player0Hand;
            player1.Hand = player1Hand;
            player2.Hand = player2Hand;
            player3.Hand = player3Hand;

            Debug.Log(player0.SeatPos + " has " + player0.Hand.HandValues.PokerHand + " with a total HandValue of " + player0.Hand.HandValues.Total + " with a highCard of " + player0.Hand.HandValues.HighCard);
            Debug.Log(player1.SeatPos + " has " + player1.Hand.HandValues.PokerHand + " with a total HandValue of " + player1.Hand.HandValues.Total + " with a highCard of " + player1.Hand.HandValues.HighCard);
            Debug.Log(player2.SeatPos + " has " + player2.Hand.HandValues.PokerHand + " with a total HandValue of " + player2.Hand.HandValues.Total + " with a highCard of " + player2.Hand.HandValues.HighCard);
            Debug.Log(player3.SeatPos + " has " + player3.Hand.HandValues.PokerHand + " with a total HandValue of " + player3.Hand.HandValues.Total + " with a highCard of " + player3.Hand.HandValues.HighCard);

            players.Add(player0);
            players.Add(player1);
            players.Add(player2);
            players.Add(player3);

            List<PokerPlayer> sortedPlayers = new List<PokerPlayer>(players.OrderByDescending(bestHand => bestHand.Hand.HandValues.PokerHand).ThenByDescending(bestHand => bestHand.Hand.HandValues.Total).ThenByDescending(bestHand => bestHand.Hand.HandValues.HighCard));
            //sortedPlayers[0].PlayerState = PlayerState.Winner;

            //so I COULD do it manually, like:
            //if(sortedplayers[0].Hand.HandValues.pokerHand == sortedPlayers[1].Hand.HandValues.pokerHand && sortedPlayer[0].Hand.HandValues.pokerHand == sortedPlayers[2].....
            //BUT THAT WILL TAKE FOREVER
            //because I need to compare the pokerHand, then the total, then the highCard to see if they're the same or not. 
            //and doing that for EACH variation of possible tie combinations seems fucking ridiculous
            //I could make the list Distinct(), but that wouldn't help me with determining a draw. I wish two objects could occupy the same position in a list...
            //if I can make a new list, that returns duplicates, then compare that list to the first list, and assign winners based on that?
            /*
             * so like, if list A has:
             *                  2, 4, 6, A, A
             *                  2, 4, 6, A, A
             *                  2, 2, 6, K, A
             *                  2, 2, 6, K, A
             *  that would means that a list of it's duplicates would look like
             *                  2, 4, 6, A, A
             *                  2, 2, 6, K, A
             *  then I can say, if (duplicates[0].Hand.HandValue.PokerHand == ListA[0].Hand.HandValue.PokerHand)
             *                     {
             *                          ListA[0].Hand.HandValue.PlayerState == PlayerState.Winner;
             *                     }
             *                     AND THEN CHECK THAT ACROSS EVERY SINGLE OTHER PLAYER FOR POKERHAND, TOTAL, AND HIGH CARD. UGH.
             *                     
             *What about if I check the first versuse the last. So like, if sortedPlayer[0] != sortedPlayer[3], then I know that they don't ALL have the same hand
             * but that still leaves me in the same position of having to write out a SHIT TON of logic, just backwards (which if I do, should probably be the case).
             * the problem is, the amount of players I may have at a given moment might change depending on the state of the game,
             * so I'm hesitant to hardcose positions in the list, because right now I'm only check for 4 players, but in the game there might be 6, or 10!
             * 
             * /*
             */





            Debug.Log(sortedPlayers[0].SeatPos + " has the best hand with a " + sortedPlayers[0].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[0].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[0].Hand.HandValues.Total);
            Debug.Log(sortedPlayers[1].SeatPos + " has the second best hand with a " + sortedPlayers[1].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[1].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[1].Hand.HandValues.Total);
            Debug.Log(sortedPlayers[2].SeatPos + " has the third best hand with a " + sortedPlayers[2].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[2].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[2].Hand.HandValues.Total);
            Debug.Log(sortedPlayers[3].SeatPos + " has the worst hand with a " + sortedPlayers[3].Hand.HandValues.PokerHand + " with a highCard of " + sortedPlayers[3].Hand.HandValues.HighCard + " and a handTotal of " + sortedPlayers[3].Hand.HandValues.Total);

            sortedPlayers.Clear();
            players.Clear();
            Debug.Log("sortedPlayers has " + sortedPlayers.Count + "players");

        }

    }
}
