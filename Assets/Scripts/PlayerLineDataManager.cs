using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DialogueDataManager
{
	private Dictionary<List<PokerPlayerRedux>, List<Conversation>> dialogueDict; 
	//dictionary keys are lists of pokerPlayers still in the game, values are lists of conversations for those combinations



	public void Awake()
	{
		ParseDialogueFile(Services.SoundManager.dialogueFile); //immediately parse the text file containing our dialogue
	}

	private class ListComparer<T> : IEqualityComparer<List<T>>
	{ //generic for comparing lists
		public bool Equals(List<T> x, List<T> y)
		{ //bool for comparing list x to list y
			return x.SequenceEqual (y); //return whether the lists are equal
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

	public class StringArrayComparer : IEqualityComparer<string[]>
	{
		public bool Equals(string[] x, string[] y)
		{
			if (x.Length != y.Length)
			{
				return false;
			}
			for (int i = 0; i < x.Length; i++)
			{
				if (x[i] != y[i])
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(string[] obj)
		{
			int hashcode = 0;
			foreach (string t in obj)
			{
				hashcode ^= t.GetHashCode();
			}
			return hashcode;
		}
	}

	public void ParseDialogueFile(TextAsset dialogueFile) //parser for dialogue text file
	{
		dialogueDict = new Dictionary<List<PokerPlayerRedux>, List<Conversation>>(); //our dictionary of dialogue 
		string fileFullString = dialogueFile.text;
		string[] fileRows; 
		string[] rowEntries;
		string fileRow;
		string[] rowSeparator = new string[] { "\r\n", "\r", "\n"};  
		char[] entrySeparator = new char[] { '\t' };
		fileRows = fileFullString.Split (rowSeparator, System.StringSplitOptions.None);

		for (int i = 1; i < fileRows.Length; i++) 
		{
			fileRow = fileRows [i];
			rowEntries = fileRow.Split (entrySeparator);
			Conversation conversation = new Conversation();

			for (int j = 2; j < rowEntries.Length; j += 3)
			{
				PlayerLine line = new PlayerLine(rowEntries[j], rowEntries[j+1], rowEntries[j+2]);
			}
		}
	}

	void Add
}


public class Conversation //class for each conversation between players
{
	private List<PokerPlayerRedux> playersInConvo; //a list of the players that are part of the conversation
	private List<PlayerLine> playerLines; // a list of the player lines in the convo
	private int cluster; //the narrative cluster number of the conversation
}


public class PlayerLine //class for each player line
{
	public string mainText; //the text the character is speaking
	public string playerName; //the name of the charater speaking
	public string audioFileName; //the name of the associated audiofile

	public PlayerLine(string _mainText, string _playerName, string _audioFileName) //constructor for player line
	{
		_mainText = mainText;
		_playerName = playerName;
		_audioFileName = audioFileName;
	}
}