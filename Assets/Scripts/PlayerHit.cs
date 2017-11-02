using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHit : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayingCard" || other.gameObject.tag == "Chip")
        {
            Debug.Log("WE HIT SOMETHING");
            AudioClip hitSound = GetComponentInParent<PokerPlayerRedux>().cardHitAudio;
            Services.SoundManager.GenerateSourceAndPlay(hitSound);
        }
    }
}
