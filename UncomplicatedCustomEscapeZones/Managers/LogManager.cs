using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Discord;
using LabApi.Features.Console;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Yaml;
using NorthwoodLib.Pools;
using UncomplicatedEscapeZones.API;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Interfaces;

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

    internal static HttpStatusCode SendReport(out string content, bool online = true)
    {
        content = null;

        if (History.Count < 1)
            return HttpStatusCode.Forbidden;

        StringBuilder builder = StringBuilderPool.Shared.Rent();

        foreach (LogEntry Element in History)
            builder.Append($"{Element}\n");

        // Now let's add the separator
        builder.Append("\n======== BEGIN CUSTOM ESCAPE ZONES ========\n");

        foreach (ICustomEscapeZone escapeZone in CustomEscapeZone.List)
            builder.Append($"{YamlConfigParser.Serializer.Serialize(escapeZone)}\n\n---\n\n");

        HttpStatusCode response = HttpStatusCode.OK;
        if (online)
            response = Plugin.HttpManager.ShareLogs(StringBuilderPool.Shared.ToStringReturn(builder), out content);
        else
            File.WriteAllText(
                Path.Combine(PathManager.Configs.FullName, $"UCR-Report-{DateTimeOffset.Now.ToUnixTimeSeconds()}.txt"),
                StringBuilderPool.Shared.ToStringReturn(builder));

        return response;
    }
}