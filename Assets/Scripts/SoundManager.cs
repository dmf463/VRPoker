using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    [Header("Floyd Lines")]
    public AudioClip checkP1;
    public AudioClip callP1;
    public AudioClip raiseP1;
    public AudioClip foldP1;
    public AudioClip allInP1;
    public List<AudioClip> genericBadP1 = new List<AudioClip>();
    public List<AudioClip> badBeatP1 = new List<AudioClip>();
    public List<AudioClip> incredulousP1 = new List<AudioClip>();
    public List<AudioClip> respectP1 = new List<AudioClip>();
    public List<AudioClip> goodResponseP1 = new List<AudioClip>();

    [Header("Willy Lines")]
    public AudioClip checkP2;
    public AudioClip callP2;
    public AudioClip raiseP2;
    public AudioClip foldP2;
    public AudioClip allInP2;
    public List<AudioClip> genericBadP2 = new List<AudioClip>();
    public List<AudioClip> badBeatP2 = new List<AudioClip>();
    public List<AudioClip> incredulousP2 = new List<AudioClip>();
    public List<AudioClip> respectP2 = new List<AudioClip>();
    public List<AudioClip> goodResponseP2 = new List<AudioClip>();

    [Header("SoundEffects")]
    public AudioClip chips;
    public AudioClip cards;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void GenerateSourceAndPlay(AudioClip clip)
    {
        GameObject specialAudioSource = Instantiate(Services.PrefabDB.GenericAudioSource);
        AudioSource source = specialAudioSource.GetComponent<AudioSource>();
        source.clip = clip;
        source.Play();
        Destroy(specialAudioSource, clip.length);
    }


}