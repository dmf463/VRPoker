using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public AudioClip check;
    public AudioClip call;
    public AudioClip raise;
    public AudioClip fold;
    public AudioClip allIn;

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