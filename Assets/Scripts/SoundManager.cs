using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls all the sounds
public class SoundManager : MonoBehaviour
{
	public PokerPlayerRedux rosa;// rosa
	public PokerPlayerRedux gonzalo;// gonzalo
	public PokerPlayerRedux minnie;// minnie
	public PokerPlayerRedux lester;// lester
	public PokerPlayerRedux floyd;// floyd


	public AudioSource rosaSource; 
	public AudioSource gonzaloSource;
	public AudioSource minnieSource;
	public AudioSource lesterSource;
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

    void Start()
    {
		
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
		if (player == rosa){
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
		if (player == gonzalo){
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
		if (player == lester){
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

	IEnumerator Aside2 (){ //lester initiates towards floyd

        conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		lester.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		lesterSource.clip = aside2Index[0];
			lesterSource.Play();
				yield return new WaitForSeconds(lesterSource.clip.length);
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
		lester.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
        aside2Played = true;
        conversationIsPlaying = false;
	}

	IEnumerator Aside3 (){ //rosa initiates toward lester

		conversationIsPlaying = true;
		rosa.playerIsInConversation = true;
		lester.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		rosaSource.clip = aside3Index[0];
			rosaSource.Play();
				yield return new WaitForSeconds(rosaSource.clip.length);
		lesterSource.clip = aside3Index[1];
			lesterSource.Play();
				yield return new WaitForSeconds(lesterSource.clip.length);
		rosaSource.clip = aside3Index[2];
			rosaSource.Play();
				yield return new WaitForSeconds(rosaSource.clip.length);
		floydSource.clip = aside3Index[3];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		rosaSource.clip = aside3Index[4];
			rosaSource.Play();
				yield return new WaitForSeconds(rosaSource.clip.length);
		floydSource.clip = aside3Index[5];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);

		yield return new WaitForSeconds (2);
		rosa.playerIsInConversation = false;
		lester.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		aside3Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator Aside5 (){ // minnie initiates towards everyone

		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		lester.playerIsInConversation = true;
		floyd.playerIsInConversation = true;


		minnieSource.clip = aside5Index[0];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		floydSource.clip = aside5Index[1];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		lesterSource.clip = aside5Index[2];
			lesterSource.Play();
				yield return new WaitForSeconds(lesterSource.clip.length);
		floydSource.clip = aside5Index[3];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		lesterSource.clip = aside5Index[4];
			lesterSource.Play();
				yield return new WaitForSeconds(lesterSource.clip.length);
		floydSource.clip = aside5Index[5];
			floydSource.Play();
				yield return new WaitForSeconds(floydSource.clip.length);
		lesterSource.clip = aside5Index[6];
			lesterSource.Play();
				yield return new WaitForSeconds(lesterSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		lester.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		aside5Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator Aside6 (){ // minnie initiates toward rosa or gonzalo

		conversationIsPlaying = true;
		rosa.playerIsInConversation = true;
		gonzalo.playerIsInConversation = true;
		minnie.playerIsInConversation = true;
		lester.playerIsInConversation = true;

		minnieSource.clip = aside6Index[0];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		rosaSource.clip = aside6Index[1];
			rosaSource.Play();
				yield return new WaitForSeconds(rosaSource.clip.length);
		gonzaloSource.clip = aside6Index[2];
			gonzaloSource.Play();
				yield return new WaitForSeconds(gonzaloSource.clip.length);
		minnieSource.clip = aside6Index[3];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		rosaSource.clip = aside6Index[4];
			rosaSource.Play();
				yield return new WaitForSeconds(rosaSource.clip.length);
		lesterSource.clip = aside6Index[5];
			lesterSource.Play();
				yield return new WaitForSeconds(lesterSource.clip.length);
		minnieSource.clip = aside6Index[6];
			minnieSource.Play();
				yield return new WaitForSeconds(minnieSource.clip.length);
		rosaSource.clip = aside6Index[7];
			rosaSource.Play();
				yield return new WaitForSeconds(rosaSource.clip.length);
		gonzaloSource.clip = aside6Index[8];
			gonzaloSource.Play();
				yield return new WaitForSeconds(gonzaloSource.clip.length);
		rosaSource.clip = aside6Index[9];
			rosaSource.Play();
				yield return new WaitForSeconds(rosaSource.clip.length);


		yield return new WaitForSeconds (2);
		rosa.playerIsInConversation = false;
		gonzalo.playerIsInConversation = false;
		minnie.playerIsInConversation = false;
		lester.playerIsInConversation = false;

		aside6Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside1 (){ //lester initiates

		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		lester.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		lesterSource.clip = lowAside1Index[0];
		lesterSource.Play();
		yield return new WaitForSeconds(lesterSource.clip.length);
		floydSource.clip = lowAside1Index[1];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);
		minnieSource.clip = lowAside1Index[2];
		minnieSource.Play();
		yield return new WaitForSeconds(minnieSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		lester.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		lowAside1Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside2 (){ //floyd initiates

		conversationIsPlaying = true;
		lester.playerIsInConversation = true;
		floyd.playerIsInConversation = true;


		floydSource.clip = lowAside1Index[0];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);
		lesterSource.clip = lowAside1Index[1];
		lesterSource.Play();
		yield return new WaitForSeconds(lesterSource.clip.length);
		floydSource.clip = lowAside1Index[2];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);

		yield return new WaitForSeconds (2);
		lester.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		lowAside2Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside3 (){ //rosa initiates toward minnie
		
		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		rosa.playerIsInConversation = true;


		rosaSource.clip = lowAside3Index[0];
		rosaSource.Play();
		yield return new WaitForSeconds(rosaSource.clip.length);
		minnieSource.clip = lowAside3Index[1];
		minnieSource.Play();
		yield return new WaitForSeconds(minnieSource.clip.length);
		rosaSource.clip = lowAside3Index[2];
		rosaSource.Play();
		yield return new WaitForSeconds(rosaSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		rosa.playerIsInConversation = false;
		lowAside3Played = true;
		conversationIsPlaying = false;
	}

	IEnumerator LowAside4 (){ //lester initiates

		conversationIsPlaying = true;
		minnie.playerIsInConversation = true;
		lester.playerIsInConversation = true;
		floyd.playerIsInConversation = true;

		lesterSource.clip = lowAside4Index[0];
		lesterSource.Play();
		yield return new WaitForSeconds(lesterSource.clip.length);
		floydSource.clip = lowAside4Index[1];
		floydSource.Play();
		yield return new WaitForSeconds(floydSource.clip.length);
		lesterSource.clip = lowAside4Index[2];
		lesterSource.Play();
		yield return new WaitForSeconds(lesterSource.clip.length);
		minnieSource.clip = lowAside4Index[3];
		minnieSource.Play();
		yield return new WaitForSeconds(minnieSource.clip.length);

		yield return new WaitForSeconds (2);
		minnie.playerIsInConversation = false;
		lester.playerIsInConversation = false;
		floyd.playerIsInConversation = false;
		lowAside4Played = true;
		conversationIsPlaying = false;
	}


}
