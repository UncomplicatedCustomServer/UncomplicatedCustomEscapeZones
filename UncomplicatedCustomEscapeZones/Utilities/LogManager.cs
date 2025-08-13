/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Discord;
using LabApi.Loader.Features.Yaml;
using UncomplicatedEscapeZones.API.Features;
using Logger = LabApi.Features.Console.Logger;

namespace UncomplicatedEscapeZones.Utilities;

internal class LogManager
{
    // We should store the data here
    public static readonly List<KeyValuePair<KeyValuePair<long, LogLevel>, string>> History = [];

    public static bool MessageSent { get; internal set; }

    public static bool DebugEnabled => Plugin.Instance.Config!.Debug;

    public static void Debug(string message)
    {
        History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Debug), message));

        if (!DebugEnabled)
            return;

        Logger.Raw($"[DEBUG] [{Plugin.Instance.Name}] {message}", ConsoleColor.Cyan);
    }

    public static void Info(string message, ConsoleColor color = ConsoleColor.Cyan)
    {
        History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Info), message));
        Logger.Raw($"[INFO] [{Plugin.Instance.Name}] {message}", color);
    }

    public static void Warn(string message, string error = "CS0000")
    {
        History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Warn), message));
        Logger.Warn(message);
    }

    public static void Error(string message, string error = "CS0000")
    {
        History.Add(new(new(DateTimeOffset.Now.ToUnixTimeMilliseconds(), LogLevel.Error), message));
        Logger.Error(message);
    }

    internal static HttpStatusCode SendReport(out HttpContent content)
    {
        content = null;

        if (MessageSent)
            return HttpStatusCode.Forbidden;

        if (History.Count < 1)
            return HttpStatusCode.Forbidden;

        string stringContent = string.Empty;

        foreach (KeyValuePair<KeyValuePair<long, LogLevel>, string> element in History)
        {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeMilliseconds(element.Key.Key);
            stringContent += $"[{date.Year}-{date.Month}-{date.Day} {date.Hour}:{date.Minute}:{date.Second} {date.Offset}]  [{element.Key.Value.ToString().ToUpper()}]  [UncomplicatedCustomEscapeZones] {element.Value}\n";
        }

        // Now let's add the separator
        stringContent += "\n======== BEGIN CUSTOM ROLES ========\n";

        foreach (CustomEscapeZone escapeZone in CustomEscapeZone.List)
            stringContent += $"{YamlConfigParser.Serializer.Serialize(escapeZone)}\n\n---\n\n";

        HttpStatusCode response = Plugin.HttpManager.ShareLogs(stringContent, out content);

        if (response is HttpStatusCode.OK)
            MessageSent = true;

        return response;
    }
}