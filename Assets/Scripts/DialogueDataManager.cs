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


	public void ParseDialogueFile(TextAsset dialogueFile)
	{
		dialogueDict = new Dictionary<string[], List<Dialogue>>();
		string fileFullString = dialogueFile.text;
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