using System;
using System.IO;
using Robust.Shared.Audio.Sources;

namespace Robust.Client.Audio;

/// <summary>
/// Public audio API for stuff that can't go through <see cref="AudioSystem"/>
/// </summary>
[NotContentImplementable]
public interface IAudioManager
{
    /// <summary>
    /// OpenAL master gain for audio.
    /// </summary>
    float MasterGain { get; }

    /// <summary>
    /// OpenAL Doppler factor for audio.
    /// </summary>
    float DopplerFactor { get; }

    IAudioSource? CreateAudioSource(AudioStream stream);

    AudioStream LoadAudioOggVorbis(Stream stream, string? name = null);

    AudioStream LoadAudioWav(Stream stream, string? name = null);

    AudioStream LoadAudioRaw(ReadOnlySpan<short> samples, int channels, int sampleRate, string? name = null);

    void SetMasterGain(float gain);
}
