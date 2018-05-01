using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DialogueDataManager
{
    private Dictionary<PlayerLineCriteria, List<PlayerLine>> oneLineDict;

    private Dictionary<List<PlayerName>, List<Conversation>> convoDict;
    //dictionary keys are lists of poker players still in the game, values are lists of conversations for those combinations
    public List<PlayerName> conversationKeys = new List<PlayerName>();
    private List<PlayerName> conversants = new List<PlayerName>();
    private List<PlayerName> potentialConversants = new List<PlayerName>(); //create a list to hold the names of the players we could potentially start a conversation with




#region Equality Comparers
    private class CriteriaComparer : IEqualityComparer<PlayerLineCriteria>
    { //generic for comparing lists
        public bool Equals(PlayerLineCriteria x, PlayerLineCriteria y)
        { //bool for comparing list x to list y
            foreach (LineCriteria lineCriterion in x.lineCriteria)
            {
                if (!y.lineCriteria.Contains(lineCriterion)) return false;
            }
            foreach (LineCriteria lineCriterion in y.lineCriteria)
            {
                if (!x.lineCriteria.Contains(lineCriterion)) return false;
            }
            return x.playerName == y.playerName;
        }

        public int GetHashCode(PlayerLineCriteria obj)
        {
            int hashcode = 0;
            foreach (LineCriteria t in obj.lineCriteria)
            {
                hashcode ^= t.GetHashCode();
            }
            hashcode ^= obj.playerName.GetHashCode();
            return hashcode;
        }
    }

	private class ListComparer<T> : IEqualityComparer<List<T>>
	{ //generic for comparing lists
		public bool Equals(List<T> x, List<T> y)
		{ //bool for comparing list x to list y
			foreach (T t in x)
			{
				if(!y.Contains(t)) return false;
			}
			foreach (T t in y)
			{
				if(!x.Contains(t)) return false;
			}
			return true;
		}

		public int GetHashCode(List<T> obj)
		{
			int hashcode = 0;
			foreach (T t in obj) 
			{
				hashcode ^= t.GetHashCode ();
			}
			return hashcode;
		}
	}
#endregion

    #region TextFileParsers
    public void ParseConvoDialogueFile(TextAsset dialogueFile) //parser for dialogue text file
	{
        convoDict = new Dictionary<List<PlayerName>, List<Conversation>> (new ListComparer<PlayerName>()); //our dictionary of dialogue
		string fileFullString = dialogueFile.text; //the raw text of the file
		string[] fileRows; //array of rows of our spreadsheet
		string[] rowEntries; //array of entries in our spreadsheet
		string fileRow; //holder for a file row
		string[] rowSeparator = new string[] { "\r\n", "\r", "\n"};  //array of row separators, all are on line end
		char[] entrySeparator = new char[] { '\t' }; //array of entry separators, specifically tab-separated

        List<PlayerName> conversantList = new List<PlayerName>(); //list to hold the players who are in the conversation
        List<PlayerLine> playerLinesList = new List<PlayerLine>(); //list to hold the player lines for this conversation
        int requiredRound = 0;

		fileRows = fileFullString.Split (rowSeparator, System.StringSplitOptions.None); //set filerows by splitting our file using row separator

        //List<Conversation> conversationList = new List<Conversation>(); //list of conversations to add to the dictionary
        for (int i = 0; i < fileRows.Length; i++)  //for each row in our array
        {
            fileRow = fileRows[i]; //set filerow to equal that row
            rowEntries = fileRow.Split(entrySeparator); //set entries by splitting the row using our entry separator
            string ident = rowEntries[0];
            if (ident != null && ident != "")
            {
                if (ident == "Convo") //if we are starting a conversation
                {
                    int.TryParse(rowEntries[1], out requiredRound);

                    for (int j = 2; j < rowEntries.Length; j++) //from the third column onward
                    {
                        if (rowEntries[j] != "")    //if the row entry isn't blank
                        {
                            PlayerName conversant = GetConversantNameFromString(rowEntries[j]); // use the entry to get the name of one of our conversants
                            conversantList.Add(conversant); //add this name to our list of conversants required for this conversation
                        }
                    }
                }
                else if (ident == "End") //if we have reached the end of a conversation
                {
                    List<PlayerLine> tempPlayerLines = new List<PlayerLine>(playerLinesList);
                    Conversation conversation = new Conversation(tempPlayerLines, requiredRound, false); //create a conversation to contain the player lines and required round
                    List<PlayerName> tempConversants = new List<PlayerName>(conversantList);

                    AddConvoDialogueEntry(tempConversants, conversation); //add the conversant list and player lines list to the dialogue dictionary
                    conversantList.Clear();//clear our previous lists
                    playerLinesList.Clear();
                }
                else if (ident == "Line") //if it's a player line
                {
                    string playerNameText = rowEntries[1]; //gets the string for the player name
                    string lineText = rowEntries[2]; //gets the text to be spoken, this isn't currently used in the game but will be needed for subtitles
                    string audioFileText = rowEntries[3]; //the string for the audiofile name

                    AudioClip audioFile = Resources.Load("Audio/Voice/Current/" + audioFileText) as AudioClip; //gets the audiofile from resources using the string name 
                    AudioSource audioSource = GetAudioSourceFromString(playerNameText); //get the correct audio source based on the players name
                    PlayerLine line = new PlayerLine(playerNameText, lineText, audioSource, audioFile); //create a player line and assign the next three entries
                    playerLinesList.Add(line); //add line to list of player lines                          
                }
                else 
                {
                    Debug.Log("Failed on ident " + ident);
                }
            }
        }
        foreach(KeyValuePair<List<PlayerName>, List<Conversation>> pair in convoDict){
            //Debug.Log(pair.Key[0] + " " + pair.Key[1]);
        }
	}

    public void ParseOneLinerDialogueFile(TextAsset dialogueFile) //parser for dialogue text file
    {
        oneLineDict = new Dictionary<PlayerLineCriteria, List<PlayerLine>>(new CriteriaComparer()); //(new ListComparer<PlayerLineCriteria>()); //our dictionary of dialogue
        string fileFullString = dialogueFile.text; //the raw text of the file
        string[] fileRows; //array of rows of our spreadsheet
        string[] rowEntries; //array of entries in our spreadsheet
        string fileRow; //holder for a file row
        string[] rowSeparator = new string[] { "\r\n", "\r", "\n" };  //array of row separators, all are on line end
        char[] entrySeparator = new char[] { '\t' }; //array of entry separators, specifically tab-separated

        fileRows = fileFullString.Split(rowSeparator, System.StringSplitOptions.None); //set filerows by splitting our file using row separator

        for (int i = 0; i < fileRows.Length; i++)  //for each row in our array
        {
            fileRow = fileRows[i]; //set filerow to equal that row
            rowEntries = fileRow.Split(entrySeparator); //set entries by splitting the row using our entry separator
            string ident = rowEntries[0];
            if (ident != null && ident != "")
            {
                if (ident == "Casey" || ident == "Floyd" || ident == "Minnie" || ident == "Nathaniel" || ident == "Zombie") //
                {
                    List<LineCriteria> lineCriteriaList = new List<LineCriteria>();// list of line criteria that will be added, along with the player name, to make a key reference for the dictionary
                    for (int j = 3; j < rowEntries.Length; j++) //from the fourth column onward
                    {
                        if (rowEntries[j] != "")    //if the row entry isn't blank
                        {
                            LineCriteria criteria = GetLineCriteriaFromString(rowEntries[j]); //get the appropriate line criteria using the entry's string
                            lineCriteriaList.Add(criteria); //add the line criteria to our list
                        }
                    }

                    string playerNameText = rowEntries[0]; //gets the string for the player name 
                    string lineText = rowEntries[1]; //gets the text to be spoken, this isn't currently used in the game but will be needed for subtitles
                    string audioFileText = rowEntries[2]; //the string for the audiofile name
                  
                    PlayerName speaker = GetConversantNameFromString(playerNameText); // use the entry to get the name of the player who is speaking
                    AudioClip audioFile = Resources.Load("Audio/Voice/Current/" + audioFileText) as AudioClip; //gets the audiofile from resources using the string name 
                    AudioSource audioSource = GetAudioSourceFromString(playerNameText); //get the correct audio source based on the player name
                 
                    PlayerLine playerline = new PlayerLine(playerNameText, lineText, audioSource, audioFile); //create a player line using our data
                    PlayerLineCriteria playerLineCriteria = new PlayerLineCriteria(speaker, lineCriteriaList); //combine the speaker's name and our criteria list intop our dictionary key

                    AddOneLinerDialogueEntry(playerLineCriteria, playerline);
                }
                else
                {
                    Debug.Log("Failed on ident " + ident);
                }
            }
        }
        foreach (KeyValuePair<PlayerLineCriteria, List<PlayerLine>> pair in oneLineDict)
        {
           // Debug.Log(pair.Key.playerName +  ", " + pair.Value);
        }
    }

#endregion


    void AddConvoDialogueEntry(List<PlayerName> playerList, Conversation conversationToAdd)
	{ //adds a new key/value entry in our dialogue dictionary
		if (convoDict.ContainsKey (playerList))
		{ //if the dictionary already contains the passed in key (list of players)
			convoDict [playerList].Add(conversationToAdd); //add the conversation to that key
		} 
		else 
		{
			List<Conversation> conversationList = new List<Conversation>(); 
			conversationList.Add (conversationToAdd);
			convoDict.Add (playerList, conversationList);
		}
	}

    void AddOneLinerDialogueEntry(PlayerLineCriteria playerLineCriteria, PlayerLine lineToAdd)
    {
        if (oneLineDict.ContainsKey(playerLineCriteria))
        {
            oneLineDict[playerLineCriteria].Add(lineToAdd);
        }
        else
        {
            List<PlayerLine> lineList = new List<PlayerLine>();
            lineList.Add(lineToAdd);
            oneLineDict.Add(playerLineCriteria, lineList); 
        }
    }

    public PlayerLine ReadyOneLiner(PlayerLineCriteria criteriaKey)
    {
        PlayerLine lineToPlay = GetLineWithCriteria(criteriaKey);

        return lineToPlay;
    }

    public Conversation ReadyConversation()
    {
        GetConversantNamesFromActivePlayers();
        Conversation convoToPlay = GetConversationWithNames(conversants);

        return convoToPlay;
    }

    public void GetConversantNamesFromActivePlayers() 
    {
        potentialConversants.Clear(); //clean slate for lists
        conversants.Clear();

        for (int i = 0; i < Services.Dealer.activePlayers.Count; i++) //populate that list with the names of our active players
        {
            potentialConversants.Add(Services.Dealer.activePlayers[i].playerName);
        }

        int conversantCount = 2;
        //int conversantCount = Random.Range(2, Services.Dealer.activePlayers.Count); //choose a random number (min two) of active players as conversants

        for (int i = 0; i < conversantCount; i++)
        {
            PlayerName randomName = potentialConversants[Random.Range(0, potentialConversants.Count)]; // randomly choose one of our potential conversants
            conversants.Add(randomName); //add them to our list of conversants
            potentialConversants.Remove(randomName); //remove them from the potential conversant list so we don't get them again
        }
        foreach (PlayerName name in conversants) 
        {
            Debug.Log("Added " + name + " to list of conversants");
        }
    }

    public Conversation GetConversationWithNames(List<PlayerName> namesKey) //using the names of our chosen conversants
    {
        if (convoDict.ContainsKey(namesKey)) // if our dialogue dictionary contains them as a key
        {
            List<Conversation> possibleConversations = convoDict[namesKey]; //list of conversations that match the key
            int correctConversation = -1;  //the conversation we'll want to play, set at first to the last in the list

            for (int i = 0; i < possibleConversations.Count; i++) //for each conversation in the list
            {
                if (!possibleConversations[i].hasBeenPlayed && //if the conversation hasn't yet played
                   possibleConversations[i].minRequiredRound <= (6 - Services.Dealer.activePlayers.Count)) //and if the conversation comes earlier than our currently chosen conversation
                {
                    correctConversation = i; //update the correct conversation to be this new, earlier convo
                }
            }
            if (correctConversation == -1) return null;
            foreach(PlayerName name in conversants)
            {
                Services.SoundManager.playersInConvo.Add(name);
            }
            Services.AnimationScript.CharacterConvoAnimation(true);
            Services.SoundManager.conversationIsPlaying = true;
            return possibleConversations[correctConversation]; //return the convo, the earliest that has not yet been played
        }
        else //if the dialogue dict does not contain the names as a key
        {
            Debug.Log("dictionary does not contain key");

            Services.SoundManager.conversationIsPlaying = false;
            return null;
        }
    }

    public PlayerLine GetLineWithCriteria(PlayerLineCriteria criteriaKey) //using the names of our chosen conversants
    {
        if (oneLineDict.ContainsKey(criteriaKey)) // if our dialogue dictionary contains them as a key
        {
            List<PlayerLine> possibleLines = oneLineDict[criteriaKey]; //list of conversations that match the key
            int correctLine = 0;
            if (possibleLines.Count > 0)
            {
                correctLine = Random.Range(0, possibleLines.Count());
            }
            else
            {
                Debug.Log("Key contained no values");
                return null;
            }
            return possibleLines[correctLine]; //return the convo, the earliest that has not yet been played
        }
        else //if the dialogue dict does not contain the names as a key
        {
            Debug.Log("dictionary does not contain key");

            Services.SoundManager.conversationIsPlaying = false;
            return null;
        }
    }

	PlayerName GetConversantNameFromString (string nameString)
	{  //uses the player name strings from the file to find the correct player names in the game
		string str = nameString.ToUpper ().Trim(); //turns all letters in string to uppercase and removes spaces on either side

		switch(str)
		{
		case "CASEY":
			return PlayerName.Casey;
		case "FLOYD":
			return PlayerName.Floyd;
		case "MINNIE":
			return PlayerName.Minnie;
		case "NATHANIEL":
			return PlayerName.Nathaniel;
		case "ZOMBIE":
			return PlayerName.Zombie;
		default:
			return PlayerName.None;
		}
	}

    AudioSource GetAudioSourceFromString(string nameString)
    {  //uses the player name strings from the file to find the correct audio sources for the audio files
        string str = nameString.ToUpper().Trim(); //turns all letters in string to uppercase and removes spaces on either side

        switch (str)
        {
            case "CASEY":
                return Services.SoundManager.caseySource;
            case "FLOYD":
                return Services.SoundManager.floydSource;
            case "MINNIE":
                return Services.SoundManager.minnieSource;
            case "NATHANIEL":
                return Services.SoundManager.nathanielSource;
            case "ZOMBIE":
                return Services.SoundManager.zombieSource;
            default:
                return null;
        }
    }

    LineCriteria GetLineCriteriaFromString(string criteriaString)
    {  //uses the player name strings from the file to find the correct player names in the game
        string str = criteriaString.ToUpper().Trim(); //turns all letters in string to uppercase and removes spaces on either side

        switch (str)
        {
            case "ALLIN":
                return LineCriteria.AllIn;
            case "BET":
                return LineCriteria.Bet;
            case "BUYIN":
                return LineCriteria.BuyIn;
            case "BUYINASK":
                return LineCriteria.BuyInAsk;
            case "CALL":
                return LineCriteria.Call;
            case "CARDHIT":
                return LineCriteria.CardHit;
            case "CHECK":
                return LineCriteria.Check;
            case "CHIPSMOVED":
                return LineCriteria.ChipsMoved;
            case "FIFTYTWO":
                return LineCriteria.FiftyTwo;
            case "FOLD":
                return LineCriteria.Fold;
            case "IDLETIME":
                return LineCriteria.IdleTime;
            case "LOSE":
                return LineCriteria.Lose;
            case "MISDEAL":
                return LineCriteria.Misdeal;
            case "RAISE":
                return LineCriteria.Raise;
            case "THROWCHIPS":
                return LineCriteria.ThrowsChips;
            case "TIP":
                return LineCriteria.Tip;
            case "TOUCHEDBYHAND":
                return LineCriteria.TouchedByHand;
            case "WIN":
                return LineCriteria.Win;
            case "WRONGCHIPS":
                return LineCriteria.WrongChips;
            default:
                return LineCriteria.None;
        }
    }

    public static PlayerLineCriteria CreatePlayerLineCriteria(PlayerName playerName, params LineCriteria[] criteriaArray)
    {
        List<LineCriteria> lineCriteriaList = new List<LineCriteria>();
        foreach (LineCriteria criteria in criteriaArray)
        {
            //Debug.Log("adding criteria: " + criteria);
            lineCriteriaList.Add(criteria);
        }
        PlayerLineCriteria newPlayerLineCriteria = new PlayerLineCriteria(playerName, lineCriteriaList);
        //Debug.Log("adding player: " + newPlayerLineCriteria.playerName);

        return newPlayerLineCriteria;
    }
}
   

public class Conversation //class for each conversation between players
{
    
	public List<PlayerLine> playerLines; // a list of the player lines in the convo
    public int minRequiredRound; //the minimum round it can be before this convo can play
    public List<string> requirements;
    public bool hasBeenPlayed = false;

	public Conversation (List<PlayerLine> _playerLines, int _minRequiredRound, bool _hasBeenPlayed)
	{
		playerLines = _playerLines;
        minRequiredRound = _minRequiredRound;
        hasBeenPlayed = _hasBeenPlayed;
	}
}


public class PlayerLine //class for each player line
{
	public string mainText; //the text the character is speaking
	public string playerName; //the name of the charater speaking

    public AudioSource audioSource;
    public AudioClip audioFile; //the name of the associated audiofile

    public PlayerLine(string _playerName, string _mainText, AudioSource _audioSource, AudioClip _audioFile) //constructor for player line
	{
		playerName = _playerName;
		mainText = _mainText;
        audioSource = _audioSource;
		audioFile = _audioFile;
	}
}

public class PlayerLineCriteria
{
    public PlayerName playerName;
    public List<LineCriteria> lineCriteria;



    public PlayerLineCriteria (PlayerName _playerName, List<LineCriteria> _lineCriteria)
    {
        playerName = _playerName;
        lineCriteria = _lineCriteria;
    }
}