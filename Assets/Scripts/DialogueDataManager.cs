using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DialogueDataManager
{
	private Dictionary<List<PlayerName>, List<Conversation>> dialogueDict;
    //dictionary keys are lists of poker players still in the game, values are lists of conversations for those combinations
    private List<PlayerName> conversants = new List<PlayerName>();
    private List<PlayerName> potentialConversants = new List<PlayerName>(); //create a list to hold the names of the players we could potentially start a conversation with


	public void Awake()
	{
		//ParseDialogueFile(Services.SoundManager.dialogueFile); //immediately parse the text file containing our dialogue
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
		

	public void ParseDialogueFile(TextAsset dialogueFile) //parser for dialogue text file
	{
		dialogueDict = new Dictionary<List<PlayerName>, List<Conversation>> (new ListComparer<PlayerName>()); //our dictionary of dialogue
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
                if (rowEntries.Length < 3) //if there are less than three entries, meaning in this case that there is not enough information to form a conversation
                {
                    continue;
                }
                else if (ident == "Convo") //if we are starting a conversation
                {
                    Debug.Log("Convo");

                    int.TryParse(rowEntries[1], out requiredRound);

                    Debug.Log("required Round" + rowEntries[1]);
                
                    for (int j = 2; j < rowEntries.Length; j++) //from the third column onward
                    {
                        //Debug.Log("Row " + j + " " + rowEntries[j]);
                        if (rowEntries[j] != "")    //if the row entry isn't blank
                        {
                            PlayerName conversant = GetConversantNameFromString(rowEntries[j]); // use the entry to get the name of one of our conversants
                            conversantList.Add(conversant); //add this name to our list of conversants required for this conversation
                            Debug.Log("Added conversant: " + conversant);
                        }
                    }
                }
                else if (ident == "End") //if we have reached the end of a conversation
                {
                    Debug.Log(("End"));
                    Conversation conversation = new Conversation(playerLinesList, requiredRound, false); //create a conversation to contain the player lines and required round
                    AddDialogueEntry(conversantList, conversation, dialogueDict); //add the conversant list and player lines list to the dialogue dictionary
                    conversantList.Clear();//clear our previous lists
                    playerLinesList.Clear();
                }
                else if (ident == "Line") //if it's a player line
                {
                    Debug.Log("PlayerLine");
                    string playerNameText = rowEntries[1]; //gets the string for the player name
                    string lineText = rowEntries[2]; //gets the text to be spoken, this isn't currently used in the game but will be needed for subtitles
                    string audioFileText = rowEntries[3]; //the string for the audiofile name
                                                          //Log(audioFileText);
                    AudioClip audioFile = Resources.Load("Audio/Voice/TutorialVO/" + audioFileText) as AudioClip; //gets the audiofile from resources using the string name 
                    Debug.Log(audioFile);
                    AudioSource audioSource = GetAudioSourceFromString(playerNameText); //get the correct audio source based on the players name

                    PlayerLine line = new PlayerLine(playerNameText, lineText, audioSource, audioFile); //create a player line and assign the next three entries
                    playerLinesList.Add(line); //add line to list of player lines                          
                }
                else {
                    Debug.Log("Failed on ident " + ident);
                }
            }
        }
	}

	
	void AddDialogueEntry(List<PlayerName> playerList, Conversation conversationToAdd, Dictionary<List<PlayerName>, List<Conversation>> dict)
	{ //adds a new key/value entry in our dialogue dictionary
		if (dict.ContainsKey (playerList))
		{ //if the dictionary already contains the passed in key (list of players)
			dict [playerList].Add(conversationToAdd); //add the conversation to that key
		} 
		else 
		{
			List<Conversation> conversationList = new List<Conversation>(); 
			conversationList.Add (conversationToAdd);
			dict.Add (playerList, conversationList);
		}
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
        for (int i = 0; i < 2; i++)
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



    public Conversation GetConversationWithNames (List<PlayerName> namesKey) //using the names of our chosen conversants
    {
        Debug.Log(namesKey[0] + ", " + namesKey[1]);
        if(dialogueDict.ContainsKey(namesKey)) // if our dialogue dictionary contains them as a key
        {
            List<Conversation> possibleConversations = dialogueDict[namesKey]; //list of conversations that match the key
            int correctConversation = possibleConversations.Count;  //the conversation we'll want to play, set at first to the last in the list
            for (int i = 0; i < possibleConversations.Count; i++) //for each conversation in the list
            {
                if(!possibleConversations[i].hasBeenPlayed && //if the conversation hasn't yet played
                   possibleConversations[i].minRequiredRound < correctConversation) //and if the conversation comes earlier than our currently chosen conversation
                {
                    correctConversation = i; //update the correct conversation to be this new, earlier convo
                }
            }
            Debug.Log(possibleConversations[correctConversation].playerLines[0]);
            return possibleConversations[correctConversation]; //return the convo, the earliest that has not yet been played

        }
        else //if the dialogue dict does not contain the names as a key
        {
            Debug.Log("dictionary does not contain key");
            return null;
        }
    }

}



public class Conversation //class for each conversation between players
{
    
	public List<PlayerLine> playerLines; // a list of the player lines in the convo
    public int minRequiredRound; //the minimum round it can be before this convo can play
    public bool hasBeenPlayed = false;

	public Conversation (List<PlayerLine> _playerLines, int _minRequiredRound, bool _hasBeenPlayed)
	{
		_playerLines = playerLines;
        _minRequiredRound = minRequiredRound;
        _hasBeenPlayed = hasBeenPlayed;
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
		_playerName = playerName;
		_mainText = mainText;
        _audioSource = audioSource;
		_audioFile = audioFile;
	}
}