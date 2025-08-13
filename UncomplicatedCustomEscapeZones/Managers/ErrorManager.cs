using System;
using System.Collections.Generic;
using System.IO;
using LabApi.Loader.Features.Yaml;
using UncomplicatedEscapeZones.API.Features;

namespace UncomplicatedEscapeZones.Managers;

public class YamlError
{
    public string File { get; set; }
    public int? Line { get; set; }
    public int? Column { get; set; }
    public string Message { get; set; }
    public string Suggestion { get; set; }
}

public static class ErrorManager
{
    public static List<YamlError> Errors { get; } = [];

    public static void Add(string file, string message, int? line = null, int? column = null, string suggestion = "")
    {
        Errors.Add(new YamlError
        {
            File = file,
            Message = message,
            Line = line,
            Column = column,
            Suggestion = suggestion
        });
    }

    public static void Clear()
    {
        Errors.Clear();
    }

    public static string GetSuggestionFromMessage(string message)
    {
        message = message.ToLowerInvariant();

        if (message.Contains("while parsing a block mapping"))
            return "Check indentation and YAML structure — something might be misaligned or nested incorrectly.";

        if (message.Contains("expected <block end>, but found"))
            return "Possibly missing a `-` for a list item or the element ends prematurely.";

        if (message.Contains("did not find expected key"))
            return
                "A key may be missing or misaligned — ensure all keys are followed by colons and correctly indented.";

        if (message.Contains("unexpected end of stream"))
            return "The file might be cut off unexpectedly — check for missing closing brackets or incomplete blocks.";

        if (message.Contains("duplicate key"))
            return "You may have defined the same key twice in the same block — YAML requires keys to be unique.";

        if (message.Contains("found character that cannot start any token"))
            return
                "There's probably an illegal character or wrong symbol — double-check for stray tabs or weird characters.";

        if (message.Contains("found unexpected ':'"))
            return "There might be a colon `:` in a value that should be quoted — try wrapping the value in quotes.";

        if (message.Contains("anchor") && message.Contains("not defined"))
            return "You're referencing an anchor (&value or *value) that hasn't been defined.";

        if (message.Contains("alias") && message.Contains("not found"))
            return "YAML alias (*) points to something that doesn't exist — check spelling or anchor placement.";

        if (message.Contains("cannot convert") && message.Contains("to"))
            return
                "A value might be of the wrong type — make sure it's in the correct format (e.g., number vs string).";

        if (message.Contains("sequence entries are not allowed here"))
            return "You're probably using a list (`- item`) in an invalid place — check indentation and nesting.";

        if (message.Contains("unexpected key") || message.Contains("unexpected property"))
            return "This key may be misplaced or invalid — double-check your schema or property names.";

        return "Check your YAML syntax near this location. Be sure indentation, colons, and types are correct.";
    }

    public static bool CustomTypeChecker(string filePath)
    {
        try
        {
            string fileContent = File.ReadAllText(filePath);

            CustomEscapeZone deserializedZone =
                YamlConfigParser.Deserializer.Deserialize<CustomEscapeZone>(fileContent);

            int zoneId;
            try
            {
                zoneId = checked(deserializedZone.Id);
            }
            catch (OverflowException)
            {
                string message = $"Zone ID {deserializedZone.Id} is too large for an int!";
                const string suggestion = "Use a smaller number for the zone ID.";
                Add(filePath, message, suggestion: suggestion);
                LogManager.Error($"{message}\n {suggestion}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            string suggestion = GetSuggestionFromMessage(message);
            Add(filePath, message, suggestion: suggestion);
            LogManager.Error($"Exception: {message}\n Suggestion: {suggestion}");
            return false;
        }
    }
}