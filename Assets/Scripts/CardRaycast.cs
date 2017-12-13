using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRaycast : MonoBehaviour {

	public bool debugHits = false;

	float timeRaycastIsHitting = 0f;
	public float rayDistance = 1000f;
    public bool hittingTable;
    public bool cardIsFaceUp = false;
    public LayerMask mask;
    private int table = 12;
    private int card = 8;
    private int floor = 14;
    private int chip = 9;
    private int player = 11;


    // Update is called once per frame
    void Update () 
	{
		Ray ray = new Ray (transform.position, -transform.forward);
		 
		RaycastHit rayHit;

		if(Physics.Raycast(ray, out rayHit, rayDistance))
		{
            //Debug.Log(rayHit.collider.gameObject.name);
            //Debug.Log(mask + " is what I want to hit");
            //Debug.Log(rayHit.collider.gameObject.layer + " is what I'm hitting");
            if (rayHit.collider.gameObject.layer == table || 
                rayHit.collider.gameObject.layer == card || 
                rayHit.collider.gameObject.layer == floor ||
                rayHit.collider.gameObject.layer == player ||
                rayHit.collider.gameObject.layer == chip)
            {
                cardIsFaceUp = false;
            }
            else cardIsFaceUp = true;
            //Debug.Log(rayHit.collider.gameObject.layer + " is being hit by " + gameObject.name);
			//Debug.DrawLine(ray.origin, rayHit.point, Color.green);
		}
		else
		{
			//Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red);
		}
	}
}
