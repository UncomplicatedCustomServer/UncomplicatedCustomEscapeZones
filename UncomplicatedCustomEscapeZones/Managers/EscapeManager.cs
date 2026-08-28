#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Integrations;

namespace UncomplicatedEscapeZones.Managers;

public static class EscapeManager
{
    public static KeyValuePair<bool, object?>? ParseEscapeRole(
        Dictionary<string, List<Dictionary<string, string>>> roleAfterEscape, Player player)
    {
        // Determine which role-specific configuration applies to this player
        Team playerTeam = player.Team;
        Faction playerFaction = player.Faction;

        // A UCR Custom Role can fake its team: if it does, the faked one is what everyone sees, so it's the one
        // the escape has to be evaluated with
        if (UCR.TryGetFakeTeam(player, out Team fakeTeam) && fakeTeam != playerTeam)
        {
            LogManager.Debug(
                $"Player {player.PlayerId} is faking the team {fakeTeam} (real one: {playerTeam}), using the faked one.");
            playerTeam = fakeTeam;
            playerFaction = fakeTeam.GetFaction();
        }

        string playerRoleKey = player.Role.ToString();
        string playerTeamKey = playerTeam.ToString();
        string playerFactionKey = playerFaction.ToString();

        LogManager.Debug($"Player Role: {playerRoleKey}");
        LogManager.Debug($"Player Team: {playerTeamKey}");
        LogManager.Debug($"Player Faction: {playerFactionKey}");

        if (string.IsNullOrWhiteSpace(playerRoleKey) || string.IsNullOrWhiteSpace(playerTeamKey) ||
            string.IsNullOrWhiteSpace(playerFactionKey))
        {
            LogManager.Warn(
                $"Unable to determine player's role or team for escape evaluation (PlayerId={player.PlayerId}). Allowing natural escape.");
            return new KeyValuePair<bool, object?>(false, null);
        }

        List<Dictionary<string, string>>? entries = ResolveEntries(
            roleAfterEscape,
            $"InternalTeam {playerTeamKey}",
            $"IT {playerTeamKey}",
            $"InternalFaction {playerFactionKey}",
            $"IF {playerFactionKey}",
            $"InternalRole {playerRoleKey}",
            $"IR {playerRoleKey}",
            "all");

        List<string> customKeys = [];

        if (UCR.TryGetSummonedCustomRole(player, out object summonedPlayer))
        {
            int? customRoleId = UCR.GetSummonedCustomRoleId(summonedPlayer);

            if (customRoleId is not null)
                customKeys.AddRange([$"CustomRole {customRoleId}", $"CR {customRoleId}"]);
        }

        List<string> customTeams = GetCustomTeams(player, summonedPlayer);

        foreach (string customTeam in customTeams)
            customKeys.AddRange([$"CustomTeam {customTeam}", $"CT {customTeam}"]);

        if (customKeys.Count > 0)
        {
            // 'all' is not part of the lookup: the resolution above already fell back to it, so asking for it
            // again here would just throw away the more specific entries it found
            LogManager.Debug(
                $"Player {player.PlayerId} has the custom keys {string.Join(", ", customKeys)}, checking for a specific escape config...");

            List<Dictionary<string, string>>? customEntries = ResolveEntries(roleAfterEscape, customKeys.ToArray());
            if (customEntries is not null)
            {
                LogManager.Debug($"Found {customEntries.Count} RoleAfterEscape entries for a custom role / team.");
                entries = customEntries;
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
        // asCuffedByCustomTeam covers both the UCR CustomTeam module and the UCT teams, UIU-RS is still missing
        Dictionary<int, KeyValuePair<bool, object?>?> asCuffedByCustomRole = new();
        Dictionary<string, KeyValuePair<bool, object?>?> asCuffedByCustomTeam = new(StringComparer.OrdinalIgnoreCase);

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

                    switch (elements[2].ToLowerInvariant())
                    {
                        case "internalfaction" or "if":
                            if (Enum.TryParse(elements[3], true, out Faction faction))
                                asCuffedByInternalFaction.TryAdd(faction, data);
                            else
                                LogManager.Warn(
                                    $"Failed to parse faction '{elements[3]}' for escape condition '{kvp.Key}'.");
                            break;
                        case "internalteam" or "it":
                            if (Enum.TryParse(elements[3], true, out Team team))
                                asCuffedByInternalTeam.TryAdd(team, data);
                            else
                                LogManager.Warn(
                                    $"Failed to parse team '{elements[3]}' for escape condition '{kvp.Key}'.");
                            break;
                        case "internalrole" or "ir":
                            if (Enum.TryParse(elements[3], true, out RoleTypeId id))
                                asCuffedByInternalRole.TryAdd(id, data);
                            else
                                LogManager.Warn(
                                    $"Failed to parse role '{elements[3]}' for escape condition '{kvp.Key}'.");
                            break;
                        case "customrole" or "cr":
                            if (int.TryParse(elements[3], out int cid) && UCR.TryGetCustomRole(cid, out _))
                                asCuffedByCustomRole.TryAdd(cid, data);
                            else
                                LogManager.Warn(
                                    $"Failed to parse custom role id '{elements[3]}' for escape condition '{kvp.Key}'.");
                            break;
                        case "customteam" or "ct":
                            if (!string.IsNullOrWhiteSpace(elements[3]))
                                asCuffedByCustomTeam.TryAdd(elements[3], data);
                            else
                                LogManager.Warn(
                                    $"Failed to parse custom team '{elements[3]}' for escape condition '{kvp.Key}'.");
                            break;
                        case "all":
                            defaultCuffedValue = data;
                            break;
                        default:
                        {
                            bool okInt = int.TryParse(elements[3], out _);
                            LogManager.Warn(
                                $"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / InternalFaction / IF / InternalRole / IR / CustomRole / CR / CustomTeam / CT. Found: {elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {elements[3]} (int32 parsable: {okInt})");
                            break;
                        }
                    }
                }
            }
        }

        // Now let's assign
        if (!player.IsDisarmed)
            return defaultValue;
        LogManager.Debug(
            $"Player {player.PlayerId} is disarmed by {player.DisarmedBy?.Team} - {player.DisarmedBy?.Role}");
        if (player is { IsDisarmed: true, DisarmedBy: not null })
        {
            // Try custom role via reflection first
            if (UCR.TryGetSummonedCustomRole(player.DisarmedBy, out object summoned))
            {
                int? customRoleId = UCR.GetSummonedCustomRoleId(summoned);
                if (customRoleId is not null &&
                    asCuffedByCustomRole.TryGetValue(customRoleId.Value, out KeyValuePair<bool, object?>? escapeRole) &&
                    escapeRole is not null)
                {
                    LogManager.Debug(
                        $"Player {player.PlayerId} disarmed by custom role {customRoleId}, applying mapped escape outcome.");
                    return escapeRole;
                }
            }

            // Then the custom team of the disarmer, if it belongs to one
            foreach (string customTeam in GetCustomTeams(player.DisarmedBy, summoned))
                if (asCuffedByCustomTeam.TryGetValue(customTeam, out KeyValuePair<bool, object?>? customTeamValue) &&
                    customTeamValue is not null)
                {
                    LogManager.Debug(
                        $"Player {player.PlayerId} disarmed by the custom team '{customTeam}', applying mapped escape outcome.");
                    return customTeamValue;
                }

            // Then try internal role
            if (asCuffedByInternalRole.TryGetValue(player.DisarmedBy.Role,
                    out KeyValuePair<bool, object?>? roleValue) && roleValue is not null)
                return roleValue;

            // The disarmer can fake its team as well, so the faked one wins over the real one here too
            Team disarmerTeam = player.DisarmedBy.Team;
            Faction disarmerFaction = player.DisarmedBy.Faction;

            if (UCR.TryGetFakeTeam(player.DisarmedBy, out Team disarmerFakeTeam) && disarmerFakeTeam != disarmerTeam)
            {
                LogManager.Debug(
                    $"Player {player.DisarmedBy.PlayerId} is faking the team {disarmerFakeTeam} (real one: {disarmerTeam}), using the faked one.");
                disarmerTeam = disarmerFakeTeam;
                disarmerFaction = disarmerFakeTeam.GetFaction();
            }

            if (asCuffedByInternalTeam.TryGetValue(disarmerTeam,
                    out KeyValuePair<bool, object?>? teamValue) && teamValue is not null)
                return teamValue;

            if (asCuffedByInternalFaction.TryGetValue(disarmerFaction,
                    out KeyValuePair<bool, object?>? factionValue) && factionValue is not null)
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

    /// <summary>
    ///     Gets every identifier the given player can be matched with by a CustomTeam key: the team of the UCR
    ///     CustomTeam module and, if the player is inside an UCT Custom Team, its name and its Id
    /// </summary>
    /// <param name="player"></param>
    /// <param name="summoned">The SummonedCustomRole of the player, if it has one.</param>
    private static List<string> GetCustomTeams(Player player, object summoned)
    {
        List<string> teams = [];

        string module = UCR.GetSummonedCustomTeam(summoned);

        if (module is not null)
            teams.Add(module);

        if (!UCT.TryGetCustomTeam(player, out uint id, out string name))
            return teams;

        if (!string.IsNullOrWhiteSpace(name))
            teams.Add(name);

        teams.Add(id.ToString());

        return teams;
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