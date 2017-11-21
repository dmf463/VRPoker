using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {

	public float speed = 100f;
	public float radius = 5;
	public Vector3 centerPoint;
	float currentAngle = 0f;
	public float noiseMagnitude = 1;
	public float noiseSpeed = 1;


	// Use this for initialization
	void Start () {
		centerPoint = transform.parent.position;
	}
	
	// Update is called once per frame
	void Update () {
		
		currentAngle += speed * Time.deltaTime; //represents movement through angle from origin
		float angleInRadians = currentAngle * Mathf.Deg2Rad;
		Vector3 noiseOffset = (noiseMagnitude * new Vector3(Mathf.PerlinNoise(100, Time.time * noiseSpeed), 
			Mathf.PerlinNoise(200, Time.time * noiseSpeed), 
			Mathf.PerlinNoise(300, Time.time * noiseSpeed)))
			- (noiseMagnitude/2 * Vector3.one);
		transform.position = centerPoint + new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians)) + noiseOffset;


		//transform.RotateAround(gameObject.transform.parent.position, new Vector3(Random.Range(1,2), 1, Mathf.Cos(1)), speed * Time.deltaTime);
	}
}
