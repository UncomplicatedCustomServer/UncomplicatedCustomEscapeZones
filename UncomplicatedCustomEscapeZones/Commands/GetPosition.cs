using System.Collections.Generic;
using AdminToys;
using CommandSystem;
using LabApi.Features.Wrappers;
using MapGeneration;
using UncomplicatedEscapeZones.API.Features;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace UncomplicatedEscapeZones.Commands
{
    public class GetPosition : IUCEZCommand
    {
        public string Name { get; } = "position";

        public string Description { get; } = "Gets the current position from the Room's origin.";

        public string RequiredPermission { get; } = "ucez.position";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (!Round.IsRoundInProgress)
            {
                response = "Sorry but you can't use this command if the round is not started!";
                return false;
            }
            
            Player player = Player.Get(sender);
            if (player is null)
            {
                response = "You must be a player to use this command!";
                return false;
            }
            Vector3? position = player.Room?.Name == RoomName.Outside ? player.Position : player.Position - player.Room?.Position;
            response = position.HasValue
                ? $"You are currently standing at: <b>{position.Value}</b>."
                : "You are currently standing at: <b>RoomNotFound</b>.";
            return true;
        }
    }
}