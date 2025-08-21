using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Utilities;

namespace UncomplicatedEscapeZones.Commands;

public class Reload : IUCEZCommand
{
    public string Name { get; } = "reload";

    public string Description { get; } = "Reload every custom escape zone loaded and search for new";

    public string RequiredPermission { get; } = "ucez.reload";

    public bool Executor(List<string> arguments, ICommandSender sender, out string response)
    {
        if (!Round.IsRoundInProgress)
        {
            response = "Sorry but you can't use this command if the round is not started!";
            return false;
        }

        ConcurrentDictionary<int, ICustomEscapeZone> oldZones = CustomEscapeZone.CustomEscapeZones.Clone();

        foreach (int zone in oldZones.Keys)
            SummonedEscapeZone.RemoveSpecificEscapeZone(zone);

        CustomEscapeZone.CustomEscapeZones = new ConcurrentDictionary<int, ICustomEscapeZone>();

        FileConfigs.LoadAll();
        FileConfigs.LoadAll(Server.Port.ToString());

        IEnumerable<int> removedZones = oldZones.Keys.Except(CustomEscapeZone.CustomEscapeZones.Keys);


        int added = CustomEscapeZone.CustomEscapeZones.Count - (oldZones.Count + removedZones.Count());

        foreach (ICustomEscapeZone customEscapeZone in CustomEscapeZone.List) new SummonedEscapeZone(customEscapeZone);
        Visibility.IsVisible = false;
        response =
            $"\nSuccessfully reloaded UncomplicatedEscapeZones\n<color=#5db30c>➕</color> Added <b>{(added <= 0 ? "0" : added)}</b> Custom Escape Zones\n<color=#c23636>➖</color> Removed <b>{removedZones.Count()}</b> Custom Escape Zones\n<color=#00ffff>🔢</color> Loaded a total of <b>{CustomEscapeZone.CustomEscapeZones.Count}</b> Custom Escape Zones";
        return true;
    }
}