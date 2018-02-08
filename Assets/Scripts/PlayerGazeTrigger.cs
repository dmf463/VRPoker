using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerGazeTrigger : MonoBehaviour
{

    float timeRemaining;
    float timeSpanForQuestion = 3;
    float timeSpanForEye;
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
                        timeRemaining = Mathf.Max((timeRemaining - Time.deltaTime), 0); //after 1 second, this variable will be 0f;
                        progressImage.fillAmount = timeRemaining / timeSpanForEye;
                        if (timeRemaining == 0 && !pokerPlayer.playerLookedAt)
                        {
                            pokerPlayer.playerLookedAt = true;
                            Debug.Log("Ready to invoke next player");
                            timeRemaining = timeSpanForEye;
                            progressImage.fillAmount = 0;
                            onEyeGazeComplete.Invoke();
                            //Debug.Log("player that just invoked was " + pokerPlayer);
                            //Debug.Log("next player to act is " + Services.Dealer.playerToAct);

                            if (Services.SoundManager.haveLookedAtFirstPlayer != true)
                            {
                                Services.SoundManager.haveLookedAtFirstPlayer = true;
                            }
                        }
                    }
                    else if (pokerPlayer.PlayerState == PlayerState.Winner || pokerPlayer.PlayerState == PlayerState.Loser)
                    {
                        Debug.Log("WE GETTIN INTO THE RAYCAST WITH " + pokerPlayer.playerName + " AS A " + pokerPlayer.PlayerState);
                        if (rayHit.transform == this.transform) //are we looking at this thing
                        {
                            timeRemaining = Mathf.Max((timeRemaining - Time.deltaTime), 0);
                            questionMark.fillAmount = timeRemaining / timeSpanForQuestion;
                            if (timeRemaining == 0 && !pokerPlayer.playerLookedAt)
                            {
                                pokerPlayer.playerLookedAt = true;
                                Debug.Log("Ready to invoke question mark");
                                timeRemaining = timeSpanForQuestion;
                                progressImage.fillAmount = 0;
                                onQuestionMarkGazeComplete.Invoke();
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
                    if (Table.gameState == GameState.PreFlop) timeSpanForEye = 1;
                    else timeSpanForEye = 1;
                    timeRemaining = timeSpanForEye;
                    eyeActivated = true;
                    progressImage.GetComponent<CardIndicatorLerp>().enabled = true;
                    progressImage.fillAmount = 1;
                }
            }

            if (pokerPlayer.PlayerState != PlayerState.Winner && pokerPlayer.PlayerState != PlayerState.Loser)
            {
                //Debug.Log("getting into the first part on f click");
                questionMark.fillAmount = 0;
                questionMarkActivated = false;
                questionMark.GetComponent<CardIndicatorLerp>().enabled = false;
            }
            else
            {
                if (!questionMarkActivated && Services.Dealer.readyToAwardPlayers)
                {
                    timeRemaining = timeSpanForQuestion;
                    Debug.Log(pokerPlayer.playerName + " is a winner or loser");
                    questionMarkActivated = true;
                    questionMark.GetComponent<CardIndicatorLerp>().enabled = true;
                    questionMark.fillAmount = 1;
                }
            }
        }
    }
}
