using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LabApi.Loader;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Integrations;

public static class DynamicInvoke
{
    private static readonly Dictionary<string, MethodInfo> Methods = new();

    private static readonly Dictionary<string, Type> Types = new();

    private static readonly Dictionary<string, Assembly> Assemblies = new();

    public static MethodInfo GetMethod(string plugin, string address, bool isLabapi = false, int methodCounter = -1,
        string[] requiredParamNames = null)
    {
        if (Methods.TryGetValue(address, out MethodInfo method))
            return method;

        if (!Assemblies.TryGetValue(plugin, out Assembly assembly))
        {
            assembly = isLabapi ? GetLabAPIAssembly(plugin) : GetExiledAssembly(plugin);

            if (assembly is not null)
                Assemblies.Add(plugin, assembly);
        }

        if (assembly is null)
            return null;

        string argument = address.Split('.').Last();
        string stringType = address.Replace($".{argument}", string.Empty);

        if (!Types.TryGetValue(stringType, out Type type))
        {
            type = assembly.GetType(stringType);
            Types.Add(stringType, type);
        }

        if (type is null)
        {
            LogManager.Warn($"[DynamicInvoke] Failed to locate type {stringType} in assembly {assembly.FullName}!");
            return null;
        }

        if (argument.Contains('_'))
        {
            string stringProperty = argument.Split('_')[0]; // Cannot be null
            PropertyInfo property = type.GetProperty(stringProperty);
            MethodInfo resultMethod;

            if (property is null)
            {
                LogManager.Warn(
                    $"[DynamicInvoke] Failed to locate property {stringProperty} in type {stringType} in assembly {assembly.FullName}!");
                return null;
            }

            resultMethod = argument.EndsWith("_get") ? property.GetGetMethod() : property.GetSetMethod();

            if (resultMethod is null)
            {
                LogManager.Warn(
                    $"[DynamicInvoke] Failed to locate method _get() or _set() in property {stringProperty} in type {stringType} in assembly {assembly.FullName}!");
                return null;
            }

            Methods.Add(address, resultMethod);
            return resultMethod;
        }
        else // Normal method
        {
            IEnumerable<MethodInfo> resultMethods = type.GetMethods().Where(m => m.Name == argument);
            MethodInfo resultMethod;

            if (methodCounter != -1 || (requiredParamNames is not null && requiredParamNames.Length > 0))
            {
                IEnumerable<MethodInfo> filtered = resultMethods;

                if (methodCounter != -1)
                    filtered = filtered.Where(m => m.GetParameters().Length == methodCounter);

                if (requiredParamNames is not null && requiredParamNames.Length > 0)
                    filtered = filtered.Where(m =>
                    {
                        string[] paramNames = m.GetParameters().Select(p => p.Name).ToArray();
                        return requiredParamNames.All(rpn =>
                            paramNames.Contains(rpn, StringComparer.OrdinalIgnoreCase));
                    });

                resultMethod = filtered.FirstOrDefault();
            }
            else
            {
                resultMethod = resultMethods.FirstOrDefault();
            }

            if (resultMethod is null)
            {
                LogManager.Warn(
                    $"[DynamicInvoke] Failed to locate method {argument} in type {stringType} in assembly {assembly.FullName}!");
                return null;
            }

            Methods.Add(address, resultMethod);
            return resultMethod;
        }
    }

    public static Type GetType(string plugin, string address, bool isLabapi = false)
    {
        if (Types.TryGetValue(address, out Type type))
            return type;

        if (!Assemblies.TryGetValue(plugin, out Assembly assembly))
        {
            assembly = isLabapi ? GetLabAPIAssembly(plugin) : GetExiledAssembly(plugin);

            if (assembly is not null)
                Assemblies.Add(plugin, assembly);
        }

        if (assembly is null)
            return null;

        type = assembly.GetType(address);
        Types.Add(address, type);

        return type;
    }

    internal static Assembly GetLabAPIAssembly(string pluginName)
    {
        try
        {
            KeyValuePair<LabApi.Loader.Features.Plugins.Plugin, Assembly>? plugin =
                PluginLoader.Plugins.FirstOrDefault(p =>
                    p.Key.Name.Contains(pluginName, StringComparison.CurrentCultureIgnoreCase));

            if (plugin is not null)
                return plugin.Value.Value;

            return null;
        }
        catch (Exception e)
        {
            LogManager.Error(e.ToString());
            return null;
        }
    }

    internal static Assembly GetExiledAssembly(string pluginName)
    {
        try
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p =>
                p.FullName.Contains(pluginName, StringComparison.CurrentCultureIgnoreCase));
            return assembly;
        }
        catch (Exception e)
        {
            LogManager.Error(e.ToString());
            return null;
        }
    }
}