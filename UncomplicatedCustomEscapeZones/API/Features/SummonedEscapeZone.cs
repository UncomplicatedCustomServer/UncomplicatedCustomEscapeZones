using System;
using System.Collections.Concurrent;
using System.Linq;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Managers;
using UnityEngine;

namespace UncomplicatedEscapeZones.API.Features;

public class SummonedEscapeZone
{
    internal SummonedEscapeZone(ICustomEscapeZone zone)
    {
        Id = Guid.NewGuid().ToString();
        Zone = zone;
        List[Id] = this;
        Bounds bounds = new(
            zone.Bounds.Center,
            zone.Bounds.Size
        );
        if (!string.IsNullOrEmpty(zone.RoomName))
        {
            Room targetRoom = Room.List.FirstOrDefault(r => r.GameObject.name == zone.RoomName);
            if (targetRoom != null)
                bounds.center = targetRoom.GetAbsolutePosition(bounds.center);
        }

        Bounds = bounds;
        Map.AddEscapeZone(Bounds);
    }

    /// <summary>
    ///     Gets every <see cref="SummonedEscapeZone" />
    /// </summary>
    public static ConcurrentDictionary<string, SummonedEscapeZone> List { get; } = new();

    /// <summary>
    ///     The unique identifier for this instance of <see cref="SummonedEscapeZone" />
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     Gets the <see cref="ICustomEscapeZone" />
    /// </summary>
    public ICustomEscapeZone Zone { get; }

    public Bounds Bounds { get; }

    internal PrimitiveObjectToy AttachedPrimitive { get; set; }

    private Room Room { get; set; }

    /// <summary>
    ///     Remove the SummonedCustomEscapeZone from the list by destroying it!
    /// </summary>
    public void Destroy()
    {
        LogManager.Debug($"Destroying instance {Id} of CEZ {Zone.Id}");
        AttachedPrimitive?.Destroy();
        Map.RemoveEscapeZone(Bounds);
        List.TryRemove(Id, out _);
    }

    /// <summary>
    ///     Gets a <see cref="SummonedEscapeZone" /> instance by the Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static SummonedEscapeZone Get(string id)
    {
        return List.Values.FirstOrDefault(scez => scez.Id == id);
    }

    public static void RemoveSpecificEscapeZone(int id)
    {
        LogManager.Debug($"Removing all SummonedEscapeZone instances with Zone Id {id}");
        foreach (SummonedEscapeZone zone in List.Values.Where(scr => scr.Zone.Id == id)) zone.Destroy();
    }
}