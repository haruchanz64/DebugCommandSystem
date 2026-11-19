using System.Collections.Generic;
using HaruChanZ64.DebugCommandSystem.Core;
using HaruChanZ64.DebugCommandSystem.Interfaces;

namespace HaruChanZ64.DebugCommandSystem.Commands
{
    public class HelpCommand : IDebugCommandProvider
    {
        public DebugCommandBase GetCommand()
        {
            return new DebugCommand("help", "Displays available commands", "help", () =>
            {
                var allCommands = DebugController.GetAllCommands();
                var output = new List<string>
                {
                    "Available Commands:"
                };

                foreach (var commandBase in allCommands)
                {
                    if (commandBase != null)
                    {
                        output.Add($"- {commandBase.commandFormat} : {commandBase.commandDescription}");
                    }
                }

                DebugController.Instance?.DisplayHelpOutput(output);
            });
        }
    }
}
