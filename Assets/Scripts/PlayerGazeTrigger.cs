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
    float glowSpeed = 5;
    float maxGlow = 2;
    public Color startColor;

    // Use this for initialization
    void Start()
    {
        pokerPlayer = GetComponentInParent<PokerPlayerRedux>();      
        //Debug.Log("PokerPlayer = " + pokerPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        PulseGlow();
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
                            ResetGazeColor();
                            StartPulse();
                        }
                    }
                    else
                    {
                        if (!pokerPlayer.playerLookedAt)
                        {
                            pokerPlayer.playerLookedAt = true;
                            ResetGazeColor();
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
        ResetGazeColor();
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
            //finalColor.a = 255;
            progressImage.color = finalColor;
            if (currentEmission < previousEmission)
            {
                pingPongCount++;
            }
            else if (currentEmission > previousEmission && pingPongCount >= 1)
            {
                callingPulse = false;
                if(!Services.Dealer.OutsideVR) onEyeGazeComplete.Invoke();
            }
        }
    }

    public void ResetGazeColor()
    {
        progressImage.color = startColor; ;
    }

    float PingPong(float time, float minLength, float maxLength)
    {
        return Mathf.PingPong(time, maxLength - minLength) + minLength;
    }
}
