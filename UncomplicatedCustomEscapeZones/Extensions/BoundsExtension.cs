using UncomplicatedEscapeZones.API.Features;
using UnityEngine;

namespace UncomplicatedEscapeZones.Extensions;

public static class BoundsExtension
{
    public static bool TryGetSummonedEscapeZone(this Bounds bounds, out SummonedEscapeZone summonedEscapeZone)
    {
        summonedEscapeZone = SummonedEscapeZone.List.Find(zone => zone.CustomEscapeZone.Bounds == bounds);
        return summonedEscapeZone != null;
    }
}