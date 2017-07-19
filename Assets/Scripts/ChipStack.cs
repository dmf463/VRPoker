using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ChipStack : MonoBehaviour {

    public List<Chip> chips = new List<Chip>();
    public int stackValue;
    private Vector3 firstChipPos;
    private Quaternion firstChipRot;
    private float incrementStackBy;

	// Use this for initialization
	void Start () {

        chips.Add(gameObject.GetComponent<Chip>());
        stackValue = chips[0].chipValue;
        firstChipPos = chips[0].gameObject.transform.position;
        firstChipRot = chips[0].gameObject.transform.rotation;
        incrementStackBy = chips[0].gameObject.GetComponent<Collider>().bounds.size.y;

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
        newChip.transform.position = new Vector3(firstChipPos.x, (firstChipPos.y - (incrementStackBy * chips.Count)), firstChipPos.z);
        chips.Add(newChip.GetComponent<Chip>());
        stackValue += newChip.GetComponent<Chip>().chipValue;

    }

    public void TakeFromStack()
    {

    }

    public void ClearStack()
    {

    }
}
