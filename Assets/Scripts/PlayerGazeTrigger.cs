using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerGazeTrigger : MonoBehaviour
{

    float timeLookedAtForEye = 1f;
    float timeLookedAtForQuestion = 2f;
    public float rayDistance;
    public LayerMask mask;
    public UnityEvent onEyeGazeComplete;
    public UnityEvent onQuestionMarkGazeComplete;
    PokerPlayerRedux pokerPlayer;
    public Image progressImage;
    public Image questionMark;
    bool eyeActivated = false;
    bool questionMarkActivated = false;

    // Use this for initialization
    void Start()
    {
        pokerPlayer = GetComponentInParent<PokerPlayerRedux>();
        //Debug.Log("PokerPlayer = " + pokerPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Services.Dealer.OutsideVR)
        {
            //Debug.Log("PlayerToAct = " + Services.Dealer.playerToAct);
            //1. declare your raycast (origin of the array, and then the direction it shoots)
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            //2. setup our raycastHit info variable
            RaycastHit rayHit = new RaycastHit();
            //3 we're ready to shoot the raycast
            if (Physics.Raycast(ray, out rayHit, rayDistance, mask))
            {
                if (rayHit.transform == this.transform) //are we looking at this thing
                {
                    if (pokerPlayer == Services.Dealer.playerToAct)
                    {
                        //Debug.Log("Hitting: " + this.gameObject.name);
                        timeLookedAtForEye = Mathf.Clamp01(timeLookedAtForEye - Time.deltaTime); //after 1 second, this variable will be 0f;
                        progressImage.fillAmount = timeLookedAtForEye;
                        if (timeLookedAtForEye == 0 && pokerPlayer == Services.Dealer.playerToAct && !pokerPlayer.playerLookedAt)
                        {
                            pokerPlayer.playerLookedAt = true;
                            Debug.Log("Ready to invoke next player");
                            timeLookedAtForEye = 1f;
                            progressImage.fillAmount = 0;
                            onEyeGazeComplete.Invoke();
                            //Debug.Log("player that just invoked was " + pokerPlayer);
                            //Debug.Log("next player to act is " + Services.Dealer.playerToAct);
                        }
                    }
                    else if (pokerPlayer.PlayerState == PlayerState.Winner || pokerPlayer.PlayerState == PlayerState.Loser)
                    {
                        if (rayHit.transform == this.transform) //are we looking at this thing
                        {
                            //Debug.Log("Hitting: " + this.gameObject.name);
                            //timeLookedAt = Mathf.Clamp01(timeLookedAt - Time.deltaTime); //after 1 second, this variable will be 0f;
                            timeLookedAtForQuestion = Mathf.Clamp(timeLookedAtForQuestion - Time.deltaTime, 0, 2);
                            questionMark.fillAmount = timeLookedAtForQuestion / 2;
                            if (timeLookedAtForEye == 0 && (pokerPlayer.PlayerState == PlayerState.Winner || pokerPlayer.PlayerState == PlayerState.Loser) && !pokerPlayer.playerLookedAt)
                            {
                                pokerPlayer.playerLookedAt = true;
                                Debug.Log("Ready to invoke question mark");
                                timeLookedAtForQuestion = 2f;
                                progressImage.fillAmount = 0;
                                onQuestionMarkGazeComplete.Invoke();
                                //Debug.Log("player that just invoked was " + pokerPlayer);
                                //Debug.Log("next player to act is " + Services.Dealer.playerToAct);
                            }
                            else timeLookedAtForEye = 2;
                        }
                    }
                }
            }
        }

        //update our UI image
        if (pokerPlayer != Services.Dealer.playerToAct)
        {
            progressImage.fillAmount = 0;
            eyeActivated = false;
            progressImage.GetComponent<CardIndicatorLerp>().enabled = false;

        }
        else
        {
            if (!eyeActivated)
            {
                eyeActivated = true;
                progressImage.GetComponent<CardIndicatorLerp>().enabled = true;
                progressImage.fillAmount = 1;
            }
        }

        if (pokerPlayer.PlayerState != PlayerState.Winner || pokerPlayer.PlayerState != PlayerState.Loser)
        {
            questionMark.fillAmount = 0;
            questionMarkActivated = false;
            questionMark.GetComponent<CardIndicatorLerp>().enabled = false;
        }
        else
        {
            if (!questionMarkActivated)
            {
                Debug.Log(pokerPlayer.playerName + " is a winner or loser");
                questionMarkActivated = true;
                questionMark.GetComponent<CardIndicatorLerp>().enabled = true;
                questionMark.fillAmount = 1;
            }
        }
    }
}
