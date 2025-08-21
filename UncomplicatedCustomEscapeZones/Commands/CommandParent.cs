using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using UncomplicatedEscapeZones.Extensions;
using UncomplicatedEscapeZones.Interfaces;
using UncomplicatedEscapeZones.Managers;

namespace UncomplicatedEscapeZones.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class CommandParent : ParentCommand
    {
        public CommandParent() => LoadGeneratedCommands();

        public override string Command { get; } = "ucez";

        public override string[] Aliases { get; } = [];

        public override string Description { get; } = "Manage the UCEZ features";

        public sealed override void LoadGeneratedCommands() 
        {
            RegisteredCommands.Add(new Reload());
            RegisteredCommands.Add(new Visibility());
            RegisteredCommands.Add(new GetRoomName());
            RegisteredCommands.Add(new GetPosition());
        }

        public List<IUCEZCommand> RegisteredCommands { get; } = [];

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!arguments.Any())
            {
                // Help page
                response = $"\n>> UncomplicatedCustomEscapeZones v{Plugin.Instance.Version}{(VersionManager.VersionInfo?.CustomName is not null ? $" '{VersionManager.VersionInfo.CustomName}'" : string.Empty)} <<\nby {Plugin.Instance.Author}\n\nAvailable commands:";

                foreach (IUCEZCommand Command in RegisteredCommands)
                    response += $"\n• <b>ucez {Command.Name.GenerateWithBuffer(12)}</b> → {Command.Description}";

                response += "\n<size=1>OwO</size>";

                return true;
            } 
            else
            {
                // Arguments compactor:
                List<string> Arguments = [];
                foreach (string Argument in arguments.Where(arg => arg != arguments.At(0)))
                {
                    Arguments.Add(Argument);
                }

                IUCEZCommand Command = RegisteredCommands.FirstOrDefault(command => command.Name == arguments.At(0));

                if (Command is not null)
                    if (sender.CheckPermission(PlayerPermissions.LongTermBanning)) // Fix for the absence of EXILED.Permissions
                        return Command.Executor(Arguments, sender, out response);
                    else
                    {
                        response = $"You don't have enough permission(s) to execute that command!\nNeeded: {Command.RequiredPermission}";
                        return false;
                    }
                else
                {
                    response = "Command not found!";
                    return false;
                }
            }
        }
    }
}
