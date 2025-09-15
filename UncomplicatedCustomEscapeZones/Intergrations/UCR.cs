using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using LabApi.Features.Wrappers;
using LabApi.Loader;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Intergrations;

internal class UCR
{
    public static Assembly Assembly =>
        PluginLoader.Plugins.FirstOrDefault(p => p.Key.Name is "UncomplicatedCustomRoles").Value;

    public static Type CustomRole => Assembly?.GetType("UncomplicatedCustomRoles.API.Features.CustomRole");

    public static Type SummonedCustomRole =>
        Assembly?.GetType("UncomplicatedCustomRoles.API.Features.SummonedCustomRole");

    public static bool Available => CustomRole is not null && SummonedCustomRole is not null;

    public static bool TryGetCustomRole(int id, out object customRole)
    {
        customRole = null;

        if (!Available)
        {
            LogManager.Warn(
                "UncomplicatedCustomRoles is not found. Please install it or change the EscapeZone configs.");
            return false;
        }


        LogManager.Debug($"UCR found, trying check if the role {id} exists...");

        try
        {
            MethodInfo tryGetCustomRole = CustomRole.GetMethod("TryGet", BindingFlags.Public | BindingFlags.Static);
            if (tryGetCustomRole is null) return false;
            object[] parameters = [id, null];
            bool success = (bool)tryGetCustomRole.Invoke(null, parameters);

            if (!success) return false;
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
        MethodInfo giveCustomRole = SummonedCustomRole.GetMethod("Summon", BindingFlags.Public | BindingFlags.Static);

        if (!Available)
        {
            LogManager.Debug($"{CustomRole} or {SummonedCustomRole} is not found. Aborting UCR integration...");
            return;
        }

        LogManager.Debug($"UCR role found, trying to give the role {id} to {player}");

        try
        {
            if (!TryGetCustomRole(id, out object customRole) || customRole is null) return;
            if (giveCustomRole != null)
                giveCustomRole.Invoke(null, [player, customRole]);
        }
        catch (Exception e)
        {
            LogManager.Error($"{e.Message}\n{e.HResult}");
        }
    }

    public static bool TryGetSummonedCustomRole(Player player, out object summonedCustomRole)
    {
        summonedCustomRole = null;

        if (!Available)
        {
            LogManager.Debug($"{CustomRole} or {SummonedCustomRole} is not found. Aborting UCR integration...");
            return false;
        }

        object listObj = SummonedCustomRole?.GetProperty("List", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

        if (listObj is not IEnumerable list)
            return false;

        object[] entries = list.Cast<object>().ToArray();
        LogManager.Debug($"Found SummonedCustomRole.List with {entries.Length} entries.");

        foreach (object entry in entries)
        {
            object candidate = entry;
            PropertyInfo valueProp = entry.GetType().GetProperty("Value");
            if (valueProp != null)
            {
                object value = valueProp.GetValue(entry);
                if (value != null)
                    candidate = value;
            }

            LogManager.Debug($"Examining entry candidate: {candidate}");

            object scrPlayer = candidate.GetType().GetProperty("Player")?.GetValue(candidate);
            LogManager.Debug($"Player property value: {scrPlayer}");
            if (scrPlayer is null)
                continue;

            PropertyInfo playerIdProp = scrPlayer.GetType().GetProperty("PlayerId");
            LogManager.Debug($"PlayerId property: {playerIdProp}");
            object idObj = playerIdProp?.GetValue(scrPlayer);
            LogManager.Debug($"Found PlayerId value: {idObj}");

            if (idObj is not int foundId || foundId != player.PlayerId) continue;
            summonedCustomRole = candidate;
            LogManager.Debug($"Matched SummonedCustomRole for PlayerId {player.PlayerId}: {candidate}");
            return true;
        }

        return false;
    }

    internal static int? GetSummonedCustomRoleId(object summoned)
    {
        try
        {
            if (summoned is null)
                return null;
            LogManager.Debug($"Trying to obtain SummonedCustomRole.Id via reflection from {summoned}");
            Type t = summoned.GetType();
            PropertyInfo roleProp = t.GetProperty("Role", BindingFlags.Public | BindingFlags.Instance);
            LogManager.Debug($"Found Role property: {roleProp}");
            object roleObject = roleProp?.GetValue(summoned);
            if (roleObject is null)
                return null;
            LogManager.Debug($"Found Role object: {roleObject}");

            PropertyInfo idProp = roleObject.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            LogManager.Debug($"Found Id property: {idProp}");
            object idValue = idProp?.GetValue(roleObject);
            LogManager.Debug($"Found Id value: {idValue}");
            if (idValue is int id)
                return id;
            LogManager.Debug("Id value is not an integer.");
        }
        catch (Exception e)
        {
            LogManager.Debug($"Reflection failed to obtain SummonedCustomRole.Id: {e.Message}");
        }
        return null;
    }
}