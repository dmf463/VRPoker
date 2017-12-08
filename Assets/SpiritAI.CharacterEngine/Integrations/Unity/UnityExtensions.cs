/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using System.Collections;
using SpiritAI.CharacterEngine.WorkQueue;
using UnityEngine;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    public static class Extensions
    {
        public static T AddMissingComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
            return component;
        }

        public static IEnumerator YieldUntilDone(this WorkManager.Work item)
        {
            while (true)
            {
                if (item.JobState == WorkManager.JobState.Done) break;
                yield return null;
            }
        }

        public static IEnumerator YieldUntilLoaded(this IBackgroundLoader loader)
        {
            while (true)
            {
                yield return null;
                if (loader.IsReady) break;
            }
        }
    }
}
