﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        GameObject cardDeck = GameObject.FindGameObjectWithTag("CardDeck");
        if (cardDeck != null)
        {
            CardDeckScript deckScript = cardDeck.GetComponent<CardDeckScript>();
            if ((other.gameObject.tag == "PlayingCard" && !deckScript.deckWasThrown) || other.gameObject.tag == "Chip" || other.gameObject.tag == "Hand")
            {
                PokerPlayerRedux player = GetComponentInParent<PokerPlayerRedux>();
                if (other.gameObject.tag == "Hand" && player.chipCount == 0 && player.PlayerState == PlayerState.Loser && Services.Dealer.playerHasBeenEliminated)
                {
                    //DAN PUT IN THE LINE FOR IF DEALER BUYS THEM BACK IN HERE
                    Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(player.playerName, LineCriteria.BuyIn));
                    Table.instance.AddChipTo(Table.instance.playerDestinations[player.SeatPos], Services.Dealer.tipCount);
                    Services.Dealer.playerHasBeenEliminated = false;
                    Services.Dealer.tipCount = 0;
                }
                else if(other.gameObject.tag == "Hand")
                {
                    //DAN PUT THE LINE FOR TOUCHING THEM HERE
                    Services.SoundManager.PlayOneLiner(DialogueDataManager.CreatePlayerLineCriteria(player.playerName, LineCriteria.TouchedByHand));
                }
                else
                {
                    AudioClip hitSound = player.cardHitAudio;
                    Table.gameState = GameState.Misdeal;
                }
            }
        }
    }
}
