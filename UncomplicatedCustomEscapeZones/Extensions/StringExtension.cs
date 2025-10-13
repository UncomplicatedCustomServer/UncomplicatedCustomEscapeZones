using System;
using System.Net;
using Newtonsoft.Json.Linq;

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
        JObject obj = JObject.Parse(str);

        message = null;
        if (obj.TryGetValue("message", out JToken token))
            message = token.ToString();

        if (obj.TryGetValue("status", out JToken status) && Enum.TryParse(status.ToString(), out HttpStatusCode statusCode))
            return statusCode;

        return HttpStatusCode.Unused;
    }
}