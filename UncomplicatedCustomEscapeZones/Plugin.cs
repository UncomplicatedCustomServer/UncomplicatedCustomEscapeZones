using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Managers;
using UncomplicatedEscapeZones.Utilities;
using EventHandler = UncomplicatedEscapeZones.Events.EventHandler;
using Version = System.Version;

namespace UncomplicatedEscapeZones;

internal class Plugin : Plugin<Config>
{
    internal static Plugin Instance;
    internal static HttpManager HttpManager;
    private EventHandler _handler;
    public override string Name => "UncomplicatedCustomEscapeZones";
    public override string Description => "Customize your SCP:SL server with Custom Escape Zones!";
    public override string Author => "MedveMarci & FoxWorn3365";
    public override Version Version => new(1, 0, 0, 0);
    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
    public override LoadPriority Priority => LoadPriority.Highest;

    public override void Enable()
    {
        Instance = this;
        _handler = new EventHandler();
        HttpManager = new HttpManager("ucez");
        Map.RemoveEscapeZone(Map.DefaultEscapeZone);
        CustomHandlersManager.RegisterEventsHandler(_handler);
        Task.Run(delegate
        {
            if (HttpManager.LatestVersion.CompareTo(Version) > 0)
                LogManager.Warn(
                    $"You are NOT using the latest version of UncomplicatedCustomEscapeZones!\nCurrent: v{Version} | Latest available: v{HttpManager.LatestVersion}\nDownload it from GitHub: https://github.com/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/releases/latest");
            VersionManager.Init();
        });
        FileConfigs.Welcome();
        FileConfigs.Welcome(Server.Port.ToString());
        FileConfigs.LoadAll();
        FileConfigs.LoadAll(Server.Port.ToString());
        foreach (ICustomEscapeZone customEscapeZone in CustomEscapeZone.CustomEscapeZones.Values)
            LogManager.Debug(
                $"Loaded zone: {customEscapeZone.Id} | EscapeRoles: {FormatRoleAfterEscape(customEscapeZone.RoleAfterEscape)}");
        LogManager.Info($"Successfully loaded {CustomEscapeZone.List.Count} zones!");
    }

    public override void Disable()
    {
        _handler = null;
        HttpManager.UnregisterEvents();
        HttpManager = null;
        Instance = null;
    }

    private static string FormatRoleAfterEscape(Dictionary<string, List<Dictionary<string, string>>> data)
    {
        if (data == null || data.Count == 0) return "{}";
        StringBuilder sb = new();
        sb.AppendLine("{");
        int outerIndex = 0;
        int outerCount = data.Count;
        foreach (KeyValuePair<string, List<Dictionary<string, string>>> kvp in data)
        {
            outerIndex++;
            AppendIndent(1);
            sb.Append(kvp.Key);
            sb.Append(": ");
            List<Dictionary<string, string>> list = kvp.Value;
            if (list == null || list.Count == 0)
            {
                sb.Append("[]");
                if (outerIndex < outerCount) sb.Append(',');
                sb.AppendLine();
                continue;
            }

            sb.AppendLine("[");
            int listIndex = 0;
            int listCount = list.Count;
            foreach (Dictionary<string, string> dict in list)
            {
                listIndex++;
                if (dict == null || dict.Count == 0)
                {
                    AppendIndent(2);
                    sb.Append("{}");
                    if (listIndex < listCount) sb.Append(',');
                    sb.AppendLine();
                    continue;
                }

                AppendIndent(2);
                sb.AppendLine("{");
                int innerIndex = 0;
                int innerCount = dict.Count;
                foreach (KeyValuePair<string, string> p in dict)
                {
                    innerIndex++;
                    AppendIndent(3);
                    sb.Append(p.Key);
                    sb.Append(": ");
                    sb.Append(p.Value);
                    if (innerIndex < innerCount) sb.Append(',');
                    sb.AppendLine();
                }

                AppendIndent(2);
                sb.Append('}');
                if (listIndex < listCount) sb.Append(',');
                sb.AppendLine();
            }

            AppendIndent(1);
            sb.Append(']');
            if (outerIndex < outerCount) sb.Append(',');
            sb.AppendLine();
        }

        sb.Append('}');
        return sb.ToString();

        void AppendIndent(int level)
        {
            sb.Append(new string(' ', level * 2));
        }
    }
}