using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DialogueDataManager
{
	private Dictionary<List<PlayerName>, List<Conversation>> dialogueDict; 
	//dictionary keys are lists of poker players still in the game, values are lists of conversations for those combinations


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


		fileRows = fileFullString.Split (rowSeparator, System.StringSplitOptions.None); //set filerows by splitting our file using row separator

		//List<Conversation> conversationList = new List<Conversation>(); //list of conversations to add to the dictionary
		for (int i = 2; i < fileRows.Length; i++)  //for each row in our array, ignoring the first two rows
		{
			//Debug.Log("Line " + i);
			fileRow = fileRows [i]; //set filerow to equal that row
			rowEntries = fileRow.Split (entrySeparator); //set entries by splitting the row using our entry separator
			if(rowEntries.Length < 8)
			{
				continue;
			}
			List<PlayerName> conversantList = new List<PlayerName>(); //list to hold the players who are in the conversation

			for (int j = 0; j < 5; j++)
			{
				//Debug.Log("Row " + j + " " + rowEntries[j]);
				if (rowEntries[j] != "")	
				{
                    PlayerName conversant = GetConversantNameFromString(rowEntries[j]);
                    conversantList.Add(conversant);
                    Debug.Log("Added conversant: " + conversant);
				}
			}



			List<PlayerLine> playerLinesList = new List<PlayerLine>(); //list to hold

			for (int j = 5; j < rowEntries.Length; j += 3) // for each set of player line data (three cells) in our entries
			{
				string playerNameText = rowEntries [j];
				string lineText = rowEntries [j+1];
				string audioFileText =  rowEntries [j+2];
				PlayerLine line = new PlayerLine(playerNameText, lineText, audioFileText); //create a player line and assign the next three entries
				playerLinesList.Add(line); //add line to list of player lines
			}

			Conversation conversation = new Conversation (playerLinesList); //create a conversation to contain the the list of conversants and player lines
			AddDialogueEntry(conversantList, conversation, dialogueDict); //add the conversant list and player lines list to the dialogue dictionary
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
}


public class Conversation //class for each conversation between players
{
	
	private List<PlayerLine> playerLines; // a list of the player lines in the convo

	public Conversation (List<PlayerLine> _playerLines)
	{
		_playerLines = playerLines;
	}
}


public class PlayerLine //class for each player line
{
	public string mainText; //the text the character is speaking
	public string playerName; //the name of the charater speaking
	public string audioFileName; //the name of the associated audiofile

	public PlayerLine(string _playerName, string _mainText, string _audioFileName) //constructor for player line
	{
		_playerName = playerName;
		_mainText = mainText;
		_audioFileName = audioFileName;
	}
}