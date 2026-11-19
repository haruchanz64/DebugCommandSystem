using HaruChanZ64.DebugCommandSystem.Commands;

namespace HaruChanZ64.DebugCommandSystem.Interfaces
{
    public interface IDebugCommandProvider
    {
        DebugCommandBase GetCommand();
    }
}
