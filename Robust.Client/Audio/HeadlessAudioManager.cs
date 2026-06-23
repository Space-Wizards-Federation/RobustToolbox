using System;
using System.IO;
using System.Numerics;
using Robust.Shared;
using Robust.Shared.Audio;
using Robust.Shared.Audio.AudioLoading;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;
using Robust.Shared.Audio.Sources;
using Robust.Shared.Maths;

namespace Robust.Client.Audio;

/// <summary>
/// Headless client audio.
/// </summary>
internal sealed partial class HeadlessAudioManager : IAudioInternal
{
    [Dependency] private IConfigurationManager _cfg = default!;

    private int _audioBuffer;
    private float _masterGain;
    private float _dopplerFactor = 1f;

    /// <inheritdoc />
    public float MasterGain => _masterGain;

    /// <inheritdoc />
    public float DopplerFactor
    {
        get => _dopplerFactor;
        set => SetDopplerFactor(value, true);
    }

    /// <inheritdoc />
    public void InitializePostWindowing()
    {
        _cfg.OnValueChanged(CVars.AudioDopplerFactor, SetDopplerFactorFromCVar, true);
    }

    private void SetDopplerFactorFromCVar(float value)
    {
        SetDopplerFactor(value, false);
    }

    private void SetDopplerFactor(float value, bool updateCVar)
    {
        var original = _dopplerFactor;

        if (!float.IsFinite(value))
            value = 0f;

        value = MathF.Max(value, 0f);

        if (MathHelper.CloseTo(original, value))
            return;

        if (updateCVar)
            _cfg.SetCVar(CVars.AudioDopplerFactor, value, true);

        _dopplerFactor = value;
    }

    /// <inheritdoc />
    public void Shutdown()
    {
    }

    /// <inheritdoc />
    public void FlushALDisposeQueues()
    {
    }

    /// <inheritdoc />
    public IAudioSource CreateAudioSource(AudioStream stream)
    {
        return DummyAudioSource.Instance;
    }

    /// <inheritdoc />
    public IBufferedAudioSource? CreateBufferedAudioSource(int buffers, bool floatAudio = false)
    {
        return DummyBufferedAudioSource.Instance;
    }

    /// <inheritdoc />
    public void SetVelocity(Vector2 velocity)
    {
    }

    /// <inheritdoc />
    public void SetPosition(Vector2 position)
    {
    }

    /// <inheritdoc />
    public void SetRotation(Angle angle)
    {
    }

    /// <inheritdoc />
    public void SetMasterGain(float newGain)
    {
        if (!float.IsFinite(newGain))
            newGain = 0f;

        _masterGain = MathF.Max(newGain, 0f);
    }

    /// <inheritdoc />
    public void SetAttenuation(Attenuation attenuation)
    {
    }

    /// <inheritdoc />
    public void Remove(AudioStream stream)
    {
    }

    /// <inheritdoc />
    public void StopAllAudio()
    {
    }

    /// <inheritdoc />
    public void SetZOffset(float f)
    {
    }

    /// <inheritdoc />
    public void _checkAlError(string callerMember = "", int callerLineNumber = -1)
    {
    }

    /// <inheritdoc />
    public float GetAttenuationGain(float distance, float rolloffFactor, float referenceDistance, float maxDistance)
    {
        return 0f;
    }

    public AudioStream LoadAudioOggVorbis(Stream stream, string? name = null)
    {
        var metadata = AudioLoaderOgg.LoadAudioMetadata(stream);
        return AudioStreamFromMetadata(metadata, name);
    }

    public AudioStream LoadAudioWav(Stream stream, string? name = null)
    {
        var metadata = AudioLoaderWav.LoadAudioMetadata(stream);
        return AudioStreamFromMetadata(metadata, name);
    }

    public AudioStream LoadAudioRaw(ReadOnlySpan<short> samples, int channels, int sampleRate, string? name = null)
    {
        var length = TimeSpan.FromSeconds((double) samples.Length / channels / sampleRate);
        return new AudioStream(this, _audioBuffer++, null, length, channels, name);
    }

    private AudioStream AudioStreamFromMetadata(AudioMetadata metadata, string? name)
    {
        return new AudioStream(this, _audioBuffer++, null, metadata.Length, metadata.ChannelCount, name, metadata.Title, metadata.Artist);
    }
}
