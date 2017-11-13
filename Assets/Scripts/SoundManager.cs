using System.Collections;
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
//    [Header("Floyd Lines")]
//    public AudioClip checkP1;
//    public AudioClip callP1;
//    public AudioClip raiseP1;
//    public AudioClip foldP1;
//    public AudioClip allInP1;
//    public List<AudioClip> genericBadP1 = new List<AudioClip>();
//    public List<AudioClip> badBeatP1 = new List<AudioClip>();
//    public List<AudioClip> incredulousP1 = new List<AudioClip>();
//    public List<AudioClip> respectP1 = new List<AudioClip>();
//    public List<AudioClip> goodResponseP1 = new List<AudioClip>();
//
//    [Header("Willy Lines")]
//    public AudioClip checkP2;
//    public AudioClip callP2;
//    public AudioClip raiseP2;
//    public AudioClip foldP2;
//    public AudioClip allInP2;
//    public List<AudioClip> genericBadP2 = new List<AudioClip>();
//    public List<AudioClip> badBeatP2 = new List<AudioClip>();
//    public List<AudioClip> incredulousP2 = new List<AudioClip>();
//    public List<AudioClip> respectP2 = new List<AudioClip>();
//    public List<AudioClip> goodResponseP2 = new List<AudioClip>();


	public AudioClip[] convo1Index;
	public AudioClip[] convo2Index;

    public bool conversationIsPlaying;

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
        source.Play();
        Destroy(specialAudioSource, clip.length);
    }

	public void GetSourceAndPlay(AudioSource source, AudioClip clip)
	{
		source.clip = clip;
		source.Play();
	}


	public void PlayConversation(int convo)
	{
		if(convo == 0){
			StartCoroutine("Conversation1");
		}
		if(convo == 1){
			StartCoroutine("Conversation2");
		}
	}

	IEnumerator Conversation1 (){

        conversationIsPlaying = true;
		player2.playerIsInConversation = true;
		player4.playerIsInConversation = true;

		p4Source.clip = convo1Index[0];
		p4Source.Play();
		yield return new WaitForSeconds(p4Source.clip.length);
		p2Source.clip = convo1Index[1];
		p2Source.Play();
		yield return new WaitForSeconds(p2Source.clip.length);
		p4Source.clip = convo1Index[2];
		p4Source.Play();
		yield return new WaitForSeconds(p4Source.clip.length);
		p2Source.clip = convo1Index[3];
		p2Source.Play();
		yield return new WaitForSeconds(p2Source.clip.length);
		p4Source.clip = convo1Index[4];
		p4Source.Play();
		yield return new WaitForSeconds(p4Source.clip.length);

		player2.playerIsInConversation = false;
		player4.playerIsInConversation = false;
        conversationIsPlaying = false;

	}

	IEnumerator Conversation2 (){

        conversationIsPlaying = true;
		player2.playerIsInConversation = true;
		player3.playerIsInConversation = true;
		player4.playerIsInConversation = true;

		p3Source.clip = convo2Index[0];
		p3Source.Play();
		yield return new WaitForSeconds(p3Source.clip.length);
		p4Source.clip = convo2Index[1];
		p4Source.Play();
		yield return new WaitForSeconds(p4Source.clip.length);
		p2Source.clip = convo2Index[2];
		p2Source.Play();
		yield return new WaitForSeconds(p2Source.clip.length);
		p4Source.clip = convo2Index[3];
		p4Source.Play();
		yield return new WaitForSeconds(p4Source.clip.length);

		player2.playerIsInConversation = false;
		player3.playerIsInConversation = false;
		player4.playerIsInConversation = false;
        conversationIsPlaying = false;
	}

}