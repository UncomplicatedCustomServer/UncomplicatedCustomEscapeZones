using System;
using System.Net;
using System.Text.Json;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Extensions;

public static class StringExtension
{
    public static string GenerateWithBuffer(this string str, int bufferSize)
    {
        for (int a = str.Length; a < bufferSize; a++)
            str += " ";

        return str;
    }

    public static HttpStatusCode GetStatusCode(this string str, out string message)
    {
        message = null;

        JsonDocument doc;

        try
        {
            doc = JsonDocument.Parse(str);
        }
        catch (Exception e)
        {
            LogManager.Debug($"The answer is not a valid JSON ({e.Message}), returning HttpStatusCode.Unused");
            message = str;
            return HttpStatusCode.Unused;
        }

        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("message", out JsonElement messageElement)) message = messageElement.GetString();

        if (root.TryGetProperty("status", out JsonElement status) &&
            Enum.TryParse(status.ToString(), out HttpStatusCode statusCode))
            return statusCode;


        return HttpStatusCode.Unused;
    }
}