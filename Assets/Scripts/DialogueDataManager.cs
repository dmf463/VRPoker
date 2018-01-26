using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DialogueDataManager
{
	public void Awake()
	{
		ParseDialogueFile();
	}


	public void ParseDialogueFile(TextAsset dialogueFile)
	{
		
	}

	 


}

public class Dialogue
{
	public string mainText;
	public string playerName;
	public string audioFileName;

	public Dialogue(string _mainText, string _playerName, string _audioFileName)
	{
		_mainText = mainText;
		_playerName = playerName;
		_audioFileName = audioFileName;
	}
}