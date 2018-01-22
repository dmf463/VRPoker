using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TESTSCRIPT : MonoBehaviour {

    Dictionary<string, object> dict = new Dictionary<string, object>();

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TESTFUNCTION()
    {
        dict.Add("concept", "onBet");
        dict.Add("who", Services.Dealer.players[0]);
        dict.Add("chipCount", Table.instance.playerChipStacks[0]);
        dict.Add("gameState", Table.gameState);
        dict.Add("num_of_players", Services.Dealer.PlayerAtTableCount());

        //ICollection keys = dict.Keys;
        //foreach (string obj in keys)
        //{
        //    Debug.Log(obj + " : " + dict[obj]);
        //}

        //IEnumerable<object> query =
        //    from obj in ht
        //    where obj != null
        //    select obj;
    }
}
