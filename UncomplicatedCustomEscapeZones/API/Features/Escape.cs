using System.Collections.Generic;

namespace UncomplicatedEscapeZones.API.Features;

internal class Escape
{
    /// <summary>
    ///     Gets the escape bucket to avoid the spam of SubclassSpawn of a custom role during the spawn
    /// </summary>
    public static HashSet<int> Bucket { get; } = [];
}