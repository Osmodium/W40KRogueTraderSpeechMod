using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using PlaybackState = NAudio.Wave.PlaybackState;
using WebSocket = WebSocketSharp.WebSocket;

namespace SpeechMod.Unity;

public class EdgeVoiceUnity : MonoBehaviour
{
    public static void Speak(string text)
    {
        EdgeVoice.Instance?.Speak(text);
    }

    public static void Stop()
    {
        EdgeVoice.Instance?.Stop();
    }

    public static string GetStatusMessage()
    {
        return EdgeVoice.Instance?.currentState.ToString();
    }

    public static bool IsSpeaking()
    {
        return EdgeVoice.Instance?.IsSpeaking() ?? false;
    }

    public static string[] GetAvailableVoices()
    {
        return EdgeVoice.Instance?.GetAvailableVoices().Result;
    }
}

public class EdgeVoice
{
    const string token = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";

    private static EdgeVoice _instance;
    public State currentState = State.Idle;
    private MemoryStream voiceStream = new();
    //private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private WaveOutEvent outputDevice;
    private StreamMediaFoundationReader audio;
    //private BufferedWaveProvider bufferedWaveProvider;
    //private long lastReadPosition = 0;

    //private Stopwatch stopwatch = new Stopwatch();

    public enum State
    {
        Idle,
        Start,
        End
    }

    public static EdgeVoice Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EdgeVoice();

            return _instance;
        }
    }

    private EdgeVoice()
    { }

    public async Task<string[]> GetAvailableVoices()
    {
        var voicesClient = new HttpClient
        {
            BaseAddress = new Uri("https://speech.platform.bing.com/consumer/speech/synthesize/readaloud/voices/")
        };

        var voicesListResponse = await voicesClient.GetAsync($"list?trustedclienttoken={token}").ConfigureAwait(false);
        if (voicesListResponse?.Content == null)
            return null;

        var voicesListResponseString = await voicesListResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (voicesListResponseString == null)
            return null;

        Main.AvailableVoices = JsonConvert.DeserializeObject<List<Voice.Edge.Voice>>(voicesListResponseString)?.ToArray();

        return Main.AvailableVoices.Select(x => x.ShortName).ToArray();
    }

    private async Task AddAudioAsync(byte[] audioData)
    {
        await voiceStream?.WriteAsync(audioData, 0, audioData.Length);
    }

    private void Play()
    {
        if (outputDevice is { PlaybackState: PlaybackState.Playing })
            outputDevice.Stop();

        audio = new StreamMediaFoundationReader(voiceStream);
        outputDevice = new WaveOutEvent();

        outputDevice.Init(audio);
        outputDevice.Play();
    }

    public bool IsSpeaking()
    {
        return outputDevice is { PlaybackState: PlaybackState.Playing };
    }

    public void Stop()
    {
        if (outputDevice is { PlaybackState: PlaybackState.Playing })
            outputDevice.Stop();

        outputDevice?.Dispose();
        audio?.Dispose();
        voiceStream?.Dispose();
        voiceStream = new MemoryStream();
    }

    public void Speak(string text, int pitch = 0, int rate = 0, int volume = 0)
    {
        var connect_id = Guid.NewGuid().ToString().Replace("-", "");
        var date = DateTime.UtcNow.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (Coordinated Universal Time)'");

        using var ws = new WebSocket($"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken={token}&ConnectionId={connect_id}");
        //ws.Compression = CompressionMethod.Deflate;
        //ws.Origin = "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold";

        ws.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        ws.OnClose += (sender, e) =>
        {
            //e.Dump();
        };

        ws.OnError += (sender, e) =>
        {
            //e.Dump();
        };

        ws.OnMessage += async (sender, e) =>
        {
            //e.Dump();
            if (e.IsText)
            {
                var info = ParseInput(e.Data);
                if (info.Path.Equals("turn.start", StringComparison.InvariantCultureIgnoreCase) && currentState == State.Idle)
                {
                    Stop();
                    currentState = State.Start;
                }
                else if (info.Path.Equals("turn.end", StringComparison.InvariantCultureIgnoreCase) && currentState == State.Start)
                {
                    //await semaphore.WaitAsync();
                    Play();
                    // semaphore.Release();

                    currentState = State.End;
                }
                else if (info.Path.Equals("audio.metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    //info.Dump();

                }
                else
                {
                    //info.Dump();
                }

            }

            if (e.IsBinary)
            {
                if (currentState == State.Idle)
                {
                    //"ERROR!".Dump();
                    return;
                }

                if (e.RawData.Length < 2)
                {
                    //"LENGTH TOO SHORT!".Dump();
                    return;
                }

                ushort headerLength = (ushort)((e.RawData[0] << 8) | e.RawData[1]);
                if (e.RawData.Length < headerLength + 2)
                {
                    //"We received a binary message, but it is missing the audio data.".Dump();
                    return;
                }

                //$"Data received {voiceStream.Length}".Dump();

                byte[] soundData = e.RawData.Skip(headerLength + 2).ToArray();

                await AddAudioAsync(soundData);

                //await semaphore.WaitAsync();
                //if (voiceStream != null && voiceStream.Length > 0 && (outputDevice == null || outputDevice.PlaybackState != PlaybackState.Playing))
                //if (outputDevice == null || outputDevice.PlaybackState != PlaybackState.Playing)
                //{
                //    Play();
                //}
                //else
                //semaphore.Release();
            }

            if (e.IsPing)
            {
                //"PING!".Dump();
                return;
            }
        };

        ws.ConnectAsync();

        ws.OnOpen += (sender, e) =>
        {
            //sender.Dump();
            //e.Dump();

            var setupRequestString = $"X-Timestamp:{date}\r\n" +
                                     "Content-Type:application/json; charset=utf-8\r\n" +
                                     "Path:speech.config\r\n\r\n" +
                                     "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{" +
                                     "\"sentenceBoundaryEnabled\":false,\"wordBoundaryEnabled\":true}," +
                                     "\"outputFormat\":\"audio-24khz-48kbitrate-mono-mp3\"" +
                                     "}}}}\r\n";

            //setupRequestString.Dump();

            ws.SendAsync(setupRequestString, (bool success) =>
            {
                //success.Dump();
                if (success)
                {
                    var voice = Main.Settings.NarratorVoice;
                    Main.Logger.Log(voice);
                    string ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'><voice name='{voice}'><prosody pitch='{FormatValue(pitch)}Hz' rate='{FormatValue(rate)}%' volume='{FormatValue(volume)}%'>{text}</prosody></voice></speak>";
                    Main.Logger.Log(ssml);
                    var s1 = $"X-RequestId:{connect_id}\r\n" +
                             "Content-Type:application/ssml+xml\r\n" +
                             $"X-Timestamp:{date}Z\r\n" +
                             "Path:ssml\r\n\r\n" +
                             $"{ssml}";
                    ws.Send(s1);
                }
            });
        };

        while (currentState != State.End)
        {
        }
        ws.Close();
        currentState = State.Idle;
    }

    private string FormatValue(int value)
    {
        return (value >= 0 ? "+" : "") + value;
    }

    private RequestInfo ParseInput(string input)
    {
        var result = new RequestInfo();

        // Split input by lines
        string[] lines = input.Split('\n');

        foreach (string line in lines)
        {
            // Process each line to extract information
            if (line.StartsWith("X-RequestId:"))
            {
                result.RequestId = line.Substring("X-RequestId:".Length).Trim();
            }
            else if (line.StartsWith("Content-Type:"))
            {
                result.ContentType = line.Substring("Content-Type:".Length).Trim();
            }
            else if (line.StartsWith("Path:"))
            {
                result.Path = line.Substring("Path:".Length).Trim();
            }
            else if (line.Trim() == "{")
            {
                // Start of JSON block, extract the data
                int jsonDataStartIndex = input.IndexOf("{", StringComparison.Ordinal);
                result.Data = input.Substring(jsonDataStartIndex);
                break; // Stop processing lines once data is found
            }
        }

        return result;
    }
}

public class RequestInfo
{
    public string RequestId { get; set; }
    public string ContentType { get; set; }
    public string Path { get; set; }
    public string Data { get; set; }
}
