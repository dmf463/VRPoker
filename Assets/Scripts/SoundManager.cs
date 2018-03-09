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

    [Header("Sound Effects")]
	public AudioClip[] chips;
    public AudioClip[] cards;
    public AudioClip[] cardTones;
    public AudioClip tipSFX;
    public AudioClip fallingTip;


    public int roundsFinished;

	
    public void PlayOneLiner(PlayerLineCriteria criteria)
    {
        
        PlayerLine line = Services.DialogueDataManager.ReadyOneLiner(criteria);
        AudioClip audioLine = line.audioFile;
        AudioSource playerSpeaking = line.audioSource;
        Debug.Log(playerSpeaking.name);
        Debug.Log(audioLine.name);
        GetSourceAndPlay(playerSpeaking, audioLine);

    }

    public void PlayConversation()
    {
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
