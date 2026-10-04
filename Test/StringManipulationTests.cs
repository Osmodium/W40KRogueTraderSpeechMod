using NSubstitute;
using SpeechMod;
using SpeechMod.Voice;
using System.Collections.Generic;
using Xunit;

namespace Test;

public class StringManipulationTests
{
    private const string NARRATOR_COLOR = "3c2d0a";

    public StringManipulationTests()
    {
        Main.Settings = new Settings
        {
            NarratorPitch = 0,
            NarratorVoice = 0,
            NarratorRate = 0,
            NarratorVolume = 100,
            FemalePitch = 0,
            FemaleVoice = 1,
            FemaleRate = 0,
            FemaleVolume = 100,
            MalePitch = 0,
            MaleVoice = 2,
            MaleRate = 0,
            MaleVolume = 100,
            AvailableVoices = new[] { "Narrator#EN", "Female#EN", "Male#EN" }
        };

        // Load an empty phonetic dictionary so PrepareText doesn't hit the file system
        PhoneticDictionary.LoadDictionary(new Dictionary<string, string>());
    }

    [Theory]
    [MemberData(nameof(GenerateMaleDialogTexts))]
    public void PrepareMaleDialogText(string input, string output)
    {
        // Arrange
        var mock = Substitute.For<WindowsSpeech>()!;
        mock.CombinedDialogVoiceStart.Returns(mock.CombinedMaleVoiceStart);

        // Act
        var text = mock.PrepareDialogText(input)!;

        // Assert
        Assert.Equal(output, text);
    }

    [Theory]
    [MemberData(nameof(GenerateFemaleDialogTexts))]
    public void PrepareFemaleDialogText(string input, string output)
    {
        // Arrange
        var mock = Substitute.For<WindowsSpeech>()!;
        mock.CombinedDialogVoiceStart.Returns(mock.CombinedFemaleVoiceStart);

        // Act
        var text = mock.PrepareDialogText(input)!;

        // Assert
        Assert.Equal(output, text);
    }

    public static TheoryData<string, string> GenerateMaleDialogTexts()
    {
        return new TheoryData<string, string>
        {
            {
                $"Here we are again, Commander. <i><color=#{NARRATOR_COLOR}>Liotr looks grim but focused.</color></i> So let us take another glimpse into the past",
                "<voice required=\"Name=Male\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/>here we are again, commander. </voice><voice required=\"Name=Narrator\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/>liotr looks grim but focused.</voice><voice required=\"Name=Male\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/> so let us take another glimpse into the past</voice>"
            },
            {
                $"<i><color=#{NARRATOR_COLOR}>The booming voice of an old man shakes the walls of the hall.</color></i> Get away from him, demon! Let the boy go.",
                "<voice required=\"Name=Narrator\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/>the booming voice of an old man shakes the walls of the hall.</voice><voice required=\"Name=Male\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/> get away from him, demon! let the boy go.</voice>"
            }
        };
    }

    public static TheoryData<string, string> GenerateFemaleDialogTexts()
    {
        return new TheoryData<string, string>
        {
            {
                $"Here we are again, Commander. <i><color=#{NARRATOR_COLOR}>Liotr looks grim but focused.</color></i> So let us take another glimpse into the past",
                "<voice required=\"Name=Female\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/>here we are again, commander. </voice><voice required=\"Name=Narrator\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/>liotr looks grim but focused.</voice><voice required=\"Name=Female\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/> so let us take another glimpse into the past</voice>"
            },
            {
                $"<i><color=#{NARRATOR_COLOR}>The booming voice of an old man shakes the walls of the hall.</color></i> Get away from him, demon! Let the boy go.",
                "<voice required=\"Name=Narrator\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/>the booming voice of an old man shakes the walls of the hall.</voice><voice required=\"Name=Female\"><pitch absmiddle=\"0\"/><rate absspeed=\"0\"/><volume level=\"100\"/> get away from him, demon! let the boy go.</voice>"
            }
        };
    }
}
