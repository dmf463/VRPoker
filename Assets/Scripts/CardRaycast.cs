using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRaycast : MonoBehaviour {

    public bool cardIsFaceUp = false;
    private Transform table;

    private void Start()
    {
        table = GameObject.Find("Table").transform;
    }
    // Update is called once per frame
    void Update () 
	{
        cardIsFaceUp = CardIsFaceUp(90);
	}

    public bool CardIsFaceUp(float angleThreshold)
    {
        Transform table = GameObject.Find("Table").transform;
        Vector3 targetDir = table.position - transform.position;
        float angle = Vector3.Angle(targetDir, -transform.forward);
        if (angle > angleThreshold) return true;
        else return false;
    }
}
