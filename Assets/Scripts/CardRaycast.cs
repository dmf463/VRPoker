using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRaycast : MonoBehaviour {

	public bool debugHits = false;

	float timeRaycastIsHitting = 0f;
	public float rayDistance = 100f;
    public bool hittingTable;
    public bool cardIsFaceUp = false;
    public LayerMask mask;


    // Update is called once per frame
    void Update () 
	{
		Ray ray = new Ray (transform.position, -transform.forward);
		 
		RaycastHit rayHit;

		if(Physics.Raycast(ray, out rayHit, rayDistance))
		{
			//if(rayHit.collider.gameObject.layer == 10 || rayHit.collider.gameObject.layer == 11)
			//{
			//	if(debugHits == true)
			//	{
			//		Debug.Log("DEAR GOD, " + rayHit.collider.gameObject.name + " CAN SEE ME!");
			//	}
			//}
            if (rayHit.collider.gameObject.layer == mask)
            {
                cardIsFaceUp = true;
            }
            //else hittingTable = false;
			Debug.DrawLine(ray.origin, rayHit.point, Color.green);
		}
		else
		{
			//Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red);
		}
	}
}
