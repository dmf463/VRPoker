using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HandRaycast : MonoBehaviour {

    public float rayDistance;
    public LayerMask mask;
    Vector3 heightThreshold;
    TaskManager tm;

    // Use this for initialization
    void Start () {

        tm = new TaskManager();
        heightThreshold = GameObject.Find("PointerHeightThreshold").transform.position;
		
	}
	
	// Update is called once per frame
	void Update () {
        tm.Update();

        //1. declare your raycast (origin of the array, and then the direction it shoots)
        Ray ray = new Ray(transform.position, transform.forward);

        //2. setup our raycastHit info variable
        RaycastHit rayHit = new RaycastHit();
        //3 we're ready to shoot the raycast
        if (Physics.Raycast(ray, out rayHit, rayDistance, mask))
        {
            if(Services.Dealer.playerToAct != null)
            {
                //Debug.Log("RayHit transform = " + rayHit.transform.gameObject.name);
                if (transform.position.y >= heightThreshold.y)
                {
                    if (rayHit.transform == Services.Dealer.playerToAct.gameObject.GetComponentInChildren<PlayerGazeTrigger>().gameObject.transform) //are we looking at this thing
                    {
                        rayHit.transform.gameObject.GetComponent<PlayerGazeTrigger>().HittingTarget();
                    }
                }
            }
            if(rayHit.transform.gameObject.tag == "PlayerChips")
            {
                Debug.Log("hitting playerChips");
                FadeInChipCount(rayHit.transform.gameObject.GetComponent<TextMeshPro>());
            }
            else
            {
                foreach(TextMeshPro t in Services.TextManager.playerChipCounts)
                {
                    if(t.color.a == 1)
                    {
                        FadeOutChipCount(t);
                    }
                }
            }
        }
    }
    public void FadeInChipCount(TextMeshPro t)
    {
        Debug.Log("FADING IN CHIPS");
        for (int i = 0; i < Services.TextManager.playerChipCounts.Count; i++)
        {
            if(Services.TextManager.playerChipCounts[i] == t)
            {
                TextMeshPro betText = Services.TextManager.playerChipCounts[i];
                betText.text = Services.Dealer.players[i].chipCount.ToString();
                Color fullAlpha = new Color(betText.color.r, betText.color.g, betText.color.b, 1); //declare variables before
                Color noAlpha = new Color(betText.color.r, betText.color.g, betText.color.b, 0);
                LerpTextMeshProColor lerpUpTask = new LerpTextMeshProColor(betText, noAlpha, fullAlpha, Easing.FunctionType.QuadEaseOut, 0.5f); //declare tasks
                //LerpTextMeshProColor lerpDownTask = new LerpTextMeshProColor(betText, fullAlpha, noAlpha, Easing.FunctionType.QuadEaseIn, 0.5f);
                //lerpUpTask. //sequence the tasks, so that DO does them in the right order
                //    Then(lerpDownTask);
                tm.Do(lerpUpTask); //DO THEM!
            }
        }
    }

    public void FadeOutChipCount(TextMeshPro t)
    {
        Debug.Log("FADING OUT CHIPS");
        for (int i = 0; i < Services.TextManager.playerChipCounts.Count; i++)
        {
            if (Services.TextManager.playerChipCounts[i] == t)
            {
                TextMeshPro betText = Services.TextManager.playerChipCounts[i];
                betText.text = Services.Dealer.players[i].chipCount.ToString();
                Color fullAlpha = new Color(betText.color.r, betText.color.g, betText.color.b, 1); //declare variables before
                Color noAlpha = new Color(betText.color.r, betText.color.g, betText.color.b, 0);
                //LerpTextMeshProColor lerpUpTask = new LerpTextMeshProColor(betText, noAlpha, fullAlpha, Easing.FunctionType.QuadEaseOut, 0.5f); //declare tasks
                LerpTextMeshProColor lerpDownTask = new LerpTextMeshProColor(betText, fullAlpha, noAlpha, Easing.FunctionType.QuadEaseIn, 0.5f);
                //lerpUpTask. //sequence the tasks, so that DO does them in the right order
                //    Then(lerpDownTask);
                tm.Do(lerpDownTask); //DO THEM!
            }
        }
    }
}
