using InventorySystem.Items.Firearms.Modules;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Events;
using UncomplicatedEscapeZones.Managers;
using UncomplicatedEscapeZones.Utilities;
using Version = System.Version;

namespace UncomplicatedEscapeZones;

internal class Plugin : Plugin<Config>
{
    internal static Plugin Instance;

    internal static HttpManager HttpManager;

    internal FileConfigs FileConfigs;

    public EventHandler Handler;
    public override string Name => "UncomplicatedCustomEscapeZones";

    public override string Description => "Customize your SCP:SL server with Custom Escape Zones!";

    public override string Author => "FoxWorn3365 & MedveMarci";

    public override Version Version => new(1, 0, 0, 0);

    public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

    public override void Enable()
    {
        Instance = this;

        Handler = new EventHandler();
        //HttpManager = new("ucez");
        FileConfigs = new FileConfigs();

        CustomEscapeZone.List.Clear();
        SummonedEscapeZone.List.Clear();
        Map.RemoveEscapeZone(Map.DefaultEscapeZone);

        CustomHandlersManager.RegisterEventsHandler(Handler);

        LogManager.Info("===========================================");
        LogManager.Info(" Thanks for using UncomplicatedCustomEscapeZones");
        LogManager.Info("        by MedveMarci & FoxWorn3365");
        LogManager.Info("===========================================");
        LogManager.Info(">> Join our discord: https://discord.gg/5StRGu8EJV <<");

        /*Task.Run(delegate
        {
            if (HttpManager.LatestVersion.CompareTo(Version) > 0)
                LogManager.Warn($"You are NOT using the latest version of UncomplicatedCustomEscapeZones!\nCurrent: v{Version} | Latest available: v{HttpManager.LatestVersion}\nDownload it from GitHub: https://github.com/UncomplicatedCustomServer/UncomplicatedCustomEscapeZones/releases/latest");
            else if (HttpManager.LatestVersion.CompareTo(Version) < 0)
            {
                LogManager.Warn($"You are using an EXPERIMENTAL or PRE-RELEASE version of UncomplicatedCustomEscapeZones!\nLatest stable release: {HttpManager.LatestVersion}\nWe do not assure that this version won't make your SCP:SL server crash! - Debug log has been enabled!");
            }
        });*/

        FileConfigs.Welcome(Server.Port.ToString());
        FileConfigs.LoadAll(Server.Port.ToString());

        LogManager.Info($"Successfully loaded {CustomEscapeZone.List.Count} zones!");
        foreach (CustomEscapeZone customEscapeZone in CustomEscapeZone.List)
            LogManager.Debug(
                $"Loaded zone: Name: {customEscapeZone.Id} | EscapeRoles: {customEscapeZone.RoleAfterEscape}");
    }

    public override void Disable()
    {
        Handler = null;

        HttpManager.UnregisterEvents();
        HttpManager = null;
        FileConfigs = null;

        Instance = null;
    }
}