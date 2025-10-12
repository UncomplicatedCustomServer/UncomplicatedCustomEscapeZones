#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Intergrations;

namespace UncomplicatedEscapeZones.Managers;

public class EscapeManager
{
    public static KeyValuePair<bool, object?>? ParseEscapeRole(
        Dictionary<string, List<Dictionary<string, string>>> roleAfterEscape, Player player)
    {
        // Determine which role-specific configuration applies to this player
        string playerRoleKey = player.Role.ToString();
        string playerTeamKey = player.Team.ToString();
        string playerFactionKey = player.Faction.ToString();
        
        LogManager.Debug($"Player Role: {playerRoleKey}");
        LogManager.Debug($"Player Team: {playerTeamKey}");
        LogManager.Debug($"Player Faction: {playerFactionKey}");
        
        if (string.IsNullOrWhiteSpace(playerRoleKey) || string.IsNullOrWhiteSpace(playerTeamKey) || string.IsNullOrWhiteSpace(playerFactionKey))
        {
            LogManager.Warn(
                $"Unable to determine player's role or team for escape evaluation (PlayerId={player.PlayerId}). Allowing natural escape.");
            return new KeyValuePair<bool, object?>(false, null);
        }

        List<Dictionary<string, string>>? entries = ResolveEntries(
            roleAfterEscape,
            $"InternalTeam {playerTeamKey}",
            $"IT {playerRoleKey}",
            $"InternalFaction {playerFactionKey}",
            $"IF {playerFactionKey}",
            playerRoleKey,
            "all");
        
        if (UCR.TryGetSummonedCustomRole(player, out object summonedPlayer))
        {
            int? customRoleId = UCR.GetSummonedCustomRoleId(summonedPlayer);
            if (customRoleId is not null)
            {
                LogManager.Debug($"Player {player.PlayerId} has custom role {customRoleId}, checking for specific escape config...");
                List<Dictionary<string, string>>? customEntries = ResolveEntries(
                    roleAfterEscape,
                    $"CustomRole {customRoleId}",
                    $"CR {customRoleId}",
                    "all");
                if (customEntries is not null)
                {
                    LogManager.Debug($"Found {customEntries.Count} RoleAfterEscape entries for custom role '{customRoleId}'.");
                    entries = customEntries;
                }
            }
        }

        if (entries is null)
        {
            LogManager.Debug($"No RoleAfterEscape entries found for role '{playerRoleKey}'. Allowing natural escape.");
            return new KeyValuePair<bool, object?>(false, null);
        }
        
        LogManager.Debug($"Found {entries.Count} RoleAfterEscape entries for role '{playerRoleKey}'.");

        Dictionary<Team, KeyValuePair<bool, object?>?> asCuffedByInternalTeam = new();
        Dictionary<Faction, KeyValuePair<bool, object?>?> asCuffedByInternalFaction = new();
        Dictionary<RoleTypeId, KeyValuePair<bool, object?>?> asCuffedByInternalRole = new();
        // Dictionary<uint, KeyValuePair<bool, object?>?> asCuffedByCustomTeam = new(); we will add the support to UCT and UIU-RS
        Dictionary<int, KeyValuePair<bool, object?>?> asCuffedByCustomRole = new();

        KeyValuePair<bool, object?>? defaultValue = new KeyValuePair<bool, object?>(false, null);
        KeyValuePair<bool, object?>? defaultCuffedValue = new KeyValuePair<bool, object?>(false, null);

        // Flatten and parse all condition/value pairs for this role
        foreach (Dictionary<string, string> dict in entries)
        {
            LogManager.Debug($"Parsing RoleAfterEscape entry with {dict.Count} conditions.");
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                KeyValuePair<bool, object?>? data = ParseEscapeString(kvp.Value);
                if (kvp.Key is "default")
                {
                    defaultValue = data;
                    LogManager.Debug(
                        $"Set default escape outcome for role '{playerRoleKey}' to: {(data is null ? "Deny" : data.Value.Key ? $"CustomRole {data.Value.Value}" : $"InternalRole {data.Value.Value}")}");
                }
                else
                {
                    List<string> elements = kvp.Key.Split(' ').ToList();

                    if (elements.Count != 4 || elements[0] is not "cuffed" || elements[1] is not "by")
                    {
                        LogManager.Warn(
                            $"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {elements.Count} args!\nSource: {kvp.Key}");
                        return new KeyValuePair<bool, object?>(false, null);
                    }

                    LogManager.Debug($"Parsing escape condition: {kvp.Key} -> {kvp.Value}");

                    switch (elements[2])
                    {
                        case "InternalFaction" or "IF" when Enum.TryParse(elements[3], out Faction faction):
                            asCuffedByInternalFaction.TryAdd(faction, data);
                            break;
                        case "InternalTeam" or "IT" when Enum.TryParse(elements[3], out Team team):
                            asCuffedByInternalTeam.TryAdd(team, data);
                            break;
                        case "InternalRole" or "IR" when Enum.TryParse(elements[3], out RoleTypeId id):
                            asCuffedByInternalRole.TryAdd(id, data);
                            break;
                        case "CustomRole" or "CR"
                            when int.TryParse(elements[3], out int id) && UCR.TryGetCustomRole(id, out _):
                            asCuffedByCustomRole.TryAdd(id, data);
                            break;
                        case "all" or "ALL":
                            defaultCuffedValue = data;
                            break;
                        default:
                        {
                            bool okInt = int.TryParse(elements[3], out _);
                            LogManager.Warn(
                                $"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / InternalFaction / IF / CustomRole / CR. Found: {elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {elements[3]} (int32 parsable: {okInt})");
                            break;
                        }
                    }
                }
            }
        }

        // Now let's assign
        if (!player.IsDisarmed)
            return defaultValue;
        LogManager.Debug($"Player {player.PlayerId} is disarmed by {player.DisarmedBy?.Team} - {player.DisarmedBy?.Role}");
        if (player is { IsDisarmed: true, DisarmedBy: not null })
        {
            // Try custom role via reflection first
            if (UCR.TryGetSummonedCustomRole(player.DisarmedBy, out object summoned))
            {
                int? customRoleId = UCR.GetSummonedCustomRoleId(summoned);
                if (customRoleId is not null && asCuffedByCustomRole.TryGetValue(customRoleId.Value, out KeyValuePair<bool, object?>? escapeRole) && escapeRole is not null)
                {
                    LogManager.Debug($"Player {player.PlayerId} disarmed by custom role {customRoleId}, applying mapped escape outcome.");
                    return escapeRole;
                }
            }
            
            // Then try internal role
            if (asCuffedByInternalRole.TryGetValue(player.DisarmedBy.Role, out KeyValuePair<bool, object?>? roleValue) && roleValue is not null)
                return roleValue;

            if (asCuffedByInternalTeam.TryGetValue(player.DisarmedBy.Team, out KeyValuePair<bool, object?>? teamValue) && teamValue is not null)
                return teamValue;
            
            if (asCuffedByInternalFaction.TryGetValue(player.DisarmedBy.Faction, out KeyValuePair<bool, object?>? factionValue) && factionValue is not null)
                return factionValue;

            if (defaultCuffedValue is not null)
            {
                LogManager.Debug($"Applying default 'cuffed by all' escape outcome for player {player.PlayerId}.");
                return defaultCuffedValue;
            }
        }

        LogManager.Debug(
            $"Returing default type for escaping evaluation of player {player.PlayerId} who's cuffed by team: {player.DisarmedBy?.Team} faction: {player.DisarmedBy?.Faction} role: {player.DisarmedBy?.Role}");
        return defaultValue;
        
        // Local function to resolve entries with case-insensitive keys
        List<Dictionary<string, string>>? ResolveEntries(
            Dictionary<string, List<Dictionary<string, string>>> source,
            params string[] keys)
        {
            foreach (string key in keys)
            {
                LogManager.Debug($"Attempting to resolve entries for key: {key}");
                if (source.TryGetValue(key, out List<Dictionary<string, string>>? value))
                    return value;

                string? ciMatch = source.Keys.FirstOrDefault(k =>
                    string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

                if (ciMatch is not null)
                    return source[ciMatch];
            }
            return null;
        }
    }

    private static KeyValuePair<bool, object?>? ParseEscapeString(string escape)
    {
        if (escape is "Deny" or "deny" or "DENY")
            return null;

        List<string> elements = escape.Split(' ').ToList();
        if (elements.Count != 2)
        {
            LogManager.Warn(
                $"Failed to parse an EscapeString[value]: syntax should be <source> <id> (2 args), found {elements.Count} args!\nSource: {escape}");
            return new KeyValuePair<bool, object?>(false, RoleTypeId.Spectator);
        }

        switch (elements[0])
        {
            case "CustomRole":
            case "CR":
                return new KeyValuePair<bool, object?>(true, int.Parse(elements[1]));
            case "InternalRole" or "IR" when Enum.TryParse(elements[1], out RoleTypeId role):
                return new KeyValuePair<bool, object?>(false, role);
        }

        bool okInt = int.TryParse(elements[1], out _);
        LogManager.Warn(
            $"Function SpawnManager::ParseEscapeString(string escape) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalRole / IR / CustomRole / CR. Found: {elements[0]}\n- The target is not a CustomRole / InternalRole. Found: {elements[1]} (int32 parsable: {okInt})");

        return new KeyValuePair<bool, object?>(false, RoleTypeId.Spectator);
    }
}