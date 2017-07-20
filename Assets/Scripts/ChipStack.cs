using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ChipStack : InteractionSuperClass {

    public List<Chip> chips = new List<Chip>();
    public int stackValue;
    private float incrementStackBy;

	// Use this for initialization
	void Start () {

        chips.Add(gameObject.GetComponent<Chip>());
        stackValue = chips[0].chipValue;
        incrementStackBy = chips[0].gameObject.GetComponent<Collider>().bounds.size.y / 16;

	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void AddToStack(Chip chip)
    {
        int chipValue = chip.chipValue;
        Debug.Log(chip.gameObject.name + " wants to be destroyed");
        Destroy(chip.gameObject);
        GameObject newChip = null;
        if (chipValue == 5)
        {
            newChip = Instantiate(Services.PrefabDB.RedChip5, transform.position, Quaternion.identity);
        }
        else if (chipValue == 25)
        {
            newChip = Instantiate(Services.PrefabDB.BlueChip25, transform.position, Quaternion.identity);
        }
        else if (chipValue == 50)
        {
            newChip = Instantiate(Services.PrefabDB.WhiteChip50, transform.position, Quaternion.identity);
        }
        else if (chipValue == 100)
        {
            newChip = Instantiate(Services.PrefabDB.BlackChip100, transform.position, Quaternion.identity);
        }
        newChip.GetComponent<BoxCollider>().enabled = false;
        newChip.GetComponent<Rigidbody>().isKinematic = true;
        newChip.transform.parent = gameObject.transform;
        newChip.transform.localPosition = new Vector3(chips[0].transform.localPosition.x, chips[0].transform.localPosition.y, (chips[chips.Count - 1].transform.localPosition.z + incrementStackBy));
        newChip.transform.rotation = chips[0].transform.rotation;
        stackValue += newChip.GetComponent<Chip>().chipValue;
        chips.Add(newChip.GetComponent<Chip>());
        Debug.Log("chipStack is worth " + stackValue);

    }

    public void TakeFromStack()
    {
        if(chips.Count > 1)
        {
            Chip fakeChip = new Chip();
            fakeChip.ChipColor = chips[0].ChipColor;
            fakeChip.chipValue = chips[0].chipValue;
            Mesh fakeMesh = chips[0].gameObject.GetComponent<MeshFilter>().mesh;

            chips[0].gameObject.GetComponent<MeshFilter>().mesh = chips[1].gameObject.GetComponent<MeshFilter>().mesh;
            chips[0].ChipColor = chips[1].ChipColor;
            chips[0].chipValue = chips[1].chipValue;

            chips[1].gameObject.GetComponent<MeshFilter>().mesh = fakeMesh;
            chips[1].ChipColor = fakeChip.ChipColor;
            chips[1].chipValue = fakeChip.chipValue;

            Physics.IgnoreCollision(chips[1].gameObject.GetComponent<Collider>(), this.GetComponent<Collider>(), true);
            chips[1].gameObject.transform.parent = null;
            chips[1].GetComponent<BoxCollider>().enabled = true;
            chips[1].GetComponent<Rigidbody>().isKinematic = false;
            stackValue -= chips[1].chipValue;
            chips.Remove(chips[1]);
            chips.TrimExcess();
            for (int i = 1; i < chips.Count; i++)
            {
                chips[i].transform.localPosition = new Vector3(chips[i].transform.localPosition.x, chips[i].transform.localPosition.y, chips[i].transform.localPosition.z - incrementStackBy);
            }
            Debug.Log("stackValue is" + stackValue);
        }
        else
        {
            GameObject[] chipsOnTable = GameObject.FindGameObjectsWithTag("Chip");
            foreach (GameObject chip in chipsOnTable)
            {
                Physics.IgnoreCollision(chip.GetComponent<Collider>(), chip.gameObject.GetComponent<Collider>(), false);
            }
        }
    }

    public void ClearStack()
    {

    }

    public override void CheckSwipeDirection()
    {
        
        base.CheckSwipeDirection();
    }

    public override void HandAttachedUpdate(Hand attachedHand)
    {
        CheckPressPosition(attachedHand);
        base.HandAttachedUpdate(attachedHand);
    }

    public override void OnPressBottom()
    {
        TakeFromStack();
        base.OnPressBottom();
    }



}
