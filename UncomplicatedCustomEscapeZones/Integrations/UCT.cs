using System;
using System.Reflection;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Integrations;

internal static class UCT
{
    private const string PluginName = "UncomplicatedCustomTeams";

    private const string TeamExtensionsType = "UncomplicatedCustomTeams.API.TeamExtensions";

    private const string SummonedTeamType = "UncomplicatedCustomTeams.API.Features.Runtime.SummonedTeam";

    private const string TeamDefinitionType = "UncomplicatedCustomTeams.API.Features.Definitions.Team";

    private static MethodInfo GetCustomTeamMethod =>
        DynamicInvoke.GetMethod(PluginName, $"{TeamExtensionsType}.GetCustomTeam", true, 1);

    private static MethodInfo DefinitionGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{SummonedTeamType}.Definition_get", true);

    private static MethodInfo DefinitionIdGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{TeamDefinitionType}.Id_get", true);

    private static MethodInfo DefinitionNameGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{TeamDefinitionType}.Name_get", true);

    public static bool Available => GetCustomTeamMethod is not null;

    internal static bool TryGetCustomTeam(Player player, out uint id, out string name)
    {
        id = 0;
        name = null;

        if (player is null)
            return false;

        MethodInfo getCustomTeam = GetCustomTeamMethod;

        if (getCustomTeam is null)
            return false;

        try
        {
            object summonedTeam = getCustomTeam.Invoke(null, [player]);

            if (summonedTeam is null)
                return false;

            object definition = DefinitionGetter?.Invoke(summonedTeam, null);

            if (definition is null)
            {
                LogManager.Debug("Failed to obtain SummonedTeam.Definition of a UCT Custom Team.");
                return false;
            }

            object idValue = DefinitionIdGetter?.Invoke(definition, null);

            if (idValue is null)
            {
                LogManager.Debug("Failed to obtain the Id of a UCT Custom Team.");
                return false;
            }

            id = Convert.ToUInt32(idValue);
            name = DefinitionNameGetter?.Invoke(definition, null) as string;

            LogManager.Debug(
                $"Player {player.PlayerId} is a member of the UCT Custom Team {id} '{name ?? "unnamed"}'.");
            return true;
        }
        catch (Exception e)
        {
            LogManager.Debug($"Reflection failed to obtain the UCT Custom Team of {player.PlayerId}: {e.Message}");
            return false;
        }
    }
}