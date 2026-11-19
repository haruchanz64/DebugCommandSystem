using System;

namespace HaruChanZ64.DebugCommandSystem.Commands
{
    public abstract class DebugCommandBase
    {
        private readonly string _commandId;
        private readonly string _commandDescription;
        private readonly string _commandFormat;

        public string commandId => _commandId;
        public string commandDescription => _commandDescription;
        public string commandFormat => _commandFormat;

        protected DebugCommandBase(string commandId, string commandDescription, string commandFormat)
        {
            if (string.IsNullOrWhiteSpace(commandId))
            {
                throw new ArgumentException("Command ID cannot be null or whitespace.", nameof(commandId));
            }

            _commandId = commandId;
            _commandDescription = commandDescription ?? string.Empty;
            _commandFormat = string.IsNullOrWhiteSpace(commandFormat) ? commandId : commandFormat;
        }
    }
}
