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
        if (other.gameObject.tag == "PlayingCard")
        {
            switch (playerName)
            {
                case "TestSpace1":
                    gm.p1HoleCards.Add(other.gameObject);
                    break;
                case "TestSpace2":
                    gm.p2HoleCards.Add(other.gameObject);
                    break;
                case "TestSpace3":
                    gm.p3HoleCards.Add(other.gameObject);
                    break;
                case "TestSpace4":
                    gm.p4HoleCards.Add(other.gameObject);
                    break;
                case "TheBoard":
                    if (gm.boardCards.Contains(other.gameObject))
                    {
                        break;
                    }
                    gm.boardCards.Add(other.gameObject);
                    break;
                case "BurnCards":
                    gm.burnCards.Add(other.gameObject);
                    break;
                default:
                    break;
            }
        }

    }
    
	
	// Update is called once per frame
	void Update () {

    }
}
