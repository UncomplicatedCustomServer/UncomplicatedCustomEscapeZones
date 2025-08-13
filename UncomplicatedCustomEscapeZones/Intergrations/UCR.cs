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
            LogManager.Debug($"{CustomRole} or {SummonedCustomRole} is not found. Aborting UCR integration...");
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

        if (SummonedCustomRole?.GetProperty("List", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) is not
            IEnumerable list)
            return false;

        foreach (object scr in list)
        {
            object scrPlayer = scr.GetType().GetProperty("Player")?.GetValue(scr);
            PropertyInfo playerIdProp = scrPlayer?.GetType().GetProperty("PlayerId");
            if (playerIdProp != null && playerIdProp.GetValue(scrPlayer)?.Equals(player.PlayerId) == true)
            {
                summonedCustomRole = scr;
                return true;
            }
        }

        return false;
    }
}