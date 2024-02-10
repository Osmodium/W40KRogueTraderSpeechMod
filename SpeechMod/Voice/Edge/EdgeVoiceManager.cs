using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SpeechMod.Voice.Edge;

public class EdgeVoiceManager : IDisposable
{
    private const string TOKEN = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";
    private int _playIndex;
    private List<Task<EdgeVoiceClient>> _voiceTasks = new();
    private static readonly object SyncRoot = new();

    private static EdgeVoiceManager s_Instance;
    [NotNull]
    public static EdgeVoiceManager Instance
    {
        get
        {
            if (s_Instance != null)
                return s_Instance;

            lock (SyncRoot!)
            {
                s_Instance = new EdgeVoiceManager();
            }
            return s_Instance;
        }
    }

    private EdgeVoiceManager()
    { }

    public string[] SetAvailableVoices()
    {
        return SetAvailableVoicesInternal().Result;
    }

    public bool IsSpeaking()
    {
        if (_voiceTasks == null)
            return false;

        var edgeVoiceClient = _voiceTasks[_playIndex]?.Result;
        return edgeVoiceClient != null &&
               _voiceTasks.Count > 0 &&
               _playIndex < _voiceTasks.Count - 1 &&
               _voiceTasks[_playIndex] != null &&
               _voiceTasks[_playIndex].Status == TaskStatus.RanToCompletion &&
               edgeVoiceClient.IsSpeaking;
    }

    public string GetStatusMessage()
    {
        if (_voiceTasks == null)
            return "Idle";

        var edgeVoiceClient = _voiceTasks[_playIndex]?.Result;
        if (edgeVoiceClient != null)
            return edgeVoiceClient.CurrentState.ToString();

        return "Error";
    }

    public void Speak(EdgeVoiceDto edgeVoiceDTO)
    {
        SpeakInternal(edgeVoiceDTO, CancellationToken.None).ConfigureAwait(false);
    }

    public void SpeakMulti(EdgeVoiceDto[] edgeVoiceDTOs)
    {
        SpeakMultiInternal(edgeVoiceDTOs, CancellationToken.None).ConfigureAwait(false);
    }

    public void Stop()
    {
        if (!IsSpeaking())
            return;

        var edgeVoiceClient = _voiceTasks?[_playIndex]?.Result;
        edgeVoiceClient?.Stop();
        _playIndex = 0;
    }

    public void Dispose()
    {
        if (_voiceTasks == null)
            return;

        foreach (var voices in _voiceTasks)
        {
            if (voices?.Result != null)
            {
                voices.Result.Stop();
                voices.Result.Dispose();
            }
            voices?.Dispose();
        }

        _voiceTasks.Clear();
    }

    private async Task<string[]> SetAvailableVoicesInternal()
    {
        var voicesClient = new HttpClient
        {
            BaseAddress = new Uri("https://speech.platform.bing.com/consumer/speech/synthesize/readaloud/voices/")
        };

        var voicesListResponse = await voicesClient.GetAsync($"list?trustedclienttoken={TOKEN}").ConfigureAwait(false);
        if (voicesListResponse?.Content == null)
            return null;

        var voicesListResponseString = await voicesListResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (voicesListResponseString == null)
            return null;

        Main.EdgeAvailableVoices = JsonConvert.DeserializeObject<List<EdgeVoiceInfo>>(voicesListResponseString)?.ToArray();

        return Main.EdgeAvailableVoices?.Select(x => x.ShortName).ToArray();
    }

    private async Task SpeakInternal(EdgeVoiceDto edgeVoiceDTO, CancellationToken cancellationToken)
    {
        Stop();
        Reset();

        _voiceTasks.Add(new Task<EdgeVoiceClient>(() =>
        {
            var edgeVoice = new EdgeVoiceClient();
            edgeVoice.Load(edgeVoiceDTO, TOKEN);
            return edgeVoice;
        }));

        var single = _voiceTasks.First();

        single.Start();
        await single;

        if (cancellationToken.IsCancellationRequested)
            return;

        single.Result?.Play();
        var edgeVoiceClient = _voiceTasks[_playIndex]!.Result;
        while (edgeVoiceClient is { IsSpeaking: true })
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            Thread.Sleep(10);
        }
    }

    private async Task SpeakMultiInternal(EdgeVoiceDto[] edgeVoiceDTOs, CancellationToken cancellationToken)
    {
        Stop();
        Reset();

        foreach (var edgeVoiceDTO in edgeVoiceDTOs)
        {
            _voiceTasks?.Add(new Task<EdgeVoiceClient>(() =>
            {
                var edgeVoice = new EdgeVoiceClient();
                edgeVoice.Load(edgeVoiceDTO, TOKEN);
                return edgeVoice;
            }));
        }

        foreach (var task in _voiceTasks)
        {
            task.Start();
        }

        await _voiceTasks.First();

        if (cancellationToken.IsCancellationRequested)
            return;

        var voiceClient = _voiceTasks[_playIndex]?.Result;
        if (voiceClient != null)
        {
            voiceClient.Play();
            while (_playIndex < _voiceTasks.Count)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                voiceClient = _voiceTasks[_playIndex].Result;
                if (voiceClient is { IsSpeaking: true })
                    Thread.Sleep(10);
                else
                {
                    if (++_playIndex >= _voiceTasks.Count)
                        break;
                    await _voiceTasks[_playIndex]!;
                    _voiceTasks[_playIndex].Result?.Play();
                }
            }
        }
    }
    private void Reset()
    {
        Stop();
        _playIndex = 0;
        _voiceTasks = new List<Task<EdgeVoiceClient>>();
    }
}