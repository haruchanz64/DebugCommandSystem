using System.Collections.Generic;
using HaruChanZ64.DebugCommandSystem.Scripts.Core.Base;
using HaruChanZ64.DebugCommandSystem.Scripts.Core.Interface;

namespace HaruChanZ64.DebugCommandSystem.Scripts.Core.Commands
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

                foreach (var cmdObj in allCommands)
                {
                    if (cmdObj is DebugCommandBase commandBase)
                    {
                        output.Add($"- {commandBase.commandFormat} : {commandBase.commanDescription}");
                    }
                }
                DebugController.Instance.DisplayHelpOutput(output);
            });
        }
    }
}