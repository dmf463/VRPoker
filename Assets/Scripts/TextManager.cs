using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public List<TextMeshPro> playerBetText;
    float alphaTest = 0;
    

	// Use this for initialization
	void Start () {
        Init();
        sr = GetComponent<SpriteRenderer>();
        startColor = sr.color; //9A9A9AFF
    }
	
	// Update is called once per frame
	void Update () {

        ActivateMisdealText();
        RunTutorial();
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            alphaTest += .01f;
            foreach (TextMeshPro t in playerBetText)
            {
                t.color = new Vector4(t.color.r, t.color.g, t.color.b, alphaTest);
            }
        }
	}

    public void Init()
    {
        foreach (TextMeshPro t in playerBetText)
        {
            t.text = "test";
            t.color = new Vector4(t.color.r, t.color.g, t.color.b, 0);
        }
    }

    public void ActivateMisdealText()
    {
        if (Table.gameState == GameState.Misdeal)
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

    public void RunTutorial()
    {
        if (inTutorial)
        {
            sr.color = Color.black;
            if (Table.gameState == GameState.Misdeal)
            {
                dealTwoCardsText.SetActive(false);
                lookAtEachPlayer.SetActive(false);
                boardCards.SetActive(false);
                whoWon.SetActive(false);
                Services.SoundManager.roundsFinished = 0;
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
            else if (Table.gameState == GameState.PreFlop && Services.SoundManager.roundsFinished == 1)
            {
                sr.color = Color.black;
                lookAtEachPlayer.SetActive(false);
                boardCards.SetActive(true);

            }
            else if (Table.gameState == GameState.Flop && Services.SoundManager.roundsFinished == 1)
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
            else if (Table.gameState == GameState.ShowDown)
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

    public void ShowBetAmount(int seatPos, int bet)
    {
        playerBetText[seatPos].text = bet.ToString();
        Debug.Log("alpha = " + playerBetText[seatPos].color.a);
        StartCoroutine(LerpTextAlpha(Time.time, 3, 0, 1, playerBetText[seatPos]));
    }

    IEnumerator LerpTextAlpha(float startTime, float speed, float startAlpha, float targetAlpha, TextMeshPro text)
    {
        while (!Mathf.Approximately(text.color.a, targetAlpha))
        {
            Debug.Log("alpha = " + text.color.a);
            float timeElapsed = (Time.time - startTime) * speed;
            float fracCovered = timeElapsed / Mathf.Abs(targetAlpha - startAlpha);
            text.color = new Vector4(text.color.r, text.color.g, text.color.b, (Mathf.Lerp(startAlpha, targetAlpha, fracCovered)));
            yield return null;
        }
        StartCoroutine(LerpTextDown(Time.time, speed, 1, 0, text));
        yield break;
    }

    IEnumerator LerpTextDown(float startTime, float speed, float startAlpha, float targetAlpha, TextMeshPro text)
    {
        while (!Mathf.Approximately(text.color.a, targetAlpha))
        {
            Debug.Log("alpha = " + text.color.a);
            float timeElapsed = (Time.time - startTime) * speed;
            float fracCovered = timeElapsed / Mathf.Abs(targetAlpha - startAlpha);
            text.color = new Vector4(text.color.r, text.color.g, text.color.b, (Mathf.Lerp(startAlpha, targetAlpha, fracCovered)));
            yield return null;
        }
        yield break;
    }
}
