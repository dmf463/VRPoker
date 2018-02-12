using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpriteGlow;

public class GlowLerp : MonoBehaviour {

    float glowMax = 1.1f;
    float glowMin = 1f;
    public float glowSpeed = .5f;
    SpriteGlowEffect glowScript;
    public bool turnOff = false;

	// Use this for initialization
	void Start () {

        glowScript = GetComponent<SpriteGlowEffect>();
		
	}
	
	// Update is called once per frame
	void Update () {

        glowScript.GlowBrightness = PingPong(glowSpeed * Time.time, glowMin, glowMax);

    }

    float PingPong(float time, float minLength, float maxLength)
    {
        return Mathf.PingPong(time, maxLength - minLength) + minLength;
    }
}
