using System.Collections.Generic;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedEscapeZones.Interfaces;

namespace UncomplicatedEscapeZones.Commands;

public class GetRoomName : IUCEZCommand
{
    public string Name { get; } = "roomname";

    public string Description { get; } = "Gets the current room name where you're standing.";

    public string RequiredPermission { get; } = "ucez.roomname";

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

        response = $"You are currently in the room: <b>{player.Room?.GameObject.name ?? "RoomNotFound"}</b>.";
        return true;
    }
}