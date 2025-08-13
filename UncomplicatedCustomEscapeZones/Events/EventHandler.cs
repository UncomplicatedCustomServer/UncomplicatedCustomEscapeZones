using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Intergrations;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Events;

public class EventHandler : CustomEventsHandler
{
    public override void OnPlayerEscaping(PlayerEscapingEventArgs ev)
    {
        LogManager.Debug($"Player {ev.Player.Nickname} is escaping at {ev.EscapeZone}");

        if (ev.EscapeZone.TryGetSummonedEscapeZone(out SummonedEscapeZone summoned))
        {
            LogManager.Debug($"Player {ev.Player.Nickname} is escaping at custom escape zone: {summoned}");
            if (summoned.CustomEscapeZone.RoleAfterEscape.Count < 1)
            {
                LogManager.Debug($"Player {ev.Player.Nickname} evaluated for a natural respawn!");
                ev.IsAllowed = true;
                return;
            }

            KeyValuePair<bool, object>? newRole =
                EscapeManager.ParseEscapeRole(summoned.CustomEscapeZone.RoleAfterEscape, ev.Player);

            if (newRole is null)
            {
                ev.IsAllowed = false;
                return;
            }

            KeyValuePair<bool, object> newRoleValue = (KeyValuePair<bool, object>)newRole;

            if (newRoleValue.Value is null)
            {
                ev.IsAllowed = true;
                return;
            }

            if (!newRoleValue.Key)
            {
                // Natural role, let's try to parse it
                if (Enum.TryParse(newRoleValue.Value.ToString(), out RoleTypeId role))
                    if (role is not RoleTypeId.None)
                    {
                        ev.NewRole = role;
                        ev.IsAllowed = true;
                    }
            }
            else
            {
                LogManager.Debug($"Trying to find CustomRole with Id {newRoleValue.Value}");
                if (int.TryParse(newRoleValue.Value.ToString(), out int id) &&
                    UCR.TryGetCustomRole(id, out object _))
                {
                    LogManager.Debug("Role found!");
                    ev.IsAllowed = false;
                    LogManager.Debug(
                        "Successfully activated the call to method SpawnManager::SummonCustomSubclass(<...>) as the player is not inside the Escape::Bucket bucket! - Adding it...");
                    UCR.GiveCustomRole(id, ev.Player);
                }
            }
        }

        base.OnPlayerEscaping(ev);
    }

    public override void OnServerMapGenerated(MapGeneratedEventArgs ev)
    {
        LogManager.Debug("Map generated, removing all escape zones.");

        foreach (SummonedEscapeZone summonedEscapeZone in SummonedEscapeZone.List.ToList())
        {
            summonedEscapeZone.Destroy();
            LogManager.Debug($"Despawned escape zone: {summonedEscapeZone.Id}");
        }

        Map.EscapeZones.ForEach(Map.RemoveEscapeZone);
        base.OnServerMapGenerated(ev);
    }

    public override void OnServerWaitingForPlayers()
    {
        foreach (SummonedEscapeZone summoned in CustomEscapeZone.List.ToList().Select(SummonedEscapeZone.Summon))
            LogManager.Debug($"Summoned escape zone on waiting: {summoned.Id}");

        base.OnServerWaitingForPlayers();
    }
}