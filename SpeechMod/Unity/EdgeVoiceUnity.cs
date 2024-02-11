using Newtonsoft.Json;
using SpeechMod.Voice.Edge;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeechMod.Unity;

public class EdgeVoiceUnity : MonoBehaviour
{
    private const string TOKEN = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";
    private int _playIndex;
    private List<Task<EdgeVoiceClient>> _voiceTasks = new();
    private static EdgeVoiceUnity s_Instance;

    private EdgeVoiceUnity()
    {
    }

    public void Start()
    {
        if (s_Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            s_Instance = this;
        }
    }

    public static string[] SetAvailableVoices()
    {
        return SetAvailableVoicesInternal().Result;
    }

    public static bool IsSpeaking()
    {
        if (s_Instance?._voiceTasks == null)
            return false;

        if (s_Instance._playIndex < 0 || s_Instance._playIndex >= s_Instance._voiceTasks.Count)
        {
            s_Instance._playIndex = 0;
            return false;
        }

        var edgeVoiceClient = s_Instance._voiceTasks[s_Instance._playIndex]?.Result;
        return edgeVoiceClient != null &&
               s_Instance._voiceTasks.Count > 0 &&
               s_Instance._playIndex < s_Instance._voiceTasks.Count - 1 &&
               s_Instance._voiceTasks[s_Instance._playIndex] != null &&
               s_Instance._voiceTasks[s_Instance._playIndex].Status == TaskStatus.RanToCompletion &&
               edgeVoiceClient.IsSpeaking;
    }

    public static string GetStatusMessage()
    {
        if (s_Instance?._voiceTasks == null)
            return "Idle";

        if (s_Instance._playIndex < 0 || s_Instance._playIndex >= s_Instance._voiceTasks.Count)
        {
            s_Instance._playIndex = 0;
            return "Idle";
        }

        var edgeVoiceClient = s_Instance._voiceTasks[s_Instance._playIndex]?.Result;
        if (edgeVoiceClient != null)
            return edgeVoiceClient.CurrentState.ToString();

        return "Error";
    }

    public static void Speak(EdgeVoiceDto edgeVoiceDTO)
    {
        var cancellationToken = new CancellationToken();
        SpeakInternal(edgeVoiceDTO, cancellationToken);
    }

    public static void SpeakMulti(EdgeVoiceDto[] edgeVoiceDTOs)
    {
        var cancellationToken = new CancellationToken();
        SpeakMultiInternal(edgeVoiceDTOs, cancellationToken);
    }

    public static void Stop()
    {
        if (!IsSpeaking())
            return;

        var edgeVoiceClient = s_Instance?._voiceTasks?[s_Instance._playIndex]?.Result;
        edgeVoiceClient?.Stop();
        s_Instance._playIndex = 0;
    }

    void OnDestroy()
    {
        if (s_Instance != this)
            return;

        Stop();

        s_Instance = null;
    }

    private static async Task<string[]> SetAvailableVoicesInternal()
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

    private static async Task SpeakInternal(EdgeVoiceDto edgeVoiceDTO, CancellationToken cancellationToken)
    {
        Debug.Log($"SpeakInternal: '{edgeVoiceDTO.Voice}' - R: {edgeVoiceDTO.Rate}, P: {edgeVoiceDTO.Pitch}, V: {edgeVoiceDTO.Volume} ");

        Stop();
        Reset();

        s_Instance._voiceTasks.Add(new Task<EdgeVoiceClient>(() =>
        {
            var edgeVoice = new EdgeVoiceClient();
            edgeVoice.Load(edgeVoiceDTO, TOKEN);
            return edgeVoice;
        }));

        var single = s_Instance._voiceTasks.First();

        single.Start();
        await single;

        if (cancellationToken.IsCancellationRequested)
            return;

        Debug.Log("Play");
        single.Result?.Play();
    }

    private static async Task SpeakMultiInternal(EdgeVoiceDto[] edgeVoiceDTOs, CancellationToken cancellationToken)
    {
        foreach (var edgeVoiceDto in edgeVoiceDTOs)
        {
            Debug.Log($"Speak: '{edgeVoiceDto.Text}'  '{edgeVoiceDto.Voice}'");
        }

        Stop();
        Reset();

        foreach (var edgeVoiceDTO in edgeVoiceDTOs)
        {
            if (string.IsNullOrWhiteSpace(edgeVoiceDTO.Text))
                continue;

            s_Instance._voiceTasks?.Add(new Task<EdgeVoiceClient>(() =>
            {
                var edgeVoice = new EdgeVoiceClient();
                edgeVoice.Load(edgeVoiceDTO, TOKEN);
                return edgeVoice;
            }));
        }

        foreach (var task in s_Instance._voiceTasks)
        {
            task.Start();
        }

        await s_Instance._voiceTasks.First();

        if (cancellationToken.IsCancellationRequested)
            return;

        var voiceClient = s_Instance._voiceTasks[s_Instance._playIndex]?.Result;
        if (voiceClient != null)
        {
            Debug.Log("Play first");
            voiceClient.Play();

            Debug.Log("Starting Coroutine");
            s_Instance.StartCoroutine(PlayMultipleCoroutine(cancellationToken));
        }
    }

    private static IEnumerator PlayMultipleCoroutine(CancellationToken cancellationToken)
    {
        Debug.Log("Play Multiple Coroutine");
        while (s_Instance._playIndex < s_Instance._voiceTasks.Count)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var edgeVoiceClient = s_Instance._voiceTasks[s_Instance._playIndex].Result;
            if (edgeVoiceClient is { IsSpeaking: true })
                yield return new WaitForSeconds(0.1f);
            else
            {
                if (++s_Instance._playIndex >= s_Instance._voiceTasks.Count)
                    break;
                Debug.Log("Play next");
                s_Instance._voiceTasks[s_Instance._playIndex]?.Result?.Play();
            }
        }
    }

    private static void Reset()
    {
        s_Instance?.StopAllCoroutines();
        s_Instance._playIndex = 0;
        s_Instance._voiceTasks = new List<Task<EdgeVoiceClient>>();
    }
}