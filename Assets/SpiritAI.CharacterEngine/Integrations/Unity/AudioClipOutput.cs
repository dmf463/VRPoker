/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using SpiritAI.CharacterEngine.Audio;
using UnityEngine;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    public class AudioClipOutput : IAudioOutput
    {
        public AudioClipOutput(AudioSource source, AudioClip clip)
        {
            Source = source;
            Output = clip;
        }

        public AudioSource Source { get; }
        public AudioClip Output { get; }
        public void Play()
        {
            Source.clip = Output;
            Source.Play();
        }
    }
}
