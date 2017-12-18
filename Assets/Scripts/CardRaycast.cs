using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRaycast : MonoBehaviour {

	public bool debugHits = false;

	float timeRaycastIsHitting = 0f;
	public float rayDistance = 1000f;
    public bool hittingTable;
    public bool cardIsFaceUp = false;
	private int roof = 13;


    // Update is called once per frame
    void Update () 
	{
		Ray ray = new Ray (transform.position, -transform.forward);
		 
		RaycastHit rayHit;

		if(Physics.Raycast(ray, out rayHit, rayDistance))
		{
            //Debug.Log(rayHit.collider.gameObject.name);
            //Debug.Log(mask + " is what I want to hit");
			if(rayHit.collider.gameObject.layer != roof || rayHit.collider.gameObject.tag != "PlayerFace")
            {
                cardIsFaceUp = false;
				Debug.DrawLine (ray.origin, rayHit.point, Color.red);
            }
            else cardIsFaceUp = true;
            //Debug.Log(rayHit.collider.gameObject.layer + " is being hit by " + gameObject.name);
			Debug.DrawLine(ray.origin, rayHit.point, Color.green);
		}
	}
}
