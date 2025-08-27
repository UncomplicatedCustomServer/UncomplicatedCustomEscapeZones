using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.API.Features;

public class CustomEscapeZone : ICustomEscapeZone
{
    /// <summary>
    ///     A more easy-to-use dictionary to store every registered <see cref="ICustomEscapeZone" />
    /// </summary>
    internal static ConcurrentDictionary<int, ICustomEscapeZone> CustomEscapeZones { get; set; } = new();

    /// <summary>
    ///     Get a list of every <see cref="ICustomEscapeZone" /> registered.
    /// </summary>
    public static List<ICustomEscapeZone> List => CustomEscapeZones.Values.ToList();

    /// <summary>
    ///     The Id of the <see cref="CustomEscapeZone" />.
    /// </summary>
    public int Id { get; set; } = 1;

    /// <summary>
    ///     Gets the <see cref="BoundsConfig" /> of the <see cref="CustomEscapeZone" />
    /// </summary>
    public BoundsConfig Bounds { get; set; } = new()
    {
        Center = Map.DefaultEscapeZone.center,
        Size = Map.DefaultEscapeZone.size
    };

    /// <summary>
    ///     The name of the <see cref="Room" /> which the <see cref="CustomEscapeZone" /> is in.
    /// </summary>
    public string RoomName { get; set; } = "";

    /// <summary>
    ///     Gets or sets the role after escape
    ///     The escaping player's role, The role to be assigned to the player after escaping.
    /// </summary>
    public virtual Dictionary<string, List<Dictionary<string, string>>> RoleAfterEscape { get; set; } =
        new()
        {
            {
                "ClassD", [
                    new Dictionary<string, string>
                    {
                        { "default", "InternalRole ChaosRepressor" },
                        { "cuffed by InternalTeam FoundationForces", "InternalRole NtfPrivate" }
                    }
                ]
            },
            {
                "Scientist", [
                    new Dictionary<string, string>
                    {
                        { "default", "InternalRole NtfSpecialist" },
                        { "cuffed by InternalTeam ChaosInsurgency", "InternalRole ChaosRepressor" }
                    }
                ]
            }
        };

    /// <summary>
    ///     Try to get a registered <see cref="ICustomEscapeZone" /> by it's Id.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="customEscapeZone"></param>
    /// <returns><see langword="true" /> if the operation was successful.</returns>
    public static bool TryGet(int id, out ICustomEscapeZone customEscapeZone)
    {
        if (CustomEscapeZones.TryGetValue(id, out ICustomEscapeZone escapeZone))
        {
            customEscapeZone = escapeZone;
            return true;
        }

        customEscapeZone = null;
        return false;
    }

    /// <summary>
    ///     Get a registered <see cref="ICustomEscapeZone" /> by it's Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The <see cref="ICustomEscapeZone" /> with the given Id or <see langword="null" /> if not found.</returns>
    public static ICustomEscapeZone Get(int id)
    {
        return TryGet(id, out ICustomEscapeZone customEscapeZone) ? customEscapeZone : null;
    }

    /// <summary>
    ///     Register a new <see cref="ICustomEscapeZone" /> instance.
    /// </summary>
    /// <param name="customEscapeZone"></param>
    public static void Register(ICustomEscapeZone customEscapeZone)
    {
        LogManager.Debug(
            $"Registering CustomEscapeZone with Id {customEscapeZone.Id} and RoomName {customEscapeZone.RoomName}");
        if (CustomEscapeZones.ContainsKey(customEscapeZone.Id))
        {
            LogManager.Error($"A CustomEscapeZone with the Id {customEscapeZone.Id} already exists!");
            return;
        }

        CustomEscapeZones.TryAdd(customEscapeZone.Id, customEscapeZone);
        LogManager.Debug($"CustomEscapeZone with Id {customEscapeZone.Id} registered successfully!");
    }

    /// <summary>
    ///     Unregister a registered <see cref="ICustomEscapeZone" />.
    /// </summary>
    /// <param name="customRole"></param>
    public static void Unregister(ICustomEscapeZone customRole)
    {
        CustomEscapeZones.TryRemove(customRole.Id, out _);
    }
}