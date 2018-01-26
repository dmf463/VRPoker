using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DialogueDataManager
{
	private Dictionary<string[], List<Dialogue>> dialogueDict;



	public void Awake()
	{
		ParseDialogueFile(Services.SoundManager.dialogueFile);
	}

	private class ListComparer<T> : IEqualityComparer<List<T>>{ //generic for comparing lists
		public bool Equals(List<T> x, List<T> y){ //bool for comparing list x to list y
			return x.SequenceEqual (y); //return whether the lists are equal
		}

		public int GetHashCode(List<T> obj){
			int hashcode = 0;
			foreach (T t in obj) {
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
		dialogueDict = new Dictionary<string[], List<Dialogue>>(); //our dictionary of dialogue 
		string fileFullString = dialogueFile.text;
		string[] fileLines; 
		string[] lineEntries;
		string fileLine;
		string[] lineSeparator = new string[] { "\r\n", "\r", "\n"};  
		char[] entrySeparator = new char[] { '\t' };
		fileLines = fileFullString.Split (lineSeparator, System.StringSplitOptions.None);

		for (int i = 1; i < fileLines.Length; i++) 
		{
			fileLine = fileLines [i];
			lineEntries = fileLine.Split (entrySeparator);
		}
	}

}

public class Dialogue //class for each piece of dialogue
{
	public string mainText; //the text the character is speaking
	public string playerName; //the name of the charater speaking
	public string audioFileName; //the name of the associated audiofile

	public Dialogue(string _mainText, string _playerName, string _audioFileName) //constructor for dialogue
	{
		_mainText = mainText;
		_playerName = playerName;
		_audioFileName = audioFileName;
	}
}