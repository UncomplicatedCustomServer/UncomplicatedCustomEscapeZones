using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace UncomplicatedEscapeZones.Extensions;

public static class RoomExtension
{
    public static Vector3 GetAbsolutePosition(this Room room, Vector3 position)
    {
        if (room is null || room.Name == RoomName.Outside)
            return position;

        return room.Transform.TransformPoint(position);
    }
}