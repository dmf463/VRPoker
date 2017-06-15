using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPosition : MonoBehaviour {

    public float cardCount;
    public bool doneDealing;

	// Use this for initialization
	void Start () {

        cardCount = 0;
		
	}

    public void OnTriggerEnter (Collider other)
    {
        cardCount += 1;
    }

    
	
	// Update is called once per frame
	void Update () {

        if(cardCount == 2)
        {
            doneDealing = true;
        }
		
	}
}
