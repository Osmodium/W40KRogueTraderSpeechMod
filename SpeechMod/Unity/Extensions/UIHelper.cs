using System;
using System.Collections;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace SpeechMod.Unity.Extensions;

public static class UIHelper
{
    public static Coroutine ExecuteLater(this MonoBehaviour behaviour, float delay, Action action)
    {
        return behaviour.StartCoroutine(InternalExecute(delay, action));
    }

    private static IEnumerator InternalExecute(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public static bool IsParentClickable(this Transform transform)
    {
        return transform.GetComponentInParents<ObservablePointerClickTrigger>() != null;
    }

    public static T GetComponentInParents<T>(this Transform transform) where T : Component
    {
        var parent = transform?.parent;
        while (parent != null && parent != transform.root)
        {
            var component = parent.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
            parent = parent.parent;
        }
        return null;
    }

    public static void SetRaycastTarget(this Graphic graphic, bool enable)
    {
        if (graphic == null)
            return;

        graphic.raycastTarget = enable;
    }

    public static Transform TryFind(this Transform transform, string n)
    {
        if (string.IsNullOrWhiteSpace(n) || transform == null)
            return null;

        try
        {
            return transform.Find(n);
        }
        catch
        {
            Debug.Log("TryFind found nothing!");
        }

        return null;
    }

    public static Transform TryFind(string n)
    {
        if (string.IsNullOrWhiteSpace(n))
            return null;

        try
        {
            return GameObject.Find(n)?.transform;
        }
        catch
        {
            Debug.Log("TryFind found nothing!");
        }

        return null;
    }

    public static string GetGameObjectPath(this Transform transform)
    {
        string path = transform?.name;
        while (transform?.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}