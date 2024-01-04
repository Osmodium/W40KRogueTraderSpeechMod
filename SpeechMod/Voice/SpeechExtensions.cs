﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SpeechMod.Voice;

public static class SpeechExtensions
{
    private static Dictionary<string, string> m_PhoneticDictionary;

    public static void LoadDictionary()
    {
        Main.Logger?.Log("Loading phonetic dictionary...");
        try
        {
            string file = Path.Combine(Constants.LOCAL_LOW_PATH!,
                "Owlcat Games",
                "Warhammer 40000 Rogue Trader",
                "UnityModManager",
                "W40KSpeechMod",
                "PhoneticDictionary.json");
            string json = File.ReadAllText(file, Encoding.UTF8);
            m_PhoneticDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        catch (Exception ex)
        {
            Main.Logger?.LogException(ex);
            Main.Logger?.Warning("Loading backup dictionary!");
            LoadBackupDictionary();
        }

#if DEBUG
        foreach (var entry in m_PhoneticDictionary)
        {
            Main.Logger?.Log($"{entry.Key}={entry.Value}");
        }
#endif
    }

    private static void LoadBackupDictionary()
    {
        m_PhoneticDictionary = new Dictionary<string, string>
        {
            { "—", "<silence msec=\"500\"/>" },
            {"...", " <silence msec=\"500\"/> "},
            { "servitor", "servitur" }
        };
    }

    public static string PrepareText(this string text)
    {
        text = text.Replace("\"", "");
        text = text.Replace("\n", ". ");
        text = text.Trim().Trim('.');

        if (m_PhoneticDictionary == null)
            LoadBackupDictionary();

        return m_PhoneticDictionary.Aggregate(text, (current, pair) => current?.Replace(pair.Key, pair.Value));
    }

    public static void AddUiElements<T>(string name) where T : MonoBehaviour
    {
        GameObject voiceGameObject = null;
        try
        {
            voiceGameObject = UnityEngine.Object.FindObjectOfType<T>()?.gameObject;
        }
        catch
        {
            // Ignored
        }

        if (voiceGameObject != null)
        {
            Debug.Log($"{typeof(T).Name} found, returning!");
            return;
        }

        Debug.Log($"Adding {typeof(T).Name} SpeechMod UI elements.");

        var windowsVoiceGameObject = new GameObject(name);
        windowsVoiceGameObject.AddComponent<T>();
        UnityEngine.Object.DontDestroyOnLoad(windowsVoiceGameObject);
    }
}