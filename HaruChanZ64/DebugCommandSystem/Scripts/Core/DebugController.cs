using System;
using System.Collections.Generic;
using HaruChanZ64.DebugCommandSystem.Scripts.Core.Base;
using HaruChanZ64.DebugCommandSystem.Scripts.Core.Commands;
using HaruChanZ64.DebugCommandSystem.Scripts.Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HaruChanZ64.DebugCommandSystem.Scripts.Core
{
    public class DebugController : MonoBehaviour
    {
        [SerializeField] private bool showConsole;
        [SerializeField] private bool showHelp;
        [SerializeField] private string commandInput;
        private Vector2 _scroll;

        [SerializeField] private InputActionAsset inputActions;
        private InputAction _toggleAction;
        private InputAction _executeAction;
        private InputAction _autoCompleteAction;
        private InputActionMap _consoleActionMap;

        private List<object> _commandList;
        private static List<object> _staticCommandList = new List<object>();

        [SerializeField] private List<string> commandListSuggestions;
        [SerializeField] private int selectedCommandListSuggestionIndex = 0;

        private List<string> _helpOutput = new();
        private float _helpOutputTimeShown = -1f;
        private const float HelpOutputDuration = 5f;

        private bool moveCaretToEnd = false;

        public static DebugController Instance { get; private set; }

        public static List<object> GetAllCommands() => _staticCommandList;

        private void Awake()
        {
            if (!Instance) Instance = this;
            else Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            _consoleActionMap = inputActions.FindActionMap("Console", throwIfNotFound: true);

            _toggleAction = _consoleActionMap.FindAction("Toggle", throwIfNotFound: true);
            _executeAction = _consoleActionMap.FindAction("Execute", throwIfNotFound: true);
            _autoCompleteAction = _consoleActionMap.FindAction("AutoComplete", throwIfNotFound: true);

            _toggleAction.performed += ctx => OnToggleDebugConsole();
            _executeAction.performed += ctx => OnExecuteCommand();
            _autoCompleteAction.performed += ctx => OnAutoCompleteCommand();
        }

        private void OnEnable()
        {
            _consoleActionMap.Enable();
        }

        private void OnDisable()
        {
            _consoleActionMap.Disable();
        }

        private void Start()
        {
            commandInput = "";
            InitializeCommands();
        }

        private void Update()
        {
            if (_commandList == null)
                InitializeCommands();

            if (Keyboard.current.anyKey.wasPressedThisFrame)
                OnShowCommandSuggestions();

            if (_helpOutputTimeShown > 0 && Time.time - _helpOutputTimeShown > HelpOutputDuration)
            {
                _helpOutput.Clear();
                _helpOutputTimeShown = -1f;
            }
        }

        private void OnGUI()
        {
            if (!showConsole) return;

            var yPosition = 0f;

            if (showHelp)
            {
                GUI.Box(new Rect(0, yPosition, Screen.width, 100f), "");
                var viewPort = new Rect(0, 0, Screen.width - 30, 20 * _commandList.Count);
                _scroll = GUI.BeginScrollView(new Rect(0, yPosition + 5f, Screen.width, 90f), _scroll, viewPort);

                for (var cmd = 0; cmd < _commandList.Count; cmd++)
                {
                    if (_commandList[cmd] is DebugCommandBase commandBase)
                    {
                        var label = $"{commandBase.commandFormat} - {commandBase.commanDescription}";
                        GUI.Label(new Rect(5, 20 * cmd, viewPort.width - 100f, 20f), label);
                    }
                }

                GUI.EndScrollView();
                yPosition += 100f;
            }

            GUI.Box(new Rect(0, yPosition, Screen.width, 30f), "");
            GUI.backgroundColor = Color.black;

            GUI.SetNextControlName("DebugConsoleInput");
            commandInput = GUI.TextField(new Rect(10f, yPosition + 5f, Screen.width - 20f, 20f), commandInput);
            GUI.FocusControl("DebugConsoleInput");

            if (moveCaretToEnd)
            {
                var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                if (textEditor != null)
                {
                    textEditor.cursorIndex = textEditor.selectIndex = commandInput.Length;
                }
                moveCaretToEnd = false;
            }

            var suggestionBoxHeight = 0f;
            if (commandListSuggestions.Count > 0)
            {
                suggestionBoxHeight = 50f * commandListSuggestions.Count;
                GUI.Box(new Rect(10f, yPosition + 30f, Screen.width - 20f, suggestionBoxHeight), "");

                for (var i = 0; i < commandListSuggestions.Count; i++)
                {
                    GUI.Label(new Rect(15f, yPosition + 35f + (20f * i), Screen.width - 30f, 20f),
                        (i == selectedCommandListSuggestionIndex ? "> " : "") + commandListSuggestions[i]);
                }
            }

            if (_helpOutput.Count > 0)
            {
                var helpBoxHeight = 50f * _helpOutput.Count;
                GUI.Box(new Rect(10f, yPosition + 35f + suggestionBoxHeight, Screen.width - 20f, helpBoxHeight), "");

                for (var i = 0; i < _helpOutput.Count; i++)
                {
                    GUI.Label(new Rect(15f, yPosition + 40f + suggestionBoxHeight + (20f * i), Screen.width - 30f, 20f), _helpOutput[i]);
                }
            }
        }

        private void OnToggleDebugConsole()
        {
            showConsole = !showConsole;
        }

        private void OnExecuteCommand()
        {
            if (!showConsole) return;
            HandleInput();
            commandInput = "";
        }

        private void OnAutoCompleteCommand()
        {
            if (commandListSuggestions.Count == 0) return;

            commandInput = commandListSuggestions[selectedCommandListSuggestionIndex];
            selectedCommandListSuggestionIndex = (selectedCommandListSuggestionIndex + 1) % commandListSuggestions.Count;

            // Set flag to move caret to end on next OnGUI
            moveCaretToEnd = true;
        }

        private void OnShowCommandSuggestions()
        {
            commandListSuggestions.Clear();

            if (string.IsNullOrEmpty(commandInput) || _commandList == null) return;

            var cmdInput = commandInput.ToLower();
            foreach (var cmd in _commandList)
            {
                if (cmd is DebugCommandBase command && command.commandId.StartsWith(cmdInput, StringComparison.OrdinalIgnoreCase))
                {
                    commandListSuggestions.Add(command.commandId);
                }
            }
        }

        private void InitializeCommands()
        {
            _commandList = new List<object>();
            var commandTypes = typeof(DebugController).Assembly.GetTypes();

            foreach (var type in commandTypes)
            {
                if (!typeof(IDebugCommandProvider).IsAssignableFrom(type) || type.IsInterface || type.IsAbstract) continue;
                if (Activator.CreateInstance(type) is not IDebugCommandProvider provider) continue;
                var command = provider.GetCommand();
                _commandList.Add(command);
            }

            _staticCommandList = _commandList;
        }

        private void HandleInput()
        {
            var properties = commandInput.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var cmdObj in _commandList)
            {
                if (cmdObj is not DebugCommandBase commandBase) continue;

                if (!commandInput.StartsWith(commandBase.commandId, StringComparison.OrdinalIgnoreCase)) continue;

                try
                {
                    if (cmdObj is DebugCommand noArgCmd)
                    {
                        noArgCmd.Invoke();
                        return;
                    }

                    if (cmdObj is not DynamicDebugCommand dynamicCmd) continue;
                    var args = properties.Length > 1 ? properties[1..] : Array.Empty<string>();
                    dynamicCmd.Invoke(args);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to execute command: {ex.Message}");
                }
            }
        }

        public void DisplayHelpOutput(List<string> outputLines)
        {
            _helpOutput = outputLines;
            _helpOutputTimeShown = Time.time;
        }
    }
}
