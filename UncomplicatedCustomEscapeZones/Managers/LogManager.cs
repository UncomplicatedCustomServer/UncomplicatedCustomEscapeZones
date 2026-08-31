using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Discord;
using LabApi.Features.Console;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Yaml;
using MEC;
using NorthwoodLib.Pools;
using UncomplicatedEscapeZones.API;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Managers.NET;

namespace UncomplicatedEscapeZones.Managers;

internal static class LogManager
{
    private static readonly HashSet<LogEntry> History = [];

    private static bool DebugEnabled => Plugin.Instance.Config.Debug;

    public static void Debug(string message)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Debug), message));
        if (!DebugEnabled)
            return;
        Logger.Debug(message);
    }

    public static void SmInfo(string message, string label = "Info")
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), label, message));
        Logger.Raw($"[{label}] [{Plugin.Instance.Name}] {message}", ConsoleColor.Gray);
    }

    public static void Info(string message, ConsoleColor color = ConsoleColor.Cyan)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Info), message));
        Logger.Raw($"[INFO] [{Plugin.Instance.Name}] {message}", color);
    }

    public static void Warn(string message, string error = "CS0000")
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Warn), message, error));
        Logger.Warn(message);
    }

    public static void Error(string message, string error = "CS0000")
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), nameof(LogLevel.Warn), message, error));
        Logger.Error(message);
    }

    public static void Silent(string message)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "Silent", message));
    }

    public static void System(string message)
    {
        History.Add(new LogEntry(DateTimeOffset.Now.ToUnixTimeMilliseconds(), "System", message));
    }

    internal static IEnumerator<float> SendReport(bool online, Action<HttpStatusCode, string> callback)
    {
        if (History.Count < 1)
        {
            callback?.Invoke(HttpStatusCode.Forbidden, null);
            yield break;
        }

        StringBuilder builder = StringBuilderPool.Shared.Rent();

        foreach (LogEntry Element in History)
            builder.Append($"{Element}\n");

        // Now let's add the separator
        builder.Append("\n======== BEGIN CUSTOM ESCAPE ZONES ========\n");

        foreach (ICustomEscapeZone escapeZone in CustomEscapeZone.List)
            builder.Append($"{YamlConfigParser.Serializer.Serialize(escapeZone)}\n\n---\n\n");

        string report = StringBuilderPool.Shared.ToStringReturn(builder);

        if (!online)
        {
            File.WriteAllText(Path.Combine(PathManager.Configs.FullName, $"UCEZ-Report-{DateTimeOffset.Now.ToUnixTimeSeconds()}.txt"), report);
            callback?.Invoke(HttpStatusCode.OK, null);
            yield break;
        }

        yield return Timing.WaitUntilDone(Plugin.HttpManager.ShareLogs(report, response => callback?.Invoke(ResolveStatus(response), response.Body)));
    }

    /// <summary>
    ///     Gets the status of the answer, preferring the one written inside the body by our APIs
    /// </summary>
    private static HttpStatusCode ResolveStatus(HttpResponse response)
    {
        if (!response.Completed)
            return response.Status;

        HttpStatusCode status = string.IsNullOrWhiteSpace(response.Body) ? HttpStatusCode.Unused : response.Body.GetStatusCode(out _);

        return status is HttpStatusCode.Unused ? response.Status : status;
    }
}