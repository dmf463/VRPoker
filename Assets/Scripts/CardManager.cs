using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour {

    public float cardCount;
    public bool doneDealing;
    GameObject gameManager;
    GameManager gm;
    string playerName;


	// Use this for initialization
	void Start () {

        cardCount = 0;
        gameManager = GameObject.Find("GameManager");
        gm = gameManager.GetComponent<GameManager>();
        playerName = gameObject.name;

	}

    public void OnTriggerEnter (Collider other)
    {
        cardCount += 1;
        if(other.gameObject.tag != "Hand")
        {
            switch (playerName)
            {
                case "TestSpace1":
                    gm.p1HoleCards.Add(other.name);
                    break;
                case "TestSpace2":
                    gm.p2HoleCards.Add(other.name);
                    break;
                case "TestSpace3":
                    gm.p3HoleCards.Add(other.name);
                    break;
                case "TestSpace4":
                    gm.p4HoleCards.Add(other.name);
                    break;
                case "TheBoard":
                    gm.boardCards.Add(other.name);
                    break;
                case "BurnCards":
                    gm.burnCards.Add(other.name);
                    break;
                default:
                    break;
            }
        }

    }
    
	
	// Update is called once per frame
	void Update () {

        if(cardCount == 2)
        {
            doneDealing = true;
        }
    }
}
