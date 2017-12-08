/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using System;
using SpiritAI.CharacterEngine.Audio;
using SpiritAI.CharacterEngine.Extensions;
using UnityEngine;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    /**
     * A Unity implementation of IAudioOutputProvider that supports AudioClipOutput.
     */
    public class AudioOutputProvider : IAudioOutputProvider<AudioClipOutput>
    {
        /**
         * The name to assign to the AudioClip.
         */
        public string ClipName { get; }

        /**
         * @param source The AudioSource to use an output vector.
         * @param clipName The name to assign to the AudioClip once created.
         */
        public AudioOutputProvider(AudioSource source, string clipName = "AudioClip")
        {
            Source = source;
            ClipName = clipName;
        }

        /**
         * The AudioSource this is attached to.
         */
        public AudioSource Source { get; }

        /**
         * A handle to notify once the AudioClipOutput is ready.
         * @param AudioClipOutput The AudioClipOutput that has been prepared for playback.
         */
        public Action<AudioClipOutput> OnOutputReady { get; set; }

        public void PrepareOutput(AudioBuffer? bufferObj)
        {
            if (bufferObj == null) return;
            var buffer = bufferObj.Value;
            AudioClip audioClip = null;
            switch (buffer.Definition.Format)
            {
                case AudioDefinition.FormatType.Wav:
                    // NOTE(Alan): This only supports WAV currently
                    var wav = new WWUtils.Audio.WAV(buffer.Data);
                    audioClip = AudioClip.Create(ClipName, wav.SampleCount, 1, wav.Frequency, false);
                    audioClip.SetData(wav.LeftChannel, 0);
                    break;
                case AudioDefinition.FormatType.PcmShort:
                    var pcmFloats = buffer.FromPcmShortToFloat();
                    audioClip = AudioClip.Create(ClipName, pcmFloats.Length, 1, buffer.Definition.SampleRate, false);
                    audioClip.SetData(pcmFloats, 0);
                    break;
               default:
                    throw new ArgumentOutOfRangeException();
            }

            var audioClipOutput = new AudioClipOutput(Source, audioClip);

            // NOTE(Alan): Call OnOutputReady if set, otherwise AudioClipOutput::Play

            if (OnOutputReady != null)
            {
                OnOutputReady.Invoke(audioClipOutput);
                return;
            }

            audioClipOutput.Play();
        }
    }
}
