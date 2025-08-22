using Newtonsoft.Json;

namespace UncomplicatedEscapeZones.Utilities;
#nullable enable

internal class VersionInfo(
    string name,
    string source,
    string? sourceLink,
    string? customName,
    bool preRelease,
    bool forceDebug,
    string message,
    bool recall,
    string? recallTarget,
    string? recallReason,
    bool? recallImportant,
    string hash)
{
    [JsonProperty("name")] public string Name { get; } = name;

    [JsonProperty("source")] public string Source { get; } = source;

    [JsonProperty("source_link")] public string? SourceLink { get; } = sourceLink;

    [JsonProperty("custom_name")] public string? CustomName { get; } = customName;

    [JsonProperty("pre_release")] public bool PreRelease { get; } = preRelease;

    [JsonProperty("force_debug")] public bool ForceDebug { get; } = forceDebug;

    [JsonProperty("message")] public string Message { get; } = message;

    [JsonProperty("recall")] public bool Recall { get; } = recall;

    [JsonProperty("recall_target")] public string? RecallTarget { get; } = recallTarget;

    [JsonProperty("recall_reason")] public string? RecallReason { get; } = recallReason;

    [JsonProperty("recall_important")] public bool? RecallImportant { get; } = recallImportant;

    [JsonProperty("hash")] public string Hash { get; } = hash;
}