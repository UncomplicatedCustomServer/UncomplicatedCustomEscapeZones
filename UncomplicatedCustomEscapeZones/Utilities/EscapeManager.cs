using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace UncomplicatedEscapeZones.Utilities;

public class EscapeManager
{
    public static KeyValuePair<bool, object>? ParseEscapeRole(Dictionary<string, string> roleAfterEscape, Player player)
        {
            Dictionary<Team, KeyValuePair<bool, object>?> AsCuffedByInternalTeam = new();
            // Dictionary<uint, KeyValuePair<bool, object>> AsCuffedByCustomTeam = new(); we will add the support to UCT and UIU-RS
            // cuffed by InternalTeam FoundationForces
            //   0     1       2             3           = 4
            Dictionary<int, KeyValuePair<bool, object>?> AsCuffedByCustomRole = new();
            KeyValuePair<bool, object>? Default = new(false, RoleTypeId.Spectator);

            foreach (KeyValuePair<string, string> kvp in roleAfterEscape)
            {
                KeyValuePair<bool, object>? Data = ParseEscapeString(kvp.Value);
                if (kvp.Key is "default")
                    Default = Data;
                else
                {
                    List<string> Elements = kvp.Key.Split(' ').ToList();

                    if (Elements.Count != 4)
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if (Elements[0] is not "cuffed")
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if (Elements[1] is not "by")
                    {
                        LogManager.Warn($"Failed to parse an EscapeRole[key]: syntax should be cuffed by <source> <id>, found {Elements.Count} args!\nSource: {kvp.Key}");
                        return new(false, RoleTypeId.Spectator);
                    }

                    if ((Elements[2] is "InternalTeam" || Elements[2] is "IT") && Enum.TryParse(Elements[3], out Team team))
                        AsCuffedByInternalTeam.TryAdd(team, Data);
                    //else if ((Elements[2] is "CustomRole" || Elements[2] is "CR") && int.TryParse(Elements[3], out int id) && CustomRole.CustomRoles.ContainsKey(id))
                    //    AsCuffedByCustomRole.TryAdd(id, Data);
                    else
                        LogManager.Warn($"Function SpawnManager::ParseEscapeRole[2](<...>) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalTeam / IT / CustomRole / CR. Found: {Elements[2]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[3]} (int32: {int.Parse(Elements[3])}");
                }
            }

            // Now let's assign
            if (!player.IsDisarmed)
                return Default;
            else if (player.IsDisarmed && player.DisarmedBy is not null)
                //if (player.DisarmedBy.TryGetSummonedInstance(out SummonedCustomRole role) && AsCuffedByCustomRole.ContainsKey(role.Role.Id))
                //    return AsCuffedByCustomRole[role.Role.Id];
                //else
                if (AsCuffedByInternalTeam.ContainsKey(player.DisarmedBy.Team))
                    return AsCuffedByInternalTeam[player.DisarmedBy.Team];

            LogManager.Debug($"Returing default type for escaping evaluation of player {player.PlayerId} who's cuffed by {player.DisarmedBy?.Team}");
            return Default;
        }

        public static KeyValuePair<bool, object>? ParseEscapeString(string escape)
        {
            if (escape is "Deny" or "deny" or "DENY")
                return null;

            List<string> Elements = escape.Split(' ').ToList();
            if (Elements.Count != 2)
            {
                LogManager.Warn($"Failed to parse an EscapeString[value]: syntax should be <source> <id> (2 args), found {Elements.Count} args!\nSource: {escape}");
                return new(false, RoleTypeId.Spectator);
            }

            if (Elements[0] is "CustomRole" || Elements[0] is "CR")
                return new(true, int.Parse(Elements[1]));
            else if ((Elements[0] is "InternalRole" || Elements[0] is "IR") && Enum.TryParse(Elements[1], out RoleTypeId role))
                return new(false, role);
            else
                LogManager.Warn($"Function SpawnManager::ParseEscapeString(string escape) failed!\nPossible causes can be:\n- The source is not valid. Allowed: InternalRole / IR / CustomRole / CR. Found: {Elements[0]}\n- The target is not a CustomRole / InternalRole. Found: {Elements[1]} (int32: {int.Parse(Elements[1])}");

            return new(false, RoleTypeId.Spectator);
        }
}