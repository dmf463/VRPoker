using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipManager {

    public List<GameObject> chipsToDestroy = new List<GameObject>();

    public void DestroyChips()
    {
        if (chipsToDestroy.Count > 0)
        {
            foreach (GameObject chip in chipsToDestroy)
            {
                GameObject.Destroy(chip);
            }
        }
    }

}
