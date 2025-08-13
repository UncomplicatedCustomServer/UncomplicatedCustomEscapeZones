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
        LogManager.Debug(playerRoleKey);
        if (string.IsNullOrWhiteSpace(playerRoleKey))
        {
            LogManager.Warn(
                $"Unable to determine player's role for escape evaluation (PlayerId={player.PlayerId}). Allowing natural escape.");
            return new KeyValuePair<bool, object?>(false, null);
        }

        // Try exact match first, then case-insensitive match as fallback
        if (!roleAfterEscape.TryGetValue(playerRoleKey, out List<Dictionary<string, string>>? entries))
            foreach (string? key in roleAfterEscape.Keys.Where(key =>
                         string.Equals(key, playerRoleKey, StringComparison.OrdinalIgnoreCase)))
            {
                entries = roleAfterEscape[key];
                break;
            }

        if (entries is null)
        {
            LogManager.Debug($"No RoleAfterEscape entries found for role '{playerRoleKey}'. Allowing natural escape.");
            return new KeyValuePair<bool, object?>(false, null);
        }

        Dictionary<Team, KeyValuePair<bool, object?>?> asCuffedByInternalTeam = new();
        // Dictionary<uint, KeyValuePair<bool, object?>?> asCuffedByCustomTeam = new(); we will add the support to UCT and UIU-RS
        Dictionary<int, KeyValuePair<bool, object?>?> asCuffedByCustomRole = new();

        KeyValuePair<bool, object?>? defaultValue = new KeyValuePair<bool, object?>(false, null);

        // Flatten and parse all condition/value pairs for this role
        foreach (Dictionary<string, string> dict in entries)
        foreach (KeyValuePair<string, string> kvp in dict)
        {
            KeyValuePair<bool, object?>? data = ParseEscapeString(kvp.Value);
            if (kvp.Key is "default")
            {
                defaultValue = data;
            }
            else
            {
                List<string> elements = kvp.Key.Split(' ').ToList();

                if (elements.Count != 4 || elements[0] is not "cuffed" || elements[1] is not "by")
                {
                    LogManager.Warn(
                        $"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {elements.Count} args!\nSource: {kvp.Key}");
                    return new KeyValuePair<bool, object?>(false, RoleTypeId.Spectator);
                }

                switch (elements[2])
                {
                    case "InternalTeam" when Enum.TryParse(elements[3], out Team team):
                        asCuffedByInternalTeam.TryAdd(team, data);
                        break;
                    case "CustomRole" when int.TryParse(elements[3], out int id) && UCR.TryGetCustomRole(id, out _):
                        asCuffedByCustomRole.TryAdd(id, data);
                        break;
                    default:
                    {
                        bool okInt = int.TryParse(elements[3], out _);
                        LogManager.Warn(
                            $"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / CustomRole / CR. Found: {elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {elements[3]} (int32 parsable: {okInt})");
                        break;
                    }
                }
            }
        }

        // Now let's assign
        if (!player.IsDisarmed)
            return defaultValue;
        if (player.IsDisarmed && player.DisarmedBy is not null)
            //if (player.DisarmedBy.TryGetSummonedInstance(out SummonedCustomRole role) && asCuffedByCustomRole.ContainsKey(role.Role.Id))
            //    return asCuffedByCustomRole[role.Role.Id];
            //else
            if (asCuffedByInternalTeam.ContainsKey(player.DisarmedBy.Team))
                return asCuffedByInternalTeam[player.DisarmedBy.Team];

        LogManager.Debug(
            $"Returing default type for escaping evaluation of player {player.PlayerId} who's cuffed by {player.DisarmedBy?.Team}");
        return defaultValue;
    }

    public static KeyValuePair<bool, object?>? ParseEscapeString(string escape)
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

        if (elements[0] is "CustomRole" || elements[0] is "CR")
            return new KeyValuePair<bool, object?>(true, int.Parse(elements[1]));
        if ((elements[0] is "InternalRole" || elements[0] is "IR") && Enum.TryParse(elements[1], out RoleTypeId role))
            return new KeyValuePair<bool, object?>(false, role);
        bool okInt = int.TryParse(elements[1], out _);
        LogManager.Warn(
            $"Function SpawnManager::ParseEscapeString(string escape) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalRole / IR / CustomRole / CR. Found: {elements[0]}\n- The target is not a CustomRole / InternalRole. Found: {elements[1]} (int32 parsable: {okInt})");

        return new KeyValuePair<bool, object?>(false, RoleTypeId.Spectator);
    }
}