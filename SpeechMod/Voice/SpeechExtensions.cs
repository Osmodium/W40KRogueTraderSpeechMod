using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SpeechMod.Voice;

public static class SpeechExtensions
{
    public static void AddUiElements<T>(string name) where T : MonoBehaviour
    {
        GameObject voiceGameObject = null;
        try
        {
            voiceGameObject = Object.FindFirstObjectByType<T>()?.gameObject;
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

        var newVoiceGameObject = new GameObject(name);
        newVoiceGameObject.AddComponent<T>();
        Object.DontDestroyOnLoad(newVoiceGameObject);
    }
}