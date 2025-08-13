using System.Collections.Generic;
using CommandSystem;

namespace UncomplicatedEscapeZones.Interfaces;

internal interface IUCEZCommand
{
    public string Name { get; }

    public string Description { get; }

    public string RequiredPermission { get; }

    public bool Executor(List<string> arguments, ICommandSender sender, out string response);
}