using System;
using System.Net;
using System.Text.Json;

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
        JsonDocument doc = JsonDocument.Parse(str);
        JsonElement root = doc.RootElement;

        message = null;
        if (root.TryGetProperty("message", out JsonElement messageElement)) message = messageElement.GetString();

        if (root.TryGetProperty("status", out JsonElement status) &&
            Enum.TryParse(status.ToString(), out HttpStatusCode statusCode))
            return statusCode;


        return HttpStatusCode.Unused;
    }
}