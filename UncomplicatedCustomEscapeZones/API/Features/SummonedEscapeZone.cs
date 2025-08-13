using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;

namespace UncomplicatedEscapeZones.API.Features
{
    public class SummonedEscapeZone
    {
        /// <summary>
        /// Gets a list of every spawned <see cref="CustomEscapeZone"/> as <see cref="SummonedEscapeZone"/>
        /// </summary>
        public static List<SummonedEscapeZone> List { get; } = [];

        
        /// <summary>
        /// Gets the ID of this <see cref="SummonedEscapeZone"/>.
        /// </summary>
        public string Id { get; }

        
        /// <summary>
        /// Gets the <see cref="CustomEscapeZone"/> that this <see cref="SummonedEscapeZone"/> represents.
        /// </summary>
        public CustomEscapeZone CustomEscapeZone { get; }

        /// <summary>
        /// Creates a new summoned custom zone and adds it to the list.
        /// </summary>
        public SummonedEscapeZone(CustomEscapeZone customEscapeZone)
        {
            CustomEscapeZone = customEscapeZone;
            Id = Guid.NewGuid().ToString();
            
            Map.AddEscapeZone(customEscapeZone.Bounds);
            List.Add(this);
        }

        /// <summary>
        /// Destroys this summoned zone and removes it from the list.
        /// </summary>
        public void Destroy()
        {
            List.Remove(this);
        }

        /// <summary>
        /// Summons a new zone and assigns players to available roles.
        /// </summary>
        public static SummonedEscapeZone Summon(CustomEscapeZone customEscapeZone)
        {
            if (customEscapeZone == null)
            {
                return null;
            }

            SummonedEscapeZone summonedEscapeZone = new(customEscapeZone);
            return summonedEscapeZone;
        }
    }
}