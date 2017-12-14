using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerGazeTrigger : MonoBehaviour
{

    float timeLookedAt = 1f;
    public float rayDistance;
	public LayerMask mask;
    public UnityEvent onGazeComplete;
    PokerPlayerRedux pokerPlayer;
    public Image progressImage;
    public Image progressImageBackground;
    bool activated = false;

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
                    //Debug.Log("Hitting: " + this.gameObject.name);
                    timeLookedAt = Mathf.Clamp01(timeLookedAt - Time.deltaTime); //after 1 second, this variable will be 0f;
                    progressImage.fillAmount = timeLookedAt;
                    if (timeLookedAt == 0 && pokerPlayer == Services.Dealer.playerToAct && !pokerPlayer.playerLookedAt)
                    {
                        pokerPlayer.playerLookedAt = true;
                        Debug.Log("Ready to invoke");
                        timeLookedAt = 1f;
                        progressImage.fillAmount = 0;
                        onGazeComplete.Invoke();
                        //Debug.Log("player that just invoked was " + pokerPlayer);
                        //Debug.Log("next player to act is " + Services.Dealer.playerToAct);
                    }
                }
            }

            //update our UI image
            if (pokerPlayer != Services.Dealer.playerToAct)
            {
                progressImage.fillAmount = 0;
                activated = false;
                progressImage.GetComponent<CardIndicatorLerp>().enabled = false;
               
            }
            else
            {
                if (!activated)
                {
                    activated = true;
                    progressImage.GetComponent<CardIndicatorLerp>().enabled = true;
                   
                    progressImage.fillAmount = 1;
                }
            }
            //else progressImage.fillAmount = timeLookedAt * 2; //fillAmount is a float from 0-1;
        }
    }
}
