using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject {

    [SerializeField]
    private GameObject cardDeck;
    public GameObject CardDeck { get { return cardDeck; } }

    [SerializeField]
    private GameObject card;
    public GameObject Card { get { return card; } }

    [SerializeField]
    private GameObject redChip5;
    public GameObject RedChip5 { get { return redChip5; } }

    [SerializeField]
    private GameObject blueChip25;
    public GameObject BlueChip25 { get { return blueChip25; } }

    [SerializeField]
    private GameObject whiteChip50;
    public GameObject WhiteChip50 { get { return whiteChip50; } }

    [SerializeField]
    private GameObject blackChip100;
    public GameObject BlackChip100 { get { return blackChip100; } }


}
