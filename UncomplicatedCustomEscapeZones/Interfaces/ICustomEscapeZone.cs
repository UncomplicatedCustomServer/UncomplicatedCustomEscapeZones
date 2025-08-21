using System.Collections.Generic;
using UncomplicatedEscapeZones.Extensions;

namespace UncomplicatedEscapeZones.Interfaces;
#nullable enable
public interface ICustomEscapeZone
{
    public abstract int Id { get; set; }
    
    public abstract BoundsConfig Bounds { get; set; }
    
    public abstract string RoomName { get; set; }

    public abstract Dictionary<string, List<Dictionary<string, string>>> RoleAfterEscape { get; set; }
}