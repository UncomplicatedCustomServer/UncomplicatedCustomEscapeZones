using System.Collections.Generic;
using UnityEngine;

namespace UncomplicatedEscapeZones.Interfaces;
#nullable enable
public interface ICustomEscapeZone
{
    public abstract int Id { get; set; }

    public abstract Bounds Bounds { get; set; }

    public abstract Dictionary<string, List<Dictionary<string, string>>> RoleAfterEscape { get; set; }
}