using System;
using System.Collections.Generic;
using System.Reflection;
using HaruChanZ64.DebugCommandSystem.Commands;
using HaruChanZ64.DebugCommandSystem.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HaruChanZ64.DebugCommandSystem.Core
{
    public class DebugController : MonoBehaviour
    {
        [SerializeField] private bool showConsole;
        [SerializeField] private bool showHelp;
        [SerializeField] private string commandInput = string.Empty;
        private string _lastCommandInput = string.Empty;
        private Vector2 _scroll;

        [SerializeField] private InputActionAsset inputActions;
        private InputAction _toggleAction;
        private InputAction _executeAction;
        private InputAction _autoCompleteAction;
        private InputActionMap _consoleActionMap;

        private List<DebugCommandBase> _commandList = new List<DebugCommandBase>();
        private static List<DebugCommandBase> _staticCommandList = new List<DebugCommandBase>();

        [SerializeField] private List<string> commandListSuggestions = new List<string>();
        [SerializeField] private int selectedCommandListSuggestionIndex;

        private List<string> _helpOutput = new List<string>();
        private float _helpOutputTimeShown = -1f;
        private const float HelpOutputDuration = 5f;
        private const float LineHeight = 20f;
        private const float BoxPadding = 10f;

        private bool _moveCaretToEnd;
        private bool _commandsInitialized;

        public static DebugController Instance { get; private set; }

        public static IReadOnlyList<DebugCommandBase> GetAllCommands() => _staticCommandList;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (inputActions == null)
            {
                Debug.LogError("DebugController: InputActionAsset is not assigned.", this);
                enabled = false;
                return;
            }

            _consoleActionMap = inputActions.FindActionMap("Console", throwIfNotFound: true);

            _toggleAction = _consoleActionMap.FindAction("Toggle", throwIfNotFound: true);
            _executeAction = _consoleActionMap.FindAction("Execute", throwIfNotFound: true);
            _autoCompleteAction = _consoleActionMap.FindAction("AutoComplete", throwIfNotFound: true);

            _toggleAction.performed += OnTogglePerformed;
            _executeAction.performed += OnExecutePerformed;
            _autoCompleteAction.performed += OnAutoCompletePerformed;
        }

        private void OnEnable()
        {
            if (Instance == this)
            {
                _consoleActionMap?.Enable();
            }
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                _consoleActionMap?.Disable();
            }
        }

        private void OnDestroy()
        {
            if (_toggleAction != null) _toggleAction.performed -= OnTogglePerformed;
            if (_executeAction != null) _executeAction.performed -= OnExecutePerformed;
            if (_autoCompleteAction != null) _autoCompleteAction.performed -= OnAutoCompletePerformed;

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            commandInput = string.Empty;
            _lastCommandInput = string.Empty;
            InitializeCommands();
        }

        private void Update()
        {
            if (!_commandsInitialized)
            {
                InitializeCommands();
            }

            if (_helpOutputTimeShown >= 0f && Time.unscaledTime - _helpOutputTimeShown > HelpOutputDuration)
            {
                _helpOutput.Clear();
                _helpOutputTimeShown = -1f;
            }
        }

        private void OnGUI()
        {
            if (!showConsole) return;

            var previousBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;

            try
            {
                var yPosition = 0f;

                if (showHelp && _commandList != null)
                {
                    GUI.Box(new Rect(0, yPosition, Screen.width, 100f), string.Empty);
                    var viewPort = new Rect(0, 0, Screen.width - 30f, LineHeight * _commandList.Count);
                    _scroll = GUI.BeginScrollView(new Rect(0, yPosition + 5f, Screen.width, 90f), _scroll, viewPort);

                    for (var i = 0; i < _commandList.Count; i++)
                    {
                        var commandBase = _commandList[i];
                        if (commandBase == null) continue;

                        var label = $"{commandBase.commandFormat} - {commandBase.commandDescription}";
                        GUI.Label(new Rect(5f, LineHeight * i, viewPort.width - 10f, LineHeight), label);
                    }

                    GUI.EndScrollView();
                    yPosition += 100f;
                }

                GUI.Box(new Rect(0, yPosition, Screen.width, 30f), string.Empty);

                GUI.SetNextControlName("DebugConsoleInput");
                var newCommandInput = GUI.TextField(
                    new Rect(10f, yPosition + 5f, Screen.width - 20f, LineHeight),
                    commandInput ?? string.Empty);
                GUI.FocusControl("DebugConsoleInput");

                if (newCommandInput != commandInput)
                {
                    commandInput = newCommandInput;
                    if (commandInput != _lastCommandInput)
                    {
                        _lastCommandInput = commandInput;
                        UpdateCommandSuggestions();
                    }
                }

                if (_moveCaretToEnd)
                {
                    var textEditor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    if (textEditor != null)
                    {
                        var length = commandInput?.Length ?? 0;
                        textEditor.cursorIndex = textEditor.selectIndex = length;
                    }

                    _moveCaretToEnd = false;
                }

                var suggestionBoxHeight = 0f;
                if (commandListSuggestions != null && commandListSuggestions.Count > 0)
                {
                    suggestionBoxHeight = LineHeight * commandListSuggestions.Count + BoxPadding;
                    GUI.Box(new Rect(10f, yPosition + 30f, Screen.width - 20f, suggestionBoxHeight), string.Empty);

                    for (var i = 0; i < commandListSuggestions.Count; i++)
                    {
                        var prefix = i == selectedCommandListSuggestionIndex ? "> " : string.Empty;
                        GUI.Label(
                            new Rect(15f, yPosition + 35f + (LineHeight * i), Screen.width - 30f, LineHeight),
                            prefix + commandListSuggestions[i]);
                    }
                }

                if (_helpOutput != null && _helpOutput.Count > 0)
                {
                    var helpBoxHeight = LineHeight * _helpOutput.Count + BoxPadding;
                    GUI.Box(new Rect(10f, yPosition + 35f + suggestionBoxHeight, Screen.width - 20f, helpBoxHeight), string.Empty);

                    for (var i = 0; i < _helpOutput.Count; i++)
                    {
                        GUI.Label(
                            new Rect(15f, yPosition + 40f + suggestionBoxHeight + (LineHeight * i), Screen.width - 30f, LineHeight),
                            _helpOutput[i]);
                    }
                }
            }
            finally
            {
                GUI.backgroundColor = previousBackgroundColor;
            }
        }

        private void OnTogglePerformed(InputAction.CallbackContext ctx) => OnToggleDebugConsole();
        private void OnExecutePerformed(InputAction.CallbackContext ctx) => OnExecuteCommand();
        private void OnAutoCompletePerformed(InputAction.CallbackContext ctx) => OnAutoCompleteCommand();

        private void OnToggleDebugConsole()
        {
            showConsole = !showConsole;
        }

        private void OnExecuteCommand()
        {
            if (!showConsole) return;

            HandleInput();
            commandInput = string.Empty;
            _lastCommandInput = string.Empty;
            commandListSuggestions?.Clear();
            selectedCommandListSuggestionIndex = 0;
        }

        private void OnAutoCompleteCommand()
        {
            if (!showConsole || commandListSuggestions == null || commandListSuggestions.Count == 0) return;

            if (selectedCommandListSuggestionIndex < 0 || selectedCommandListSuggestionIndex >= commandListSuggestions.Count)
            {
                selectedCommandListSuggestionIndex = 0;
            }

            commandInput = commandListSuggestions[selectedCommandListSuggestionIndex];
            _lastCommandInput = commandInput;
            selectedCommandListSuggestionIndex = (selectedCommandListSuggestionIndex + 1) % commandListSuggestions.Count;

            _moveCaretToEnd = true;
        }

        private void UpdateCommandSuggestions()
        {
            if (commandListSuggestions == null)
            {
                commandListSuggestions = new List<string>();
            }
            else
            {
                commandListSuggestions.Clear();
            }

            selectedCommandListSuggestionIndex = 0;

            if (string.IsNullOrWhiteSpace(commandInput) || _commandList == null) return;

            var trimmedInput = commandInput.TrimStart();
            foreach (var command in _commandList)
            {
                if (command != null && command.commandId.StartsWith(trimmedInput, StringComparison.OrdinalIgnoreCase))
                {
                    commandListSuggestions.Add(command.commandId);
                }
            }
        }

        private void InitializeCommands()
        {
            _commandList = new List<DebugCommandBase>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null) continue;

                foreach (var type in types)
                {
                    if (type == null || type.IsInterface || type.IsAbstract) continue;
                    if (!typeof(IDebugCommandProvider).IsAssignableFrom(type)) continue;

                    try
                    {
                        if (Activator.CreateInstance(type) is not IDebugCommandProvider provider) continue;
                        var command = provider.GetCommand();
                        if (command != null)
                        {
                            _commandList.Add(command);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"DebugController: Failed to register command provider '{type.FullName}': {ex.Message}");
                    }
                }
            }

            _staticCommandList = _commandList;
            _commandsInitialized = true;
        }

        private void HandleInput()
        {
            if (string.IsNullOrWhiteSpace(commandInput) || _commandList == null) return;

            var properties = commandInput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (properties.Length == 0) return;

            var enteredCommandId = properties[0];

            foreach (var commandBase in _commandList)
            {
                if (commandBase == null) continue;
                if (!string.Equals(enteredCommandId, commandBase.commandId, StringComparison.OrdinalIgnoreCase)) continue;

                try
                {
                    if (commandBase is DebugCommand noArgCmd)
                    {
                        noArgCmd.Invoke();
                        return;
                    }

                    if (commandBase is DynamicDebugCommand dynamicCmd)
                    {
                        var args = properties.Length > 1 ? properties[1..] : Array.Empty<string>();
                        dynamicCmd.Invoke(args);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to execute command '{enteredCommandId}': {ex.Message}");
                    return;
                }
            }

            Debug.LogWarning($"Unknown command: '{enteredCommandId}'. Type 'help' to see available commands.");
        }

        public void DisplayHelpOutput(List<string> outputLines)
        {
            _helpOutput = outputLines != null ? new List<string>(outputLines) : new List<string>();
            _helpOutputTimeShown = Time.unscaledTime;
        }
    }
}
