using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls all the sounds
public class SoundManager : MonoBehaviour
{
	public TextAsset convoDialogueFile; //the text file we draw our dialogue from
    public TextAsset oneLinerDialogueFiler;

    [Header("Poker Players")]
	public PokerPlayerRedux casey;// rosa
	public PokerPlayerRedux zombie;// gonzalo
	public PokerPlayerRedux minnie;// minnie
	public PokerPlayerRedux nathaniel;// nathaniel
	public PokerPlayerRedux floyd;// floyd

    [Header("Player Audio Sources")]
	public AudioSource caseySource; 
	public AudioSource zombieSource;
	public AudioSource minnieSource;
	public AudioSource nathanielSource;
	public AudioSource floydSource;

    [Header("Conversations")]
    public bool conversationIsPlaying;

    /// 
    public List<PlayerName> playersInConvo = new List<PlayerName>(); //DAVID THIS IS WHERE THE PLAYERS TALKING ARE
																	 /// 
    
   
	[Header("Music Audio Sources")]
	public AudioSource jazzyIntroSource;
	public AudioSource drumsAndBassSource;
	public AudioSource saxSource;
	public AudioSource trumpetSource;
	public AudioSource tromboneSource;
	public AudioSource vibesSource;
	public AudioSource chaosSource;
	public AudioSource dramaSource;
   
    [Header("Sound Effects")]
	public AudioClip[] chips;
    public AudioClip[] cards;
    public AudioClip[] cardTones;
    public AudioClip tipSFX;
    public AudioClip fallingTip;
    public AudioClip clockTick;
    public AudioClip clockDing;
    public AudioClip deckThrown;
    public AudioClip cardsReturn;
    public AudioClip cardsBurning;
    public AudioClip poofNoise;

    [Header("IntroConversation")]
    public AudioClip Nathaniel_Intro1;
    public AudioClip Floyd_Intro;
    public AudioClip Zombie_Intro;
    public AudioClip Minnie_Intro;
    public AudioClip Casey_Intro;
    public AudioClip Nathaniel_Intro2;
    public AudioClip Minnie_Intro2;


    public int roundsFinished;
	public ShuffleBag<AudioSource> shuffle_main_themes;
	public TaskManager music_player;


    public void PlayOneLiner(PlayerLineCriteria criteria)
    {
        
        PlayerLine line = Services.DialogueDataManager.ReadyOneLiner(criteria);
        AudioClip audioLine = line.audioFile;
        AudioSource playerSpeaking = line.audioSource;
        //Debug.Log(playerSpeaking.name);
        //Debug.Log(audioLine.name);
        GetSourceAndPlay(playerSpeaking, audioLine);

    }

    public void PlayConversation()
    {
        Debug.Log("calling for conversation");
        if (!conversationIsPlaying)
        {
            conversationIsPlaying = true;
            Conversation convoAudio = Services.DialogueDataManager.ReadyConversation(); //find us an appropriate conversation from our dictionary
            if (convoAudio != null)
            {
                StartCoroutine(PlayConversationLines(convoAudio)); //plays through the lines in our chosen conversation
            }
        }
    }

    IEnumerator PlayConversationLines(Conversation convo) //coroutine for playing conversation audio lines
    {

        for (int i = 0; i < convo.playerLines.Count; i++) //for each line in our conversation
        {
            AudioClip audioLine = convo.playerLines[i].audioFile; //get the audio to play
            AudioSource playerSpeaking = convo.playerLines[i].audioSource; // get the source to play at
            GetSourceAndPlay(playerSpeaking, audioLine); //pass these and play

            while (playerSpeaking.isPlaying) //don't move to the next line while our current source is still playing
            {
                yield return null;
            }
        }
        Services.AnimationScript.CharacterConvoAnimation(false);
        playersInConvo.Clear();
        conversationIsPlaying = false;
        convo.hasBeenPlayed = true; //once all lines have been played, set the bool on the conversation so that we don't choose it again

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
        if (Services.Dealer.audioSources.Contains(source))
        {
            Services.Dealer.audioSources.Remove(source);
        }
        Destroy(specialAudioSource, clip.length);
        //Debug.Log("Clip played: " + clip.name);
    }
    
	public void GetSourceAndPlay(AudioSource source, AudioClip clip)
	{
        PokerPlayerRedux player = source.gameObject.GetComponentInParent<PokerPlayerRedux>();
        player.playerIsInConversation = true;
       // Debug.Log(player + " is in conversation: " + player.playerIsInConversation);
		source.clip = clip;
		source.Play();
        StartCoroutine(PlayerStopsTalking(clip.length, player));
	}

    public void GetNonPlayerSourceAndPlay(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }

    IEnumerator PlayerStopsTalking(float time, PokerPlayerRedux player)
    {
        yield return new WaitForSeconds(time);
        player.playerIsInConversation = false;
    }
    

	/*public float PlayIntroMusic()
	{
		jazzyIntroSource.Play();
		return jazzyIntroSource.clip.length;

	}

	public float PlayDrumAndBass() {
		drumsAndBassSource.Play();
		return drumsAndBassSource.clip.length;
	}

	public float PlayOther() {
		var new_source = shuffle_main_themes.Next();
		new_source.Play();
		return new_source.clip.length;
	}*/

	public void NewSongs() {
		var reg_source = drumsAndBassSource;
		var reg_source_length = reg_source.clip.length;

		var weird_source = shuffle_main_themes.Next();
		var weird_source_length = weird_source.clip.length;


		ActionTask play_next = new ActionTask(() =>{ reg_source.Play(); });

		play_next
			.Then(new Wait(reg_source_length))
			.Then(new ActionTask(() => { weird_source.Play(); }))
			.Then(new Wait(weird_source_length))
			.Then(new ActionTask(NewSongs));

		music_player.Do(play_next);

	}

	void Start() {
		music_player = new TaskManager();
		AudioSource[] audio_sources = { saxSource, trumpetSource, tromboneSource, vibesSource };
		shuffle_main_themes = new ShuffleBag<AudioSource>();

		foreach (var source in audio_sources)
			shuffle_main_themes.Add(source);

		ActionTask play_intro = new ActionTask(() => { jazzyIntroSource.Play();   Debug.Log("Playing"); });

		play_intro
			.Then(new Wait(jazzyIntroSource.clip.length))
			.Then(new ActionTask(NewSongs));

		music_player.Do(play_intro);
	}

	void Update()
	{
		music_player.Update();
	}

	public void InterruptChaos() {
		chaosSource.volume = 0.0f;
		chaosSource.Play();
        var start_time = Time.time;
        var fade_length = 2.0f;

		OnGoingTask fader = new OnGoingTask(() =>
        {
			var new_volume = 1.0f - ((Time.time - start_time) / fade_length);
            jazzyIntroSource.volume = new_volume;
			drumsAndBassSource.volume = new_volume;
			saxSource.volume = new_volume;
			trumpetSource.volume = new_volume;
			tromboneSource.volume = new_volume;
			vibesSource.volume = new_volume;
            chaosSource.volume = (Time.time - start_time) / fade_length;
		}, fade_length);      

		music_player.Do(fader);
	}

	public void PlayDrama()
	{
		var fade_length = 2.0f;
		var start_time = Time.time;
		var fade_in_start_time = Time.time + dramaSource.clip.length;
		var wait_until_drama_start = 1.0f;

		OnGoingTask fader = new OnGoingTask(() =>
		{
			chaosSource.volume = 1.0f - ((Time.time - start_time) / fade_length);
			if (!dramaSource.isPlaying)
			{
				if (Time.time - start_time >= wait_until_drama_start)
				{
					dramaSource.volume = 1.0f;
					dramaSource.Play();
				}
			}

		}, fade_length);

		fader
			.Then(new Wait(dramaSource.clip.length - fade_length))
			.Then(new OnGoingTask(() =>
			{
				jazzyIntroSource.Stop();
				drumsAndBassSource.Stop();
				saxSource.Stop();
				trumpetSource.Stop();
				tromboneSource.Stop();
				vibesSource.Stop();

				var new_volume = ((Time.time - fade_in_start_time) / fade_length);

				jazzyIntroSource.volume = new_volume;
				drumsAndBassSource.volume = new_volume;
				saxSource.volume = new_volume;
				trumpetSource.volume = new_volume;
				tromboneSource.volume = new_volume;
				vibesSource.volume = new_volume;
			}, fade_length));

		music_player.Do(fader);
	}

}

public class AudioData 
{
	public AudioClip audio;
	public bool hasBeenPlayed;
    public bool finishedPlaying;

    public AudioData(AudioClip _audio, bool _hasBeenPlayed)
	{
		audio = _audio;
		hasBeenPlayed = _hasBeenPlayed;
	}

}
