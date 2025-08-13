using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Utilities;

namespace UncomplicatedEscapeZones.Events;

public class EventHandler : CustomEventsHandler
{
    public override void OnPlayerEscaping(PlayerEscapingEventArgs ev)
    {
        LogManager.Debug($"Player {ev.Player.Nickname} is escaping at {ev.EscapeZone}");
        if (ev.EscapeZone.TryGetSummonedEscapeZone(out SummonedEscapeZone summoned))
        {
            LogManager.Debug($"Player {ev.Player.Nickname} is escaping at {summoned}");
            if (summoned.CustomEscapeZone.RoleAfterEscape.Count < 1)
            {
                LogManager.Debug($"Player {ev.Player.Nickname} evaluated for a natural respawn!");
                ev.IsAllowed = true;
                return;
            }
            
            KeyValuePair<bool, object>? newRole = EscapeManager.ParseEscapeRole(summoned.CustomEscapeZone.RoleAfterEscape, ev.Player);
            
            if (newRole is null)
            { 
                ev.IsAllowed = false;
                return;
            }

            KeyValuePair<bool, object> NewRole = (KeyValuePair<bool, object>)newRole;

            if (NewRole.Value is null)
            {
                ev.IsAllowed = true;
                return;
            }

            if (!NewRole.Key)
            {
                // Natural role, let's try to parse it
                if (Enum.TryParse(NewRole.Value.ToString(), out RoleTypeId role)) 
                {
                    if (role is not RoleTypeId.None)
                    {
                        ev.NewRole = role;
                        ev.IsAllowed = true;
                    }
                }
            } 
            /*else
            {
                LogManager.Debug($"Trying to find CustomRole with Id {NewRole.Value}");
                if (int.TryParse(NewRole.Value.ToString(), out int id) && CustomRole.TryGet(id, out ICustomRole role))
                {
                    LogManager.Silent($"Role found!");
                    Escaping.IsAllowed = false;
                    if (!API.Features.Escape.Bucket.Contains(Escaping.Player.PlayerId))
                    {
                        LogManager.Silent($"Successfully activated the call to method SpawnManager::SummonCustomSubclass(<...>) as the player is not inside the Escape::Bucket bucket! - Adding it...");
                        API.Features.Escape.Bucket.Add(Escaping.Player.PlayerId);
                        SpawnManager.SummonCustomSubclass(Escaping.Player, role.Id);
                    }
                    else
                        LogManager.Debug($"Canceled call to method SpawnManager::SummonCustomSubclass(<...>) due to the presence of the player inside the Escape::Bucket! - Event already fired!");
                }
            }*/
            
        }
        base.OnPlayerEscaping(ev);
    }

    public override void OnServerMapGenerated(MapGeneratedEventArgs ev)
    {
        LogManager.Debug("Map generated, removing all escape zones.");
        Map.EscapeZones.ForEach(Map.RemoveEscapeZone);
        base.OnServerMapGenerated(ev);
    }

    public override void OnServerWaitingForPlayers()
    {
        foreach (SummonedEscapeZone summoned in CustomEscapeZone.List.ToList().Select(SummonedEscapeZone.Summon))
        {
            LogManager.Debug($"Summoned escape zone on waiting: {summoned.Id}");
        }

        base.OnServerWaitingForPlayers();
    }

    public override void OnServerRoundEnded(RoundEndedEventArgs ev)
    {
        LogManager.Debug("Round ended, despawning all summoned escape zones.");
        foreach (SummonedEscapeZone summonedEscapeZone in SummonedEscapeZone.List.ToList())
        {
            summonedEscapeZone.Destroy();
            LogManager.Debug($"Despawned escape zone: {summonedEscapeZone.Id}");
        }
        base.OnServerRoundEnded(ev);
    }

    public override void OnServerRoundRestarted()
    {
        LogManager.Debug("Round restarted, despawning all summoned escape zones.");
        foreach (SummonedEscapeZone summonedEscapeZone in SummonedEscapeZone.List.ToList())
        {
            summonedEscapeZone.Destroy();
            LogManager.Debug($"Despawned escape zone: {summonedEscapeZone.Id}");
        }
        base.OnServerRoundRestarted();
    }
}