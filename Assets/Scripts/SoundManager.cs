﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls all the sounds
public class SoundManager : MonoBehaviour
{
	public PokerPlayerRedux player0;
	public PokerPlayerRedux player1;
	public PokerPlayerRedux player2;
	public PokerPlayerRedux player3;
	public PokerPlayerRedux player4;


	public AudioSource p0Source;
	public AudioSource p1Source;
	public AudioSource p2Source;
	public AudioSource p3Source;
	public AudioSource p4Source;


	public AudioClip[] aside1Index;
	public AudioClip[] aside2Index;
	public AudioClip[] aside3Index;
	public AudioClip[] aside5Index;
	public AudioClip[] aside6Index;

	public AudioClip[] lowAside1Index;
	public AudioClip[] lowAside2Index;
	public AudioClip[] lowAside3Index;
	public AudioClip[] lowAside4Index;



    public bool conversationIsPlaying;
    public bool aside1Played;
    public bool aside2Played;
	public bool aside3Played;
	public bool aside5Played;
	public bool aside6Played;

	public bool lowAside1Played;
	public bool lowAside2Played;
	public bool lowAside3Played;
	public bool lowAside4Played;

    [Header("SoundEffects")]
    public AudioClip chips;
    public AudioClip cards;
    public AudioClip[] cardTones;

    void Start()
    {
		
    }

    // Update is called once per frame
    void Update()
    {

    }
		
    //so basically when we want to play a sound we generate a prefab object with an audiosource
    //play the clip
    //then destroy the object after the clip is over
    //this works for sound effects and random things
    //but will not be ideal for the final version
	public void GenerateSourceAndPlay(AudioClip clip)
    {
        GameObject specialAudioSource = Instantiate(Services.PrefabDB.GenericAudioSource);
        AudioSource source = specialAudioSource.GetComponent<AudioSource>();
        source.clip = clip;
        if (conversationIsPlaying) source.volume = 0.25f;
        else source.volume = 0.5f;
        source.Play();
        Destroy(specialAudioSource, clip.length);
    }

	public void GetSourceAndPlay(AudioSource source, AudioClip clip)
	{
		source.clip = clip;
		source.Play();
	}


	public void PlayAsideConversation(int convo)
	{
		if(convo == 0){
            if(!aside1Played) StartCoroutine("Aside1");
		}
		if(convo == 1){
            if(!aside2Played) StartCoroutine("Aside2");
		}
		if(convo == 2){
			if(!aside3Played) StartCoroutine("Aside3");
		}
		if(convo == 3){
			if(!aside5Played) StartCoroutine("Aside5");
		}
		if(convo == 4){
			if(!aside6Played) StartCoroutine("Aside6");
		}
	}

	IEnumerator Aside1 (){

        conversationIsPlaying = true;
        
		player2.playerIsInConversation = true;
		player4.playerIsInConversation = true;

		p4Source.clip = aside1Index[0];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);
		p2Source.clip = aside1Index[1];
			p2Source.Play();
				yield return new WaitForSeconds(p2Source.clip.length);
		p4Source.clip = aside1Index[2];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);
		p2Source.clip = aside1Index[3];
			p2Source.Play();
				yield return new WaitForSeconds(p2Source.clip.length);
		p4Source.clip = aside1Index[4];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);

		player2.playerIsInConversation = false;
		player4.playerIsInConversation = false;
        aside1Played = true;
        conversationIsPlaying = false;

	}

	IEnumerator Aside2 (){

        conversationIsPlaying = true;
		player2.playerIsInConversation = true;
		player3.playerIsInConversation = true;
		player4.playerIsInConversation = true;

		p3Source.clip = aside2Index[0];
			p3Source.Play();
				yield return new WaitForSeconds(p3Source.clip.length);
		p4Source.clip = aside2Index[1];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);
		p2Source.clip = aside2Index[2];
			p2Source.Play();
				yield return new WaitForSeconds(p2Source.clip.length);
		p4Source.clip = aside2Index[3];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);

		player2.playerIsInConversation = false;
		player3.playerIsInConversation = false;
		player4.playerIsInConversation = false;
        aside2Played = true;
        conversationIsPlaying = false;
	}

	IEnumerator Aside3 (){

		conversationIsPlaying = true;
		player0.playerIsInConversation = true;
		player3.playerIsInConversation = true;
		player4.playerIsInConversation = true;

		p0Source.clip = aside3Index[0];
			p0Source.Play();
				yield return new WaitForSeconds(p0Source.clip.length);
		p3Source.clip = aside3Index[1];
			p3Source.Play();
				yield return new WaitForSeconds(p3Source.clip.length);
		p0Source.clip = aside3Index[2];
			p0Source.Play();
				yield return new WaitForSeconds(p0Source.clip.length);
		p4Source.clip = aside3Index[3];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);
		p0Source.clip = aside3Index[4];
			p0Source.Play();
				yield return new WaitForSeconds(p0Source.clip.length);
		p4Source.clip = aside3Index[5];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);

		player0.playerIsInConversation = false;
		player3.playerIsInConversation = false;
		player4.playerIsInConversation = false;
		aside3Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator Aside5 (){

		conversationIsPlaying = true;
		player2.playerIsInConversation = true;
		player3.playerIsInConversation = true;
		player4.playerIsInConversation = true;


		p2Source.clip = aside5Index[0];
			p2Source.Play();
				yield return new WaitForSeconds(p2Source.clip.length);
		p4Source.clip = aside5Index[1];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);
		p3Source.clip = aside5Index[2];
			p3Source.Play();
				yield return new WaitForSeconds(p3Source.clip.length);
		p4Source.clip = aside5Index[3];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);
		p3Source.clip = aside5Index[4];
			p3Source.Play();
				yield return new WaitForSeconds(p3Source.clip.length);
		p4Source.clip = aside5Index[5];
			p4Source.Play();
				yield return new WaitForSeconds(p4Source.clip.length);
		p3Source.clip = aside5Index[6];
			p3Source.Play();
				yield return new WaitForSeconds(p3Source.clip.length);

		player2.playerIsInConversation = false;
		player3.playerIsInConversation = false;
		player4.playerIsInConversation = false;
		aside5Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator Aside6 (){

		conversationIsPlaying = true;
		player0.playerIsInConversation = true;
		player1.playerIsInConversation = true;
		player2.playerIsInConversation = true;
		player3.playerIsInConversation = true;

		p2Source.clip = aside6Index[0];
			p2Source.Play();
				yield return new WaitForSeconds(p2Source.clip.length);
		p0Source.clip = aside6Index[1];
			p0Source.Play();
				yield return new WaitForSeconds(p0Source.clip.length);
		p1Source.clip = aside6Index[2];
			p1Source.Play();
				yield return new WaitForSeconds(p1Source.clip.length);
		p2Source.clip = aside6Index[3];
			p2Source.Play();
				yield return new WaitForSeconds(p2Source.clip.length);
		p0Source.clip = aside6Index[4];
			p0Source.Play();
				yield return new WaitForSeconds(p0Source.clip.length);
		p3Source.clip = aside6Index[5];
			p3Source.Play();
				yield return new WaitForSeconds(p3Source.clip.length);
		p2Source.clip = aside6Index[6];
			p2Source.Play();
				yield return new WaitForSeconds(p2Source.clip.length);
		p0Source.clip = aside6Index[7];
			p0Source.Play();
				yield return new WaitForSeconds(p0Source.clip.length);
		p1Source.clip = aside6Index[8];
			p1Source.Play();
				yield return new WaitForSeconds(p1Source.clip.length);
		p0Source.clip = aside6Index[9];
			p0Source.Play();
				yield return new WaitForSeconds(p0Source.clip.length);



		player0.playerIsInConversation = true;
		player1.playerIsInConversation = true;
		player2.playerIsInConversation = false;
		player3.playerIsInConversation = false;

		aside6Played = true;
		conversationIsPlaying = false;
	}
}
