using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour {

    SpriteRenderer sr;
    Vector4 startColor;
    public GameObject misdealText;
    bool playedSound = false;

	// Use this for initialization
	void Start () {

        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color; //9A9A9AFF
    }
	
	// Update is called once per frame
	void Update () {

        if(Table.gameState == GameState.Misdeal)
        {
            sr.color = Color.black;
            misdealText.SetActive(true);
            if (!playedSound)
            {
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[7], .05f);
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[8], .05f);
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[4], .05f);
                Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.cardTones[5], .05f);
                playedSound = true;
            }
        }
        else
        {
            sr.color = startColor;
            misdealText.SetActive(false);
            playedSound = false;
        }
		
	}
}
