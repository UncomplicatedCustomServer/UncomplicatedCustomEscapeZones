using System.Text.Json.Serialization;

namespace UncomplicatedEscapeZones.Managers;
#nullable enable

[method: JsonConstructor]
internal class VersionInfo(
    string name,
    string source,
    string? sourceLink,
    string? customName,
    int preRelease,
    int forceDebug,
    string message,
    int recall,
    string? recallTarget,
    string? recallReason,
    bool? recallImportant,
    string hash)
{
    [JsonPropertyName("name")] public string Name { get; } = name;

    [JsonPropertyName("source")] public string Source { get; } = source;

    [JsonPropertyName("source_link")] public string? SourceLink { get; } = sourceLink;

    [JsonPropertyName("custom_name")] public string? CustomName { get; } = customName;

    [JsonPropertyName("pre_release")] public int PreRelease { get; } = preRelease;

    [JsonPropertyName("force_debug")] public int ForceDebug { get; } = forceDebug;

    [JsonPropertyName("message")] public string Message { get; } = message;

    [JsonPropertyName("recall")] public int Recall { get; } = recall;

    [JsonPropertyName("recall_target")] public string? RecallTarget { get; } = recallTarget;

    [JsonPropertyName("recall_reason")] public string? RecallReason { get; } = recallReason;

    [JsonPropertyName("recall_important")] public bool? RecallImportant { get; } = recallImportant;

    [JsonPropertyName("hash")] public string Hash { get; } = hash;
}