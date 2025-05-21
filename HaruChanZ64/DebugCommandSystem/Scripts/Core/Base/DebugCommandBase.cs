using System;

namespace HaruChanZ64.DebugCommandSystem.Scripts.Core.Base
{
    public class DebugCommandBase
    {
        private readonly string _commandId;
        private readonly string _commandDescription;
        private readonly string _commandFormat;
        
        public string commandId => _commandId;
        public string commanDescription => _commandDescription;
        public string commandFormat => _commandFormat;

        protected DebugCommandBase(string commandId, string commandDescription, string commandFormat)
        {
            _commandId = commandId;
            _commandDescription = commandDescription;
            _commandFormat = commandFormat;
        }
    }

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
        public DebugCommand(string commandId, string commandDescription, string commandFormat, Action command) : base(commandId, commandDescription, commandFormat)
        {
            _command = command;
        }

        public void Invoke()
        {
            _command.Invoke();
        }
    }
    
    public class DynamicDebugCommand : DebugCommandBase
    {
        private readonly Action<string[]> _command;

        /// <summary>
        /// Creates a command that takes dynamic parameters as strings.
        /// </summary>
        /// <param name="commandId">Unique identifier</param>
        /// <param name="commandDescription">What the command does</param>
        /// <param name="commandFormat">How to use the command</param>
        /// <param name="command">The action to execute</param>
        public DynamicDebugCommand(string commandId, string commandDescription, string commandFormat, Action<string[]> command)
            : base(commandId, commandDescription, commandFormat)
        {
            _command = command;
        }

        public void Invoke(params string[] args)
        {
            _command.Invoke(args);
        }
    }

}