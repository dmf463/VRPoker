using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls all the sounds
public class SoundManager : MonoBehaviour
{
	public TextAsset dialogueFile;

	public PokerPlayerRedux casey;// rosa
	public PokerPlayerRedux zombie;// gonzalo
	public PokerPlayerRedux minnie;// minnie
	public PokerPlayerRedux nathaniel;// Nathaniel
	public PokerPlayerRedux floyd;// floyd

	public AudioSource tutorial;

	public AudioSource caseySource; 
	public AudioSource zombieSource;
	public AudioSource minnieSource;
	public AudioSource nathanielSource;
	public AudioSource floydSource;


	public AudioClip[] aside1Index;
	public AudioClip[] aside2Index;
	public AudioClip[] aside3Index;
	public AudioClip[] aside5Index;
	public AudioClip[] aside6Index;

	public AudioClip[] lowAside1Index;
	public AudioClip[] lowAside2Index;
	public AudioClip[] lowAside3Index;
	public AudioClip[] lowAside4Index;

	public AudioClip[] tutorialAudio;

	public List <AudioData> tutorialAudioFiles = new List <AudioData>();



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
	public AudioClip[] chips;
    public AudioClip[] cards;
    public AudioClip[] cardTones;

	void Awake()
	{
		
	}

    void Start()
    {
		for (int i = 0; i < tutorialAudio.Length; i++) {
			tutorialAudioFiles.Add(new AudioData(tutorialAudio[i], false));
		}
    }

    // Update is called once per frame
    void Update()
    {

    }
		

	public void GenerateSourceAndPlay(AudioClip clip, float volume, float pitch = 1)
	{
		GenerateSourceAndPlay(clip, volume, pitch, transform.position);
	}

    //so basically when we want to play a sound we generate a prefab object with an audiosource
    //play the clip
    //then destroy the object after the clip is over
    //this works for sound effects and random things
    //but will not be ideal for the final version
	public void GenerateSourceAndPlay(AudioClip clip, float volume, float pitch, Vector3 position)
    {
        GameObject specialAudioSource = Instantiate(Services.PrefabDB.GenericAudioSource);
        AudioSource source = specialAudioSource.GetComponent<AudioSource>();
		specialAudioSource.transform.position = position;
        source.clip = clip;
        source.volume = volume;
		source.pitch = pitch;
        source.Play();
        Destroy(specialAudioSource, clip.length);
    }

	public void GetSourceAndPlay(AudioSource source, AudioClip clip)
	{
        PokerPlayerRedux player = source.gameObject.GetComponentInParent<PokerPlayerRedux>();
        player.playerIsInConversation = true;
		source.clip = clip;
		source.Play();
        StartCoroutine(PlayerStopsTalking(clip.length, player));
	}


//	public void PlayAsideConversation(int convo)
//	{
//		if(convo == 0){
//            if(!aside1Played) StartCoroutine("Aside1");
//		}
//		if(convo == 1){
//            if(!aside2Played) StartCoroutine("Aside2");
//		}
//		if(convo == 2){
//			if(!aside3Played) StartCoroutine("Aside3");
//		}
//		if(convo == 3){
//			if(!aside5Played) StartCoroutine("Aside5");
//		}
//		if(convo == 4){
//			if(!aside6Played) StartCoroutine("Aside6");
//		}
//	}

	public void PlayAsideConversation(PokerPlayerRedux player)
	{
		if (player == casey){
			int convo = UnityEngine.Random.Range(0, 3);

			if(convo == 0){
                if (!aside3Played) StartCoroutine("Aside3");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
			}
			if(convo == 1){
				if(!aside5Played) StartCoroutine("Aside5");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 2){
				if(!aside6Played) StartCoroutine("Aside6");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 3){
				if(!lowAside3Played) StartCoroutine("lowAside3");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
		}
		if (player == zombie){
			int convo = UnityEngine.Random.Range(0, 1);
			if(convo == 0){
				if(!aside5Played) StartCoroutine("Aside5");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 1){
				if(!aside6Played) StartCoroutine("Aside6");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
		}
		if (player == minnie){
			int convo = UnityEngine.Random.Range(0, 3);
			if(convo == 0){
				if(!aside1Played) StartCoroutine("Aside1");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 1){
				if(!aside5Played) StartCoroutine("Aside5");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 2){
				if(!aside6Played) StartCoroutine("Aside6");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 3){
				if(!lowAside3Played) StartCoroutine("lowAside3");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
		}
		if (player == nathaniel){
			int convo = UnityEngine.Random.Range(0, 4);
			if(convo == 0){
				if(!aside2Played) StartCoroutine("Aside2");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 1){
				if(!aside3Played) StartCoroutine("Aside3");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 2){
				if(!aside5Played) StartCoroutine("Aside5");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 3){
				if(!lowAside1Played) StartCoroutine("LowAside1");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 4){
				if(!lowAside4Played) StartCoroutine("LowAside4");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
		}
		if (player == floyd){
			int convo = UnityEngine.Random.Range(0, 3);
			if(convo == 0){
				if(!aside1Played) StartCoroutine("Aside1");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 1){
				if(!aside2Played) StartCoroutine("Aside2");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 2){
				if(!aside5Played) StartCoroutine("Aside5");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
			if(convo == 3){
				if(!lowAside2Played) StartCoroutine("lowAside2");
                else GetSourceAndPlay(player.playerAudioSource, player.foldAudio);
            }
		}
	}

    IEnumerator PlayerStopsTalking(float time, PokerPlayerRedux player)
    {
        yield return new WaitForSeconds(time);
        player.playerIsInConversation = false;
    }

	IEnumerator Aside1 (){ //floyd initiates toward minnie

        conversationIsPlaying = true;
        
		minnie.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		floydSource.clip = aside1Index[0];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		minnieSource.clip = aside1Index[1];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		floydSource.clip = aside1Index[2];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		minnieSource.clip = aside1Index[3];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		floydSource.clip = aside1Index[4];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
        aside1Played = true;
        conversationIsPlaying = false;

	}

	IEnumerator Aside2 (){ //Nathaniel initiates towards floyd

        conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		nathaniel.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		nathanielSource.clip = aside2Index[0];
			nathanielSource.Play();
				yield return new WaitForSeconds(nathanielSource.clip.length);
		floydSource.clip = aside2Index[1];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		minnieSource.clip = aside2Index[2];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		floydSource.clip = aside2Index[3];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		nathaniel.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
        aside2Played = true;
        conversationIsPlaying = false;
	}

	IEnumerator Aside3 (){ //Casey initiates toward Nathaniel

		conversationIsPlaying = true;
		casey.playerIsInConversation = true;
		nathaniel.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		caseySource.clip = aside3Index[0];
			caseySource.Play();
				yield return new WaitForSeconds(caseySource.clip.length);
		nathanielSource.clip = aside3Index[1];
			nathanielSource.Play();
				yield return new WaitForSeconds(nathanielSource.clip.length);
		caseySource.clip = aside3Index[2];
			caseySource.Play();
				yield return new WaitForSeconds(caseySource.clip.length);
		floydSource.clip = aside3Index[3];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		caseySource.clip = aside3Index[4];
			caseySource.Play();
				yield return new WaitForSeconds(caseySource.clip.length);
		floydSource.clip = aside3Index[5];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);

		yield return new WaitForSeconds (2);
		casey.playerIsInConversation = false;
		nathaniel.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		aside3Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator Aside5 (){ // minnie initiates towards everyone

		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		nathaniel.playerIsInConversation = true;
		floyd.playerIsInConversation = true;


		minnieSource.clip = aside5Index[0];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		floydSource.clip = aside5Index[1];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		nathanielSource.clip = aside5Index[2];
			nathanielSource.Play();
				yield return new WaitForSeconds(nathanielSource.clip.length);
		floydSource.clip = aside5Index[3];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		nathanielSource.clip = aside5Index[4];
			nathanielSource.Play();
				yield return new WaitForSeconds(nathanielSource.clip.length);
		floydSource.clip = aside5Index[5];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		nathanielSource.clip = aside5Index[6];
			nathanielSource.Play();
				yield return new WaitForSeconds(nathanielSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		nathaniel.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		aside5Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator Aside6 (){ // minnie initiates toward Casey or Zombie

		conversationIsPlaying = true;
		casey.playerIsInConversation = true;
		zombie.playerIsInConversation = true;
		minnie.playerIsInConversation = true;
		nathaniel.playerIsInConversation = true;

		minnieSource.clip = aside6Index[0];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		caseySource.clip = aside6Index[1];
			caseySource.Play();
				yield return new WaitForSeconds(caseySource.clip.length);
		zombieSource.clip = aside6Index[2];
			zombieSource.Play();
				yield return new WaitForSeconds(zombieSource.clip.length);
		minnieSource.clip = aside6Index[3];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		caseySource.clip = aside6Index[4];
			caseySource.Play();
				yield return new WaitForSeconds(caseySource.clip.length);
		nathanielSource.clip = aside6Index[5];
			nathanielSource.Play();
				yield return new WaitForSeconds(nathanielSource.clip.length);
		minnieSource.clip = aside6Index[6];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		caseySource.clip = aside6Index[7];
			caseySource.Play();
				yield return new WaitForSeconds(caseySource.clip.length);
		zombieSource.clip = aside6Index[8];
			zombieSource.Play();
				yield return new WaitForSeconds(zombieSource.clip.length);
		caseySource.clip = aside6Index[9];
			caseySource.Play();
				yield return new WaitForSeconds(caseySource.clip.length);


		yield return new WaitForSeconds (2);
		casey.playerIsInConversation = false;
		zombie.playerIsInConversation = false;
		minnie.playerIsInConversation = false;
		nathaniel.playerIsInConversation = false;

		aside6Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside1 (){ //Nathaniel initiates

		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		nathaniel.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		nathanielSource.clip = lowAside1Index[0];
		nathanielSource.Play();
		yield return new WaitForSeconds(nathanielSource.clip.length);
		floydSource.clip = lowAside1Index[1];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);
		minnieSource.clip = lowAside1Index[2];
		minnieSource.Play();
		yield return new WaitForSeconds(minnieSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		nathaniel.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		lowAside1Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside2 (){ //floyd initiates

		conversationIsPlaying = true;
		nathaniel.playerIsInConversation = true;
		floyd.playerIsInConversation = true;


		floydSource.clip = lowAside1Index[0];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);
		nathanielSource.clip = lowAside1Index[1];
		nathanielSource.Play();
		yield return new WaitForSeconds(nathanielSource.clip.length);
		floydSource.clip = lowAside1Index[2];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);

		yield return new WaitForSeconds (2);
		nathaniel.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		lowAside2Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside3 (){ //Casey initiates toward minnie
		
		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		casey.playerIsInConversation = true;


		caseySource.clip = lowAside3Index[0];
		caseySource.Play();
		yield return new WaitForSeconds(caseySource.clip.length);
		minnieSource.clip = lowAside3Index[1];
		minnieSource.Play();
		yield return new WaitForSeconds(minnieSource.clip.length);
		caseySource.clip = lowAside3Index[2];
		caseySource.Play();
		yield return new WaitForSeconds(caseySource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		casey.playerIsInConversation = false;
		lowAside3Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside4 (){ //Nathaniel initiates

		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		nathaniel.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		nathanielSource.clip = lowAside4Index[0];
		nathanielSource.Play();
		yield return new WaitForSeconds(nathanielSource.clip.length);
		floydSource.clip = lowAside4Index[1];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);
		nathanielSource.clip = lowAside4Index[2];
		nathanielSource.Play();
		yield return new WaitForSeconds(nathanielSource.clip.length);
		minnieSource.clip = lowAside4Index[3];
		minnieSource.Play();
		yield return new WaitForSeconds(minnieSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		nathaniel.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		lowAside4Played = true;
		conversationIsPlaying = false;
	}


}

public class AudioData 
{
	public AudioClip audio;
	public bool hasBeenPlayed;

	public AudioData(AudioClip _audio, bool _hasBeenPlayed)
	{
		_audio = audio;
		_hasBeenPlayed = hasBeenPlayed;
	}

}
