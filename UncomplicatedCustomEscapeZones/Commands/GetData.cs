using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using MapGeneration;
using UncomplicatedEscapeZones.Interfaces;
using UnityEngine;

namespace UncomplicatedEscapeZones.Commands;

public class GetData : IUCEZCommand
{
    public string Name { get; } = "getdata";

    public string Description { get; } = "Gets the current position from the Room's origin and the Room's name.";

    public string RequiredPermission { get; } = "ucez.getdata";

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

        Vector3? position = player.Room?.Name == RoomName.Outside
            ? player.Position
            : player.Position - player.Room?.Position;
        response = position.HasValue
            ? $"You are currently standing at: <b>Room: {player.Room?.GameObject.name}\nPosition: {position.Value}</b>."
            : "You are currently standing at: <b>Room not found</b>.";
        return true;
    }
}