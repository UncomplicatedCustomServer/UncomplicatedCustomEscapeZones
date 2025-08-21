using System.Linq;
using UncomplicatedEscapeZones.API.Features;
using UnityEngine;

namespace UncomplicatedEscapeZones.Extensions;

public static class BoundsExtension
{
    public static bool TryGetEscapeZone(this Bounds bounds, out SummonedEscapeZone summonedEscapeZone)
    {
        summonedEscapeZone = SummonedEscapeZone.List.Values.FirstOrDefault(zone => zone.Bounds == bounds);
        return summonedEscapeZone != null;
    }
}

public sealed class BoundsConfig
{
    public Vector3 Center { get; set; }
    public Vector3 Size { get; set; }
}