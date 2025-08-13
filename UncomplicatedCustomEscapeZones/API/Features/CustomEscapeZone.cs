using System.Collections.Generic;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.Interfaces;
using UnityEngine;

namespace UncomplicatedEscapeZones.API.Features;

public class CustomEscapeZone : ICustomEscapeZone
{
    /// <summary>
    ///     Gets a complete list of every custom <see cref="CustomEscapeZone" /> registered
    /// </summary>
    public static List<CustomEscapeZone> List { get; } = [];

    /// <summary>
    ///     The Id of the <see cref="CustomEscapeZone" />.
    /// </summary>
    public int Id { get; set; } = 1;

    /// <summary>
    ///     Gets the <see cref="Bounds" /> of the <see cref="CustomEscapeZone" />
    /// </summary>
    public Bounds Bounds { get; set; } = Map.DefaultEscapeZone;

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
    ///     Register a new <see cref="CustomEscapeZone" />
    /// </summary>
    /// <param name="customEscapeZone"></param>
    public static void Register(CustomEscapeZone customEscapeZone)
    {
        List.Add(customEscapeZone);
    }

    /// <summary>
    ///     Unregister a <see cref="CustomEscapeZone" />
    /// </summary>
    /// <param name="customEscapeZone"></param>
    public static void Unregister(CustomEscapeZone customEscapeZone)
    {
        List.Remove(customEscapeZone);
    }
}