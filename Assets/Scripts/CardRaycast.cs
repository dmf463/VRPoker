using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRaycast : MonoBehaviour {

	float timeRaycastIsHitting = 0f;
	public float rayDistance = 100f;

	public LayerMask mask;


	// Update is called once per frame
	void Update () {
		Ray ray = new Ray (transform.position, -transform.forward);

		RaycastHit rayHit;

		if(Physics.Raycast(ray, out rayHit, rayDistance, mask))
		{
			//print(rayHit.collider.gameObject.name);
			Debug.DrawLine(ray.origin, rayHit.point, Color.green);
		}else{
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.red);
		}
	}
}
