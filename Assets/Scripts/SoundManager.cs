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


    #region TUTORIAL STUFF 
    public AudioSource tutorial;
	public AudioClip[] tutorialAudio;
    int tutorialIndex = 1;
    public List<AudioData> tutorialAudioFiles = new List<AudioData>();
   
    public int handCounter = 1; //which hand are we on? First, second, third.... 
    public int roundsFinished = 0; //how many rounds of betting have we finished this hand
    public bool roundOneComplete = false;
    public bool roundTwoComplete = false;
    public bool roundThreeComplete = false;
    public int cardsBurned = 0; //how many cards have we burned
    public bool burnedFirstCard = false;
    public bool burnedSecondCard = false;
    public bool burnedThirdCard = false;
    public bool flopDealt = false;
    public bool turnDealt = false;
    public bool riverDealt = false;
    public bool havePickedUpDeck = false;
    public bool havePickedUpDeckForFirstTime = false;
    public int numberOfShuffles = 0;
    public bool haveShuffledOnce = false;
    public bool haveLookedAtFirstPlayer = false;
    public bool lookAudioPlayed = false;
    public bool dealtTwoCards = false;
    public bool fiveFaceUpCardsDealt = false;
    public int faceUpTableCards = 0; 
    public bool dealerButtonMoved = false;
    public bool buttonMovedAgain = false;
    public bool gaveWinnerEarnings = false;
    public bool secondHand = false;
    public bool cheatingEngaged = false;
    public bool letEmLoose = false;

    #endregion
    [Header("Conversations")]
    public bool conversationIsPlaying;

    #region Old Audio 

    public AudioClip[] aside1Index;
    public AudioClip[] aside2Index;
    public AudioClip[] aside3Index;
    public AudioClip[] aside5Index;
    public AudioClip[] aside6Index;

    public AudioClip[] lowAside1Index;
    public AudioClip[] lowAside2Index;
    public AudioClip[] lowAside3Index;
    public AudioClip[] lowAside4Index;

    public bool aside1Played;
    public bool aside2Played;
	public bool aside3Played;
	public bool aside5Played;
	public bool aside6Played;

	public bool lowAside1Played;
	public bool lowAside2Played;
	public bool lowAside3Played;
	public bool lowAside4Played;

    #endregion

    [Header("Sound Effects")]
	public AudioClip[] chips;
    public AudioClip[] cards;
    public AudioClip[] cardTones;
    public AudioClip tipSFX;
    public AudioClip fallingTip;

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



    #region TUTORIAL FUNCTIONS
    public void PlayTutorialAudio(int index)

    {
        GenerateSourceAndPlay(tutorialAudioFiles[index].audio, 1f, 1f);
        tutorialAudioFiles[index].hasBeenPlayed = true;
        StartCoroutine(WaitForClipToFinishPlaying(tutorialAudioFiles[index].audio.length, tutorialAudioFiles[index]));
    }


    IEnumerator WaitForClipToFinishPlaying(float time, AudioData clip)
    {
        yield return new WaitForSeconds(time);
        clip.finishedPlaying = true;
    }

    public void CheckForTutorialAudioToBePlayed()
    {
        Debug.Log("Tutorial index: " + tutorialIndex);
        if (handCounter == 1)
        {
            if (tutorialAudioFiles[tutorialIndex - 1].finishedPlaying && !tutorialAudioFiles[tutorialIndex].hasBeenPlayed) 
            {
                //player picks up deck for first time 
                if (havePickedUpDeck)
                {
                   // Debug.Log("Thinks we have picked up deck");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    havePickedUpDeck = false;
                }
                //player deals face up card to each player 
                else if (Services.Dealer.cardsTouchingTable.Count >= 5 && !fiveFaceUpCardsDealt)
                {
                    //Debug.Log("Caught if seen more than once");
                    int cardsFaceUp = 0;
                    for (int i = 0; i < Services.Dealer.cardsTouchingTable.Count; i++)
                    {
                        if (Services.Dealer.cardsTouchingTable[i].CardIsFaceUp()) cardsFaceUp++;
                    }
                    if (cardsFaceUp >= 5)
                    {
                        //Debug.Log("Thinks we have dealt a face up card to each player");
                        PlayTutorialAudio(tutorialIndex);
                        tutorialIndex++;
                        fiveFaceUpCardsDealt = true;

                    }
                }
                else if (!dealerButtonMoved && tutorialIndex == 3) //player placed dealer button in correct place (not in right now so just plays automatically)
                {
                   // Debug.Log("Should play automatically since there is no dealer button movement mechanic");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    dealerButtonMoved = true;
                }

                //player collects cards into deck 
                else if (haveShuffledOnce && fiveFaceUpCardsDealt && Services.Dealer.cardsTouchingTable.Count == 0 && numberOfShuffles >= 2)
                {
                   // Debug.Log("Thinks we have shuffled for the first time");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    haveShuffledOnce = false;
                    Table.gameState = GameState.NewRound;
                }
                //player deals 2 cards to each character 
                else if (Table.gameState == GameState.PreFlop && !dealtTwoCards)
                {
                   // Debug.Log("Thinks we have dealt 2 cards to each character ");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    dealtTwoCards = true;
                }
                //looks at first player 
                else if (haveLookedAtFirstPlayer && !lookAudioPlayed)
                {
                   // Debug.Log("Thinks we have looked at the first player");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    haveLookedAtFirstPlayer = false;
                    lookAudioPlayed = true;
                }
                //1st round over
                else if (roundsFinished == 1 && !roundOneComplete)
                {
                   // Debug.Log("Thinks the first round is finished");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    roundOneComplete = true;
                }
                //put first card in burn pile
                else if (Table.instance.burn.Count == 1 && !Table.instance.burn[0].CardIsFaceUp() && !burnedFirstCard)
                {
                  //  Debug.Log("Thinks we've burned one card");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    burnedFirstCard = true;
                }
                //puts three cards in center
                else if (Table.instance.board.Count == 3 && !flopDealt)
                {
                    foreach (Card card in Table.instance.board)
                    {
                        if (card.cardFacingUp)
                        {
                            faceUpTableCards++;
                        }
                    }
                    if (faceUpTableCards >= 3)
                    {
                      //  Debug.Log("thinks we have dealt the flop");
                        PlayTutorialAudio(tutorialIndex);
                        tutorialIndex++;
                        flopDealt = true;
                        faceUpTableCards = 0;
                    }
                }
                //2nd round over
                else if (roundsFinished == 2 && !roundTwoComplete)
                {
                   // Debug.Log("Thinks the second round is finished");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    roundTwoComplete = true;
                }
                //burn and turn
                else if (Table.instance.board.Count == 4 && Table.instance.burn.Count == 2 && !turnDealt && !burnedSecondCard)
                {
                    foreach (Card card in Table.instance.board)
                    {
                        if (card.cardFacingUp)
                        {
                            faceUpTableCards++;
                        }
                    }
                    foreach (Card card in Table.instance.burn)
                    {
                        if (!card.CardIsFaceUp())
                        {
                            cardsBurned++;
                        }
                    }
                    if (faceUpTableCards >= 4 && cardsBurned == 2)
                    {
                        //Debug.Log("thinks we have burned and turned");
                        PlayTutorialAudio(tutorialIndex);
                        tutorialIndex++;
                        turnDealt = true;
                        burnedSecondCard = true;
                        faceUpTableCards = 0;
                        cardsBurned = 0;
                    }
                }
                //3rd round over
                else if (roundsFinished == 3 && !roundThreeComplete)
                {
                   // Debug.Log("Thinks the third round is finished");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    roundThreeComplete = true;
                }
                //burn and river
                else if (Table.instance.board.Count == 5 && Table.instance.burn.Count == 3 && !riverDealt && !burnedThirdCard)
                {
                    foreach (Card card in Table.instance.board)
                    {
                        if (card.cardFacingUp)
                        {
                            faceUpTableCards++;
                        }
                    }
                    foreach (Card card in Table.instance.burn)
                    {
                        if (!card.CardIsFaceUp())
                        {
                            cardsBurned++;
                        }
                    }
                    if (faceUpTableCards >= 5 && cardsBurned == 3)
                    {
                      //  Debug.Log("thinks we have burned and river");
                        PlayTutorialAudio(tutorialIndex);
                        tutorialIndex++;
                        riverDealt = true;
                        burnedThirdCard = true;
                        faceUpTableCards = 0;
                        cardsBurned = 0;
                    }
                }
                else if (!gaveWinnerEarnings && Services.Dealer.cleaningCards && tutorialIndex == 14) //player placed dealer button in correct place (not in right now so just plays automatically)
                {
                   // Debug.Log("gave player winnings");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    gaveWinnerEarnings = true;

                }
                else if (!secondHand && tutorialIndex == 15) //player placed dealer button in correct place (not in right now so just plays automatically)
                {
                   // Debug.Log("automatic");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    secondHand = true;

                }
                else if (!buttonMovedAgain && tutorialIndex == 16) //player placed dealer button in correct place (not in right now so just plays automatically)
                {
                  //  Debug.Log("automatic");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex++;
                    buttonMovedAgain = true;

                }
                else if (GameObject.FindGameObjectWithTag("CardDeck").GetComponent<CardDeckScript>().cheating && !cheatingEngaged)
                {
                    Debug.Log("CHEATING");
                    PlayTutorialAudio(tutorialIndex);
                    tutorialIndex = 23;
                    cheatingEngaged = true;
                    Debug.Log("Let em loose is: " + letEmLoose);
                }
                else if (!letEmLoose && tutorialIndex == 23)
                {
                    Debug.Log("Let em loose");
                    PlayTutorialAudio(tutorialIndex);        
                    letEmLoose = true;
                    tutorialIndex = 27;
                }
                else if (Services.Dealer.killingCards && tutorialIndex >= 23)
                {
                    PlayTutorialAudio(tutorialIndex);
                    Table.instance.RestartRound();
                }

            }
        }
    }
#endregion


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
    public bool finishedPlaying;

    public AudioData(AudioClip _audio, bool _hasBeenPlayed)
	{
		audio = _audio;
		hasBeenPlayed = _hasBeenPlayed;
	}

}
