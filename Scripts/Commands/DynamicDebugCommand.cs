using System;

namespace HaruChanZ64.DebugCommandSystem.Commands
{
    public class DynamicDebugCommand : DebugCommandBase
    {
        private readonly Action<string[]> _command;

        /// <summary>
        /// Creates a command that takes dynamic parameters as strings.
        /// </summary>
        /// <param name="commandId">The unique identifier of the command.</param>
        /// <param name="commandDescription">A brief summary of what the command does.</param>
        /// <param name="commandFormat">The string that shows how the command is typed.</param>
        /// <param name="command">The action to execute.</param>
        public DynamicDebugCommand(string commandId, string commandDescription, string commandFormat, Action<string[]> command)
            : base(commandId, commandDescription, commandFormat)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public void Invoke(params string[] args)
        {
            _command.Invoke(args ?? Array.Empty<string>());
        }
    }
}
