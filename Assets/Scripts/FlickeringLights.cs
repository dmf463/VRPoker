using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLights : MonoBehaviour
{
    public List<GameObject> lights;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject obj in lights)
        {
            if (obj.activeSelf == true)
            {
                if (Random.Range(0, 100) < 5) obj.SetActive(false);
            }
            else if (obj.activeSelf == false)
            {
                if (Random.Range(0, 100) < 5) obj.SetActive(true);
            }

        }
    }
}
