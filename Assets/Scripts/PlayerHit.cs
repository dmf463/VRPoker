using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        GameObject cardDeck = GameObject.FindGameObjectWithTag("CardDeck");
        if(cardDeck != null)
        {
            CardDeckScript deckScript = cardDeck.GetComponent<CardDeckScript>();
            if ((other.gameObject.tag == "PlayingCard" && !deckScript.deckWasThrown) || other.gameObject.tag == "Chip")
            {
                PokerPlayerRedux player = GetComponentInParent<PokerPlayerRedux>();
                //Debug.Log("WE HIT SOMETHING");
                AudioClip hitSound = player.cardHitAudio;
                Services.SoundManager.GetSourceAndPlay(player.playerAudioSource, hitSound);
            }
        }
    }
}
