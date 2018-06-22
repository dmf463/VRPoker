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

    public void Init()
    {
        tm = new TaskManager();
    }

    public void Floyd_Wins_Against_Minnie()
    {
        Wait waitSecond = new Wait(1);
        PlayPlayerLine Floyd_Line = new PlayPlayerLine(Floyd, Services.SoundManager.Floyd_Minnie_Floyd_Win);
        PlayPlayerLine Minnie_Line = new PlayPlayerLine(Minnie, Services.SoundManager.Floyd_Minnue_Minnie_Lose);
        RollCredits Credits = new RollCredits();

        Floyd_Line.Then
            (Minnie_Line).Then
            (Credits);

        tm.Do(Floyd_Line);  
    }

}
