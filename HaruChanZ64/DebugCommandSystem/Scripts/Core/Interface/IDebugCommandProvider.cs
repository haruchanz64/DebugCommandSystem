using HaruChanZ64.DebugCommandSystem.Scripts.Core.Base;

namespace HaruChanZ64.DebugCommandSystem.Scripts.Core.Interface
{
    public interface IDebugCommandProvider
    {
        DebugCommandBase GetCommand();
    }
}