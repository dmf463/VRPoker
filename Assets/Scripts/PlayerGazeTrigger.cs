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
    PokerPlayerRedux pokerPlayer;
    public SpriteRenderer progressImage;
    bool eyeActivated = false;
    float startTime;
    Ray cameraRay;
    RaycastHit cameraRayHit;

    bool callingPulse = false;
    int pingPongCount = 0;
    float emission = 0;
    float glowSpeed = 10;
    float maxGlow = 2;
    Color startColor;

    // Use this for initialization
    void Start()
    {
        pokerPlayer = GetComponentInParent<PokerPlayerRedux>();
        startColor = progressImage.color;
        
        //Debug.Log("PokerPlayer = " + pokerPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        PulseGlow();
        if (!Services.Dealer.OutsideVR)
        {
            ////update our UI image
            //if (pokerPlayer != Services.Dealer.playerToAct)
            //{
            //    //progressImage.fillAmount = 0;
            //    eyeActivated = false;
            //    progressImage.GetComponent<CardIndicatorLerp>().enabled = false;

            //}
            //else
            //{
            //    if (!eyeActivated)
            //    {
            //        startTime = Time.time;
            //        progressImage.GetComponent<Image>().color = startColor;
            //        eyeActivated = true;
            //        if (Table.gameState == GameState.PreFlop) timeSpanForEye = 1.5f;
            //        else timeSpanForEye = .5f;
            //        timeRemaining = timeSpanForEye;
            //        progressImage.GetComponent<CardIndicatorLerp>().enabled = true;
            //        //progressImage.fillAmount = 1;
            //    }
            //}
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
                        if (!pokerPlayer.playerLookedAt)
                        {
                            pokerPlayer.playerLookedAt = true;
                            StartPulse();
                        }
                    }
                    else
                    {
                        if (!pokerPlayer.playerLookedAt)
                        {
                            pokerPlayer.playerLookedAt = true;
                            StartPulse();
                        }
                    }
                }
            }
        }
    }
    public void StartPulse()
    {
        callingPulse = true;
        startTime = Time.time;
        pingPongCount = 0;
        emission = 0;
    }

    public void PulseGlow()
    {
        if (callingPulse)
        {
            Color baseColor = new Vector4(0.8235294f, 0.1574394f, 0.5570934f, 255);
            float previousEmission = emission;
            emission = PingPong(glowSpeed * (startTime - Time.time), 0, maxGlow);
            float currentEmission = emission;
            Color finalColor = (baseColor * Mathf.LinearToGammaSpace(emission));
            finalColor.a = 255;
            progressImage.color = finalColor;
            if (currentEmission < previousEmission)
            {
                pingPongCount++;
            }
            else if (currentEmission > previousEmission && pingPongCount >= 1)
            {
                callingPulse = false;
                progressImage.color = startColor;
                if(!Services.Dealer.OutsideVR) onEyeGazeComplete.Invoke();
                progressImage.color = startColor;
            }
        }
    }

    float PingPong(float time, float minLength, float maxLength)
    {
        return Mathf.PingPong(time, maxLength - minLength) + minLength;
    }
}
