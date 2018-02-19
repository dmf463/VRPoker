using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRaycast : MonoBehaviour {

    public float rayDistance;
    public LayerMask mask;
    Vector3 heightThreshold;

    // Use this for initialization
    void Start () {

        heightThreshold = GameObject.Find("PointerHeightThreshold").transform.position;
		
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.DrawRay(transform.position, transform.forward, Color.green);
        //1. declare your raycast (origin of the array, and then the direction it shoots)
        Ray ray = new Ray(transform.position, transform.forward);

        //2. setup our raycastHit info variable
        RaycastHit rayHit = new RaycastHit();
        //3 we're ready to shoot the raycast
        if (Physics.Raycast(ray, out rayHit, rayDistance, mask))
        {
            if(Services.Dealer.playerToAct != null)
            {
                //Debug.Log("RayHit transform = " + rayHit.transform.gameObject.name);
                if (transform.position.y >= heightThreshold.y)
                {
                    if (rayHit.transform == Services.Dealer.playerToAct.gameObject.GetComponentInChildren<PlayerGazeTrigger>().gameObject.transform) //are we looking at this thing
                    {
                        rayHit.transform.gameObject.GetComponent<PlayerGazeTrigger>().HittingTarget();
                    }
                }
            }
        }
    }
}
