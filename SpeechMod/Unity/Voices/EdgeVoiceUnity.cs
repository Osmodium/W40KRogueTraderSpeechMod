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

namespace SpeechMod.Unity.Voices;

public class EdgeVoiceUnity : MonoBehaviour
{
    private const string TOKEN = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";
    private static EdgeVoiceUnity s_Instance;
    public int PlayIndex;
    public List<Task<EdgeVoiceClient>> VoiceTasks = new();
    public CancellationToken CancellationToken;

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
        if (s_Instance?.VoiceTasks == null || s_Instance.VoiceTasks.Count == 0)
            return false;

        if (s_Instance.PlayIndex < 0 || s_Instance.PlayIndex >= s_Instance.VoiceTasks.Count)
            return false;

        if (s_Instance.PlayIndex + 1 < s_Instance.VoiceTasks.Count)
            return true;

        var edgeVoiceClient = s_Instance.VoiceTasks[s_Instance.PlayIndex]?.Result;
        return edgeVoiceClient is { IsSpeaking: true };
    }

    public static string GetStatusMessage()
    {
        if (s_Instance?.VoiceTasks == null)
            return EdgeVoiceClient.EdgeVoiceClientState.Ready.ToString();

        if (s_Instance.PlayIndex < 0 || s_Instance.PlayIndex >= s_Instance.VoiceTasks.Count)
            return EdgeVoiceClient.EdgeVoiceClientState.Ready.ToString();

        var edgeVoiceClient = s_Instance.VoiceTasks[s_Instance.PlayIndex]?.Result;
        if (edgeVoiceClient != null)
            return edgeVoiceClient.CurrentState.ToString();

        return EdgeVoiceClient.EdgeVoiceClientState.Error.ToString();
    }

    public static void Speak(EdgeVoiceDto edgeVoiceDTO)
    {
        s_Instance.CancellationToken = new CancellationToken();
        SpeakInternal(edgeVoiceDTO);
    }

    public static void SpeakMulti(EdgeVoiceDto[] edgeVoiceDTOs)
    {
        s_Instance.CancellationToken = new CancellationToken();
        SpeakMultiInternal(edgeVoiceDTOs);
    }

    public static void Stop()
    {
        if (!IsSpeaking())
            return;

        s_Instance?.StopCoroutine(PlayMultipleCoroutine());

        var edgeVoiceClient = s_Instance?.VoiceTasks?[s_Instance.PlayIndex]?.Result;
        edgeVoiceClient?.Stop();
        s_Instance.PlayIndex = 0;
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

    private static async Task SpeakInternal(EdgeVoiceDto edgeVoiceDTO)
    {
#if DEBUG
        Debug.Log($"SpeakInternal: {edgeVoiceDTO.Text}");
#endif

        Stop();
        Reset();

        s_Instance.VoiceTasks.Add(new Task<EdgeVoiceClient>(() =>
        {
            var edgeVoice = new EdgeVoiceClient();
            edgeVoice.Load(edgeVoiceDTO, TOKEN);
            return edgeVoice;
        }));

        var single = s_Instance.VoiceTasks.First();

        //s_Instance.StartCoroutine(SpeakingCoroutine());

        single.Start();
        await single;

        if (s_Instance.CancellationToken.IsCancellationRequested)
            return;

        single.Result?.Play();
    }

    private static async Task SpeakMultiInternal(EdgeVoiceDto[] edgeVoiceDTOs)
    {
#if DEBUG
        Debug.Log($"SpeakMultiInternal: {edgeVoiceDTOs.Length}");
#endif
        Stop();
        Reset();

        foreach (var edgeVoiceDTO in edgeVoiceDTOs)
        {
            if (string.IsNullOrWhiteSpace(edgeVoiceDTO.Text))
                continue;

            s_Instance.VoiceTasks?.Add(new Task<EdgeVoiceClient>(() =>
            {
                var edgeVoice = new EdgeVoiceClient();
                edgeVoice.Load(edgeVoiceDTO, TOKEN);
                return edgeVoice;
            }));
        }

        //s_Instance.StartCoroutine(SpeakingCoroutine());

        foreach (var task in s_Instance.VoiceTasks)
        {
            task.Start();
        }

        await s_Instance.VoiceTasks.First();

        if (s_Instance.CancellationToken.IsCancellationRequested)
            return;

        var voiceClient = s_Instance.VoiceTasks[s_Instance.PlayIndex]?.Result;
        if (voiceClient != null)
        {
            voiceClient.Play();

            s_Instance.StartCoroutine(PlayMultipleCoroutine());
        }
    }

    private static IEnumerator PlayMultipleCoroutine()
    {
        while (s_Instance.PlayIndex < s_Instance.VoiceTasks.Count)
        {
            if (s_Instance.CancellationToken.IsCancellationRequested)
                break;

            var edgeVoiceClient = s_Instance.VoiceTasks[s_Instance.PlayIndex].Result;
            if (edgeVoiceClient is { IsSpeaking: true })
                yield return new WaitForSeconds(0.1f);
            else
            {
                if (++s_Instance.PlayIndex >= s_Instance.VoiceTasks.Count)
                    break;

                s_Instance.VoiceTasks[s_Instance.PlayIndex]?.Result?.Play();
            }
        }
    }

    private static IEnumerator SpeakingCoroutine()
    {
        while (IsSpeaking())
        {
            yield return new WaitForEndOfFrame();
        }
    }

    private static void Reset()
    {
        s_Instance?.StopCoroutine(PlayMultipleCoroutine());
        s_Instance.PlayIndex = 0;
        s_Instance.VoiceTasks = new List<Task<EdgeVoiceClient>>();
    }
}