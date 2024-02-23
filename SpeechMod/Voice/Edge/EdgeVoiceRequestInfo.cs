using System;

namespace SpeechMod.Voice.Edge;

public class EdgeVoiceRequestInfo
{
    public string RequestId { get; set; }
    public string ContentType { get; set; }
    public string Path { get; set; }
    public string Data { get; set; }

    public static EdgeVoiceRequestInfo ParseInput(string input)
    {
        EdgeVoiceRequestInfo result = new EdgeVoiceRequestInfo();

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

