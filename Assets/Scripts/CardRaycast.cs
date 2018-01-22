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

    //-----------------------//

    private Transform table;

    private void Start()
    {
        table = GameObject.Find("Table").transform;
    }
    // Update is called once per frame
    void Update () 
	{
        Vector3 targetDir = table.position - transform.position;
        float angle = Vector3.Angle(targetDir, -transform.forward);
        if (angle > 90) cardIsFaceUp = true;
        else cardIsFaceUp = false;

        //Debug.Log("cardIsFaceUp = " + cardIsFaceUp + " and angle = " + angle);


        //Debug.Log("cardIsFaceUp = " + cardIsFaceUp);
		//Ray ray = new Ray (transform.position, -transform.forward);
		 
		//RaycastHit rayHit;

		//if(Physics.Raycast(ray, out rayHit, rayDistance))
		//{
  //          //Debug.Log(rayHit.collider.gameObject.name);
  //          //Debug.Log(mask + " is what I want to hit");
		//	if(rayHit.collider.gameObject.layer != roof || rayHit.collider.gameObject.tag != "PlayerFace")
  //          {
  //              cardIsFaceUp = false;
		//		Debug.DrawLine (ray.origin, rayHit.point, Color.red);
  //          }
  //          else cardIsFaceUp = true;
  //          //Debug.Log(rayHit.collider.gameObject.layer + " is being hit by " + gameObject.name);
		//	Debug.DrawLine(ray.origin, rayHit.point, Color.green);
		//}
	}
}
