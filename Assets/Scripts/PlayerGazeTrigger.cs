using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerGazeTrigger : MonoBehaviour
{

    float timeLookedAt = 0f;
    public float rayDistance;
	public LayerMask mask;
    public UnityEvent onGazeComplete;
    PokerPlayerRedux pokerPlayer;

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
                    timeLookedAt = Mathf.Clamp01(timeLookedAt + Time.deltaTime); //after 1 second, this variable will be 1f;
                    if (timeLookedAt / 2 == 0.5f && pokerPlayer == Services.Dealer.playerToAct && !pokerPlayer.playerLookedAt)
                    {
                        pokerPlayer.playerLookedAt = true;
                        //Debug.Log("Ready to invoke");
                        timeLookedAt = 0f;
                        onGazeComplete.Invoke();
                        //Debug.Log("player that just invoked was " + pokerPlayer);
                        //Debug.Log("next player to act is " + Services.Dealer.playerToAct);
                    }
                }
            }

            //update our UI image
            //progressImage.fillAmount = timeLookedAt; //fillAmount is a float from 0-1;
        }
    }
}
