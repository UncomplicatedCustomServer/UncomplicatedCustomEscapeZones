using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Integrations;

internal static class UCR
{
    private const string PluginName = "UncomplicatedCustomRoles";

    private const string CustomRoleType = "UncomplicatedCustomRoles.API.Features.CustomRole";

    private const string CustomRoleInterface = "UncomplicatedCustomRoles.API.Interfaces.ICustomRole";

    private const string SummonedCustomRoleType = "UncomplicatedCustomRoles.API.Features.SummonedCustomRole";

    private const string CustomModuleType = "UncomplicatedCustomRoles.API.Features.CustomModules.CustomModule";

    private const string DisguiseTeamType = "UncomplicatedCustomRoles.API.Features.DisguiseTeam";

    private const string NotFound =
        "UncomplicatedCustomRoles is not found. Please install it or change the EscapeZone configs.";

    private static MethodInfo TryGetCustomRoleMethod => DynamicInvoke.GetMethod(PluginName, $"{CustomRoleType}.TryGet",
        true, requiredParamNames: ["id", "customRole"]);

    private static MethodInfo SummonMethod =>
        DynamicInvoke.GetMethod(PluginName, $"{SummonedCustomRoleType}.Summon", true, 2);

    private static MethodInfo SummonedListGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{SummonedCustomRoleType}.List_get", true);

    private static MethodInfo SummonedPlayerGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{SummonedCustomRoleType}.Player_get", true);

    private static MethodInfo SummonedRoleGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{SummonedCustomRoleType}.Role_get", true);

    private static MethodInfo SummonedModulesGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{SummonedCustomRoleType}.CustomModules_get", true);

    private static MethodInfo CustomRoleIdGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{CustomRoleInterface}.Id_get", true);

    private static MethodInfo ModuleStringArgsGetter =>
        DynamicInvoke.GetMethod(PluginName, $"{CustomModuleType}.StringArgs_get", true);

    public static bool Available => TryGetCustomRoleMethod is not null && SummonedListGetter is not null;

    public static bool TryGetCustomRole(int id, out object customRole)
    {
        customRole = null;

        MethodInfo tryGet = TryGetCustomRoleMethod;

        if (tryGet is null)
        {
            LogManager.Warn(NotFound);
            return false;
        }

        LogManager.Debug($"UCR found, trying check if the role {id} exists...");

        try
        {
            object[] parameters = [id, null];

            if (tryGet.Invoke(null, parameters) is not true)
                return false;

            customRole = parameters[1];

            LogManager.Debug($"returning {customRole}");
            return customRole is not null;
        }
        catch (Exception e)
        {
            LogManager.Error($"{e.Message}\n{e.HResult}");
            return false;
        }
    }

    public static void GiveCustomRole(int id, Player player)
    {
        MethodInfo summon = SummonMethod;

        if (summon is null)
        {
            LogManager.Debug($"{SummonedCustomRoleType}.Summon() is not found. Aborting UCR integration...");
            return;
        }

        LogManager.Debug($"UCR role found, trying to give the role {id} to {player}");

        try
        {
            if (!TryGetCustomRole(id, out object customRole) || customRole is null) return;
            summon.Invoke(null, [player, customRole]);
        }
        catch (Exception e)
        {
            LogManager.Error($"{e.Message}\n{e.HResult}");
        }
    }

    public static bool TryGetSummonedCustomRole(Player player, out object summonedCustomRole)
    {
        summonedCustomRole = null;

        if (player is null)
            return false;

        if (!Available)
        {
            LogManager.Debug(NotFound);
            return false;
        }

        MethodInfo playerGetter = SummonedPlayerGetter;

        if (playerGetter is null || SummonedListGetter?.Invoke(null, null) is not IEnumerable list)
            return false;

        try
        {
            foreach (object entry in list)
            {
                object candidate = entry?.GetType().GetProperty("Value")?.GetValue(entry) ?? entry;

                if (candidate is null)
                    continue;

                if (playerGetter.Invoke(candidate, null) is not Player scrPlayer ||
                    scrPlayer.PlayerId != player.PlayerId)
                    continue;

                summonedCustomRole = candidate;
                LogManager.Debug($"Matched SummonedCustomRole for PlayerId {player.PlayerId}: {candidate}");
                return true;
            }
        }
        catch (Exception e)
        {
            LogManager.Debug($"Reflection failed to obtain the SummonedCustomRole of {player.PlayerId}: {e.Message}");
        }

        return false;
    }

    internal static bool TryGetFakeTeam(Player player, out Team team)
    {
        team = default;

        if (player is null)
            return false;

        try
        {
            Type disguiseTeam = DynamicInvoke.GetType(PluginName, DisguiseTeamType, true);

            if (disguiseTeam?.GetField("List", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) is not
                IDictionary<int, Team> list)
                return false;

            if (!list.TryGetValue(player.PlayerId, out Team fakeTeam))
                return false;

            team = fakeTeam;
            return true;
        }
        catch (Exception e)
        {
            LogManager.Debug($"Reflection failed to obtain the faked team of {player.PlayerId}: {e.Message}");
            return false;
        }
    }

    internal static string GetSummonedCustomTeam(object summoned)
    {
        if (summoned is null)
            return null;

        try
        {
            if (SummonedModulesGetter?.Invoke(summoned, null) is not IEnumerable modules)
                return null;

            MethodInfo stringArgsGetter = ModuleStringArgsGetter;

            if (stringArgsGetter is null)
                return null;

            foreach (object module in modules)
            {
                if (module is null || module.GetType().Name is not "CustomTeam")
                    continue;

                LogManager.Debug($"Found the CustomTeam module on {summoned}, reading the team name...");

                if (stringArgsGetter.Invoke(module, null) is IDictionary<string, string> args &&
                    args.TryGetValue("team", out string team) && !string.IsNullOrWhiteSpace(team))
                    return team.Trim();

                LogManager.Debug("The CustomTeam module doesn't have a valid 'team' argument.");
            }
        }
        catch (Exception e)
        {
            LogManager.Debug($"Reflection failed to obtain the CustomTeam of a SummonedCustomRole: {e.Message}");
        }

        return null;
    }

    internal static int? GetSummonedCustomRoleId(object summoned)
    {
        if (summoned is null)
            return null;

        try
        {
            LogManager.Debug($"Trying to obtain SummonedCustomRole.Id via reflection from {summoned}");

            object role = SummonedRoleGetter?.Invoke(summoned, null);

            if (role is null)
                return null;

            LogManager.Debug($"Found Role object: {role}");

            object id = CustomRoleIdGetter?.Invoke(role, null);
            LogManager.Debug($"Found Id value: {id}");

            if (id is int value)
                return value;

            LogManager.Debug("Id value is not an integer.");
        }
        catch (Exception e)
        {
            LogManager.Debug($"Reflection failed to obtain SummonedCustomRole.Id: {e.Message}");
        }

        return null;
    }
}