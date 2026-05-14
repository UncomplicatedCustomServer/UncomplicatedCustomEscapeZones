using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.API;
using UncomplicatedEscapeZones.API.Struct;
using UncomplicatedEscapeZones.Extensions;

namespace UncomplicatedEscapeZones.Managers;

internal class HttpManager
{
    /// <summary>
    ///     Create a new instance of the HttpManager
    /// </summary>
    /// <param name="prefix"></param>
    public HttpManager(string prefix)
    {
        Prefix = prefix;
        RegisterEvents();
        Task.Run(LoadCreditTags);
    }

    /// <summary>
    ///     Gets the prefix of the plugin for our APIs
    /// </summary>
    private string Prefix { get; }

    /// <summary>
    ///     Gets the UCS APIs endpoint
    /// </summary>
    private static string Endpoint => "https://api.ucserver.it/v3/plugin";

    /// <summary>
    ///     Gets the CreditTag storage for the plugin, downloaded from our central server
    /// </summary>
    private Dictionary<string, Triplet<string, string, bool>> Credits { get; set; } = new();

    /// <summary>
    ///     Gets the latest <see cref="Version" /> of the plugin, loaded by the UCS cloud
    /// </summary>
    public Version LatestVersion
    {
        get
        {
            if (_latestVersion is null)
                LoadLatestVersion();
            return _latestVersion;
        }
    }

    private Version _latestVersion { get; set; }

    private bool _alreadyManaged { get; set; }

    private void RegisterEvents()
    {
        PlayerEvents.Joined += OnVerified;
    }

    internal void UnregisterEvents()
    {
        PlayerEvents.Joined -= OnVerified;
    }

    private void OnVerified(PlayerJoinedEventArgs ev)
    {
        ApplyCreditTag(ev.Player);
    }

    private void LoadLatestVersion()
    {
        string version = HttpQuery.Get($"{Endpoint}/{Prefix}/versions/latest@text/plain");

        if (!string.IsNullOrEmpty(version) && version.Contains("."))
            _latestVersion = new Version(version);
        else
            _latestVersion = new Version();
    }

    private void LoadCreditTags()
    {
        Credits = new Dictionary<string, Triplet<string, string, bool>>();
        try
        {
            Dictionary<string, Dictionary<string, JsonElement>> data =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(
                    HttpQuery.Get("https://api.ucserver.it/credits.json"));

            if (data is null)
            {
                LogManager.Warn("Failed to connect to the UCS Central Server to get the credit tags informations!");
                return;
            }

            foreach (KeyValuePair<string, Dictionary<string, JsonElement>> kvp in data.Where(kvp =>
                         kvp.Value is not null && kvp.Value.ContainsKey("role") && kvp.Value.ContainsKey("color") &&
                         kvp.Value.ContainsKey("override") && kvp.Value.ContainsKey("job")))
            {
                string role = kvp.Value["role"].GetString();
                string color = kvp.Value["color"].GetString();
                bool overrideStr = kvp.Value["override"].ValueKind switch
                {
                    JsonValueKind.String => bool.Parse(kvp.Value["override"].GetString() ?? string.Empty),
                    JsonValueKind.True => true,
                    _ => false
                };
                Credits.Add(kvp.Key, new Triplet<string, string, bool>(role, color, overrideStr));
            }
        }
        catch (Exception e)
        {
            LogManager.Error("An error occurred while loading the credit tags from the UCS Central Server!");
            LogManager.Debug(
                $"Failed to act HttpManager::LoadCreditTags() - {e.GetType().FullName}: {e.Message}\n{e.StackTrace}");
        }
    }

    private Triplet<string, string, bool> GetCreditTag(Player player)
    {
        if (Credits.TryGetValue(player.UserId, out Triplet<string, string, bool> tag))
            return tag;

        return new Triplet<string, string, bool>(null, null, false);
    }

    private void ApplyCreditTag(Player player)
    {
        if (!Plugin.Instance.Config.EnableCreditTags)
            return;

        if (_alreadyManaged)
            return;

        Triplet<string, string, bool> tag = GetCreditTag(player);

        if (!string.IsNullOrEmpty(player.ReferenceHub.serverRoles.Network_myText))
        {
            if (Credits.Any(k =>
                    k.Value.First == player.ReferenceHub.serverRoles.Network_myText &&
                    k.Value.Second == player.ReferenceHub.serverRoles.Network_myColor))
                _alreadyManaged = true;

            if (!tag.Third)
                return;
        }

        if (tag.First is null || tag.Second is null)
            return;
        player.ReferenceHub.serverRoles.SetText(tag.First);
        player.ReferenceHub.serverRoles.SetColor(tag.Second);
    }

    internal HttpStatusCode ShareLogs(string data, out string content)
    {
        content = HttpQuery.Post($"{Endpoint}/{Prefix}/logs", JsonSerializer.Serialize(new ShareLogMessage(data)),
            "application/json");
        return content.GetStatusCode(out _);
    }

#nullable enable
    internal string VersionInfo()
    {
        return HttpQuery.Get($"{Endpoint}/{Prefix}/versions/{Plugin.Instance.Version.ToString(4)}");
    }
}