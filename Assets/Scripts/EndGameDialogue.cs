using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameDialogue {

    public TaskManager tm;
    public PokerPlayerRedux Casey;
    public PokerPlayerRedux Zombie;
    public PokerPlayerRedux Minnie;
    public PokerPlayerRedux Nathaniel;
    public PokerPlayerRedux Floyd;


    public GameObject creditsObj;
    Vector3 creditsStartPos;
    Vector3 creditsEndPos;

    const float WAIT_BEFORE_START = 1f;
    const float CREDITS_OFFSET = 30f;
    const float CREDITS_DURATION = 30f;

    public void Init()
    {
        tm = new TaskManager();
        creditsObj = Services.Dealer.credits;
        creditsStartPos = creditsObj.transform.position;
        creditsObj.SetActive(false);
        creditsEndPos = new Vector3(creditsStartPos.x, creditsStartPos.y + CREDITS_OFFSET, creditsStartPos.z);
    }

    public void ClosingCutScene(PokerPlayerRedux winner, PokerPlayerRedux loser)
    {
        if(winner.playerName == PlayerName.Floyd)
        {
            if (loser.playerName == PlayerName.Minnie) Floyd_Wins_Against_Minnie();
            else if (loser.playerName == PlayerName.Casey) Floyd_Wins_Against_Casey();
            else if (loser.playerName == PlayerName.Nathaniel) Floyd_Wins_Against_Nathaniel();
            else if (loser.playerName == PlayerName.Zombie) Floyd_Wins_Against_Zombie();
            else Debug.Log("Error: No Loser Found");
        }
        else if(winner.playerName == PlayerName.Minnie)
        {
            if (loser.playerName == PlayerName.Casey) Minnie_Wins_Against_Casey();
            else if (loser.playerName == PlayerName.Nathaniel) Minnie_Wins_Against_Nathaniel();
            else if (loser.playerName == PlayerName.Zombie) Minnie_Wins_Against_Zombie();
            else if (loser.playerName == PlayerName.Floyd) Floyd_Loses_Against_Minnie();
            else Debug.Log("Error: No Loser Found");
        }
        else if(winner.playerName == PlayerName.Casey)
        {
            if (loser.playerName == PlayerName.Floyd) Floyd_Loses_Against_Casey();
            else if (loser.playerName == PlayerName.Minnie) Minnie_Loses_Against_Casey();
            else if (loser.playerName == PlayerName.Nathaniel) Casey_Wins_Against_Nathaniel();
            else if (loser.playerName == PlayerName.Zombie) Casey_Wins_Against_Zombie();
        }
        else if(winner.playerName == PlayerName.Nathaniel)
        {
            if (loser.playerName == PlayerName.Casey) Casey_Loses_Against_Nathaniel();
            else if (loser.playerName == PlayerName.Zombie) Zombie_Loses_Against_Nathaniel();
            else if (loser.playerName == PlayerName.Minnie) Minnie_Loses_Against_Nathaniel();
            else if (loser.playerName == PlayerName.Floyd) Floyd_Loses_Against_Nathaniel();
        }
        else if(winner.playerName == PlayerName.Zombie)
        {
            if (loser.playerName == PlayerName.Casey) Casey_Loses_Against_Zombie();
            else if (loser.playerName == PlayerName.Minnie) Minnie_Loses_Against_Zombie();
            else if (loser.playerName == PlayerName.Nathaniel) Zombie_Wins_Against_Nathaniel();
            else if (loser.playerName == PlayerName.Floyd) Floyd_Loses_Against_Zombie();
        }
    }

    public void Floyd_Wins_Against_Minnie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Minnie_Floyd_Win);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Floyd_Minnue_Minnie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Floyd_Line.Then
            (new Wait(Services.SoundManager.Floyd_Minnie_Floyd_Win.length)).Then
            (Minnie_Line).Then
            (new Wait(Services.SoundManager.Floyd_Minnue_Minnie_Lose.length)).Then
            (Credits);

        tm.Do(Floyd_Line);  
    }

    public void Floyd_Loses_Against_Minnie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Floyd_Minnie_Minnie_Win);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Minnie_Floyd_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Minnie_Line.Then
            (new Wait(Services.SoundManager.Floyd_Minnie_Minnie_Win.length)).Then
            (Floyd_Line).Then
            (new Wait(Services.SoundManager.Floyd_Minnie_Floyd_Lose.length)).Then
            (Credits);

        tm.Do(Minnie_Line);
    }

    public void Floyd_Wins_Against_Casey()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Casey_Floyd_Win);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Floyd_Casey_Casey_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Floyd_Line.Then
            (new Wait(Services.SoundManager.Floyd_Casey_Floyd_Win.length)).Then
            (Casey_Line).Then
            (new Wait(Services.SoundManager.Floyd_Casey_Casey_Lose.length)).Then
            (Credits);

        tm.Do(Floyd_Line);
    }

    public void Floyd_Loses_Against_Casey()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Floyd_Casey_Casey_Win);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Casey_Floyd_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Casey_Line.Then
            (new Wait(Services.SoundManager.Floyd_Casey_Casey_Win.length)).Then
            (Floyd_Line).Then
            (new Wait(Services.SoundManager.Floyd_Casey_Floyd_Lose.length)).Then
            (Credits);

        tm.Do(Casey_Line);
    }

    public void Floyd_Wins_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Nathaniel_Floyd_Win);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Floyd_Nathaniel_Nathaniel_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Floyd_Line.Then
            (new Wait(Services.SoundManager.Floyd_Nathaniel_Floyd_Win.length)).Then
            (Nathaniel_Line).Then
            (new Wait(Services.SoundManager.Floyd_Nathaniel_Nathaniel_Lose.length)).Then
            (Credits);
        tm.Do(Floyd_Line);
    }

    public void Floyd_Loses_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Floyd_Nathaniel_Nathaniel_Win);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Nathaniel_Floyd_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Nathaniel_Line.Then
            (new Wait(Services.SoundManager.Floyd_Nathaniel_Nathaniel_Win.length)).Then
            (Floyd_Line).Then
            (new Wait(Services.SoundManager.Floyd_Nathaniel_Floyd_Lose.length)).Then
            (Credits);

        tm.Do(Nathaniel_Line);
    }

    public void Floyd_Wins_Against_Zombie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Zombie_Floyd_Win);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Floyd_Zombie_Zombie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Floyd_Line.Then
            (new Wait(Services.SoundManager.Floyd_Zombie_Floyd_Win.length)).Then
            (Zombie_Line).Then
            (new Wait(Services.SoundManager.Floyd_Zombie_Zombie_Lose.length)).Then
            (Credits);

        tm.Do(Floyd_Line);
    }

    public void Floyd_Loses_Against_Zombie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Floyd_Zombie_Zombie_Win);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Zombie_Floyd_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Zombie_Line.Then
            (new Wait(Services.SoundManager.Floyd_Zombie_Zombie_Win.length)).Then
            (Floyd_Line).Then
            (new Wait(Services.SoundManager.Floyd_Zombie_Floyd_Lose.length)).Then
            (Credits);

        tm.Do(Zombie_Line);
    }

    public void Minnie_Wins_Against_Casey()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Minnie_Casey_Minnie_Win);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Minnie_Casey_Casey_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Minnie_Line.Then
            (new Wait(Services.SoundManager.Minnie_Casey_Minnie_Win.length)).Then
            (Casey_Line).Then
            (new Wait(Services.SoundManager.Minnie_Casey_Casey_Lose.length)).Then
            (Credits);

        tm.Do(Minnie_Line);
    }

    public void Minnie_Loses_Against_Casey()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Minnie_Casey_Casey_Win);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Minnie_Casey_Minnie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Casey_Line.Then
            (new Wait(Services.SoundManager.Minnie_Casey_Casey_Win.length)).Then
            (Minnie_Line).Then
            (new Wait(Services.SoundManager.Minnie_Casey_Minnie_Lose.length)).Then
            (Credits);

        tm.Do(Casey_Line);
    }

    public void Minnie_Wins_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Minnie_Nathaniel_Minnie_Win);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Minnie_Nathaniel_Nathaniel_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Minnie_Line.Then
            (new Wait(Services.SoundManager.Minnie_Nathaniel_Minnie_Win.length)).Then
            (Nathaniel_Line).Then
            (new Wait(Services.SoundManager.Minnie_Nathaniel_Nathaniel_Lose.length)).Then
            (Credits);

        tm.Do(Minnie_Line);
    }

    public void Minnie_Loses_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Minnie_Nathaniel_Nathaniel_Win);
        PlayPlayerLine Minnnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Minnie_Nathaniel_Minnie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Nathaniel_Line.Then
            (new Wait(Services.SoundManager.Minnie_Nathaniel_Nathaniel_Win.length)).Then
            (Minnnie_Line).Then
            (new Wait(Services.SoundManager.Minnie_Nathaniel_Minnie_Lose.length)).Then
            (Credits);

        tm.Do(Nathaniel_Line);
    }

    public void Minnie_Wins_Against_Zombie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Minnie_Zombie_Minnie_Win);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Minnie_Zombie_Zombie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Minnie_Line.Then
            (new Wait(Services.SoundManager.Minnie_Zombie_Minnie_Win.length)).Then
            (Zombie_Line).Then
            (new Wait(Services.SoundManager.Minnie_Zombie_Zombie_Lose.length)).Then
            (Credits);

        tm.Do(Minnie_Line);
    }

    public void Minnie_Loses_Against_Zombie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Minnie_Zombie_Zombie_Win);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Minnie_Zombie_Minnie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Zombie_Line.Then
            (new Wait(Services.SoundManager.Minnie_Zombie_Zombie_Win.length)).Then
            (Minnie_Line).Then
            (new Wait(Services.SoundManager.Minnie_Zombie_Minnie_Lose.length)).Then
            (Credits);

        tm.Do(Zombie_Line);
    }

    public void Casey_Wins_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Casey_Nathaniel_Casey_Win);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Casey_Nathaniel_Nathaniel_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Casey_Line.Then
            (new Wait(Services.SoundManager.Casey_Nathaniel_Casey_Win.length)).Then
            (Nathaniel_Line).Then
            (new Wait(Services.SoundManager.Casey_Nathaniel_Nathaniel_Lose.length)).Then
            (Credits);

        tm.Do(Casey_Line);
    }

    public void Casey_Loses_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Casey_Nathaniel_Nathaniel_Win);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Casey_Nathaniel_Casey_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Nathaniel_Line.Then
            (new Wait(Services.SoundManager.Casey_Nathaniel_Nathaniel_Win.length)).Then
            (Casey_Line).Then
            (new Wait(Services.SoundManager.Casey_Nathaniel_Casey_Lose.length)).Then
            (Credits);

        tm.Do(Nathaniel_Line);
    }

    public void Casey_Wins_Against_Zombie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Casey_Zombie_Casey_Win);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Casey_Zombie_Zombie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Casey_Line.Then
            (new Wait(Services.SoundManager.Casey_Zombie_Casey_Win.length)).Then
            (Zombie_Line).Then
            (new Wait(Services.SoundManager.Casey_Zombie_Zombie_Lose.length)).Then
            (Credits);

        tm.Do(Casey_Line);
    }

    public void Casey_Loses_Against_Zombie()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Casey_Zombie_Zombie_Win);
        PlayPlayerLine Casey_Line = new PlayPlayerLine(Casey, Services.SoundManager.Casey_Zombie_Casey_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Zombie_Line.Then
            (new Wait(Services.SoundManager.Casey_Zombie_Zombie_Win.length)).Then
            (Casey_Line).Then
            (new Wait(Services.SoundManager.Casey_Zombie_Casey_Lose.length)).Then
            (Credits);

        tm.Do(Zombie_Line);
    }

    public void Zombie_Wins_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Zombie_Nathaniel_Zombie_Win);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Zombie_Nathaniel_Nathaniel_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Zombie_Line.Then
            (new Wait(Services.SoundManager.Zombie_Nathaniel_Zombie_Win.length)).Then
            (Nathaniel_Line).Then
            (new Wait(Services.SoundManager.Zombie_Nathaniel_Nathaniel_Lose.length)).Then
            (Credits);

        tm.Do(Zombie_Line);
    }

    public void Zombie_Loses_Against_Nathaniel()
    {
        Wait waitSecond = new Wait(WAIT_BEFORE_START);
        PlayPlayerLine Nathaniel_Line = new PlayPlayerLine(Nathaniel, Services.SoundManager.Zombie_Nathaniel_Nathaniel_Win);
        PlayPlayerLine Zombie_Line = new PlayPlayerLine(Zombie, Services.SoundManager.Zombie_Nathaniel_Zombie_Lose);
        RollCredits Credits = new RollCredits(creditsObj, creditsStartPos, creditsEndPos, CREDITS_DURATION);

        Nathaniel_Line.Then
            (new Wait(Services.SoundManager.Zombie_Nathaniel_Nathaniel_Win.length)).Then
            (Zombie_Line).Then
            (new Wait(Services.SoundManager.Zombie_Nathaniel_Zombie_Lose.length)).Then
            (Credits);

        tm.Do(Nathaniel_Line);
    }

}
