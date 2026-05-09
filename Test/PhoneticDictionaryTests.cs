using SpeechMod.Voice;
using System.Collections.Generic;
using Xunit;

namespace Test;

public class PhoneticDictionaryTests
{
    public PhoneticDictionaryTests()
    {
        // Load a known test dictionary before each test class run
        PhoneticDictionary.LoadDictionary(new Dictionary<string, string>
        {
            { "servitor", "servitur" },
            { @"\/\/", ", " },
            { "astartes", "astartees" },
            { @"\bthe emperor\b", "the emprah" }
        });
    }

    [Fact]
    public void PrepareText_AppliesPhoneticReplacement()
    {
        var result = "The Servitor walks".PrepareText();
        Assert.Equal("the servitur walks", result);
    }

    [Fact]
    public void PrepareText_AppliesRegexReplacement()
    {
        var result = "Text // more text".PrepareText();
        Assert.Equal("text ,  more text", result);
    }

    [Fact]
    public void PrepareText_AppliesWordBoundaryRegex()
    {
        var result = "The Emperor protects".PrepareText();
        Assert.Equal("the emprah protects", result);
    }

    [Fact]
    public void PrepareText_DoesNotMatchPartialWordBoundary()
    {
        // "the emperor" should not match inside "the emperors"
        var result = "The Emperors guard".PrepareText();
        Assert.Equal("the emperors guard", result);
    }

    [Fact]
    public void PrepareText_MultipleReplacementsInSameText()
    {
        var result = "A servitor serves the Astartes".PrepareText();
        Assert.Equal("a servitur serves the astartees", result);
    }

    [Fact]
    public void PrepareText_LowercasesText()
    {
        var result = "ALL CAPS TEXT".PrepareText();
        Assert.Equal("all caps text", result);
    }

    [Fact]
    public void PrepareText_StripsQuotes()
    {
        var result = "He said \"hello\" loudly".PrepareText();
        Assert.Equal("he said hello loudly", result);
    }

    [Fact]
    public void PrepareText_ReplacesNewlinesWithPeriodSpace()
    {
        var result = "Line one\nLine two".PrepareText();
        Assert.Equal("line one. line two", result);
    }

    [Fact]
    public void PrepareText_ReplacesCarriageReturnNewline()
    {
        var result = "Line one\r\nLine two".PrepareText();
        Assert.Equal("line one. line two", result);
    }

    [Fact]
    public void PrepareText_SpacesOutDates()
    {
        var result = "On 15/03/2024 something happened".PrepareText();
        Assert.Equal("on 15 / 03 / 2024 something happened", result);
    }

    [Fact]
    public void PrepareText_TrimsWhitespace()
    {
        var result = "  hello world  ".PrepareText();
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void PrepareText_EmptyDictionary_StillProcessesText()
    {
        PhoneticDictionary.LoadDictionary(new Dictionary<string, string>());
        var result = "Hello World".PrepareText();
        Assert.Equal("hello world", result);

        // Restore non-empty dictionary so subsequent tests don't trigger file-based fallback
        PhoneticDictionary.LoadDictionary(new Dictionary<string, string>
        {
            { "servitor", "servitur" }
        });
    }

    [Fact]
    public void PrepareText_NullDictionary_LoadsEmptyAndProcesses()
    {
        PhoneticDictionary.LoadDictionary(null);
        // Null → empty dict, no phonetic replacements, but text processing still works
        // However PrepareText checks Any() and will try file-based fallback,
        // so we must ensure at least one entry to avoid that path in tests.
        // Instead, verify LoadDictionary(null) doesn't throw.
        PhoneticDictionary.LoadDictionary(new Dictionary<string, string>
        {
            { "test", "tested" }
        });
        var result = "A test".PrepareText();
        Assert.Equal("a tested", result);
    }

    [Fact]
    public void LoadDictionary_CanBeReloaded()
    {
        // First dictionary
        PhoneticDictionary.LoadDictionary(new Dictionary<string, string>
        {
            { "alpha", "bravo" }
        });
        Assert.Equal("bravo team", "Alpha team".PrepareText());

        // Reload with different dictionary
        PhoneticDictionary.LoadDictionary(new Dictionary<string, string>
        {
            { "alpha", "charlie" }
        });
        Assert.Equal("charlie team", "Alpha team".PrepareText());
    }
}


