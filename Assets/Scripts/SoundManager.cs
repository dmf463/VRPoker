using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioClip checkP1;
    public AudioClip callP1;
    public AudioClip raiseP1;
    public AudioClip foldP1;
    public AudioClip allInP1;

    public AudioClip checkP2;
    public AudioClip callP2;
    public AudioClip raiseP2;
    public AudioClip foldP2;
    public AudioClip allInP2;

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