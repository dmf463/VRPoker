using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManager : MonoBehaviour {

    SpriteRenderer sr;
    Vector4 startColor;
    public GameObject misdealText;
    public GameObject dealTwoCardsText;
    public GameObject lookAtEachPlayer;
    public GameObject boardCards;
    public GameObject whoWon;
    bool playedSound = false;
    public bool inTutorial = false;
    bool tutorialFinished = false;
    

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

        if (inTutorial)
        {
            sr.color = Color.black;
            if (Table.gameState == GameState.Misdeal)
            {
                dealTwoCardsText.SetActive(false);
                lookAtEachPlayer.SetActive(false);
                boardCards.SetActive(false);
                whoWon.SetActive(false);
            }
            else if (Table.gameState == GameState.NewRound)
            {
                sr.color = Color.black;
                dealTwoCardsText.SetActive(true);
            }
            else if (Table.gameState == GameState.PreFlop && Services.SoundManager.roundsFinished == 0) 
            {
                sr.color = Color.black;
                dealTwoCardsText.SetActive(false);
                lookAtEachPlayer.SetActive(true);
            }
            else if(Table.gameState == GameState.PreFlop && Services.SoundManager.roundsFinished == 1)
            {
                sr.color = Color.black;
                lookAtEachPlayer.SetActive(false);
                boardCards.SetActive(true);

            }
            else if(Table.gameState == GameState.Flop && Services.SoundManager.roundsFinished == 1)
            {
                sr.color = Color.black;
                boardCards.SetActive(false);
                lookAtEachPlayer.SetActive(true);
            }
            else if (Table.gameState == GameState.Flop && Services.SoundManager.roundsFinished == 2)
            {
                sr.color = Color.black;
                lookAtEachPlayer.SetActive(false);
                boardCards.SetActive(true);
            }
            else if (Table.gameState == GameState.Turn && Services.SoundManager.roundsFinished == 2)
            {
                sr.color = Color.black;
                boardCards.SetActive(false);
                lookAtEachPlayer.SetActive(true);
            }
            else if (Table.gameState == GameState.Turn && Services.SoundManager.roundsFinished == 3)
            {
                sr.color = Color.black;
                lookAtEachPlayer.SetActive(false);
                boardCards.SetActive(true);
            }
            else if (Table.gameState == GameState.River && Services.SoundManager.roundsFinished == 3)
            {
                sr.color = Color.black;
                boardCards.SetActive(false);
                lookAtEachPlayer.SetActive(true);
            }
            else if(Table.gameState == GameState.ShowDown)
            {
                sr.color = Color.black;
                lookAtEachPlayer.SetActive(false);
                whoWon.SetActive(true);
            }
        }
        else
        {
            if (!tutorialFinished)
            {
                tutorialFinished = true;
                dealTwoCardsText.SetActive(false);
                lookAtEachPlayer.SetActive(false);
                boardCards.SetActive(false);
                whoWon.SetActive(false);
                sr.color = startColor;
            }
        }
	}
}
