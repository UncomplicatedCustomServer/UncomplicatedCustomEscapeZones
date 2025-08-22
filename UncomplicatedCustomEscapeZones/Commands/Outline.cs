using System.Collections.Generic;
using AdminToys;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace UncomplicatedEscapeZones.Commands;

public class Outline : IUCEZCommand
{
    internal static bool IsVisible;
    public string Name { get; } = "outline";

    public string Description { get; } = "Make all Custom Escape Zones visible or invisible";

    public string RequiredPermission { get; } = "ucez.visibility";

    public bool Executor(List<string> arguments, ICommandSender sender, out string response)
    {
        if (!Round.IsRoundInProgress)
        {
            response = "Sorry but you can't use this command if the round is not started!";
            return false;
        }

        foreach (SummonedEscapeZone summonedEscapeZone in SummonedEscapeZone.List.Values)
        {
            if (!summonedEscapeZone.Bounds.TryGetEscapeZone(out SummonedEscapeZone customEscapeZone)) continue;
            if (!IsVisible)
            {
                if (customEscapeZone.AttachedPrimitive != null) continue;
                customEscapeZone.AttachedPrimitive = PrimitiveObjectToy.Create();
                customEscapeZone.AttachedPrimitive.Type = PrimitiveType.Cube;
                customEscapeZone.AttachedPrimitive.Scale = customEscapeZone.Bounds.size;
                customEscapeZone.AttachedPrimitive.Position = customEscapeZone.Bounds.center;
                customEscapeZone.AttachedPrimitive.Rotation = Quaternion.identity;
                customEscapeZone.AttachedPrimitive.Color = new Color(1, 1, 1, 0.3f);
                customEscapeZone.AttachedPrimitive.Flags = PrimitiveFlags.Visible;
                customEscapeZone.AttachedPrimitive.Spawn();
            }
            else
            {
                if (customEscapeZone.AttachedPrimitive == null) continue;
                customEscapeZone.AttachedPrimitive.Destroy();
                customEscapeZone.AttachedPrimitive = null;
            }
        }

        IsVisible = !IsVisible;
        response = "Custom Escape Zones are now " + (IsVisible ? "visible" : "invisible") + ".";
        return true;
    }
}