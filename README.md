
# Debug Command System

A flexible and extendable developer debug console system for Unity projects, supporting typed commands, auto-completion, and command suggestions. Fully integrated with Unity's **New Input System**. 
## Features

- Supports commands with any number of parameters using params string[].
- Auto-completion and command suggestions while typing.
- Toggle console visibility with a hotkey (default: F1).
- Execute commands with the Enter key.
- Auto-complete commands with the Tab key.
- Outputs can be shown in the console window or in a GUI box.
- Help command displays all available commands in a scrollable GUI.
- Easy to add new commands by implementing ``IDebugCommandProvider``.


## Installation

Import the `.unitypackage` into your Unity project:
   - In Unity Editor, go to **Assets > Import Package > Custom Package...**
   - Select the `.unitypackage` file.
   - In the import dialog, ensure **all files and folders** are selected, then click **Import**.
## Usage

1. Add the ``DebugController`` component to a GameObject in your scene.
2. Create commands by implementing the ``IDebugCommandProvider`` interface and returning either a ``DebugCommand`` or ``DynamicDebugCommand``.
3. Use the console by pressing F1.
4. Type your command and press Enter to execute.
5. Use Tab to cycle through matching command suggestions.
6. Type help to display a list of all available commands.


## Setup
- Unity 2020.1 or later is required.
- Enable and configure Unity’s New Input System.
- Make sure your InputActionAsset includes a Console action map with:
  - Toggle (e.g., bound to F1)
  - Execute (e.g., Enter)
  - AutoComplete (e.g., Tab)


## Example Command

```csharp
using HaruChanZ64.DebugCommandSystem.Scripts.Core;
using HaruChanZ64.DebugCommandSystem.Scripts.Examples;
using UnityEngine;

public class SetHealthCommand : IDebugCommandProvider
{
    public DebugCommandBase GetCommand()
    {
        return new DynamicDebugCommand(
            "set_health",
            "Set player health",
            "set_health <value>",
            args =>
            {
                if (args.Length < 1)
                {
                    Debug.LogWarning("Usage: set_health <value>");
                    return;
                }

                if (int.TryParse(args[0], out var health))
                {
                    HealthManager.Instance.SetHealth(health);
                    Debug.Log($"Health set to {health}");
                }
                else
                {
                    Debug.LogWarning("Invalid value.");
                }
            }
        );
    }
}
```


## Format
Each command has:
- **commandId** (the keyword you type)
- **commandDescription** (brief description)
- **commandFormat** (how to use it)
- **command** (execute when the command is called)
## License

[MIT License](https://choosealicense.com/licenses/mit/)


## Contact

Created by [Haru](https://github.com/haruchanz64)


## Feedback
Feel free to open issues or contribute!