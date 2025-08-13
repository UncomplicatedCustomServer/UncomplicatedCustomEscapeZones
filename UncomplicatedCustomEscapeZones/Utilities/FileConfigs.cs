using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Yaml;
using UncomplicatedEscapeZones.API.Features;
using UnityEngine;
using YamlDotNet.Core;

namespace UncomplicatedEscapeZones.Utilities
{
    internal class FileConfigs
    {
        internal string Dir = Path.Combine(PathManager.Configs.FullName, "UncomplicatedCustomEscapeZones");

        public bool Is(string localDir = "")
        {
            return Directory.Exists(Path.Combine(Dir, localDir));
        }

        public string[] List(string localDir = "")
        {
            return Directory.GetFiles(Path.Combine(Dir, localDir));
        }

        public void LoadAll(string localDir = "")
        {
            LoadAction(CustomEscapeZone.List.Add, localDir);
        }

        public void LoadAction(Action<CustomEscapeZone> action, string localDir = "")
        {
            CustomEscapeZone.List.Clear();
            foreach (string file in List(localDir))
            {
                try
                {
                    if (Directory.Exists(file))
                        continue;

                    if (file.Split().First() == ".")
                        return;

                    if (!ErrorManager.CustomTypeChecker(file))
                    {
                        LogManager.Error($"Skipping file {file} due to validation errors.");
                        continue;
                    }

                    CustomEscapeZone zone = YamlConfigParser.Deserializer.Deserialize<CustomEscapeZone>(File.ReadAllText(file));
                    LogManager.Debug($"Proposed to the registerer the external zone ID: {zone.Id} from file: {file}");
                    action(zone);
                }
                catch (Exception ex)
                {
                    int? line = ex is YamlException yamlEx ? yamlEx.Start.Line : null;
                    int? column = ex is YamlException yamlEx2 ? yamlEx2.Start.Column : null;

                    ErrorManager.Add(
                        file: file,
                        message: ex.Message,
                        line: line,
                        column: column,
                        suggestion: ErrorManager.GetSuggestionFromMessage(ex.Message)
                    );
                    LogManager.Error($"Failed to parse {file}. YAML Exception: {ex.Message}");
                }
            }
        }

        public void Welcome(string localDir = "")
        {
            if (!Is(localDir))
            {
                Directory.CreateDirectory(Path.Combine(Dir, localDir));

                File.WriteAllText(Path.Combine(Dir, localDir, "default-zone.yml"), YamlConfigParser.Serializer.Serialize(new CustomEscapeZone()));

                LogManager.Info($"Plugin does not have a role folder, generated one in {Path.Combine(Dir, localDir)}");
            }
        }
    }
}