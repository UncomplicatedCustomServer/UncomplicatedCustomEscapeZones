using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Intergrations;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Events;

public class EventHandler : CustomEventsHandler
{
    public override void OnPlayerEscaping(PlayerEscapingEventArgs ev)
    {
        LogManager.Debug($"Player {ev.Player.Nickname} is escaping at {ev.EscapeZone}");

        if (ev.EscapeZone.TryGetEscapeZone(out SummonedEscapeZone escapeZone))
        {
            LogManager.Debug($"Player {ev.Player.Nickname} is escaping at custom escape zone: {escapeZone}");
            if (escapeZone.Zone.RoleAfterEscape.Count < 1)
            {
                LogManager.Debug($"Player {ev.Player.Nickname} evaluated for a natural respawn!");
                ev.IsAllowed = true;
                return;
            }

            KeyValuePair<bool, object>? newRole =
                EscapeManager.ParseEscapeRole(escapeZone.Zone.RoleAfterEscape, ev.Player);

            if (newRole is null)
            {
                ev.IsAllowed = false;
                LogManager.Debug($"Player {ev.Player.Nickname} has no role to be assigned after escaping!");
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
                LogManager.Debug($"Trying to find CustomRole with Id {newRoleValue.Key}");
                if (int.TryParse(newRoleValue.Key.ToString(), out int id) && UCR.TryGetCustomRole(id, out object _))
                {
                    LogManager.Debug("Role found!");
                    ev.IsAllowed = false;
                    if (!API.Features.Escape.Bucket.Contains(ev.Player.PlayerId))
                    {
                        LogManager.Debug("Successfully activated the call to method SpawnManager::SummonCustomSubclass(<...>) as the player is not inside the Escape::Bucket bucket! - Adding it...");
                        API.Features.Escape.Bucket.Add(ev.Player.PlayerId);
                        UCR.GiveCustomRole(id, ev.Player);
                    }
                    else
                        LogManager.Debug("Canceled call to method SpawnManager::SummonCustomSubclass(<...>) due to the presence of the player inside the Escape::Bucket! - Event already fired!");
                }
            }
        }

        base.OnPlayerEscaping(ev);
    }

    public override void OnPlayerEscaped(PlayerEscapedEventArgs ev)
    {
        if (API.Features.Escape.Bucket.Contains(ev.Player.PlayerId))
            API.Features.Escape.Bucket.Remove(ev.Player.PlayerId);
        base.OnPlayerEscaped(ev);
    }

    public override void OnServerWaitingForPlayers()
    {
        LogManager.Debug("Waiting For Player, reloading all escape zones.");

        foreach (SummonedEscapeZone summonedEscapeZone in SummonedEscapeZone.List.Values)
        {
            summonedEscapeZone.Destroy();
            LogManager.Debug($"Despawned escape zone: {summonedEscapeZone.Id}");
        }
        
        SummonedEscapeZone.List.Clear();
        Map.EscapeZones.ForEach(Map.RemoveEscapeZone);

        foreach (ICustomEscapeZone customEscapeZone in CustomEscapeZone.List)
        {
            new SummonedEscapeZone(customEscapeZone);
        }
        
        LogManager.Info($"Thanks for using UncomplicatedCustomEscapeZones v{Plugin.Instance.Version.ToString(3)} by {Plugin.Instance.Author}! Note that if you're using UCR, this plugin is the higher priority.", ConsoleColor.Blue);
        LogManager.Info("To receive support and to stay up-to-date, join our official Discord server: https://discord.gg/5StRGu8EJV", ConsoleColor.DarkYellow);

        base.OnServerWaitingForPlayers();
    }
}