using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerGazeTrigger : MonoBehaviour
{

    float timeLookedAt = 0f;
    public float rayDistance;
    public UnityEvent onGazeComplete;
    bool hasActed = false;
    PokerPlayerRedux pokerPlayer;

    // Use this for initialization
    void Start()
    {
        pokerPlayer = GetComponentInParent<PokerPlayerRedux>();
        Debug.Log("PokerPlayer = " + pokerPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("PlayerToAct = " + Services.Dealer.playerToAct);
        //1. declare your raycast (origin of the array, and then the direction it shoots)
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        //2. setup our raycastHit info variable
        RaycastHit rayHit = new RaycastHit();

        //3 we're ready to shoot the raycast
        if (Physics.Raycast(ray, out rayHit, rayDistance))
        {
            if (rayHit.transform == this.transform) //are we looking at this thing
            {
                Debug.Log("Hitting: " + this.gameObject.name);
                timeLookedAt = Mathf.Clamp01(timeLookedAt + Time.deltaTime); //after 1 second, this variable will be 1f;
                if (timeLookedAt/4 == 0.25f && pokerPlayer == Services.Dealer.playerToAct)
                {
                    Debug.Log("Ready to invoke");
                    timeLookedAt = 0f;
                    onGazeComplete.Invoke();
                }
            }
        }

        //update our UI image
        //progressImage.fillAmount = timeLookedAt; //fillAmount is a float from 0-1;

    }
}
