/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using System.Collections;
using SpiritAI.CharacterEngine.Logging;
using SpiritAI.CharacterEngine.WorkQueue;
using UnityEngine;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    public static class CharacterEngineUnity
    {
        public static void Init(LogLevel logLevel = LogLevel.Normal)
        {
            var pluginsPath = "/Plugins/";
            if (Application.isEditor)
            {
                pluginsPath = "/SpiritAI.CharacterEngine" + pluginsPath;
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    if (PlatformInfo.Is64BitProcess) pluginsPath += "x64/";
                    else pluginsPath += "x86/";
                }
            }
            pluginsPath = Application.dataPath + pluginsPath;

            var dllName = pluginsPath + "SpiritAI.Core.dll";
            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                dllName = pluginsPath + "SpiritAI.Core.bundle";
            }
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
            {
                dllName = pluginsPath + "SpiritAI.Core.dll";
            }

            SpiritAI.CharacterEngine.MachineLearning.Settings.DllName = dllName;
            SpiritAI.CharacterEngine.TextGen.Settings.DllName = dllName;

            SpiritAI.CharacterEngine.TextGen.Logging.Init();

            CharacterEngine.SetDataDirectory(Application.streamingAssetsPath);
            CharacterEngine.SetLogDirectory(Application.persistentDataPath);
            CharacterEngine.SetStoreDirectory(Application.persistentDataPath);

            CEDebug.SetLogLevel(logLevel);

            CEDebug.OnLogInfo = (level, s) =>
            {
                UnityEngine.Debug.Log(s);
            };
            CEDebug.OnLogWarning = (level, s) =>
            {
                UnityEngine.Debug.LogWarning(s);
            };
            CEDebug.OnLogError = (level, s) =>
            {
                UnityEngine.Debug.LogError(s);
            };
        }

        public static void SetLogProvider(ILogProvider logProvider)
        {
            CEDebug.SetLogProvider(logProvider);
        }

        public static IEnumerator WaitForLoad(params IBackgroundLoader[] loaders)
        {
            if (loaders == null) yield break;
            
            while (true)
            {
                var allReady = true;
                foreach (var loader in loaders)
                {
                    if (loader.IsReady) continue;
                    allReady = false;
                    break;
                }
                if (allReady) break;
                yield return null;
            }
        }
    }
}
