using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningSceneScript : MonoBehaviour {

    TaskManager tm;
    TaskManager parallelTM;
    TaskManager winnerTM;
    public GameObject handHolder;
    public GameObject rosa;
    public GameObject winner;
    bool startedTasks;
    public float upOffset;
    public float handDuration;

	// Use this for initialization
	void Start () {
        tm = new TaskManager();
        parallelTM = new TaskManager();
        winnerTM = new TaskManager();
	}
	
	// Update is called once per frame
	void Update () {
        tm.Update();
        parallelTM.Update();
        winnerTM.Update();

        if (!startedTasks)
        {
            StartCutScene();
        }
	}

    public void StartCutScene()
    {
        Vector3 verticalOffset = new Vector3(0, upOffset, 0);
        LerpPos lerpHandsUp = new LerpPos(handHolder, handHolder.transform.position, handHolder.transform.position + verticalOffset, handDuration / 4);
        LerpPos lerpHandsDown = new LerpPos(handHolder, handHolder.transform.position + verticalOffset, handHolder.transform.position - verticalOffset, handDuration);
        Wait waitForRosa = new Wait(handDuration / 4);
        LerpPos lerpRosa = new LerpPos(rosa, rosa.transform.position, rosa.transform.position - verticalOffset, handDuration);
        LerpPos lerpWinner = new LerpPos(winner, winner.transform.position, winner.transform.position + verticalOffset * 2, handDuration);

        waitForRosa.Then(lerpRosa);
        lerpHandsUp.Then(lerpHandsDown);

        tm.Do(lerpHandsUp);
        parallelTM.Do(waitForRosa);
        winnerTM.Do(lerpWinner);

        startedTasks = true;
    }
}
