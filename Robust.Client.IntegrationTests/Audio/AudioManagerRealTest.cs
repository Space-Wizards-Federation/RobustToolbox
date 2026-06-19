using NUnit.Framework;
using Robust.Client.Audio;
using Robust.Shared;
using Robust.Shared.IoC;
using Robust.UnitTesting;

namespace Robust.Client.IntegrationTests.Audio;

[Explicit("Uses the real AudioManager for audio tests")]
public sealed class AudioManagerRealTest : RobustIntegrationTest
{
    // Skip it by default because the runner may not have any audio means.
    [Test]
    public async Task SwitchAudioDevice()
    {
        var client = StartClient(new ClientIntegrationOptions
        {
            Pool = false,
            InitIoC = () =>
            {
                IoCManager.Register<IAudioManager, AudioManager>(overwrite: true);
                IoCManager.Register<IAudioInternal, AudioManager>(overwrite: true);
            },
        });

        await client.WaitIdleAsync();

        var audio = client.ResolveDependency<IAudioManager>();
        Assert.That(audio, Is.TypeOf<AudioManager>());

        var defaultDevice = audio.GetDefaultAudioDevice();
        Assert.That(defaultDevice, Is.Not.Null, "OpenAL did not expose a default audio output device.");

        var devices = audio.GetAudioDevices();
        var testDevice = devices.FirstOrDefault(device => device != defaultDevice) ?? defaultDevice!;

        await client.WaitAssertion(() =>
        {
            client.CfgMan.SetCVar(CVars.AudioDevice, testDevice);
            Assert.That(client.CfgMan.GetCVar(CVars.AudioDevice), Is.EqualTo(testDevice));

            client.CfgMan.SetCVar(CVars.AudioDevice, string.Empty);
            Assert.That(client.CfgMan.GetCVar(CVars.AudioDevice), Is.Empty);
        });
    }
}
