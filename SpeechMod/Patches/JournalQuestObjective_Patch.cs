using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using TMPro;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(BaseJournalItemPCView), "UpdateView")]
public static class JournalQuestObjective_Patch
{
    private static readonly string m_ButtonName = "JQSpeechButton";

    private const string BODY_GROUP_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/JournalView/Device/ContentGroup/Screen_view/ItemView/JournalQuestPCView/BodyGroup";
    private const string CHAPTER_HEADER_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/JournalView/Device/ContentGroup/Screen_view/ItemView/JournalQuestPCView/HeaderGroup/Title/TitleGroup/Text";

    public static void Postfix()
    {
        // TODO!
        return;
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(BaseJournalItemPCView)}_UpdateView_Postfix");
#endif

        HookChapterHeader();
        HookBodyGroup();
    }

    private static void HookChapterHeader()
    {
        Hooks.HookUpTextToSpeechOnTransformWithPath(CHAPTER_HEADER_PATH, true);
    }

    private static void HookBodyGroup()
    {
        var bodyGroup = UIHelper.TryFind(BODY_GROUP_PATH);
        if (bodyGroup == null)
        {
            Debug.LogWarning("Couldn't find BodyGroup...");
            return;
        }

        var allTexts = bodyGroup.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (allTexts == null || allTexts.Length == 0)
        {
            Debug.LogWarning("Couldn't any TextMeshProUGUI...");
            return;
        }

        bool isFirst = true;
        foreach (var textMeshPro in allTexts)
        {
            var tmpTransform = textMeshPro?.transform;
            Debug.LogWarning($"Found {tmpTransform?.name}...");
            //if (!ShouldAddButton(tmpTransform))
                //continue;

            var button = tmpTransform?.TryFind(m_ButtonName)?.gameObject;

            if (button != null)
            {
                ResetButton(button, tmpTransform, ref isFirst);
                continue;
            }

            AddButton(tmpTransform, textMeshPro, ref isFirst);
        }

        // Move the line back behind our buttons.
        //var allImages = bodyGroup.GetComponentsInChildren<Image>();
        //foreach (var image in allImages)
        //{
        //    if (image.gameObject.name.Equals("LeftVerticalBorderImage"))
        //        image.transform.SetAsFirstSibling();
        //}
    }

    private static void ResetButton(GameObject button, Transform transform, ref bool isFirst)
    {

#if DEBUG
        Debug.Log("Button already added, relocating and activating...");
#endif

        button.transform.localRotation = Quaternion.Euler(0, 0, 270);
        //transform.gameObject.RectAlignTopLeft();
        //button.RectAlignTopLeft();
        //SetNewPosition(transform, button.transform, ref isFirst);
        button.SetActive(true);
    }

    private static void AddButton(Transform transform, TextMeshProUGUI textMeshPro, ref bool isFirst)
    {

#if DEBUG
        Debug.Log($"Adding playbutton to {transform.name}...");
#endif

        var button = ButtonFactory.CreatePlayButton(transform, () =>
        {
            Main.Speech.Speak(textMeshPro.text);
        });
        button.name = m_ButtonName;
        button.transform.localRotation = Quaternion.Euler(0, 0, 270);
        //transform.gameObject.RectAlignTopLeft();
        //button.RectAlignTopLeft();
        //SetNewPosition(transform, button.transform, ref isFirst);
        button.SetActive(true);
    }

    private static bool ShouldAddButton(Transform transform)
    {
        switch (transform.name)
        {
            case "CompletionItem":
            case "DescriptionItem":
            case "TextLabel":
            case "Text":
                return true;
            default:
                return false;
        }
    }

    private static void SetNewPosition(Transform tmpTransform, Transform transform, ref bool isFirst)
    {
        switch (tmpTransform.name)
        {
            case "CompletionItem":
                transform.localPosition = new Vector3(-72, -35, 0);
                break;
            case "Text":
                transform.localPosition = new Vector3(0, -42, 0);
                break;
            case "DescriptionItem":
                if (isFirst)
                {
                    isFirst = false;
                    transform.localPosition = new Vector3(-10, -24, 0);
                    break;
                }
                transform.localPosition = new Vector3(-35, -24, 0);
                break;
            //case "TextLabel":
            //    var ipi = tmpTransform.parent.TryFind("InProgressImage").gameObject;
            //    transform.localPosition = new Vector3(-82, ipi.transform.InverseTransformPoint(ipi.transform.position).y - 26, 0);
            //    break;
            default:
                transform.localPosition = Vector3.zero;
                break;
        }
    }
}