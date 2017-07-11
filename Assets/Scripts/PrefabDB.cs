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

}
