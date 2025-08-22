using System;
using System.IO;
using System.Linq;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Yaml;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Utilities;

internal static class FileConfigs
{
    private static readonly string Dir = Path.Combine(PathManager.Configs.FullName, "UncomplicatedCustomEscapeZones");

    private static bool Is(string localDir = "")
    {
        return Directory.Exists(Path.Combine(Dir, localDir));
    }

    private static string[] List(string localDir = "")
    {
        return Directory.GetFiles(Path.Combine(Dir, localDir));
    }

    public static void LoadAll(string localDir = "")
    {
        LoadAction(CustomEscapeZone.Register, localDir);
    }

    private static void LoadAction(Action<ICustomEscapeZone> action, string localDir = "")
    {
        foreach (string file in List(localDir))
            try
            {
                if (Directory.Exists(file))
                    continue;

                if (file.Split().First() == ".")
                    return;

                CustomEscapeZone zone =
                    YamlConfigParser.Deserializer.Deserialize<CustomEscapeZone>(File.ReadAllText(file));
                LogManager.Debug($"Proposed to the registerer the external zone ID: {zone.Id} from file: {file}");
                action(zone);
            }
            catch (Exception ex)
            {
                LogManager.Error($"Failed to parse {file}. YAML Exception: {ex.Message}");
            }
    }

    public static void Welcome(string localDir = "")
    {
        if (!Is(localDir))
        {
            Directory.CreateDirectory(Path.Combine(Dir, localDir));

            File.WriteAllText(Path.Combine(Dir, localDir, "default-zone.yml"),
                YamlConfigParser.Serializer.Serialize(new CustomEscapeZone()));

            LogManager.Info(
                $"Plugin does not have a escape zone folder, generated one in {Path.Combine(Dir, localDir)}");
        }
    }
}