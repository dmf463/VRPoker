/*
 * Copyright (C) 2017 Spirit AI Limited. All Rights Reserved.
 * Unauthorized copying of this file, via any medium is strictly prohibited.
 * Proprietary and confidential.
 */

using System;
using SpiritAI.CharacterEngine.Audio;
using UnityEngine;

namespace SpiritAI.CharacterEngine.Integrations.Unity
{
    public class MicrophoneInputProvider : IAudioInputProvider
    {
        public MicrophoneInputProvider(int lengthSeconds = 5, bool loop = true, int sampleRate = 16000, string deviceName = null)
        {
            LengthSeconds = lengthSeconds;
            Loop = loop;
            Definition = new AudioDefinition(AudioDefinition.FormatType.Signed16Bit, sampleRate);
            _deviceName = deviceName;
        }

        public AudioDefinition Definition { get; }

        public bool Loop { get; set; }
        public int LengthSeconds { get; set; }
        public AudioClip CurrentAudioClip { get; private set; }

        private readonly string _deviceName;
        private bool _isRecording;
        private int _lastSamplePosition;
        private int _lastCurrentPosition;

        private float[] _sampleBuffer;

        public void Start()
        {
            // TODO(Alan): Support named microphones
            // TODO(Alan): Handle low frequency microphones
            // TODO(Alan): Handle Endianness
            Stop();

            CurrentAudioClip = Microphone.Start(_deviceName, Loop, LengthSeconds, Definition.SampleRate);
            _isRecording = true;
            _lastSamplePosition = 0;
        }

        public void Stop()
        {
            _isRecording = false;
            if (IsActive)
            {
                _lastCurrentPosition = Microphone.GetPosition(_deviceName);
                Microphone.End(_deviceName);
            }
        }

        public bool IsActive
        {
            get { return Microphone.IsRecording(_deviceName); }
        }

        public bool IsLoopingBuffer
        {
            get { return Loop; }
        }

        public AudioBuffer? GetBuffer(int maxBufferSize, bool fillMaxBufferUnlessDone)
        {
            if (CurrentAudioClip == null) return null;

            // NOTE(Alan): Get current position and see how many pending samples we have left
            var currentPosition = Microphone.IsRecording(_deviceName) ? Microphone.GetPosition(_deviceName) : _lastCurrentPosition;
            if (Loop && currentPosition < _lastSamplePosition) _lastSamplePosition = 0;
            var pendingSamples = currentPosition - _lastSamplePosition;

            // NOTE(Alan): If we're done recording and have nothing left, notify
            if (_isRecording == false && pendingSamples <= 0) return AudioBuffer.EmptyFinishedBuffer;
            if (pendingSamples <= 0) return AudioBuffer.EmptyUnfinishedBuffer;

            var bytesPerSample = 1;
            switch (Definition.Format)
            {
                case AudioDefinition.FormatType.Unknown:
                    return AudioBuffer.EmptyFinishedBuffer;
                case AudioDefinition.FormatType.Signed16Bit:
                    bytesPerSample = 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var maxBytes = maxBufferSize;
            var maxSamples = maxBytes/bytesPerSample;
            var samples = Math.Min(maxSamples, pendingSamples);
            var bytesOut = samples*bytesPerSample;

            // NOTE(Alan): Only copy to buffer if we're done, or we've hit max samples and we requested fill output
            if (fillMaxBufferUnlessDone && _isRecording)
            {
                if (samples < maxSamples) return AudioBuffer.EmptyUnfinishedBuffer;
                _lastCurrentPosition = currentPosition;
            }

            // NOTE(Alan): Create or resize our sample buffer if needed
            if (_sampleBuffer == null || _sampleBuffer.Length < maxSamples)
            {
                _sampleBuffer = new float[maxSamples];
            }

            CurrentAudioClip.GetData(_sampleBuffer, _lastSamplePosition);
            _lastSamplePosition += samples;

            var buffer = AudioBuffer.Create(bytesOut, false, Definition);
            switch (Definition.Format)
            {
                case AudioDefinition.FormatType.Signed16Bit:
                    for (var sampleIndex = 0; sampleIndex < samples; ++sampleIndex)
                    {
                        var data = (short)(_sampleBuffer[sampleIndex] * 32767);
                        BitConverter.GetBytes(data).CopyTo(buffer.Data, sampleIndex*2);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return buffer;
        }
    }
}
