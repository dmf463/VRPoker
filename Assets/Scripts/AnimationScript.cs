using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour {

    public Animator CaseyAnim;
    public Animator ZombieAnim;
    public Animator MinnieAnim;
    public Animator NathanielAnim;
    public Animator FloydAnim;

	// Use this for initialization
	void Start () {

        AnimationInit();
    }

    public void AnimationInit()
    {
        CaseyAnim.SetBool("Idle", true);
        ZombieAnim.SetBool("Idle", true);
        MinnieAnim.SetBool("Idle", true);
        NathanielAnim.SetBool("Idle", true);
        FloydAnim.SetBool("Idle", true);
    }

    public void CharacterConvoAnimation(bool playing)
    {
        List<PokerPlayerRedux> playersTalking = new List<PokerPlayerRedux>();
        for (int i = 0; i < Services.Dealer.players.Count; i++)
        {
            if (Services.SoundManager.playersInConvo.Contains(Services.Dealer.players[i].playerName))
            {
                playersTalking.Add(Services.Dealer.players[i]);
            }
        }
        if(playersTalking[0].SeatPos > playersTalking[1].SeatPos)
        {
            ConvoAnimation(playersTalking[0].playerName, "TalkingLeft", playing);
            ConvoAnimation(playersTalking[1].playerName, "TalkingRight", playing);
        }
        else
        {
            ConvoAnimation(playersTalking[0].playerName, "TalkingRight", playing);
            ConvoAnimation(playersTalking[1].playerName, "TalkingLeft", playing);
        }

        if (Services.SoundManager.playersInConvo.Contains(PlayerName.Casey) && !Services.SoundManager.playersInConvo.Contains(PlayerName.Floyd))
        {
            ConvoAnimation(PlayerName.Casey, "TalkingRight", playing);
        }
        else if (Services.SoundManager.playersInConvo.Contains(PlayerName.Floyd) && !Services.SoundManager.playersInConvo.Contains(PlayerName.Casey))
        {
            ConvoAnimation(PlayerName.Floyd, "TalkingLeft", playing);
        }
    }

    public void ConvoAnimation(PlayerName name, string anim, bool setBool)
    {
        switch (name)
        {
            case PlayerName.None:
                break;
            case PlayerName.Casey:
                CaseyAnim.SetBool(anim, setBool);
                break;
            case PlayerName.Zombie:
                ZombieAnim.SetBool(anim, setBool);
                break;
            case PlayerName.Minnie:
                MinnieAnim.SetBool(anim, setBool);
                break;
            case PlayerName.Nathaniel:
                NathanielAnim.SetBool(anim, setBool);
                break;
            case PlayerName.Floyd:
                FloydAnim.SetBool(anim, setBool);
                break;
            default:
                break;
        }
    }

    public void ActionAnimation(PlayerName name, string animationName)
    {
        switch (name)
        {
            case PlayerName.None:
                break;
            case PlayerName.Casey:
                //CaseyAnim.ResetTrigger(animationName);
                CaseyAnim.SetTrigger(animationName);
                break;
            case PlayerName.Zombie:
                //ZombieAnim.ResetTrigger(animationName);
                ZombieAnim.SetTrigger(animationName);
                break;
            case PlayerName.Minnie:
                //MinnieAnim.ResetTrigger(animationName);
                MinnieAnim.SetTrigger(animationName);
                break;
            case PlayerName.Nathaniel:
               // NathanielAnim.ResetTrigger(animationName);
                NathanielAnim.SetTrigger(animationName);
                break;
            case PlayerName.Floyd:
                //FloydAnim.ResetTrigger(animationName);
                FloydAnim.SetTrigger(animationName);
                break;
            default:
                break;
        }
    }
}
