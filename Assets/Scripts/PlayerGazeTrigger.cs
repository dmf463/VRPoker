using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerGazeTrigger : MonoBehaviour
{

    float timeRemaining;
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
    float startTime;
    Ray cameraRay;
    RaycastHit cameraRayHit;

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
                    startTime = Time.time;
                    eyeActivated = true;
                    if (Table.gameState == GameState.PreFlop) timeSpanForEye = 1.5f;
                    else timeSpanForEye = .5f;
                    timeRemaining = timeSpanForEye;
                    progressImage.GetComponent<CardIndicatorLerp>().enabled = true;
                    progressImage.fillAmount = 1;
                }
            }


        }
    }

    public void HittingTarget()
    {
        cameraRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        cameraRayHit = new RaycastHit();
        if (Physics.Raycast(cameraRay, out cameraRayHit, rayDistance, mask))
        {
            if (cameraRayHit.transform == this.transform)
            {
                if (pokerPlayer == Services.Dealer.playerToAct)
                {
                    if (Table.gameState == GameState.PreFlop)
                    {
                        //timeRemaining = Mathf.Max((timeRemaining - Time.deltaTime), 0); //after 1 second, this variable will be 0f;
                        //progressImage.fillAmount = timeRemaining / timeSpanForEye;
                        ////Debug.Log("time remaining = " + timeRemaining + " and fillAmount = " + progressImage.fillAmount);
                        //if (timeRemaining <= 0.5f && !pokerPlayer.playerLookedAt)
                        //{
                        //    pokerPlayer.playerLookedAt = true;
                        //    //Debug.Log("Ready to invoke next player");
                        //    onEyeGazeComplete.Invoke();
                        //}
                        if (!pokerPlayer.playerLookedAt)
                        {
                            pokerPlayer.playerLookedAt = true;
                            progressImage.fillAmount = 0;
                            onEyeGazeComplete.Invoke();
                        }
                    }
                    else
                    {
                        //timeRemaining = Mathf.Max((timeRemaining - Time.deltaTime), 0);
                        //progressImage.fillAmount = timeRemaining / timeSpanForEye;
                        ////Debug.Log("time remaining = " + timeRemaining + " and fillAmount = " + progressImage.fillAmount);
                        //if (timeRemaining <= 0 && !pokerPlayer.playerLookedAt)
                        //{
                        //    pokerPlayer.playerLookedAt = true;
                        //    Debug.Log("Ready to invoke next player");
                        //    timeRemaining = timeSpanForEye;
                        //    onEyeGazeComplete.Invoke();
                        //}
                        if (!pokerPlayer.playerLookedAt)
                        {
                            pokerPlayer.playerLookedAt = true;
                            progressImage.fillAmount = 0;
                            onEyeGazeComplete.Invoke();
                        }
                    }
                }
            }
        }
    }
}
