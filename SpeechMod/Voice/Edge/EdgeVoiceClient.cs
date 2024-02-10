using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;

namespace SpeechMod.Voice.Edge;

public class EdgeVoiceClient : IDisposable
{
    private EdgeVoiceClientState _currentState = EdgeVoiceClientState.Ready;
    private readonly MemoryStream _voiceStream = new();
    private WaveOutEvent _outputDevice;
    private StreamMediaFoundationReader _audio;

    public enum EdgeVoiceClientState
    {
        Ready,
        Loading,
        Done,
        Playing,
        Paused,
        Stopped,
        Error
    }

    private string FormatValue(int value)
    {
        return (value >= 0 ? "+" : "") + value;
    }

    private async Task AddAudioAsync(byte[] audioData)
    {
        await _voiceStream?.WriteAsync(audioData, 0, audioData.Length);
    }

    public EdgeVoiceClientState CurrentState => _currentState;

    public bool IsSpeaking
    {
        get
        {
            if (_outputDevice != null)
                return _outputDevice.PlaybackState == PlaybackState.Playing;
            return false;
        }
    }

    public void Play()
    {
        if (_outputDevice == null)
        {
            _audio = new StreamMediaFoundationReader(_voiceStream);
            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_audio);
        }
        _currentState = EdgeVoiceClientState.Playing;
        _outputDevice.Play();
    }

    public void Stop()
    {
        if (_outputDevice is not { PlaybackState: PlaybackState.Playing })
            return;

        _outputDevice.Stop();
        _currentState = EdgeVoiceClientState.Stopped;
    }

    public void Pause()
    {
        if (_outputDevice is not { PlaybackState: PlaybackState.Playing })
            return;

        _outputDevice.Pause();
        _currentState = EdgeVoiceClientState.Paused;
    }

    public void Load(EdgeVoiceDto edgeVoiceDTO, string token)
    {
        var connectId = Guid.NewGuid().ToString().Replace("-", "");
        var dateTime = DateTime.UtcNow.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (Coordinated Universal Time)'");

        using var webSocket = new WebSocket($"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken={token}&ConnectionId={connectId}");

        webSocket.OnError += (sender, e) =>
        {
            Debug.LogError(e);
            _currentState = EdgeVoiceClientState.Error;
        };

        webSocket.OnMessage += async (sender, e) =>
        {
            if (e.IsText)
            {
                var info = EdgeVoiceRequestInfo.ParseInput(e.Data);
                if (info.Path.Equals("turn.start", StringComparison.InvariantCultureIgnoreCase) && _currentState == EdgeVoiceClientState.Ready)
                {
                    _currentState = EdgeVoiceClientState.Loading;
                }
                else if (info.Path.Equals("turn.end", StringComparison.InvariantCultureIgnoreCase) && _currentState == EdgeVoiceClientState.Loading)
                {
                    _currentState = EdgeVoiceClientState.Done;
                }
            }

            if (e.IsBinary)
            {
                if (_currentState != EdgeVoiceClientState.Loading)
                {
                    Debug.LogError("Binary received when not loading!");
                    return;
                }

                if (e.RawData is { Length: < 2 })
                {
                    Debug.LogError("Binary data received length is too short!");
                    return;
                }

                if (e.RawData != null)
                {
                    ushort headerLength = (ushort)((e.RawData[0] << 8) | e.RawData[1]);
                    if (e.RawData.Length < headerLength + 2)
                    {
                        Debug.LogError("We received a binary message, but it is missing the audio data.");
                        return;
                    }

                    byte[] soundData = e.RawData.Skip(headerLength + 2).ToArray();
                    await AddAudioAsync(soundData);
                }
            }

            if (e.IsPing)
            {
                Debug.Log("Ping received!");
                return;
            }
        };

        webSocket.ConnectAsync();

        webSocket.OnOpen += (sender, e) =>
        {
            //sender.Dump();
            //e.Dump();

            var setupRequestString = $"X-Timestamp:{dateTime}\r\n" +
                                     "Content-Type:application/json; charset=utf-8\r\n" +
                                     "Path:speech.config\r\n\r\n" +
                                     "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{" +
                                     "\"sentenceBoundaryEnabled\":false,\"wordBoundaryEnabled\":true}," +
                                     "\"outputFormat\":\"audio-24khz-48kbitrate-mono-mp3\"" +
                                     "}}}}\r\n";

            //setupRequestString.Dump();

            webSocket.SendAsync(setupRequestString, success =>
            {
                //success.Dump();
                if (!success)
                    return;

                string ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'><voice name='{edgeVoiceDTO.Voice}'><prosody pitch='{FormatValue(edgeVoiceDTO.Pitch)}Hz' rate='{FormatValue(edgeVoiceDTO.Rate)}%' volume='{FormatValue(edgeVoiceDTO.Volume)}%'>{edgeVoiceDTO.Text}</prosody></voice></speak>";
                var s1 = $"X-RequestId:{connectId}\r\n" +
                         "Content-Type:application/ssml+xml\r\n" +
                         $"X-Timestamp:{dateTime}Z\r\n" +
                         "Path:ssml\r\n\r\n" +
                         $"{ssml}";

                //s1.Dump();
                webSocket!.Send(s1);
                //$"STA {connect_id}".Dump();
            });
        };

        while (_currentState != EdgeVoiceClientState.Done)
        {
        }
        webSocket.Close();
    }

    public void Dispose()
    {
        _outputDevice?.Dispose();
        _voiceStream?.Dispose();
        _audio?.Dispose();
    }
}