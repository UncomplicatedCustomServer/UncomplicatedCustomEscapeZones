using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Integrations;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Managers;
using UnityEngine;

namespace UncomplicatedEscapeZones.Events;

public class EventHandler : CustomEventsHandler
{
    public override void OnPlayerEscaping(PlayerEscapingEventArgs ev)
    {
        LogManager.Debug($"Player {ev.Player.Nickname} is escaping at {ev.EscapeZone}");

        if (ev.EscapeZone.TryGetEscapeZone(out SummonedEscapeZone escapeZone))
        {
            LogManager.Debug($"Player {ev.Player.Nickname} is escaping at custom escape zone: {escapeZone.Bounds}");
            if (escapeZone.Zone.RoleAfterEscape.Count < 1)
            {
                LogManager.Debug($"Player {ev.Player.Nickname} evaluated for a natural respawn Reason: No RoleAfterEscape configured! {ev.EscapeScenario}");
                ev.IsAllowed = true;
                base.OnPlayerEscaping(ev);
                return;
            }

            KeyValuePair<bool, object>? newRole = EscapeManager.ParseEscapeRole(escapeZone.Zone.RoleAfterEscape, ev.Player);

            if (newRole is null)
            {
                ev.IsAllowed = false;
                LogManager.Debug($"Player {ev.Player.Nickname} is not allowed to escape! Reason: the escape has been denied by the RoleAfterEscape configuration. {ev.EscapeScenario}");
                base.OnPlayerEscaping(ev);
                return;
            }

            // bool: isCustomRole | object: RoleTypeId or CustomRoleId
            KeyValuePair<bool, object> newRoleValue = (KeyValuePair<bool, object>)newRole;

            if (newRoleValue.Value is null)
            {
                ev.IsAllowed = true;
                LogManager.Debug($"Player {ev.Player.Nickname} evaluated for a natural respawn! Reason: RoleAfterEscape returned null! {ev.EscapeScenario}");
                base.OnPlayerEscaping(ev);
                return;
            }

            if (!newRoleValue.Key)
            {
                // Natural role, let's try to parse it
                if (Enum.TryParse(newRoleValue.Value.ToString(), out RoleTypeId role))
                    if (role is not RoleTypeId.None)
                    {
                        ev.NewRole = role;
                        if (ev.EscapeScenario == Escape.EscapeScenarioType.None)
                            ev.EscapeScenario = Escape.EscapeScenarioType.Custom;

                        if (UCR.TryGetSummonedCustomRole(ev.Player, out _))
                        {
                            ev.IsAllowed = false;
                            ev.Player.ConnectionToClient.Send(new Escape.EscapeMessage
                            {
                                ScenarioId = (byte)ev.EscapeScenario,
                                EscapeTime = (ushort)Mathf.CeilToInt(ev.Player.RoleBase.ActiveTime)
                            });
                            ev.Player.SetRole(ev.NewRole, RoleChangeReason.Escaped);
                            return;
                        }

                        ev.IsAllowed = true;
                        LogManager.Debug($"Player {ev.Player.Nickname} will respawn as {role}!");
                    }
            }
            else
            {
                LogManager.Debug($"Trying to find CustomRole with Id {newRoleValue.Value}");
                if (int.TryParse(newRoleValue.Value.ToString(), out int id) && UCR.TryGetCustomRole(id, out object _))
                {
                    LogManager.Debug("Role found!");
                    ev.IsAllowed = false;
                    if (!API.Features.Escape.Bucket.Contains(ev.Player.PlayerId))
                    {
                        LogManager.Debug("Successfully activated the call to method SpawnManager::SummonCustomSubclass(<...>) as the player is not inside the Escape::Bucket bucket! - Adding it...");
                        API.Features.Escape.Bucket.Add(ev.Player.PlayerId);
                        UCR.GiveCustomRole(id, ev.Player);
                        LogManager.Debug($"Successfully called method SpawnManager::SummonCustomSubclass(<...>) for player {ev.Player.Nickname}!");
                        return;
                    }

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

        foreach (ICustomEscapeZone customEscapeZone in CustomEscapeZone.List) new SummonedEscapeZone(customEscapeZone);

        if (Plugin.Instance.Config.EnableBasicLogs)
        {
            LogManager.Info($"Thanks for using UncomplicatedCustomEscapeZones v{Plugin.Instance.Version.ToString(3)} by {Plugin.Instance.Author}! Note that if you're using UCR, this plugin is the higher priority.", ConsoleColor.Blue);
            LogManager.Info("To receive support and to stay up-to-date, join our official Discord server: https://discord.gg/5StRGu8EJV", ConsoleColor.DarkYellow);
        }

        base.OnServerWaitingForPlayers();
    }
}