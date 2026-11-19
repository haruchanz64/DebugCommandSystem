# Debug Command System

A flexible and extendable developer debug console system for Unity projects, supporting typed commands, auto-completion, and command suggestions. Fully integrated with Unity's **New Input System**.

## Features

- Supports commands with any number of parameters using `params string[]`.
- Auto-completion and command suggestions while typing.
- Toggle console visibility with a hotkey (default: **F1**).
- Execute commands with the **Enter** key.
- Auto-complete commands with the **Tab** key.
- Outputs can be shown in the console window or in a GUI box.
- Help command displays all available commands in a scrollable GUI.
- Easy to add new commands by implementing `IDebugCommandProvider`.

## Installation

### Option 1: Unity Package Manager (Recommended for Unity 6)
1. Open **Window > Package Manager** in Unity 6.
2. Click the **+** button in the top-left corner and select **Install package from git URL...**
3. Enter:
   ```text
   https://github.com/haruchanz64/DebugCommandSystem.git
   ```

### Option 2: Manual Installation
1. **Clone or download** this repository and copy the folder into your Unity project's `Assets` directory (e.g., `Assets/DebugCommandSystem/`):
   ```bash
   git clone https://github.com/haruchanz64/DebugCommandSystem.git
   ```

### Scene Setup
1. Make sure you have the **Input System** enabled in your project:
   - Go to **Edit > Project Settings > Player > Active Input Handling** and select **Input System Package (New)** (default in Unity 6).
2. Add the `DebugController` prefab to your scene:
   - Navigate to `Prefabs/` (or `Packages/Debug Command System/Prefabs/` if installed via UPM).
   - Drag `DebugController.prefab` into your scene.
3. Press **Play**, then toggle the debug console using the default hotkey (**F1**).

## Usage

1. Add the `DebugController` prefab (or attach the `DebugController` component with `InputActions/DebugInputActions.inputactions` assigned) to a GameObject in your scene.
2. Create commands by implementing the `IDebugCommandProvider` interface and returning either a `DebugCommand` or `DynamicDebugCommand`.
3. Use the console by pressing **F1**.
4. Type your command and press **Enter** to execute.
5. Use **Tab** to cycle through matching command suggestions.
6. Type `help` to display a list of all available commands.


## Requirements & Setup
- **Unity 6 (6000.0)** or later is required.
- **Input System** (`com.unity.inputsystem` 1.11.2+) enabled.
- Compatible with **Enter Play Mode without Domain Reload** and Unity 6 **Project-Wide Input Actions** (`InputSystem.actions` is used as a fallback if no `InputActionAsset` is assigned on `DebugController`).
- Make sure your `InputActionAsset` includes a `Console` action map with:
  - `Toggle` (e.g., bound to F1)
  - `Execute` (e.g., Enter / Numpad Enter)
  - `AutoComplete` (e.g., Tab)


## Example Command

```csharp
using HaruChanZ64.DebugCommandSystem.Commands;
using HaruChanZ64.DebugCommandSystem.Interfaces;
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
