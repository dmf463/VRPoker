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

        CaseyAnim.SetBool("Idle", true);
        ZombieAnim.SetBool("Idle", true);
        MinnieAnim.SetBool("Idle", true);
        NathanielAnim.SetBool("Idle", true);
        FloydAnim.SetBool("Idle", true); 

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MinnieAnim.SetBool("Idle", false);
            MinnieAnim.SetBool("TalkingLeft", true);
        }
    }

    public void ActionAnimation(PlayerName name, string animationName)
    {
        switch (name)
        {
            case PlayerName.None:
                break;
            case PlayerName.Casey:
                CaseyAnim.SetTrigger(animationName);
                break;
            case PlayerName.Zombie:
                ZombieAnim.SetTrigger(animationName);
                break;
            case PlayerName.Minnie:
                MinnieAnim.SetTrigger(animationName);
                break;
            case PlayerName.Nathaniel:
                NathanielAnim.SetTrigger(animationName);
                break;
            case PlayerName.Floyd:
                FloydAnim.SetTrigger(animationName);
                break;
            default:
                break;
        }
    }
}
