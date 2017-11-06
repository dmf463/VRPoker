using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour
{
    public CardDeckScript deckScript;

    private void OnTriggerEnter(Collider other)
    {
        GameObject cardDeck = GameObject.FindGameObjectWithTag("CardDeck");
        if(cardDeck != null)
        {
            deckScript = cardDeck.GetComponent<CardDeckScript>();
            if ((other.gameObject.tag == "PlayingCard" && !deckScript.deckWasThrown) || other.gameObject.tag == "Chip")
            {
                Debug.Log("WE HIT SOMETHING");
                AudioClip hitSound = GetComponentInParent<PokerPlayerRedux>().cardHitAudio;
                Services.SoundManager.GenerateSourceAndPlay(hitSound);
            }
        }
    }
}
