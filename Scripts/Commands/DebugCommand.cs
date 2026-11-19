using System;

namespace HaruChanZ64.DebugCommandSystem.Commands
{
    public class DebugCommand : DebugCommandBase
    {
        private readonly Action _command;

        /// <summary>
        /// Creates a debug command with no parameters.
        /// </summary>
        /// <param name="commandId">The unique identifier of the command.</param>
        /// <param name="commandDescription">A brief summary of what the command does.</param>
        /// <param name="commandFormat">The string that shows how the command is typed.</param>
        /// <param name="command">The method to execute when the command is called.</param>
        public DebugCommand(string commandId, string commandDescription, string commandFormat, Action command)
            : base(commandId, commandDescription, commandFormat)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public void Invoke()
        {
            _command.Invoke();
        }
    }
}
