using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using SpiritAI.CharacterEngine;
using SpiritAI.CharacterEngine.Integrations.Unity;
using SpiritAI.CharacterEngine.Logging;
using SpiritAI.CharacterEngine.MachineLearning;
using SpiritAI.CharacterEngine.Messages;

public class SpiritAIManager : MonoBehaviour {

    CharacterEngine characterEngine;

	// Use this for initialization
	void Start () {
        StartCoroutine(SetupCharacterEngine());
    }

    // Update is called once per frame
    void Update () {
		
	}

    IEnumerator SetupCharacterEngine()
    {
        CharacterEngineUnity.Init();
        CEDebug.EscalateLogLevel(LogLevel.High);
        CharacterEngine.SetDataDirectory(Application.streamingAssetsPath);
        CharacterEngine.SetLogDirectory(Application.persistentDataPath);
        CharacterEngine.SetStoreDirectory(Application.persistentDataPath);
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        var  platform =  Platform.Windows;
        #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        var  platform  =  Platform.MacOSX;
        #else
                throw new InvalidOperationException("Cannot  determine  host  enviroment");
        #endif
        #if UNITY_64 || UNITY_EDITOR_64
        var  architecture  =  Architecture.X64;
        #else
        var architecture = Architecture.X86;
        #endif
               CharacterEngine characterEngine = CharacterEngine.Create(platform, architecture, gameObject.AddMissingComponent<TickManager>(), new NetworkState());
               //characterEngine.SetProjectIdentifier("TheDealer");
        // Test that the new CharacterEngine initialized OK
        yield return characterEngine.TestConnection().YieldUntilDone();
        // Wait for it to finish loading all classifier and other data
        yield return CharacterEngineUnity.WaitForLoad(characterEngine);
        characterEngine.StartScene();
        //characterEngine.AddHandler (OutputHandler.Response, OnCharacterEngineResponse);
        //characterEngine.AddHandler (OutputHandler.EngineMessage, OnCharacterEngineResponse);

    }
}
